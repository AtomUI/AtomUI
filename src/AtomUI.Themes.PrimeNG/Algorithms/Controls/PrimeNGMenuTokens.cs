using AtomUI.Desktop.Controls;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms.Controls;

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
    /// <summary>
    /// The preset's menu border (<c>content.border.color</c>) for the chain's appearance.
    /// </summary>
    internal static Color Border(in ThemeAlgorithmContext context)
    {
        var rules = PrimeNGPresets.Resolve(context).For(context.Appearance);
        return rules.MenuBorder.Resolve(rules.Surface.AsSpan());
    }

    /// <summary>
    /// The menu popup frame: the global <c>BoxShadowsSecondary</c> (<c>overlay.navigation.shadow</c>) behind a one-line
    /// ring in the menu border.
    /// </summary>
    internal static BoxShadows Shadows(in ThemeAlgorithmContext context, DesignToken effectiveGlobal) =>
        PrimeNGPopupTokens.WithOutlineRing(effectiveGlobal.BoxShadowsSecondary, Border(context), effectiveGlobal.LineWidth);
}

/// <summary>
/// The menubar (<c>Menu</c>) under the PrimeNG presets: its drop-downs and submenus take the menu frame.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGMenuTokens : IControlTokenAlgorithm<MenuToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, MenuToken token)
    {
        token.MenuPopupShadows = PrimeNGMenuFrame.Shadows(context, effectiveGlobal);
    }
}

/// <summary>
/// The context menu under the PrimeNG presets: it and its submenus take the menu frame.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGContextMenuTokens : IControlTokenAlgorithm<ContextMenuToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ContextMenuToken token)
    {
        token.MenuPopupShadows = PrimeNGMenuFrame.Shadows(context, effectiveGlobal);
    }
}

/// <summary>
/// The menu flyout (drop-down menus) under the PrimeNG presets: its arrow box border is the menu border (PrimeUIX
/// <c>menu</c> as a popup) and its submenus take the menu frame.
/// </summary>
[ControlTokenAlgorithm("PrimeNG", "Aura", 1)]
[ControlTokenAlgorithm("PrimeNG", "Aura.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara", 1)]
[ControlTokenAlgorithm("PrimeNG", "Lara.Dark", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora", 1)]
[ControlTokenAlgorithm("PrimeNG", "Nora.Dark", 1)]
internal sealed class PrimeNGMenuFlyoutPresenterTokens : IControlTokenAlgorithm<MenuFlyoutPresenterToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, MenuFlyoutPresenterToken token)
    {
        token.MenuPopupBorderColor = PrimeNGMenuFrame.Border(context);
        token.MenuPopupShadows     = PrimeNGMenuFrame.Shadows(context, effectiveGlobal);
    }
}
