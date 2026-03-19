using System.Collections;
using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public interface ITransferView
{
    IList<EntityKey>? SelectedKeys { get; set; }
    int ItemCount { get; }
    bool IsSupportItemTemplate { get; }
    bool IsSupportPagination { get; }
    TransferViewType ViewType { get; set; }
  
    event EventHandler<TransferItemRemovedEventArgs>? ItemRemoved;
    event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;
    event EventHandler? SelectedKeyChanged;
    
    void SelectAll();
    void DeselectAll();
    void NotifyAboutToTransfer(TransferDirection transferDirection);
    void NotifyTransferCompleted(TransferDirection transferDirection);
    void NotifySelectAction(TransferSelectAction selectAction);
    void SetSelectionEnabled(bool enabled);
    void SetItemsSource(IEnumerable? itemsSource);
    void SetPaginationEnabled(bool enabled);
    void SetPageSize(int pageSize);
}