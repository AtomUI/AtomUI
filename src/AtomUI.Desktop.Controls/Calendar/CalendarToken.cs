using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Calendar 控件的 Design Token，承载产品视觉语义。
/// </summary>
[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class CalendarToken : AbstractControlDesignToken, IControlTokenDefaults
{

    public CalendarToken()

    {
    }

    /// <summary>
    /// 完整 Calendar 背景，派生自容器背景。
    /// </summary>
    public Color FullBg { get; set; }

    /// <summary>
    /// 完整 Calendar Panel 背景，派生自容器背景。
    /// </summary>
    public Color FullPanelBg { get; set; }

    /// <summary>
    /// 完整模式选中日期/月单元背景，派生自 active item 背景。
    /// </summary>
    public Color ItemActiveBg { get; set; }

    /// <summary>
    /// Year Select 最小宽度。
    /// </summary>
    public double YearControlWidth { get; set; }

    /// <summary>
    /// Month Select 最小宽度。
    /// </summary>
    public double MonthControlWidth { get; set; }

    /// <summary>
    /// Year 模式月份单元的内容宽度。
    /// </summary>
    public double YearMonthCellWidth { get; set; }

    /// <summary>
    /// Mini 内容高度。
    /// </summary>
    public double MiniContentHeight { get; set; }

    /// <summary>
    /// Fullscreen 周标题的总高度和文字内边距。
    /// </summary>
    public double WeekHeaderHeight { get; set; }
    public Thickness WeekHeaderPadding { get; set; }

    /// <summary>
    /// Fullscreen 日期/月单元最小高度。
    /// </summary>
    public double FullCellMinHeight { get; set; }

    /// <summary>
    /// Fullscreen 日期范围条默认高度。
    /// </summary>
    public double RangeBarHeight { get; set; }

    [ControlTokenDefault(nameof(FullBg))]
    private static Color DefaultForFullBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(FullPanelBg))]
    private static Color DefaultForFullPanelBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(ItemActiveBg))]
    private static Color DefaultForItemActiveBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActive;

    [ControlTokenDefault(nameof(YearControlWidth))]
    private static double DefaultForYearControlWidth(in ControlTokenDefaultContext context) =>
        80;

    [ControlTokenDefault(nameof(MonthControlWidth))]
    private static double DefaultForMonthControlWidth(in ControlTokenDefaultContext context) =>
        70;

    [ControlTokenDefault(nameof(YearMonthCellWidth))]
    private static double DefaultForYearMonthCellWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 1.5;

    [ControlTokenDefault(nameof(MiniContentHeight))]
    private static double DefaultForMiniContentHeight(in ControlTokenDefaultContext context) =>
        256;

    [ControlTokenDefault(nameof(WeekHeaderHeight))]
    private static double DefaultForWeekHeaderHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM * 0.75 + context.Global.UniformlyPaddingXXS;

    [ControlTokenDefault(nameof(WeekHeaderPadding))]
    private static Thickness DefaultForWeekHeaderPadding(in ControlTokenDefaultContext context) =>
        new Thickness(0, 0, context.Global.UniformlyPaddingSM, context.Global.UniformlyPaddingXXS);

    [ControlTokenDefault(nameof(FullCellMinHeight))]
    private static double DefaultForFullCellMinHeight(in ControlTokenDefaultContext context)
    {
        var dateContentHeight = (context.Global.FontHeightSM + context.Global.UniformlyMarginXS) * 3 +
                                context.Global.LineWidth * 2;
        return context.Global.ControlHeightSM +
               dateContentHeight +
               context.Global.UniformlyPaddingXS / 2 +
               context.Global.LineWidthBold;
    }

    [ControlTokenDefault(nameof(RangeBarHeight))]
    private static double DefaultForRangeBarHeight(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightSM - context.Global.UniformlyMarginXXS;
}
