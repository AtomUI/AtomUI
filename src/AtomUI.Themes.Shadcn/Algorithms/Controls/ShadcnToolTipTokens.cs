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
internal sealed class ShadcnToolTipTokens : IControlTokenAlgorithm<ToolTipToken>
{
    /// <summary>
    /// TooltipPrimitive.Arrow: a size-2.5 square rotated 45° with rounded-[2px] corners, moved up by half its size
    /// plus 2px, so 5√2 - 2 of its unrounded corner shows beyond the box: a right-angled arrow twice that wide with a
    /// 2px tip and no flare.
    /// </summary>
    internal static readonly double ArrowSize = 2 * (5 * Math.Sqrt(2) - 2);

    /// <summary>The <c>rounded-[2px]</c> corner of the arrow square.</summary>
    internal const double ArrowTipRadius = 2;

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
        token.ArrowSize       = ArrowSize;
        token.ArrowTipRadius  = ArrowTipRadius;
        token.ArrowBaseRadius = 0;
    }
}
