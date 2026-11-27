using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractListViewToken" /> layer.
/// </summary>
public static class ListViewTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values into the shared list view layer: the item colors and backgrounds in the normal,
    /// hover, selected and disabled states, the item paddings of each size and the group header color.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractListViewToken token)
    {
        var colorTextDisabled  = global.ColorTextDisabled;
        var colorTextSecondary = global.ColorTextSecondary;
        var colorBgTextHover   = global.ColorBgTextHover;

        token.ItemColor         = colorTextSecondary;
        token.ItemHoverColor    = colorTextSecondary;
        token.ItemSelectedColor = global.ControlItemColorActive;

        token.ItemBgColor         = global.ColorTransparent;
        token.ItemHoverBgColor    = colorBgTextHover;
        token.ItemSelectedBgColor = global.ControlItemBgActive;

        token.ItemDisabledColor = colorTextDisabled;

        // Ant Design formula kept as is; the PrimeNG list view stage (PrimeNGListViewTokens.EvaluateOwn)
        // replaces ItemPadding and ContentPadding here, the item and hover text colors and the hover
        // background above, and the group header color, with the PrimeUIX list values. The SM and LG item
        // paddings stay.
        token.ItemPaddingLG = new Thickness(global.UniformlyPadding, 0);
        token.ItemPaddingSM = new Thickness(global.UniformlyPaddingXS, 0);
        token.ItemPadding   = new Thickness(global.UniformlyPaddingSM, 0);

        token.ContentPadding          = new Thickness(0);
        token.ItemMargin              = new Thickness(0);
        token.GroupHeaderColor        = global.ColorTextDescription;
    }
}
