using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TimelineToken : AbstractControlDesignToken, IControlTokenDefaults
{
    /// <summary>
    /// Timeline 轨迹颜色
    /// </summary>
    public Color IndicatorTailColor { get; set; }

    /// <summary>
    /// 轨迹宽度
    /// </summary>
    public double IndicatorTailWidth { get; set; }

    /// <summary>
    /// 时间项内间距
    /// </summary>
    public Thickness ItemPaddingBottom { get; set; }

    /// <summary>
    /// 时间项下大内间距
    /// </summary>
    public Thickness ItemPaddingBottomLG { get; set; }
    
    /// <summary>
    /// 最后一个 Item 的 Content 最小高度；仅明确 Own 配置时应用，未配置保留原生 0。
    /// </summary>
    public double LastItemContentMinHeight { get; set; }

    /// <summary>
    /// 节点指示器的大小
    /// </summary>
    public double IndicatorSize { get; set; }

    /// <summary>
    /// 节点指示器内置圆形大小
    /// </summary>
    public double IndicatorDotSize { get; set; }
    
    /// <summary>
    /// 指示器在 Start 模式下的外边距
    /// </summary>
    public Thickness IndicatorStartModeMargin { get; set; }
    
    /// <summary>
    /// 指示器在 End 模式下的外边距
    /// </summary>
    public Thickness IndicatorEndModeMargin { get; set; }
    
    /// <summary>
    /// 指示器在中间的外边距
    /// </summary>
    public Thickness IndicatorMiddleModeMargin { get; set; }

    /// <summary>
    /// 节点边框宽度
    /// </summary>
    public double IndicatorDotBorderWidth { get; set; }

    [ControlTokenDefault(nameof(IndicatorTailColor))]
    private static Color DefaultForIndicatorTailColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorSplit;

    [ControlTokenDefault(nameof(IndicatorTailWidth))]
    private static double DefaultForIndicatorTailWidth(in ControlTokenDefaultContext context) =>
        context.Global.LineWidthBold;

    [ControlTokenDefault(nameof(ItemPaddingBottom))]
    private static Thickness DefaultForItemPaddingBottom(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, 0, context.Global.UniformlyPadding * 1.25);

    [ControlTokenDefault(nameof(ItemPaddingBottomLG), nameof(ItemPaddingBottom))]
    private static Thickness DefaultForItemPaddingBottomLG(in ControlTokenDefaultContext context, Thickness itemPaddingBottom) =>
        itemPaddingBottom * 2;

    [ControlTokenDefault(nameof(LastItemContentMinHeight))]
    private static double DefaultForLastItemContentMinHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 1.2;

    [ControlTokenDefault(nameof(IndicatorSize))]
    private static double DefaultForIndicatorSize(in ControlTokenDefaultContext context) =>
        context.Global.SizeMS;

    [ControlTokenDefault(nameof(IndicatorDotSize))]
    private static double DefaultForIndicatorDotSize(in ControlTokenDefaultContext context) =>
        8;

    [ControlTokenDefault(nameof(IndicatorStartModeMargin))]
    private static Thickness DefaultForIndicatorStartModeMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyMargin, 0);

    [ControlTokenDefault(nameof(IndicatorEndModeMargin))]
    private static Thickness DefaultForIndicatorEndModeMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMargin, 0, 0, 0);

    [ControlTokenDefault(nameof(IndicatorMiddleModeMargin))]
    private static Thickness DefaultForIndicatorMiddleModeMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMargin, 0);

    [ControlTokenDefault(nameof(IndicatorDotBorderWidth))]
    private static double DefaultForIndicatorDotBorderWidth(in ControlTokenDefaultContext context) =>
        context.Global.LineWidth * 3;
}
