using AtomUI.Theme.DesignTokens;
using AtomUI.Media;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTabToken : AbstractControlDesignToken
{
    public partial double TitleFontSize { get; set; }
    public partial double TitleFontSizeLG { get; set; }
    public partial double TitleFontSizeSM { get; set; }

    /// <summary>The font weight of the tab titles.</summary>
    public partial FontWeight TitleFontWeight { get; set; }
    public partial Thickness HorizontalItemMargin { get; set; }
    public partial Color ItemColor { get; set; }
    public partial Color ItemHoverColor { get; set; }
    public partial Color ItemSelectedColor { get; set; }
    public partial Thickness ItemIconMargin { get; set; }
    public partial Thickness MenuIndicatorPaddingHorizontal { get; set; }
    public partial Thickness MenuIndicatorPaddingVertical { get; set; }
    public partial BoxShadows BoxShadowTabsOverflowLeft { get; set; }
    public partial BoxShadows BoxShadowTabsOverflowRight { get; set; }
    public partial BoxShadows BoxShadowTabsOverflowTop { get; set; }
    public partial BoxShadows BoxShadowTabsOverflowBottom { get; set; }
    public partial Thickness CloseIconMargin { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        TitleFontSize = EffectiveGlobalToken.FontSize;
        TitleFontSizeLG = EffectiveGlobalToken.FontSizeLG;
        TitleFontSizeSM = EffectiveGlobalToken.FontSize;
        TitleFontWeight = FontWeight.Normal;
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
