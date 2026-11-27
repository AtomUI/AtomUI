// The Ant Design map calculators (default, dark, compact), copied into the PrimeNG language as the base its presets
// refine: PrimeNG derived its Map tokens by running them first and overwriting the preset's colors and sizes.
// The copy belongs to PrimeNG and evolves on its own, because first-party languages do not derive from each other.
using AtomUI.Media;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.DesignLanguages.PrimeNG.TokenEvaluation;

/// <summary>
/// The two seeds the Ant Design map algorithms read from the original seed rather than from the token in progress.
/// </summary>
internal readonly record struct PrimeNGBaseSeeds(Color? ColorBgBase, Color? ColorTextBase);

internal abstract class PrimeNGBaseVariantCalculator
{
    protected Color _colorBgBase;
    protected Color _colorTextBase;

    public Color ColorBgBase => _colorBgBase;
    public Color ColorTextBase => _colorTextBase;

    protected virtual ColorMap GenerateColorPalettes(Color baseColor)
    {
        return default!;
    }

    protected virtual void CalculateNeutralColorPalettes(Color? bgBaseColor, Color? textBaseColor, DesignToken designToken)
    {
    }

    public abstract void Evaluate(in PrimeNGBaseSeeds effectiveSeed, bool hasPrevious, DesignToken nextMap);

    protected void CalculateColorMapTokenValues(DesignToken designToken)
    {
        var colorSuccessBase = designToken.ColorSuccess;
        var colorWarningBase = designToken.ColorWarning;
        var colorErrorBase   = designToken.ColorError;
        var colorInfoBase    = designToken.ColorInfo;
        var colorPrimaryBase = designToken.ColorPrimary;
        var colorBgBase      = designToken.ColorBgBase;
        var colorTextBase    = designToken.ColorTextBase;

        var primaryColors = GenerateColorPalettes(colorPrimaryBase);
        var successColors = GenerateColorPalettes(colorSuccessBase);
        var warningColors = GenerateColorPalettes(colorWarningBase);
        var errorColors   = GenerateColorPalettes(colorErrorBase);
        var infoColors    = GenerateColorPalettes(colorInfoBase);

        CalculateNeutralColorPalettes(colorBgBase, colorTextBase, designToken);
        var colorLink  = designToken.ColorLink ?? designToken.ColorInfo;
        var linkColors = GenerateColorPalettes(colorLink);

        designToken.ColorPrimaryBg          = primaryColors.Color1;
        designToken.ColorPrimaryBgHover     = primaryColors.Color2;
        designToken.ColorPrimaryBorder      = primaryColors.Color3;
        designToken.ColorPrimaryBorderHover = primaryColors.Color4;
        designToken.ColorPrimaryHover       = primaryColors.Color5;
        designToken.ColorPrimary            = primaryColors.Color6;
        designToken.ColorPrimaryActive      = primaryColors.Color7;
        designToken.ColorPrimaryTextHover   = primaryColors.Color8;
        designToken.ColorPrimaryText        = primaryColors.Color9;
        designToken.ColorPrimaryTextActive  = primaryColors.Color10;

        designToken.ColorSuccessBg          = successColors.Color1;
        designToken.ColorSuccessBgHover     = successColors.Color2;
        designToken.ColorSuccessBorder      = successColors.Color3;
        designToken.ColorSuccessBorderHover = successColors.Color4;
        designToken.ColorSuccessHover       = successColors.Color4;
        designToken.ColorSuccess            = successColors.Color6;
        designToken.ColorSuccessActive      = successColors.Color7;
        designToken.ColorSuccessTextHover   = successColors.Color8;
        designToken.ColorSuccessText        = successColors.Color9;
        designToken.ColorSuccessTextActive  = successColors.Color10;

        designToken.ColorErrorBg          = errorColors.Color1;
        designToken.ColorErrorBgHover     = errorColors.Color2;
        designToken.ColorErrorBgActive    = errorColors.Color3;
        designToken.ColorErrorBorder      = errorColors.Color3;
        designToken.ColorErrorBorderHover = errorColors.Color4;
        designToken.ColorErrorHover       = errorColors.Color5;
        designToken.ColorError            = errorColors.Color6;
        designToken.ColorErrorActive      = errorColors.Color7;
        designToken.ColorErrorTextHover   = errorColors.Color8;
        designToken.ColorErrorText        = errorColors.Color9;
        designToken.ColorErrorTextActive  = errorColors.Color10;

        designToken.ColorWarningBg          = warningColors.Color1;
        designToken.ColorWarningBgHover     = warningColors.Color2;
        designToken.ColorWarningBorder      = warningColors.Color3;
        designToken.ColorWarningBorderHover = warningColors.Color4;
        designToken.ColorWarningHover       = warningColors.Color4;
        designToken.ColorWarning            = warningColors.Color6;
        designToken.ColorWarningActive      = warningColors.Color7;
        designToken.ColorWarningTextHover   = warningColors.Color8;
        designToken.ColorWarningText        = warningColors.Color9;
        designToken.ColorWarningTextActive  = warningColors.Color10;

        designToken.ColorInfoBg          = infoColors.Color1;
        designToken.ColorInfoBgHover     = infoColors.Color2;
        designToken.ColorInfoBorder      = infoColors.Color3;
        designToken.ColorInfoBorderHover = infoColors.Color4;
        designToken.ColorInfoHover       = infoColors.Color4;
        designToken.ColorInfo            = infoColors.Color6;
        designToken.ColorInfoActive      = infoColors.Color7;
        designToken.ColorInfoTextHover   = infoColors.Color8;
        designToken.ColorInfoText        = infoColors.Color9;
        designToken.ColorInfoTextActive  = infoColors.Color10;

        designToken.ColorLinkHover  = linkColors.Color4;
        designToken.ColorLink       = linkColors.Color6;
        designToken.ColorLinkActive = linkColors.Color7;

        designToken.ColorBgMask         = ColorUtils.FromRgbF(0.45, 0, 0, 0);
        designToken.ColorWhite          = Color.FromRgb(255, 255, 255);
        designToken.ColorBlack          = Color.FromRgb(0, 0, 0);
        designToken.SelectionBackground = primaryColors.Color6;
        designToken.SelectionForeground = Color.FromRgb(255, 255, 255);
    }
}

