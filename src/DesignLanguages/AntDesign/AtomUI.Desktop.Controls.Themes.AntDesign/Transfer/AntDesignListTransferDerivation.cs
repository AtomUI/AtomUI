using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ListTransferToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignListTransferDerivation : IControlDerivation<AntDesignToken, ListTransferToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<ListTransferToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractTransferDerivation.DeriveShared(in context, global, token);
        token.ListWidthLG = 250;
    }
}
