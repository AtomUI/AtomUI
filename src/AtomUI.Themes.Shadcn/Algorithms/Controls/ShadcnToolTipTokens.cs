using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// The Tooltip under the shadcn styles: the inverted surface (the base color's foreground as the background and
/// back), the style's radius, the 12 × 6 padding and the 12px text; no shadow and no ring (design 5.6.1).
/// </summary>
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova", 1)]
[ControlTokenAlgorithm(ShadcnStyles.Catalog, "Nova.Dark", 1)]
internal sealed class ShadcnToolTipTokens : IControlTokenAlgorithm<ToolTipToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ToolTipToken token)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var style   = ShadcnStyles.Resolve(context);
        var surface = ShadcnBaseColors.Resolve(effectiveGlobal.ColorNeutral, dark);
        token.ToolTipBackground = surface.Foreground;
        token.ToolTipColor      = surface.Background;
        token.ToolTipCornerRadius = new CornerRadius(style.Overlay.TooltipRadius);
        token.ContentPadding    = new Thickness(12, 6, 12, 6);
        // The official tooltip has no shadow and no ring.
        token.ToolTipShadows    = default;
    }
}
