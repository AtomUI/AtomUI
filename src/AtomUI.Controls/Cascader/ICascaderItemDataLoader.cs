namespace AtomUI.Desktop.Controls.DataLoad;

public interface ICascaderItemDataLoader
{
    Task<CascaderItemLoadResult> LoadAsync(ICascaderOption targetNode, CancellationToken token);
}