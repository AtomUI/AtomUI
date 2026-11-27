namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="CascaderView.ItemCollapsed" /> event, carrying the item of the option that the view has
/// just collapsed.
/// </summary>
public class CascaderItemCollapsedEventArgs : EventArgs
{
    /// <summary>
    /// The item of the option that was collapsed.
    /// </summary>
    public CascaderViewItem Item { get; }

    /// <summary>
    /// Creates the event data that reports <paramref name="item" /> as the collapsed item.
    /// </summary>
    public CascaderItemCollapsedEventArgs(CascaderViewItem item)
    {
        Item = item;
    }
}