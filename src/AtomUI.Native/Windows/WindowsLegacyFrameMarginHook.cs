using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Native.Windows;

/// <summary>
/// DWM border correction for a Windows 10 extended-client-area window under a RedirectionSurface.
/// </summary>
/// <remarks>
/// <para>
/// When Avalonia uses the redirection bitmap, it extends the DWM frame 1px into the client area on all four
/// sides (<c>WindowImpl.UpdateExtendMargins</c>: <c>defaultMargin = UseRedirectionBitmap ? 1 : 0</c>).
/// The Windows 10 DWM paints the visible 1px border of the active window below this extended layer of glass,
/// where the opaque client content covers it. The left, right and bottom borders therefore disappear while the
/// window is active and are visible only while it is inactive. With the extended margins set to zero, the DWM
/// paints the translucent border the way it does for native windows, and the active and the inactive state both
/// match a native window pixel for pixel.
/// </para>
/// <para>
/// Avalonia writes the 1px margin again when the window is shown and when the window state, the full-screen
/// state or an extend hint changes. The margins are therefore zeroed again only after the window messages that
/// these operations raise, at the end of the synchronous call chain. Windows 11 has the DWM paint its border on
/// the inner side of the window and is not affected by the margin, so this hook is not created there.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class WindowsLegacyFrameMarginHook : IDisposable
{
    private readonly Window _window;
    private bool _isSizeMoving;
    private bool _isResetQueued;
    private bool _isDisposed;

    private WindowsLegacyFrameMarginHook(Window window)
    {
        _window = window;
        Win32Properties.AddWndProcHookCallback(_window, HandleWndProc);
    }

    public static WindowsLegacyFrameMarginHook? TryCreate(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        return IsLegacyWindows10()
            ? new WindowsLegacyFrameMarginHook(window)
            : null;
    }

    internal static bool IsLegacyWindows10()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240) &&
               !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        Win32Properties.RemoveWndProcHookCallback(_window, HandleWndProc);
    }

    private IntPtr HandleWndProc(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (_isDisposed)
        {
            return IntPtr.Zero;
        }

        switch (msg)
        {
            case WindowUtilsInterop.WM_ENTERSIZEMOVE:
                _isSizeMoving = true;
                break;
            case WindowUtilsInterop.WM_EXITSIZEMOVE:
                _isSizeMoving = false;
                QueueReset();
                break;
            case WindowUtilsInterop.WM_WINDOWPOSCHANGED:
                // Avalonia does not rewrite the margins during a live resize, so DWM is not called on every frame.
                if (!_isSizeMoving)
                {
                    QueueReset();
                }
                break;
            case WindowUtilsInterop.WM_SHOWWINDOW:
            case WindowUtilsInterop.WM_ACTIVATE:
            case WindowUtilsInterop.WM_DPICHANGED:
                QueueReset();
                break;
        }

        return IntPtr.Zero;
    }

    private void QueueReset()
    {
        if (_isResetQueued)
        {
            return;
        }

        _isResetQueued = true;
        // In synchronous call chains such as ShowWindow / SetFullScreen, Avalonia raises the window messages first
        // and writes the margins afterwards, so the zeroing is deferred until the chain has ended.
        Dispatcher.UIThread.Post(ResetMargins, DispatcherPriority.Send);
    }

    private void ResetMargins()
    {
        _isResetQueued = false;
        if (_isDisposed ||
            !_window.ExtendClientAreaToDecorationsHint ||
            _window.TryGetPlatformHandle() is not { Handle: var handle } ||
            handle == IntPtr.Zero ||
            (WindowUtilsInterop.GetWindowLongPtr(handle, WindowUtilsInterop.GWL_EXSTYLE) &
             WindowUtilsInterop.WS_EX_NOREDIRECTIONBITMAP) != 0)
        {
            return;
        }

        var margins = new WindowUtilsInterop.MARGINS();
        WindowUtilsInterop.DwmExtendFrameIntoClientArea(handle, ref margins);
    }
}
