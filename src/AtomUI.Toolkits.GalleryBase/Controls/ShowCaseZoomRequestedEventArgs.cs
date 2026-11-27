using Avalonia.Interactivity;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Describes a request to show a showcase item enlarged in the zoom overlay: the item itself, its title and
/// its description.
/// </summary>
public class ShowCaseZoomRequestedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// The showcase item that requested the zoom view.
    /// </summary>
    public ShowCaseItem Item { get; }

    /// <summary>
    /// The title that the zoom overlay shows for the item.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// The description that the zoom overlay shows for the item. The overlay hides it when it is null or empty.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Creates the arguments of a zoom request raised by one showcase item.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event to raise the arguments with.
    /// </param>
    /// <param name="item">
    /// The showcase item that requested the zoom view.
    /// </param>
    /// <param name="title">
    /// The title that the zoom overlay shows for the item.
    /// </param>
    /// <param name="description">
    /// The description that the zoom overlay shows for the item.
    /// </param>
    public ShowCaseZoomRequestedEventArgs(RoutedEvent routedEvent,
                                          ShowCaseItem item,
                                          string? title,
                                          string? description)
        : base(routedEvent)
    {
        Item        = item;
        Title       = title;
        Description = description;
    }
}
