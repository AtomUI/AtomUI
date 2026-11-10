using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractMenuPopupToken : AbstractControlDesignToken
{
    public Thickness MenuPopupContentPadding { get; set; }
    public double MenuPopupMinWidth { get; set; }
    public double MenuPopupMaxWidth { get; set; }
    public Color MenuPopupBgColor { get; set; }
    public Color ItemColor { get; set; }
    public Color KeyGestureColor { get; set; }
    public Thickness ItemMargin { get; set; }
    public Color ItemHoverColor { get; set; }
    public Color ItemDisabledColor { get; set; }
    public Color DangerItemColor { get; set; }
    public Color DangerItemHoverColor { get; set; }
    public Color ItemBg { get; set; }
    public Color ItemHoverBg { get; set; }
    public double ItemHeight { get; set; }
    public double ItemIconSize { get; set; }
    public double ItemIconMarginInlineEnd { get; set; }
    public Thickness GroupTitlePadding { get; set; }
    public CornerRadius ItemBorderRadius { get; set; }
    public Thickness ItemPaddingInline { get; set; }
    public double SeparatorItemHeight { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        KeyGestureColor = EffectiveGlobalToken.ColorTextQuaternary;
        ItemBorderRadius = EffectiveGlobalToken.BorderRadius;
        ItemBg = EffectiveGlobalToken.ColorBgElevated;
        ItemHoverBg = EffectiveGlobalToken.ColorBgTextHover;
        ItemMargin = new Thickness(0, 0, EffectiveGlobalToken.UniformlyMarginXXS, 0);
        ItemDisabledColor = EffectiveGlobalToken.ColorTextDisabled;
        DangerItemColor = EffectiveGlobalToken.ColorError;
        DangerItemHoverColor = EffectiveGlobalToken.ColorError;
        ItemHeight = EffectiveGlobalToken.ControlHeight;
        MenuPopupBgColor = EffectiveGlobalToken.ColorBgElevated;
        ItemPaddingInline = new Thickness(EffectiveGlobalToken.UniformlyPadding, EffectiveGlobalToken.UniformlyPaddingXXS);
        ItemIconSize = EffectiveGlobalToken.IconSize;
        ItemIconMarginInlineEnd = EffectiveGlobalToken.UniformlyMarginXXS;
        GroupTitlePadding = new Thickness(EffectiveGlobalToken.UniformlyPadding, EffectiveGlobalToken.UniformlyPaddingXS);
        MenuPopupMinWidth = 120;
        MenuPopupMaxWidth = 800;
        SeparatorItemHeight = EffectiveGlobalToken.LineWidth * 5;
        MenuPopupContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, EffectiveGlobalToken.BorderRadiusLG.TopLeft / 2);
    }
}
