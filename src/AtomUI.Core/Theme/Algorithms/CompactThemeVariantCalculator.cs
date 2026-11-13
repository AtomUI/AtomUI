using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Theme.Algorithms;

[ThemeAlgorithmAttribute(ThemeAlgorithm.BuiltInCatalog, "Compact", 1, ThemeAppearanceEffect.Preserve)]
public class CompactThemeVariantCalculator : AbstractThemeVariantCalculator
{
    public CompactThemeVariantCalculator()
    {
    }

    public override void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
    {
        ArgumentNullException.ThrowIfNull(effectiveSeed);
        ArgumentNullException.ThrowIfNull(nextMap);
        if (previousMap is null)
        {
            new DefaultThemeVariantCalculator().Evaluate(effectiveSeed, null, nextMap);
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
