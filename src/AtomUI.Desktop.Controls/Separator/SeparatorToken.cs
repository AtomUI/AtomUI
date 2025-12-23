using AtomUI.Theme.TokenSystem;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class SeparatorToken : AbstractControlDesignToken
{
    public const string ID = "Separator";

    public SeparatorToken()
        : base(ID)
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
    
    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TextPaddingInline              = 1.0;
        OrientationMarginPercent       = 0.05;
        VerticalMarginInline           = SharedToken.UniformlyMarginXS;
        HorizontalMarginBlockSM        = new Thickness(0, SharedToken.UniformlyMarginXS);
        HorizontalMarginBlock          = new Thickness(0, SharedToken.UniformlyMargin);
        HorizontalMarginBlockLG        = new Thickness(0, SharedToken.UniformlyMarginLG);
        HorizontalWithTextGutterMargin = new Thickness(0, SharedToken.UniformlyMargin);
    }
}