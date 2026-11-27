using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AtomUI.Native.MacOS;
using Avalonia.Controls;

namespace AtomUI.Native;

/// <summary>
/// The frame of one standard window button in its superview, in Cocoa coordinates (origin at the bottom left).
/// </summary>
internal readonly record struct ButtonFrame(double X, double Y, double Width, double Height);

[SupportedOSPlatform("macos")]
internal static partial class WindowUtilsMacOS
{
    private static readonly ConcurrentDictionary<IntPtr, INativeNotificationTarget>
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
    /// Reads the current frame of one standard window button, in Cocoa coordinates with the origin at the bottom left
    /// of its superview.
    /// </summary>
    /// <returns>
    /// <c>null</c> when not on macOS, when the window has no NSWindow handle or when the button does not exist.
    /// </returns>
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
    /// Gets whether the macOS standard window buttons are visible, with the red close button as the sentinel
    /// of the whole group.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the close button exists and is not hidden, and
    /// <see langword="false" /> otherwise, which is also the result when not on macOS or when the window has no
    /// NSWindow handle.
    /// </returns>
    /// <remarks>
    /// During screen recording or window sharing AppKit hides the whole standard button group for a while.
    /// Lay the buttons out again only once the close button is back. This avoids writing the positions of the
    /// hidden buttons back into the sharing control state that macOS itself displays.
    /// </remarks>
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
    /// Listens to the AppKit native notifications of the standard window buttons.
    /// <c>NSViewFrameDidChangeNotification</c> is posted only while <c>postsFrameChangedNotifications</c> is on,
    /// so this method turns the option on for every button. This covers the path on which AppKit lays the
    /// buttons out again after a screen recording ends.
    /// </summary>
    /// <param name="window">
    /// The window whose standard window buttons are observed.
    /// </param>
    /// <param name="callback">
    /// Invoked when a button changes its frame or its <c>hidden</c> state.
    /// </param>
    /// <returns>
    /// A disposable that removes the observers and restores the previous
    /// <c>postsFrameChangedNotifications</c> value of every button, or <c>null</c> when the buttons cannot be
    /// observed: when not on macOS, when the window has no NSWindow handle or when no button exists.
    /// </returns>
    /// <remarks>
    /// Dispose the result when the window closes. Otherwise <c>NSNotificationCenter</c> keeps the native observer,
    /// and with it the managed callback and the window instance.
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

    /// <summary>
    /// Receives the notifications delivered to one native observer instance.
    /// </summary>
    private interface INativeNotificationTarget
    {
        void Notify();
    }

    private sealed class NativeNotificationObserverRegistration : IDisposable, INativeNotificationTarget
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
                // Objective-C does not let an exception cross a message dispatch boundary.
                // The window callback re-posts its work on the UI thread.
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
    /// Gets the default height of the macOS system title bar, in points, as the window frame height minus the
    /// height of the <c>contentLayoutRect</c> of the window.
    /// </summary>
    /// <returns>
    /// <c>null</c> when not on macOS, when the window has no NSWindow handle or when the difference is not
    /// positive.
    /// </returns>
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

        // The window frame includes the title bar.
        var windowFrame = WindowUtilsInterop.GetFrame(ns);

        // The content layout rect is the content area without the title bar.
        var selContentLayoutRect = WindowUtilsInterop.sel_registerName("contentLayoutRect");
        var contentRect = WindowUtilsInterop.GetRect(ns, selContentLayoutRect);

