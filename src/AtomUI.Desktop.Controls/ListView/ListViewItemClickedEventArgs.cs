namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the list item container that was clicked, for the <see cref="ListView.ItemClicked" /> event.
/// </summary>
public class ListViewItemClickedEventArgs : EventArgs
{
    /// <summary>
    /// The clicked list item container.
    /// </summary>
    public ListViewItem Item { get; }

    /// <summary>
    /// Creates the arguments for a click on <paramref name="item" />.
    /// </summary>
    /// <param name="item">
    /// The clicked list item container.
    /// </param>
    public ListViewItemClickedEventArgs(ListViewItem item)
    {
        Item = item;
    }
}