using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG.Presets;

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
                Bg: new(50), BgHover: new(100),
                Border: new(200), BorderHover: new(300),
                TextHover: new(600), Text: new(700), TextActive: new(800)),
            // Links take deeper stops than the primary fill so link text stays readable on white; hover and active go deeper.
            Link: new PrimeNGLinkRule(Base: new(700), Hover: new(800), Active: new(900)),
            Status: new PrimeNGFamilyRule(
                Base: new(500), Hover: new(600), Active: new(700),
                Bg: new(50), BgHover: new(100),
                Border: new(200), BorderHover: new(300),
                TextHover: new(500), Text: new(600), TextActive: new(700)),
            Error: new PrimeNGFamilyRule(
                Base: new(400), Hover: new(500), Active: new(600),
                Bg: new(50), BgHover: new(100),
                Border: new(200), BorderHover: new(300),
                TextHover: new(500), Text: new(600), TextActive: new(700)),
            ErrorBgActive: new(200),
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
            // PrimeUIX lara focus.ring.color is transparent; the visible ring is focus.ring.shadow (primary.200).
            FocusBorder: static token => token.ColorPrimaryBorder,
            CardBorderSecondary: null,
            // PrimeUIX highlight: the pale P50 / P100 (ColorPrimaryBg / BgHover) with primary.700 text (ColorPrimaryText).
            Highlight: null,
            HighlightColor: static token => token.ColorPrimaryText,
            // PrimeUIX toggleswitch (3.1.1): background / hoverBackground surface.300 / 400, handle.background /
            // hoverBackground / checkedBackground surface.0, checkedHoverBackground primary.hover.color.
            Switch: new PrimeNGSwitchRule(Track: new(300), TrackHover: new(400), Handle: new(White),
                HandleHover: new(White), HandleChecked: new(White), TrackBorder: null, CheckedHoverKeepsPrimary: false),
            // PrimeUIX overlay.{select,popover}.border.color, light: surface.200.
            OverlayBorder: new(200),
            // PrimeUIX progressbar.background (content.border.color).
            ProgressTrack: new(200),
            // PrimeUIX navigation.item.active.background / focusBackground (surface.100) with activeColor / focusColor
            // (text.hover.color).
            NavigationItemActive: new(100),
            NavigationItemActiveColor: new(800),
            // PrimeUIX menu / tieredmenu / contextmenu / menubar submenu border: content.border.color (surface.200).
            MenuBorder: new(200)),
        Dark: new PrimeNGAppearanceRules(
            Surface: PrimeNGTailwindPalettes.Zinc,
            Primary: new PrimeNGFamilyRule(
                Base: new(400), Hover: new(300), Active: new(200),
                Bg: new(400, 0.16), BgHover: new(400, 0.24),
                Border: new(700, 0.36), BorderHover: new(600, 0.36),
                TextHover: new(300), Text: new(White, 0.87), TextActive: new(White, 0.87)),
            Link: new PrimeNGLinkRule(Base: new(400), Hover: new(300), Active: new(200)),
            Status: new PrimeNGFamilyRule(
                Base: new(400), Hover: new(300), Active: new(200),
                Bg: new(500, 0.16), BgHover: new(500, 0.24),
                Border: new(700, 0.36), BorderHover: new(600, 0.36),
                TextHover: new(400), Text: new(500), TextActive: new(300)),
            Error: new PrimeNGFamilyRule(
                Base: new(300), Hover: new(200), Active: new(100),
                Bg: new(500, 0.16), BgHover: new(500, 0.24),
                Border: new(700, 0.36), BorderHover: new(600, 0.36),
                TextHover: new(400), Text: new(500), TextActive: new(300)),
            ErrorBgActive: new(500, 0.32),
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
            // PrimeUIX lara focus.ring.shadow, dark: the primary colour at 20%.
            FocusBorder: static token => PrimeNGPalette.WithOpacity(token.ColorPrimary, 0.2),
            CardBorderSecondary: null,
            // PrimeUIX highlight: P400 at 16% / 24% (ColorPrimaryBg / BgHover) with white at 87% (ColorPrimaryText).
            Highlight: null,
            HighlightColor: static token => token.ColorPrimaryText,
            // PrimeUIX toggleswitch (3.1.1), dark: background / hoverBackground surface.700 / 600, handle.background /
            // hoverBackground surface.400 / 300, handle.checkedBackground surface.900.
            Switch: new PrimeNGSwitchRule(Track: new(700), TrackHover: new(600), Handle: new(400),
                HandleHover: new(300), HandleChecked: new(900), TrackBorder: null, CheckedHoverKeepsPrimary: false),
            // PrimeUIX overlay.{select,popover}.border.color, dark: surface.700.
            OverlayBorder: new(700),
            // PrimeUIX progressbar.background (content.border.color).
            ProgressTrack: new(700),
            // PrimeUIX navigation.item.active.background / focusBackground (surface.800) with activeColor / focusColor
            // (text.hover.color).
            NavigationItemActive: new(800),
            NavigationItemActiveColor: new(White),
            // PrimeUIX menu / tieredmenu / contextmenu / menubar submenu border: content.border.color (surface.700).
            MenuBorder: new(700)),
        Sizes: new PrimeNGSizeLadder(
            BorderRadiusXSOffset: -4, BorderRadiusSMOffset: -2, BorderRadiusLGOffset: 2, BorderRadiusXLOffset: 6,
            ControlHeightSMOffset: -7, ControlHeightLGOffset: 7),
        Alias: new PrimeNGAliasRule(
            FocusVisualBorderLineWidths: 0,
            ControlOutlineLineWidths: 3,
            // PrimeUIX focus.ring.shadow 0 0 0 0.2rem: a 3.2px glow at offset 0 (focus.ring.width 0).
            FocusRingLineWidths: 3.2,
            // The Lara glow sits on the element edge (box-shadow, no outline-offset).
            FocusRingOffsetLineWidths: 0,
            // PrimeUIX lara overlay.modal.shadow 0 1px 3px rgba(0,0,0,0.3).
            BoxShadows: BoxShadows.Parse("0 1 3 0 #4D000000"),
            BoxShadowsSecondary: BoxShadows.Parse("0 2 12 0 #1A000000")),
        Controls: new PrimeNGControlRule(
            ButtonFontWeight: FontWeight.SemiBold,
            // PrimeUIX button.padding.x 0.875rem (14), wider than the 0.625rem field padding; on the size ladder.
            ButtonPaddingHorizontal: static global => global.SizeSM + 2,
            // PrimeUIX checkbox.width 1.25rem, icon.size 0.875rem.
            CheckIndicatorSize: 20,
            CheckedMarkSize: 14,
            // PrimeUIX list.option.padding 0.5rem 0.875rem (14, 8) and list.padding 0.5rem 0 (0, 8), on the size
            // ladder; the panel padding has no compact counterpart that a size token gives (the old compact XML
            // wrote 4), see the README.
            SelectOptionPadding: static global =>
                new Thickness(global.UniformlyPaddingSM + 2, global.UniformlyPaddingXS),
            SelectPopupContentPadding: static global => new Thickness(0, global.UniformlyPaddingXS),
            // PrimeUIX card.body.padding and overlay.modal.padding 1.25rem; dialog.title.font.size 1.125rem.
            CardBodyPadding: 20,
            // PrimeUIX lara card.border.radius border.radius.lg (8).
            CardBorderRadiusOffset: 2,
            // PrimeUIX lara card.shadow.
            CardRestingShadows: BoxShadows.Parse("0 2 4 0 #13000000"),
            DialogPadding: 20,
            DialogHeaderFontSize: 18,
            // PrimeUIX tooltip.padding 0.5rem 0.625rem.
            ToolTipPadding: new Thickness(10, 8),
            ToolTipColor: new(White),
            // PrimeUIX overlay.popover.padding 0.875rem, popover.gutter 10px, overlay.popover.shadow (lighter than the
            // select / navigation shadow).
            PopoverPadding: 14,
            PopoverGutter: 10,
            PopoverShadows: BoxShadows.Parse("0 1 3 0 #1A000000")));
}
