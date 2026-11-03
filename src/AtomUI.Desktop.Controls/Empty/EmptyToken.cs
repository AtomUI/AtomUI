using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class EmptyToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 空图片的高度
    /// </summary>
    public double EmptyImgHeight { get; set; }

    public double EmptyImgHeightSM { get; set; }
    public double EmptyImgHeightMD { get; set; }
    
    public Thickness DescriptionMargin { get; set; }
    public Thickness DescriptionMarginSM { get; set; }
    public Thickness FooterMargin { get; set; }



    [ControlTokenDefault(nameof(EmptyImgHeight))]
    private static double DefaultForEmptyImgHeight(in ControlTokenDefaultContext context) => context.Global.ControlHeightLG * 2.5;

    [ControlTokenDefault(nameof(EmptyImgHeightMD))]
    private static double DefaultForEmptyImgHeightMD(in ControlTokenDefaultContext context) => context.Global.ControlHeightLG * 1.85;

    [ControlTokenDefault(nameof(EmptyImgHeightSM))]
    private static double DefaultForEmptyImgHeightSM(in ControlTokenDefaultContext context) => context.Global.ControlHeightLG * 0.875;

    [ControlTokenDefault(nameof(DescriptionMargin))]
    private static Thickness DefaultForDescriptionMargin(in ControlTokenDefaultContext context) => new Thickness(0, context.Global.UniformlyMarginSM, 0, 0);

    [ControlTokenDefault(nameof(DescriptionMarginSM))]
    private static Thickness DefaultForDescriptionMarginSM(in ControlTokenDefaultContext context) => new Thickness(0, context.Global.UniformlyMarginXS, 0, 0);

    [ControlTokenDefault(nameof(FooterMargin))]
    private static Thickness DefaultForFooterMargin(in ControlTokenDefaultContext context) => new Thickness(0, context.Global.UniformlyMargin, 0, 0);
}
