namespace AtomUI.Desktop.Controls;

public interface ITreeItemFilter
{
    bool Filter(TreeView treeView, TreeViewItem treeViewItem, object? filterValue);
}