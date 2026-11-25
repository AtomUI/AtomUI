using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CircleProgressToken"/>.</summary>
[ControlTokenRule]
internal sealed class CircleProgressTokenRule : IControlTokenRule<PrimeNGToken, CircleProgressToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<CircleProgressToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGProgressTokens<CircleProgressToken>.EvaluateGlobal(in context, global);
        CircleProgressTokenCalculator.Calculate(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGProgressTokens<CircleProgressToken>.EvaluateOwn(in context, global, token);
    }
}
