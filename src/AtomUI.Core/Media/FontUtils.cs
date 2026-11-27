namespace AtomUI.Media;

/// <summary>
/// Helpers for converting font-related lengths.
/// </summary>
public static class FontUtils
{
   /// <summary>
   /// Converts a length in <c>em</c> units to device pixels.
   /// </summary>
   /// <param name="value">
   /// The length as a multiple of the font size; 1 is one em.
   /// </param>
   /// <param name="fontSize">
   /// The font size that one em is based on.
   /// </param>
   /// <param name="renderScaling">
   /// The factor from device-independent units to device pixels. 1 leaves the result in device-independent units.
   /// </param>
   /// <returns>
   /// The length in device pixels.
   /// </returns>
   public static double ConvertEmToPixel(double value, double fontSize, double renderScaling = 1.0)
    {
        var fontSizePx = fontSize * value * renderScaling;
        return fontSizePx;
    }
}