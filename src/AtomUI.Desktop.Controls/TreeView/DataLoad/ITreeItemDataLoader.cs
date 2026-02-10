namespace AtomUI.Desktop.Controls;

public interface ITreeItemDataLoader
{
    Task<TreeItemLoadResult> LoadAsync(ITreeItemData targetTreeItem, CancellationToken token);
}