using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data of the <c>TabReordering</c> event, which a tab control or tab strip raises before a
/// dragged tab is moved to another position. The move can still be canceled.
/// </summary>
public class TabReorderingEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the arguments for a tab that is about to be moved.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event that the arguments are raised with.
    /// </param>
    /// <param name="item">
    /// The dragged tab's item in the items list.
    /// </param>
    /// <param name="oldIndex">
    /// The zero-based position of the item before the move.
    /// </param>
    /// <param name="newIndex">
    /// The zero-based position the item is moved to.
    /// </param>
    public TabReorderingEventArgs(RoutedEvent routedEvent, object? item, int oldIndex, int newIndex)
        : base(routedEvent)
    {
        Item     = item;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }

    /// <summary>
    /// Gets the dragged tab's item in the items list.
    /// </summary>
    public object? Item { get; }

    /// <summary>
    /// Gets the zero-based position of the item before the move.
    /// </summary>
    public int OldIndex { get; }

    /// <summary>
    /// Gets the zero-based position the item is moved to.
    /// </summary>
    public int NewIndex { get; }

    /// <summary>
    /// Gets or sets whether the move is abandoned. When the value is <see langword="true" />, the tabs keep
    /// their original order and no <c>TabReordered</c> event is raised.
    /// </summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// Carries the data of the <c>TabReordered</c> event, which a tab control or tab strip raises after a
/// dragged tab has been moved to another position.
/// </summary>
public class TabReorderedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the arguments for a tab that has been moved.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event that the arguments are raised with.
    /// </param>
    /// <param name="item">
    /// The dragged tab's item in the items list.
    /// </param>
    /// <param name="oldIndex">
    /// The zero-based position of the item before the move.
    /// </param>
    /// <param name="newIndex">
    /// The zero-based position of the item after the move.
    /// </param>
    public TabReorderedEventArgs(RoutedEvent routedEvent, object? item, int oldIndex, int newIndex)
        : base(routedEvent)
    {
        Item     = item;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }

    /// <summary>
    /// Gets the dragged tab's item in the items list.
    /// </summary>
    public object? Item { get; }

    /// <summary>
    /// Gets the zero-based position of the item before the move.
    /// </summary>
    public int OldIndex { get; }

    /// <summary>
    /// Gets the zero-based position of the item after the move.
    /// </summary>
    public int NewIndex { get; }
}
