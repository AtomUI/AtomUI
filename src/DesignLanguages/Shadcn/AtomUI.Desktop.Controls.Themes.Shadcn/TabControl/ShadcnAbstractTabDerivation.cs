using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractTabToken"/> layer.</summary>
public static class ShadcnAbstractTabDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractTabToken token)
    {
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
