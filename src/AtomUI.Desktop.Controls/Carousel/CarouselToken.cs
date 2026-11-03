using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class CarouselToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// 指示点宽度
    /// Width of indicator
    /// </summary>
    public double IndicatorWidth { get; set; }
    
    /// <summary>
    /// 指示点高度
    /// Height of indicator
    /// </summary>
    public double IndicatorHeight { get; set; }
    
    /// <summary>
    /// 指示点之间的间距
    /// gap between indicator
    /// </summary>
    public double IndicatorGap { get; set; }
    
    /// <summary>
    /// 指示点距离边缘的距离
    /// dot offset to Carousel edge
    /// </summary>
    public double PaginationOffset { get; set; }
    
    /// <summary>
    /// 激活态指示点宽度
    /// Width of active indicator
    /// </summary>
    public double IndicatorActiveWidth { get; set; }
    
    /// <summary>
    /// 切换箭头大小
    /// Size of arrows
    /// </summary>
    public double ArrowSize { get; set; }
    
    /// <summary>
    /// 切换箭头边距
    /// arrows offset to Carousel edge
    /// </summary>
    public double ArrowOffset { get; set; }
    
    [ControlTokenDefault(nameof(IndicatorActiveWidth))]
    private static double DefaultForIndicatorActiveWidth(in ControlTokenDefaultContext context) => 24;

    [ControlTokenDefault(nameof(ArrowSize))]
    private static double DefaultForArrowSize(in ControlTokenDefaultContext context) => 16;

    [ControlTokenDefault(nameof(ArrowOffset))]
    private static double DefaultForArrowOffset(in ControlTokenDefaultContext context) => context.Global.SpacingXS;

    [ControlTokenDefault(nameof(IndicatorWidth))]
    private static double DefaultForIndicatorWidth(in ControlTokenDefaultContext context) => 16;

    [ControlTokenDefault(nameof(IndicatorHeight))]
    private static double DefaultForIndicatorHeight(in ControlTokenDefaultContext context) => 3;

    [ControlTokenDefault(nameof(IndicatorGap))]
    private static double DefaultForIndicatorGap(in ControlTokenDefaultContext context) => context.Global.UniformlyMarginXS;

    [ControlTokenDefault(nameof(PaginationOffset))]
    private static double DefaultForPaginationOffset(in ControlTokenDefaultContext context) => 12;
}
