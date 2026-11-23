using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TransferTreeViewToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTransferTreeViewDerivation : IControlDerivation<AntDesignToken, TransferTreeViewToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TransferTreeViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractTreeViewDerivation.DeriveShared(in context, global, token);
    }
}
