using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// A watermark glyph that draws an <see cref="IImage" /> scaled to <see cref="Height" /> with its aspect
/// ratio kept. The watermark renderer assumes that this exact type draws only within the size reported by
/// <see cref="GetDesiredSize" />, and skips the tiles that fall outside the visible area.
/// </summary>
public class ImageGlyph : WatermarkGlyph
{
    /// <summary>
    /// Height the image is scaled to; the width follows the aspect ratio of <see cref="Source" />. It applies to
    /// drawing and to the size <see cref="GetDesiredSize" /> reports. The default is 28, and clearing a value
    /// restores it.
    /// </summary>
    public double Height
    {
        get => GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Height" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HeightProperty = AvaloniaProperty
        .Register<ImageGlyph, double>(nameof(Height), 28);

    /// <summary>
    /// The image to draw, or <see langword="null" /> to draw nothing and report an empty size, which keeps the
    /// watermark from tiling this glyph.
    /// </summary>
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Source" /> property.
    /// </summary>
    public static readonly StyledProperty<IImage?> SourceProperty = AvaloniaProperty
        .Register<ImageGlyph, IImage?>(nameof(Source));

    static ImageGlyph()
    {
    }

    /// <summary>
    /// Draws the whole <see cref="Source" /> at the origin of the drawing context, scaled to
    /// <see cref="Height" /> with its aspect ratio kept. Draws nothing while <see cref="Source" /> is
    /// <see langword="null" />.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        if (Source == null)
        {
            return;
        }

        var w = Height / Source.Size.Height * Source.Size.Width;
        Source.Draw(context, new Rect(new Point(), Source.Size), new Rect(new Point(), new Size(w, Height)));
    }

    /// <summary>
    /// Reports the size the image is drawn at.
    /// </summary>
    /// <returns>
    /// The aspect-preserving size for <see cref="Height" />, or an empty size while <see cref="Source" /> is
    /// <see langword="null" />.
    /// </returns>
    public override Size GetDesiredSize()
    {
        if (Source == null)
        {
            return new Size();
        }

        var w = Height / Source.Size.Height * Source.Size.Width;
        return new Size(w, Height);
    }
}