using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractMenuPopupToken : AbstractControlDesignToken
{
    public partial Thickness MenuPopupContentPadding { get; set; }
    public partial double MenuPopupMinWidth { get; set; }
    public partial double MenuPopupMaxWidth { get; set; }
    public partial Color MenuPopupBgColor { get; set; }
    public partial Color ItemColor { get; set; }
    public partial Color KeyGestureColor { get; set; }
    public partial Thickness ItemMargin { get; set; }
    public partial Color ItemHoverColor { get; set; }
    public partial Color ItemDisabledColor { get; set; }
    public partial Color DangerItemColor { get; set; }
    public partial Color DangerItemHoverColor { get; set; }
    public partial Color ItemBg { get; set; }
    public partial Color ItemHoverBg { get; set; }
    public partial double ItemHeight { get; set; }
    public partial double ItemIconSize { get; set; }
    public partial double ItemIconMarginInlineEnd { get; set; }
    public partial Thickness GroupTitlePadding { get; set; }
    public partial CornerRadius ItemBorderRadius { get; set; }
    public partial Thickness ItemPaddingInline { get; set; }
    public partial double SeparatorItemHeight { get; set; }

    /// <summary>
    /// The shadows of the menu's popups (the context menu, submenus and menubar drop-downs). Defaults to
    /// <c>BoxShadowsSecondary</c>, the value the shared Popup Token gives other overlays, so a theme can give menus
    /// their own frame (for example an outline ring in a menu-specific border colour).
    /// </summary>
    public partial BoxShadows MenuPopupShadows { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
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
        MenuPopupShadows = EffectiveGlobalToken.BoxShadowsSecondary;
        MenuPopupContentPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, EffectiveGlobalToken.BorderRadiusLG.TopLeft / 2);
    }
}
