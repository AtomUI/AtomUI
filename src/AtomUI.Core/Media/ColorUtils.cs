using AtomUI.Utils;
using Avalonia.Media;

namespace AtomUI.Media;

/// <summary>
/// The WCAG 2 conformance level that decides how much contrast two colors need to be readable together.
/// </summary>
public enum WCAG2Level
{
    /// <summary>
    /// The lower level: the contrast ratio has to be at least 4.5 for small text and 3 for large text.
    /// </summary>
    AA,
    /// <summary>
    /// The higher level: the contrast ratio has to be at least 7 for small text and 4.5 for large text.
    /// </summary>
    AAA
}

/// <summary>
/// The size class of the text a readability check is made for. Large text is accepted with less contrast.
/// </summary>
public enum WCAG2Size
{
    /// <summary>
    /// Large text, which is allowed a lower contrast ratio: 3 at level AA and 4.5 at level AAA.
    /// </summary>
    Large,
    /// <summary>
    /// Text that is not large, the default: 4.5 at level AA and 7 at level AAA.
    /// </summary>
    Small
}

/// <summary>
/// The contrast requirement of a readability check: a WCAG 2 conformance level and the size class of the text.
/// </summary>
public class WCAG2Parms
{
    /// <summary>
    /// The conformance level the contrast ratio has to meet. The default is <see cref="WCAG2Level.AA" />.
    /// </summary>
    public WCAG2Level Level { get; set; } = WCAG2Level.AA;
    /// <summary>
    /// The size class of the text being checked. The default is <see cref="WCAG2Size.Small" />.
    /// </summary>
    public WCAG2Size Size { get; set; } = WCAG2Size.Small;
}

/// <summary>
/// The parameters of <c>MostReadable</c>: a contrast requirement plus a switch that allows a fallback
/// color when no candidate meets it.
/// </summary>
public class WCAG2FallbackParms : WCAG2Parms
{
    /// <summary>
    /// Controls what is returned when the most readable candidate still fails the contrast check.
    /// <see langword="true" /> returns white or black instead, whichever contrasts more with the base color.
    /// <see langword="false" />, the default, returns the most readable candidate regardless.
    /// </summary>
    public bool IncludeFallbackColors { get; set; }
}

/// <summary>
/// Color helpers for theme tokens and controls that draw themselves: building colors from normalized channels,
/// compositing, contrast checks and parsing CSS color expressions.
/// </summary>
public static class ColorUtils
{
    private static readonly Color[] s_readabilityFallbackColors =
    [
        Color.Parse("#fff"),
        Color.Parse("#000")
    ];

    /// <summary>
    /// Creates a color from four channels given as fractions from 0 to 1.
    /// </summary>
    /// <param name="alpha">
    /// The opacity: 0 is fully transparent and 1 is fully opaque.
    /// </param>
    /// <param name="red">
    /// The red channel.
    /// </param>
    /// <param name="green">
    /// The green channel.
    /// </param>
    /// <param name="blue">
    /// The blue channel.
    /// </param>
    /// <returns>
    /// The color, with each channel multiplied by 255 and rounded to the nearest byte. Values outside the range 0 to
    /// 1 are not clamped.
    /// </returns>
    public static Color FromRgbF(double alpha, double red, double green, double blue)
    {
        return Color.FromArgb((byte)Math.Round(alpha * 255d),
            (byte)Math.Round(red * 255d),
            (byte)Math.Round(green * 255d),
            (byte)Math.Round(blue * 255d));
    }

    /// <summary>
    /// Creates an opaque color from three channels given as fractions from 0 to 1. Same as
    /// <see cref="FromRgbF(double, double, double, double)" /> with an alpha of 1.
    /// </summary>
    public static Color FromRgbF(double red, double green, double blue)
    {
        return FromRgbF(1d, red, green, blue);
    }

    /// <summary>
    /// Returns white with an alpha of 0, a fully transparent color.
    /// </summary>
    public static Color TransparentColor()
    {
        return Color.FromArgb(0, 255, 255, 255);
    }

