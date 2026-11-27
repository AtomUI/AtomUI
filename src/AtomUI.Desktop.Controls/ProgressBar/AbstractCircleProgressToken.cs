using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens of the circle-shaped progress controls (<see cref="CircleProgress" /> and <see cref="DashboardProgress"
/// />).
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractCircleProgressToken : AbstractProgressToken
{
    /// <summary>
    /// Text color of the progress info in the center of a circle or dashboard, for example the percentage label.
    /// It is the Foreground of the control while the control is enabled (antd Progress circleTextColor).
    /// </summary>
    public partial Color CircleTextColor { get; set; }

    /// <summary>
    /// Lower bound for the font size of the percentage label in the center of a circle or dashboard. Read only while
    /// <c>UseExtraInfoSizeTokenResources</c> is <see langword="true" />.
    /// </summary>
    public partial double CircleMinimumTextFontSize { get; set; }

    /// <summary>
    /// Lower bound for the size of the status icon in the center of a circle or dashboard. Read only while
    /// <c>UseExtraInfoSizeTokenResources</c> is <see langword="true" />.
    /// </summary>
    public partial double CircleMinimumIconSize { get; set; }
}
