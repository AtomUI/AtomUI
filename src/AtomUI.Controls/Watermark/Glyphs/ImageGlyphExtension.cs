using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// XAML markup extension that creates an <see cref="ImageGlyph" />, for example as the value of
/// <c>Watermark.Glyph</c>.
/// </summary>
public class ImageGlyphExtension : WatermarkGlyphExtension
{
    /// <summary>
    /// The image the created glyph draws, or <see langword="null" /> for a glyph that draws nothing and has no
    /// size.
    /// </summary>
    public IImage? Source { get; set; }

    /// <summary>
    /// Height the created glyph scales the image to. The default is 28.
    /// </summary>
    public double Height { get; set; } = 28;

    /// <summary>
    /// Creates an <see cref="ImageGlyph" /> with <see cref="Source" /> and <see cref="Height" />, then copies the
    /// tile settings inherited from <see cref="WatermarkGlyphExtension" /> onto it.
    /// </summary>
    /// <returns>
    /// A new glyph on every call.
    /// </returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var glyph = new ImageGlyph
        {
            Source = Source,
            Height = Height
        };

        SetProperties(glyph);

        return glyph;
    }
}