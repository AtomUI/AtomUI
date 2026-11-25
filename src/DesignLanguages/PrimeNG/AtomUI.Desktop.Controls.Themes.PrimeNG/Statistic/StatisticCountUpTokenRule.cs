using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="StatisticCountUpToken"/>.</summary>
[ControlTokenRule]
internal sealed class StatisticCountUpTokenRule : IControlTokenRule<PrimeNGToken, StatisticCountUpToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<StatisticCountUpToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        StatisticValueTokenCalculator.Calculate(in context, global, token);
    }
}
