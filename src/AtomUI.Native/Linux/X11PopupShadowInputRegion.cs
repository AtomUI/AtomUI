using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Native.Linux;

/// <summary>
/// X11 窗口型 Popup 的阴影穿透：用 SHAPE 扩展的 <c>ShapeInput</c> 把输入区域收缩到内容形状，
/// 可见区域不变，阴影仍与内容同一 surface 同帧绘制。
/// <para>
/// 与 Windows 的两层方案不同，X 服务器按输入形状选择接收窗口，穿透对同进程与跨进程窗口一致生效，
/// 无需轮询光标。输入形状是预先提交的区域而非逐点查询，因此在布局变化与动效状态变化时重新计算，
/// 内容未变时不重复下发请求。
/// </para>
/// <para>
/// GPU 渲染时 Avalonia 在 Popup 窗口下创建同尺寸的渲染子窗口。X 服务器查找指针所在窗口时，
/// 父窗口输入形状外的点不会再进入其子窗口，因此只需设置顶层 XID（与 <c>X11WindowChromeManager</c> 一致）。
/// </para>
/// </summary>
[SupportedOSPlatform("linux")]
internal sealed class X11PopupShadowInputRegion : IPopupShadowHitTest
{
    private readonly TopLevel _popupRoot;
    private readonly IntPtr _handle;
    private readonly Func<bool> _isInputSuspended;
    private readonly Func<PopupContentShape?> _getContentShape;
    // null 表示当前为默认输入形状（整个窗口）
    private PixelRect[]? _appliedRegion;
    private bool _isDisposed;

    private X11PopupShadowInputRegion(
        TopLevel popupRoot,
        IntPtr handle,
        Func<bool> isInputSuspended,
        Func<PopupContentShape?> getContentShape)
    {
        _popupRoot             = popupRoot;
        _handle                = handle;
        _isInputSuspended      = isInputSuspended;
        _getContentShape       = getContentShape;
        _popupRoot.LayoutUpdated += HandleLayoutUpdated;
        Invalidate();
    }

    public static X11PopupShadowInputRegion? TryCreate(
        TopLevel popupRoot,
        Func<bool> isInputSuspended,
        Func<PopupContentShape?> getContentShape)
    {
        ArgumentNullException.ThrowIfNull(popupRoot);
        ArgumentNullException.ThrowIfNull(isInputSuspended);
        ArgumentNullException.ThrowIfNull(getContentShape);
        if (TryGetX11Handle(popupRoot) is not { } handle)
        {
            return null;
        }

        return new X11PopupShadowInputRegion(popupRoot, handle, isInputSuspended, getContentShape);
    }

    private static IntPtr? TryGetX11Handle(TopLevel popupRoot)
    {
        // Wayland 等非 X11 后端的句柄描述不是 XID
        if (popupRoot.TryGetPlatformHandle() is not { HandleDescriptor: "XID", Handle: var handle } ||
            handle == IntPtr.Zero ||
            !XcbConnectionHolder.IsShapeSupported)
        {
            return null;
        }

        return handle;
    }

    public static void SuspendInputBeforeShow(TopLevel popupRoot)
    {
        if (TryGetX11Handle(popupRoot) is { } handle)
        {
            WindowUtilsLinux.SetInputRectangles(handle, []);
        }
    }

    public void Invalidate()
    {
        if (_isDisposed || _popupRoot.PlatformImpl is null)
        {
            return;
        }

        // 动效期间空区域，整窗穿透；动效结束后由 Popup 通知恢复为内容形状
        List<PixelRect>? region = null;
        if (_isInputSuspended())
        {
            region = [];
        }
        else if (_getContentShape() is { } shape)
        {
            region = PopupContentRegion.Rasterize(shape, _popupRoot.RenderScaling);
        }

        if (region is null)
        {
            if (_appliedRegion is not null)
            {
                _appliedRegion = null;
                WindowUtilsLinux.ClearInputShape(_handle);
            }
            return;
        }

        var regionArray = region.ToArray();
        if (_appliedRegion is not null && _appliedRegion.AsSpan().SequenceEqual(regionArray))
        {
            return;
        }

        _appliedRegion = regionArray;
        WindowUtilsLinux.SetInputRectangles(_handle, regionArray);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _popupRoot.LayoutUpdated -= HandleLayoutUpdated;
        // Popup 关闭时 Avalonia 先销毁原生窗口并置空 PlatformImpl，再触发 Popup.Closed；
        // 窗口仍存活时才恢复默认输入形状，避免对已销毁的 XID 发请求
        if (_appliedRegion is not null && _popupRoot.PlatformImpl is not null)
        {
            WindowUtilsLinux.ClearInputShape(_handle);
        }
    }

    private void HandleLayoutUpdated(object? sender, EventArgs e)
    {
        Invalidate();
    }
}
