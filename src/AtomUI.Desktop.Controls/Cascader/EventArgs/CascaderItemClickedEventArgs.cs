namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="CascaderView.ItemClicked" /> event, carrying the clicked item before the view reacts to
/// the click.
/// </summary>
public class CascaderItemClickedEventArgs : EventArgs
{
    /// <summary>
    /// The item of the option that the user clicked.
    /// </summary>
    public CascaderViewItem Item { get; }

    /// <summary>
    /// Creates the event data that reports <paramref name="item" /> as the clicked item.
    /// </summary>
    public CascaderItemClickedEventArgs(CascaderViewItem item)
    {
        Item = item;
    }
}

/// <summary>
/// Data for the <see cref="CascaderView.ItemDoubleClicked" /> event, carrying the item inside which the user
/// double-tapped.
/// </summary>
public class CascaderItemDoubleClickedEventArgs : EventArgs
{
    /// <summary>
    /// The item of the option inside which the user double-tapped.
    /// </summary>
    public CascaderViewItem Item { get; }

    /// <summary>
    /// Creates the event data that reports <paramref name="item" /> as the double-tapped item.
    /// </summary>
    public CascaderItemDoubleClickedEventArgs(CascaderViewItem item)
    {
        Item = item;
    }
}