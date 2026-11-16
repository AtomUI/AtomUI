using System.Collections.Immutable;
using System.Diagnostics;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms;

/// <summary>
/// One shade of a colour family: a palette stop, optionally made translucent. <see cref="White"/> and
/// <see cref="Black"/> stand for pure white (PrimeUIX <c>surface.0</c>) and pure black whatever the family.
/// </summary>
/// <param name="Stop">A palette stop (50 to 950), <see cref="White"/> or <see cref="Black"/>.</param>
/// <param name="Opacity">The opacity applied on top of the stop's own alpha, from 0 to 1.</param>
/// <param name="FromSurface">
/// In a family rule, takes the stop from the neutral (surface) scale instead of the family, for a family token that
/// PrimeUIX maps to a surface colour (Nora's dark highlight text, <c>surface.950</c>). Has no effect where the palette
/// is the surface scale already.
/// </param>
internal readonly record struct PrimeNGShade(int Stop, double Opacity = 1.0, bool FromSurface = false)
{
    internal const int White = 0;
    internal const int Black = 1000;

    /// <summary>
    /// A stop of the neutral (surface) scale, for use in a family rule.
    /// </summary>
    internal static PrimeNGShade Surface(int stop) => new(stop, 1.0, FromSurface: true);

    /// <summary>
    /// Resolves a family rule's shade: from the family palette, or from the surface scale when
    /// <see cref="FromSurface"/> is set.
    /// </summary>
    internal Color Resolve(ReadOnlySpan<Color> family, ReadOnlySpan<Color> surface) =>
        ResolveIn(FromSurface ? surface : family);

    /// <summary>
    /// Resolves a shade against a single palette that is already the right one (the neutral rows, the control
    /// stages' surface values). A <see cref="FromSurface"/> shade belongs in a family rule and must go through
    /// <see cref="Resolve(ReadOnlySpan{Color}, ReadOnlySpan{Color})"/>; passing one here is a programming error.
    /// </summary>
    internal Color Resolve(ReadOnlySpan<Color> palette)
    {
        Debug.Assert(!FromSurface, "A surface shade of a family rule must be resolved with the family and the surface.");
        return ResolveIn(palette);
    }

    private Color ResolveIn(ReadOnlySpan<Color> palette)
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
/// Only the error family has an active background token; it is <see cref="PrimeNGAppearanceRules.ErrorBgActive"/>.
/// </summary>
internal sealed record PrimeNGFamilyRule(
    PrimeNGShade Base,
    PrimeNGShade Hover,
    PrimeNGShade Active,
    PrimeNGShade Bg,
    PrimeNGShade BgHover,
    PrimeNGShade Border,
    PrimeNGShade BorderHover,
    PrimeNGShade TextHover,
    PrimeNGShade Text,
    PrimeNGShade TextActive);

/// <summary>
/// Which shades of the link seed's family (the primary unless the theme sets <c>ColorLink</c>) the link colours take.
/// They are separate from the primary row because link text must stay readable on the container: in light the links
/// take a deeper stop than the primary fill.
/// </summary>
internal sealed record PrimeNGLinkRule(
    PrimeNGShade Base,
    PrimeNGShade Hover,
    PrimeNGShade Active);

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
/// The colour rules of one appearance. <see cref="Primary"/> also drives the selection background;
/// <see cref="Status"/> drives the success, warning and info families.
/// </summary>
/// <param name="Surface">The neutral scale, a constant of the preset (spec 4.7).</param>
/// <param name="Link">The link colours' stops.</param>
/// <param name="ErrorBgActive">The error family's active background (<c>ColorErrorBgActive</c>).</param>
/// <param name="SelectionForeground">
/// Text on the selection background and on solid primary fills (the PrimeUIX primary contrast colour); the Alias
/// stage also writes it to <c>ColorTextLightSolid</c>.
/// </param>
/// <param name="ControlOutline">
/// The colour of the glow around a focused field (<c>ColorControlOutline</c>), computed in the Alias stage from the
/// final Map tokens; <see langword="null"/> keeps the Ant Design formula (a preset without a field glow).
/// </param>
/// <param name="FocusBorder">
/// The focus ring colour (<c>ColorFocusBorder</c>), computed in the Alias stage from the final Map tokens (Aura and
/// Lara: the primary colour in both appearances).
/// </param>
/// <param name="CardBorderSecondary">
/// The card's own <c>ColorBorderSecondary</c>, a stop of the surface scale; <see langword="null"/> keeps the global
/// value.
/// </param>
/// <param name="Highlight">
/// PrimeUIX <c>highlight.background</c> and <c>highlight.focusBackground</c>, the selected item's background and its
/// hover, written by the Alias stage to <c>ControlItemBgActive</c> / <c>ControlItemBgActiveHover</c> from the final Map
/// tokens; <see langword="null"/> keeps the Ant Design formula (<c>ColorPrimaryBg</c> / <c>ColorPrimaryBgHover</c>),
/// which is the pale highlight of Aura and Lara. <c>ColorPrimaryBg</c> itself stays the pale primary background
/// (filled buttons, tags, step panels).
/// </param>
/// <param name="HighlightColor">
/// PrimeUIX <c>highlight.color</c>, the selected item's text, computed from the final tokens; the Alias stage writes it
/// to the global <c>ControlItemColorActive</c>, which the selected-text tokens of the list, tree, data-grid and date /
/// time controls default to.
/// PrimeUIX <c>highlight.focusColor</c> equals it in every preset except Aura / Lara light (P800); AtomUI has no
/// separate selected-hover text slot.
/// </param>
/// <param name="SwitchHandleChecked">
/// PrimeUIX <c>toggleswitch.handle.checked.background</c>, a stop of the surface scale (the ToggleSwitch's own
/// <c>HandleCheckedBg</c>).
/// </param>
/// <param name="OverlayBorder">
/// PrimeUIX <c>overlay.select.border.color</c> and <c>overlay.popover.border.color</c> (equal in every preset), a stop
/// of the surface scale: the border of arrow flyouts and the outline ring of select panels and menus.
/// </param>
internal sealed record PrimeNGAppearanceRules(
    ImmutableArray<Color> Surface,
    PrimeNGFamilyRule Primary,
    PrimeNGLinkRule Link,
    PrimeNGFamilyRule Status,
    PrimeNGFamilyRule Error,
    PrimeNGShade ErrorBgActive,
    PrimeNGNeutralRule Neutral,
    PrimeNGShade SelectionForeground,
    Func<DesignToken, Color>? ControlOutline,
    Func<DesignToken, Color> FocusBorder,
    PrimeNGShade? CardBorderSecondary,
    Func<DesignToken, (Color Background, Color FocusBackground)>? Highlight,
    Func<DesignToken, Color> HighlightColor,
    PrimeNGShade SwitchHandleChecked,
    PrimeNGShade OverlayBorder);

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
/// <param name="BoxShadowsSecondary">
/// Select panels and menus (PrimeUIX <c>overlay.select.shadow</c> and <c>overlay.navigation.shadow</c>, equal in every
/// preset); arrow flyouts take <see cref="PrimeNGControlRule.PopoverShadows"/>.
/// </param>
internal sealed record PrimeNGAliasRule(
    double FocusVisualBorderLineWidths,
    double ControlOutlineLineWidths,
    BoxShadows BoxShadows,
    BoxShadows BoxShadowsSecondary);

/// <summary>
/// The fixed control stage values of a preset: the standard presets of <c>@primeuix/themes</c> 3.1.1 at a 16px root
/// (PrimeNG Styled Mode, "Base Font Size"), which is what PrimeNG 22 (primeng.dev) renders. The <c>*-compat</c>
/// presets are calibrated for a legacy 14px root; AtomUI maps 1rem to 16px, so their values would be 16/14 too large. Fixed sizes do not follow <c>AtomUI:Compact</c>; values that follow from other
/// tokens are delegates evaluated in the control stages.
/// </summary>
/// <param name="ButtonPaddingHorizontal">
/// The button's horizontal label padding (PrimeUIX <c>button.padding.x</c>) where it differs from the field padding,
/// from the button's effective global tokens; <see langword="null"/> keeps the field padding.
/// </param>
/// <param name="DialogHeaderFontSize">The dialog title size (PrimeUIX <c>dialog.title.font.size</c>).</param>
/// <param name="SelectOptionPadding">The Select option padding, from the control's effective global tokens.</param>
/// <param name="SelectPopupContentPadding">The Select panel padding, from the control's effective global tokens.</param>
/// <param name="PopoverPadding">The arrow flyout's content padding (PrimeUIX <c>overlay.popover.padding</c>).</param>
/// <param name="PopoverGutter">
/// The distance between the anchor and an arrow flyout's box, which the arrow spans (PrimeUIX <c>popover.gutter</c>;
/// the arrow is twice as wide as it is long).
/// </param>
/// <param name="PopoverShadows">The arrow flyout's shadow (PrimeUIX <c>overlay.popover.shadow</c>).</param>
/// <param name="OverlayAnchorGutter">
/// The distance between the anchor and a select panel, menu or other overlay without an arrow (PrimeUIX
/// <c>anchor.gutter</c>).
/// </param>
internal sealed record PrimeNGControlRule(
    FontWeight ButtonFontWeight,
    Func<DesignToken, double>? ButtonPaddingHorizontal,
    double CheckIndicatorSize,
    double CheckedMarkSize,
    Func<DesignToken, Thickness> SelectOptionPadding,
    Func<DesignToken, Thickness> SelectPopupContentPadding,
    double CardBodyPadding,
    BoxShadows CardRestingShadows,
    double DialogPadding,
    double DialogHeaderFontSize,
    Thickness ToolTipPadding,
    PrimeNGShade ToolTipColor,
    double PopoverPadding,
    double PopoverGutter,
    BoxShadows PopoverShadows,
    double OverlayAnchorGutter);

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
