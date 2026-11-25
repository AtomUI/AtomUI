using AtomUI.Media;
using AtomUI.DesignLanguages.PrimeNG.Presets;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG.TokenEvaluation;

/// <summary>
/// The PrimeNG alias tokens: the Ant Design alias formulas (copied from components/theme/util/alias.ts) refined by the
/// preset's alias rules. The three tokens the formulas write and read again (focus ring width, field glow width and
/// colour) take the preset's values in place, so the input shadows follow them; the preset's other alias values are
/// written after the formulas. This gives the result of the previous staged evaluation (formulas, preset step, formulas
/// again with the preset's values kept, preset step again). Explicit Map and Alias values are frozen before this runs.
/// </summary>
internal static class AliasTokenRules
{
    internal static void Apply(in DesignVariant variant, PrimeNGPreset preset, PrimeNGToken language, DesignToken token)
    {
        // A noir primary (see PrimeNGNoir) changes the contrast colour.
        var rules = PrimeNGPresets.Rules(preset, variant.Appearance, language, token);
        var alias = preset.Alias;
        ApplyFormulas(rules, alias, token);
        ApplyPresetRules(variant.Appearance, rules, alias, token);
    }

    private static void ApplyFormulas(PrimeNGAppearanceRules rules, PrimeNGAliasRule alias, DesignToken token)
    {
        var screenXS  = 480;
        var screenSM  = 576;
        var screenMD  = 768;
        var screenLG  = 992;
        var screenXL  = 1200;
        var screenXXL = 1600;
        var screenXXXL = 1920;

        // setup alias token
        // ============== Background ============== //
        token.ColorFillContent         = token.ColorFillSecondary;
        token.ColorFillContentHover    = token.ColorFill;
        token.ColorFillAlter           = token.ColorFillQuaternary;
        token.ColorBgContainerDisabled = token.ColorFillTertiary;

        // ============== Split ============== //
        token.ColorBorderBg = token.ColorBgContainer;
        token.ColorSplit    = ColorUtils.CalculateAlphaColor(token.ColorBorderSecondary, token.ColorBgContainer);

        // ============== Text ============== //
        token.ColorTextPlaceholder = token.ColorTextQuaternary;
        token.ColorTextDisabled    = token.ColorTextQuaternary;
        token.ColorTextHeading     = token.ColorText;
        token.ColorTextLabel       = token.ColorTextSecondary;
        token.ColorTextDescription = token.ColorTextTertiary;
        token.ColorTextLightSolid  = token.ColorWhite;
        token.ColorHighlight       = token.ColorError;
        token.ColorBgTextHover     = token.ColorFillSecondary;
        token.ColorBgTextActive    = token.ColorFill;

        token.ColorIcon      = token.ColorTextTertiary;
        token.ColorIconHover = token.ColorText;

        token.ColorErrorOutline = ColorUtils.CalculateAlphaColor(token.ColorErrorBg, token.ColorBgContainer);
        token.ColorWarningOutline = ColorUtils.CalculateAlphaColor(token.ColorWarningBg, token.ColorBgContainer);

        // Font
        token.FontSizeIcon = token.FontSizeSM;

        // icon
        token.IconSizeXS = (int)token.FontSizeSM - 2;
        token.IconSizeSM = (int)token.FontSizeSM;
        token.IconSize   = (int)token.FontSize;
        token.IconSizeLG = (int)token.FontSizeLG;

        // Line
        // The PrimeNG focus ring width (the preset rules below write it too; the formulas that follow read it).
        token.LineWidthFocus = token.LineWidth * alias.FocusRingLineWidths;
        token.FocusVisualBorderThickness = new Thickness(token.LineWidthFocus);
        token.ColorFocusBorder           = token.ColorPrimaryBorder;
        token.WaveAnimationRange         = token.LineWidth * 6;
        token.WaveStartOpacity           = 0.4;

        // Control
        // The PrimeNG field glow width, read by the input shadows below.
        token.ControlOutlineWidth = token.LineWidth * alias.ControlOutlineLineWidths;

        // Checkbox size and expand icon size
        token.ControlInteractiveSize = token.ControlHeight / 2;

        token.ControlItemBgHover          = token.ColorFillTertiary;
        token.ControlItemBgActive         = token.ColorPrimaryBg;
        token.ControlItemBgActiveHover    = token.ColorPrimaryBgHover;
        token.ControlItemColorActive      = token.ColorText;
        token.ControlItemBgActiveDisabled = token.ColorFill;
        // PrimeUIX focus.ring.shadow of a preset that glows focused fields; otherwise the Ant Design formula. The input
        // shadows below read it.
        token.ColorControlOutline = rules.ControlOutline is { } controlOutline
            ? controlOutline(token)
            : ColorUtils.CalculateAlphaColor(token.ColorPrimaryBg, token.ColorBgContainer);

        token.FontWeightStrong    = FontWeight.SemiBold;
        token.OpacityLoading      = 0.65;
        token.LinkDecoration      = null;
        token.LinkHoverDecoration = null;
        token.LinkFocusDecoration = null;

        token.ControlPaddingHorizontal = 12;
        token.ControlPaddingHorizontalSM = 8;

        token.UniformlyPaddingXXS = token.SizeXXS;
        token.UniformlyPaddingXS  = token.SizeXS;
        token.UniformlyPaddingSM = token.SizeSM;
        token.UniformlyPadding    = token.Size;
        token.UniformlyPaddingMD  = token.SizeMD;
        token.UniformlyPaddingLG  = token.SizeLG;
        token.UniformlyPaddingXL  = token.SizeXL;

        token.PaddingXXS = new Thickness(token.SizeXXS);
        token.PaddingXS  = new Thickness(token.SizeXS);
        token.PaddingSM  = new Thickness(token.SizeSM);
        token.Padding    = new Thickness(token.Size);
        token.PaddingMD  = new Thickness(token.SizeMD);
        token.PaddingLG  = new Thickness(token.SizeLG);
        token.PaddingXL  = new Thickness(token.SizeXL);

        token.InputPadding = new Thickness(
            token.UniformlyPaddingSM - token.LineWidth,
            Math.Round((token.ControlHeight - token.FontSize * token.RelativeLineHeight) / 2 * 10) / 10 - token.LineWidth);
        token.InputPaddingSM = new Thickness(
            token.ControlPaddingHorizontalSM - token.LineWidth,
            Math.Round((token.ControlHeightSM - token.FontSize * token.RelativeLineHeight) / 2 * 10) / 10 - token.LineWidth * 2);
        token.InputPaddingLG = new Thickness(
            token.ControlPaddingHorizontal - token.LineWidth,
            Math.Ceiling((token.ControlHeightLG - token.FontSizeLG * token.RelativeLineHeightLG) / 2 * 10) / 10 - token.LineWidth);
        token.InputContentMargin = new Thickness(token.LineWidth);
        token.InputActiveShadow = new BoxShadows(new BoxShadow
        {
            Spread = token.ControlOutlineWidth,
            Color  = token.ColorControlOutline
        });
        token.InputErrorActiveShadow = new BoxShadows(new BoxShadow
        {
            Spread = token.ControlOutlineWidth,
            Color  = token.ColorErrorOutline
        });
        token.InputWarningActiveShadow = new BoxShadows(new BoxShadow
        {
            Spread = token.ControlOutlineWidth,
            Color  = token.ColorWarningOutline
        });

        token.PaddingContentHorizontalLG = token.SizeLG;
        token.PaddingContentVerticalLG   = token.SizeMS;
        token.PaddingContentHorizontal   = token.SizeMS;
        token.PaddingContentVertical     = token.SizeSM;
        token.PaddingContentHorizontalSM = token.SizeSM;
        token.PaddingContentHorizontalXS = token.SizeXS;
        token.PaddingContentVerticalSM   = token.SizeXS;

        token.UniformlyMarginXXS = token.SizeXXS;
        token.UniformlyMarginXS  = token.SizeXS;
        token.UniformlyMarginSM  = token.SizeSM;
        token.UniformlyMargin    = token.Size;
        token.UniformlyMarginMD  = token.SizeMD;
        token.UniformlyMarginLG  = token.SizeLG;
        token.UniformlyMarginXL  = token.SizeXL;
        token.UniformlyMarginXXL = token.SizeXXL;

        token.SpacingXXS = token.SizeXXS;
        token.SpacingXS  = token.SizeXS;
        token.SpacingSM  = token.SizeSM;
        token.Spacing    = token.Size;
        token.SpacingMD  = token.SizeMD;
        token.SpacingLG  = token.SizeLG;
        token.SpacingXL  = token.SizeXL;
        token.SpacingXXL = token.SizeXXL;

        token.MarginXXS          = new Thickness(token.SizeXXS);
        token.MarginXS           = new Thickness(token.SizeXS);
        token.MarginSM           = new Thickness(token.SizeSM);
        token.Margin             = new Thickness(token.Size);
        token.MarginMD           = new Thickness(token.SizeMD);
        token.MarginLG           = new Thickness(token.SizeLG);
        token.MarginXL           = new Thickness(token.SizeXL);
        token.MarginXXL          = new Thickness(token.SizeXXL);

        token.ScreenXS     = screenXS;
        token.ScreenXSMin  = screenXS;
        token.ScreenXSMax  = screenSM - 1;
        token.ScreenSM     = screenSM;
        token.ScreenSMMin  = screenSM;
        token.ScreenSMMax  = screenMD - 1;
        token.ScreenMD     = screenMD;
        token.ScreenMDMin  = screenMD;
        token.ScreenMDMax  = screenLG - 1;
        token.ScreenLG     = screenLG;
        token.ScreenLGMin  = screenLG;
        token.ScreenLGMax  = screenXL - 1;
        token.ScreenXL     = screenXL;
        token.ScreenXLMin  = screenXL;
        token.ScreenXLMax  = screenXXL - 1;
        token.ScreenXXL     = screenXXL;
        token.ScreenXXLMin  = screenXXL;
        token.ScreenXXLMax  = screenXXXL - 1;
        token.ScreenXXXL    = screenXXXL;
        token.ScreenXXXLMin = screenXXXL;

        token.BoxShadows = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 6,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.08, 0, 0, 0)
        }, [new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 3,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 9,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.05, 0, 0, 0)
            }]);

        token.BoxShadowsSecondary = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 6,
            Blur    = 16,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.10, 0, 0, 0)
        }, [new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 3,
                Blur    = 6,
                Spread  = -4,
                Color   = ColorUtils.FromRgbF(0.12, 0, 0, 0)
            },
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 9,
                Blur    = 28,
                Spread  = 8,
                Color   = ColorUtils.FromRgbF(0.07, 0, 0, 0)
            }]);

        token.BoxShadowsTertiary = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 1,
            Blur    = 2,
            Spread  = 0,
            Color   = ColorUtils.FromRgbF(0.03, 0, 0, 0)
        }, [new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 1,
                Blur    = 6,
                Spread  = -1,
                Color   = ColorUtils.FromRgbF(0.02, 0, 0, 0)
            },
            new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 2,
                Blur    = 4,
                Spread  = 0,
                Color   = ColorUtils.FromRgbF(0.02, 0, 0, 0)
            }]);
    }

    /// <summary>
    /// The preset's alias rules: focus, placeholder, disabled, contrast, highlight, field padding and shadows. The rules
    /// follow the variant's appearance; widths and paddings derive from Map size tokens and follow the compact density.
    /// </summary>
    private static void ApplyPresetRules(ThemeAppearance appearance, PrimeNGAppearanceRules rules, PrimeNGAliasRule alias,
                                          DesignToken token)
    {
        var surface = rules.Surface.AsSpan();

        // PrimeUIX focus.ring.color (the primary colour; Nora dark: the field border, see its table); the ring and the
        // field glow widths come from the preset (Aura: a one-line ring, no glow; Lara: no ring, a three-line glow;
        // Nora: a two-line ring, no glow).
        token.ColorFocusBorder           = rules.FocusBorder(token);
        token.FocusVisualBorderThickness = new Thickness(token.LineWidth * alias.FocusVisualBorderLineWidths);

        // PrimeUIX form.field.placeholder.color is the muted text (surface 500, Nora 600; dark surface 400), which reads
        // almost like a typed value. AtomUI keeps the placeholder one step lighter than that in every preset: surface
        // 400 in light and surface 500 in dark (a deliberate deviation; ColorTextSecondary keeps the muted text).
        token.ColorTextPlaceholder     = surface[PrimeNGPalette.IndexOf(appearance == ThemeAppearance.Dark ? 500 : 400)];
        // PrimeUIX text.muted.color is also the description text.
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
        // PrimeUIX list.option.focus.background, the hovered or keyboard-focused option, equals
        // navigation.item.active.background in all three presets (Nora: surface.200 / 700).
        token.ControlItemBgHover = rules.NavigationItemActive.Resolve(surface);

        // @primeuix/themes 3.1.1 form.field.padding.x is 0.625rem, sm 0.5rem, lg 0.75rem (10 / 8 / 12) in all three
        // presets. They follow the size ladder (SizeSM 12, compact 8), so the compact density narrows them too. The Ant
        // formula pads inputs by UniformlyPaddingSM - LineWidth (11). The vertical padding makes a field exactly its
        // control height, as in PrimeNG, instead of the Ant rounding (35 rendered 36, SM 28 rendered 26).
        var fieldPaddingX = token.SizeSM - 2;
        // The small field also insets its text by InputContentMargin; its text sits on the FontHeight line, as in
        // every AtomUI Small field.
        var chrome = 2 * token.LineWidth;
        token.InputPadding   = FieldPadding(fieldPaddingX, token.ControlHeight - token.FontHeight - chrome);
        token.InputPaddingSM = FieldPadding(token.SizeSM - 4, token.ControlHeightSM - token.FontHeight - chrome -
                                                              token.InputContentMargin.Top - token.InputContentMargin.Bottom);
        token.InputPaddingLG = FieldPadding(token.SizeSM, token.ControlHeightLG - token.FontHeightLG - chrome);
        // Content inset of item rows and other content: the field padding inside a border (the Button derivation sets
        // the Button's own label paddings).
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
}