    /// <summary>
    /// Parses a color string and returns the color with its HSL saturation lowered by <paramref name="amount" />
    /// percentage points.
    /// </summary>
    /// <param name="color">
    /// The color to adjust, in any format that <see cref="Color.Parse(string)" /> accepts.
    /// </param>
    /// <param name="amount">
    /// The change in percentage points, clamped to 0 to 100.
    /// </param>
    /// <returns>
    /// The adjusted color. It is opaque, whatever alpha the input had.
    /// </returns>
    public static Color Desaturate(string color, int amount = 10)
    {
        return Color.Parse(color).Desaturate(amount);
    }

    /// <summary>
    /// Parses a color string and returns the color with its HSL saturation raised by <paramref name="amount" />
    /// percentage points.
    /// </summary>
    /// <param name="color">
    /// The color to adjust, in any format that <see cref="Color.Parse(string)" /> accepts.
    /// </param>
    /// <param name="amount">
    /// The change in percentage points, clamped to 0 to 100.
    /// </param>
    /// <returns>
    /// The adjusted color. It is opaque, whatever alpha the input had.
    /// </returns>
    public static Color Saturate(string color, int amount = 10)
    {
        return Color.Parse(color).Saturate(amount);
    }

    /// <summary>
    /// Parses a color string and returns the color with its HSL lightness raised by <paramref name="amount" />
    /// percentage points.
    /// </summary>
    /// <param name="color">
    /// The color to adjust, in any format that <see cref="Color.Parse(string)" /> accepts.
    /// </param>
    /// <param name="amount">
    /// The change in percentage points, clamped to 0 to 100.
    /// </param>
    /// <returns>
    /// The adjusted color. It is opaque, whatever alpha the input had.
    /// </returns>
    public static Color Lighten(string color, int amount = 10)
    {
        return Color.Parse(color).Lighten(amount);
    }

    /// <summary>
    /// Parses a color string and returns the color with each RGB channel raised by <paramref name="amount" /> percent
    /// of the 0 to 255 range.
    /// </summary>
    /// <param name="color">
    /// The color to adjust, in any format that <see cref="Color.Parse(string)" /> accepts.
    /// </param>
    /// <param name="amount">
    /// The change in percent of the full channel range, clamped to 0 to 100.
    /// </param>
    /// <returns>
    /// The adjusted color. It is opaque, whatever alpha the input had.
    /// </returns>
    public static Color Brighten(string color, int amount = 10)
    {
        return Color.Parse(color).Brighten(amount);
    }

    /// <summary>
    /// Parses a color string and returns the color with its HSL lightness lowered by <paramref name="amount" />
    /// percentage points.
    /// </summary>
    /// <param name="color">
    /// The color to adjust, in any format that <see cref="Color.Parse(string)" /> accepts.
    /// </param>
    /// <param name="amount">
    /// The change in percentage points, clamped to 0 to 100.
    /// </param>
    /// <returns>
    /// The adjusted color. It is opaque, whatever alpha the input had.
    /// </returns>
    public static Color Darken(string color, int amount = 10)
    {
        return Color.Parse(color).Darken(amount);
    }

    /// <summary>
    /// Parses a color string and returns the color with its HSL hue rotated by <paramref name="amount" /> degrees.
    /// </summary>
    /// <param name="color">
    /// The color to adjust, in any format that <see cref="Color.Parse(string)" /> accepts.
    /// </param>
    /// <param name="amount">
    /// The rotation in degrees. It is not clamped, so negative values lower the hue.
    /// </param>
    /// <returns>
    /// The adjusted color. It is opaque, whatever alpha the input had.
    /// </returns>
    public static Color Spin(string color, int amount = 10)
    {
        return Color.Parse(color).Spin(amount);
    }

