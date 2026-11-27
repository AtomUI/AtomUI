namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the list item that raised the <see cref="ListBox.ItemClicked" /> event.
/// </summary>
public class ListBoxItemClickedEventArgs : EventArgs
{
    /// <summary>
    /// The list item whose click raised the event.
    /// </summary>
    public ListBoxItem Item { get; }

    /// <summary>
    /// Creates the arguments for a click on the given list item.
    /// </summary>
    /// <param name="item">
    /// The list item that was clicked.
    /// </param>
    public ListBoxItemClickedEventArgs(ListBoxItem item)
    {
        Item = item;
    }
}