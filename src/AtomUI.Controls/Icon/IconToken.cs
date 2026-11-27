using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Design tokens of the <see cref="Icon" /> control: the default pen geometry of its glyph and the theme colors
/// of its secondary-layer and fallback brushes.
/// </summary>
[ControlDesignToken]
public sealed partial class IconToken : AbstractControlDesignToken
{
    /// <summary>
    /// Fallback outline color of the secondary layer in a two-tone glyph.
    /// </summary>
    public partial Color SecondaryStrokeColor { get; set; }

    /// <summary>
    /// Fallback interior color of the secondary layer in a two-tone glyph.
    /// </summary>
    public partial Color SecondaryFillColor { get; set; }

    /// <summary>
    /// Paint used for icon geometry without a resolved foreground color.
    /// </summary>
    public partial Color FallbackColor { get; set; }

    /// <summary>
    /// Default pen width in the icon geometry’s coordinate space.
    /// </summary>
    public partial double StrokeWidth { get; set; }

    /// <summary>
    /// Shape of the ends of open paths in a stroked glyph.
    /// </summary>
    public partial PenLineCap StrokeLineCap { get; set; }

    /// <summary>
    /// Shape of the corners joining path segments in a stroked glyph.
    /// </summary>
    public partial PenLineJoin StrokeLineJoin { get; set; }
}
