namespace AtomUI.Desktop.Controls;

public interface ITreeNodeDataLoader
{
    Task<TreeNodeLoadResult> LoadAsync(ITreeViewItemData targetTreeItem, CancellationToken token);
}