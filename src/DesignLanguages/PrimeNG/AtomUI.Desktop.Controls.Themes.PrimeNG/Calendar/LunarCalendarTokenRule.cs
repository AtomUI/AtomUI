using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="LunarCalendarToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class LunarCalendarTokenRule : IControlTokenRule<PrimeNGToken, LunarCalendarToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<LunarCalendarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGLunarCalendarTokens.EvaluateGlobal(in context, global);
        CalendarTokenCalculator.Calculate(in context, global, token);

        // Ant Design formula kept as is; in dark appearance the fullscreen month view fills a selected date
        // with ItemActiveBg and the two text lines take the body text color there. The PrimeNG lunar
        // calendar stage (PrimeNGLunarCalendarTokens.EvaluateOwn) replaces ItemActiveBg and ItemActiveColor
        // in every preset.
        if ((context.Variant.Appearance == ThemeAppearance.Dark))
        {
            token.ItemActiveColor = global.ColorText;
        }
        token.MiniDateCellSize = global.ControlHeightLG;

        token.MiniMonthCellWidth = global.ControlHeightLG * 2;
        token.SecondaryTextFontSize = global.FontSizeSM;
        token.SecondaryTextLineHeight = global.FontHeightSM;
        token.MiniDateCellPadding = new Thickness(0, 3);
        token.MiniDateCellCornerRadius = new CornerRadius(6);
        token.MiniDateCellHeight = Math.Max(token.MiniDateCellSize, Math.Ceiling(
                global.FontHeight + token.SecondaryTextLineHeight +
                token.MiniDateCellPadding.Top + token.MiniDateCellPadding.Bottom));

        token.MiniContentHeight = global.FontHeightSM +
                            (token.MiniDateCellHeight + global.UniformlyMarginXS) * 6;

        token.SecondaryTextColor = global.ColorTextTertiary;
        token.WeekendTextColor = global.ColorError;
        token.HolidayMarkerColor = global.ColorError;
        token.WorkdayMarkerColor = global.ColorTextTertiary;
        token.MarkerSize = global.UniformlyMarginXXS;

        token.MarkerMargin = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        token.MarkerCornerRadius = new CornerRadius(token.MarkerSize / 2);

        var calendarContentHeight =
            (global.FontHeightSM + global.UniformlyMarginXS) * 3 +
            global.LineWidth * 2;
        token.FullCellMinHeight = global.ControlHeightSM +
                            calendarContentHeight +
                            global.UniformlyPaddingXS / 2 +
                            global.LineWidthBold +
                            token.SecondaryTextLineHeight +
                            global.UniformlyMarginXXS;
        token.RangeBarTopOffset = global.ControlHeightSM +
                            token.SecondaryTextLineHeight +
                            global.UniformlyMarginXXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGLunarCalendarTokens.EvaluateOwn(in context, global, token);
    }
}
