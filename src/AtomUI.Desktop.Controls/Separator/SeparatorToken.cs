using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the separator: the space around horizontal and vertical separators, the padding and the
/// default position of the title, and the font weight of the title of a separator that is not plain.
/// </summary>
[ControlDesignToken]
public sealed partial class SeparatorToken : AbstractControlDesignToken
{

    /// <summary>
    /// Horizontal padding around the label, expressed in em units.
    /// </summary>
    public partial double TextPaddingInline { get; set; }

    /// <summary>
    /// Ratio from zero to one that positions an offset label between the ends of the separator.
    /// </summary>
    public partial double OrientationMarginPercent { get; set; }

    /// <summary>
    /// Horizontal space around a vertical separator.
    /// </summary>
    public partial double VerticalMarginInline { get; set; }

    /// <summary>
    /// Vertical inset above and below the rail of a vertical separator.
    /// </summary>
    public partial Thickness VerticalMarginBlock { get; set; }
    
    /// <summary>
    /// Vertical space around a small horizontal separator.
    /// </summary>
    public partial Thickness HorizontalMarginBlockSM { get; set; }
    
    /// <summary>
    /// Vertical space around a regular horizontal separator.
    /// </summary>
    public partial Thickness HorizontalMarginBlock { get; set; }
    
    /// <summary>
    /// Vertical space around a large horizontal separator.
    /// </summary>
    public partial Thickness HorizontalMarginBlockLG { get; set; }
    
    /// <summary>
    /// Margin around a horizontal separator of the Large size step while it shows a title. The Middle and Small size
    /// steps keep <see cref="HorizontalMarginBlock" /> and <see cref="HorizontalMarginBlockSM" /> with a title.
    /// </summary>
    public partial Thickness HorizontalWithTextGutterMargin { get; set; }

    /// <summary>
    /// Font weight of the separator title while <c>IsPlain</c> is false. A plain separator uses a normal weight.
    /// </summary>
    public partial FontWeight TitleFontWeight { get; set; }
    
}
