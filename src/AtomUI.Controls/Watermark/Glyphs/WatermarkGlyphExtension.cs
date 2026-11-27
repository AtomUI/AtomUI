using Avalonia.Markup.Xaml;

namespace AtomUI.Controls;

/// <summary>
/// Base class of XAML markup extensions that create a <see cref="WatermarkGlyph" /> for the attached
/// <c>Watermark.Glyph</c> property. Its properties hold the tile layout; <see cref="SetProperties" />
/// copies them onto the glyph that a derived extension creates.
/// </summary>
public abstract class WatermarkGlyphExtension : MarkupExtension
{
    /// <summary>
    /// Horizontal gap between neighbouring glyphs, copied to <see cref="WatermarkGlyph.HorizontalSpace" />.
    /// The default is 280.
    /// </summary>
    public double HorizontalSpace { get; set; } = 280d;

    /// <summary>
    /// Vertical gap between neighbouring glyphs, copied to <see cref="WatermarkGlyph.VerticalSpace" />.
    /// The default is 40.
    /// </summary>
    public double VerticalSpace { get; set; } = 40d;

    /// <summary>
    /// Horizontal origin of the tile grid, copied to <see cref="WatermarkGlyph.HorizontalOffset" />.
    /// The default is 0.
    /// </summary>
    public double HorizontalOffset { get; set; } = 0;

    /// <summary>
    /// Vertical origin of the tile grid, copied to <see cref="WatermarkGlyph.VerticalOffset" />.
    /// The default is 0.
    /// </summary>
    public double VerticalOffset { get; set; } = 0;

    /// <summary>
    /// Rotation of every glyph in degrees, copied to <see cref="WatermarkGlyph.Rotate" />. The default is -20.
    /// </summary>
    public double Rotate { get; set; } = -20;

    /// <summary>
    /// Opacity of the whole watermark pattern, copied to <see cref="WatermarkGlyph.Opacity" />.
    /// The default is 0.3.
    /// </summary>
    public double Opacity { get; set; } = 0.3;

    /// <summary>
    /// Whether every second column tilts the other way, copied to <see cref="WatermarkGlyph.IsMirrorUsed" />.
    /// The default is false.
    /// </summary>
    public bool IsMirrorUsed { get; set; } = false;

    /// <summary>
    /// Whether every second row is staggered by half a column, copied to <see cref="WatermarkGlyph.IsCrossUsed" />.
    /// The default is true.
    /// </summary>
    public bool IsCrossUsed { get; set; } = true;

    /// <summary>
    /// Copies the tile settings of this extension onto <paramref name="glyph" />. A derived extension calls it
    /// for the glyph it creates.
    /// </summary>
    /// <param name="glyph">
    /// The glyph created by a derived extension.
    /// </param>
    protected void SetProperties(WatermarkGlyph glyph)
    {
        glyph.HorizontalSpace  = HorizontalSpace;
        glyph.VerticalSpace    = VerticalSpace;
        glyph.HorizontalOffset = HorizontalOffset;
        glyph.VerticalOffset   = VerticalOffset;
        glyph.Rotate           = Rotate;
        glyph.Opacity          = Opacity;
        glyph.IsMirrorUsed        = IsMirrorUsed;
        glyph.IsCrossUsed         = IsCrossUsed;
    }
}