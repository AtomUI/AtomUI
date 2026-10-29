using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Native;

/// <summary>
/// 窗口型 Popup 阴影区域的输入穿透。阴影与内容画在同一窗口 surface 中，只把输入区域收缩到内容形状。
/// </summary>
internal static class NativePopupShadowHitTest
{
    /// <param name="popupRoot">窗口型 Popup 的 TopLevel（独立原生窗口）。</param>
    /// <param name="isInputSuspended">为 true 时整窗穿透（打开/关闭动画期间）。</param>
    /// <param name="isInContentShape">客户区 DIP 坐标是否落在内容形状（圆角矩形 + 箭头）内。</param>
    /// <returns>当前平台不支持时返回 <c>null</c>。</returns>
    public static IDisposable? TryAttach(
        TopLevel popupRoot,
        Func<bool> isInputSuspended,
        Func<Point, bool> isInContentShape)
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        return Windows.WindowsPopupShadowHitTestHook.TryCreate(popupRoot, isInputSuspended, isInContentShape);
    }
}
