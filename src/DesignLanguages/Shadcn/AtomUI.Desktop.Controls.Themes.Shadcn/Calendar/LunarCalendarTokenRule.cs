using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="LunarCalendarToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class LunarCalendarTokenRule : IControlTokenRule<ShadcnToken, LunarCalendarToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<LunarCalendarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        CalendarTokenCalculator.Calculate(in context, global, token);

        if ((context.Variant.Appearance == ThemeAppearance.Dark))
        {
            // In dark appearance the fullscreen month view fills a selected date with ItemActiveBg, so the two
            // text lines take the body text color instead of the primary color.
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
        if (context.Variant.IsDark)
        {
            // Dark shadcn resolves the two source surfaces to the same shade.
            token.ItemActiveBg = ShadcnRecipes.CompositeOver(global.ColorText, 0.14, token.ItemActiveBg);
        }
    }
}
