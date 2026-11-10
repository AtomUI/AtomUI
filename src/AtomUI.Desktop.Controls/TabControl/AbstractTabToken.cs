using AtomUI.Theme.DesignTokens;
using AtomUI.Media;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractTabToken : AbstractControlDesignToken
{
    public double TitleFontSize { get; set; }
    public double TitleFontSizeLG { get; set; }
    public double TitleFontSizeSM { get; set; }
    public Thickness HorizontalItemMargin { get; set; }
    public Color ItemColor { get; set; }
    public Color ItemHoverColor { get; set; }
    public Color ItemSelectedColor { get; set; }
    public Thickness ItemIconMargin { get; set; }
    public Thickness MenuIndicatorPaddingHorizontal { get; set; }
    public Thickness MenuIndicatorPaddingVertical { get; set; }
    public BoxShadows BoxShadowTabsOverflowLeft { get; set; }
    public BoxShadows BoxShadowTabsOverflowRight { get; set; }
    public BoxShadows BoxShadowTabsOverflowTop { get; set; }
    public BoxShadows BoxShadowTabsOverflowBottom { get; set; }
    public Thickness CloseIconMargin { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TitleFontSize = EffectiveGlobalToken.FontSize;
        TitleFontSizeLG = EffectiveGlobalToken.FontSizeLG;
        TitleFontSizeSM = EffectiveGlobalToken.FontSize;
        HorizontalItemMargin = new Thickness();
        ItemColor = EffectiveGlobalToken.ColorText;
        ItemHoverColor = EffectiveGlobalToken.ColorPrimaryHover;
        ItemSelectedColor = EffectiveGlobalToken.ColorPrimary;
        ItemIconMargin = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginSM, 0);
        MenuIndicatorPaddingHorizontal = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, 0, 0, 0);
        MenuIndicatorPaddingVertical = new Thickness(0, EffectiveGlobalToken.UniformlyPaddingXS, 0, 0);
        BoxShadowTabsOverflowLeft = new BoxShadows(new BoxShadow
        {
            OffsetX = 10,
            Blur    = 8,
            Spread  = -8,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        });
        BoxShadowTabsOverflowRight = new BoxShadows(new BoxShadow
        {
            OffsetX = -10,
            Blur    = 8,
            Spread  = -8,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        });
        BoxShadowTabsOverflowTop = new BoxShadows(new BoxShadow
        {
            OffsetY = 10,
            Blur    = 8,
            Spread  = -8,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        });
        BoxShadowTabsOverflowBottom = new BoxShadows(new BoxShadow
        {
            OffsetY = -10,
            Blur    = 8,
            Spread  = -8,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        });
        CloseIconMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginXXS, 0, 0, 0);
    }
}
