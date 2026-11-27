using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="DashboardProgressToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class DashboardProgressTokenRule : IControlTokenRule<ShadcnToken, DashboardProgressToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<DashboardProgressToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        CircleProgressTokenCalculator.Calculate(in context, global, token);
    }
}
