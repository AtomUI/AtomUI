using Avalonia.Media;

namespace AtomUI.Controls;

public class TextGlyphExtension : WatermarkGlyphExtension
{
    private string? Text { get; }

    public double FontSize
    {
        get => _fontSize ?? TextGlyph.FontSizeProperty.GetDefaultValue(typeof(TextGlyph));
        set => _fontSize = value;
    }

    // Null keeps the glyph's theme-driven default foreground.
    public IBrush? Foreground { get; set; }

    // Whether FontSize was set is recorded on its own: comparing with the default value cannot tell an explicit 16 from
    // the theme-driven default.
    private double? _fontSize;

    public TextGlyphExtension(string text)
    {
        Text = text;
    }

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
