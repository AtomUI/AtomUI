namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="CascaderView.ItemExpanded" /> event, carrying the item of the option that the view has
/// just expanded.
/// </summary>
public class CascaderItemExpandedEventArgs : EventArgs
{
    /// <summary>
    /// The item of the option that was expanded.
    /// </summary>
    public CascaderViewItem Item { get; }

    /// <summary>
    /// Creates the event data that reports <paramref name="item" /> as the expanded item.
    /// </summary>
    public CascaderItemExpandedEventArgs(CascaderViewItem item)
    {
        Item = item;
    }
}