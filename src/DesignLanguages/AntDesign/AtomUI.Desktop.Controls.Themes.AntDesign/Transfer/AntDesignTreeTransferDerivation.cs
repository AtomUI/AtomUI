using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TreeTransferToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTreeTransferDerivation : IControlDerivation<AntDesignToken, TreeTransferToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TreeTransferToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractTransferDerivation.DeriveShared(in context, global, token);
        token.ListWidthLG = 250;
    }
}
