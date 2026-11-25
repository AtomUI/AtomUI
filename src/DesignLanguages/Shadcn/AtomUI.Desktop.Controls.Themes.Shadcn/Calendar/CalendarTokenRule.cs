using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="CalendarToken"/>.</summary>
[ControlTokenRule]
internal sealed class CalendarTokenRule : IControlTokenRule<ShadcnToken, CalendarToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<CalendarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        CalendarTokenCalculator.Calculate(in context, global, token);
        token.YearMonthCellWidth = global.ControlHeightLG * 1.5;
        token.MiniContentHeight = 256;
        var dateContentHeight = (global.FontHeightSM + global.UniformlyMarginXS) * 3 +
                                global.LineWidth * 2;
        token.FullCellMinHeight = global.ControlHeightSM + dateContentHeight +
                            global.UniformlyPaddingXS / 2 + global.LineWidthBold;
        if (context.Variant.IsDark)
        {
            token.ItemActiveBg = ShadcnRecipes.CompositeOver(global.ColorText, 0.14, token.ItemActiveBg);
        }
    }
}
