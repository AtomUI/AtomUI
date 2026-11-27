namespace AtomUI.Desktop.Controls;

/// <summary>
/// The edge of a <see cref="Carousel" /> on which the pagination indicator is placed. The value also sets the
/// slide axis of the <see cref="CarouselTransitionEffect.Scroll" /> transition and the edges the arrow buttons
/// are inset from.
/// </summary>
public enum CarouselPaginationPosition
{
    /// <summary>
    /// Pagination indicator on the top edge of the carousel.
    /// </summary>
    Top,
    /// <summary>
    /// Pagination indicator on the bottom edge of the carousel. This is the default.
    /// </summary>
    Bottom,
    /// <summary>
    /// Pagination indicator on the left edge of the carousel.
    /// </summary>
    Left,
    /// <summary>
    /// Pagination indicator on the right edge of the carousel.
    /// </summary>
    Right
}

/// <summary>
/// The transition played when a <see cref="Carousel" /> moves to another page.
/// </summary>
public enum CarouselTransitionEffect
{
    /// <summary>
    /// The pages slide in and out: horizontally when the pagination indicator is on the top or bottom edge, and
    /// vertically when it is on the left or right edge. This is the default.
    /// </summary>
    Scroll,
    /// <summary>
    /// The outgoing page fades out while the incoming page fades in.
    /// </summary>
    Fade
}