    /// <summary>
    /// Composites a front color over a background color and returns the visible result as a single color.
    /// </summary>
    /// <param name="frontColor">
    /// The color drawn on top. Its alpha decides how much of the background shows through it.
    /// </param>
    /// <param name="backgroundColor">
    /// The color behind the front color.
    /// </param>
    /// <returns>
    /// A color with the channels of the composite and an alpha that combines the opacity of both colors.
    /// </returns>
    public static Color OnBackground(in Color frontColor, in Color backgroundColor)
    {
        var fr = frontColor.GetRedF();
        var fg = frontColor.GetGreenF();
        var fb = frontColor.GetBlueF();
        var fa = frontColor.GetAlphaF();

        var br = backgroundColor.GetRedF();
        var bg = backgroundColor.GetGreenF();
        var bb = backgroundColor.GetBlueF();
        var ba = backgroundColor.GetAlphaF();

        var alpha = fa + ba * (1 - fa);

        var nr = (fr * fa + br * ba * (1 - fa)) / alpha;
        var ng = (fg * fa + bg * ba * (1 - fa)) / alpha;
        var nb = (fb * fa + bb * ba * (1 - fa)) / alpha;
        var na = alpha;

        return FromRgbF(na, nr, ng, nb);
    }

    private static bool IsStableColor(int color)
    {
        return color >= 0 && color <= 255;
    }
    
    private static bool IsStableColor(double color)
    {
        return MathUtils.GreaterThanOrClose(color, 0.0d) && MathUtils.LessThanOrClose(color, 255.0d);
    }
    
    /// <summary>
    /// Computes the relative luminance of a color on a scale from 0, black, to 1, white. The alpha channel is ignored.
    /// </summary>
    /// <param name="color">
    /// The color to measure.
    /// </param>
    /// <returns>
    /// The relative luminance in the range 0 to 1.
    /// </returns>
    /// <remarks>
    /// This is an approximation of the WCAG 2 relative luminance. <see cref="Readability" /> uses the exact WCAG
    /// curve of <see cref="ColorExtensions.GetLuminance(Color)" /> instead.
    /// </remarks>
    public static double GetRelativeLuminance(Color color)
    {
        // The equation for relative luminance is given by
        //
        // L = 0.2126 * Rg + 0.7152 * Gg + 0.0722 * Bg
        //
        // where Xg = { X/3294 if X <= 10, (R/269 + 0.0513)^2.4 otherwise }
        //
        // If L is closer to 1, then the color is closer to white; if it is closer to 0,
        // then the color is closer to black.  This is based on the fact that the human
        // eye perceives green to be much brighter than red, which in turn is perceived to be
        // brighter than blue.

        double rg = color.R <= 10 ? color.R / 3294.0 : Math.Pow(color.R / 269.0 + 0.0513, 2.4);
        double gg = color.G <= 10 ? color.G / 3294.0 : Math.Pow(color.G / 269.0 + 0.0513, 2.4);
        double bg = color.B <= 10 ? color.B / 3294.0 : Math.Pow(color.B / 269.0 + 0.0513, 2.4);

        return (0.2126 * rg + 0.7152 * gg + 0.0722 * bg);
    }
    
    /// <summary>
    /// Determines whether a color is perceived as bright, that is its relative luminance is greater than 0.5.
    /// </summary>
    /// <param name="color">
    /// The color to test.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the relative luminance is greater than 0.5; otherwise <see langword="false" />.
    /// </returns>
    public static bool IsBright(Color color)
    {
        var lum = GetRelativeLuminance(color);
        return lum > 0.5;
    }
    
    /// <summary>
    /// Determines whether a color is perceived as bright after it is composited over a background color, which
    /// matters when the color is translucent.
    /// </summary>
    /// <param name="color">
    /// The color to test.
    /// </param>
    /// <param name="bgColor">
    /// The background the color is composited over with <see cref="OnBackground" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the composited color has a relative luminance greater than 0.5; otherwise
    /// <see langword="false" />.
    /// </returns>
    public static bool IsBright(Color color, Color bgColor)
    {
        var resultColor = OnBackground(color, bgColor);
        var lum         = GetRelativeLuminance(resultColor);
        return lum > 0.5;
    }