internal sealed class PrimeNGBaseDefaultCalculator : PrimeNGBaseVariantCalculator
{
    public PrimeNGBaseDefaultCalculator()
    {
        _colorBgBase   = Color.FromRgb(255, 255, 255);
        _colorTextBase = Color.FromRgb(0, 0, 0);
    }

    public override void Evaluate(in PrimeNGBaseSeeds effectiveSeed, bool hasPrevious, DesignToken nextMap)
    {
        ArgumentNullException.ThrowIfNull(nextMap);

        _colorBgBase      = effectiveSeed.ColorBgBase ?? Color.FromRgb(255, 255, 255);
        _colorTextBase    = effectiveSeed.ColorTextBase ?? Color.FromRgb(0, 0, 0);
        nextMap.ColorBgBase   = _colorBgBase;
        nextMap.ColorTextBase = _colorTextBase;

        SetupColorPalettes(nextMap);
        CalculateColorMapTokenValues(nextMap);

        CalculatorUtils.CalculateFontMapTokenValues(nextMap);
        CalculatorUtils.CalculateSizeMapTokenValues(nextMap);
        CalculatorUtils.CalculateControlHeightMapTokenValues(nextMap);
        CalculatorUtils.CalculateStyleMapTokenValues(nextMap);
    }

    protected override ColorMap GenerateColorPalettes(Color baseColor)
    {
        var colors = PaletteGenerator.GeneratePalette(baseColor);
        var colorMap = new ColorMap
        {
            Color1  = colors[0],
            Color2  = colors[1],
            Color3  = colors[2],
            Color4  = colors[3],
            Color5  = colors[4],
            Color6  = colors[5],
            Color7  = colors[6],
            Color8  = colors[4],
            Color9  = colors[5],
            Color10 = colors[6]
            // Color8 = colors[7],
            // Color9 = colors[8],
            // Color10 = colors[9],
        };
        return colorMap;
    }

