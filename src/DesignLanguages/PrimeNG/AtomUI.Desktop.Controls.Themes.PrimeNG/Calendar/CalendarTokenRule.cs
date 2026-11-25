using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CalendarToken"/>.</summary>
[ControlTokenRule]
internal sealed class CalendarTokenRule : IControlTokenRule<PrimeNGToken, CalendarToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<CalendarToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGCalendarTokens.EvaluateGlobal(in context, global);
        CalendarTokenCalculator.Calculate(in context, global, token);
        token.YearMonthCellWidth = global.ControlHeightLG * 1.5;
        token.MiniContentHeight = 256;
        var dateContentHeight = (global.FontHeightSM + global.UniformlyMarginXS) * 3 +
                                global.LineWidth * 2;
        token.FullCellMinHeight = global.ControlHeightSM + dateContentHeight +
                            global.UniformlyPaddingXS / 2 + global.LineWidthBold;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGCalendarTokens.EvaluateOwn(in context, global, token);
    }
}