    /// <summary>
    /// Converts an opaque color into a semi-transparent color that reproduces it when drawn over a background color,
    /// using the lowest alpha that can do so.
    /// </summary>
    /// <param name="frontColor">
    /// The color to reproduce.
    /// </param>
    /// <param name="backgroundColor">
    /// The background the returned color will be drawn over.
    /// </param>
    /// <returns>
    /// A color whose alpha starts at 0.01 and increases in steps of 0.01 until its rounded channels lie in the range
    /// 0 to 255. <paramref name="frontColor" /> itself when it already has an alpha below 1 or when no step works.
    /// </returns>
    public static Color CalculateAlphaColor(in Color frontColor, in Color backgroundColor)
    {
        var fR          = frontColor.R;
        var fG          = frontColor.G;
        var fB          = frontColor.B;
        var originAlpha = frontColor.GetAlphaF();
        if (originAlpha < 1d)
        {
            return frontColor;
        }

        var bR = backgroundColor.R;
        var bG = backgroundColor.G;
        var bB = backgroundColor.B;

        for (var fA = 0.01d; fA <= 1.0d; fA += 0.01d)
        {
            var r = Math.Round((fR - bR * (1d - fA)) / fA);
            var g = Math.Round((fG - bG * (1d - fA)) / fA);
            var b = Math.Round((fB - bB * (1d - fA)) / fA);
            if (IsStableColor(r) && IsStableColor(g) && IsStableColor(b))
            {
                return Color.FromArgb((byte)(Math.Round(fA * 100d) / 100d * 255), (byte)r, (byte)g, (byte)b);
            }
        }

        // No alpha below 1 can reproduce the front color on this background, as with black on white,
        // so the front color is kept opaque.
        return Color.FromArgb(255, fR, fG, fB);
    }

    /// <summary>
    /// Parses a color written as a hexadecimal string or in <c>rgb()</c> or <c>rgba()</c> notation.
    /// </summary>
    /// <param name="colorExpr">
    /// The expression to parse, in one of the forms <see cref="TryParseCssRgbColor" /> accepts.
    /// </param>
    /// <returns>
    /// The parsed color.
    /// </returns>
    /// <exception cref="FormatException">
    /// The expression is not a well-formed color.
    /// </exception>
    public static Color ParseCssRgbColor(string colorExpr)
    {
        if (TryParseCssRgbColor(colorExpr, out var color))
        {
            return color;
        }

        throw new FormatException($"Invalid color string: '{colorExpr}'.");
    }

    /// <summary>
    /// Tries to parse a color written as a hexadecimal string or in <c>rgb()</c> or <c>rgba()</c> notation.
    /// </summary>
    /// <param name="colorExpr">
    /// The expression to parse. The <c>rgb</c> and <c>rgba</c> keywords are matched case-insensitively.
    /// </param>
    /// <param name="color">
    /// The parsed color when the method returns <see langword="true" />; otherwise <see langword="default" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the expression was parsed. <see langword="false" /> when
    /// <paramref name="colorExpr" /> is <see langword="null" />, empty or not one of these notations, or when its
    /// values are not integers in the expected count.
    /// </returns>
    /// <remarks>
    /// A hexadecimal string is passed to <see cref="Color.TryParse(ReadOnlySpan{char}, out Color)" />. An
    /// <c>rgb()</c> expression needs three comma-separated integers and an <c>rgba()</c> expression four, the last
    /// being an alpha from 0 to 255. Integers outside 0 to 255 are truncated to a byte instead of being rejected.
    /// </remarks>
    public static bool TryParseCssRgbColor(string? colorExpr, out Color color)
    {
        color = default;
        if (string.IsNullOrEmpty(colorExpr))
        {
            return false;
        }

        if (colorExpr[0] == '#')
        {
            return Color.TryParse(colorExpr.AsSpan(), out color);
        }

        var isRgba = colorExpr.StartsWith("rgba", StringComparison.InvariantCultureIgnoreCase);
        var isRgb  = false;
        if (!isRgba)
        {
            isRgb = colorExpr.StartsWith("rgb", StringComparison.InvariantCultureIgnoreCase);
        }

        if (isRgb || isRgba)
        {
            var leftParen  = colorExpr.IndexOf('(');
            var rightParen = colorExpr.IndexOf(')');
            if (leftParen == -1 || rightParen == -1 || rightParen < leftParen)
            {
                return false;
            }

            var valueSpan = colorExpr.AsSpan(leftParen + 1, rightParen - leftParen - 1);
            return TryParseCssRgbValues(valueSpan, isRgb, out color);
        }

        return false;
    }

