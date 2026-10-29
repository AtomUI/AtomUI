using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Native;

/// <summary>
/// 一次打开会话内的窗口型 Popup 阴影输入穿透。
/// </summary>
internal interface IPopupShadowHitTest : IDisposable
{
    /// <summary>
    /// 输入暂停状态或内容形状可能已变化。逐点查询的平台（Windows）忽略；
    /// 预先提交输入区域的平台（X11）据此重新计算并提交。
    /// </summary>
    void Invalidate();
}

/// <summary>
/// 窗口型 Popup 箭头所在的一侧。
/// </summary>
internal enum PopupArrowSide
{
    None,
    Left,
    Top,
    Right,
    Bottom
}

/// <summary>
/// 窗口型 Popup 内容形状的平台无关描述（客户区 DIP 坐标，不含阴影）：
/// 圆角矩形主体 + 一个底边贴住主体、顶点朝外的箭头三角形。
/// </summary>
internal readonly record struct PopupContentShape(
    Rect Body,
    CornerRadius CornerRadius,
    Rect ArrowBounds,
    PopupArrowSide ArrowSide);

/// <summary>
/// 窗口型 Popup 阴影区域的输入穿透。阴影与内容画在同一窗口 surface 中，只把输入区域收缩到内容形状。
/// </summary>
internal static class NativePopupShadowHitTest
{
    /// <summary>
    /// 原生窗口映射（显示）前先让整窗对输入透明，由随后的 <see cref="TryAttach"/> 接管。
    /// X11：窗口带默认（整窗）输入形状映射到指针下时，X 服务器立即给下方窗口发送 Leave，
    /// 下方控件丢失悬停（Hover 浮层因此关闭再打开）；映射前设好空输入形状可避免这次越界事件。
    /// Windows 按 <c>WM_NCHITTEST</c> 逐点查询，不需要也不做处理。
    /// </summary>
    /// <param name="popupRoot">尚未映射的窗口型 Popup TopLevel。</param>
    public static void SuspendInputBeforeShow(TopLevel popupRoot)
    {
        if (OperatingSystem.IsLinux())
        {
            Linux.X11PopupShadowInputRegion.SuspendInputBeforeShow(popupRoot);
        }
    }

    /// <param name="popupRoot">窗口型 Popup 的 TopLevel（独立原生窗口）。</param>
    /// <param name="isInputSuspended">为 true 时整窗穿透（打开/关闭动画期间）。</param>
    /// <param name="isInContentShape">客户区 DIP 坐标是否落在内容形状（圆角矩形 + 箭头）内，供逐点命中的平台使用。</param>
    /// <param name="getContentShape">
    /// 内容形状几何，供预先提交输入区域的平台使用；返回 <c>null</c> 表示无法计算，此时不限制输入区域。
    /// </param>
    /// <returns>当前平台不支持时返回 <c>null</c>。</returns>
    public static IPopupShadowHitTest? TryAttach(
        TopLevel popupRoot,
        Func<bool> isInputSuspended,
        Func<Point, bool> isInContentShape,
        Func<PopupContentShape?> getContentShape)
    {
        if (OperatingSystem.IsWindows())
        {
            return Windows.WindowsPopupShadowHitTestHook.TryCreate(popupRoot, isInputSuspended, isInContentShape);
        }

        if (OperatingSystem.IsLinux())
        {
            return Linux.X11PopupShadowInputRegion.TryCreate(popupRoot, isInputSuspended, getContentShape);
        }

        return null;
    }
}
