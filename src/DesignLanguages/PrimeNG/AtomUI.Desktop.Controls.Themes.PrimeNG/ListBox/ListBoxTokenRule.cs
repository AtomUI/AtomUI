using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="ListBoxToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class ListBoxTokenRule : IControlTokenRule<PrimeNGToken, ListBoxToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<ListBoxToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        var item   = context.Language.GetListItemRecipe(global);
        // PrimeNG: no control-global overrides; EvaluateGlobal writes nothing.
        PrimeNGListBoxTokens.EvaluateGlobal(in context, global);
        var colorTextDisabled  = global.ColorTextDisabled;
        var colorTextSecondary = global.ColorTextSecondary;
        var colorBgTextHover   = global.ColorBgTextHover;

        token.ItemColor         = colorTextSecondary;
        token.ItemHoverColor    = colorTextSecondary;
        // PrimeUIX list.option: highlight.background / highlight.color for the selected item, focus.background for the
        // hovered one.
        token.ItemSelectedColor = item.SelectedForeground;
        token.ItemSelectedIndicatorColor = global.ColorPrimary;

        token.ItemBgColor         = global.ColorTransparent;
        token.ItemHoverBgColor    = colorBgTextHover;
        token.ItemSelectedBgColor = item.SelectedBackground;

        token.ItemDisabledColor = colorTextDisabled;

        // Ant Design formula kept as is; the PrimeNG list box stage (PrimeNGListBoxTokens.EvaluateOwn) replaces
        // ItemPadding and ContentPadding here and the item and hover colors above with the PrimeUIX list
        // values. The SM and LG item paddings and ItemMargin stay.
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
