namespace AtomUI.Desktop.Controls;

/// <summary>
/// The notation in which a color picker shows its value as text.
/// </summary>
public enum ColorFormat
{
    /// <summary>
    /// A hexadecimal string. The format selector of the color input lists it as HEX.
    /// </summary>
    Hex,
    /// <summary>
    /// Hue, saturation, value and alpha components. The format selector of the color input lists it as HSV.
    /// </summary>
    Hsva,
    /// <summary>
    /// Red, green, blue and alpha components. The format selector of the color input lists it as RGB.
    /// </summary>
    Rgba
}