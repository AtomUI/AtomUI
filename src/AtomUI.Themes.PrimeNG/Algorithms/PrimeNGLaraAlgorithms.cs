using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms;

/// <summary>
/// The light PrimeNG Lara preset as a theme algorithm (<c>PrimeNG:Lara</c>): primary 500/600/700, slate surfaces,
/// a primary 200 glow around focused fields instead of a focus ring.
/// </summary>
[ThemeAlgorithm("PrimeNG", "Lara", 1, ThemeAppearanceEffect.Light)]
public sealed class PrimeNGLaraAlgorithm : PrimeNGPresetAlgorithm
{
    public PrimeNGLaraAlgorithm()
        : base(PrimeNGLaraPreset.Table, dark: false)
    {
    }
}

/// <summary>
/// The dark PrimeNG Lara preset as a theme algorithm (<c>PrimeNG:Lara.Dark</c>): primary 400/300/200 from the same
/// 500 seed, zinc surfaces, a 20% primary glow around focused fields.
/// </summary>
[ThemeAlgorithm("PrimeNG", "Lara.Dark", 1, ThemeAppearanceEffect.Dark)]
public sealed class PrimeNGLaraDarkAlgorithm : PrimeNGPresetAlgorithm
{
    public PrimeNGLaraDarkAlgorithm()
        : base(PrimeNGLaraPreset.Table, dark: true)
    {
    }
}

/// <summary>
/// The Lara rule table (spec 4.7), transcribed from the four pre-algorithm Lara XML profiles (the acceptance
/// baseline). The colour rows equal the Aura ones: the semantic colour scheme of the <c>lara</c> and
/// <c>lara-compat</c> presets of <c>@primeuix/themes</c> 3.1.1 matches <c>aura</c> there. Lara differs in the focus
/// treatment (a glow, no ring), the select overlay shadow, the control height and the control values; the README of
/// this package lists the sources row by row.
/// </summary>
internal static class PrimeNGLaraPreset
{
    private const int White = PrimeNGShade.White;
    private const int Black = PrimeNGShade.Black;

