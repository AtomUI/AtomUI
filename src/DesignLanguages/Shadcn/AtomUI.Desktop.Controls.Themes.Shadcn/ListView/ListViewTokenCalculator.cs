using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractListViewToken" /> layer.
/// </summary>
public static class ListViewTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractListViewToken" /> layer. The hover and selected item colors and backgrounds come from
    /// the list item recipe of the language in <paramref name="context" />.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractListViewToken token)
    {
        var colorTextDisabled  = global.ColorTextDisabled;
        var colorTextSecondary = global.ColorTextSecondary;
        var item               = context.Language.GetListItemRecipe(global);

        token.ItemColor         = colorTextSecondary;
        token.ItemHoverColor    = item.HoverForeground;
        token.ItemSelectedColor = item.SelectedForeground;

        token.ItemBgColor         = global.ColorTransparent;
        token.ItemHoverBgColor    = item.HoverBackground;
        token.ItemSelectedBgColor = item.SelectedBackground;

        token.ItemDisabledColor = colorTextDisabled;

        // The item padding and the values after it keep the Ant formula.
        token.ItemPaddingLG = new Thickness(global.UniformlyPadding, 0);
        token.ItemPaddingSM = new Thickness(global.UniformlyPaddingXS, 0);
        token.ItemPadding   = new Thickness(global.UniformlyPaddingSM, 0);

        token.ContentPadding          = new Thickness(0);
        token.ItemMargin              = new Thickness(0);
        token.GroupHeaderColor        = global.ColorTextDescription;
    }
}
