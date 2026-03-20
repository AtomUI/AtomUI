using System.Collections;
using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

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
    public void NotifyIsOneWay(bool isOneWay) {}
    void SetSelectionEnabled(bool enabled);
    public void SetSelectionsIcon(PathIcon? icon) {}
    void SetItemsSource(IEnumerable? itemsSource);
    public void SetItemTemplate(IDataTemplate? itemTemplate) {}
    public void SetPaginationEnabled(bool enabled) {}
    void SetPageSize(int pageSize);
}

internal interface ITransferDecoratorProvider
{
    void ProvideTransferDecorator(TransferItemDecorator decorator);
}