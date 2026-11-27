using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the desktop rate: the star colors, the star size per size level and the hover scale.
/// </summary>
[ControlDesignToken]
public sealed partial class RateToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Color of the glyph of the selected stars, of the left half of a half-selected star, and of the dashed outline
    /// of the star that marks the start of keyboard stepping.
    /// </summary>
    public partial Color StarColor { get; set; }
    
    /// <summary>
    /// Width and height of a star in a regular-size rate.
    /// </summary>
    public partial double StarSize { get; set; }
    
    /// <summary>
    /// Width and height of a star in a small rate.
    /// </summary>
    public partial double StarSizeSM { get; set; }
    
    /// <summary>
    /// Width and height of a star in a large rate.
    /// </summary>
    public partial double StarSizeLG { get; set; }
    
    /// <summary>
    /// Scale factor that enlarges a star while the pointer is over it, and the star that marks the start of keyboard
    /// stepping.
    /// </summary>
    public partial double StarHoverScale { get; set; }
    
    /// <summary>
    /// Color of the glyph of the unselected stars and of the right half of a half-selected star.
    /// </summary>
    public partial Color StarBg { get; set; }
    
}
