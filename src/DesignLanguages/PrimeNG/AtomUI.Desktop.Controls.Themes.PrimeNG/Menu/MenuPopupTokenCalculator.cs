using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractMenuPopupToken" /> layer.
/// </summary>
public static class MenuPopupTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values into the shared menu popup layer: the popup background, shadow, padding and width
    /// limits, and the geometry and colors of the items, group titles and separators.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractMenuPopupToken token)
    {
        // Ant Design formula kept as is; the PrimeNG menu stages (PrimeNGMenuFrame.ApplyItems, then the owner's
        // EvaluateOwn) replace the item padding, height, icon gap and radius, the group title padding, the
        // popup padding, minimum width and shadow; everything else stays (the maximum width 800, the colors,
        // the item margin, the separator height).
        token.KeyGestureColor = global.ColorTextQuaternary;
        token.ItemBorderRadius = global.BorderRadius;
        token.ItemBg = global.ColorBgElevated;
        token.ItemHoverBg = global.ColorBgTextHover;
        token.ItemMargin = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        token.ItemDisabledColor = global.ColorTextDisabled;
        token.DangerItemColor = global.ColorError;
        token.DangerItemHoverColor = global.ColorError;
        token.ItemHeight = global.ControlHeight;
        token.MenuPopupBgColor = global.ColorBgElevated;
        token.ItemPaddingInline = new Thickness(global.UniformlyPadding, global.UniformlyPaddingXXS);
        token.ItemIconSize = global.IconSize;
        token.ItemIconMarginInlineEnd = global.UniformlyMarginXXS;
        token.GroupTitlePadding = new Thickness(global.UniformlyPadding, global.UniformlyPaddingXS);
        token.MenuPopupMinWidth = 120;
        token.MenuPopupMaxWidth = 800;
        token.SeparatorItemHeight = global.LineWidth * 5;
        token.MenuPopupShadows = global.BoxShadowsSecondary;
        token.MenuPopupContentPadding = new Thickness(global.UniformlyPaddingXXS, global.BorderRadiusLG.TopLeft / 2);
    }
}
