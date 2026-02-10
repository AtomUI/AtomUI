namespace AtomUI.Desktop.Controls;

public class TreeViewItemLoadedEventArgs : EventArgs
{
    public TreeItemLoadResult Result { get; }
    public TreeItem Target { get; }
    
    public TreeViewItemLoadedEventArgs(TreeItem target, TreeItemLoadResult result)
    {
        Target = target;
        Result = result;
    }
}