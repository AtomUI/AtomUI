using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="NavMenuToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGNavMenuDerivation : IControlDerivation<PrimeNGToken, NavMenuToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<NavMenuToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGNavMenuTokens.EvaluateGlobal(in context, global);

        var colorTextLightSolid = global.ColorTextLightSolid;
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
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGNavMenuTokens.EvaluateOwn(in context, global, token);
        // PrimeNG: the formulas that read a value the preset replaced above run again with it, as the previous
        // staged evaluation did (its second round kept the preset's value).
        token.MenuPopupMaxHeight              = token.ItemHeight * 8;
    }
}
