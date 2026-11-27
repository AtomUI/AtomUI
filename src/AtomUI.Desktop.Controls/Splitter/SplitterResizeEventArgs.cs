namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data of the <see cref="Splitter.ResizeStarted" />, <see cref="Splitter.ResizeDelta" /> and
/// <see cref="Splitter.ResizeCompleted" /> events: the dragged split bar handle and the pane sizes at that
/// moment of the drag.
/// </summary>
public class SplitterResizeEventArgs : EventArgs
{
    /// <summary>
    /// Creates the event data of a drag of one split bar handle.
    /// </summary>
    /// <param name="handleIndex">
    /// The zero-based index of the dragged handle.
    /// </param>
    /// <param name="sizes">
    /// The size of every pane along the split axis, in pane order.
    /// </param>
    public SplitterResizeEventArgs(int handleIndex, IReadOnlyList<double> sizes)
    {
        HandleIndex = handleIndex;
        Sizes = sizes;
    }

    /// <summary>
    /// The zero-based index of the dragged split bar handle. Handle <c>i</c> lies between pane <c>i</c> and pane
    /// <c>i + 1</c>.
    /// </summary>
    public int HandleIndex { get; }
    /// <summary>
    /// The size of every pane along the split axis, in pane order, as it results from the reported step of the
    /// drag. Index <c>i</c> belongs to pane <c>i</c>.
    /// </summary>
    public IReadOnlyList<double> Sizes { get; }
}
