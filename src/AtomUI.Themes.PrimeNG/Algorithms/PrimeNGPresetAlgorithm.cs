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
/// families; <c>ColorLink</c> defaults to the primary seed. The neutral scale is a constant of the preset.
/// </para>
/// <para>
/// A dark algorithm that follows another algorithm only recomputes the colours: the sizes of the previous Map stay,
/// as with <c>AtomUI:Dark</c>. Compact density is <c>AtomUI:Compact</c> appended to the chain.
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

        WriteColors(effectiveSeed, nextMap, _dark ? _preset.Dark : _preset.Light);
        if (!_dark || previousMap is null)
        {
            WriteSizes(effectiveSeed, nextMap, _preset.Sizes);
        }
    }

    /// <inheritdoc />
    public void EvaluateAlias(in ThemeAlgorithmContext context, DesignToken token)
    {
        // The Alias rules of the presets are not defined yet; the stage writes nothing.
    }

    private static void WriteColors(DesignToken seed, DesignToken map, PrimeNGAppearanceRules rules)
    {
        var primary = PrimeNGPalette.Resolve(seed.ColorPrimary);
        var link    = PrimeNGPalette.Resolve(seed.ColorLink ?? seed.ColorPrimary);
        var success = PrimeNGPalette.Resolve(seed.ColorSuccess);
        var warning = PrimeNGPalette.Resolve(seed.ColorWarning);
        var error   = PrimeNGPalette.Resolve(seed.ColorError);
        var info    = PrimeNGPalette.Resolve(seed.ColorInfo);
        var surface = rules.Surface.AsSpan();

        var p = rules.Primary;
        map.ColorPrimary            = p.Base.Resolve(primary);
        map.ColorPrimaryHover       = p.Hover.Resolve(primary);
        map.ColorPrimaryActive      = p.Active.Resolve(primary);
        map.ColorPrimaryBg          = p.Bg.Resolve(primary);
        map.ColorPrimaryBgHover     = p.BgHover.Resolve(primary);
        map.ColorPrimaryBorder      = p.Border.Resolve(primary);
        map.ColorPrimaryBorderHover = p.BorderHover.Resolve(primary);
        map.ColorPrimaryTextHover   = p.TextHover.Resolve(primary);
        map.ColorPrimaryText        = p.Text.Resolve(primary);
        map.ColorPrimaryTextActive  = p.TextActive.Resolve(primary);

        map.ColorLink       = p.Base.Resolve(link);
        map.ColorLinkHover  = p.Hover.Resolve(link);
        map.ColorLinkActive = p.Active.Resolve(link);

        var s = rules.Status;
        map.ColorSuccess            = s.Base.Resolve(success);
        map.ColorSuccessHover       = s.Hover.Resolve(success);
        map.ColorSuccessActive      = s.Active.Resolve(success);
        map.ColorSuccessBg          = s.Bg.Resolve(success);
        map.ColorSuccessBgHover     = s.BgHover.Resolve(success);
        map.ColorSuccessBorder      = s.Border.Resolve(success);
        map.ColorSuccessBorderHover = s.BorderHover.Resolve(success);
        map.ColorSuccessTextHover   = s.TextHover.Resolve(success);
        map.ColorSuccessText        = s.Text.Resolve(success);
        map.ColorSuccessTextActive  = s.TextActive.Resolve(success);

        map.ColorWarning            = s.Base.Resolve(warning);
        map.ColorWarningHover       = s.Hover.Resolve(warning);
        map.ColorWarningActive      = s.Active.Resolve(warning);
        map.ColorWarningBg          = s.Bg.Resolve(warning);
        map.ColorWarningBgHover     = s.BgHover.Resolve(warning);
        map.ColorWarningBorder      = s.Border.Resolve(warning);
        map.ColorWarningBorderHover = s.BorderHover.Resolve(warning);
        map.ColorWarningTextHover   = s.TextHover.Resolve(warning);
        map.ColorWarningText        = s.Text.Resolve(warning);
        map.ColorWarningTextActive  = s.TextActive.Resolve(warning);

        map.ColorInfo            = s.Base.Resolve(info);
        map.ColorInfoHover       = s.Hover.Resolve(info);
        map.ColorInfoActive      = s.Active.Resolve(info);
        map.ColorInfoBg          = s.Bg.Resolve(info);
        map.ColorInfoBgHover     = s.BgHover.Resolve(info);
        map.ColorInfoBorder      = s.Border.Resolve(info);
        map.ColorInfoBorderHover = s.BorderHover.Resolve(info);
        map.ColorInfoTextHover   = s.TextHover.Resolve(info);
        map.ColorInfoText        = s.Text.Resolve(info);
        map.ColorInfoTextActive  = s.TextActive.Resolve(info);

        var e = rules.Error;
        map.ColorError            = e.Base.Resolve(error);
        map.ColorErrorHover       = e.Hover.Resolve(error);
        map.ColorErrorActive      = e.Active.Resolve(error);
        map.ColorErrorBg          = e.Bg.Resolve(error);
        map.ColorErrorBgHover     = e.BgHover.Resolve(error);
        map.ColorErrorBgActive    = e.BgActive.Resolve(error);
        map.ColorErrorBorder      = e.Border.Resolve(error);
        map.ColorErrorBorderHover = e.BorderHover.Resolve(error);
        map.ColorErrorTextHover   = e.TextHover.Resolve(error);
        map.ColorErrorText        = e.Text.Resolve(error);
        map.ColorErrorTextActive  = e.TextActive.Resolve(error);

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
