using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Native.Windows;

/// <summary>
/// 窗口型 Popup 的阴影穿透，分两层：
/// <list type="number">
/// <item>同进程：<c>WM_NCHITTEST</c> 在阴影上返回 <c>HTTRANSPARENT</c>，系统把鼠标事件交给同线程下方的窗口（主窗口、其他 Popup）。</item>
/// <item>跨进程：光标位于所属主窗口外的阴影上时临时加 <c>WS_EX_TRANSPARENT</c>，事件交给下方其他程序；
/// 之后按帧轮询光标，回到内容形状、移出 Popup 或回到主窗口内时移除。</item>
/// </list>
/// <c>WS_EX_TRANSPARENT</c> 只在 <c>WS_EX_LAYERED</c> 窗口上跨进程生效，因此挂接时即设置
/// <c>WS_EX_LAYERED</c> + 整窗 alpha 255；运行时只切换 <c>WS_EX_TRANSPARENT</c>，不触发重绘。
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class WindowsPopupShadowHitTestHook : IPopupShadowHitTest
{
    // 按帧轮询光标，约 60Hz
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
        // 整窗 alpha 255 不改变逐像素透明：内容仍由 Avalonia 透明合成，DWM 只额外乘以 1.0
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

    // WM_NCHITTEST 每次实时查询动效状态与内容形状，没有需要刷新的缓存
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
                    // 内容形状内交还 Avalonia 处理
                    return IntPtr.Zero;
                }

                if (!IsInsideOwnerWindow(screenPoint))
                {
                    // HTTRANSPARENT 只在同线程窗口之间转发，主窗口外由 WS_EX_TRANSPARENT 交给其他程序
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
        // 光标移出 Popup 或回到主窗口内时交还第一层：下一次进入 Popup 会重新收到 WM_NCHITTEST
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
            // 只改扩展样式，不调用 SetWindowPos(SWP_FRAMECHANGED)，不会触发重绘
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

        // 扩展帧边界不含不可见的 resize 边框，与用户看到的主窗口范围一致
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
