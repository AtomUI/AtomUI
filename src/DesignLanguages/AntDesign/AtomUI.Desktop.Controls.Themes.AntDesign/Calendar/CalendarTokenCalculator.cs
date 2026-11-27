using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign values of the shared <see cref="AbstractCalendarToken" /> layer.
/// </summary>
public static class CalendarTokenCalculator
{
    /// <summary>
    /// Assigns the Ant Design values of the shared <see cref="AbstractCalendarToken" /> layer to
    /// <paramref name="token" />. The Token rules of Calendar and LunarCalendar call it.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<AntDesignToken> context, DesignToken global, AbstractCalendarToken token)
    {


        token.FullBg = global.ColorBgContainer;
        token.FullPanelBg = global.ColorBgContainer;
        token.ItemActiveBg = global.ControlItemBgActive;
        token.ItemActiveColor = global.ColorPrimary;
        token.YearControlWidth = 80;
        token.MonthControlWidth = 70;
        token.WeekHeaderHeight = global.ControlHeightSM * 0.75 +
                           global.UniformlyPaddingXXS;
        token.WeekHeaderPadding = new Thickness(0, 0, global.UniformlyPaddingSM,
            global.UniformlyPaddingXXS);
        token.FullCellMargin = new Thickness(global.UniformlyMarginXS / 2, 0);
        token.FullCellPadding = new Thickness(global.UniformlyPaddingXS, global.UniformlyPaddingXS / 2,
            global.UniformlyPaddingXS, 0);
        token.FullCellBorderThickness = new Thickness(0, global.LineWidthBold, 0, 0);
        token.RangeBarHeight = global.ControlHeightSM - global.UniformlyMarginXXS;
        token.RangeBarGap = global.UniformlyMarginXXS / 2;
        token.RangeBarLabelPadding = new Thickness(global.UniformlyPaddingXS, 0);
    }
}
