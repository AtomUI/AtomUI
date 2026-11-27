using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the status dot of <see cref="DotBadge" />: the diameter and fill of the dot, the ring
/// around it, and the margin of the label next to it.
/// </summary>
[ControlDesignToken]
public sealed partial class DotBadgeToken : AbstractControlDesignToken
{
    /// <summary>
    /// Diameter of the badge dot.
    /// </summary>
    public partial double DotSize { get; set; }

    /// <summary>
    /// Fill color of the badge dot.
    /// </summary>
    public partial Color DotColor { get; set; }

    /// <summary>
    /// Spread of the ring around the badge dot.
    /// </summary>
    public partial double ShadowSize { get; set; }

    /// <summary>
    /// Color of the ring around the badge dot.
    /// </summary>
    public partial Color ShadowColor { get; set; }

    /// <summary>
    /// Margin around the label next to the badge dot.
    /// </summary>
    public partial Thickness LabelMargin { get; set; }
}
