using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Native.Windows;

/// <summary>
/// Shadow pass-through for a windowed Popup, in two layers:
/// <list type="number">
/// <item>
/// Same process: <c>WM_NCHITTEST</c> returns <c>HTTRANSPARENT</c> over the shadow, and the system hands the
/// mouse events to the window below on the same thread (the main window or another Popup).
/// </item>
/// <item>
/// Other processes: while the cursor is over the shadow outside the owner main window,
/// <c>WS_EX_TRANSPARENT</c> is added temporarily so that the events go to the programs below. The cursor is
/// then polled every frame, and the style is removed when the cursor returns to the content shape, leaves the
/// Popup or returns inside the main window.
/// </item>
/// </list>
/// <c>WS_EX_TRANSPARENT</c> passes events to other processes only on a <c>WS_EX_LAYERED</c> window, so the hook
/// sets <c>WS_EX_LAYERED</c> plus a whole-window alpha of 255 when it is attached. At run time only
/// <c>WS_EX_TRANSPARENT</c> is toggled, which triggers no redraw.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class WindowsPopupShadowHitTestHook : IPopupShadowHitTest
{
    // Polls the cursor every frame, about 60 Hz.
    private static readonly TimeSpan PassThroughPollInterval = TimeSpan.FromMilliseconds(16);

    private readonly TopLevel _popupRoot;
    private readonly IntPtr _hwnd;
    private readonly long _originalExStyle;
    private readonly Func<bool> _isInputSuspended;
    private readonly Func<Point, bool> _isInContentShape;
    private DispatcherTimer? _passThroughTimer;
    private bool _isWindowDestroyed;
    private bool _isDisposed;

    private WindowsPopupShadowHitTestHook(
        TopLevel popupRoot,
        IntPtr hwnd,
        Func<bool> isInputSuspended,
        Func<Point, bool> isInContentShape)
    {
        _popupRoot        = popupRoot;
        _hwnd             = hwnd;
        _isInputSuspended = isInputSuspended;
        _isInContentShape = isInContentShape;
        _originalExStyle  = WindowUtilsInterop.GetWindowLongPtr(hwnd, WindowUtilsInterop.GWL_EXSTYLE);

        if ((_originalExStyle & WindowUtilsInterop.WS_EX_LAYERED) == 0)
        {
            WindowUtilsInterop.SetWindowLongPtr(hwnd, WindowUtilsInterop.GWL_EXSTYLE,
                _originalExStyle | WindowUtilsInterop.WS_EX_LAYERED);
        }
        // A whole-window alpha of 255 leaves per-pixel transparency unchanged: Avalonia still composites the
        // content transparently, and the DWM only multiplies it by an extra 1.0.
        WindowUtilsInterop.SetLayeredWindowAttributes(hwnd, 0, 255, WindowUtilsInterop.LWA_ALPHA);
        Win32Properties.AddWndProcHookCallback(popupRoot, HandleWndProc);
    }

    public static WindowsPopupShadowHitTestHook? TryCreate(
        TopLevel popupRoot,
        Func<bool> isInputSuspended,
        Func<Point, bool> isInContentShape)
    {
        ArgumentNullException.ThrowIfNull(popupRoot);
        ArgumentNullException.ThrowIfNull(isInputSuspended);
        ArgumentNullException.ThrowIfNull(isInContentShape);
        if (popupRoot.TryGetPlatformHandle() is not { Handle: var hwnd } || hwnd == IntPtr.Zero)
        {
            return null;
        }

        return new WindowsPopupShadowHitTestHook(popupRoot, hwnd, isInputSuspended, isInContentShape);
    }

    internal bool IsPassThroughActive => _passThroughTimer is not null;

    // WM_NCHITTEST queries the motion state and the content shape live each time; there is no cache to refresh.
    public void Invalidate()
    {
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        StopPassThroughPolling();
        Win32Properties.RemoveWndProcHookCallback(_popupRoot, HandleWndProc);
        if (!_isWindowDestroyed)
        {
            WindowUtilsInterop.SetWindowLongPtr(_hwnd, WindowUtilsInterop.GWL_EXSTYLE, _originalExStyle);
        }
    }

    private IntPtr HandleWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (_isDisposed)
        {
            return IntPtr.Zero;
        }

        switch (msg)
        {
            case WindowUtilsInterop.WM_NCHITTEST:
                var screenPoint = new PixelPoint(
                    unchecked((short)(long)lParam),
                    unchecked((short)((long)lParam >> 16)));
                if (!IsShadowHit(screenPoint))
                {
                    // Inside the content shape: hand the event back to Avalonia.
                    return IntPtr.Zero;
                }

                if (!IsInsideOwnerWindow(screenPoint))
                {
                    // HTTRANSPARENT forwards only between windows of the same thread. Outside the main window,
                    // WS_EX_TRANSPARENT hands the events over to other programs.
                    BeginPassThrough();
                }

                handled = true;
                return new IntPtr(WindowUtilsInterop.HTTRANSPARENT);
            case WindowUtilsInterop.WM_DESTROY:
                _isWindowDestroyed = true;
                StopPassThroughPolling();
                break;
        }

        return IntPtr.Zero;
    }

    private bool IsShadowHit(PixelPoint screenPoint)
    {
        return _isInputSuspended() || !_isInContentShape(_popupRoot.PointToClient(screenPoint));
    }

    private void BeginPassThrough()
    {
        if (_passThroughTimer is not null)
        {
            return;
        }

        SetTransparentExStyle(true);
        _passThroughTimer = new DispatcherTimer(PassThroughPollInterval, DispatcherPriority.Input, HandlePassThroughPoll);
        _passThroughTimer.Start();
    }

    private void HandlePassThroughPoll(object? sender, EventArgs e)
    {
        if (_isDisposed || _isWindowDestroyed)
        {
            StopPassThroughPolling();
            return;
        }

        if (!WindowUtilsInterop.GetCursorPos(out var cursor))
        {
            return;
        }

        var screenPoint = new PixelPoint(cursor.X, cursor.Y);
        // When the cursor leaves the Popup or returns inside the main window, input goes back to the first layer:
        // the next entry into the Popup receives WM_NCHITTEST again.
        if (!WindowUtilsInterop.GetWindowRect(_hwnd, out var popupRect) ||
            !popupRect.Contains(cursor.X, cursor.Y) ||
            IsInsideOwnerWindow(screenPoint) ||
            !IsShadowHit(screenPoint))
        {
            StopPassThroughPolling();
        }
    }

    private void StopPassThroughPolling()
    {
        if (_passThroughTimer is null)
        {
            return;
        }

        _passThroughTimer.Stop();
        _passThroughTimer.Tick -= HandlePassThroughPoll;
        _passThroughTimer = null;
        if (!_isWindowDestroyed)
        {
            SetTransparentExStyle(false);
        }
    }

    private void SetTransparentExStyle(bool isTransparent)
    {
        var style = WindowUtilsInterop.GetWindowLongPtr(_hwnd, WindowUtilsInterop.GWL_EXSTYLE);
        var newStyle = isTransparent
            ? style | WindowUtilsInterop.WS_EX_TRANSPARENT
            : style & ~WindowUtilsInterop.WS_EX_TRANSPARENT;
        if (newStyle != style)
        {
            // Only the extended style changes. SetWindowPos(SWP_FRAMECHANGED) is not called, so nothing is redrawn.
            WindowUtilsInterop.SetWindowLongPtr(_hwnd, WindowUtilsInterop.GWL_EXSTYLE, newStyle);
        }
    }

    private bool IsInsideOwnerWindow(PixelPoint screenPoint)
    {
        var owner = WindowUtilsInterop.GetAncestor(_hwnd, WindowUtilsInterop.GA_ROOTOWNER);
        if (owner == IntPtr.Zero || owner == _hwnd)
        {
            return false;
        }

        // The extended frame bounds exclude the invisible resize border and match the extent of the main window
        // that the user sees.
        if (WindowUtilsInterop.DwmGetWindowRectAttribute(
                owner,
                WindowUtilsInterop.DWMWA_EXTENDED_FRAME_BOUNDS,
                out var ownerRect,
                System.Runtime.InteropServices.Marshal.SizeOf<WindowUtilsInterop.RECT>()) != WindowUtilsInterop.S_OK &&
            !WindowUtilsInterop.GetWindowRect(owner, out ownerRect))
        {
            return false;
        }

        return ownerRect.Contains(screenPoint.X, screenPoint.Y);
    }
}
