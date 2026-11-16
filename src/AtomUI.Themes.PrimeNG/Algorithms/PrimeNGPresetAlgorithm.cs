using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms;

/// <summary>
/// The shared implementation of the PrimeNG preset algorithms (Aura, Lara, Nora, each in a light and a dark
/// variant). Each variant is the same code over its preset's rule table.
/// </summary>
/// <remarks>
/// <para>
/// The Map stage first runs the matching Ant Design algorithm, so every Map token has a value, then writes the
/// preset's colours and, when the algorithm starts its chain, its size ladder. The <c>ColorPrimary</c>,
/// <c>ColorSuccess</c>, <c>ColorWarning</c>, <c>ColorError</c> and <c>ColorInfo</c> seeds are the 500 stops of their
/// families; <c>ColorLink</c> defaults to the primary seed. The neutral scale is a constant of the preset. A primary
/// or link seed that is the 950 stop of a neutral family is the primeng.dev "noir" primary (<see cref="PrimeNGNoir"/>).
/// </para>
/// <para>
/// A dark algorithm that follows another algorithm only recomputes the colours: the sizes of the previous Map stay,
/// as with <c>AtomUI:Dark</c>. Compact density is <c>AtomUI:Compact</c> appended to the chain.
/// </para>
/// <para>
/// The Alias stage writes the preset's focus, placeholder, disabled, contrast, highlight, padding and shadow rules; the
/// per-control values are control stages under <c>Algorithms/Controls</c>, shared by the light and dark variant.
/// </para>
/// </remarks>
public abstract class PrimeNGPresetAlgorithm : IThemeAlgorithm, IThemeAliasAlgorithm
{
    private readonly PrimeNGPreset _preset;
    private readonly bool _dark;

    private protected PrimeNGPresetAlgorithm(PrimeNGPreset preset, bool dark)
    {
        ArgumentNullException.ThrowIfNull(preset);
        _preset = preset;
        _dark   = dark;
    }

