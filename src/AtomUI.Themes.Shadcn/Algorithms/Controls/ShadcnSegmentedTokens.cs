using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// The Segmented under the shadcn styles: shadcn has no segmented component, so the Ant track pattern stays
/// (grey track, elevated thumb, soft thumb shadow) and only the palette flows in from the shadcn globals —
/// thumb = <c>--popover</c>, hover = <c>--secondary</c>, text = <c>--foreground</c>, radius ladder = the global
/// per-style <c>BorderRadius</c> (clamped at draw time by <c>CornerRadiusUtils</c>). The one override is the
/// track: the default reads <c>ColorBgLayout</c>, which the shadcn Map maps to <c>--background</c> (the page
/// itself), so the track would disappear into the page — it reads <c>--muted</c> instead, the grey the Ant
/// track renders in.
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
internal sealed class ShadcnSegmentedTokens : IControlTokenAlgorithm<SegmentedToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SegmentedToken token)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var surface = ShadcnBaseColors.Resolve(effectiveGlobal.ColorNeutral, dark);
        token.TrackBg = surface.Muted;
    }
}
