namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="TreeView.TreeItemLoaded" /> event: the outcome of loading the children of one node,
/// and the node itself.
/// </summary>
public class TreeViewItemLoadedEventArgs : EventArgs
{
    /// <summary>
    /// The outcome of the load: the children when it succeeded, otherwise the failure.
    /// </summary>
    public TreeItemLoadResult Result { get; }
    /// <summary>
    /// The node whose children were loaded.
    /// </summary>
    public TreeViewItem Target { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TreeViewItemLoadedEventArgs" /> class.
    /// </summary>
    /// <param name="target">
    /// The node whose children were loaded.
    /// </param>
    /// <param name="result">
    /// The outcome of the load.
    /// </param>
    public TreeViewItemLoadedEventArgs(TreeViewItem target, TreeItemLoadResult result)
    {
        Target = target;
        Result = result;
    }
}