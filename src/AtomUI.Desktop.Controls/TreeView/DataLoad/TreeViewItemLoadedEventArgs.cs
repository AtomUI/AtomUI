namespace AtomUI.Desktop.Controls;

public class TreeViewItemLoadedEventArgs : EventArgs
{
    public TreeNodeLoadResult Result { get; }
    public TreeViewItem Target { get; }
    
    public TreeViewItemLoadedEventArgs(TreeViewItem target, TreeNodeLoadResult result)
    {
        Target = target;
        Result = result;
    }
}