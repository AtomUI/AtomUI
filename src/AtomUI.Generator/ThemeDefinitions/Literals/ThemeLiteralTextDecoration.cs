using System.Globalization;

namespace AtomUI.Generator;

internal sealed class ThemeTextDecorationValue
{
    internal uint Color = 0xff808080;
    internal int LineType;
    internal int LineStyle;
    internal int Thickness = 1;
}
internal static partial class ThemeLiteralCodec
{
    internal static ThemeTextDecorationValue ParseTextDecoration(string text)
    {
        bool Contains(string value) => text.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0;
        var value = new ThemeTextDecorationValue();
        if (Contains("none")) { value.Thickness = 0; return value; }
        value.LineType = Contains("underline") ? 1 : Contains("overline") ? 2 : Contains("line-through") ? 3 : throw new FormatException("Unsupported decoration line.");
        value.LineStyle = Contains("solid") ? 0 : Contains("double") ? 1 : Contains("dotted") ? 2 : Contains("dashed") ? 3 : Contains("wavy") ? 4 : throw new FormatException("Unsupported decoration style.");
        var start = text.IndexOf('#');
        var length = 0;
        if (start >= 0)
        {
            var end = start;
            while (end < text.Length && !char.IsWhiteSpace(text[end])) { end++; }
            length = end - start;
        }
        else if (Contains("rgb"))
        {
            start = text.IndexOf("rgb", StringComparison.InvariantCultureIgnoreCase);
            var end = start;
            while (end < text.Length && text[end] != ')') { end++; }
            length = end < text.Length ? end - start + 1 : text.Length - start;
        }
        if (length > 0) { value.Color = ParseColor(text.Substring(start, length)); }
        var seen = false;
        var thickness = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (length > 0 && i >= start && i < start + length) { continue; }
            if (seen && !char.IsDigit(text[i])) { break; }
            if (char.IsDigit(text[i])) { seen = true; thickness = checked(thickness * 10 + text[i] - '0'); }
        }
        if (seen) { value.Thickness = thickness; }
        return value;
    }
    internal static string FormatTextDecoration(ThemeTextDecorationValue value) => "TextDecorationInfo { Color = " + FormatColor(value.Color) +
        ", LineType = " + new[] { "None", "Underline", "Overline", "LineThrough" }[value.LineType] +
        ", LineStyle = " + new[] { "Solid", "Double", "Dotted", "Dashed", "Wavy" }[value.LineStyle] + ", Thickness = " + value.Thickness.ToString(CultureInfo.InvariantCulture) + " }";
}
