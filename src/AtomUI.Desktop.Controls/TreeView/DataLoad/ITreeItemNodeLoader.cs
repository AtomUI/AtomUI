namespace AtomUI.Desktop.Controls;

public interface ITreeItemNodeLoader
{
    Task<TreeItemLoadResult> LoadAsync(ITreeItemNode targetTreeItem, CancellationToken token);
}