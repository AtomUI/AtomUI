using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="ListBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignListBoxDerivation : IControlDerivation<AntDesignToken, ListBoxToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<ListBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        var colorTextDisabled  = global.ColorTextDisabled;
        var colorTextSecondary = global.ColorTextSecondary;
        var colorBgTextHover   = global.ColorBgTextHover;

        token.ItemColor         = colorTextSecondary;
        token.ItemHoverColor    = colorTextSecondary;
        token.ItemSelectedColor = global.ControlItemColorActive;
        token.ItemSelectedIndicatorColor = global.ColorPrimary;

        token.ItemBgColor         = global.ColorTransparent;
        token.ItemHoverBgColor    = colorBgTextHover;
        token.ItemSelectedBgColor = global.ControlItemBgActive;

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
