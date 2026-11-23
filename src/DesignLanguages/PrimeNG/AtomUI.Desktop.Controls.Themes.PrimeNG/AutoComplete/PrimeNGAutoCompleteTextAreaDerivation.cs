using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="AutoCompleteTextAreaToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGAutoCompleteTextAreaDerivation : IControlDerivation<PrimeNGToken, AutoCompleteTextAreaToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<AutoCompleteTextAreaToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGAutoCompleteTextAreaTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractAutoCompleteDerivation.DeriveShared(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGAutoCompleteTextAreaTokens.EvaluateOwn(in context, global, token);
    }
}
