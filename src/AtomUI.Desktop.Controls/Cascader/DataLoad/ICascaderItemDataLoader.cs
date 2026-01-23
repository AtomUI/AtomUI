namespace AtomUI.Desktop.Controls.DataLoad;

public interface ICascaderItemDataLoader
{
    Task<CascaderItemLoadResult> LoadAsync(ICascaderViewItemData targetCascaderItem, CancellationToken token);
}