using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// XAML markup extension that creates a <see cref="TextGlyph" />, for example as the value of
/// <c>Watermark.Glyph</c>.
/// </summary>
public class TextGlyphExtension : WatermarkGlyphExtension
{
    private string? Text { get; }

    /// <summary>
    /// Font size of the created text. An unset value is reported as the <see cref="TextGlyph.FontSizeProperty" />
    /// default and is not assigned to the glyph, which therefore keeps its theme-driven size.
    /// </summary>
    public double FontSize
    {
        get => _fontSize ?? TextGlyph.FontSizeProperty.GetDefaultValue(typeof(TextGlyph));
        set => _fontSize = value;
    }

    /// <summary>
    /// Brush of the created text, or <see langword="null" /> to keep the glyph's theme-driven default foreground.
    /// </summary>
    public IBrush? Foreground { get; set; }

    // Whether FontSize was set is recorded on its own: comparing with the default value cannot tell an explicit 16 from
    // the theme-driven default.
    private double? _fontSize;

    /// <summary>
    /// Creates an extension whose glyph draws <paramref name="text" />.
    /// </summary>
    /// <param name="text">
    /// The text to draw, passed to <see cref="TextGlyph.Text" /> unchanged. <see langword="null" /> produces a
    /// glyph that draws nothing.
    /// </param>
    public TextGlyphExtension(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Creates a <see cref="TextGlyph" /> for the text. <see cref="FontSize" /> and <see cref="Foreground" />
    /// are assigned only when they were set. The tile settings inherited from
    /// <see cref="WatermarkGlyphExtension" /> are then copied onto it.
    /// </summary>
    /// <returns>
    /// A new glyph on every call.
    /// </returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var glyph = new TextGlyph { Text = Text };
        if (_fontSize is { } fontSize)
        {
            glyph.FontSize = fontSize;
        }

        if (Foreground is not null)
        {
            glyph.Foreground = Foreground;
        }

        SetProperties(glyph);

        return glyph;
    }
}
