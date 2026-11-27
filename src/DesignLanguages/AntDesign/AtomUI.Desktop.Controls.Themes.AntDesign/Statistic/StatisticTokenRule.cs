using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="StatisticToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class StatisticTokenRule : IControlTokenRule<AntDesignToken, StatisticToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<StatisticToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        StatisticTokenCalculator.Calculate(in context, global, token);
    }
}
