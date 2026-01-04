namespace AtomUI.Desktop.Controls;

public class TreeViewItemLoadedEventArgs : EventArgs
{
    public TreeItemLoadResult Result { get; }
    public TreeViewItem Target { get; }
    
    public TreeViewItemLoadedEventArgs(TreeViewItem target, TreeItemLoadResult result)
    {
        Target = target;
        Result = result;
    }
}