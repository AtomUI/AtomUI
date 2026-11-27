using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Theme.Derivation;

/// <summary>
/// The five corner radius tiers derived from one base radius by <see cref="CalculatorUtils.CalculateRadius" />.
/// </summary>
public struct RadiusInfo
{
    /// <summary>
    /// Extra small tier; becomes the <see cref="DesignToken.BorderRadiusXS" /> map token.
    /// </summary>
    public double BorderRadiusXS { get; set; }
    /// <summary>
    /// Small tier; becomes the <see cref="DesignToken.BorderRadiusSM" /> map token.
    /// </summary>
    public double BorderRadiusSM { get; set; }
    /// <summary>
    /// Large tier; becomes the <see cref="DesignToken.BorderRadiusLG" /> map token.
    /// </summary>
    public double BorderRadiusLG { get; set; }
    /// <summary>
    /// The base radius, unchanged. No map token is written from this member.
    /// </summary>
    public double BorderRadius { get; set; }
    /// <summary>
    /// Outer tier; becomes the <see cref="DesignToken.BorderRadiusOuter" /> map token.
    /// </summary>
    public double BorderRadiusOuter { get; set; }
}

/// <summary>
/// One step of the font size scale: a font size and its line height ratio.
/// </summary>
public struct FontSizeInfo
{
    /// <summary>
    /// The font size of this step.
    /// </summary>
    public double Size { get; set; }
    /// <summary>
    /// The line height of this step as a multiple of <see cref="Size" />, not as a length.
    /// </summary>
    public double LineHeight { get; set; }
}

/// <summary>
/// Derives the map tokens that every design language shares from the seed tokens.
/// </summary>
public static class CalculatorUtils
{
    /// <summary>
    /// Derives the motion, line and radius map tokens of a global token object from its seed tokens.
    /// </summary>
    /// <param name="designToken">
    /// The token object to update in place. Sets <see cref="DesignToken.MotionDurationFast" />,
    /// <see cref="DesignToken.MotionDurationMid" />, <see cref="DesignToken.MotionDurationSlow" /> and
    /// <see cref="DesignToken.MotionDurationVerySlow" /> to <see cref="DesignToken.MotionBase" /> plus 1, 2, 3 and
    /// 8 times <see cref="DesignToken.MotionUnit" /> milliseconds; sets <see cref="DesignToken.LineWidthBold" /> to
    /// <see cref="DesignToken.LineWidth" /> plus 1 and <see cref="DesignToken.BorderThickness" /> to the line width
    /// on all sides; and sets <see cref="DesignToken.BorderRadiusXS" />, <see cref="DesignToken.BorderRadiusSM" />,
    /// <see cref="DesignToken.BorderRadiusLG" /> and <see cref="DesignToken.BorderRadiusOuter" /> from the top-left
    /// corner of <see cref="DesignToken.BorderRadius" />, which itself is not changed.
    /// </param>
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

    /// <summary>
    /// Derives the nine size map tokens of a global token object from its size unit and size step.
    /// </summary>
    /// <param name="designToken">
    /// The token object to update in place. Sets <see cref="DesignToken.SizeXXL" />, <see cref="DesignToken.SizeXL" />,
    /// <see cref="DesignToken.SizeLG" />, <see cref="DesignToken.SizeMD" />, <see cref="DesignToken.SizeMS" />,
    /// <see cref="DesignToken.Size" />, <see cref="DesignToken.SizeSM" />, <see cref="DesignToken.SizeXS" /> and
    /// <see cref="DesignToken.SizeXXS" /> to <see cref="DesignToken.SizeUnit" /> times
    /// (<see cref="DesignToken.SizeStep" /> + n), where n is 8, 4, 2, 1, 0, 0, -1, -2 and -3 in that order.
    /// </param>
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

    /// <summary>
    /// Derives the font size, line height and font height map tokens of a global token object from its base font size.
    /// </summary>
    /// <param name="designToken">
    /// The token object to update in place. Builds ten font size steps with <see cref="CalculateFontSize" /> from
    /// <see cref="DesignToken.FontSize" /> and writes <see cref="DesignToken.FontSizeSM" />,
    /// <see cref="DesignToken.FontSize" />, <see cref="DesignToken.FontSizeLG" />,
    /// <see cref="DesignToken.FontSizeXL" />, <see cref="DesignToken.FontSizeHeading1" /> to
    /// <see cref="DesignToken.FontSizeHeading5" />, the matching
    /// <see cref="DesignToken.RelativeLineHeight" /> tokens and <see cref="DesignToken.FontHeight" />,
    /// <see cref="DesignToken.FontHeightSM" />, <see cref="DesignToken.FontHeightLG" />.
    /// </param>
    public static void CalculateFontMapTokenValues(DesignToken designToken)
    {
        var fontSizePairs = CalculateFontSize(designToken.FontSize);

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

    /// <summary>
    /// Derives the small, extra small and large control heights of a global token object from its control height.
    /// </summary>
    /// <param name="designToken">
    /// The token object to update in place. Sets <see cref="DesignToken.ControlHeightSM" />,
    /// <see cref="DesignToken.ControlHeightXS" /> and <see cref="DesignToken.ControlHeightLG" /> to 0.75, 0.5 and
    /// 1.25 times <see cref="DesignToken.ControlHeight" />.
    /// </param>
    public static void CalculateControlHeightMapTokenValues(DesignToken designToken)
    {
        var controlHeight = designToken.ControlHeight;
        designToken.ControlHeightSM = controlHeight * 0.75;
        designToken.ControlHeightXS = controlHeight * 0.5;
        designToken.ControlHeightLG = controlHeight * 1.25;
    }

    /// <summary>
    /// Returns the line height ratio for a font size: the font size plus 8, divided by the font size.
    /// </summary>
    /// <param name="fontSize">
    /// The font size.
    /// </param>
    /// <returns>
    /// The line height as a multiple of the font size, not as a length.
    /// </returns>
    public static double CalculateLineHeight(double fontSize)
    {
        return (fontSize + 8) / fontSize;
    }

    /// <summary>
    /// Derives the five corner radius tiers from a base radius.
    /// </summary>
    /// <param name="radiusBase">
    /// The base corner radius.
    /// </param>
    /// <returns>
    /// The base radius and its extra small, small, large and outer tiers. SM and LG equal the base radius below 5,
    /// outer at 4 or below, and XS below 2.
    /// </returns>
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

    /// <summary>
    /// Builds a ten-step font size scale around a base size, each step with its line height ratio.
    /// </summary>
    /// <param name="baseValue">
    /// The base font size. It is returned unchanged at index 1.
    /// </param>
    /// <returns>
    /// Ten pairs ordered from the smallest to the largest size. The other sizes are the base size times
    /// e^((index - 1) / 5), rounded up below the base, down above it, and then down to an even number.
    /// </returns>
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
