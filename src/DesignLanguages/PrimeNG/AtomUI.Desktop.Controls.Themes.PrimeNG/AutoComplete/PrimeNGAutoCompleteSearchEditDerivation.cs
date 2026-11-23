using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="AutoCompleteSearchEditToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGAutoCompleteSearchEditDerivation : IControlDerivation<PrimeNGToken, AutoCompleteSearchEditToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<AutoCompleteSearchEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGAutoCompleteSearchEditTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractAutoCompleteDerivation.DeriveShared(in context, global, token);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGAutoCompleteSearchEditTokens.EvaluateOwn(in context, global, token);
    }
}