    private static bool TryParseCssRgbValues(ReadOnlySpan<char> valueSpan, bool isRgb, out Color color)
    {
        color = default;

        Span<int> rgbaValues = stackalloc int[4];
        var       valueCount = 0;
        var       startIndex = 0;
        for (var i = 0; i <= valueSpan.Length; i++)
        {
            if (i != valueSpan.Length && valueSpan[i] != ',')
            {
                continue;
            }

            var part = valueSpan.Slice(startIndex, i - startIndex);
            startIndex = i + 1;
            if (part.IsEmpty)
            {
                continue;
            }

            if (valueCount == rgbaValues.Length || !int.TryParse(part, out var partValue))
            {
                return false;
            }

            rgbaValues[valueCount++] = partValue;
        }

        if (isRgb)
        {
            if (valueCount != 3)
            {
                return false;
            }

            rgbaValues[3] = 255;
        }
        else if (valueCount != 4)
        {
            return false;
        }

        color = Color.FromArgb((byte)rgbaValues[0], (byte)rgbaValues[1], (byte)rgbaValues[2], (byte)rgbaValues[3]);
        return true;
    }

    /// <summary>
    /// Calculates the WCAG 2 contrast ratio between two colors.
    /// </summary>
    /// <param name="color1">
    /// The first color.
    /// </param>
    /// <param name="color2">
    /// The second color.
    /// </param>
    /// <returns>
    /// The contrast ratio between <paramref name="color1" /> and <paramref name="color2" />.
    /// </returns>
    /// <remarks>
    /// Based on the
    /// <see href="https://www.w3.org/TR/2008/REC-WCAG20-20081211/#contrast-ratiodef">WCAG 2 contrast ratio definition</see>.
    /// </remarks>
    public static double Readability(Color color1, Color color2)
    {
        return (Math.Max(color1.GetLuminance(), color2.GetLuminance()) + 0.05) /
               (Math.Min(color1.GetLuminance(), color2.GetLuminance()) + 0.05);
    }

    /// <summary>
    /// Determines whether two colors contrast enough to be read together under WCAG 2.
    /// </summary>
    /// <param name="color1">
    /// The first color of the pair.
    /// </param>
    /// <param name="color2">
    /// The second color of the pair. The order of the two colors does not matter.
    /// </param>
    /// <param name="wcag2">
    /// The conformance level and text size that set the minimum contrast ratio. <see langword="null" /> selects level
    /// AA with small text.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the ratio from <see cref="Readability" /> is at least the threshold of the level
    /// and size: 4.5 for AA and 7 for AAA with small text, 3 for AA and 4.5 for AAA with large text. Otherwise
    /// <see langword="false" />.
    /// </returns>
    public static bool IsReadable(Color color1, Color color2, WCAG2Parms? wcag2 = null)
    {
        wcag2 ??= new WCAG2Parms();
        var readabilityLevel = Readability(color1, color2);
        if (wcag2.Level == WCAG2Level.AA)
        {
            if (wcag2.Size == WCAG2Size.Large)
            {
                return MathUtils.GreaterThanOrClose(readabilityLevel, 3);
            }

            return MathUtils.GreaterThanOrClose(readabilityLevel, 4.5);
        }

        if (wcag2.Level == WCAG2Level.AAA)
        {
            if (wcag2.Size == WCAG2Size.Large)
            {
                return MathUtils.GreaterThanOrClose(readabilityLevel, 4.5);
            }

            return MathUtils.GreaterThanOrClose(readabilityLevel, 7);
        }

        return false;
    }

