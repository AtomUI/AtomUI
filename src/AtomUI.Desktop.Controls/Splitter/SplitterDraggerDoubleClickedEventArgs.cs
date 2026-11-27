namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data of the <see cref="Splitter.DraggerDoubleClicked" /> event: the split bar handle that was
/// double-clicked.
/// </summary>
public class SplitterDraggerDoubleClickedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the event data of a double-click on one split bar handle.
    /// </summary>
    /// <param name="handleIndex">
    /// The zero-based index of the double-clicked handle.
    /// </param>
    public SplitterDraggerDoubleClickedEventArgs(int handleIndex)
    {
        HandleIndex = handleIndex;
    }

    /// <summary>
    /// The zero-based index of the double-clicked split bar handle. Handle <c>i</c> lies between pane <c>i</c>
    /// and pane <c>i + 1</c>.
    /// </summary>
    public int HandleIndex { get; }
}
