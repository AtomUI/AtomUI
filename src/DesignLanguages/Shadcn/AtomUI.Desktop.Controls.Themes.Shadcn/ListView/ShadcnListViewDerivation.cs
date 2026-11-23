using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ListViewToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnListViewDerivation : IControlDerivation<ShadcnToken, ListViewToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ListViewToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractListViewDerivation.DeriveShared(in context, global, token);
        token.GroupHeaderPadding = new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingXS);
        token.SelectedIndicatorMargin = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
        token.ItemSelectedIndicatorColor = global.ColorPrimary;
        token.PaginationMargin = new Thickness(0, global.UniformlyMarginXS);
    }
}