    protected override void CalculateNeutralColorPalettes(Color? bgBaseColor, Color? textBaseColor, DesignToken designToken)
    {
        var colorBgBase   = bgBaseColor ?? _colorBgBase;
        var colorTextBase = textBaseColor ?? _colorTextBase;

        designToken.ColorText           = AlphaColor(colorTextBase, 0.88);
        designToken.ColorTextSecondary  = AlphaColor(colorTextBase, 0.65);
        designToken.ColorTextTertiary   = AlphaColor(colorTextBase, 0.45);
        designToken.ColorTextQuaternary = AlphaColor(colorTextBase, 0.25);

        designToken.ColorFill           = AlphaColor(colorTextBase, 0.15);
        designToken.ColorFillSecondary  = AlphaColor(colorTextBase, 0.06);
        designToken.ColorFillTertiary   = AlphaColor(colorTextBase, 0.04);
        designToken.ColorFillQuaternary = AlphaColor(colorTextBase, 0.02);

        designToken.ColorBgLayout    = SolidColor(colorBgBase, 4);
        designToken.ColorBgContainer = SolidColor(colorBgBase, 0);
        designToken.ColorBgElevated  = SolidColor(colorBgBase, 0);
        designToken.ColorBgSpotlight = AlphaColor(colorTextBase, 0.85);
        designToken.ColorBgBlur      = ColorUtils.TransparentColor();

        designToken.ColorBgSolid       = AlphaColor(colorTextBase, 1);
        designToken.ColorBgSolidHover  = AlphaColor(colorTextBase, 0.75);
        designToken.ColorBgSolidActive = AlphaColor(colorTextBase, 0.95);

        designToken.ColorBorder          = SolidColor(colorBgBase, 15);
        designToken.ColorBorderSecondary = SolidColor(colorBgBase, 6);
    }

    private Color AlphaColor(in Color baseColor, double alpha)
    {
        return Color.FromArgb((byte)(alpha * 255), baseColor.R, baseColor.G, baseColor.B);
    }

    private Color SolidColor(Color baseColor, int brightness)
    {
        return baseColor.Darken(brightness);
    }

    private void SetupColorPalettes(DesignToken designToken)
    {
        // Generates the palette of every preset color.
        foreach (var presetColor in PresetPrimaryColor.AllColorTypes())
        {
            var colors   = PaletteGenerator.GeneratePalette(designToken.GetPresetPrimaryColor(presetColor.Type).Color());
            var colorMap = ColorMap.FromColors(colors);
            designToken.SetColorPalette(presetColor, colorMap);
        }
    }
}

internal sealed class PrimeNGBaseDarkCalculator : PrimeNGBaseVariantCalculator
{
    private static readonly PaletteGenerateOption s_darkPaletteOption = new()
    {
        ThemeVariant = ThemeVariant.Dark
    };

    public PrimeNGBaseDarkCalculator()
    {
        _colorBgBase   = Color.FromRgb(0, 0, 0);
        _colorTextBase = Color.FromRgb(255, 255, 255);
    }

    public override void Evaluate(in PrimeNGBaseSeeds effectiveSeed, bool hasPrevious, DesignToken nextMap)
    {
        ArgumentNullException.ThrowIfNull(nextMap);
        if (!hasPrevious)
        {
            new PrimeNGBaseDefaultCalculator().Evaluate(in effectiveSeed, false, nextMap);
        }

        _colorBgBase         = effectiveSeed.ColorBgBase ?? Color.FromRgb(0, 0, 0);
        _colorTextBase       = effectiveSeed.ColorTextBase ?? Color.FromRgb(255, 255, 255);
        nextMap.ColorBgBase   = _colorBgBase;
        nextMap.ColorTextBase = _colorTextBase;

        // Dark tokens
        SetupColorPalettes(nextMap);
        CalculateColorMapTokenValues(nextMap);
        // Customize selected item background color
        // https://github.com/ant-design/ant-design/issues/30524#issuecomment-871961867
        nextMap.ColorPrimaryBg      = nextMap.ColorPrimaryBorder;
        nextMap.ColorPrimaryBgHover = nextMap.ColorPrimaryBorderHover;
    }

    protected override ColorMap GenerateColorPalettes(Color baseColor)
    {
        var colors = PaletteGenerator.GeneratePalette(baseColor, s_darkPaletteOption);
        var colorMap = new ColorMap
        {
            Color1  = colors[0],
            Color2  = colors[1],
            Color3  = colors[2],
            Color4  = colors[3],
            Color5  = colors[6],
            Color6  = colors[5],
            Color7  = colors[4],
            Color8  = colors[6],
            Color9  = colors[5],
            Color10 = colors[4]
            // Color8 = colors[9],
            // Color9 = colors[8],
            // Color10 = colors[7],
        };
        return colorMap;
    }

