using System.Collections;
using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public interface ITransferView
{
    IEnumerable? ItemsSource { get; set; }
    IList<EntityKey>? SelectedKeys { get; set; }
    int ItemCount { get; }
    bool IsSupportItemTemplate { get; }
    TransferViewType ViewType { get; set; }
    event EventHandler<TransferItemRemovedEventArgs>? ItemRemoved;
    event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;
    event EventHandler? SelectedKeyChanged;
    
    void SelectAll();
    void DeselectAll();
    void NotifyAboutToTransfer(TransferDirection transferDirection);
    void NotifyTransferCompleted(TransferDirection transferDirection);
    void SetSelectionEnabled(bool enabled);
}