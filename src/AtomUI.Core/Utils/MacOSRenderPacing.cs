using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;

namespace AtomUI.Utils;

[SupportedOSPlatform("macos14.0")]
internal static class MacOSRenderPacing
{
    /// <summary>
    /// Installs a display-synchronized render clock for a single-screen Metal desktop session.
    /// </summary>
    internal static void Apply(AppBuilder builder)
    {
        if (Design.IsDesignMode || !Dispatcher.UIThread.CheckAccess() ||
            AvaloniaLocator.Current.GetService<IScreenImpl>() is not { ScreenCount: 1 } screenImpl ||
            AvaloniaLocator.Current.GetService<IPlatformGraphics>()?.GetType().FullName != "Avalonia.Native.MetalPlatformGraphics" ||
            AvaloniaLocator.Current.GetService<IRenderLoop>() is not { } loop ||
            RenderLoopReflection.TryInstallProxyClock(loop, static original =>
                original.GetType().FullName == "Avalonia.Native.AvaloniaNativeRenderTimer"
                    ? ScreenDisplayLinkTimer.TryCreate(original)
                    : null, static replacement => ((ScreenDisplayLinkTimer)replacement).Start()) is not ScreenDisplayLinkTimer timer)
        {
            return;
        }

        // A single screen supplies the clock for all windows. There is no single screen clock
        // that can pace a shared compositor across displays with different refresh rates.
        builder.AfterSetup(appBuilder =>
        {
            if (appBuilder.Instance?.ApplicationLifetime is not IControlledApplicationLifetime lifetime || screenImpl.ScreenCount != 1)
            {
                timer.UseOriginalClock();
                return;
            }
            var screens = new Screens(screenImpl);
            screens.Changed += OnScreensChanged;
            lifetime.Exit += OnExit;

            void OnScreensChanged(object? sender, EventArgs args) => RestoreOriginalTimer();

            void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs args) => RestoreOriginalTimer();

            void RestoreOriginalTimer()
            {
                screens.Changed -= OnScreensChanged;
                lifetime.Exit -= OnExit;
                timer.UseOriginalClock();
            }
        });
    }

    private sealed unsafe class ScreenDisplayLinkTimer : IRenderTimer, IDisposable
    {
        private const string ObjectiveC = "/usr/lib/libobjc.A.dylib";
        private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
        private static readonly Lazy<nint> TargetClass = new(RegisterTargetClass);
        private static ScreenDisplayLinkTimer? s_timer;

        private readonly IRenderTimer _original;
        private readonly nint _target;
        private readonly nint _link;
        private readonly object _gate = new();
        private readonly TaskCompletionSource<bool> _initialized = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private nint _runLoop;
        private nint _activitySource;
        private int _disposed;
        private Action<TimeSpan>? _tick;
        private bool _useOriginal;
        private bool _clockActive;

        private ScreenDisplayLinkTimer(IRenderTimer original, nint target, nint link)
        {
            _original = original;
            _target = target;
            _link = link;
        }

        public bool RunsInBackground => true;

        public Action<TimeSpan>? Tick
        {
            get
            {
                lock (_gate)
                {
                    return _useOriginal ? _original.Tick : _tick;
                }
            }
            set
            {
                lock (_gate)
                {
                    if (_useOriginal)
                    {
                        if (value is not null || _original.Tick is not null)
                        {
                            _original.Tick = value;
                        }
                        return;
                    }
                    Volatile.Write(ref _tick, value);
                    SignalActivity();
                }
            }
        }

        internal static ScreenDisplayLinkTimer? TryCreate(IRenderTimer original)
        {
            var screenClass = objc_getClass("NSScreen");
            var screens = Send(screenClass, Selector("screens"));
            if (Send(screens, Selector("count")) != 1 || TargetClass.Value == 0)
            {
                return null;
            }
            var screen = Send(screenClass, Selector("mainScreen"));
            var refreshRate = (float)Send(screen, Selector("maximumFramesPerSecond"));
            if (refreshRate <= 0)
            {
                return null;
            }
            var target = Send(Send(TargetClass.Value, Selector("alloc")), Selector("init"));
            var link = CreateLink(screen, Selector("displayLinkWithTarget:selector:"), target, Selector("onDisplay:"));
            if (link == 0)
            {
                SendVoid(target, Selector("release"));
                return null;
            }
            _ = Send(link, Selector("retain"));
            SetFrameRateRange(link, Selector("setPreferredFrameRateRange:"), new FrameRateRange(refreshRate));
            SetBoolean(link, Selector("setPaused:"), 1);

            var timer = new ScreenDisplayLinkTimer(original, target, link);
            if (Interlocked.CompareExchange(ref s_timer, timer, null) is not null)
            {
                timer.ReleaseLink();
                return null;
            }
            try
            {
                new Thread(timer.Run) { IsBackground = true, Name = "AtomUI macOS render clock" }.Start();
            }
            catch (ThreadStateException)
            {
                Interlocked.CompareExchange(ref s_timer, null, timer);
                timer.ReleaseLink();
                return null;
            }
            return timer._initialized.Task.GetAwaiter().GetResult() ? timer : null;
        }

        private void Run()
        {
            var pool = objc_autoreleasePoolPush();
            nint source = 0;
            nint mode = 0;
            try
            {
                // A run-loop source carries start, stop and shutdown requests to the link's own thread.
                // Tick can be set while Avalonia holds its render-loop lock, so the setter never waits.
                var context = new SourceContext { Perform = (nint)(delegate* unmanaged<nint, void>)&ApplyActivity };
                source = CFRunLoopSourceCreate(0, 0, ref context);
                mode = CFStringCreateWithCString(0, "kCFRunLoopDefaultMode", 0x08000100);
                if (source == 0 || mode == 0)
                {
                    return;
                }
                var runLoop = CFRunLoopGetCurrent();
                CFRunLoopAddSource(runLoop, source, mode);
                var runLoopObject = Send(objc_getClass("NSRunLoop"), Selector("currentRunLoop"));
                SendTwo(_link, Selector("addToRunLoop:forMode:"), runLoopObject, mode);
                lock (_gate)
                {
                    _runLoop = runLoop;
                    _activitySource = source;
                }
                _initialized.TrySetResult(true);
                while (Volatile.Read(ref _disposed) == 0)
                {
                    var framePool = objc_autoreleasePoolPush();
                    try
                    {
                        _ = CFRunLoopRunInMode(mode, 3600, 1);
                    }
                    finally
                    {
                        objc_autoreleasePoolPop(framePool);
                    }
                }
            }
            finally
            {
                lock (_gate)
                {
                    _activitySource = 0;
                    _runLoop = 0;
                }
                ReleaseLink();
                if (source != 0)
                {
                    CFRunLoopSourceInvalidate(source);
                    CFRelease(source);
                }
                if (mode != 0)
                {
                    CFRelease(mode);
                }
                Interlocked.CompareExchange(ref s_timer, null, this);
                _initialized.TrySetResult(false);
                objc_autoreleasePoolPop(pool);
            }
        }

        private void ReleaseLink()
        {
            SendVoid(_link, Selector("invalidate"));
            SendVoid(_link, Selector("release"));
            SendVoid(_target, Selector("release"));
        }

        private void SignalActivity()
        {
            lock (_gate)
            {
                if (_activitySource != 0)
                {
                    CFRunLoopSourceSignal(_activitySource);
                    CFRunLoopWakeUp(_runLoop);
                }
            }
        }

        internal void Start()
        {
            lock (_gate)
            {
                Volatile.Write(ref _tick, _original.Tick);
                if (_tick is null)
                {
                    Volatile.Write(ref _clockActive, true);
                    return;
                }
                // Avalonia can already have queued a frame during platform setup. Retire the
                // CVDisplayLink on its own callback thread before allowing the new clock to tick.
                _original.Tick = FinishHandover;
            }
        }

        private void FinishHandover(TimeSpan time)
        {
            lock (_gate)
            {
                if (_useOriginal)
                {
                    // If fallback won the handover while idle, the temporary callback still
                    // owns the old source. A late callback must leave a restored subscriber alone.
                    if (_original.Tick == FinishHandover)
                    {
                        _original.Tick = null;
                    }
                    return;
                }
                _original.Tick = null;
                Volatile.Write(ref _clockActive, true);
                SignalActivity();
            }
        }

        internal void UseOriginalClock()
        {
            lock (_gate)
            {
                var tick = _tick;
                _useOriginal = true;
                Dispose();
                if (tick is not null)
                {
                    _original.Tick = tick;
                }
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                Volatile.Write(ref _tick, null);
                SignalActivity();
            }
        }

        [UnmanagedCallersOnly]
        private static void ApplyActivity(nint info)
        {
            if (Volatile.Read(ref s_timer) is { } timer)
            {
                var active = Volatile.Read(ref timer._clockActive) && Volatile.Read(ref timer._tick) is not null;
                SetBoolean(timer._link, Selector("setPaused:"), active ? (byte)0 : (byte)1);
            }
        }

        [UnmanagedCallersOnly]
        private static void OnDisplay(nint self, nint command, nint link)
        {
            if (Volatile.Read(ref s_timer) is { } timer && timer._link == link)
            {
                try
                {
                    Volatile.Read(ref timer._tick)?.Invoke(timer._clock.Elapsed);
                }
                catch (Exception error)
                {
                    StartupProcess.LogWarning($"macOS render clock callback failed: {error.Message}");
                }
            }
        }

        private static nint RegisterTargetClass()
        {
            var cls = objc_allocateClassPair(objc_getClass("NSObject"), "AtomUIScreenDisplayLinkTarget", 0);
            if (cls == 0)
            {
                return 0;
            }
            if (!class_addMethod(cls, Selector("onDisplay:"),
                                (nint)(delegate* unmanaged<nint, nint, nint, void>)&OnDisplay, "v@:@"))
            {
                objc_disposeClassPair(cls);
                return 0;
            }
            objc_registerClassPair(cls);
            return cls;
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct FrameRateRange(float refreshRate)
        {
            private readonly float _minimum = refreshRate;
            private readonly float _maximum = refreshRate;
            private readonly float _preferred = refreshRate;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SourceContext
        {
            public nint Version, Info, Retain, Release, CopyDescription, Equal, Hash, Schedule, Cancel, Perform;
        }

        [DllImport(ObjectiveC)] private static extern nint objc_getClass(string name);
        [DllImport(ObjectiveC)] private static extern nint objc_allocateClassPair(nint cls, string name, nuint extra);
        [DllImport(ObjectiveC)] private static extern void objc_registerClassPair(nint cls);
        [DllImport(ObjectiveC)] private static extern void objc_disposeClassPair(nint cls);
        [DllImport(ObjectiveC)] [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool class_addMethod(nint cls, nint selector, nint implementation, string types);
        [DllImport(ObjectiveC, EntryPoint = "sel_registerName")] private static extern nint Selector(string name);
        [DllImport(ObjectiveC, EntryPoint = "objc_msgSend")] private static extern nint Send(nint receiver, nint selector);
        [DllImport(ObjectiveC, EntryPoint = "objc_msgSend")] private static extern void SendVoid(nint receiver, nint selector);
        [DllImport(ObjectiveC, EntryPoint = "objc_msgSend")] private static extern nint CreateLink(nint screen, nint selector, nint target, nint callback);
        [DllImport(ObjectiveC, EntryPoint = "objc_msgSend")] private static extern void SendTwo(nint receiver, nint selector, nint a, nint b);
        [DllImport(ObjectiveC, EntryPoint = "objc_msgSend")] private static extern void SetBoolean(nint receiver, nint selector, byte value);
        [DllImport(ObjectiveC, EntryPoint = "objc_msgSend")] private static extern void SetFrameRateRange(nint receiver, nint selector, FrameRateRange range);
        [DllImport(ObjectiveC)] private static extern nint objc_autoreleasePoolPush();
        [DllImport(ObjectiveC)] private static extern void objc_autoreleasePoolPop(nint pool);
        [DllImport(CoreFoundation)] private static extern nint CFStringCreateWithCString(nint allocator, string value, uint encoding);
        [DllImport(CoreFoundation)] private static extern nint CFRunLoopGetCurrent();
        [DllImport(CoreFoundation)] private static extern nint CFRunLoopSourceCreate(nint allocator, nint order, ref SourceContext context);
        [DllImport(CoreFoundation)] private static extern void CFRunLoopAddSource(nint runLoop, nint source, nint mode);
        [DllImport(CoreFoundation)] private static extern void CFRunLoopSourceSignal(nint source);
        [DllImport(CoreFoundation)] private static extern void CFRunLoopSourceInvalidate(nint source);
        [DllImport(CoreFoundation)] private static extern void CFRunLoopWakeUp(nint runLoop);
        [DllImport(CoreFoundation)] private static extern int CFRunLoopRunInMode(nint mode, double seconds, byte returnAfterSourceHandled);
        [DllImport(CoreFoundation)] private static extern void CFRelease(nint value);
    }
}
