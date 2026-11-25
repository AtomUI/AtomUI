using System.Diagnostics;
using System.Runtime.Versioning;
using AtomUI.Native.MacOS;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Native;

internal static partial class WindowUtilsMacOS
{
    /// <summary>
    /// Tracks the system menu bar and title strip AppKit slides in over a full-screen window.
    /// </summary>
    /// <returns><c>null</c> when the window has no native handle or AppKit does not expose the move notification.</returns>
    public static IFullScreenNativeChrome? AttachFullScreenNativeChrome(Window window, Action metricsChanged)
    {
        var ns = GetNSWindow(window);
        if (ns == IntPtr.Zero ||
            WindowUtilsInterop.NSWindowDidMoveNotificationName == IntPtr.Zero ||
            WindowUtilsInterop.NSWindowDidEnterFullScreenNotificationName == IntPtr.Zero)
        {
            return null;
        }

        return new MacOSFullScreenNativeChrome(ns, metricsChanged);
    }

    /// <summary>
    /// In full screen AppKit moves the standard window buttons into a separate system window (the title strip) that
    /// slides in together with the menu bar when the pointer reaches the top of the screen. The strip window is found
    /// through the public relation "the window that owns the close button", and its move notifications describe the
    /// slide. On a display with a camera housing the strip stays at the top of the client area and is faded in and
    /// out instead, so its alpha is observed as well. When fusion is enabled, the strip's own background, decoration and shadow are hidden so that only the
    /// buttons remain over the managed title bar; the strip is restored exactly when fusion ends.
    /// </summary>
    [SupportedOSPlatform("macos")]
    private sealed class MacOSFullScreenNativeChrome : IFullScreenNativeChrome, INativeNotificationTarget
    {
        private const double ControlTrailingMargin = 8;
        private static readonly IntPtr RemoveObserverNameObjectSelector =
            WindowUtilsInterop.sel_registerName("removeObserver:name:object:");
        private static readonly IntPtr BoundsSelector = WindowUtilsInterop.sel_registerName("bounds");
        private static readonly IntPtr SetBoundsOriginSelector = WindowUtilsInterop.sel_registerName("setBoundsOrigin:");
        private static readonly IntPtr AlphaValueSelector = WindowUtilsInterop.sel_registerName("alphaValue");
        // Lives for the process: every tracker observes the same key path.
        private static readonly IntPtr AlphaValueKeyPath = WindowUtilsInterop.CreateRetainedNSString("alphaValue");

        private readonly IntPtr _window;
        private readonly Action _metricsChanged;
        private readonly IntPtr _notificationCenter;
        private readonly IntPtr _observer;
        private readonly List<IntPtr> _hiddenStripElements = new();
        private IntPtr _stripWindow;
        private IntPtr _offsetTitleBarView;
        private FullScreenTitleStripFusion _fusion;
        private bool _isFused;
        private bool _isStripAlphaObserved;
        private bool _isDisposed;

        public MacOSFullScreenNativeChrome(IntPtr window, Action metricsChanged)
        {
            _window         = window;
            _metricsChanged = metricsChanged;
            _notificationCenter = WindowUtilsInterop.objc_msgSend_intptr(
                WindowUtilsInterop.objc_getClass("NSNotificationCenter"),
                WindowUtilsInterop.NotificationCenterDefaultCenterSelector);
            var instance = WindowUtilsInterop.class_createInstance(EnsureNativeNotificationObserverClass(), IntPtr.Zero);
            _observer = WindowUtilsInterop.objc_msgSend_intptr(instance, WindowUtilsInterop.InitSelector);
            if (_notificationCenter == IntPtr.Zero || _observer == IntPtr.Zero ||
                !NativeNotificationObservers.TryAdd(_observer, this))
            {
                if (_observer != IntPtr.Zero)
                {
                    WindowUtilsInterop.objc_msgSend_void(_observer, WindowUtilsInterop.ReleaseSelector);
                }

                throw new InvalidOperationException("Unable to observe the macOS full-screen chrome.");
            }

            // The window state can report full screen before AppKit finishes the transition and creates the strip.
            AddObserver(WindowUtilsInterop.NSWindowDidEnterFullScreenNotificationName, _window);
            Refresh();
        }

        public FullScreenNativeChromeMetrics Metrics { get; private set; }

        public void ConfigureTitleStripFusion(FullScreenTitleStripFusion fusion)
        {
            if (_isDisposed || _fusion == fusion)
            {
                return;
            }

            _fusion = fusion;
            Refresh();
        }

