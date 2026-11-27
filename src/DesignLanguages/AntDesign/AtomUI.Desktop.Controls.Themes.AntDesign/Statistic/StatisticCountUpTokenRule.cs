using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="StatisticCountUpToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class StatisticCountUpTokenRule : IControlTokenRule<AntDesignToken, StatisticCountUpToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<StatisticCountUpToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        StatisticValueTokenCalculator.Calculate(in context, global, token);
    }
}
