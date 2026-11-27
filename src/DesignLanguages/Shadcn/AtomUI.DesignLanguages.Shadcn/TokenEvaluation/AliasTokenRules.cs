using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.Shadcn.TokenEvaluation;

/// <summary>
/// The alias tokens: antd's alias formulas (components/theme/util/alias.ts, copied as the starting point) with the
/// shadcn variables merged in. Explicit Map and Alias values are frozen before this runs, so the tokens derived from
/// an explicit value follow it.
/// </summary>
/// <remarks>
/// The shadcn values a later formula reads (the focus ring color and width, read by the input shadows) are written in
/// place of their formula; the others are written after the formulas, so the formulas that read them keep the antd
/// value, as the pre-refactor staged alias evaluation did.
/// </remarks>
internal static class AliasTokenRules
{
    internal static void Apply(DesignToken token, ShadcnToken language)
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
        token.LineWidthFocus = token.LineWidth * 3;
        token.FocusVisualBorderThickness = new Thickness(token.LineWidthFocus);
        token.ColorFocusBorder           = token.ColorPrimaryBorder;
        token.WaveAnimationRange         = token.LineWidth * 6;
        token.WaveStartOpacity           = 0.4;
        token.WaveFadeDuration           = TimeSpan.FromSeconds(2);
        token.WaveEasing                 = null;

        // Control
        // ring-[3px]: the focus ring width of the style.
        token.ControlOutlineWidth = language.FocusRingWidth;

        // Checkbox size and expand icon size
        token.ControlInteractiveSize = token.ControlHeight / 2;

        token.ControlItemBgHover          = token.ColorFillTertiary;
        token.ControlItemBgActive         = token.ColorPrimaryBg;
        token.ControlItemBgActiveHover    = token.ColorPrimaryBgHover;
        token.ControlItemColorActive      = token.ColorText;
        token.ControlItemBgActiveDisabled = token.ColorFill;
        // ring-ring/50: the ring variable at the style's focus alpha.
        token.ColorControlOutline = ShadcnRecipes.WithPercent(language.Ring, language.FocusRingAlpha);

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

        WriteShadcnAliases(token, language);
    }

    /// <summary>
    /// The shadcn variables that have the last word over the antd alias formulas.
    /// </summary>
    private static void WriteShadcnAliases(DesignToken token, ShadcnToken language)
    {
        token.ColorTextLightSolid = language.ThemePrimaryForeground;
        token.ColorTextHeading    = language.Foreground;

        token.ColorIcon                = language.Foreground;
        token.ColorIconHover           = language.Foreground;
        token.ColorFillContent         = language.Muted;
        token.ColorSplit               = language.Border;
        token.ControlItemBgHover       = language.Accent;
        token.ControlItemBgActive      = language.Accent;
        token.ControlItemColorActive   = language.AccentForeground;
        token.ColorTextDescription     = language.MutedForeground;
        token.ColorTextPlaceholder     = language.MutedForeground;
        token.ColorBgContainerDisabled = language.Muted;
        token.ColorTextDisabled        = ShadcnRecipes.CompositeOver(language.MutedForeground, 0.5, language.Background);

        // Global shadow pair: the elevated surface carries the style's card ring and shadow; the secondary keeps the xs
        // level. The floating surfaces get their own assembly in the control token rules.
        var cardRing = ShadcnRecipes.WithPercent(language.Foreground, language.CardRingAlpha);
        token.BoxShadows          = ShadcnRecipes.RingThenShadow(cardRing, language.CardShadow);
        token.BoxShadowsSecondary = ShadcnRecipes.ShadowXS;
    }
}
