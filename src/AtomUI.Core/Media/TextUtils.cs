using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace AtomUI.Media;

/// <summary>
/// Measures the size of laid-out text.
/// </summary>
public static class TextUtils
{
    private const int MeasureCacheCapacity = 512;
    private static readonly object s_measureCacheGate = new();
    private static readonly Dictionary<MeasureKey, Size> s_measureCache = new();

    /// <summary>
    /// Lays out the text without wrapping and returns the width and height of the layout, in device-independent
    /// pixels.
    /// </summary>
    /// <param name="text">
    /// The text to measure.
    /// </param>
    /// <param name="fontSize">
    /// The font size, in device-independent pixels.
    /// </param>
    /// <param name="fontFamily">
    /// The font family.
    /// </param>
    /// <param name="fontStyle">
    /// The font style.
    /// </param>
    /// <param name="fontWeight">
    /// The font weight.
    /// </param>
    /// <returns>
    /// The width and height of the laid-out text, in device-independent pixels.
    /// </returns>
    public static Size CalculateTextSize(string text,
                                         double fontSize,
                                         FontFamily fontFamily,
                                         FontStyle fontStyle = FontStyle.Normal,
                                         FontWeight fontWeight = FontWeight.Normal)
    {
        var       typeface   = new Typeface(fontFamily, fontStyle, fontWeight);
        using var textLayout = new TextLayout(text, typeface, fontSize, null);
        return new Size(textLayout.Width, textLayout.Height);
    }

    /// <summary>
    /// Returns the same size as <see cref="CalculateTextSize" />, reusing an earlier measurement of the identical
    /// text with the identical font. The key also holds the resolved primary glyph typeface, the font collection
    /// generation and the current cultures, so any change that could alter shaping or fallback measures again.
    /// The cache is bounded and is cleared when full.
    /// </summary>
    internal static Size CalculateTextSizeCached(string text,
                                                 double fontSize,
                                                 FontFamily fontFamily,
                                                 FontStyle fontStyle = FontStyle.Normal,
                                                 FontWeight fontWeight = FontWeight.Normal)
    {
        FontManager.Current.TryGetGlyphTypeface(new Typeface(fontFamily, fontStyle, fontWeight), out var primary);
        var key = new MeasureKey(text,
            fontSize,
            fontFamily,
            fontStyle,
            fontWeight,
            primary,
            FontCollectionLifetime.Generation,
            CultureInfo.CurrentCulture.Name,
            CultureInfo.CurrentUICulture.Name);
        lock (s_measureCacheGate)
        {
            if (s_measureCache.TryGetValue(key, out var cached))
            {
                return cached;
            }
        }

        var size = CalculateTextSize(text, fontSize, fontFamily, fontStyle, fontWeight);
        lock (s_measureCacheGate)
        {
            if (s_measureCache.Count >= MeasureCacheCapacity)
            {
                s_measureCache.Clear();
            }

            s_measureCache[key] = size;
        }

        return size;
    }

    private readonly record struct MeasureKey(
        string Text,
        double FontSize,
        FontFamily FontFamily,
        FontStyle FontStyle,
        FontWeight FontWeight,
        GlyphTypeface? PrimaryGlyphTypeface,
        int FontGeneration,
        string Culture,
        string UICulture);
}
