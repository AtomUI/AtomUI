using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CountBadgeToken : AbstractControlDesignToken
{
    /// <summary>Height and minimum width of the regular count indicator.</summary>
    public partial double IndicatorHeight { get; set; }

    /// <summary>Height and minimum width of the small count indicator.</summary>
    public partial double IndicatorHeightSM { get; set; }

    /// <summary>Font size of the regular count label.</summary>
    public partial double TextFontSize { get; set; }

    /// <summary>Font size of the small count label.</summary>
    public partial double TextFontSizeSM { get; set; }

    /// <summary>The font weight of the count text.</summary>
    public partial FontWeight TextFontWeight { get; set; }

    /// <summary>Count label foreground on <see cref="IndicatorColor"/>.</summary>
    public partial Color TextColor { get; set; }

    /// <summary>Fill of the count indicator. Pair with <see cref="TextColor"/>.</summary>
    public partial Color IndicatorColor { get; set; }

    /// <summary>Thickness of the outline shadow separating the badge from its target.</summary>
    public partial double ShadowSize { get; set; }

    /// <summary>Color of the outline shadow defined by <see cref="ShadowSize"/>.</summary>
    public partial Color ShadowColor { get; set; }

    /// <summary>Space between the badge edge and count label.</summary>
    public partial Thickness TextPadding { get; set; }

    /// <summary>Corner radius of the regular count indicator.</summary>
    public partial CornerRadius CornerRadius { get; set; }

    /// <summary>Corner radius of the small count indicator.</summary>
    public partial CornerRadius CornerRadiusSM { get; set; }
}
