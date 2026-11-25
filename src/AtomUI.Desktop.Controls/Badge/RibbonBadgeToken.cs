using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media.Immutable;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class RibbonBadgeToken : AbstractControlDesignToken
{
    /// <summary>Horizontal and vertical offsets of the ribbon from its adorned element.</summary>
    public partial Point Offset { get; set; }

    /// <summary>Transform that shapes the folded corner of the ribbon.</summary>
    public partial ImmutableTransform? CornerTransform { get; set; }

    /// <summary>Amount by which the ribbon's folded corner is darkened.</summary>
    public partial int CornerDarkenAmount { get; set; }

    /// <summary>Padding around the ribbon label.</summary>
    public partial Thickness TextPadding { get; set; }
}
