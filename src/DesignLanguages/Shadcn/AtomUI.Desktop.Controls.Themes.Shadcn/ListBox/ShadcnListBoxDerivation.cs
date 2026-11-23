using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="ListBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnListBoxDerivation : IControlDerivation<ShadcnToken, ListBoxToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<ListBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        var colorTextDisabled  = global.ColorTextDisabled;
        var colorTextSecondary = global.ColorTextSecondary;
        var item               = context.Language.GetListItemRecipe(global);

        token.ItemColor         = colorTextSecondary;
        token.ItemHoverColor    = item.HoverForeground;
        token.ItemSelectedColor = item.SelectedForeground;
        token.ItemSelectedIndicatorColor = global.ColorPrimary;

        token.ItemBgColor         = global.ColorTransparent;
        token.ItemHoverBgColor    = item.HoverBackground;
        token.ItemSelectedBgColor = item.SelectedBackground;

        token.ItemDisabledColor = colorTextDisabled;

        token.ItemPaddingLG = new Thickness(global.UniformlyPadding, 0);
        token.ItemPaddingSM = new Thickness(global.UniformlyPaddingXS, 0);
        token.ItemPadding   = new Thickness(global.UniformlyPaddingSM, 0);

        token.ContentPadding   = new Thickness(0);
        token.ItemMargin       = new Thickness(0);

        token.FilterHighlightColor = global.ColorError;

        token.SelectedIndicatorMargin = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
    }
}
