using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="SearchEditToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGSearchEditDerivation : IControlDerivation<PrimeNGToken, SearchEditToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<SearchEditToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGSearchEditTokens.EvaluateGlobal(in context, global);
        token.InputFontSizeSM = global.FontSize;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSearchEditTokens.EvaluateOwn(in context, global, token);
    }
}