    /// <inheritdoc />
    public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
    {
        ArgumentNullException.ThrowIfNull(effectiveSeed);
        ArgumentNullException.ThrowIfNull(nextMap);

        // The Ant Design base gives every Map token a value (the preset palettes, the font, size, motion and line
        // ladders); a dark follower keeps the previous sizes, exactly like AtomUI:Dark.
        if (_dark)
        {
            new DarkThemeVariantCalculator().Evaluate(effectiveSeed, previousMap, nextMap);
        }
        else
        {
            new DefaultThemeVariantCalculator().Evaluate(effectiveSeed, previousMap, nextMap);
        }

        WriteColors(effectiveSeed, nextMap, _dark ? _preset.Dark : _preset.Light, PrimeNGNoir.For(_dark));
        if (!_dark || previousMap is null)
        {
            WriteSizes(effectiveSeed, nextMap, _preset.Sizes);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// The rules follow <see cref="ThemeAlgorithmContext.Appearance"/>, not the variant of this instance, so a
    /// mixed chain such as <c>[PrimeNG:Aura, AtomUI:Dark]</c> gets the dark Alias values. Widths and paddings are
    /// derived from Map size tokens and follow <c>AtomUI:Compact</c>.
    /// </remarks>
    public void EvaluateAlias(in ThemeAlgorithmContext context, DesignToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        // A noir primary (see PrimeNGNoir) changes the contrast colour.
        var rules   = PrimeNGNoir.Resolve(_preset, context.Appearance, token);
        var surface = rules.Surface.AsSpan();
        var alias   = _preset.Alias;

        // PrimeUIX focus.ring.color (the primary colour; Nora dark: the field border, see its table); the ring and the
        // field glow widths come from the preset (Aura: a one-line ring, no glow; Lara: no ring, a three-line glow;
        // Nora: a two-line ring, no glow).
        token.ColorFocusBorder           = rules.FocusBorder(token);
        token.FocusVisualBorderThickness = new Thickness(token.LineWidth * alias.FocusVisualBorderLineWidths);
        token.ControlOutlineWidth        = token.LineWidth * alias.ControlOutlineLineWidths;
        if (rules.ControlOutline is { } controlOutline)
        {
            // PrimeUIX focus.ring.shadow of a preset that glows focused fields; otherwise the Ant formula stays.
            token.ColorControlOutline = controlOutline(token);
        }

        // PrimeUIX form.field.placeholder.color and the muted text both use text.muted.color.
        token.ColorTextPlaceholder     = token.ColorTextSecondary;
        token.ColorTextDescription     = token.ColorTextSecondary;
        token.ColorBgContainerDisabled = rules.Neutral.BgContainerDisabled.Resolve(surface);
        // PrimeUIX form.field.disabled.color is the muted-text stop and relies on a component opacity AtomUI does not
        // apply; one stop further from the text (lighter in light, darker in dark) keeps disabled text distinct
        // from secondary text and readable on the disabled background.
        token.ColorTextDisabled        = rules.Neutral.TextDisabled.Resolve(surface);
        token.ColorTextLightSolid      = rules.SelectionForeground.Resolve(surface);
        // PrimeUIX highlight.background / focusBackground: the selected item's background. Without a row the Ant
        // formula (ColorPrimaryBg / BgHover) stays, the pale highlight of Aura and Lara.
        if (rules.Highlight is { } highlight)
        {
            (token.ControlItemBgActive, token.ControlItemBgActiveHover) = highlight(token);
        }
        // PrimeUIX highlight.color, the selected item's text, once for every control: the selected-text tokens of the
        // list, tree, data-grid and date / time controls default to ControlItemColorActive.
        token.ControlItemColorActive = rules.HighlightColor(token);

        // @primeuix/themes 3.1.1 form.field.padding.x is 0.625rem, sm 0.5rem, lg 0.75rem (10 / 8 / 12) in all three
        // presets. They follow the size ladder (SizeSM 12, compact 8), so AtomUI:Compact narrows them too. The Ant
        // formula pads inputs by UniformlyPaddingSM - LineWidth (11). The vertical padding makes a field exactly its
        // control height, as in PrimeNG, instead of the Ant rounding (35 rendered 36, SM 28 rendered 26).
        var fieldPaddingX = token.SizeSM - 2;
        // The small field also insets its text by InputContentMargin.
        var chrome = 2 * token.LineWidth;
        token.InputPadding   = FieldPadding(fieldPaddingX, token.ControlHeight - token.FontHeight - chrome);
        token.InputPaddingSM = FieldPadding(token.SizeSM - 4, token.ControlHeightSM - token.FontHeightSM - chrome -
                                                              token.InputContentMargin.Top - token.InputContentMargin.Bottom);
        token.InputPaddingLG = FieldPadding(token.SizeSM, token.ControlHeightLG - token.FontHeightLG - chrome);
        // Content inset of item rows and other content: the field padding inside a border (the Button stage sets the
        // Button's own label paddings).
        token.PaddingContentHorizontal = fieldPaddingX + token.LineWidth;

        token.BoxShadows          = alias.BoxShadows;
        token.BoxShadowsSecondary = alias.BoxShadowsSecondary;
    }

    /// <summary>
    /// A field padding whose vertical part is the control height left over around the text line, border and content
    /// margin. An odd remainder goes to the bottom, so both sides are whole pixels and the height is exact at any scale.
    /// </summary>
    private static Thickness FieldPadding(double horizontal, double vertical)
    {
        vertical = Math.Max(0, vertical);
        var top  = Math.Floor(vertical / 2);
        return new Thickness(horizontal, top, horizontal, vertical - top);
    }

    private static void WriteColors(DesignToken seed, DesignToken map, PrimeNGAppearanceRules rules, PrimeNGNoirRule noir)
    {
        // A noir seed (primeng.dev's default primary) takes the surface scale and the noir rows; the link follows its
        // own seed, which defaults to the primary one.
        var linkSeed = seed.ColorLink ?? seed.ColorPrimary;
        var linkRule = rules.Link;
        Color[] primary;
        Color[] link;
        if (PrimeNGNoir.IsSeed(seed.ColorPrimary))
        {
            primary = PrimeNGNoir.Palette(rules.Surface, seed.ColorPrimary);
            rules   = PrimeNGNoir.Apply(rules, noir);
        }
        else
        {
            primary = PrimeNGPalette.Resolve(seed.ColorPrimary);
        }
        if (PrimeNGNoir.IsSeed(linkSeed))
        {
            link     = PrimeNGNoir.Palette(rules.Surface, linkSeed);
            linkRule = noir.Link;
        }
        else
        {
            link = PrimeNGPalette.Resolve(linkSeed);
        }

        var surface = rules.Surface.AsSpan();
        var success = PrimeNGPalette.Resolve(seed.ColorSuccess);
        var warning = PrimeNGPalette.Resolve(seed.ColorWarning);
        var error   = PrimeNGPalette.Resolve(seed.ColorError);
        var info    = PrimeNGPalette.Resolve(seed.ColorInfo);

        var p = rules.Primary;
        map.ColorPrimary            = p.Base.Resolve(primary, surface);
        map.ColorPrimaryHover       = p.Hover.Resolve(primary, surface);
        map.ColorPrimaryActive      = p.Active.Resolve(primary, surface);
        map.ColorPrimaryBg          = p.Bg.Resolve(primary, surface);
        map.ColorPrimaryBgHover     = p.BgHover.Resolve(primary, surface);
        map.ColorPrimaryBorder      = p.Border.Resolve(primary, surface);
        map.ColorPrimaryBorderHover = p.BorderHover.Resolve(primary, surface);
        map.ColorPrimaryTextHover   = p.TextHover.Resolve(primary, surface);
        map.ColorPrimaryText        = p.Text.Resolve(primary, surface);
        map.ColorPrimaryTextActive  = p.TextActive.Resolve(primary, surface);

        var l = linkRule;
        map.ColorLink       = l.Base.Resolve(link, surface);
        map.ColorLinkHover  = l.Hover.Resolve(link, surface);
        map.ColorLinkActive = l.Active.Resolve(link, surface);

        var s = rules.Status;
        map.ColorSuccess            = s.Base.Resolve(success, surface);
        map.ColorSuccessHover       = s.Hover.Resolve(success, surface);
        map.ColorSuccessActive      = s.Active.Resolve(success, surface);
        map.ColorSuccessBg          = s.Bg.Resolve(success, surface);
        map.ColorSuccessBgHover     = s.BgHover.Resolve(success, surface);
        map.ColorSuccessBorder      = s.Border.Resolve(success, surface);
        map.ColorSuccessBorderHover = s.BorderHover.Resolve(success, surface);
        map.ColorSuccessTextHover   = s.TextHover.Resolve(success, surface);
        map.ColorSuccessText        = s.Text.Resolve(success, surface);
        map.ColorSuccessTextActive  = s.TextActive.Resolve(success, surface);

        map.ColorWarning            = s.Base.Resolve(warning, surface);
        map.ColorWarningHover       = s.Hover.Resolve(warning, surface);
        map.ColorWarningActive      = s.Active.Resolve(warning, surface);
        map.ColorWarningBg          = s.Bg.Resolve(warning, surface);
        map.ColorWarningBgHover     = s.BgHover.Resolve(warning, surface);
        map.ColorWarningBorder      = s.Border.Resolve(warning, surface);
        map.ColorWarningBorderHover = s.BorderHover.Resolve(warning, surface);
        map.ColorWarningTextHover   = s.TextHover.Resolve(warning, surface);
        map.ColorWarningText        = s.Text.Resolve(warning, surface);
        map.ColorWarningTextActive  = s.TextActive.Resolve(warning, surface);

        map.ColorInfo            = s.Base.Resolve(info, surface);
        map.ColorInfoHover       = s.Hover.Resolve(info, surface);
        map.ColorInfoActive      = s.Active.Resolve(info, surface);
        map.ColorInfoBg          = s.Bg.Resolve(info, surface);
        map.ColorInfoBgHover     = s.BgHover.Resolve(info, surface);
        map.ColorInfoBorder      = s.Border.Resolve(info, surface);
        map.ColorInfoBorderHover = s.BorderHover.Resolve(info, surface);
        map.ColorInfoTextHover   = s.TextHover.Resolve(info, surface);
        map.ColorInfoText        = s.Text.Resolve(info, surface);
        map.ColorInfoTextActive  = s.TextActive.Resolve(info, surface);

        var e = rules.Error;
        map.ColorError            = e.Base.Resolve(error, surface);
        map.ColorErrorHover       = e.Hover.Resolve(error, surface);
        map.ColorErrorActive      = e.Active.Resolve(error, surface);
        map.ColorErrorBg          = e.Bg.Resolve(error, surface);
        map.ColorErrorBgHover     = e.BgHover.Resolve(error, surface);
        map.ColorErrorBgActive    = rules.ErrorBgActive.Resolve(error, surface);
        map.ColorErrorBorder      = e.Border.Resolve(error, surface);
        map.ColorErrorBorderHover = e.BorderHover.Resolve(error, surface);
        map.ColorErrorTextHover   = e.TextHover.Resolve(error, surface);
        map.ColorErrorText        = e.Text.Resolve(error, surface);
        map.ColorErrorTextActive  = e.TextActive.Resolve(error, surface);

        var n = rules.Neutral;
        map.ColorBgBase   = seed.ColorBgBase ?? n.BgBase.Resolve(surface);
        map.ColorTextBase = seed.ColorTextBase ?? n.TextBase.Resolve(surface);

        map.ColorText           = n.Text.Resolve(surface);
        map.ColorTextSecondary  = n.TextSecondary.Resolve(surface);
        map.ColorTextTertiary   = n.TextTertiary.Resolve(surface);
        map.ColorTextQuaternary = n.TextQuaternary.Resolve(surface);

        map.ColorFill           = n.Fill.Resolve(surface);
        map.ColorFillSecondary  = n.FillSecondary.Resolve(surface);
        map.ColorFillTertiary   = n.FillTertiary.Resolve(surface);
        map.ColorFillQuaternary = n.FillQuaternary.Resolve(surface);

        map.ColorBgContainer   = n.BgContainer.Resolve(surface);
        map.ColorBgElevated    = n.BgElevated.Resolve(surface);
        map.ColorBgLayout      = n.BgLayout.Resolve(surface);
        map.ColorBgSpotlight   = n.BgSpotlight.Resolve(surface);
        map.ColorBgBlur        = n.BgBlur.Resolve(surface);
        map.ColorBgSolid       = n.BgSolid.Resolve(surface);
        map.ColorBgSolidHover  = n.BgSolidHover.Resolve(surface);
        map.ColorBgSolidActive = n.BgSolidActive.Resolve(surface);

        map.ColorBorder          = n.Border.Resolve(surface);
        map.ColorBorderSecondary = n.BorderSecondary.Resolve(surface);
        map.ColorBgMask          = n.BgMask.Resolve(surface);

        map.ColorWhite          = Colors.White;
        map.ColorBlack          = Colors.Black;
        map.SelectionBackground = map.ColorPrimary;
        map.SelectionForeground = rules.SelectionForeground.Resolve(surface);
    }

    private static void WriteSizes(DesignToken seed, DesignToken map, PrimeNGSizeLadder ladder)
    {
        var radius = seed.BorderRadius.TopLeft;
        map.BorderRadiusXS = new CornerRadius(Math.Max(0, radius + ladder.BorderRadiusXSOffset));
        map.BorderRadiusSM = new CornerRadius(Math.Max(0, radius + ladder.BorderRadiusSMOffset));
        map.BorderRadiusLG = new CornerRadius(Math.Max(0, radius + ladder.BorderRadiusLGOffset));

        var height = seed.ControlHeight;
        map.ControlHeightSM = height + ladder.ControlHeightSMOffset;
        map.ControlHeightLG = height + ladder.ControlHeightLGOffset;
    }
}
