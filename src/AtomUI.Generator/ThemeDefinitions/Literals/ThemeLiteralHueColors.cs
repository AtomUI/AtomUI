using System;
using System.Globalization;

namespace AtomUI.Generator;

internal static partial class ThemeLiteralCodec
{
    // The build codec preserves Avalonia Color.Parse's HSL/HSV input and rounding rules.
    private static bool TryParseHueColor(string text, out uint color)
    {
        color = 0;
        if (text.Length < 10) { return false; }
        var input = text.Trim();
        var hsl = input.StartsWith("hsl(", StringComparison.OrdinalIgnoreCase) || input.StartsWith("hsla(", StringComparison.OrdinalIgnoreCase);
        var hsv = input.StartsWith("hsv(", StringComparison.OrdinalIgnoreCase) || input.StartsWith("hsva(", StringComparison.OrdinalIgnoreCase);
        if ((!hsl && !hsv) || !input.EndsWith(")", StringComparison.Ordinal)) { return false; }
        var open = input.IndexOf('(');
        var parts = input.Substring(open + 1, input.Length - open - 2).Split(',');
        if (parts.Length is not 3 and not 4 ||
            !double.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var hue) ||
            !TryHueComponent(parts[1], out var saturation) ||
            !TryHueComponent(parts[2], out var lightnessOrValue)) { return false; }
        var alpha = 1d;
        if (parts.Length == 4 && !TryHueComponent(parts[3], out alpha)) { return false; }
        hue = Clamp(hue, 0, 360);
        if (hue >= 360)
        {
            hue = 0;
        }
        saturation = Clamp(saturation, 0, 1);
        lightnessOrValue = Clamp(lightnessOrValue, 0, 1);
        alpha = Clamp(alpha, 0, 1);
        color = hsl
            ? ThemeLiteralColorConversion.HslToArgb(hue, saturation, lightnessOrValue, alpha)
            : ThemeLiteralColorConversion.HsvToArgb(hue, saturation, lightnessOrValue, alpha);
        return true;
    }

    private static bool TryHueComponent(string text, out double value)
    {
        var percent = text.IndexOf('%');
        var valid = double.TryParse(percent >= 0 ? text.Substring(0, percent) : text,
            NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        if (percent >= 0) { value /= 100; }
        return valid;
    }

    private static double Clamp(double value, double min, double max) => value < min ? min : value > max ? max : value;
}
