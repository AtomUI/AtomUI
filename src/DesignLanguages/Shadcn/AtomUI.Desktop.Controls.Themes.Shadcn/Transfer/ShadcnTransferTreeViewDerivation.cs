using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TransferTreeViewToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTransferTreeViewDerivation : IControlDerivation<ShadcnToken, TransferTreeViewToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TransferTreeViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractTreeViewDerivation.DeriveShared(in context, global, token);
    }
}
