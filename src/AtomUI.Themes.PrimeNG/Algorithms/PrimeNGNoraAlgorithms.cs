using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms;

/// <summary>
/// The light PrimeNG Nora preset as a theme algorithm (<c>PrimeNG:Nora</c>): primary 600/700/800 of the 500 seed,
/// slate surfaces with slate-900 text and slate-500 borders, transparent secondary borders, 2px radii and a two-line
/// focus ring.
/// </summary>
[ThemeAlgorithm("PrimeNG", "Nora", 1, ThemeAppearanceEffect.Light)]
public sealed class PrimeNGNoraAlgorithm : PrimeNGPresetAlgorithm
{
    public PrimeNGNoraAlgorithm()
        : base(PrimeNGNoraPreset.Table, dark: false)
    {
    }
}

/// <summary>
/// The dark PrimeNG Nora preset as a theme algorithm (<c>PrimeNG:Nora.Dark</c>): primary 500/400/300 from the same 500
/// seed, a solid primary selected background with zinc-950 text, zinc surfaces with zinc-500 borders, and a focus ring
/// in the border colour.
/// </summary>
[ThemeAlgorithm("PrimeNG", "Nora.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class PrimeNGNoraDarkAlgorithm : PrimeNGPresetAlgorithm
{
    public PrimeNGNoraDarkAlgorithm()
        : base(PrimeNGNoraPreset.Table, dark: true)
    {
    }
}

/// <summary>
/// The Nora rule table (spec 4.7), transcribed from the four pre-algorithm Nora XML profiles (the acceptance baseline).
/// It differs from Aura in the primary and status stops (one stop darker in both appearances: light 600, dark 500), the
/// deeper text and border stops, the transparent secondary border, the solid dark selected background, the dark focus
/// ring, the 2px radius ladder and the control values; the README of this package lists the sources row by row.
/// </summary>
internal static class PrimeNGNoraPreset
{
    private const int White = PrimeNGShade.White;
    private const int Black = PrimeNGShade.Black;

    internal static readonly PrimeNGPreset Table = new(
        Light: new PrimeNGAppearanceRules(
            Surface: PrimeNGTailwindPalettes.Slate,
            Primary: new PrimeNGFamilyRule(
                Base: new(600), Hover: new(700), Active: new(800),
                Bg: new(50), BgHover: new(100), BgActive: new(200),
                Border: new(200), BorderHover: new(300),
                TextHover: new(700), Text: new(800), TextActive: new(900)),
            // PrimeUIX nora-compat button.{success,info,warn}: the 600 / 700 / 800 stops, as the primary.
            Status: new PrimeNGFamilyRule(
                Base: new(600), Hover: new(700), Active: new(800),
                Bg: new(50), BgHover: new(100), BgActive: new(200),
                Border: new(200), BorderHover: new(300),
                TextHover: new(500), Text: new(600), TextActive: new(700)),
            Error: new PrimeNGFamilyRule(
                Base: new(400), Hover: new(500), Active: new(600),
                Bg: new(50), BgHover: new(100), BgActive: new(200),
                Border: new(200), BorderHover: new(300),
                TextHover: new(500), Text: new(600), TextActive: new(700)),
            Neutral: new PrimeNGNeutralRule(
                BgBase: new(White), TextBase: new(Black),
                // One stop deeper than Aura below the secondary text, so icons and tertiary text keep the 3:1 of the
                // old profiles on white (PrimeUIX nora has a single muted level, N600).
                Text: new(900), TextSecondary: new(500), TextTertiary: new(500), TextQuaternary: new(400),
                Fill: new(300), FillSecondary: new(200), FillTertiary: new(100), FillQuaternary: new(50),
                BgContainer: new(White), BgElevated: new(White), BgLayout: new(50), BgSpotlight: new(700),
                BgBlur: new(White, 0), BgSolid: new(900), BgSolidHover: new(800), BgSolidActive: new(950),
                Border: new(500), BorderSecondary: new(Black, 0), BgMask: new(Black, 0.4),
                BgContainerDisabled: new(200), TextDisabled: new(400)),
            SelectionForeground: new(White),
            ControlOutline: null,
            FocusBorder: static token => token.ColorPrimary,
            CardBorderSecondary: null,
            ButtonPrimaryBg: null),
        Dark: new PrimeNGAppearanceRules(
            Surface: PrimeNGTailwindPalettes.Zinc,
            // PrimeUIX nora-compat highlight: a solid primary 500 background with surface.950 text. Its focus
            // background is P400 with dark text, but AtomUI draws ColorText (white) on the selected-hover background
            // (ControlItemBgActiveHover), so BgHover departs from PrimeUIX and takes P700.
            Primary: new PrimeNGFamilyRule(
                Base: new(500), Hover: new(400), Active: new(300),
                Bg: new(500), BgHover: new(700), BgActive: new(300),
                Border: new(700, 0.36), BorderHover: new(600, 0.36),
                TextHover: PrimeNGShade.Surface(950), Text: PrimeNGShade.Surface(950),
                TextActive: PrimeNGShade.Surface(950)),
            Status: new PrimeNGFamilyRule(
                Base: new(500), Hover: new(400), Active: new(300),
                Bg: new(500, 0.16), BgHover: new(500, 0.24), BgActive: new(500, 0.32),
                Border: new(700, 0.36), BorderHover: new(600, 0.36),
                TextHover: new(400), Text: new(500), TextActive: new(300)),
            Error: new PrimeNGFamilyRule(
                Base: new(400), Hover: new(300), Active: new(200),
                Bg: new(500, 0.16), BgHover: new(500, 0.24), BgActive: new(500, 0.32),
                Border: new(700, 0.36), BorderHover: new(600, 0.36),
                TextHover: new(400), Text: new(500), TextActive: new(300)),
            Neutral: new PrimeNGNeutralRule(
                BgBase: new(950), TextBase: new(White),
                Text: new(White), TextSecondary: new(400), TextTertiary: new(500), TextQuaternary: new(600),
                Fill: new(600), FillSecondary: new(700), FillTertiary: new(800), FillQuaternary: new(900),
                BgContainer: new(950), BgElevated: new(900), BgLayout: new(950), BgSpotlight: new(700),
                BgBlur: new(White, 0.04), BgSolid: new(White), BgSolidHover: new(300), BgSolidActive: new(100),
                Border: new(500), BorderSecondary: new(Black, 0), BgMask: new(Black, 0.6),
                BgContainerDisabled: new(700), TextDisabled: new(500)),
            SelectionForeground: new(950),
            ControlOutline: null,
            // The dark focus ring is the field border colour (N500), PrimeUIX form.field.focus.border.color; the
            // primary ring would sit on the solid primary selected background.
            FocusBorder: static token => token.ColorBorder,
            // PrimeUIX nora-compat overlay.*.border.color: transparent in light, surface.700 in dark.
            CardBorderSecondary: new(700),
            // The Filled primary button draws the primary colour on ColorPrimaryBg, which is the solid primary here;
            // for the button only it takes the Aura shape (the primary at 16% / 24%).
            ButtonPrimaryBg: static token => (PrimeNGPalette.WithOpacity(token.ColorPrimary, 0.16),
                                              PrimeNGPalette.WithOpacity(token.ColorPrimary, 0.24))),
        Sizes: new PrimeNGSizeLadder(
            BorderRadiusXSOffset: 0, BorderRadiusSMOffset: 0, BorderRadiusLGOffset: 0, BorderRadiusXLOffset: 0,
            ControlHeightSMOffset: -4, ControlHeightLGOffset: 8),
        Alias: new PrimeNGAliasRule(
            FocusVisualBorderLineWidths: 2,
            ControlOutlineLineWidths: 0,
            BoxShadows: BoxShadows.Parse("0 20 25 -5 #1A000000, 0 8 10 -6 #1A000000"),
            BoxShadowsSecondary: BoxShadows.Parse("0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000")),
        Controls: new PrimeNGControlRule(
            ButtonFontWeight: FontWeight.Bold,
            ButtonPaddingHorizontal: null,
            CheckIndicatorSize: 20,
            CheckedMarkSize: 14,
            // The Aura fit to 12,8 (compact 8,6); PrimeUIX nora-compat list.padding (0.125rem 0) in both densities.
            SelectOptionPadding: static global =>
                new Thickness(global.UniformlyPaddingSM, global.UniformlyPaddingSM / 2 + 2),
            SelectPopupContentPadding: static _ => new Thickness(0, 2),
            CardBodyPadding: 20,
            CardRestingShadows: BoxShadows.Parse("0 1 3 0 #1A000000, 0 1 2 -1 #1A000000"),
            DialogPadding: 20,
            ToolTipPadding: new Thickness(12, 8),
            ToolTipColor: new(White)));
}
