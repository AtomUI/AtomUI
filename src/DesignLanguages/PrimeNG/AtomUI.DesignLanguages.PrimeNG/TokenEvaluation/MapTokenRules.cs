using AtomUI.DesignLanguages.PrimeNG.Presets;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG.TokenEvaluation;

/// <summary>
/// The seeds the preset rules read from the original seed rather than from the token in progress.
/// </summary>
internal readonly record struct PrimeNGSeeds(
    Color ColorPrimary,
    Color? ColorLink,
    Color ColorSuccess,
    Color ColorWarning,
    Color ColorError,
    Color ColorInfo,
    Color? ColorBgBase,
    Color? ColorTextBase,
    CornerRadius BorderRadius,
    double ControlHeight)
{
    internal static PrimeNGSeeds Of(DesignToken seed) => new(
        seed.ColorPrimary, seed.ColorLink, seed.ColorSuccess, seed.ColorWarning, seed.ColorError, seed.ColorInfo,
        seed.ColorBgBase, seed.ColorTextBase, seed.BorderRadius, seed.ControlHeight);
}

/// <summary>
/// The PrimeNG map tokens. The Ant Design base (the default calculator, or the dark one for the dark appearance) gives
/// every Map token a value (the preset palettes, the font, size, motion and line ladders); then the preset writes its
/// colors, its state transition duration and its size ladder; the compact density then applies the compact size and
/// height ladders on top, as the <c>[PrimeNG preset, Compact]</c> chain did.
/// </summary>
/// <remarks>
/// The <c>ColorPrimary</c>, <c>ColorSuccess</c>, <c>ColorWarning</c>, <c>ColorError</c> and <c>ColorInfo</c> seeds are
/// the 500 stops of their families; <c>ColorLink</c> defaults to the primary seed. The neutral scale is the language's
/// surface scale. A primary or link seed that is the 950 stop of a neutral family is the primeng.dev "noir" primary
/// (<see cref="PrimeNGNoir" />).
/// </remarks>
internal static class MapTokenRules
{
    internal static void Apply(in DesignVariant variant, PrimeNGPreset preset, PrimeNGToken language, DesignToken token)
    {
        var seeds    = PrimeNGSeeds.Of(token);
        var antSeeds = new PrimeNGBaseSeeds(token.ColorBgBase, token.ColorTextBase);
        var dark     = variant.Appearance == ThemeAppearance.Dark;
        if (dark)
        {
            new PrimeNGBaseDarkCalculator().Evaluate(in antSeeds, false, token);
        }
        else
        {
            new PrimeNGBaseDefaultCalculator().Evaluate(in antSeeds, false, token);
        }

        WriteColors(in seeds, token, PrimeNGPresets.For(preset, variant.Appearance, language), PrimeNGNoir.For(dark));
        if (preset.TransitionDuration is { } transition)
        {
            // The state-color transitions; VerySlow drives value animations (progress, count-up), which PrimeNG
            // keeps in every preset (progressbar: width 1s).
            token.MotionDurationFast = transition;
            token.MotionDurationMid  = transition;
            token.MotionDurationSlow = transition;
        }
        WriteSizes(in seeds, token, preset.Sizes);

        if (variant.Density == DesignDensity.Compact)
        {
            new PrimeNGBaseCompactCalculator().Evaluate(in antSeeds, true, token);
        }
    }

    private static void WriteColors(in PrimeNGSeeds seed, DesignToken map, PrimeNGAppearanceRules rules, PrimeNGNoirRule noir)
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

    private static void WriteSizes(in PrimeNGSeeds seed, DesignToken map, PrimeNGSizeLadder ladder)
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
