namespace AtomUI.Desktop.Controls;

public interface ITreeItemDataLoader
{
    Task<TreeItemLoadResult> LoadAsync(ITreeViewItemData targetTreeItem, CancellationToken token);
}