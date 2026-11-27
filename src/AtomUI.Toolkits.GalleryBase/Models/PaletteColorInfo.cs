using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Models;

/// <summary>
/// One color swatch of a palette in the Gallery palette list: the color, its name, its hexadecimal
/// text and the brush of the label drawn on it.
/// </summary>
public record PaletteColorInfo
{
    /// <summary>
    /// The brush of the swatch. It fills the color item and is the source of <see cref="Hex" />.
    /// </summary>
    public IBrush Brush { get; }
    /// <summary>
    /// The brush of the text drawn on the swatch: black or white, chosen by the position of the color in
    /// its palette, independent of the theme colors of the UI.
    /// </summary>
    public IBrush TextBrush { get; }
    /// <summary>
    /// Name shown on the swatch.
    /// </summary>
    public string DisplayName { get; }
    /// <summary>
    /// Upper-case text form of <see cref="Brush" /> (the hexadecimal color of a solid color brush), or the
    /// text <c>Invalid Color</c> when the brush has no text form.
    /// </summary>
    public string Hex { get; }

    /// <summary>
    /// Creates a swatch and picks <see cref="TextBrush" /> from the position of the color in its palette.
    /// </summary>
    /// <param name="displayName">
    /// Name shown on the swatch.
    /// </param>
    /// <param name="brush">
    /// Brush of the swatch.
    /// </param>
    /// <param name="light">
    /// Whether the palette is the light variant of the preset color.
    /// </param>
    /// <param name="index">
    /// Zero-based position of the color in the palette. The text is black on the first five colors of a
    /// light palette and on the sixth and later colors of a dark palette, and white otherwise.
    /// </param>
    public PaletteColorInfo(string displayName, IBrush brush, bool light, int index)
    {
        DisplayName = displayName;
        Brush       = brush;
        Hex         = brush.ToString()?.ToUpperInvariant() ?? "Invalid Color";
        if ((light && index < 5) || (!light && index >= 5))
        {
            // Reference palette swatches use absolute black/white contrast data, independent of UI theme colors.
#pragma warning disable ATOMUIGEN044
            TextBrush = Brushes.Black;
#pragma warning restore ATOMUIGEN044
        }
        else
        {
            // Reference palette swatches use absolute black/white contrast data, independent of UI theme colors.
#pragma warning disable ATOMUIGEN044
            TextBrush = Brushes.White;
#pragma warning restore ATOMUIGEN044
        }
    }
}
