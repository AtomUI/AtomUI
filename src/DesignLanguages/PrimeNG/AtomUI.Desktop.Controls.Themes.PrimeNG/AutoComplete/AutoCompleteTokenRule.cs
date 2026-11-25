using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="AutoCompleteToken"/>.</summary>
[ControlTokenRule]
internal sealed class AutoCompleteTokenRule : IControlTokenRule<PrimeNGToken, AutoCompleteToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<AutoCompleteToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGAutoCompleteTokens.EvaluateGlobal(in context, global);
        AutoCompleteTokenCalculator.Calculate(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGAutoCompleteTokens.EvaluateOwn(in context, global, token);
    }
}
