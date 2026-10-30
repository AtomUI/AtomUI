using Avalonia.Interactivity;

namespace AtomUI.Toolkits.GalleryBase.Controls;

public class ShowCaseZoomRequestedEventArgs : RoutedEventArgs
{
    public ShowCaseItem Item { get; }

    public string? Title { get; }

    public string? Description { get; }

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
