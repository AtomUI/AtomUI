using Avalonia.Media;

namespace AtomUI.Theme.ContractTests;

/// <summary>
/// WCAG 2.x contrast ratio between two sRGB colors, used by the readability checks of the algorithm contract.
/// </summary>
public static class ContrastRatio
{
    /// <summary>
    /// Minimum contrast of body text (<c>ColorText</c>) on the container background.
    /// </summary>
    public const double BodyText = 4.5;

    /// <summary>
    /// Minimum contrast of text on a solid fill (<c>ColorTextLightSolid</c> on <c>ColorPrimary</c>).
    /// </summary>
    public const double TextOnSolid = 3;

    /// <summary>
    /// Returns the contrast ratio, from 1 to 21, of <paramref name="foreground" /> drawn on
    /// <paramref name="background" />. A translucent foreground is first composited over the background; the
    /// background is treated as opaque, so callers flatten a translucent background beforehand.
    /// </summary>
    public static double Of(Color foreground, Color background)
    {
        var opaqueBackground = Opaque(background);
        var first = RelativeLuminance(Composite(foreground, opaqueBackground));
        var second = RelativeLuminance(opaqueBackground);
        var lighter = Math.Max(first, second);
        var darker = Math.Min(first, second);
        return (lighter + 0.05) / (darker + 0.05);
    }

    /// <summary>
    /// Returns the WCAG relative luminance of the color's RGB channels; alpha is ignored.
    /// </summary>
    public static double RelativeLuminance(Color color)
    {
        return 0.2126 * Linearize(color.R) + 0.7152 * Linearize(color.G) + 0.0722 * Linearize(color.B);
    }

    /// <summary>
    /// Composites <paramref name="foreground" /> over <paramref name="background" />, which is treated as opaque, by
    /// per-channel alpha blending in sRGB. The result is opaque.
    /// </summary>
    public static Color Composite(Color foreground, Color background)
    {
        var alpha = foreground.A / 255.0;
        return Color.FromRgb(
            Blend(foreground.R, background.R, alpha),
            Blend(foreground.G, background.G, alpha),
            Blend(foreground.B, background.B, alpha));
    }

    private static Color Opaque(Color color) => Color.FromRgb(color.R, color.G, color.B);

    private static byte Blend(byte foreground, byte background, double alpha) =>
        (byte)Math.Round(foreground * alpha + background * (1 - alpha), MidpointRounding.AwayFromZero);

    private static double Linearize(byte channel)
    {
        var value = channel / 255.0;
        return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
    }
}