    // The colour rows equal Aura's by design: each preset is transcribed from its own XML and is expected to diverge
    // (see "Lara rules" in the README).
    internal static readonly PrimeNGPreset Table = new(
        Light: new PrimeNGAppearanceRules(
            Surface: PrimeNGTailwindPalettes.Slate,
            Primary: new PrimeNGFamilyRule(
                Base: new(500), Hover: new(600), Active: new(700),
                Bg: new(50), BgHover: new(100), BgActive: new(200),
                Border: new(200), BorderHover: new(300),
                TextHover: new(600), Text: new(700), TextActive: new(800)),
            Status: new PrimeNGFamilyRule(
                Base: new(500), Hover: new(600), Active: new(700),
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
                // Tertiary and quaternary are AtomUI's (PrimeUIX has a single muted level): N500 / N400 keep icons and
                // tertiary text at 3:1 on white and quaternary above the old profiles' 25% black (N400 / N300 would not).
                Text: new(700), TextSecondary: new(500), TextTertiary: new(500), TextQuaternary: new(400),
                Fill: new(300), FillSecondary: new(200), FillTertiary: new(100), FillQuaternary: new(50),
                BgContainer: new(White), BgElevated: new(White), BgLayout: new(50), BgSpotlight: new(700),
                BgBlur: new(White, 0), BgSolid: new(900), BgSolidHover: new(800), BgSolidActive: new(950),
                Border: new(300), BorderSecondary: new(200), BgMask: new(Black, 0.4),
                BgContainerDisabled: new(200), TextDisabled: new(400)),
            SelectionForeground: new(White),
            // PrimeUIX focus.ring.shadow, light: primary 200 (the primary border stop of this table).
            ControlOutline: static token => token.ColorPrimaryBorder,
            FocusBorder: static token => token.ColorPrimary,
            CardBorderSecondary: null,
            ButtonPrimaryBg: null),
        Dark: new PrimeNGAppearanceRules(
            Surface: PrimeNGTailwindPalettes.Zinc,
            Primary: new PrimeNGFamilyRule(
                Base: new(400), Hover: new(300), Active: new(200),
                Bg: new(400, 0.16), BgHover: new(400, 0.24), BgActive: new(400, 0.32),
                Border: new(700, 0.36), BorderHover: new(600, 0.36),
                TextHover: new(300), Text: new(White, 0.87), TextActive: new(White, 0.87)),
            Status: new PrimeNGFamilyRule(
                Base: new(400), Hover: new(300), Active: new(200),
                Bg: new(500, 0.16), BgHover: new(500, 0.24), BgActive: new(500, 0.32),
                Border: new(700, 0.36), BorderHover: new(600, 0.36),
                TextHover: new(400), Text: new(500), TextActive: new(300)),
            Error: new PrimeNGFamilyRule(
                Base: new(300), Hover: new(200), Active: new(100),
                Bg: new(500, 0.16), BgHover: new(500, 0.24), BgActive: new(500, 0.32),
                Border: new(700, 0.36), BorderHover: new(600, 0.36),
                TextHover: new(400), Text: new(500), TextActive: new(300)),
            Neutral: new PrimeNGNeutralRule(
                BgBase: new(950), TextBase: new(White),
                Text: new(White), TextSecondary: new(400), TextTertiary: new(500), TextQuaternary: new(600),
                Fill: new(600), FillSecondary: new(700), FillTertiary: new(800), FillQuaternary: new(900),
                BgContainer: new(950), BgElevated: new(900), BgLayout: new(950), BgSpotlight: new(700),
                BgBlur: new(White, 0.04), BgSolid: new(White), BgSolidHover: new(300), BgSolidActive: new(100),
                Border: new(600), BorderSecondary: new(700), BgMask: new(Black, 0.6),
                BgContainerDisabled: new(700), TextDisabled: new(500)),
            SelectionForeground: new(900),
            // PrimeUIX focus.ring.shadow, dark: primary.color mixed with 80% transparent.
            ControlOutline: static token => PrimeNGPalette.WithOpacity(token.ColorPrimary, 0.2),
            FocusBorder: static token => token.ColorPrimary,
            CardBorderSecondary: null,
            ButtonPrimaryBg: null),
        Sizes: new PrimeNGSizeLadder(
            BorderRadiusXSOffset: -4, BorderRadiusSMOffset: -2, BorderRadiusLGOffset: 2, BorderRadiusXLOffset: 6,
            ControlHeightSMOffset: -4, ControlHeightLGOffset: 8),
        Alias: new PrimeNGAliasRule(
            FocusVisualBorderLineWidths: 0,
            ControlOutlineLineWidths: 3,
            BoxShadows: BoxShadows.Parse("0 20 25 -5 #1A000000, 0 8 10 -6 #1A000000"),
            BoxShadowsSecondary: BoxShadows.Parse("0 2 12 0 #1A000000")),
        Controls: new PrimeNGControlRule(
            ButtonFontWeight: FontWeight.SemiBold,
            ButtonPaddingHorizontal: 16,
            CheckIndicatorSize: 24,
            CheckedMarkSize: 14,
            // PrimeUIX list.option.padding (16, 10) and list.padding (0, 8). The option padding follows the size
            // ladder (compact 8, 8: a fit to the baseline, not a PrimeUIX derivation); the panel padding has no
            // compact counterpart that a size token gives (the old compact XML wrote 4), see the README.
            SelectOptionPadding: static global =>
                new Thickness(global.UniformlyPadding, global.UniformlyPaddingSM / 2 + 4),
            SelectPopupContentPadding: static global => new Thickness(0, global.UniformlyPaddingXS),
            CardBodyPadding: 20,
            CardRestingShadows: BoxShadows.Parse("0 1 3 0 #1A000000, 0 1 2 -1 #1A000000"),
            DialogPadding: 20,
            ToolTipPadding: new Thickness(12, 8),
            ToolTipColor: new(White)));
}
