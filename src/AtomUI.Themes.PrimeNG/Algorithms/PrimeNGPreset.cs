using System.Collections.Immutable;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms;

/// <summary>
/// One shade of a colour family: a palette stop, optionally made translucent. <see cref="White"/> and
/// <see cref="Black"/> stand for pure white (PrimeUIX <c>surface.0</c>) and pure black whatever the family.
/// </summary>
/// <param name="Stop">A palette stop (50 to 950), <see cref="White"/> or <see cref="Black"/>.</param>
/// <param name="Opacity">The opacity applied on top of the stop's own alpha, from 0 to 1.</param>
internal readonly record struct PrimeNGShade(int Stop, double Opacity = 1.0)
{
    internal const int White = 0;
    internal const int Black = 1000;

    internal Color Resolve(ReadOnlySpan<Color> palette)
    {
        var color = Stop switch
        {
            White => Colors.White,
            Black => Colors.Black,
            _     => palette[PrimeNGPalette.IndexOf(Stop)]
        };
        return Opacity >= 1.0
            ? color
            : Color.FromArgb(PrimeNGPalette.RoundHalfUp(color.A * Opacity), color.R, color.G, color.B);
    }
}

/// <summary>
/// Which shade of a semantic colour family each of its Map tokens takes (primary, a status family, or error).
/// </summary>
internal sealed record PrimeNGFamilyRule(
    PrimeNGShade Base,
    PrimeNGShade Hover,
    PrimeNGShade Active,
    PrimeNGShade Bg,
    PrimeNGShade BgHover,
    PrimeNGShade BgActive,
    PrimeNGShade Border,
    PrimeNGShade BorderHover,
    PrimeNGShade TextHover,
    PrimeNGShade Text,
    PrimeNGShade TextActive);

/// <summary>
/// Which shade of the surface (neutral) scale each neutral Map token takes. <see cref="BgBase"/> and
/// <see cref="TextBase"/> apply only when the theme leaves the matching seed unset.
/// </summary>
internal sealed record PrimeNGNeutralRule(
    PrimeNGShade BgBase,
    PrimeNGShade TextBase,
    PrimeNGShade Text,
    PrimeNGShade TextSecondary,
    PrimeNGShade TextTertiary,
    PrimeNGShade TextQuaternary,
    PrimeNGShade Fill,
    PrimeNGShade FillSecondary,
    PrimeNGShade FillTertiary,
    PrimeNGShade FillQuaternary,
    PrimeNGShade BgContainer,
    PrimeNGShade BgElevated,
    PrimeNGShade BgLayout,
    PrimeNGShade BgSpotlight,
    PrimeNGShade BgBlur,
    PrimeNGShade BgSolid,
    PrimeNGShade BgSolidHover,
    PrimeNGShade BgSolidActive,
    PrimeNGShade Border,
    PrimeNGShade BorderSecondary,
    PrimeNGShade BgMask);

/// <summary>
/// The colour rules of one appearance. <see cref="Primary"/> also drives the link colours and the selection
/// background; <see cref="Status"/> drives the success, warning and info families.
/// </summary>
/// <param name="Surface">The neutral scale, a constant of the preset (spec 4.7).</param>
/// <param name="SelectionForeground">Text on the selection background (the PrimeUIX primary contrast colour).</param>
internal sealed record PrimeNGAppearanceRules(
    ImmutableArray<Color> Surface,
    PrimeNGFamilyRule Primary,
    PrimeNGFamilyRule Status,
    PrimeNGFamilyRule Error,
    PrimeNGNeutralRule Neutral,
    PrimeNGShade SelectionForeground);

/// <summary>
/// The size ladder around the seeds, written only when the algorithm starts its chain: the radii relative to the
/// <c>BorderRadius</c> seed (never below 0) and the small and large control heights relative to the
/// <c>ControlHeight</c> seed. The font ladder is the Ant Design one, which already gives PrimeNG's 12/14/16.
/// </summary>
internal sealed record PrimeNGSizeLadder(
    double BorderRadiusXSOffset,
    double BorderRadiusSMOffset,
    double BorderRadiusLGOffset,
    double ControlHeightSMOffset,
    double ControlHeightLGOffset);

/// <summary>
/// The rule table of one PrimeNG preset (Aura, Lara or Nora): which palette stop each semantic token takes, per
/// appearance, and the size ladder. The light and dark algorithms of a preset share one table.
/// </summary>
internal sealed record PrimeNGPreset(
    PrimeNGAppearanceRules Light,
    PrimeNGAppearanceRules Dark,
    PrimeNGSizeLadder Sizes);
