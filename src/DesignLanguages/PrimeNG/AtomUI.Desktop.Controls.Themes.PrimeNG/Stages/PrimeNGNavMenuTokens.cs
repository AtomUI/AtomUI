using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// NavMenu under the PrimeNG presets: the selected, hovered and keyboard-active items follow PrimeUIX 3.1.1
/// <c>navigation.item</c> (menubar, tieredmenu, menu, panelmenu), which never uses the highlight: an active item (an
/// open submenu's path) has the neutral <c>navigation.item.active.background</c> (Aura and Lara surface 100 / 800,
/// Nora surface 200 / 700) with <c>navigation.item.active.color</c> (<c>text.hover.color</c>; Nora dark
/// <c>text.color</c>), and Aura and Lara focus (hover) on the same pair. PrimeNG menus have no persistent selection;
/// the selected item takes the active pair, so a NavMenu looks like PrimeNG's own navigation in every preset and
/// under any primary (noir included). The selected-path headers keep that text colour on the menu background, where
/// it stays readable.
/// <para>
/// Nora focuses on the solid primary with the contrast colour (<c>navigation.item.focus.background</c>
/// <c>primary.color</c>). AtomUI keeps a hovered selected-path header in the selected text colour, which would vanish
/// on the solid primary (noir light: surface 950 on surface 950), so Nora hovers on its neutral
/// <c>content.hover.background</c> / <c>content.hover.color</c> (surface 200 / 700, the active pair) instead. The
/// horizontal menu (PrimeUIX menubar) shows the active pair with the content radius (<c>menubar.base.item.border.radius</c>)
/// and without Ant Design's indicator bar.
/// </para>
/// </summary>
internal static class PrimeNGNavMenuTokens
{
    internal static void EvaluateGlobal(in TokenEvaluationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, mid: PrimeNGMotion.Collapsible);
    }
    internal static void EvaluateOwn(in TokenEvaluationContext<PrimeNGToken> context, DesignToken effectiveGlobal, NavMenuToken token)
    {
        // The dark menu keeps AtomUI's fixed navy surface; its text is white-based in both appearances (under noir
        // dark the contrast colour ColorTextLightSolid is near-black and would vanish on it). The selected item keeps
        // the contrast colour on its primary fill.
        var white = Colors.White;
        token.DarkItemColor         = Color.FromArgb(0xA6, 255, 255, 255);
        token.DarkGroupTitleColor   = Color.FromArgb(0xA6, 255, 255, 255);
        token.DarkItemHoverColor    = white;
        token.DarkItemDisabledColor = Color.FromArgb(0x40, 255, 255, 255);
        var (background, color) = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant).NavigationItemActive;

        token.ItemSelectedBg    = background;
        token.ItemSelectedColor = color;
        token.ItemHoverBg       = background;
        token.ItemHoverColor    = color;
        token.ItemActiveBg      = background;

        token.HorizontalItemSelectedBg    = background;
        token.HorizontalItemSelectedColor = color;
        token.HorizontalItemHoverBg       = background;
        token.HorizontalItemBorderRadius  = effectiveGlobal.BorderRadius;
        token.ActiveBarHeight             = 0;

        // The submenu popups of the horizontal and vertical menu take the menu frame (PrimeUIX menubar submenu).
        token.MenuPopupShadows = PrimeNGMenuFrame.Shadows(context, effectiveGlobal);

        // PrimeUIX panelmenu: item.padding 4px 10px (Lara 8px 14px) around the 1.5 line box, item.border.radius
        // border.radius.md, panel.background content.background for the items and the submenus alike,
        // submenu.indent 16 (Lara 14), item.gap 8; menubar item.focus.color for the horizontal hover text;
        // submenu.label.color text.muted in Aura, text.color in Lara and Nora.
        var kind    = context.Variant.Style;
        var padding = kind == PrimeNGLanguage.Lara ? new Avalonia.Thickness(14, 8) : new Avalonia.Thickness(10, 4);
        token.ItemContentPadding      = padding;
        token.ItemHeight              = effectiveGlobal.FontSize * 1.5 + padding.Top + padding.Bottom;
        token.ItemBorderRadius        = effectiveGlobal.BorderRadius;
        token.SubMenuItemBorderRadius = effectiveGlobal.BorderRadius;
        token.ItemBg                  = effectiveGlobal.ColorBgElevated;
        token.SubMenuItemBg           = effectiveGlobal.ColorBgElevated;
        token.InlineItemIndentUnit    = kind == PrimeNGLanguage.Lara ? 14 : 16;
        token.IconMargin              = new Avalonia.Thickness(0, 0, 8, 0);
        token.HorizontalItemHoverColor = color;
        token.GroupTitleColor = kind == PrimeNGLanguage.Aura ? effectiveGlobal.ColorTextSecondary : effectiveGlobal.ColorText;
        token.MenuPopupMinWidth = 200;
    }
}
