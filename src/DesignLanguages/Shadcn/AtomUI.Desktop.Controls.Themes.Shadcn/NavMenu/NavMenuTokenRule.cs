using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="NavMenuToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class NavMenuTokenRule : IControlTokenRule<ShadcnToken, NavMenuToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<NavMenuToken> target)
    {
        var token  = target.Own;
        var global = target.Global;

        var colorTextLightSolid = global.ColorTextLightSolid;
        // The item values keep the Ant formula; the registry values are merged at the end of the method: submenu
        // background, selected and hover fills, dark rows and the popup shadow.
        token.ActiveBarScaleX = 1.0d;
        token.ActiveBarHeight = global.LineWidthBold;
        token.ItemContentMargin = new Thickness(
                global.UniformlyMarginXXS,
                0,
                global.UniformlyMarginXXS,
                global.UniformlyMarginXXS);

        var colorTextDark = ColorUtils.FromRgbF(
            0.65d,
            colorTextLightSolid.GetRedF(),
            colorTextLightSolid.GetGreenF(),
            colorTextLightSolid.GetBlueF());

        token.ItemBorderRadius            = global.BorderRadiusLG;
        token.SubMenuItemBorderRadius     = global.BorderRadiusSM;
        token.ItemColor                   = global.ColorText;
        token.ItemHoverColor              = global.ColorText;
        token.HorizontalItemHoverColor    = global.ColorPrimary;
        token.GroupTitleColor             = global.ColorTextDescription;
        token.ItemSelectedColor           = global.ColorPrimary;
        token.HorizontalItemSelectedColor = global.ColorPrimary;
        token.ItemBg                      = global.ColorBgContainer;
        token.ItemHoverBg                 = global.ColorBgTextHover;
        token.ItemActiveBg                = global.ColorFillContent;
        token.SubMenuItemBg               = global.ColorFillAlter;
        token.ItemSelectedBg              = global.ControlItemBgActive;
        token.HorizontalItemSelectedBg    = Colors.Transparent;

        // Disabled
        token.ItemDisabledColor = global.ColorTextDisabled;

        // Danger
        token.DangerItemColor         = global.ColorError;
        token.DangerItemHoverColor    = global.ColorError;
        token.DangerItemSelectedColor = global.ColorError;
        token.DangerItemActiveBg      = global.ColorErrorBg;
        token.DangerItemSelectedBg    = global.ColorErrorBg;

        token.ItemContentPadding   = new Thickness(global.UniformlyPadding, global.UniformlyPaddingXXS);
        token.ItemMargin           = new Thickness(0, 0, global.UniformlyMarginXS, 0);
        token.ItemHeight           = global.ControlHeightLG;

        token.GroupTitleLineHeight = global.ControlHeight;
        token.CollapsedWidth       = global.ControlHeight * 2;
        token.InlineCollapsedWidth = 48d;
        token.IconMargin           = new Thickness(0, 0, global.ControlHeightSM - global.FontSize, 0);
        token.CollapsedIconSize    = global.IconSizeLG;
        token.GroupTitleFontSize   = global.FontSize;

        // Disabled
        token.DarkItemDisabledColor = ColorUtils.FromRgbF(0.25d,
            colorTextLightSolid.GetRedF(),
            colorTextLightSolid.GetGreenF(),
            colorTextLightSolid.GetBlueF());

        // Dark
        token.DarkItemColor       = colorTextDark;
        token.DarkDangerItemColor = global.ColorError;
        token.DarkItemBg          = Colors.Transparent;
        token.DarkMenuBg          = Color.Parse("#001529");
        token.DarkMenuPopupBg     = Color.Parse("#001529");
        token.DarkSubMenuItemBg   = Color.Parse("#000c17");

        token.DarkItemSelectedColor       = colorTextLightSolid;
        token.DarkItemSelectedBg          = global.ColorPrimary;
        token.DarkDangerItemSelectedBg    = global.ColorError;
        token.DarkItemHoverBg             = Colors.Transparent;
        token.DarkGroupTitleColor         = colorTextDark;
        token.DarkItemHoverColor          = colorTextLightSolid;
        token.DarkDangerItemHoverColor    = global.ColorErrorHover;
        token.DarkDangerItemSelectedColor = colorTextLightSolid;
        token.DarkDangerItemActiveBg      = global.ColorError;

        token.MenuHorizontalHeight       = global.ControlHeightLG * 1.15;
        token.HorizontalItemMargin       = new Thickness(global.UniformlyPadding, 0);
        token.HorizontalLineHeight       = global.ControlHeightLG * 1.15;
        token.HorizontalItemBorderRadius = new CornerRadius(0);
        token.HorizontalItemHoverBg      = Colors.Transparent;

        token.MenuArrowSize        = global.FontSize / 7 * 5;
        token.MenuSubMenuBg        = global.ColorBgElevated;

        token.ItemIconSize = global.IconSize;

        token.MenuPopupMinWidth               = 160d;
        token.MenuPopupMaxWidth               = 800d;
        token.MenuPopupMaxHeight              = token.ItemHeight * 8;
        token.TopLevelItemPopupMarginToAnchor = global.UniformlyMarginXXS;

        token.MenuPopupBg               = global.ColorBgElevated;
        token.MenuPopupShadows          = global.BoxShadowsSecondary;
        token.MenuPopupContentPadding   = new Thickness(0, global.UniformlyMarginXXS, 0, 0);
        token.VerticalItemsPanelSpacing = 0;
        token.VerticalChildItemsMargin  = new Thickness(0, 0, 0, global.UniformlyMarginXXS);

        token.InlineItemIndentUnit       = token.ItemHeight / 2;
        token.VerticalMenuContentPadding = new Thickness(0, global.UniformlyMarginXXS, 0, 0);
        var navLanguage = context.Language;
        // Expanded inline submenus use the root surface. The accent and hover fills share a neutral stop.
        token.SubMenuItemBg     = Colors.Transparent;
        token.DarkSubMenuItemBg = Colors.Transparent;
        token.ItemSelectedBg    = ShadcnRecipes.CompositeOver(navLanguage.Foreground, 0.14, token.ItemSelectedBg);
        if (context.Variant.IsDark)
        {
            // IsDarkStyle keeps its own menu surface: the row tints over that surface and the dark foreground for row text,
            // since the primary button foreground is inverted.
            token.DarkItemColor         = navLanguage.MutedForeground;
            token.DarkGroupTitleColor   = navLanguage.MutedForeground;
            token.DarkItemDisabledColor = ShadcnRecipes.CompositeOver(navLanguage.MutedForeground, 0.5, token.DarkMenuBg);
            token.DarkItemHoverColor    = navLanguage.Foreground;
            token.DarkItemSelectedColor = navLanguage.Foreground;
            token.DarkItemHoverBg       = ShadcnRecipes.CompositeOver(navLanguage.Foreground, 0.12, token.DarkMenuBg);
            token.DarkItemSelectedBg    = ShadcnRecipes.CompositeOver(navLanguage.Foreground, 0.25, token.DarkMenuBg);
        }
        // The popup of an inline item: the outline ring ahead of the menu shadow of the style.
        token.MenuPopupShadows = navLanguage.GetMenuOverlayRecipe(global).ShadowsWithOutline;
    }
}
