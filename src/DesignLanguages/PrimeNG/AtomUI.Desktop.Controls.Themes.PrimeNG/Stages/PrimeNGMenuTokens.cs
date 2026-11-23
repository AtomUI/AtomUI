using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

// Menus under the PrimeNG presets: PrimeUIX 3.1.1 menu, tieredmenu, contextmenu and the menubar submenu draw a
// content.border.color border (the preset's MenuBorder row: Aura and Lara surface 200 / 700, as their select panels;
// Nora surface 400 / 500, while its select panels and popovers are transparent / surface 700) with the
// overlay.navigation.shadow (the global BoxShadowsSecondary). The menus' popup frame is their own MenuPopupShadows
// (a one-line outline ring ahead of the shadow, as the select panels' ring), and the menu flyout's arrow box border is
// MenuFlyoutPresenter.MenuPopupBorderColor.

/// <summary>
/// The shared value of the PrimeNG menu stages.
/// </summary>
internal static class PrimeNGMenuFrame
{
    /// <summary>The menu popup frame with the PrimeUIX content border and navigation shadow.</summary>
    internal static BoxShadows Shadows(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal) =>
        context.Language.GetMenuOverlayRecipe(effectiveGlobal, context.Variant).ShadowsWithOutline;

    /// <summary>
    /// The focused (hovered) item of a menu: PrimeUIX <c>item.focus.background</c> / <c>focus.color</c> (Aura and Lara
    /// navigation.item.active, Nora the solid primary with its contrast colour).
    /// </summary>
    internal static (Color Background, Color Foreground) Focus(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal)
    {
        if (context.Variant.Style == PrimeNGLanguage.Nora)
        {
            return (effectiveGlobal.ColorPrimary, effectiveGlobal.ColorTextLightSolid);
        }
        return context.Language.GetVisualRecipes(effectiveGlobal, context.Variant).NavigationItemActive;
    }

    /// <summary>
    /// Popup menu rows, PrimeUIX 3.1.1 <c>tieredmenu</c> / <c>menu</c> / <c>contextmenu</c>: <c>item.padding</c> 4px
    /// 10px (Lara 8px 14px) around the 1.5 line box, <c>item.gap</c> 8 between icon and label,
    /// <c>item.border.radius</c> 4 (Lara and Nora 0), <c>list.padding</c> 4px 4px (Lara 8px 0, Nora 2px 0), a
    /// <c>min-width</c> of 12.5rem and the focus colours.
    /// </summary>
    internal static void ApplyItems(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, AbstractMenuPopupToken token)
    {
        var kind    = context.Variant.Style;
        var padding = kind == PrimeNGLanguage.Lara ? new Thickness(14, 8) : new Thickness(10, 4);
        token.ItemPaddingInline       = padding;
        token.GroupTitlePadding       = padding;
        token.ItemHeight              = effectiveGlobal.FontSize * 1.5 + padding.Top + padding.Bottom;
        token.ItemIconMarginInlineEnd = 8;
        token.ItemBorderRadius        = kind == PrimeNGLanguage.Aura ? effectiveGlobal.BorderRadiusSM : new CornerRadius(0);
        token.MenuPopupContentPadding = context.Language.GetMenuOverlayRecipe(effectiveGlobal, context.Variant).Padding;
        token.MenuPopupMinWidth = 200;
        (token.ItemHoverBg, token.ItemHoverColor) = Focus(context, effectiveGlobal);
    }
}

/// <summary>
/// The menubar (<c>Menu</c>) under the PrimeNG presets: its drop-downs and submenus take the menu frame.
/// </summary>
internal static class PrimeNGMenuTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, MenuToken token)
    {
        token.MenuPopupShadows = PrimeNGMenuFrame.Shadows(context, effectiveGlobal);
        PrimeNGMenuFrame.ApplyItems(context, effectiveGlobal, token);
        // The menu bar, PrimeUIX menubar: base.item.padding 4px 10px (Lara 10px 14px), item.focus for hover and
        // item.active (navigation.item.active) while a submenu is open, one radius at every size.
        var kind = context.Variant.Style;
        var active = context.Language.GetVisualRecipes(effectiveGlobal, context.Variant).NavigationItemActive;
        token.TopLevelItemPadding = kind == PrimeNGLanguage.Lara ? new Thickness(14, 10) : new Thickness(10, 4);
        (token.TopLevelItemHoverBg, token.TopLevelItemHoverColor) = PrimeNGMenuFrame.Focus(context, effectiveGlobal);
        token.TopLevelItemSelectedBg    = active.Background;
        token.TopLevelItemSelectedColor = active.Foreground;
        token.TopLevelItemBorderRadius   = effectiveGlobal.BorderRadius;
        token.TopLevelItemBorderRadiusSM = effectiveGlobal.BorderRadius;
        token.TopLevelItemBorderRadiusLG = effectiveGlobal.BorderRadius;
    }
}

/// <summary>
/// The context menu under the PrimeNG presets: it and its submenus take the menu frame.
/// </summary>
internal static class PrimeNGContextMenuTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
        PrimeNGMotion.Restore(context, controlGlobal, mid: PrimeNGMotion.AnchoredOverlay);
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, ContextMenuToken token)
    {
        token.MenuPopupShadows = PrimeNGMenuFrame.Shadows(context, effectiveGlobal);
        PrimeNGMenuFrame.ApplyItems(context, effectiveGlobal, token);
    }
}

/// <summary>
/// The menu flyout (drop-down menus) under the PrimeNG presets: its arrow box border is the menu border (PrimeUIX
/// <c>menu</c> as a popup) and its submenus take the menu frame.
/// </summary>
internal static class PrimeNGMenuFlyoutPresenterTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, MenuFlyoutPresenterToken token)
    {
        token.MenuPopupBorderColor   = context.Language.GetMenuOverlayRecipe(effectiveGlobal, context.Variant).BorderColor;
        token.MenuPopupBorderThickness = new Avalonia.Thickness(effectiveGlobal.LineWidth);
        token.MenuPopupShadows     = PrimeNGMenuFrame.Shadows(context, effectiveGlobal);
        PrimeNGMenuFrame.ApplyItems(context, effectiveGlobal, token);
    }
}

/// <summary>
/// MenuItemGroup under the PrimeNG presets, PrimeUIX 3.1.1 <c>menu.submenu.label.font.weight</c>: 600 (Nora 700).
/// </summary>
internal static class PrimeNGMenuItemGroupTokens
{
    internal static void EvaluateGlobal(in DerivationContext<PrimeNGToken> context, DesignToken controlGlobal)
    {
    }
    internal static void EvaluateOwn(in DerivationContext<PrimeNGToken> context, DesignToken effectiveGlobal, MenuItemGroupToken token)
    {
        token.TitleFontWeight = context.Variant.Style == PrimeNGLanguage.Nora ? FontWeight.Bold : FontWeight.SemiBold;
    }
}