    /// <summary>
    /// Returns the color from a list that contrasts most with a base color.
    /// </summary>
    /// <param name="baseColor">
    /// The color the candidates are compared with.
    /// </param>
    /// <param name="colorList">
    /// The colors to choose from. The first one wins when several have the same contrast ratio.
    /// </param>
    /// <param name="args">
    /// The contrast requirement for the result and the fallback switch. <see langword="null" /> selects level AA with
    /// small text and no fallback.
    /// </param>
    /// <returns>
    /// The candidate with the highest <see cref="Readability" /> ratio against <paramref name="baseColor" />. When
    /// <c>args.IncludeFallbackColors</c> is <see langword="true" /> and that candidate is not readable, whichever of
    /// white and black contrasts more with <paramref name="baseColor" /> is returned instead.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="colorList" /> is empty.
    /// </exception>
    /// <remarks>
    /// The fallback is returned even when it is not readable either, and taking it sets
    /// <c>args.IncludeFallbackColors</c> to <see langword="false" /> on the object that was passed in.
    /// </remarks>
    public static Color? MostReadable(Color baseColor, List<Color> colorList, WCAG2FallbackParms? args = null)
    {
        args ??= new WCAG2FallbackParms
        {
            IncludeFallbackColors = false,
            Level                 = WCAG2Level.AA,
            Size                  = WCAG2Size.Small
        };

        return MostReadableCore(baseColor, colorList, args);
    }

    private static Color? MostReadableCore(Color baseColor, IReadOnlyList<Color> colorList, WCAG2FallbackParms args)
    {
        Color? bestColor = null;
        var    bestScore = 0d;
        for (var i = 0; i < colorList.Count; ++i)
        {
            var color = colorList[i];
            var score = Readability(baseColor, color);
            if (score > bestScore)
            {
                bestScore = score;
                bestColor = color;
            }
        }

        if (IsReadable(baseColor, bestColor!.Value, args) ||
            !args.IncludeFallbackColors)
        {
            return bestColor;
        }

        args.IncludeFallbackColors = false;
        return MostReadableCore(baseColor, s_readabilityFallbackColors, args);
    }
    
