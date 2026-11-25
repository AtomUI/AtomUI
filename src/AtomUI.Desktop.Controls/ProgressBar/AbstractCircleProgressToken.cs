using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens of the circle-shaped progress controls (<see cref="CircleProgress"/> and <see cref="DashboardProgress"/>).
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractCircleProgressToken : AbstractProgressToken
{
    /// <summary>
    /// 圆形进度条文字颜色
    /// Text color of the circle percentage label (antd Progress circleTextColor)
    /// </summary>
    public partial Color CircleTextColor { get; set; }

    /// <summary>
    /// 圆形进度条文本最小大小
    /// Minimum font size of the circle percentage label
    /// </summary>
    public partial double CircleMinimumTextFontSize { get; set; }

    /// <summary>
    /// 圆形进度条图标最小大小
    /// Minimum size of the circle status icon
    /// </summary>
    public partial double CircleMinimumIconSize { get; set; }
}
