using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SeparatorToken : AbstractControlDesignToken
{

    /// <summary>Horizontal padding around the label, expressed in em units.</summary>
    public partial double TextPaddingInline { get; set; }

    /// <summary>Ratio from zero to one that positions an offset label between the ends of the separator.</summary>
    public partial double OrientationMarginPercent { get; set; }

    /// <summary>Horizontal space around a vertical separator.</summary>
    public partial double VerticalMarginInline { get; set; }
    
    /// <summary>Vertical space around a small horizontal separator.</summary>
    public partial Thickness HorizontalMarginBlockSM { get; set; }
    
    /// <summary>Vertical space around a regular horizontal separator.</summary>
    public partial Thickness HorizontalMarginBlock { get; set; }
    
    /// <summary>Vertical space around a large horizontal separator.</summary>
    public partial Thickness HorizontalMarginBlockLG { get; set; }
    
    /// <summary>
    /// 带文本的水平分割线的外边距
    /// Horizontal margin of divider with text
    /// </summary>
    public partial Thickness HorizontalWithTextGutterMargin { get; set; }

    /// <summary>
    /// 带文本分割线的标题字重（非 Plain 样式）
    /// Font weight of the title of a divider with text (not plain)
    /// </summary>
    public partial FontWeight TitleFontWeight { get; set; }
    
}
