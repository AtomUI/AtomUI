using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="DashboardProgressToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class DashboardProgressTokenRule : IControlTokenRule<AntDesignToken, DashboardProgressToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<DashboardProgressToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        CircleProgressTokenCalculator.Calculate(in context, global, token);
    }
}
