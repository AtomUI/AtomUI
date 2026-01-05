namespace AtomUI.Desktop.Controls;

public interface ITreeItemFilter
{
    bool Filter(TreeView treeView, TreeViewItem treeItem, object? filterValue);
}