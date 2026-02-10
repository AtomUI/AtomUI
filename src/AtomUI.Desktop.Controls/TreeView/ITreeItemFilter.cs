namespace AtomUI.Desktop.Controls;

public interface ITreeItemFilter
{
    bool Filter(TreeView treeView, TreeItem treeItem, object? filterValue);
}