    protected override void CalculateNeutralColorPalettes(Color? bgBaseColor, Color? textBaseColor,
                                                          DesignToken designToken)
    {
        var colorBgBase   = bgBaseColor ?? _colorBgBase;
        var colorTextBase = textBaseColor ?? _colorTextBase;

        designToken.ColorText           = CalculateAlphaColor(colorTextBase, 0.85);
        designToken.ColorTextSecondary  = CalculateAlphaColor(colorTextBase, 0.65);
        designToken.ColorTextTertiary   = CalculateAlphaColor(colorTextBase, 0.45);
        designToken.ColorTextQuaternary = CalculateAlphaColor(colorTextBase, 0.25);

        designToken.ColorFill           = CalculateAlphaColor(colorTextBase, 0.18);
        designToken.ColorFillSecondary  = CalculateAlphaColor(colorTextBase, 0.12);
        designToken.ColorFillTertiary   = CalculateAlphaColor(colorTextBase, 0.08);
        designToken.ColorFillQuaternary = CalculateAlphaColor(colorTextBase, 0.04);

        designToken.ColorBgSolid       = CalculateAlphaColor(colorTextBase, 1);
        designToken.ColorBgSolidHover  = CalculateAlphaColor(colorTextBase, 0.75);
        designToken.ColorBgSolidActive = CalculateAlphaColor(colorTextBase, 0.95);

        designToken.ColorBgElevated  = CalculateSolidColor(colorBgBase, 12);
        designToken.ColorBgContainer = CalculateSolidColor(colorBgBase, 8);
        designToken.ColorBgLayout    = CalculateSolidColor(colorBgBase, 0);
        designToken.ColorBgSpotlight = CalculateSolidColor(colorBgBase, 26);
        designToken.ColorBgBlur      = CalculateAlphaColor(colorTextBase, 0.04);

        designToken.ColorBorder          = CalculateSolidColor(colorBgBase, 26);
        designToken.ColorBorderSecondary = CalculateSolidColor(colorBgBase, 19);
    }

    private Color CalculateAlphaColor(in Color baseColor, double alpha)
    {
        return Color.FromArgb((byte)(alpha * 255), baseColor.R, baseColor.G, baseColor.B);
    }

    private Color CalculateSolidColor(Color baseColor, int brightness)
    {
        return baseColor.Lighten(brightness);
    }

    private void SetupColorPalettes(DesignToken designToken)
    {
        // Generates the palette of every preset color.
        foreach (var presetColor in PresetPrimaryColor.AllColorTypes())
        {
            var colors = PaletteGenerator.GeneratePalette(designToken.GetPresetPrimaryColor(presetColor.Type).Color(),
                s_darkPaletteOption);
            var colorMap = ColorMap.FromColors(colors);
            designToken.SetColorPalette(presetColor, colorMap);
        }
    }
}

internal sealed class PrimeNGBaseCompactCalculator : PrimeNGBaseVariantCalculator
{
    public PrimeNGBaseCompactCalculator()
    {
    }

    public override void Evaluate(in PrimeNGBaseSeeds effectiveSeed, bool hasPrevious, DesignToken nextMap)
    {
        ArgumentNullException.ThrowIfNull(nextMap);
        if (!hasPrevious)
        {
            new PrimeNGBaseDefaultCalculator().Evaluate(in effectiveSeed, false, nextMap);
        }

        _colorBgBase   = nextMap.ColorBgBase ?? Color.FromRgb(255, 255, 255);
        _colorTextBase = nextMap.ColorTextBase ?? Color.FromRgb(0, 0, 0);

        var controlHeight = nextMap.ControlHeight - 4;

        CalculateCompactSizeMapTokenValues(nextMap);
        CalculatorUtils.CalculateFontMapTokenValues(nextMap);

        nextMap.ControlHeight = controlHeight;
        CalculatorUtils.CalculateControlHeightMapTokenValues(nextMap);
    }

    private void CalculateCompactSizeMapTokenValues(DesignToken designToken)
    {
        var sizeUnit        = designToken.SizeUnit;
        var sizeStep        = designToken.SizeStep;
        var compactSizeStep = sizeStep - 2;

        designToken.SizeXXL = sizeUnit * (compactSizeStep + 10);
        designToken.SizeXL  = sizeUnit * (compactSizeStep + 6);
        designToken.SizeLG  = sizeUnit * (compactSizeStep + 2);
        designToken.SizeMD  = sizeUnit * (compactSizeStep + 2);
        designToken.SizeMS  = sizeUnit * (compactSizeStep + 1);
        designToken.Size    = sizeUnit * compactSizeStep;
        designToken.SizeSM  = sizeUnit * compactSizeStep;
        designToken.SizeXS  = sizeUnit * (compactSizeStep - 1);
        designToken.SizeXXS = sizeUnit * (compactSizeStep - 1);
    }
}
