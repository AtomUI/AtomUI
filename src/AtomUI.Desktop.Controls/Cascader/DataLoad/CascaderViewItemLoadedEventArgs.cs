namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="CascaderView.ItemAsyncLoaded" /> event, carrying the item of the option whose children
/// were loaded and the outcome of the load.
/// </summary>
public class CascaderViewItemLoadedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="CascaderItemLoadResult" /> that states the outcome: the result the loader returned, or a
    /// failed result built by the view when the load timed out, was canceled or threw.
    /// </summary>
    public CascaderItemLoadResult Result { get; }
    /// <summary>
    /// The item of the option whose children were loaded.
    /// </summary>
    public CascaderViewItem Target { get; }
    
    /// <summary>
    /// Creates the event data that reports <paramref name="result" /> as the outcome of loading the children of
    /// <paramref name="target" />.
    /// </summary>
    public CascaderViewItemLoadedEventArgs(CascaderViewItem target, CascaderItemLoadResult result)
    {
        Target = target;
        Result = result;
    }
}