using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="TransferListViewToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignTransferListViewDerivation : IControlDerivation<AntDesignToken, TransferListViewToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<TransferListViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractListViewDerivation.DeriveShared(in context, global, token);

        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXXS);
    }
}
