using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Provides data for the <see cref="Masonry.LayoutChanged" /> event.
/// </summary>
/// <remarks>
/// The event only notifies effective column assignment changes, not pixel-level arrange
/// rectangles, to avoid frequent raising during scroll and resize. Data item identity is
/// maintained by the ItemsControl container and the user's view model; no key is required.
/// </remarks>
public class MasonryLayoutChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the effective column assignment for every child, in child order.
    /// </summary>
    public IReadOnlyList<MasonryItemLayout> Items { get; }

    /// <summary>
    /// Initializes the event data with the column assignment of every child.
    /// </summary>
    /// <param name="items">
    /// The effective column assignment for every child, in child order.
    /// </param>
    public MasonryLayoutChangedEventArgs(IReadOnlyList<MasonryItemLayout> items)
    {
        Items = items;
    }
}

/// <summary>
/// Describes the effective layout result of a single Masonry child.
/// </summary>
public readonly record struct MasonryItemLayout
{
    /// <summary>
    /// The child element reference.
    /// </summary>
    public Control Element { get; init; }

    /// <summary>
    /// The index of the child in child order.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// The zero-based column the child was placed into, or <c>-1</c> when the child is not visible.
    /// For a full-span item this is <c>0</c>.
    /// </summary>
    public int Column { get; init; }

    /// <summary>
    /// Whether the child occupies the full Masonry width.
    /// </summary>
    public bool IsFullSpan { get; init; }

    /// <summary>
    /// Initializes the layout result of a single child.
    /// </summary>
    /// <param name="element">
    /// The child element.
    /// </param>
    /// <param name="index">
    /// The zero-based index of the child in child order.
    /// </param>
    /// <param name="column">
    /// The zero-based column the child occupies: <c>0</c> for a full-span child, <c>-1</c> for a child that is not
    /// visible.
    /// </param>
    /// <param name="isFullSpan">
    /// Whether the child occupies the full Masonry width.
    /// </param>
    public MasonryItemLayout(Control element, int index, int column, bool isFullSpan)
    {
        Element     = element;
        Index       = index;
        Column      = column;
        IsFullSpan  = isFullSpan;
    }
}
