using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.Shadcn.Algorithms.Controls;

/// <summary>
/// The Segmented under the shadcn styles is the official toggle-group look: no track background and no border,
/// the selected item a muted rounded pill (the item radius per style, appendix C: rounded-md/xl/4xl/full) with
/// no shadow — not the Ant grey track with the white shadowed thumb.
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
        var style = ShadcnStyles.Resolve(context);
        // The theme reads the global radius ladder: track = BorderRadius (LG for the large size), the selected
        // pill = BorderRadiusSM (XS for the small size). Item radius: rounded-md 8 (Vega/Nova/Mira), rounded-xl 14
        // (Maia), the capsule styles stay at the container radius (Luma full, Rhea 18), Lyra/Sera square.
        var (track, thumb) = style.Id switch
        {
            "Vega" or "Mira" => (8d, 8d),
            "Nova"                     => (10d, 10d),
            "Maia"                     => (26d, 14d),
            "Luma"                     => (26d, 26d),
            "Rhea"                     => (18d, 18d),
            _                          => (0d, 0d)
        };
        controlGlobal.BorderRadius   = new CornerRadius(track);
        controlGlobal.BorderRadiusLG = new CornerRadius(track);
        controlGlobal.BorderRadiusSM = new CornerRadius(thumb);
        controlGlobal.BorderRadiusXS = new CornerRadius(thumb);
        // The theme binds the thumb shadow to the global BoxShadowsTertiary (the Shadcn card ring); the official
        // pill has no shadow.
        controlGlobal.BoxShadowsTertiary = default;
    }

    /// <inheritdoc />
    public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, SegmentedToken token)
    {
        var dark    = context.Appearance == ThemeAppearance.Dark;
        var surface = ShadcnBaseColors.Resolve(effectiveGlobal.ColorNeutral, dark);
        token.TrackBg           = Colors.Transparent; // no track: the items sit on the page
        token.ItemSelectedBg    = surface.Muted;      // data-[state=on]:bg-muted
        token.ItemSelectedColor = surface.Foreground;
        token.ItemHoverBg       = surface.Muted;      // hover:bg-muted
        token.ItemHoverColor    = surface.Foreground;
        token.ItemActiveBg      = surface.Muted;
        token.ItemColor         = surface.MutedForeground;
    }
}
