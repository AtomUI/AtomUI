using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TreeTransferToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTreeTransferDerivation : IControlDerivation<PrimeNGToken, TreeTransferToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TreeTransferToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractTransferDerivation.DeriveShared(in context, global, token);
        token.ListWidthLG = 250;
    }
}
