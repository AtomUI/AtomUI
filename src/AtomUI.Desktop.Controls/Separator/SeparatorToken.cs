using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class SeparatorToken : AbstractControlDesignToken, IControlTokenDefaults
{

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

    [ControlTokenDefault(nameof(TextPaddingInline))]
    private static double DefaultForTextPaddingInline(in ControlTokenDefaultContext context) =>
        1.0;

    [ControlTokenDefault(nameof(OrientationMarginPercent))]
    private static double DefaultForOrientationMarginPercent(in ControlTokenDefaultContext context) =>
        0.05;

    [ControlTokenDefault(nameof(VerticalMarginInline))]
    private static double DefaultForVerticalMarginInline(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXS;

    [ControlTokenDefault(nameof(HorizontalMarginBlockSM))]
    private static Thickness DefaultForHorizontalMarginBlockSM(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXS);

    [ControlTokenDefault(nameof(HorizontalMarginBlock))]
    private static Thickness DefaultForHorizontalMarginBlock(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMargin);

    [ControlTokenDefault(nameof(HorizontalMarginBlockLG))]
    private static Thickness DefaultForHorizontalMarginBlockLG(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginLG);

    [ControlTokenDefault(nameof(HorizontalWithTextGutterMargin))]
    private static Thickness DefaultForHorizontalWithTextGutterMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMargin);

}
