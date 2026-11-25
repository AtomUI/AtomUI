using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="DashboardProgressToken"/>.</summary>
[ControlTokenRule]
internal sealed class DashboardProgressTokenRule : IControlTokenRule<PrimeNGToken, DashboardProgressToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<DashboardProgressToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGProgressTokens<DashboardProgressToken>.EvaluateGlobal(in context, global);
        CircleProgressTokenCalculator.Calculate(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGProgressTokens<DashboardProgressToken>.EvaluateOwn(in context, global, token);
    }
}
