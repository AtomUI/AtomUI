using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Native.Windows;

/// <summary>
/// Windows 10 扩展客户区窗口在 RedirectionSurface 下的 DWM 边框修正。
/// </summary>
/// <remarks>
/// Avalonia 在使用 redirection bitmap 时把 DWM 框架向客户区四边各扩展 1px
/// （<c>WindowImpl.UpdateExtendMargins</c>：<c>defaultMargin = UseRedirectionBitmap ? 1 : 0</c>）。
/// Win10 DWM 会把激活态的 1px 可见边框画在这层扩展玻璃下方，被不透明的客户区内容覆盖，
/// 表现为激活时左、右、下三边边框消失，只有非激活态可见。把扩展边距归零后，
/// DWM 按系统原生方式绘制半透明边框，激活与非激活状态都与原生窗口逐像素一致。
/// Avalonia 在显示、窗口状态、全屏与扩展提示变化时会重新写入 1px，因此在这些操作触发的
/// 窗口消息之后延迟到同步调用链结束再重新归零。Windows 11 的边框由 DWM 在窗口内侧绘制，
/// 不受该边距影响，因此不创建本 hook。
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
                // live resize 期间 Avalonia 不会重写边距，不在每帧调用 DWM。
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
        // Avalonia 在 ShowWindow / SetFullScreen 等同步调用链中先触发窗口消息、后写入边距，
        // 延迟到调用链结束后再归零。
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
