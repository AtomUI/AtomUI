using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG.Presets;

/// <summary>
/// The Aura rule table (spec 4.7). Stops are checked against the Aura preset of <c>@primeuix/themes</c> 3.1.1;
/// the README of this package lists which rows are verified there and which are AtomUI proposals.
/// </summary>
internal static class PrimeNGAuraPreset
{
    private const int White = PrimeNGShade.White;
    private const int Black = PrimeNGShade.Black;

    // The colour rows equal Lara's by design: each preset is transcribed from its own XML and is expected to diverge
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
            ControlOutline: null,
            FocusBorder: static token => token.ColorPrimary,
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
            ControlOutline: null,
            FocusBorder: static token => token.ColorPrimary,
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
            FocusVisualBorderLineWidths: 1,
            ControlOutlineLineWidths: 0,
            // PrimeUIX focus.ring.width 1px.
            FocusRingLineWidths: 1,
            // PrimeUIX focus.ring.offset 2px.
            FocusRingOffsetLineWidths: 2,
            BoxShadows: BoxShadows.Parse("0 20 25 -5 #1A000000, 0 8 10 -6 #1A000000"),
            BoxShadowsSecondary: BoxShadows.Parse("0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000")),
        Controls: new PrimeNGControlRule(
            ButtonFontWeight: FontWeight.Medium,
            ButtonPaddingHorizontal: null,
            // PrimeUIX checkbox.width 1.125rem, icon.size 0.75rem.
            CheckIndicatorSize: 18,
            CheckedMarkSize: 12,
            // PrimeUIX list.option.padding 0.25rem 0.625rem (10, 4) and list.padding 0.25rem, on the size ladder.
            SelectOptionPadding: static global =>
                new Thickness(global.UniformlyPaddingSM - 2, global.UniformlyPaddingXXS),
            SelectPopupContentPadding: static global => new Thickness(global.UniformlyPaddingXXS),
            // PrimeUIX card.body.padding and overlay.modal.padding 1.125rem; dialog.title.font.size 1.125rem.
            CardBodyPadding: 18,
            // PrimeUIX aura card.border.radius border.radius.xl (12).
            CardBorderRadiusOffset: 6,
            // PrimeUIX aura card.shadow.
            CardRestingShadows: BoxShadows.Parse("0 1 3 0 #1A000000, 0 1 2 -1 #1A000000"),
            DialogPadding: 18,
            DialogHeaderFontSize: 18,
            // PrimeUIX tooltip.padding 0.375rem 0.625rem.
            ToolTipPadding: new Thickness(10, 6),
            ToolTipColor: new(White),
            // PrimeUIX overlay.popover.padding 0.625rem, popover.gutter 10px, overlay.popover.shadow.
            PopoverPadding: 10,
            PopoverGutter: 10,
            PopoverShadows: BoxShadows.Parse("0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000")));
}
