namespace AtomUI.Desktop.Controls.DataLoad;

public interface ICascaderItemDataLoader
{
    Task<CascaderItemLoadResult> LoadAsync(ICascaderViewOption targetNode, CancellationToken token);
}