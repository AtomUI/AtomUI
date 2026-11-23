using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ListTransferToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGListTransferDerivation : IControlDerivation<PrimeNGToken, ListTransferToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<ListTransferToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractTransferDerivation.DeriveShared(in context, global, token);
        token.ListWidthLG = 250;
    }
}
