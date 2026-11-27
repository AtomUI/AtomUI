using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="StatisticToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class StatisticTokenRule : IControlTokenRule<PrimeNGToken, StatisticToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<StatisticToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        StatisticTokenCalculator.Calculate(in context, global, token);
    }
}