        // Title bar height = window frame height - content layout rect height.
        var titleBarHeight = windowFrame.Size.Height - contentRect.Size.Height;
        return titleBarHeight > 0 ? titleBarHeight : null;
    }

    /// <summary>
    /// Gets the original position of the standard window buttons, that is their default position before any
    /// adjustment, as the X and Y of the close button in Cocoa coordinates with the origin at the bottom left.
    /// </summary>
    /// <returns>
    /// <c>(0, 0)</c> when the frame of the close button cannot be read: when not on macOS, when the window has
    /// no NSWindow handle or when the button does not exist.
    /// </returns>
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
    /// Gets the total width occupied by the standard window buttons in points, from the left edge of the leftmost
    /// button to the right edge of the rightmost one. A custom title bar reserves this much width on its left.
    /// </summary>
    /// <param name="window">
    /// The window whose standard window buttons are measured.
    /// </param>
    /// <param name="spacing">
    /// The gap between adjacent buttons. It is not read; the width comes from the current button frames only.
    /// The default of 20 points is the AppKit native value.
    /// </param>
    /// <returns>
    /// The total width, gaps included, or <c>null</c> when the frame of one of the three buttons cannot be read.
    /// </returns>
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

        // The total width runs from the left edge of the leftmost button to the right edge of the rightmost one.
        // The usual order is Close, Miniaturize, Zoom.
        double leftMost = Math.Min(Math.Min(cf.X, mf.X), zf.X);
        double rightMost = Math.Max(Math.Max(cf.X + cf.Width, mf.X + mf.Width), zf.X + zf.Width);

        return rightMost - leftMost;
    }

    /// <summary>
    /// Gets how far the content of a custom title bar must be offset from the left edge of the window so that
    /// it does not overlap the standard window buttons, in points. The value covers the buttons, the gaps between
    /// them and the extra margin to their right.
    /// </summary>
    /// <param name="window">
    /// The window whose standard window buttons must be avoided.
    /// </param>
    /// <param name="spacing">
    /// The gap between adjacent buttons. It is not read; the result comes from the current button frames and
    /// <paramref name="extraMargin" /> only. The default is 20 points.
    /// </param>
    /// <param name="extraMargin">
    /// Extra space to the right of the buttons. The default is 8 points.
    /// </param>
    /// <returns>
    /// The recommended left margin, or <c>null</c> when the frame of the close button or of the zoom button
    /// cannot be read.
    /// </returns>
    public static double? GetRecommendedTitleBarContentLeftMargin(this Window window, double spacing = 20.0, double extraMargin = 8.0)
    {
        var closeFrame = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.CloseButton);
        var zoomFrame = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.ZoomButton);

        if (closeFrame is not { } cf || zoomFrame is not { } zf)
        {
            return null;
        }

        // The right edge of the rightmost button.
        double rightEdge = Math.Max(cf.X + cf.Width, zf.X + zf.Width);

        // Add the extra margin.
        return rightEdge + extraMargin;
    }

    /// <summary>
    /// Reads the height, in points, of the superview of the standard window buttons (usually <c>NSThemeFrame</c>),
    /// which approximates the total height of the window including the title bar. It converts a Y measured from the
    /// bottom in Cocoa into a Y measured from the top in Avalonia:
    /// <c>cocoaY = superHeight - topY - buttonHeight</c>.
    /// </summary>
    /// <returns>
    /// <c>null</c> when not on macOS, when the window has no NSWindow handle, or when the close button or its
    /// superview cannot be found.
    /// </returns>
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
    /// Moves one standard window button to the origin (<paramref name="x" />, <paramref name="y" />), in
    /// Cocoa coordinates with the origin at the bottom left.
    /// Does nothing when not on macOS, when the window is full screen or when the button cannot be found.
    /// </summary>
    /// <remarks>
    ///     Typical use, converting a Y measured from the top:
    ///     <code>
    ///     var superH = window.GetStandardWindowButtonSuperviewHeight();
    ///     var frame  = window.GetStandardWindowButtonFrame(WindowUtilsInterop.NSWindowButton.CloseButton);
    ///     if (superH is double h &amp;&amp; frame is { } f)
    ///         window.SetStandardWindowButtonOrigin(WindowUtilsInterop.NSWindowButton.CloseButton,
    ///             x: 12, y: h - topY - f.Height);
    ///     </code>
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
    /// Places the three standard window buttons in one row: close at
    /// (<paramref name="x" />, <paramref name="y" />), miniaturize at (x + spacing, y) and zoom at
    /// (x + 2 * spacing, y), in Cocoa coordinates with the origin at the bottom left.
    /// Does nothing when not on macOS, when the window is full screen or when the close button cannot be found.
    /// A call that leaves the close button within 0.5 points of its target moves nothing at all.
    /// </summary>
    /// <param name="window">
    /// The window whose standard window buttons are placed.
    /// </param>
    /// <param name="x">
    /// The X of the close button, in points, measured from the left edge of the window.
    /// </param>
    /// <param name="y">
    /// The Y shared by all buttons, in Cocoa coordinates, measured upward from the bottom of the window.
    /// </param>
    /// <param name="spacing">
    /// The distance between the left edges of adjacent buttons. The AppKit native value is 20 points.
    /// </param>
    /// <remarks>
    ///     Equivalent to:
    ///     <code>
    ///     window.SetStandardWindowButtonOrigin(NSWindowButton.CloseButton,       x,               y);
    ///     window.SetStandardWindowButtonOrigin(NSWindowButton.MiniaturizeButton, x +     spacing, y);
    ///     window.SetStandardWindowButtonOrigin(NSWindowButton.ZoomButton,        x + 2 * spacing, y);
    ///     </code>
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

        // During a live resize the autoresizing of AppKit keeps the buttons at the same position by itself.
        // Moving the buttons and refreshing the tracking areas at that point competes with the resize rendering
        // path of macOS and makes the title bar and the content area flicker visibly, so a button that is already
        // at the target position is skipped.
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

        // The tracking areas are refreshed only after the buttons really moved, so that hover events hit the new
        // position. setFrameOrigin already invalidates the old and new frames of the button, so the superview
        // (NSThemeFrame, which covers the whole window) gets no setNeedsDisplay:YES. That was the root cause of the
        // resize flicker in 6.0.
        var superview = WindowUtilsInterop.GetSuperview(closeBtn);
        WindowUtilsInterop.UpdateTrackingAreas(superview);
    }

    /// <summary>
    /// Vertically centers the three standard window buttons in an extended title bar of the height
    /// <paramref name="titleBarHeight" />.
    /// This is a wrapper over <see cref="SetStandardWindowButtonsLayout" />. It reads the superview height
    /// and the close button size itself, computes the centered Y and delegates to
    /// <see cref="SetStandardWindowButtonsLayout" />.
    /// Does nothing when not on macOS, when the window is full screen, or when the superview height or the close
    /// button frame cannot be read.
    /// </summary>
    /// <param name="window">
    /// The window whose standard window buttons are centered.
    /// </param>
    /// <param name="titleBarHeight">
    /// The height of the extended title bar, that is <c>ExtendClientAreaTitleBarHeightHint</c>.
    /// </param>
    /// <param name="leftInset">
    /// Optional X offset of the close button from the left edge of the window, in points. When <c>null</c>, the
    /// current X of the close button is kept, which is about 7 to 8 points by AppKit default.
    /// </param>
    /// <param name="spacing">
    /// Optional distance between the left edges of adjacent buttons. When <c>null</c>, the AppKit native value of
    /// 20 points is used.
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
