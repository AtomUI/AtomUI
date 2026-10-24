using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class LunarCalendarToken : AbstractControlDesignToken, IControlTokenDefaults
{
    public double MiniContentHeight { get; set; }
    public double MiniDateCellSize { get; set; }
    public double MiniMonthCellWidth { get; set; }
    public double SecondaryTextFontSize { get; set; }
    public double SecondaryTextLineHeight { get; set; }
    public Color SecondaryTextColor { get; set; }
    public Color WeekendTextColor { get; set; }
    public Color HolidayMarkerColor { get; set; }
    public Color WorkdayMarkerColor { get; set; }
    public double FullCellMinHeight { get; set; }
    public double RangeBarTopOffset { get; set; }

    [ControlTokenDefault(nameof(MiniContentHeight), nameof(MiniDateCellSize))]
    private static double DefaultForMiniContentHeight(in ControlTokenDefaultContext context, double miniDateCellSize) =>
        context.Global.FontHeightSM + (miniDateCellSize + context.Global.UniformlyMarginXS) * 6;

    [ControlTokenDefault(nameof(MiniDateCellSize))]
    private static double DefaultForMiniDateCellSize(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG;

    [ControlTokenDefault(nameof(MiniMonthCellWidth))]
    private static double DefaultForMiniMonthCellWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeightLG * 2;

    [ControlTokenDefault(nameof(SecondaryTextFontSize))]
    private static double DefaultForSecondaryTextFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSizeSM;

    [ControlTokenDefault(nameof(SecondaryTextLineHeight))]
    private static double DefaultForSecondaryTextLineHeight(in ControlTokenDefaultContext context) =>
        context.Global.FontHeightSM;

    [ControlTokenDefault(nameof(SecondaryTextColor))]
    private static Color DefaultForSecondaryTextColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextTertiary;

    [ControlTokenDefault(nameof(WeekendTextColor))]
    private static Color DefaultForWeekendTextColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(HolidayMarkerColor))]
    private static Color DefaultForHolidayMarkerColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorError;

    [ControlTokenDefault(nameof(WorkdayMarkerColor))]
    private static Color DefaultForWorkdayMarkerColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextTertiary;

    [ControlTokenDefault(nameof(FullCellMinHeight), nameof(SecondaryTextLineHeight))]
    private static double DefaultForFullCellMinHeight(in ControlTokenDefaultContext context, double secondaryTextLineHeight)
    {
        var calendarContentHeight =
            (context.Global.FontHeightSM + context.Global.UniformlyMarginXS) * 3 +
            context.Global.LineWidth * 2;
        return context.Global.ControlHeightSM +
               calendarContentHeight +
               context.Global.UniformlyPaddingXS / 2 +
               context.Global.LineWidthBold +
               secondaryTextLineHeight +
               context.Global.UniformlyMarginXXS;
    }

    [ControlTokenDefault(nameof(RangeBarTopOffset), nameof(SecondaryTextLineHeight))]
    private static double DefaultForRangeBarTopOffset(in ControlTokenDefaultContext context, double secondaryTextLineHeight) =>
        context.Global.ControlHeightSM + secondaryTextLineHeight + context.Global.UniformlyMarginXXS;
}
