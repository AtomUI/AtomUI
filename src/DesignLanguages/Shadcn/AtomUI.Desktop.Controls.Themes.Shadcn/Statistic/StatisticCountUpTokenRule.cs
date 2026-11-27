using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="StatisticCountUpToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class StatisticCountUpTokenRule : IControlTokenRule<ShadcnToken, StatisticCountUpToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<StatisticCountUpToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        StatisticValueTokenCalculator.Calculate(in context, global, token);
    }
}
