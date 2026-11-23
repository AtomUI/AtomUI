using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="CheckableTagToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGCheckableTagDerivation : IControlDerivation<PrimeNGToken, CheckableTagToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<CheckableTagToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractTagDerivation.DeriveShared(in context, global, token);
    }
}
