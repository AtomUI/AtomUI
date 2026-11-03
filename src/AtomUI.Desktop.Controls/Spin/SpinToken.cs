using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class SpinToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 加载圆圈尺寸
    /// </summary>
    public double DotSize { get; set; }

    /// <summary>
    /// 小号加载圆圈尺寸
    /// </summary>
    public double DotSizeSM { get; set; }

    /// <summary>
    /// 大号加载圆圈尺寸
    /// </summary>
    public double DotSizeLG { get; set; }
    
    /// <summary>
    /// 大号加载器尺寸
    /// </summary>
    public double IndicatorSizeLG { get; set; }
    
    /// <summary>
    /// 加载器尺寸
    /// </summary>
    public double IndicatorSize { get; set; }
    
    /// <summary>
    /// 小号加载器尺寸
    /// </summary>
    public double IndicatorSizeSM { get; set; }

    /// <summary>
    /// 加载器的周期时间
    /// </summary>
    public TimeSpan IndicatorDuration { get; set; }

    [ControlTokenDefault(nameof(IndicatorDuration))]
    private static TimeSpan DefaultForIndicatorDuration(in ControlTokenDefaultContext context) => context.Global.MotionDurationSlow * 4;

    [ControlTokenDefault(nameof(DotSize))]
    private static double DefaultForDotSize(in ControlTokenDefaultContext context) => ((context.Global.ControlHeightLG / 2 - context.Global.UniformlyMarginXXS / 2) / 2) * 0.75;

    [ControlTokenDefault(nameof(DotSizeLG))]
    private static double DefaultForDotSizeLG(in ControlTokenDefaultContext context) => ((context.Global.ControlHeight - context.Global.UniformlyMarginXXS) / 2) * 0.75;

    [ControlTokenDefault(nameof(DotSizeSM))]
    private static double DefaultForDotSizeSM(in ControlTokenDefaultContext context) => ((context.Global.ControlHeightLG * 0.35 - context.Global.UniformlyMarginXXS / 2) / 2) * 0.75;

    [ControlTokenDefault(nameof(IndicatorSize))]
    private static double DefaultForIndicatorSize(in ControlTokenDefaultContext context) => context.Global.ControlHeightLG / 2 + 2;

    [ControlTokenDefault(nameof(IndicatorSizeSM))]
    private static double DefaultForIndicatorSizeSM(in ControlTokenDefaultContext context) => context.Global.ControlHeightLG * 0.35 + 1;

    [ControlTokenDefault(nameof(IndicatorSizeLG))]
    private static double DefaultForIndicatorSizeLG(in ControlTokenDefaultContext context) => context.Global.ControlHeight + 4;
}
