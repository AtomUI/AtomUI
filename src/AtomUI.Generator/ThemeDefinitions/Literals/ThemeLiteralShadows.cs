using System.Globalization;

namespace AtomUI.Generator;

internal sealed class ThemeShadowValue
{
    internal double OffsetX;
    internal double OffsetY;
    internal double Blur;
    internal double Spread;
    internal uint Color;
    internal bool IsInset;
    // Literal serialization requires exact zero, including signed zero. A tolerance
    // would discard small nonzero values and change Avalonia's shadow semantics.
    internal bool IsDefault => OffsetX.Equals(0d) && OffsetY.Equals(0d) &&
                               Blur.Equals(0d) && Spread.Equals(0d) && Color == 0 && !IsInset;
}
internal static partial class ThemeLiteralCodec
{
    internal static ThemeShadowValue ParseShadow(string text)
    {
        var parts = SplitBracketed(text, new[] { ' ', '\t' });
        if (parts.Length == 1 && parts[0] == "none") { return new ThemeShadowValue(); }
        if (parts.Length < 3 || parts.Length > 6) { throw new FormatException("Invalid box shadow."); }
        var inset = parts[0] == "inset";
        var start = inset ? 1 : 0;
        var count = parts.Length - start;
        if (count < 3) { throw new FormatException("Invalid box shadow."); }
        // Avalonia's bounded reader consumes at most five fields after the optional inset marker.
        count = Math.Min(5, count);
        return new ThemeShadowValue
        {
            IsInset = inset,
            OffsetX = double.Parse(parts[start], CultureInfo.InvariantCulture),
            OffsetY = double.Parse(parts[start + 1], CultureInfo.InvariantCulture),
            Blur = count >= 4 ? double.Parse(parts[start + 2], CultureInfo.InvariantCulture) : 0,
            Spread = count >= 5 ? double.Parse(parts[start + 3], CultureInfo.InvariantCulture) : 0,
            Color = ParseColor(parts[start + count - 1])
        };
    }
    internal static ThemeShadowValue[] ParseShadows(string text)
    {
        var parts = SplitBracketed(text, new[] { ',' });
        if (parts.Length == 0 || (parts.Length == 1 && (string.IsNullOrWhiteSpace(parts[0]) || parts[0] == "none"))) { return Array.Empty<ThemeShadowValue>(); }
        var shadows = parts.Select(ParseShadow).ToArray();
        return shadows.Length == 1 && shadows[0].IsDefault ? Array.Empty<ThemeShadowValue>() : shadows;
    }
    internal static string FormatShadow(ThemeShadowValue shadow)
    {
        if (shadow.IsDefault) { return "none"; }
        return (shadow.IsInset ? "inset " : "") + General(shadow.OffsetX) + " " + General(shadow.OffsetY) + " " +
            (!shadow.Blur.Equals(0d) || !shadow.Spread.Equals(0d) ? General(shadow.Blur) + " " : "") +
            (!shadow.Spread.Equals(0d) ? General(shadow.Spread) + " " : "") + FormatColor(shadow.Color);
    }
    private static string[] SplitBracketed(string text, char[] separators)
    {
        var result = new List<string>();
        var depth = 0;
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '(') { depth++; }
            else if (text[i] == ')') { if (depth == 0) { throw new FormatException("Unmatched closing bracket."); } depth--; }
            else if (depth == 0 && separators.Contains(text[i]))
            {
                if (i != start) { result.Add(text.Substring(start, i - start)); }
                start = i + 1;
            }
        }
        if (depth != 0) { throw new FormatException("Unmatched opening bracket."); }
        if (start != text.Length) { result.Add(text.Substring(start)); }
        return result.ToArray();
    }
}
