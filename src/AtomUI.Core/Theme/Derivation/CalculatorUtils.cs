using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Theme.Derivation;

public struct RadiusInfo
{
    public double BorderRadiusXS { get; set; }
    public double BorderRadiusSM { get; set; }
    public double BorderRadiusLG { get; set; }
    public double BorderRadius { get; set; }
    public double BorderRadiusOuter { get; set; }
}

public struct FontSizeInfo
{
    public double Size { get; set; }
    public double LineHeight { get; set; }
}

public static class CalculatorUtils
{
    public static void CalculateStyleMapTokenValues(DesignToken designToken)
    {
        var motionUnit   = designToken.MotionUnit;
        var motionBase   = designToken.MotionBase;
        var borderRadius = designToken.BorderRadius.TopLeft;
        var lineWidth    = designToken.LineWidth;

        var radiusInfo = CalculateRadius(borderRadius);

        // motion
        designToken.MotionDurationFast     = TimeSpan.FromMilliseconds(motionBase + motionUnit);
        designToken.MotionDurationMid      = TimeSpan.FromMilliseconds(motionBase + motionUnit * 2);
        designToken.MotionDurationSlow     = TimeSpan.FromMilliseconds(motionBase + motionUnit * 3);
        designToken.MotionDurationVerySlow = TimeSpan.FromMilliseconds(motionBase + motionUnit * 8);
        
        // line
        designToken.LineWidthBold = lineWidth + 1;
        
        // radius
        designToken.BorderRadiusXS    = new CornerRadius(radiusInfo.BorderRadiusXS);
        designToken.BorderRadiusSM    = new CornerRadius(radiusInfo.BorderRadiusSM);
        designToken.BorderRadiusLG    = new CornerRadius(radiusInfo.BorderRadiusLG);
        designToken.BorderRadiusOuter = new CornerRadius(radiusInfo.BorderRadiusOuter);
        designToken.BorderThickness   = new Thickness(lineWidth);
    }

    public static void CalculateSizeMapTokenValues(DesignToken designToken)
    {
        var sizeUnit = designToken.SizeUnit;
        var sizeStep = designToken.SizeStep;

        designToken.SizeXXL = sizeUnit * (sizeStep + 8); // 48
        designToken.SizeXL  = sizeUnit * (sizeStep + 4); // 32
        designToken.SizeLG  = sizeUnit * (sizeStep + 2); // 24
        designToken.SizeMD  = sizeUnit * (sizeStep + 1); // 20
        designToken.SizeMS  = sizeUnit * sizeStep; // 16
        designToken.Size    = sizeUnit * sizeStep; // 16
        designToken.SizeSM  = sizeUnit * (sizeStep - 1); // 12
        designToken.SizeXS  = sizeUnit * (sizeStep - 2); // 8
        designToken.SizeXXS = sizeUnit * (sizeStep - 3); // 4
    }

    public static void CalculateFontMapTokenValues(DesignToken designToken)
    {
        var fontSizePairs = CalculateFontSize(designToken.FontSize); // Smaller size font-size as base

        var fontSizeMD   = fontSizePairs[1].Size;
        var fontSizeSM   = fontSizePairs[0].Size;
        var fontSizeLG   = fontSizePairs[2].Size;
        var lineHeight   = fontSizePairs[1].LineHeight;
        var lineHeightSM = fontSizePairs[0].LineHeight;
        var lineHeightLG = fontSizePairs[2].LineHeight;

        designToken.FontSizeSM = fontSizeSM;
        designToken.FontSize   = fontSizeMD;
        designToken.FontSizeLG = fontSizeLG;
        designToken.FontSizeXL = fontSizePairs[3].Size;

        designToken.FontSizeHeading1 = fontSizePairs[6].Size;
        designToken.FontSizeHeading2 = fontSizePairs[5].Size;
        designToken.FontSizeHeading3 = fontSizePairs[4].Size;
        designToken.FontSizeHeading4 = fontSizePairs[3].Size;
        designToken.FontSizeHeading5 = fontSizePairs[2].Size;

        designToken.RelativeLineHeight   = lineHeight;
        designToken.RelativeLineHeightLG = lineHeightLG;
        designToken.RelativeLineHeightSM = lineHeightSM;

        designToken.FontHeight   = Math.Round(lineHeight * fontSizeMD);
        designToken.FontHeightLG = Math.Round(lineHeightLG * fontSizeLG);
        designToken.FontHeightSM = Math.Round(lineHeightSM * fontSizeSM);

        designToken.RelativeLineHeightHeading1 = fontSizePairs[6].LineHeight;
        designToken.RelativeLineHeightHeading2 = fontSizePairs[5].LineHeight;
        designToken.RelativeLineHeightHeading3 = fontSizePairs[4].LineHeight;
        designToken.RelativeLineHeightHeading4 = fontSizePairs[3].LineHeight;
        designToken.RelativeLineHeightHeading5 = fontSizePairs[2].LineHeight;
    }

