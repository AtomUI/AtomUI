using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class SeparatorToken : AbstractControlDesignToken
{

    public SeparatorToken()

    {
    }

    /// <summary>
    /// 文本横向内间距 单位 em
    /// </summary>
    public double TextPaddingInline { get; set; }

    /// <summary>
    /// 文本与边缘距离的比例，取值 0 ～ 1
    /// </summary>
    public double OrientationMarginPercent { get; set; }

    /// <summary>
    /// 纵向分割线的横向外间距
    /// </summary>
    public double VerticalMarginInline { get; set; }
    
    /// <summary>
    /// 横向分割线的垂直外间距（小号）
    /// </summary>
    public Thickness HorizontalMarginBlockSM { get; set; }
    
    /// <summary>
    /// 横向分割线的垂直外间距
    /// </summary>
    public Thickness HorizontalMarginBlock { get; set; }
    
    /// <summary>
    /// 横向分割线的垂直外间距（大号）
    /// </summary>
    public Thickness HorizontalMarginBlockLG { get; set; }
    
    /// <summary>
    /// 带文本的水平分割线的外边距
    /// Horizontal margin of divider with text
    /// </summary>
    public Thickness HorizontalWithTextGutterMargin { get; set; }

    /// <summary>
    /// 带文本分割线的标题字重（非 Plain 样式）
    /// Font weight of the title of a divider with text (not plain)
    /// </summary>
    public FontWeight TitleFontWeight { get; set; }
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TextPaddingInline              = 1.0;
        OrientationMarginPercent       = 0.05;
        VerticalMarginInline           = EffectiveGlobalToken.UniformlyMarginXS;
        HorizontalMarginBlockSM        = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS);
        HorizontalMarginBlock          = new Thickness(0, EffectiveGlobalToken.UniformlyMargin);
        HorizontalMarginBlockLG        = new Thickness(0, EffectiveGlobalToken.UniformlyMarginLG);
        HorizontalWithTextGutterMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMargin);
        // antd Divider renders the title of a divider with text at weight 500 (a fixed value, not fontWeightStrong).
        TitleFontWeight                = FontWeight.Medium;
    }
    
}
