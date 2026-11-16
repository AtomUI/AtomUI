using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// Overlays without an arrow (menus, select and the other input panels): the style's outline ring — the foreground
/// at the overlay ring alpha — ahead of the menu shadow, the radius from the overlay table (design 5.6.1).
/// </summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnPopupTokens : IControlTokenAlgorithm<PopupToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, PopupToken token)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var style   = ShadcnStyles.Resolve(context);
        var surface = ShadcnBaseColors.Resolve(effectiveGlobal.ColorNeutral, dark);
        var shadows = ShadcnStyles.RingThenShadow(
            ShadcnStyles.OutlineRing(surface, dark, style), style.Overlay.MenuShadow);
        token.PopupRootShadow   = shadows;
        token.OverlayHostShadow = shadows;
        token.PopupCornerRadius = new CornerRadius(style.Overlay.MenuRadius);
    }
}

/// <summary>
/// The arrow flyouts (popover, confirm popup, picker panels): the plain popover shadow — no ring, the arrow box
/// draws the integrated border (design 5.6.2).
/// </summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnFlyoutHostTokens : IControlTokenAlgorithm<FlyoutHostToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, FlyoutHostToken token)
    {
        var style = ShadcnStyles.Resolve(context);
        token.PopupRootShadow   = ShadcnStyles.Shadow(style.Overlay.PopoverShadow);
        token.OverlayHostShadow = token.PopupRootShadow;
    }
}

/// <summary>
/// The arrow box of the flyouts: the outline ring colour as an integrated one-line border that also runs along the
/// arrow's sides, with the popover padding (design 5.6.2's fix of the old "pure shadow" look).
/// </summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnArrowDecoratedBoxTokens : IControlTokenAlgorithm<ArrowDecoratedBoxToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ArrowDecoratedBoxToken token)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var style   = ShadcnStyles.Resolve(context);
        var surface = ShadcnBaseColors.Resolve(effectiveGlobal.ColorNeutral, dark);
        token.ContentBorderColor     = ShadcnStyles.OutlineRing(surface, dark, style);
        token.ContentBorderThickness = new Thickness(1);
        token.ContentPadding         = new Thickness(style.Overlay.PopoverPadding);
    }
}
