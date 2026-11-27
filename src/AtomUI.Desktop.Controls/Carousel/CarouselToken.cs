using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own design tokens of the <see cref="Carousel" />: the pagination indicator and the previous/next arrow buttons.
/// </summary>
[ControlDesignToken]
public sealed partial class CarouselToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Width of one pagination indicator dot. A dot is <see cref="IndicatorActiveWidth" /> wide while its page is
    /// the current one.
    /// </summary>
    public partial double IndicatorWidth { get; set; }
    
    /// <summary>
    /// Height of one pagination indicator dot.
    /// </summary>
    public partial double IndicatorHeight { get; set; }
    
    /// <summary>
    /// Gap between adjacent pagination indicator dots.
    /// </summary>
    public partial double IndicatorGap { get; set; }
    
    /// <summary>
    /// Distance between the pagination indicator and the carousel edge it sits on.
    /// </summary>
    public partial double PaginationOffset { get; set; }
    
    /// <summary>
    /// Width of the pagination indicator dot of the current page.
    /// </summary>
    public partial double IndicatorActiveWidth { get; set; }
    
    /// <summary>
    /// Width and height of the icon in the previous and next arrow buttons.
    /// </summary>
    public partial double ArrowSize { get; set; }
    
    /// <summary>
    /// Distance between an arrow button and the carousel edge it is inset from. The previous button is inset from
    /// the left edge and the next button from the right edge; with the pagination on the left or right, from the top
    /// and bottom edges instead.
    /// </summary>
    public partial double ArrowOffset { get; set; }

    /// <summary>
    /// Opacity of the previous and next arrow buttons at rest; a hovered button is fully opaque. The token is an
    /// AtomUI extension: antd fixes the resting opacity to 0.4.
    /// </summary>
    public partial double ArrowOpacity { get; set; }
    
}
