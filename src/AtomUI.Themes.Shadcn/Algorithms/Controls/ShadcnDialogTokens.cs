using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// The Dialog surface under the shadcn styles: the dialog surface reads the dialog's effective global
/// <c>BoxShadows</c> (<c>DialogSurfaceTheme</c>), which this stage assembles as the overlay ring plus the style's
/// dialog shadow level (design 5.6.1) — the dialog shadow differs from the card's in Luma, Sera and Rhea, so it
/// cannot ride the global value.
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
internal sealed class ShadcnDialogTokens : IControlTokenAlgorithm<DialogToken>
{
    /// <inheritdoc />
    public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var style   = ShadcnStyles.Resolve(context);
        var surface = ShadcnBaseColors.Resolve(controlGlobal.ColorNeutral, dark);
        var ring    = ShadcnStyles.OutlineRing(surface, dark, style);
        controlGlobal.BoxShadows = ShadcnStyles.RingThenShadow(ring, style.Overlay.DialogShadow);
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, DialogToken token)
    {
    }
}
