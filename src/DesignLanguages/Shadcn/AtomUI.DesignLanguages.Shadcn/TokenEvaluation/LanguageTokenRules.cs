using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.Shadcn.TokenEvaluation;

/// <summary>
/// The language seeds and the language tokens of a variant.
/// </summary>
internal static class LanguageTokenRules
{
    /// <summary>
    /// The light <c>--primary</c> of the neutral base color (neutral-900): the default theme.
    /// </summary>
    public static readonly Color DefaultPrimarySeed = ShadcnTailwindPalette.Stop("neutral", 900);

    /// <summary>
    /// The seeds of every style and appearance: the neutral theme and base color, and no wave effect (shadcn
    /// controls have none).
    /// </summary>
    public static void DefineSeeds(DesignToken seed, ShadcnToken languageSeed)
    {
        seed.ColorPrimary        = DefaultPrimarySeed;
        seed.EnableWaveSpirit    = false;
        languageSeed.BaseColor   = ShadcnBaseColors.DefaultSeed;
    }

    public static void Apply(in DesignVariant variant, DesignToken seed, ShadcnToken language)
    {
        var dark    = variant.IsDark;
        var surface = ShadcnBaseColors.Resolve(language.BaseColor, dark);
        language.Background          = surface.Background;
        language.Foreground          = surface.Foreground;
        language.Card                = surface.Card;
        language.Popover             = surface.Popover;
        language.Primary             = surface.Primary;
        language.PrimaryForeground   = surface.PrimaryForeground;
        language.Secondary           = surface.Secondary;
        language.SecondaryForeground = surface.SecondaryForeground;
        language.Muted               = surface.Muted;
        language.MutedForeground     = surface.MutedForeground;
        language.Accent              = surface.Accent;
        language.AccentForeground    = surface.AccentForeground;
        language.Destructive         = surface.Destructive;
        language.Border              = surface.Border;
        language.Input               = surface.Input;
        language.Ring                = surface.Ring;

        // The Map writes the appearance-adjusted primary; the foreground and the theme identity are resolved back
        // from that computed value (the later stages never see the seed).
        var primary  = ShadcnThemeColor.Resolve(seed.ColorPrimary, surface, dark).Primary;
        var computed = ShadcnThemeColor.ResolveComputed(primary, surface, dark);
        language.ThemePrimary           = primary;
        language.ThemePrimaryForeground = computed.Foreground;
        language.HasColorTheme          = computed.Theme is not null and not "neutral";

        // The registry's default chart color is independent of the base color and the theme primary.
        const string chartColor = "neutral";
        language.Chart1 = ShadcnTailwindPalette.Stop(chartColor, 300);
        language.Chart2 = ShadcnTailwindPalette.Stop(chartColor, 500);
        language.Chart3 = ShadcnTailwindPalette.Stop(chartColor, 600);
        language.Chart4 = ShadcnTailwindPalette.Stop(chartColor, 700);
        language.Chart5 = ShadcnTailwindPalette.Stop(chartColor, 800);

        var style = ShadcnStyleTables.Of(variant.Style);
        language.StyleControlHeight   = style.ControlHeight;
        language.StyleControlHeightSM = style.ControlHeightSM;
        language.StyleControlHeightLG = style.ControlHeightLG;
        language.StyleControlHeightXS = style.ControlHeightXS;
        language.ButtonRadius    = style.ButtonRadius;
        language.ButtonRadiusSM  = style.ButtonRadiusSM;
        language.ButtonRadiusLG  = style.ButtonRadiusLG;
        language.InputRadius     = style.InputRadius;
        language.DarkInputFill   = style.DarkInputFill;
        language.CheckBoxRadius  = style.CheckBoxRadius;
        language.ProgressHeight  = style.ProgressHeight;
        language.ProgressRadius  = style.ProgressRadius;
        language.PaddingX        = style.PaddingX;
        language.PaddingXSM      = style.PaddingXSM;
        language.PaddingXLG      = style.PaddingXLG;
        language.StyleFontSize        = style.FontSize;
        language.StyleFontSizeSM      = style.FontSizeSM;
        language.ButtonWeight    = style.ButtonWeight;
        language.FocusRingWidth  = style.FocusRingWidth;
        language.FocusRingAlpha  = style.FocusRingAlpha;
        language.CardRadius      = style.CardRadius;
        language.CardPadding     = style.CardPadding;
        language.CardShadow      = ShadcnStyleTables.Shadow(style.CardShadow);
        language.CardRingAlpha   = style.CardRingAlpha;

        var overlay = style.Overlay;
        language.MenuShadow         = ShadcnStyleTables.Shadow(overlay.MenuShadow);
        language.MenuRadius         = overlay.MenuRadius;
        language.MenuPadding        = overlay.MenuPadding;
        language.PopoverShadow      = ShadcnStyleTables.Shadow(overlay.PopoverShadow);
        language.PopoverRadius      = overlay.PopoverRadius;
        language.PopoverPadding     = overlay.PopoverPadding;
        language.RingAlphaLight     = overlay.RingAlphaLight;
        language.RingAlphaDark      = overlay.RingAlphaDark;
        language.OutlineRing        = ShadcnRecipes.WithPercent(language.Foreground,
            dark ? language.RingAlphaDark : language.RingAlphaLight);
        language.TooltipRadius      = overlay.TooltipRadius;
        language.TooltipArrowRadius = overlay.TooltipArrowRadius;
        language.DialogShadow       = ShadcnStyleTables.Shadow(overlay.DialogShadow);
        language.DialogRadius       = overlay.DialogRadius;
        language.DialogPadding      = overlay.DialogPadding;
        language.DrawerShadow       = ShadcnStyleTables.Shadow(overlay.DrawerShadow);
        language.MaskAlpha          = overlay.MaskAlpha;
    }
}
