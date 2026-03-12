using System.Collections;
using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public interface ITransferView
{
    IEnumerable? ItemsSource { get; set; }
    IList<EntityKey>? SelectedKeys { get; set; }
    int ItemCount { get; }
    TransferViewType ViewType { get; set; }
    event EventHandler<TransferItemDeletedEventArgs>? ItemDeleted;
    event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;
    event EventHandler? SelectedKeyChanged;
    
    void SelectAll();
    void DeselectAll();
    void NotifyAboutToTransfer(TransferDirection transferDirection);
    void NotifyTransferCompleted(TransferDirection transferDirection);
}