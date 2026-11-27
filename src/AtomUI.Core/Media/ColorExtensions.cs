using System.Globalization;
using AtomUI.Utils;
using Avalonia.Media;

namespace AtomUI.Media;

/// <summary>
/// Provides channel accessors and color adjustments for <see cref="Color" /> values.
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Returns the red channel as a value from 0 to 1.
    /// </summary>
    public static double GetRedF(this Color color)
    {
        return color.R / 255d;
    }

    /// <summary>
    /// Returns a copy of the color with its red channel set to <paramref name="value" />,
    /// clamped to the range 0 to 1.
    /// </summary>
    public static Color SetRedF(this Color color, double value)
    {
        value = Math.Clamp(value, 0.0, 1.0);
        return ColorUtils.FromRgbF(color.GetAlphaF(), value, color.GetGreenF(), color.GetBlueF());
    }

    /// <summary>
    /// Returns the green channel as a value from 0 to 1.
    /// </summary>
    public static double GetGreenF(this Color color)
    {
        return color.G / 255d;
    }

    /// <summary>
    /// Returns a copy of the color with its green channel set to <paramref name="value" />,
    /// clamped to the range 0 to 1.
    /// </summary>
    public static Color SetGreenF(this Color color, double value)
    {
        value = Math.Clamp(value, 0.0, 1.0);
        return ColorUtils.FromRgbF(color.GetAlphaF(), color.GetRedF(), value, color.GetBlueF());
    }

    /// <summary>
    /// Returns the blue channel as a value from 0 to 1.
    /// </summary>
    public static double GetBlueF(this Color color)
    {
        return color.B / 255d;
    }

    /// <summary>
    /// Returns a copy of the color with its blue channel set to <paramref name="value" />,
    /// clamped to the range 0 to 1.
    /// </summary>
    public static Color SetBlueF(this Color color, double value)
    {
        value = Math.Clamp(value, 0.0, 1.0);
        return ColorUtils.FromRgbF(color.GetAlphaF(), color.GetRedF(), color.GetGreenF(), value);
    }

    /// <summary>
    /// Returns the alpha channel as a value from 0 to 1.
    /// </summary>
    public static double GetAlphaF(this Color color)
    {
        return color.A / 255d;
    }

    /// <summary>
    /// Returns a copy of the color with its alpha channel set to <paramref name="value" />,
    /// clamped to the range 0 to 1.
    /// </summary>
    public static Color SetAlphaF(this Color color, double value)
    {
        value = Math.Clamp(value, 0.0, 1.0);
        return ColorUtils.FromRgbF(value, color.GetRedF(), color.GetGreenF(), color.GetBlueF());
    }

    /// <summary>
    /// Returns the color as a lowercase hexadecimal string: <c>#rrggbb</c> by default, or
    /// <c>#aarrggbb</c> when <paramref name="format" /> is <see cref="ColorNameFormat.HexArgb" />.
    /// </summary>
    public static string HexName(this Color color, ColorNameFormat format = ColorNameFormat.HexRgb)
    {
        var rgb       = color.ToUInt32();
        var formatStr = "x8";
        if (format == ColorNameFormat.HexRgb)
        {
            formatStr =  "x6";
            rgb       &= 0xFFFFFF;
        }

        return $"#{rgb.ToString(formatStr, CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Returns a copy of the color with its HSL saturation lowered by <paramref name="amount" />
    /// percentage points. <paramref name="amount" /> is clamped to 0 to 100, and the result is opaque.
    /// </summary>
    public static Color Desaturate(this Color color, int amount = 10)
    {
        amount = Math.Clamp(amount, 0, 100);
        var hslColor = color.ToHsl();
        var s        = hslColor.S;
        s -= amount / 100d;
        s =  Math.Clamp(s, 0d, 1d);
        return HslColor.FromHsl(hslColor.H, s, hslColor.L).ToRgb();
    }

    /// <summary>
    /// Returns a copy of the color with its HSL saturation raised by <paramref name="amount" />
    /// percentage points. <paramref name="amount" /> is clamped to 0 to 100, and the result is opaque.
    /// </summary>
    public static Color Saturate(this Color color, int amount = 10)
    {
        amount = Math.Clamp(amount, 0, 100);
        var hslColor = color.ToHsl();
        var s        = hslColor.S;
        s += amount / 100d;
        s =  Math.Clamp(s, 0d, 1d);
        return HslColor.FromHsl(hslColor.H, s, hslColor.L).ToRgb();
    }

    /// <summary>
    /// Returns a gray copy of the color that keeps its HSL lightness. The result is opaque.
    /// </summary>
    public static Color Greyscale(this Color color)
    {
        return color.Desaturate(100);
    }

    /// <summary>
    /// Returns a copy of the color with its HSL lightness raised by <paramref name="amount" />
    /// percentage points. <paramref name="amount" /> is clamped to 0 to 100, and the result is opaque.
    /// </summary>
    public static Color Lighten(this Color color, int amount = 10)
    {
        amount = Math.Clamp(amount, 0, 100);
        var hslColor = color.ToHsl();
        var l        = hslColor.L;
        l += amount / 100d;
        l =  Math.Clamp(l, 0d, 1d);
        return HslColor.FromHsl(hslColor.H, hslColor.S, l).ToRgb();
    }

    /// <summary>
    /// Returns a copy of the color with each RGB channel raised by <paramref name="amount" /> percent of the
    /// 0 to 255 range. <paramref name="amount" /> is clamped to 0 to 100, and the result is opaque.
    /// </summary>
    public static Color Brighten(this Color color, int amount = 10)
    {
        amount = Math.Clamp(amount, 0, 100);
        int r     = color.R;
        int g     = color.G;
        int b     = color.B;
        var delta = (int)Math.Round(255d * -(amount / 100d));
        r = Math.Max(0, Math.Min(255, r - delta));
        g = Math.Max(0, Math.Min(255, g - delta));
        b = Math.Max(0, Math.Min(255, b - delta));
        return Color.FromRgb((byte)r, (byte)g, (byte)b);
    }

    /// <summary>
    /// Returns a copy of the color with its HSL lightness lowered by <paramref name="amount" />
    /// percentage points. <paramref name="amount" /> is clamped to 0 to 100, and the result is opaque.
    /// </summary>
    public static Color Darken(this Color color, int amount = 10)
    {
        amount = Math.Clamp(amount, 0, 100);
        var hslColor = color.ToHsl();
        var l        = hslColor.L;
        l -= amount / 100d;
        l =  Math.Clamp(l, 0d, 1d);
        return HslColor.FromHsl(hslColor.H, hslColor.S, l).ToRgb();
    }

    /// <summary>
    /// Returns a copy of the color with its HSL hue rotated by <paramref name="amount" /> degrees.
    /// <paramref name="amount" /> is not clamped, so negative values lower the hue. The result is opaque.
    /// </summary>
    public static Color Spin(this Color color, int amount = 10)
    {
        var hslColor = color.ToHsl();
        var h        = hslColor.H;
        h = (h + amount) % 360;
        h = h < 0 ? 360 + h : h;
        return HslColor.FromHsl(h, hslColor.S, hslColor.L).ToRgb();
    }

    /// <summary>
    /// Returns the perceived brightness of the color, from 0-255.
    /// </summary>
    public static int GetBrightness(this Color color)
    {
        return (color.R * 299 + color.G * 587 + color.B * 114) / 1000;
    }

    /// <summary>
    /// Returns the perceived luminance of a color, from 0-1.
    /// </summary>
    public static double GetLuminance(this Color color)
    {
        // http://www.w3.org/TR/2008/REC-WCAG20-20081211/#relativeluminancedef
        var    rsRGB = color.R / 255d;
        var    gsRGB = color.G / 255d;
        var    bsRGB = color.B / 255d;
        double r     = 0;
        double g     = 0;
        double b     = 0;
        if (MathUtils.LessThanOrClose(rsRGB, 0.03928))
        {
            r = rsRGB / 12.92;
        }
        else
        {
            r = Math.Pow((rsRGB + 0.055) / 1.055, 2.4);
        }

        if (gsRGB <= 0.03928)
        {
            g = gsRGB / 12.92;
        }
        else
        {
            g = Math.Pow((gsRGB + 0.055) / 1.055, 2.4);
        }

        if (bsRGB <= 0.03928)
        {
            b = bsRGB / 12.92;
        }
        else
        {
            b = Math.Pow((bsRGB + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }
}