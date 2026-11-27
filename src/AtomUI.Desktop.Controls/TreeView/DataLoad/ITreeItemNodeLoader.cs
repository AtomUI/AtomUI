namespace AtomUI.Desktop.Controls;

/// <summary>
/// Loads the children of tree nodes on demand for a control whose <c>DataLoader</c> is set, such as a tree
/// view or a tree select.
/// </summary>
public interface ITreeItemNodeLoader
{
    /// <summary>
    /// Loads the children of the node given as <paramref name="targetTreeItem" />. The node shows its loading state
    /// until the task completes.
    /// </summary>
    /// <param name="targetTreeItem">
    /// The data item of the node whose children are loaded.
    /// </param>
    /// <param name="token">
    /// Canceled when the load takes longer than the tree view's <see cref="TreeView.AsyncLoadTimeout" />.
    /// </param>
    /// <returns>
    /// A task that completes when the load finished. The tree view raises
    /// <see cref="TreeView.TreeItemLoaded" /> with the result. It adds the children as nodes and expands the node
    /// only when <see cref="TreeItemLoadResult.IsSuccess" /> is <see langword="true" /> and
    /// <see cref="TreeItemLoadResult.Data" /> has entries; a load that did not succeed, for example a timeout, is
    /// reported as a failed result.
    /// </returns>
    Task<TreeItemLoadResult> LoadAsync(ITreeItemNode targetTreeItem, CancellationToken token);
}