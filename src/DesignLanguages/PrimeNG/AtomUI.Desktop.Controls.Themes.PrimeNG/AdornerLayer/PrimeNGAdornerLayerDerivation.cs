using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="AdornerLayerToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGAdornerLayerDerivation : IControlDerivation<PrimeNGToken, AdornerLayerToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<AdornerLayerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGAdornerLayerTokens.EvaluateGlobal(in context, global);
        token.FocusOutlineOffset = global.LineWidth;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGAdornerLayerTokens.EvaluateOwn(in context, global, token);
    }
}
