using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AtomUI.Native.MacOS;
using Avalonia.Controls;

namespace AtomUI.Native;

/// <summary>单个 standard window button 在其 superview 中的矩形 (Cocoa 坐标，原点左下)。</summary>
internal readonly record struct ButtonFrame(double X, double Y, double Width, double Height);

[SupportedOSPlatform("macos")]
internal static class WindowUtilsMacOS
{
    private static readonly ConcurrentDictionary<IntPtr, NativeNotificationObserverRegistration>
        NativeNotificationObservers = new();
    private static readonly object NativeNotificationObserverClassGate = new();
    private static IntPtr s_nativeNotificationObserverClass;
    private static readonly IntPtr NativeNotificationObserverCallbackSelector =
        WindowUtilsInterop.sel_registerName("atomui_nativeNotification:");
    private static readonly IntPtr NativeKeyValueObserverCallbackSelector =
        WindowUtilsInterop.sel_registerName("observeValueForKeyPath:ofObject:change:context:");

    public static void SetWindowIgnoreMouseEventsMacOS(IntPtr handle, bool flag)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid window handle");
        }
        WindowUtilsInterop.objc_msgSend_void_bool(handle, WindowUtilsInterop.SetIgnoresMouseEventsSelector, flag);
    }
    
    public static bool IsWindowIgnoreMouseEventsMacOS(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid window handle");
        }
        return WindowUtilsInterop.objc_msgSend_bool(handle, WindowUtilsInterop.IgnoresMouseEventsSelector);
    }

    // ------------------------------ MacStandardWindowButtons API ------------------------------

    private static IntPtr GetNSWindow(Window window)
    {
        var handle = window.TryGetPlatformHandle();
        return handle?.Handle ?? IntPtr.Zero;
    }

    public static void SetResizeIndicatorVisible(Window window, bool isVisible)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        var ns = GetNSWindow(window);
        if (ns == IntPtr.Zero)
        {
            return;
        }

        WindowUtilsInterop.objc_msgSend_void_bool(ns, WindowUtilsInterop.SetShowsResizeIndicatorSelector, isVisible);
    }
    
    private static IntPtr GetButton(IntPtr ns, WindowUtilsInterop.NSWindowButton which)
    {
        if (ns == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }
        return WindowUtilsInterop.GetStandardWindowButton(ns, (long)which);
    }

    /// <summary>
    /// 读取某个 standard window button 的当前 frame (Cocoa 坐标，原点在 superview 左下)。
    /// </summary>
    /// <returns>非 macOS、无 NSWindow 句柄或按钮不存在时返回 <c>null</c>。</returns>
    public static ButtonFrame? GetStandardWindowButtonFrame(this Window window, WindowUtilsInterop.NSWindowButton button)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return null;
        }

        var ns  = GetNSWindow(window);
        var btn = GetButton(ns, button);
        if (btn == IntPtr.Zero)
        {
            return null;
        }

        var f = WindowUtilsInterop.GetFrame(btn);
        return new ButtonFrame(f.Origin.X, f.Origin.Y, f.Size.Width, f.Size.Height);
    }

    /// <summary>
    /// 以红色关闭按钮作为 macOS 标准按钮组的可见性哨兵。
    /// 录屏/窗口共享期间 AppKit 会暂时隐藏整组标准按钮；关闭按钮恢复时才允许重新布局。
    /// 这能避免把隐藏按钮的位置写回到 macOS 自己显示的共享控件状态。
    /// </summary>
    public static bool IsStandardWindowButtonsVisible(this Window window)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return false;
        }

        var ns = GetNSWindow(window);
        if (ns == IntPtr.Zero)
        {
            return false;
        }

        var closeButton = GetButton(ns, WindowUtilsInterop.NSWindowButton.CloseButton);
        return closeButton != IntPtr.Zero && !WindowUtilsInterop.IsHidden(closeButton);
    }

    /// <summary>
    /// 监听标准窗口按钮的 AppKit 原生通知。
    /// <c>NSViewFrameDidChangeNotification</c> 只有在
    /// <c>postsFrameChangedNotifications</c> 打开时才会发布；这里为每个按钮开启该选项，
    /// 覆盖录屏结束后 AppKit 重新布局按钮的路径。
    /// </summary>
    /// <remarks>
    /// 返回的 disposable 必须在窗口关闭时释放，否则 NotificationCenter 会继续持有原生 observer，
    /// 同时也会保留托管回调和窗口实例。
    /// </remarks>
    public static IDisposable? ObserveStandardWindowButtonChanges(this Window window, Action callback)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return null;
        }

        var ns = GetNSWindow(window);
        if (ns == IntPtr.Zero)
        {
            return null;
        }

        if (WindowUtilsInterop.NSViewFrameDidChangeNotificationName == IntPtr.Zero)
        {
            return null;
        }

        var observedObjects = new List<IntPtr>(3);
        for (int i = 0; i <= 2; i++)
        {
            var button = GetButton(ns, (WindowUtilsInterop.NSWindowButton)i);
            if (button != IntPtr.Zero && !observedObjects.Contains(button))
            {
                observedObjects.Add(button);
            }
        }

        return observedObjects.Count == 0
            ? null
            : NativeNotificationObserverRegistration.Create(observedObjects, callback);
    }

    private static IntPtr EnsureNativeNotificationObserverClass()
    {
        if (s_nativeNotificationObserverClass != IntPtr.Zero)
        {
            return s_nativeNotificationObserverClass;
        }

        lock (NativeNotificationObserverClassGate)
        {
            if (s_nativeNotificationObserverClass != IntPtr.Zero)
            {
                return s_nativeNotificationObserverClass;
            }

            var baseClass = WindowUtilsInterop.objc_getClass("NSObject");
            if (baseClass == IntPtr.Zero)
            {
                throw new InvalidOperationException("NSObject is not available on macOS.");
            }

            const string className = "AtomUIWindowNotificationObserver";
            var observerClass = WindowUtilsInterop.objc_getClass(className);
            if (observerClass == IntPtr.Zero)
            {
                observerClass = WindowUtilsInterop.objc_allocateClassPair(baseClass, className, IntPtr.Zero);
                if (observerClass == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Unable to create the macOS notification observer class.");
                }

                unsafe
                {
                    var notificationCallbackPointer =
                        (IntPtr)(void*)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void>)
                        &HandleNativeNotification;
                    if (!WindowUtilsInterop.class_addMethod(
                            observerClass,
                            NativeNotificationObserverCallbackSelector,
                            notificationCallbackPointer,
                            "v@:@"))
                    {
                        throw new InvalidOperationException(
                            "Unable to add the macOS notification observer callback.");
                    }

                    var keyValueCallbackPointer =
                        (IntPtr)(void*)(delegate* unmanaged[Cdecl]<
                            IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, void>)
                        &HandleNativeKeyValueChange;
                    if (!WindowUtilsInterop.class_addMethod(
                            observerClass,
                            NativeKeyValueObserverCallbackSelector,
                            keyValueCallbackPointer,
                            "v@:@@@^v"))
                    {
                        throw new InvalidOperationException(
                            "Unable to add the macOS key-value observer callback.");
                    }
                }

                WindowUtilsInterop.objc_registerClassPair(observerClass);
            }

            s_nativeNotificationObserverClass = observerClass;
            return observerClass;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleNativeNotification(IntPtr observer, IntPtr selector, IntPtr notification)
    {
        if (!NativeNotificationObservers.TryGetValue(observer, out var registration))
        {
            return;
        }

        registration.Notify();
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void HandleNativeKeyValueChange(
        IntPtr observer,
        IntPtr selector,
        IntPtr keyPath,
        IntPtr observedObject,
        IntPtr change,
        IntPtr context)
    {
        if (!NativeNotificationObservers.TryGetValue(observer, out var registration))
        {
            return;
        }

        registration.Notify();
    }

    private sealed class NativeNotificationObserverRegistration : IDisposable
    {
        private readonly IntPtr _notificationCenter;
        private readonly IntPtr _observer;
        private readonly IntPtr _hiddenKeyPath;
        private readonly IntPtr[] _observedObjects;
        private readonly bool[] _previousFrameNotificationStates;
        private readonly bool[] _frameNotificationStateCaptured;
        private readonly bool[] _keyValueObserversRegistered;
        private Action? _callback;
        private int _disposed;

        private NativeNotificationObserverRegistration(IReadOnlyList<IntPtr> observedObjects, Action callback)
        {
            _callback = callback;
            _observedObjects = observedObjects.ToArray();
            _previousFrameNotificationStates = new bool[_observedObjects.Length];
            _frameNotificationStateCaptured = new bool[_observedObjects.Length];
            _keyValueObserversRegistered = new bool[_observedObjects.Length];

            _hiddenKeyPath = WindowUtilsInterop.CreateRetainedNSString("hidden");
            if (_hiddenKeyPath == IntPtr.Zero)
            {
                throw new InvalidOperationException("Unable to create the macOS hidden key path.");
            }

            _notificationCenter = WindowUtilsInterop.objc_msgSend_intptr(
                WindowUtilsInterop.objc_getClass("NSNotificationCenter"),
                WindowUtilsInterop.NotificationCenterDefaultCenterSelector);
            if (_notificationCenter == IntPtr.Zero)
            {
                WindowUtilsInterop.objc_msgSend_void(_hiddenKeyPath, WindowUtilsInterop.ReleaseSelector);
                throw new InvalidOperationException("NSNotificationCenter is not available on macOS.");
            }

            var observer = WindowUtilsInterop.class_createInstance(
                EnsureNativeNotificationObserverClass(),
                IntPtr.Zero);
            if (observer == IntPtr.Zero)
            {
                WindowUtilsInterop.objc_msgSend_void(_hiddenKeyPath, WindowUtilsInterop.ReleaseSelector);
                throw new InvalidOperationException("Unable to allocate the macOS notification observer.");
            }

            _observer = WindowUtilsInterop.objc_msgSend_intptr(observer, WindowUtilsInterop.InitSelector);
            if (_observer == IntPtr.Zero)
            {
                WindowUtilsInterop.objc_msgSend_void(_hiddenKeyPath, WindowUtilsInterop.ReleaseSelector);
                throw new InvalidOperationException("Unable to initialize the macOS notification observer.");
            }

            if (!NativeNotificationObservers.TryAdd(_observer, this))
            {
                WindowUtilsInterop.objc_msgSend_void(_observer, WindowUtilsInterop.ReleaseSelector);
                WindowUtilsInterop.objc_msgSend_void(_hiddenKeyPath, WindowUtilsInterop.ReleaseSelector);
                throw new InvalidOperationException("Unable to register the macOS notification observer.");
            }
            try
            {
                for (var i = 0; i < _observedObjects.Length; i++)
                {
                    WindowUtilsInterop.objc_msgSend_void_intptr_intptr_intptr_intptr(
                        _notificationCenter,
                        WindowUtilsInterop.NotificationCenterAddObserverSelector,
                        _observer,
                        NativeNotificationObserverCallbackSelector,
                        WindowUtilsInterop.NSViewFrameDidChangeNotificationName,
                        _observedObjects[i]);
                }

                for (var i = 0; i < _observedObjects.Length; i++)
                {
                    var button = _observedObjects[i];
                    _previousFrameNotificationStates[i] =
                        WindowUtilsInterop.PostsFrameChangedNotifications(button);
                    _frameNotificationStateCaptured[i] = true;
                    WindowUtilsInterop.SetPostsFrameChangedNotifications(button, true);
                    WindowUtilsInterop.objc_msgSend_void_intptr_intptr_nuint_intptr(
                        button,
                        WindowUtilsInterop.AddObserverForKeyPathSelector,
                        _observer,
                        _hiddenKeyPath,
                        0,
                        IntPtr.Zero);
                    _keyValueObserversRegistered[i] = true;
                }
            }
            catch
            {
                NativeNotificationObservers.TryRemove(_observer, out _);
                RemoveNativeRegistrations();
                WindowUtilsInterop.objc_msgSend_void(_observer, WindowUtilsInterop.ReleaseSelector);
                WindowUtilsInterop.objc_msgSend_void(_hiddenKeyPath, WindowUtilsInterop.ReleaseSelector);
                throw;
            }
        }

        public static IDisposable Create(IReadOnlyList<IntPtr> observedObjects, Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            return new NativeNotificationObserverRegistration(observedObjects, callback);
        }

        public void Notify()
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                return;
            }

            var callback = _callback;
            if (callback is null)
            {
                return;
            }

            try
            {
                callback();
            }
            catch (Exception ex)
            {
                // Objective-C 不允许异常穿过消息分发边界；窗口回调会在 UI 线程重新投递工作。
                Debug.WriteLine($"AtomUI macOS window notification callback failed: {ex}");
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _callback = null;
            NativeNotificationObservers.TryRemove(_observer, out _);
            RemoveNativeRegistrations();
            WindowUtilsInterop.objc_msgSend_void(_observer, WindowUtilsInterop.ReleaseSelector);
            WindowUtilsInterop.objc_msgSend_void(_hiddenKeyPath, WindowUtilsInterop.ReleaseSelector);
        }

        private void RemoveNativeRegistrations()
        {
            WindowUtilsInterop.objc_msgSend_void_intptr(
                _notificationCenter,
                WindowUtilsInterop.NotificationCenterRemoveObserverSelector,
                _observer);

            for (var i = 0; i < _observedObjects.Length; i++)
            {
                if (_keyValueObserversRegistered[i])
                {
                    WindowUtilsInterop.objc_msgSend_void_intptr_intptr(
                        _observedObjects[i],
                        WindowUtilsInterop.RemoveObserverForKeyPathSelector,
                        _observer,
                        _hiddenKeyPath);
                    _keyValueObserversRegistered[i] = false;
                }

                if (_frameNotificationStateCaptured[i])
                {
                    WindowUtilsInterop.SetPostsFrameChangedNotifications(
                        _observedObjects[i],
                        _previousFrameNotificationStates[i]);
                    _frameNotificationStateCaptured[i] = false;
                }
            }
        }
    }

    /// <summary>
    /// 获取 macOS 系统默认的标题栏高度。
    /// 通过 superview 高度和 contentView 高度的差值来计算。
    /// </summary>
    /// <returns>非 macOS 或无窗口句柄时返回 <c>null</c>。</returns>
    public static double? GetSystemTitleBarHeightMacOS(Window window)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return null;
        }

        var ns = GetNSWindow(window);
        if (ns == IntPtr.Zero)
        {
            return null;
        }

        // 获取 window 的 frame (包含标题栏)
        var windowFrame = WindowUtilsInterop.GetFrame(ns);

        // 获取 contentLayoutRect：这是不包含标题栏的内容区域
        var selContentLayoutRect = WindowUtilsInterop.sel_registerName("contentLayoutRect");
        var contentRect = WindowUtilsInterop.GetRect(ns, selContentLayoutRect);

        // 标题栏高度 = 窗口总高度 - 内容布局区域高度
        var titleBarHeight = windowFrame.Size.Height - contentRect.Size.Height;
        return titleBarHeight > 0 ? titleBarHeight : null;
    }

    /// <summary>
    /// 获取 Standard window buttons 的原始位置（未调整前的默认位置）。
    /// 返回 close 按钮的 X 和 Y 坐标 (Cocoa 坐标，原点左下)。
    /// </summary>
    /// <returns>非 macOS、无 NSWindow 句柄或按钮不存在时返回 <c>null</c>。</returns>
    public static (double X, double Y) GetStandardWindowButtonsOriginalPosition(this Window window)
    {
        var closeFrame = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.CloseButton);
        if (closeFrame is not { } cf)
        {
            return (0, 0);
        }

        return (cf.X, cf.Y);
    }

    /// <summary>
    /// 获取 Standard window buttons 占用的总宽度（从最左边的按钮到最右边的按钮）。
    /// 用于计算自定义标题栏需要为按钮预留的左侧 margin。
    /// </summary>
    /// <param name="window">目标窗口。</param>
    /// <param name="spacing">按钮之间的间距，默认为 20pt（AppKit 原生值）。</param>
    /// <returns>按钮占用的总宽度（包括间距），如果无法获取则返回 <c>null</c>。</returns>
    public static double? GetStandardWindowButtonsTotalWidth(this Window window, double spacing = 20.0)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return null;
        }

        var closeFrame = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.CloseButton);
        var miniaturizeFrame = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.MiniaturizeButton);
        var zoomFrame = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.ZoomButton);

        if (closeFrame is not { } cf || miniaturizeFrame is not { } mf || zoomFrame is not { } zf)
        {
            return null;
        }

        // 计算从最左边按钮的左边缘到最右边按钮的右边缘的总宽度
        // 通常顺序是：Close, Miniaturize, Zoom
        double leftMost = Math.Min(Math.Min(cf.X, mf.X), zf.X);
        double rightMost = Math.Max(Math.Max(cf.X + cf.Width, mf.X + mf.Width), zf.X + zf.Width);

        return rightMost - leftMost;
    }

    /// <summary>
    /// 获取自定义标题栏内容应该从左边缘偏移多少距离，以避免与 Standard window buttons 重叠。
    /// 这个值包括了按钮的宽度、按钮之间的间距，以及按钮右侧的额外间距。
    /// </summary>
    /// <param name="window">目标窗口。</param>
    /// <param name="spacing">按钮之间的间距，默认为 20pt。</param>
    /// <param name="extraMargin">按钮右侧的额外间距，默认为 8pt。</param>
    /// <returns>建议的左侧 margin 值，如果无法获取则返回 <c>null</c>。</returns>
    public static double? GetRecommendedTitleBarContentLeftMargin(this Window window, double spacing = 20.0, double extraMargin = 8.0)
    {
        var closeFrame = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.CloseButton);
        var zoomFrame = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.ZoomButton);

        if (closeFrame is not { } cf || zoomFrame is not { } zf)
        {
            return null;
        }

        // 找到最右边按钮的右边缘
        double rightEdge = Math.Max(cf.X + cf.Width, zf.X + zf.Width);

        // 加上额外的间距
        return rightEdge + extraMargin;
    }

    /// <summary>
    /// 读取按钮 superview (约为 NSThemeFrame) 的高度，近似等于窗口总高度 (含标题栏)。
    /// 用于 Cocoa 底部原点 Y 与 Avalonia 顶部原点 Y 的换算：
    /// <c>cocoaY = superHeight - topY - buttonHeight</c>。
    /// </summary>
    /// <returns>非 macOS 或无窗口句柄时返回 <c>null</c>。</returns>
    public static double? GetStandardWindowButtonSuperviewHeight(this Window window)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return null;
        }

        var ns  = GetNSWindow(window);
        var btn = GetButton(ns, WindowUtilsInterop.NSWindowButton.CloseButton);
        if (btn == IntPtr.Zero)
        {
            return null;
        }

        var super = WindowUtilsInterop.GetSuperview(btn);
        if (super == IntPtr.Zero)
        {
            return null;
        }

        return WindowUtilsInterop.GetFrame(super).Size.Height;
    }

    /// <summary>
    /// 把某个 standard window button 移到 (<paramref name="x"/>, <paramref name="y"/>)，
    /// 坐标为 Cocoa 原生语义 (原点左下)。
    /// 非 macOS、全屏状态或按钮不存在时直接 no-op。
    /// </summary>
    /// <remarks>
    /// 典型用法（顶部原点换算）：
    /// <code>
    /// var superH = window.GetStandardWindowButtonSuperviewHeight();
    /// var frame  = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.CloseButton);
    /// if (superH is double h &amp;&amp; frame is { } f)
    ///     window.SetStandardWindowButtonOrigin(WindowUtilsInterop.NSWindowButton.CloseButton,
    ///         x: 12, y: h - topY - f.Height);
    /// </code>
    /// </remarks>
    public static void SetStandardWindowButtonOrigin(this Window window, WindowUtilsInterop.NSWindowButton button, double x, double y)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }
        if (window.WindowState == WindowState.FullScreen)
        {
            return;
        }

        var ns  = GetNSWindow(window);
        var btn = GetButton(ns, button);
        if (btn == IntPtr.Zero)
        {
            return;
        }

        WindowUtilsInterop.SetFrameOrigin(btn, new WindowUtilsInterop.CGPoint(x, y));
    }

    /// <summary>
    /// 把三个 standard window buttons 按一行布局：close 放到 (<paramref name="x"/>, <paramref name="y"/>)，
    /// miniaturize 放到 (x + spacing, y)，zoom 放到 (x + 2·spacing, y)。
    /// 坐标为 Cocoa 原生语义 (原点左下)。非 macOS、全屏或无 NSWindow 时 no-op。
    /// </summary>
    /// <param name="window">目标窗口。</param>
    /// <param name="x">close 按钮距窗口左边缘的 X (points)。</param>
    /// <param name="y">所有按钮共用的 Y，Cocoa 坐标 (从窗口底部向上量)。</param>
    /// <param name="spacing">相邻按钮左边缘间距，AppKit 原生为 20 pt。</param>
    /// <remarks>
    /// 内部等价于：
    /// <code>
    /// window.SetStandardWindowButtonOrigin(NSWindowButton.CloseButton,       x,               y);
    /// window.SetStandardWindowButtonOrigin(NSWindowButton.MiniaturizeButton, x +     spacing, y);
    /// window.SetStandardWindowButtonOrigin(NSWindowButton.ZoomButton,        x + 2 * spacing, y);
    /// </code>
    /// </remarks>
    public static void SetStandardWindowButtonsLayout(this Window window, double x, double y, double spacing = 20.0)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }
        if (window.WindowState == WindowState.FullScreen)
        {
            return;
        }

        var ns = GetNSWindow(window);
        var closeBtn = GetButton(ns, WindowUtilsInterop.NSWindowButton.CloseButton);
        if (closeBtn == IntPtr.Zero)
        {
            return;
        }

        // live resize 期间 AppKit 的 autoresizing 会自动把按钮维持在相同位置，
        // 这时再触发按钮移动 + tracking area 刷新会和 macOS 的 resize 渲染路径竞争，
        // 造成标题栏/内容区可见抖动。若按钮已在目标位置，直接跳过。
        var currentFrame = WindowUtilsInterop.GetFrame(closeBtn);
        if (Math.Abs(currentFrame.Origin.X - x) <= 0.5 &&
            Math.Abs(currentFrame.Origin.Y - y) <= 0.5)
        {
            return;
        }

        for (int i = 0; i <= 2; i++)
        {
            window.SetStandardWindowButtonOrigin((WindowUtilsInterop.NSWindowButton)i, x + spacing * i, y);
        }

        // 按钮确实被移动了才刷新 tracking areas，保证 hover 事件命中新位置。
        // setFrameOrigin 已经正确 invalidate 了按钮的新旧 frame，不再额外对 superview
        // (NSThemeFrame，覆盖整个窗体) 做 setNeedsDisplay:YES — 那是 6.0 resize 抖动根因。
        var superview = WindowUtilsInterop.GetSuperview(closeBtn);
        WindowUtilsInterop.UpdateTrackingAreas(superview);
    }

    /// <summary>
    /// 把三个 standard window buttons 垂直居中到高度为 <paramref name="titleBarHeight"/>
    /// 的扩展标题栏里。本方法是 <see cref="SetStandardWindowButtonsLayout"/> 上的封装 —— 自动读 superview 高度和
    /// close 按钮尺寸，算出居中的 Y 后委托给 <see cref="SetStandardWindowButtonsLayout"/>。
    /// </summary>
    /// <param name="window">目标窗口。</param>
    /// <param name="titleBarHeight">扩展后的标题栏高度 (即 <c>ExtendClientAreaTitleBarHeightHint</c>)。</param>
    /// <param name="leftInset">
    /// 可选：close 按钮距窗口左边缘的 X 像素偏移。<c>null</c> 时沿用 close 当前的 X
    /// (即 AppKit 默认的约 7–8 pt)。
    /// </param>
    /// <param name="spacing">
    /// 可选：相邻按钮左边缘间距。<c>null</c> 时使用 AppKit 原生值 20 pt。
    /// </param>
    public static void CenterStandardWindowButtons(this Window window, double titleBarHeight, double? leftInset = null,
                                                   double? spacing = null)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }
        if (window.WindowState == WindowState.FullScreen)
        {
            return;
        }

        var superH     = window.GetStandardWindowButtonSuperviewHeight();
        var closeFrame = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.CloseButton);
        if (superH is not { } sh || closeFrame is not { } cf)
        {
            return;
        }

        double x = leftInset ?? cf.X;
        double y = sh - (titleBarHeight + cf.Height) / 2.0;
        window.SetStandardWindowButtonsLayout(x, y, spacing ?? 20.0);
    }

    public static double? CalculateVerticalCenter(this Window window, double titleBarHeight)
    {
        var superH     = window.GetStandardWindowButtonSuperviewHeight();
        var closeFrame = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.CloseButton);
        if (superH is not { } sh || closeFrame is not { } cf)
        {
            return null;
        }
        return sh - (titleBarHeight + cf.Height) / 2.0;
    }
}
