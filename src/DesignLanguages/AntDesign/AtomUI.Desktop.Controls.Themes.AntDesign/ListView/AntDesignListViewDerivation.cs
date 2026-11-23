using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ListViewToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignListViewDerivation : IControlDerivation<AntDesignToken, ListViewToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<ListViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractListViewDerivation.DeriveShared(in context, global, token);
        token.GroupHeaderPadding = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingXS);
        token.SelectedIndicatorMargin = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
        token.ItemSelectedIndicatorColor = global.ColorPrimary;
        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXS);
    }
}
