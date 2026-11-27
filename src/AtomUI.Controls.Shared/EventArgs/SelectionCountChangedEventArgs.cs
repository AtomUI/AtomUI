namespace AtomUI.Controls;

/// <summary>
/// Event data for the change of the number of selected items in a transfer view.
/// </summary>
public class SelectionCountChangedEventArgs
{
    /// <summary>
    /// The number of selected keys after the change.
    /// </summary>
    public int Count { get; }
    
    /// <summary>
    /// Initializes the event data with a selection count.
    /// </summary>
    /// <param name="count">
    /// The number of selected keys after the change.
    /// </param>
    public SelectionCountChangedEventArgs(int count)
    {
        Count = count;
    }
}