        public void Notify()
        {
            try
            {
                Refresh();
            }
            catch (Exception ex)
            {
                // Objective-C does not allow exceptions to cross the message dispatch boundary.
                Debug.WriteLine($"AtomUI macOS full-screen chrome refresh failed: {ex}");
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            NativeNotificationObservers.TryRemove(_observer, out _);
            WindowUtilsInterop.objc_msgSend_void_intptr(
                _notificationCenter,
                WindowUtilsInterop.NotificationCenterRemoveObserverSelector,
                _observer);
            RestoreFusion();
            SetStripWindow(IntPtr.Zero);
            WindowUtilsInterop.objc_msgSend_void(_observer, WindowUtilsInterop.ReleaseSelector);
        }

        private void Refresh()
        {
            if (_isDisposed)
            {
                return;
            }

            var stripWindow = FindStripWindow();
            if (stripWindow != _stripWindow)
            {
                RestoreFusion();
                SetStripWindow(stripWindow);
            }

            ApplyFusion();
            UpdateMetrics();
        }

        private IntPtr FindStripWindow()
        {
            var closeButton = GetButton(_window, WindowUtilsInterop.NSWindowButton.CloseButton);
            if (closeButton == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            var owner = WindowUtilsInterop.objc_msgSend_intptr(closeButton, WindowUtilsInterop.WindowSelector);
            return owner == _window ? IntPtr.Zero : owner;
        }

        private void SetStripWindow(IntPtr stripWindow)
        {
            if (_stripWindow != IntPtr.Zero)
            {
                WindowUtilsInterop.objc_msgSend_void_intptr_intptr_intptr(
                    _notificationCenter,
                    RemoveObserverNameObjectSelector,
                    _observer,
                    WindowUtilsInterop.NSWindowDidMoveNotificationName,
                    _stripWindow);
                if (_isStripAlphaObserved)
                {
                    WindowUtilsInterop.objc_msgSend_void_intptr_intptr(
                        _stripWindow,
                        WindowUtilsInterop.RemoveObserverForKeyPathSelector,
                        _observer,
                        AlphaValueKeyPath);
                    _isStripAlphaObserved = false;
                }

                WindowUtilsInterop.objc_msgSend_void(_stripWindow, WindowUtilsInterop.ReleaseSelector);
            }

            _stripWindow = stripWindow;
            if (stripWindow != IntPtr.Zero)
            {
                WindowUtilsInterop.objc_msgSend_void(stripWindow, WindowUtilsInterop.RetainSelector);
                AddObserver(WindowUtilsInterop.NSWindowDidMoveNotificationName, stripWindow);
                // Where the full-screen window does not reach the top of the screen (a display with a camera
                // housing), AppKit reveals the strip by fading it in at a fixed position: no move notification is
                // posted and only the strip's alpha tells whether it is shown.
                if (AlphaValueKeyPath != IntPtr.Zero)
                {
                    WindowUtilsInterop.objc_msgSend_void_intptr_intptr_nuint_intptr(
                        stripWindow,
                        WindowUtilsInterop.AddObserverForKeyPathSelector,
                        _observer,
                        AlphaValueKeyPath,
                        0,
                        IntPtr.Zero);
                    _isStripAlphaObserved = true;
                }
            }
        }

        private void AddObserver(IntPtr name, IntPtr observedObject)
        {
            WindowUtilsInterop.objc_msgSend_void_intptr_intptr_intptr_intptr(
                _notificationCenter,
                WindowUtilsInterop.NotificationCenterAddObserverSelector,
                _observer,
                NativeNotificationObserverCallbackSelector,
                name,
                observedObject);
        }

        private void ApplyFusion()
        {
            if (!_fusion.IsEnabled || _stripWindow == IntPtr.Zero)
            {
                RestoreFusion();
                return;
            }

            if (!_isFused)
            {
                if (!TryHideStripBackground())
                {
                    return;
                }

                _isFused = true;
            }
            else
            {
                // AppKit may present the strip again with its own appearance; keep the hidden elements hidden.
                foreach (var element in _hiddenStripElements)
                {
                    if (!WindowUtilsInterop.IsHidden(element))
                    {
                        WindowUtilsInterop.SetHidden(element, true);
                    }
                }
            }

            OffsetButtons();
        }

        /// <summary>
        /// Hides the strip's background view, its decoration views and the shadow layers under the strip. The
        /// elements are identified by structure only: the background is the single sibling of the buttons that hosts a
        /// visual effect view, decorations are the siblings of the title bar view, and shadow layers are the visible
        /// layers of the strip content that do not back the title bar container.
        /// </summary>
        private bool TryHideStripBackground()
        {
            var closeButton  = GetButton(_window, WindowUtilsInterop.NSWindowButton.CloseButton);
            var titleBarView = closeButton == IntPtr.Zero ? IntPtr.Zero : WindowUtilsInterop.GetSuperview(closeButton);
            var container    = titleBarView == IntPtr.Zero ? IntPtr.Zero : WindowUtilsInterop.GetSuperview(titleBarView);
            var contentView  = WindowUtilsInterop.objc_msgSend_intptr(_stripWindow, WindowUtilsInterop.ContentViewSelector);
            if (titleBarView == IntPtr.Zero || container == IntPtr.Zero || contentView == IntPtr.Zero)
            {
                return false;
            }

            var backgrounds = WindowUtilsInterop
                              .GetArrayItems(WindowUtilsInterop.objc_msgSend_intptr(titleBarView, WindowUtilsInterop.SubviewsSelector))
                              .Where(HostsVisualEffectView)
                              .ToList();
            if (backgrounds.Count != 1)
            {
                return false;
            }

            var decorations = WindowUtilsInterop
                              .GetArrayItems(WindowUtilsInterop.objc_msgSend_intptr(container, WindowUtilsInterop.SubviewsSelector))
                              .Where(view => view != titleBarView);
            var containerLayer = WindowUtilsInterop.objc_msgSend_intptr(container, WindowUtilsInterop.LayerSelector);
            var contentLayer   = WindowUtilsInterop.objc_msgSend_intptr(contentView, WindowUtilsInterop.LayerSelector);
            var shadowLayers = WindowUtilsInterop
                               .GetArrayItems(contentLayer == IntPtr.Zero
                                   ? IntPtr.Zero
                                   : WindowUtilsInterop.objc_msgSend_intptr(contentLayer, WindowUtilsInterop.SublayersSelector))
                               .Where(layer => layer != containerLayer);

            foreach (var element in backgrounds.Concat(decorations).Concat(shadowLayers))
            {
                if (WindowUtilsInterop.IsHidden(element))
                {
                    continue;
                }

                WindowUtilsInterop.objc_msgSend_void(element, WindowUtilsInterop.RetainSelector);
                WindowUtilsInterop.SetHidden(element, true);
                _hiddenStripElements.Add(element);
            }

            return true;
        }

        private static bool HostsVisualEffectView(IntPtr view)
        {
            return WindowUtilsInterop
                   .GetArrayItems(WindowUtilsInterop.objc_msgSend_intptr(view, WindowUtilsInterop.SubviewsSelector))
                   .Any(subview => WindowUtilsInterop.IsKindOfClass(subview, "NSVisualEffectView"));
        }

        /// <summary>
        /// Centres the buttons vertically in the managed surface measured from the strip's top edge and applies the
        /// requested leading offset. AppKit owns the button frames and lays them out again on every frame of the
        /// strip's slide, so the buttons are moved together by offsetting the bounds origin of the title bar view
        /// instead of by changing their frames.
        /// </summary>
        private void OffsetButtons()
        {
            var closeButton  = GetButton(_window, WindowUtilsInterop.NSWindowButton.CloseButton);
            var titleBarView = closeButton == IntPtr.Zero ? IntPtr.Zero : WindowUtilsInterop.GetSuperview(closeButton);
            if (titleBarView == IntPtr.Zero)
            {
                return;
            }

            if (_offsetTitleBarView != titleBarView)
            {
                ResetButtonOffset();
                _offsetTitleBarView = titleBarView;
                WindowUtilsInterop.objc_msgSend_void(titleBarView, WindowUtilsInterop.RetainSelector);
            }

            var titleBarHeight = WindowUtilsInterop.GetFrame(titleBarView).Size.Height;
            var closeFrame     = WindowUtilsInterop.GetFrame(closeButton);
            var x = double.IsFinite(_fusion.ButtonOffsetX) ? _fusion.ButtonOffsetX : closeFrame.Origin.X;
            // The title bar view is not flipped: y grows upwards from its bottom edge.
            var y = _fusion.SurfaceHeight > 0
                ? titleBarHeight - (_fusion.SurfaceHeight - closeFrame.Size.Height) / 2 - closeFrame.Size.Height
                : closeFrame.Origin.Y;
            var origin  = new WindowUtilsInterop.CGPoint(closeFrame.Origin.X - x, closeFrame.Origin.Y - y);
            var current = WindowUtilsInterop.GetRect(titleBarView, BoundsSelector).Origin;
            if (Math.Abs(current.X - origin.X) <= 0.5 && Math.Abs(current.Y - origin.Y) <= 0.5)
            {
                return;
            }

            WindowUtilsInterop.objc_msgSend_void_point(titleBarView, SetBoundsOriginSelector, origin);
            WindowUtilsInterop.UpdateTrackingAreas(titleBarView);
            WindowUtilsInterop.SetNeedsDisplay(titleBarView, true);
        }

        private void ResetButtonOffset()
        {
            if (_offsetTitleBarView == IntPtr.Zero)
            {
                return;
            }

            WindowUtilsInterop.objc_msgSend_void_point(
                _offsetTitleBarView,
                SetBoundsOriginSelector,
                new WindowUtilsInterop.CGPoint(0, 0));
            WindowUtilsInterop.UpdateTrackingAreas(_offsetTitleBarView);
            WindowUtilsInterop.objc_msgSend_void(_offsetTitleBarView, WindowUtilsInterop.ReleaseSelector);
            _offsetTitleBarView = IntPtr.Zero;
        }

        private void RestoreFusion()
        {
            if (!_isFused && _hiddenStripElements.Count == 0 && _offsetTitleBarView == IntPtr.Zero)
            {
                return;
            }

            foreach (var element in _hiddenStripElements)
            {
                WindowUtilsInterop.SetHidden(element, false);
                WindowUtilsInterop.objc_msgSend_void(element, WindowUtilsInterop.ReleaseSelector);
            }

            _hiddenStripElements.Clear();
            ResetButtonOffset();
            _isFused = false;
        }

        private void UpdateMetrics()
        {
            var windowFrame     = WindowUtilsInterop.GetFrame(_window);
            var contentLayout   = WindowUtilsInterop.GetRect(_window, WindowUtilsInterop.ContentLayoutRectSelector);
            var windowTop       = windowFrame.Origin.Y + windowFrame.Size.Height;
            var persistentInset = Math.Max(0, windowFrame.Size.Height - (contentLayout.Origin.Y + contentLayout.Size.Height));
            TitleStripState? strip = null;

            var closeButton = GetButton(_window, WindowUtilsInterop.NSWindowButton.CloseButton);
            if (_stripWindow != IntPtr.Zero && closeButton != IntPtr.Zero)
            {
                var stripFrame = WindowUtilsInterop.GetFrame(_stripWindow);
                var container  = WindowUtilsInterop.GetSuperview(WindowUtilsInterop.GetSuperview(closeButton));
                var trailingEdge = 0.0;
                foreach (var which in new[]
                         {
                             WindowUtilsInterop.NSWindowButton.CloseButton,
                             WindowUtilsInterop.NSWindowButton.MiniaturizeButton,
                             WindowUtilsInterop.NSWindowButton.ZoomButton
                         })
                {
                    var button = GetButton(_window, which);
                    if (button != IntPtr.Zero)
                    {
                        var frame = WindowUtilsInterop.GetFrame(button);
                        trailingEdge = Math.Max(trailingEdge, frame.Origin.X + frame.Size.Width);
                    }
                }

                // Button frames are in the title bar view's bounds, which the fusion offsets.
                var boundsOffsetX = _offsetTitleBarView == IntPtr.Zero
                    ? 0
                    : WindowUtilsInterop.GetRect(_offsetTitleBarView, BoundsSelector).Origin.X;
                strip = new TitleStripState(
                    TopOffset: windowTop - (stripFrame.Origin.Y + stripFrame.Size.Height),
                    // Without the expected container the strip window itself is the best known extent of the strip.
                    Height: container == IntPtr.Zero
                        ? stripFrame.Size.Height
                        : WindowUtilsInterop.GetFrame(container).Size.Height,
                    Alpha: WindowUtilsInterop.objc_msgSend_double(_stripWindow, AlphaValueSelector),
                    IsFused: _isFused,
                    ButtonsTrailingEdge: stripFrame.Origin.X - windowFrame.Origin.X + trailingEdge - boundsOffsetX +
                                         ControlTrailingMargin);
            }

            var metrics = MacOSFullScreenChromeGeometry.Compute(persistentInset, strip);
            if (metrics == Metrics)
            {
                return;
            }

            Metrics = metrics;
            _metricsChanged();
        }
    }
}
