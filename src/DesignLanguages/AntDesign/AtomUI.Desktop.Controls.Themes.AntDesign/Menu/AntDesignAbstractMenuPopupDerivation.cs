using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractMenuPopupToken"/> layer.</summary>
public static class AntDesignAbstractMenuPopupDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractMenuPopupToken token)
    {
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
