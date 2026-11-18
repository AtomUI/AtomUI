using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// The Empty preset-illustration palette of the shadcn styles. The built-in illustration draws its tones from the
/// fill ramp (<c>ColorFill</c> for the accent, <c>ColorFillTertiary</c> / <c>ColorFillQuaternary</c> for the body
/// and the faintest tone) plus <c>ColorBorderSecondary</c>, but the shadcn base colours put the whole ramp on one
/// neutral stop (<c>--accent</c>, <c>--secondary</c> and <c>--muted</c> are all stop 100). Empty therefore rebuilds
/// the antd-shaped ladder here, as a foreground-over-background composite so it keeps its direction in dark mode
/// and for every base colour.
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
internal sealed class ShadcnEmptyTokens : IControlTokenAlgorithm<EmptyToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var surface = ShadcnBaseColors.Resolve(controlGlobal.ColorNeutral, dark);

        // The Ant Design fill ladder (15% / 6% / 4% / 2% of the foreground), the tones the preset image expects.
        controlGlobal.ColorFill            = ShadcnBaseColors.CompositeOver(surface.Foreground, 0.15, surface.Background);
        controlGlobal.ColorFillSecondary   = ShadcnBaseColors.CompositeOver(surface.Foreground, 0.06, surface.Background);
        controlGlobal.ColorFillTertiary    = ShadcnBaseColors.CompositeOver(surface.Foreground, 0.04, surface.Background);
        controlGlobal.ColorFillQuaternary  = ShadcnBaseColors.CompositeOver(surface.Foreground, 0.02, surface.Background);
        controlGlobal.ColorBorderSecondary = ShadcnBaseColors.CompositeOver(surface.Foreground, 0.06, surface.Background);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, EmptyToken token)
    {
    }
}
