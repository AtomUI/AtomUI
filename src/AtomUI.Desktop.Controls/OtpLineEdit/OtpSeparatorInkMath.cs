using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Computes the translation that centers the ink of a separator glyph. The ink box comes from the glyph metrics
/// of the font table, in design units and relative to the baseline origin: XBearing is the horizontal offset of
/// the left edge of the ink, YBearing is the height of the top edge of the ink above the baseline, positive
/// upward. The goal is to move the ink center onto the center of the line box: horizontally onto the middle of
/// the advance, vertically onto the geometric center of the line box.
/// </summary>
internal static class OtpSeparatorInkMath
{
    public static (double OffsetX, double OffsetY) ComputeInkCenterOffset(
        GlyphMetrics metrics,
        double advance,
        double baseline,
        double lineHeight,
        double fontRenderingEmSize,
        double designEmHeight)
    {
        if (designEmHeight <= 0 || fontRenderingEmSize <= 0 || lineHeight <= 0)
        {
            return (0, 0);
        }

        var scale = fontRenderingEmSize / designEmHeight;
        var inkWidth  = metrics.Width * scale;
        var inkHeight = metrics.Height * scale;
        if (inkWidth <= 0 || inkHeight <= 0)
        {
            return (0, 0);
        }

        var inkLeft  = metrics.XBearing * scale;
        var inkTop   = metrics.YBearing * scale;
        var inkCenterX = inkLeft + inkWidth / 2;
        var offsetX    = advance / 2 - inkCenterX;

        // Vertical: a positive inkCenterAboveBaseline means the ink center lies above the baseline.
        var inkCenterAboveBaseline = inkTop - inkHeight / 2;
        var boxCenterAboveBaseline = baseline - lineHeight / 2;
        var offsetY                = inkCenterAboveBaseline - boxCenterAboveBaseline;

        return (offsetX, offsetY);
    }
}
