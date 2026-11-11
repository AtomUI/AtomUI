using Avalonia.Media;

namespace AtomUI.Controls;

public class TextGlyphExtension : WatermarkGlyphExtension
{
    private string? Text { get; }

    public double FontSize { get; set; } = 16;

    // Null keeps the glyph's theme-driven default foreground.
    public IBrush? Foreground { get; set; }

    public TextGlyphExtension(string text)
    {
        Text = text;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var glyph = new TextGlyph
        {
            Text     = Text,
            FontSize = FontSize
        };

        if (Foreground is not null)
        {
            glyph.Foreground = Foreground;
        }

        SetProperties(glyph);

        return glyph;
    }
}