using System.Globalization;
using System.Text.RegularExpressions;

namespace AtomUI.Generator;

// Generator-only literal parsing. Values contain primitives used to emit standard C# expressions.
internal sealed class ThemeLiteralValue
{
    internal ThemeLiteralValue(object value, string canonicalValue) { Value = value; CanonicalValue = canonicalValue; }
    internal object Value { get; }
    internal string CanonicalValue { get; }
}
internal static partial class ThemeLiteralCodec
{
    internal const string Identity = "atomui-literal-v1.1";
    internal static string NormalizeType(string type)
    {
        type = type.Replace("global::", "");
        if (type.EndsWith("?", StringComparison.Ordinal)) { type = type.Substring(0, type.Length - 1); }
        return type;
    }
    internal static bool Supports(string type)
    {
        var nullable = type.EndsWith("?", StringComparison.Ordinal) || type.Replace("global::", "").StartsWith("System.Nullable<", StringComparison.Ordinal);
        type = NormalizeType(type);
        if (type.StartsWith("System.Nullable<", StringComparison.Ordinal)) { type = type.Substring(16, type.Length - 17); }
        if (nullable && type is not ("Avalonia.Media.Color" or "AtomUI.TextDecorationInfo" or "System.String" or
            "Avalonia.Media.FontFamily" or "Avalonia.Animation.Easings.Easing" or "Avalonia.Media.IBrush" or
            "Avalonia.Media.IImmutableBrush" or "Avalonia.Media.SolidColorBrush" or "Avalonia.Media.Immutable.ImmutableTransform"))
        {
            return false;
        }
        return type is "System.String" or "System.Int32" or "System.Double" or "System.Single" or "System.Boolean" or "System.TimeSpan" or
            "Avalonia.Media.FontFamily" or "Avalonia.Media.Color" or "Avalonia.Thickness" or "Avalonia.CornerRadius" or "Avalonia.Point" or "Avalonia.Size" or
            "AtomUI.TextDecorationInfo" or "Avalonia.Media.BoxShadow" or "Avalonia.Media.BoxShadows" or "AtomUI.Dimension" or "Avalonia.Animation.Easings.Easing" or "Avalonia.Media.IBrush" or "Avalonia.Media.IImmutableBrush" or
            "Avalonia.Media.SolidColorBrush" or "Avalonia.Media.Immutable.ImmutableTransform" or
            "Avalonia.Media.FontWeight" or "Avalonia.Media.PenLineCap" or "Avalonia.Media.PenLineJoin" or "AtomUI.LineStyle";
    }
    internal static ThemeLiteralValue Parse(string type, string text)
    {
        type = NormalizeType(type);
        if (type.StartsWith("System.Nullable<", StringComparison.Ordinal)) { type = type.Substring(16, type.Length - 17); }
        switch (type)
        {
            case "AtomUI.TextDecorationInfo":
                var decoration = ParseTextDecoration(text);
                return new ThemeLiteralValue(decoration, FormatTextDecoration(decoration));
            case "Avalonia.Media.BoxShadow":
                var shadow = ParseShadow(text);
                return new ThemeLiteralValue(shadow, FormatShadow(shadow));
            case "Avalonia.Media.BoxShadows":
                var shadows = ParseShadows(text);
                return new ThemeLiteralValue(shadows, shadows.Length == 0 ? "none" : string.Join(", ", shadows.Select(FormatShadow)));
            case "Avalonia.Media.FontFamily":
                return new ThemeLiteralValue(text, FormatFontFamily(text));
            case "System.String": return new ThemeLiteralValue(text, text);
            case "System.Int32":
                var integer = int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
                return new ThemeLiteralValue(integer, integer.ToString(CultureInfo.InvariantCulture));
            case "System.Double":
                var number = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                return new ThemeLiteralValue(number, number.ToString("R", CultureInfo.InvariantCulture));
            case "System.Single":
                var single = float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                return new ThemeLiteralValue(single, single.ToString("R", CultureInfo.InvariantCulture));
            case "System.Boolean":
                if (!string.Equals(text, "true", StringComparison.InvariantCultureIgnoreCase) && !string.Equals(text, "false", StringComparison.InvariantCultureIgnoreCase)) { throw new FormatException("Invalid Boolean."); }
                var boolean = string.Equals(text, "true", StringComparison.InvariantCultureIgnoreCase);
                return new ThemeLiteralValue(boolean, boolean ? "true" : "false");
            case "System.TimeSpan":
                var duration = TimeSpan.ParseExact(text, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None);
                return new ThemeLiteralValue(duration.Ticks, duration.ToString("c", CultureInfo.InvariantCulture));
            case "Avalonia.Media.Color":
            case "Avalonia.Media.IBrush":
            case "Avalonia.Media.IImmutableBrush":
            case "Avalonia.Media.SolidColorBrush":
                var color = ParseColor(text);
                return new ThemeLiteralValue(color, FormatColor(color));
            case "Avalonia.Thickness":
            case "Avalonia.CornerRadius":
            case "Avalonia.Point":
            case "Avalonia.Size":
            case "Avalonia.Media.Immutable.ImmutableTransform":
                var values = type is "Avalonia.Thickness" or "Avalonia.CornerRadius" ? ParseGeometryNumbers(text) : ParseNumbers(text);
                var pair = type is "Avalonia.Point" or "Avalonia.Size";
                if (pair && values.Length != 2) { throw new FormatException("Expected two coordinates."); }
                if (type == "Avalonia.Media.Immutable.ImmutableTransform")
                {
                    if (values.Length != 6) { throw new FormatException("Expected six matrix components."); }
                    // ImmutableTransform retains the existing object identity spelling.
                    return new ThemeLiteralValue(values, "Avalonia.Media.Immutable.ImmutableTransform");
                }
                if (!pair)
                {
                    values = values.Length switch
                    {
                        1 => new[] { values[0], values[0], values[0], values[0] },
                        2 when type == "Avalonia.Thickness" => new[] { values[0], values[1], values[0], values[1] },
                        2 => new[] { values[0], values[0], values[1], values[1] },
                        4 => values,
                        _ => throw new FormatException("Expected one, two or four components.")
                    };
                }
                return new ThemeLiteralValue(values, string.Join(pair ? ", " : ",", values.Select(General)));
            case "AtomUI.Dimension":
                var match = Regex.Match(text.Trim(), @"^([+-]?(?:[0-9]+(?:\.[0-9]+)?|\.[0-9]+))\s*([%a-zA-Z]*)$");
                if (!match.Success) { throw new FormatException("Invalid Dimension."); }
                var size = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                var percent = match.Groups[2].Value == "%";
                if (percent && (double.IsNaN(size) || double.IsInfinity(size) || size < 0 || size > 100)) { throw new FormatException("Percentage must be in [0, 100]."); }
                return new ThemeLiteralValue(new[] { size, percent ? 0d : 1d }, General(size) + (percent ? "%" : "px"));
            case "Avalonia.Animation.Easings.Easing":
                if (text.IndexOf(',') >= 0)
                {
                    var spline = ParseNumbers(text);
                    if (spline.Length != 4) { throw new FormatException("Invalid key spline."); }
                    return new ThemeLiteralValue(spline, "Spline(" + string.Join(",", spline.Select(n => n.ToString("R", CultureInfo.InvariantCulture))) + ")");
                }
                if (!StatelessEasings.Contains(text, StringComparer.Ordinal)) { throw new FormatException("Unsupported static easing. Use a standard easing name or four spline coordinates."); }
                return new ThemeLiteralValue(text, "Avalonia.Animation.Easings." + text);
            default: throw new NotSupportedException("No static literal codec exists for '" + type + "'.");
        }
    }
    internal static double[] ParseNumbers(string text) => ReadNumericTokens(text).Select(n => double.Parse(n, NumberStyles.Float, CultureInfo.InvariantCulture)).ToArray();
    private static double[] ParseGeometryNumbers(string text)
    {
        var tokens = ReadNumericTokens(text);
        var values = new List<double>();
        for (var i = 0; i < tokens.Length; i++)
        {
            if (double.TryParse(tokens[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var value)) { values.Add(value); }
            else if ((i == 1 || i == 2) && i == tokens.Length - 1) { return values.ToArray(); }
            else { throw new FormatException("Invalid geometry component."); }
        }
        return values.ToArray();
    }
    private static string[] ReadNumericTokens(string text)
    {
        var numbers = new List<string>();
        var index = 0;
        while (index < text.Length && char.IsWhiteSpace(text[index])) { index++; }
        while (index < text.Length)
        {
            var start = index;
            while (index < text.Length && text[index] != ',' && !char.IsWhiteSpace(text[index])) { index++; }
            if (index == start) { throw new FormatException("Empty numeric component."); }
            numbers.Add(text.Substring(start, index - start));
            var commas = 0;
            while (index < text.Length && (text[index] == ',' || char.IsWhiteSpace(text[index])))
            {
                if (text[index] == ',') { commas++; }
                if (commas > 1) { throw new FormatException("Repeated numeric separator."); }
                index++;
            }
            if (commas != 0 && index == text.Length) { throw new FormatException("Trailing numeric separator."); }
        }
        return numbers.ToArray();
    }
    private static string General(double value) => value.ToString(CultureInfo.InvariantCulture);
    internal static uint ParseColor(string text)
    {
        if (text.Length == 0) { throw new FormatException("Invalid color."); }
        if (text[0] == '#')
        {
            var hex = text.Substring(1);
            if (hex.Length is 3 or 4) { hex = string.Concat(hex.Select(c => new string(c, 2))); }
            if (hex.Length is not 6 and not 8) { throw new FormatException("Invalid hex color."); }
            return uint.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture) | (hex.Length == 6 ? 0xff000000u : 0u);
        }
        if (KnownColors.TryGetValue(text, out var known)) { return known; }
        var trimmed = text.Trim();
        var rgba = trimmed.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase);
        if ((rgba || trimmed.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase)) && trimmed.EndsWith(")", StringComparison.Ordinal))
        {
            var offset = rgba ? 5 : 4;
            var channels = trimmed.Substring(offset, trimmed.Length - offset - 1).Split(',');
            if (channels.Length is not 3 and not 4) { throw new FormatException("Invalid CSS color."); }
            byte Byte(string s)
            {
                var index = s.IndexOf('%');
                return index >= 0 ? unchecked((byte)Math.Round(double.Parse(s.Substring(0, index), NumberStyles.Number, CultureInfo.InvariantCulture) / 100d * 255d)) : byte.Parse(s, NumberStyles.Number, CultureInfo.InvariantCulture);
            }
            var alpha = 255;
            if (channels.Length == 4)
            {
                var index = channels[3].IndexOf('%');
                var a = double.Parse(index >= 0 ? channels[3].Substring(0, index) : channels[3], NumberStyles.Number, CultureInfo.InvariantCulture);
                alpha = unchecked((byte)Math.Round((index >= 0 ? a / 100d : a) * 255d));
            }
            return ((uint)alpha << 24) | ((uint)Byte(channels[0]) << 16) | ((uint)Byte(channels[1]) << 8) | Byte(channels[2]);
        }
        if (TryParseHueColor(text, out var hueColor)) { return hueColor; }
        throw new FormatException("Unsupported static color spelling. Supported forms: named, hex, rgb()/rgba(), hsl()/hsla(), hsv()/hsva().");
    }
    internal static string FormatColor(uint value)
    {
        foreach (var item in KnownColors) { if (item.Value == value) { return item.Key; } }
        return "#" + value.ToString("x8", CultureInfo.InvariantCulture);
    }
    private static readonly string[] StatelessEasings =
    {
        "LinearEasing", "BackEaseIn", "BackEaseOut", "BackEaseInOut", "BounceEaseIn", "BounceEaseOut", "BounceEaseInOut",
        "CircularEaseIn", "CircularEaseOut", "CircularEaseInOut", "CubicEaseIn", "CubicEaseOut", "CubicEaseInOut",
        "ElasticEaseIn", "ElasticEaseOut", "ElasticEaseInOut", "ExponentialEaseIn", "ExponentialEaseOut", "ExponentialEaseInOut",
        "QuadraticEaseIn", "QuadraticEaseOut", "QuadraticEaseInOut", "QuarticEaseIn", "QuarticEaseOut", "QuarticEaseInOut",
        "QuinticEaseIn", "QuinticEaseOut", "QuinticEaseInOut", "SineEaseIn", "SineEaseOut", "SineEaseInOut"
    };
}
