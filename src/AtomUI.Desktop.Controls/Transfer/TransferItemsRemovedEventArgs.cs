using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the items that a <see cref="TransferListView" /> reports as removed: the item of a row whose remove
/// button was clicked, or the items of the current page.
/// </summary>
public class TransferItemsRemovedEventArgs : EventArgs
{
    /// <summary>
    /// The removed items, or <see langword="null" /> when the view has none to report.
    /// </summary>
    public IList<IItemKey>? Items { get; }
    /// <summary>
    /// Initializes the event data with the removed items.
    /// </summary>
    /// <param name="items">
    /// The removed items, or <see langword="null" /> when the view has none to report.
    /// </param>
    public TransferItemsRemovedEventArgs(IList<IItemKey>? items)
    {
        Items = items;
    }
}