using AtomUI.Media;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Theme.Algorithms;

[ThemeAlgorithmAttribute(ThemeAlgorithm.BuiltInCatalog, "Dark", 1, ThemeAppearanceEffect.Dark)]
public class DarkThemeVariantCalculator : AbstractThemeVariantCalculator
{
    private static readonly PaletteGenerateOption s_darkPaletteOption = new()
    {
        ThemeVariant = ThemeVariant.Dark
    };

    public DarkThemeVariantCalculator()
    {
        _colorBgBase   = Color.FromRgb(0, 0, 0);
        _colorTextBase = Color.FromRgb(255, 255, 255);
    }

    public override void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
    {
        ArgumentNullException.ThrowIfNull(effectiveSeed);
        ArgumentNullException.ThrowIfNull(nextMap);
        if (previousMap is null)
        {
            new DefaultThemeVariantCalculator().Evaluate(effectiveSeed, null, nextMap);
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
        // 生成所有预置颜色的色系
        foreach (var presetColor in PresetPrimaryColor.AllColorTypes())
        {
            var colors = PaletteGenerator.GeneratePalette(designToken.GetPresetPrimaryColor(presetColor.Type).Color(),
                s_darkPaletteOption);
            var colorMap = ColorMap.FromColors(colors);
            designToken.SetColorPalette(presetColor, colorMap);
        }
    }
}
