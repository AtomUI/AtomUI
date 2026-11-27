using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractTabToken" /> layer.
/// </summary>
public static class TabTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractTabToken" /> layer. The line and card tab calculators start with it.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractTabToken token)
    {
        // Ant Design formula kept as is: no registry value is merged into the shared tab layer.
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