    /// <summary>
    /// Converts a color given as normalized RGBA channels to HSV. The channels are expected in the range 0 to 1 and
    /// are not clamped.
    /// </summary>
    /// <param name="r">
    /// The red channel.
    /// </param>
    /// <param name="g">
    /// The green channel.
    /// </param>
    /// <param name="b">
    /// The blue channel.
    /// </param>
    /// <param name="a">
    /// The alpha channel, which is copied to the result.
    /// </param>
    /// <returns>
    /// The color as an <see cref="HsvColor" />: hue in degrees from 0 up to 360, saturation and value from 0 to 1.
    /// A gray color gets a hue and a saturation of 0.
    /// </returns>
    public static HsvColor ToHsv(
        double r,
        double g,
        double b,
        double a = 1.0)
    {
        // Note: Conversion code is originally based on the C++ in WinUI (licensed MIT)
        // https://github.com/microsoft/microsoft-ui-xaml/blob/main/dev/Common/ColorConversion.cpp
        // This was used because it is the best documented and likely most optimized for performance
        // Alpha support was added

        double hue;
        double saturation;
        double value;

        double max = r >= g ? (r >= b ? r : b) : (g >= b ? g : b);
        double min = r <= g ? (r <= b ? r : b) : (g <= b ? g : b);

        // The value, a number between 0 and 1, is the largest of R, G, and B.
        // Conceptually speaking, it represents how much color is present.
        // If at least one of R, G, B is 255, then there exists as much color as there can be.
        // If RGB = (0, 0, 0), then there exists no color at all - a value of zero corresponds
        // to black (i.e., the absence of any color).
        value = max;

        // The "chroma" of the color is a value directly proportional to the extent to which
        // the color diverges from greyscale.  If, for example, we have RGB = (255, 255, 0),
        // then the chroma is maximized - this is a pure yellow, no gray of any kind.
        // On the other hand, if we have RGB = (128, 128, 128), then the chroma being zero
        // implies that this color is pure greyscale, with no actual hue to be found.
        var chroma = max - min;

        // If the chrome is zero, then hue is technically undefined - a greyscale color
        // has no hue.  For the sake of convenience, we'll just set hue to zero, since
        // it will be unused in this circumstance.  Since the color is purely gray,
        // saturation is also equal to zero - you can think of saturation as basically
        // a measure of hue intensity, such that no hue at all corresponds to a
        // nonexistent intensity.
        if (chroma == 0)
        {
            hue        = 0.0;
            saturation = 0.0;
        }
        else
        {
            // In this block, hue is properly defined, so we'll extract both hue
            // and saturation information from the RGB color.

            // Hue can be thought of as a cyclical thing, between 0 degrees and 360 degrees.
            // A hue of 0 degrees is red; 120 degrees is green; 240 degrees is blue; and 360 is back to red.
            // Every other hue is somewhere between either red and green, green and blue, and blue and red,
            // so every other hue can be thought of as an angle on this color wheel.
            // These if/else statements determines where on this color wheel our color lies.
            if (r == max)
            {
                // If the red channel is the most pronounced channel, then we exist
                // somewhere between (-60, 60) on the color wheel - i.e., the section around 0 degrees
                // where red dominates.  We figure out where in that section we are exactly
                // by considering whether the green or the blue channel is greater - by subtracting green from blue,
                // then if green is greater, we'll nudge ourselves closer to 60, whereas if blue is greater, then
                // we'll nudge ourselves closer to -60.  We then divide by chroma (which will actually make the result larger,
                // since chroma is a value between 0 and 1) to normalize the value to ensure that we get the right hue
                // even if we're very close to greyscale.
                hue = 60 * (g - b) / chroma;
            }
            else if (g == max)
            {
                // We do the exact same for the case where the green channel is the most pronounced channel,
                // only this time we want to see if we should tilt towards the blue direction or the red direction.
                // We add 120 to center our value in the green third of the color wheel.
                hue = 120 + (60 * (b - r) / chroma);
            }
            else // blue == max
            {
                // And we also do the exact same for the case where the blue channel is the most pronounced channel,
                // only this time we want to see if we should tilt towards the red direction or the green direction.
                // We add 240 to center our value in the blue third of the color wheel.
                hue = 240 + (60 * (r - g) / chroma);
            }

            // Since we want to work within the range [0, 360), we'll add 360 to any value less than zero -
            // this will bump red values from within -60 to -1 to 300 to 359.  The hue is the same at both values.
            if (hue < 0.0)
            {
                hue += 360.0;
            }

            // The saturation, our final HSV axis, can be thought of as a value between 0 and 1 indicating how intense our color is.
            // To find it, we divide the chroma - the distance between the minimum and the maximum RGB channels - by the maximum channel (i.e., the value).
            // This effectively normalizes the chroma - if the maximum is 0.5 and the minimum is 0, the saturation will be (0.5 - 0) / 0.5 = 1,
            // meaning that although this color is not as bright as it can be, the dark color is as intense as it possibly could be.
            // If, on the other hand, the maximum is 0.5 and the minimum is 0.25, then the saturation will be (0.5 - 0.25) / 0.5 = 0.5,
            // meaning that this color is partially washed out.
            // A saturation value of 0 corresponds to a greyscale color, one in which the color is *completely* washed out and there is no actual hue.
            saturation = chroma / value;
        }

        return new HsvColor(a, hue, saturation, value);
    }
}
