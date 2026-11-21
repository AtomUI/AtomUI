using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// The menu family under the shadcn styles: the menus read their own <c>MenuPopupShadows</c> scope (not
/// <c>Controls[Popup]</c>), so every menu identity — Menu, ContextMenu, the flyout presenter and NavMenu — carries
/// the outline ring ahead of the style's menu shadow level, and the flyout presenter's integrated border takes the
/// ring colour (design 5.6.1). One class per token identity (the generator allows one interface each).
/// </summary>
internal static class ShadcnMenuShadows
{
    public static void Apply(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, AbstractMenuPopupToken token)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var style   = ShadcnStyles.Resolve(context);
        var surface = ShadcnBaseColors.Resolve(effectiveGlobal.ColorNeutral, dark);
        token.MenuPopupShadows = ShadcnStyles.RingThenShadow(
            ShadcnStyles.OutlineRing(surface, dark, style), style.Overlay.MenuShadow);
    }
}

/// <summary>The Menu (menu bar's dropdown popups) under the shadcn styles.</summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea.Dark", 1)]
internal sealed class ShadcnMenuTokens : IControlTokenAlgorithm<MenuToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, MenuToken token)
    {
        ShadcnMenuShadows.Apply(context, effectiveGlobal, token);

        // Square presets still need space between the first and last hover fills and the popup edges.
        var padding = token.MenuPopupContentPadding;
        var minVerticalPadding = effectiveGlobal.UniformlyPaddingXXS;
        token.MenuPopupContentPadding = new Thickness(padding.Left,
            Math.Max(padding.Top, minVerticalPadding),
            padding.Right,
            Math.Max(padding.Bottom, minVerticalPadding));
    }
}

/// <summary>The ContextMenu popups under the shadcn styles.</summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea.Dark", 1)]
internal sealed class ShadcnContextMenuTokens : IControlTokenAlgorithm<ContextMenuToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ContextMenuToken token)
    {
        ShadcnMenuShadows.Apply(context, effectiveGlobal, token);
    }
}

/// <summary>
/// The MenuFlyoutPresenter (flyout menus) under the shadcn styles: the ring plus the menu shadow, and the
/// integrated border the ring colour.
/// </summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea.Dark", 1)]
internal sealed class ShadcnMenuFlyoutPresenterTokens : IControlTokenAlgorithm<MenuFlyoutPresenterToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, MenuFlyoutPresenterToken token)
    {
        ShadcnMenuShadows.Apply(context, effectiveGlobal, token);
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var style   = ShadcnStyles.Resolve(context);
        var surface = ShadcnBaseColors.Resolve(effectiveGlobal.ColorNeutral, dark);
        token.MenuPopupBorderColor   = ShadcnStyles.OutlineRing(surface, dark, style);
        token.MenuPopupBorderThickness = new Avalonia.Thickness(1);
    }
}

/// <summary>The NavMenu row fills and submenu surfaces under the shadcn styles.</summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Vega.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Maia.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Lyra.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Mira.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Luma.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Sera.Dark", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Rhea.Dark", 1)]
internal sealed class ShadcnNavMenuTokens : IControlTokenAlgorithm<NavMenuToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, NavMenuToken token)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var style   = ShadcnStyles.Resolve(context);
        var surface = ShadcnBaseColors.Resolve(effectiveGlobal.ColorNeutral, dark);
        // Expanded inline submenus use the root surface. The accent and hover fills share a neutral stop.
        token.SubMenuItemBg = Colors.Transparent;
        token.DarkSubMenuItemBg = Colors.Transparent;
        token.ItemSelectedBg = ShadcnBaseColors.CompositeOver(surface.Foreground, 0.14, token.ItemSelectedBg);

        if (dark)
        {
            // IsDarkStyle retains its own menu surface. Use that surface for the row tints and the dark
            // appearance foreground for row text, since the primary button foreground is inverted.
            token.DarkItemColor         = surface.MutedForeground;
            token.DarkGroupTitleColor   = surface.MutedForeground;
            token.DarkItemDisabledColor = ShadcnBaseColors.CompositeOver(surface.MutedForeground, 0.5, token.DarkMenuBg);
            token.DarkItemHoverColor    = surface.Foreground;
            token.DarkItemSelectedColor = surface.Foreground;
            token.DarkItemHoverBg       = ShadcnBaseColors.CompositeOver(surface.Foreground, 0.12, token.DarkMenuBg);
            token.DarkItemSelectedBg    = ShadcnBaseColors.CompositeOver(surface.Foreground, 0.25, token.DarkMenuBg);
        }

        token.MenuPopupShadows = ShadcnStyles.RingThenShadow(
            ShadcnStyles.OutlineRing(surface, dark, style), style.Overlay.MenuShadow);
    }
}
