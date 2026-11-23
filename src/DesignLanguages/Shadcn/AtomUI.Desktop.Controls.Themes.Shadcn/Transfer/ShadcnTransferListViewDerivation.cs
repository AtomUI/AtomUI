using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TransferListViewToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTransferListViewDerivation : IControlDerivation<ShadcnToken, TransferListViewToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TransferListViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractListViewDerivation.DeriveShared(in context, global, token);

        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXXS);
    }
}
