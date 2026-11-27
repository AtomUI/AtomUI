using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG values of the shared <see cref="AbstractTabToken" /> layer.
/// </summary>
public static class TabTokenCalculator
{
    /// <summary>
    /// Writes the PrimeNG values into the shared tab layer: the title typography, the item colors, margins and icon
    /// spacing, the menu indicator paddings, the shadows at the four overflow edges and the margin of the close icon.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractTabToken token)
    {
        // Ant Design formula kept as is: the tab stage (PrimeNGTabStage) replaces the title weight, the item
        // colors, the item paddings and the ink bar (PrimeUIX tabs); the rest keeps the Ant Design formula.
        token.TitleFontSize = global.FontSize;
        token.TitleFontSizeLG = global.FontSizeLG;
        token.TitleFontSizeSM = global.FontSize;
        token.TitleFontWeight = FontWeight.Normal;
        token.HorizontalItemMargin = new Thickness();
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = global.ColorPrimaryHover;
        token.ItemSelectedColor = global.ColorPrimary;
        token.ItemIconMargin = new Thickness(0, 0, global.UniformlyMarginSM, 0);
        token.MenuIndicatorPaddingHorizontal = new Thickness(global.UniformlyPaddingXS, 0, 0, 0);
        token.MenuIndicatorPaddingVertical = new Thickness(0, global.UniformlyPaddingXS, 0, 0);
        // The overflow shadows of Ant Design Tabs: offset 10, blur 8 and spread -8 along the placement axis, in 8
        // percent black.
        token.BoxShadowTabsOverflowLeft = new BoxShadows(new BoxShadow
        {
            OffsetX = 10,
            Blur    = 8,
            Spread  = -8,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        });
        token.BoxShadowTabsOverflowRight = new BoxShadows(new BoxShadow
        {
            OffsetX = -10,
            Blur    = 8,
            Spread  = -8,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        });
        token.BoxShadowTabsOverflowTop = new BoxShadows(new BoxShadow
        {
            OffsetY = 10,
            Blur    = 8,
            Spread  = -8,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        });
        token.BoxShadowTabsOverflowBottom = new BoxShadows(new BoxShadow
        {
            OffsetY = -10,
            Blur    = 8,
            Spread  = -8,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        });
        token.CloseIconMargin = new Thickness(global.UniformlyMarginXXS, 0, 0, 0);
    }
}
