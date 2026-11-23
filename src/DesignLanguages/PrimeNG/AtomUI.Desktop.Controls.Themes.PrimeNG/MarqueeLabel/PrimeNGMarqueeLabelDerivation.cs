using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="MarqueeLabelToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGMarqueeLabelDerivation : IControlDerivation<PrimeNGToken, MarqueeLabelToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<MarqueeLabelToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.CycleSpace   = 200;
        token.DefaultSpeed = 150;
    }
}
