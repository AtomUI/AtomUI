using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TimerStatisticToken"/>.</summary>
[ControlTokenRule]
internal sealed class TimerStatisticTokenRule : IControlTokenRule<PrimeNGToken, TimerStatisticToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<TimerStatisticToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        StatisticTokenCalculator.Calculate(in context, global, token);
    }
}
