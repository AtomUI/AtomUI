using System.Collections.Immutable;
using AtomUI.Theme;
using Avalonia;
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
    PrimeNGShade BgMask,
    PrimeNGShade BgContainerDisabled,
    PrimeNGShade TextDisabled);

/// <summary>
/// The colour rules of one appearance. <see cref="Primary"/> also drives the link colours and the selection
/// background; <see cref="Status"/> drives the success, warning and info families.
/// </summary>
/// <param name="Surface">The neutral scale, a constant of the preset (spec 4.7).</param>
/// <param name="SelectionForeground">
/// Text on the selection background and on solid primary fills (the PrimeUIX primary contrast colour); the Alias
/// stage also writes it to <c>ColorTextLightSolid</c>.
/// </param>
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
/// <see cref="BorderRadiusXLOffset"/> has no global token: the control stages use it for the PrimeUIX
/// <c>border.radius.xl</c> of cards and dialogs.
/// </summary>
internal sealed record PrimeNGSizeLadder(
    double BorderRadiusXSOffset,
    double BorderRadiusSMOffset,
    double BorderRadiusLGOffset,
    double BorderRadiusXLOffset,
    double ControlHeightSMOffset,
    double ControlHeightLGOffset);

/// <summary>
/// The Alias rules of a preset that do not depend on the appearance. Widths are multiples of the <c>LineWidth</c>
/// Map token, so they follow a theme that changes it.
/// </summary>
/// <param name="FocusVisualBorderLineWidths">The focus ring width, in line widths (PrimeUIX <c>focus.ring.width</c>).</param>
/// <param name="ControlOutlineLineWidths">The width of the glow around a focused field, in line widths.</param>
/// <param name="BoxShadows">Modal and large overlays (PrimeUIX <c>overlay.modal.shadow</c>).</param>
/// <param name="BoxShadowsSecondary">Popovers and select panels (PrimeUIX <c>overlay.popover.shadow</c>).</param>
internal sealed record PrimeNGAliasRule(
    double FocusVisualBorderLineWidths,
    double ControlOutlineLineWidths,
    BoxShadows BoxShadows,
    BoxShadows BoxShadowsSecondary);

/// <summary>
/// The fixed control stage values of a preset. The sizes are the values of the pre-algorithm XML profiles (the
/// acceptance baseline); the compact profiles have always kept them, so they do not follow <c>AtomUI:Compact</c>.
/// For Aura they equal the <c>aura-compat</c> preset of <c>@primeuix/themes</c> 3.1.1 at a 16px root; the standard
/// <c>aura</c> preset is denser (for example card padding 18, tooltip padding 6/10). Values that follow from other
/// tokens are computed in the control stages instead.
/// </summary>
internal sealed record PrimeNGControlRule(
    FontWeight ButtonFontWeight,
    double CheckIndicatorSize,
    double CheckedMarkSize,
    double CardBodyPadding,
    BoxShadows CardRestingShadows,
    double DialogPadding,
    Thickness ToolTipPadding,
    PrimeNGShade ToolTipColor);

/// <summary>
/// The rule table of one PrimeNG preset (Aura, Lara or Nora): which palette stop each semantic token takes, per
/// appearance, and the size ladder. The light and dark algorithms of a preset share one table.
/// </summary>
internal sealed record PrimeNGPreset(
    PrimeNGAppearanceRules Light,
    PrimeNGAppearanceRules Dark,
    PrimeNGSizeLadder Sizes,
    PrimeNGAliasRule Alias,
    PrimeNGControlRule Controls)
{
    /// <summary>
    /// The rules of the appearance a chain resolves to.
    /// </summary>
    internal PrimeNGAppearanceRules For(ThemeAppearance appearance) =>
        appearance == ThemeAppearance.Dark ? Dark : Light;
}
