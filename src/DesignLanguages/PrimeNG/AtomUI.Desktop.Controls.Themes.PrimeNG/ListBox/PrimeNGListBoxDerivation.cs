using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ListBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGListBoxDerivation : IControlDerivation<PrimeNGToken, ListBoxToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<ListBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        var item   = context.Language.GetListItemRecipe(global);
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGListBoxTokens.EvaluateGlobal(in context, global);
        var colorTextDisabled  = global.ColorTextDisabled;
        var colorTextSecondary = global.ColorTextSecondary;
        var colorBgTextHover   = global.ColorBgTextHover;

        token.ItemColor         = colorTextSecondary;
        token.ItemHoverColor    = colorTextSecondary;
        token.ItemSelectedColor = item.SelectedForeground;
        token.ItemSelectedIndicatorColor = global.ColorPrimary;

        token.ItemBgColor         = global.ColorTransparent;
        token.ItemHoverBgColor    = colorBgTextHover;
        token.ItemSelectedBgColor = item.SelectedBackground;

        token.ItemDisabledColor = colorTextDisabled;

        token.ItemPaddingLG = new Thickness(global.UniformlyPadding, 0);
        token.ItemPaddingSM = new Thickness(global.UniformlyPaddingXS, 0);
        token.ItemPadding   = new Thickness(global.UniformlyPaddingSM, 0);

        token.ContentPadding   = new Thickness(0);
        token.ItemMargin       = new Thickness(0);

        token.FilterHighlightColor = global.ColorError;

        token.SelectedIndicatorMargin = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGListBoxTokens.EvaluateOwn(in context, global, token);
    }
}