    public static void CalculateControlHeightMapTokenValues(DesignToken designToken)
    {
        var controlHeight = designToken.ControlHeight;
        designToken.ControlHeightSM = controlHeight * 0.75;
        designToken.ControlHeightXS = controlHeight * 0.5;
        designToken.ControlHeightLG = controlHeight * 1.25;
    }

    public static double CalculateLineHeight(double fontSize)
    {
        return (fontSize + 8) / fontSize;
    }

    public static RadiusInfo CalculateRadius(double radiusBase)
    {
        var radiusLG    = radiusBase;
        var radiusSM    = radiusBase;
        var radiusXS    = radiusBase;
        var radiusOuter = radiusBase;

        // radiusLG
        if (radiusBase < 6 && radiusBase >= 5)
        {
            radiusLG = radiusBase + 1;
        }
        else if (radiusBase < 16 && radiusBase >= 6)
        {
            radiusLG = radiusBase + 2;
        }
        else if (radiusBase >= 16)
        {
            radiusLG = 16;
        }

        // radiusSM
        if (radiusBase < 7 && radiusBase >= 5)
        {
            radiusSM = 4;
        }
        else if (radiusBase < 8 && radiusBase >= 7)
        {
            radiusSM = 5;
        }
        else if (radiusBase < 14 && radiusBase >= 8)
        {
            radiusSM = 6;
        }
        else if (radiusBase < 16 && radiusBase >= 14)
        {
            radiusSM = 7;
        }
        else if (radiusBase >= 16)
        {
            radiusSM = 8;
        }

        // radiusXS
        if (radiusBase < 6 && radiusBase >= 2)
        {
            radiusXS = 1;
        }
        else if (radiusBase >= 6)
        {
            radiusXS = 2;
        }

        // radiusOuter
        if (radiusBase > 4 && radiusBase < 8)
        {
            radiusOuter = 4;
        }
        else if (radiusBase >= 8)
        {
            radiusOuter = 6;
        }

        return new RadiusInfo
        {
            BorderRadius      = radiusBase,
            BorderRadiusXS    = radiusXS,
            BorderRadiusSM    = radiusSM,
            BorderRadiusLG    = radiusLG,
            BorderRadiusOuter = radiusOuter
        };
    }

    public static IList<FontSizeInfo> CalculateFontSize(double baseValue)
    {
        var fontSizes = new double[10];
        for (var index = 0; index < 10; ++index)
        {
            var i        = index - 1;
            var baseSize = baseValue * Math.Pow(2.71828, i / 5.0);
            var intSize  = index > 1 ? Math.Floor(baseSize) : Math.Ceiling(baseSize);
            // Convert to even
            fontSizes[index] = (int)(Math.Floor(intSize / 2.0d) * 2);
        }

        fontSizes[1] = baseValue;
        var results = new FontSizeInfo[10];
        for (var i = 0; i < fontSizes.Length; i++)
        {
            var size = fontSizes[i];
            results[i] = new FontSizeInfo
            {
                Size       = size,
                LineHeight = CalculateLineHeight(size)
            };
        }

        return results;
    }
}
