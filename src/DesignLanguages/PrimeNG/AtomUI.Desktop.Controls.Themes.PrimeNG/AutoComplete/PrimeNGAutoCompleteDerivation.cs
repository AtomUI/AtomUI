using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="AutoCompleteToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGAutoCompleteDerivation : IControlDerivation<PrimeNGToken, AutoCompleteToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<AutoCompleteToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGAutoCompleteTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractAutoCompleteDerivation.DeriveShared(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGAutoCompleteTokens.EvaluateOwn(in context, global, token);
    }
}
