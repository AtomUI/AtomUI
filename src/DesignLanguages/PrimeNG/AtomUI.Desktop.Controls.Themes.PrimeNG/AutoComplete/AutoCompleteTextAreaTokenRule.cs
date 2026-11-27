using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="AutoCompleteTextAreaToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class AutoCompleteTextAreaTokenRule : IControlTokenRule<PrimeNGToken, AutoCompleteTextAreaToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<AutoCompleteTextAreaToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides of PrimeNGInputStage.Apply (form-field values); the formulas below
        // read none, the theme and the text box it hosts (CornerRadius, Foreground, Background, IconBrush).
        PrimeNGAutoCompleteTextAreaTokens.EvaluateGlobal(in context, global);
        AutoCompleteTokenCalculator.Calculate(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGAutoCompleteTextAreaTokens.EvaluateOwn(in context, global, token);
    }
}
