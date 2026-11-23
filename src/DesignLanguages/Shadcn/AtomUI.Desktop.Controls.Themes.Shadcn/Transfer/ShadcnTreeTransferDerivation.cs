using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TreeTransferToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTreeTransferDerivation : IControlDerivation<ShadcnToken, TreeTransferToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TreeTransferToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractTransferDerivation.DeriveShared(in context, global, token);
        token.ListWidthLG = 250;
    }
}
