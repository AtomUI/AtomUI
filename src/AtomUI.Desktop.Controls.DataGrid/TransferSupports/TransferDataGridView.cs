using System.Collections;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Data;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public class TransferDataGridView : DataGrid, 
                                    ITransferView,
                                    ITransferDecoratorProvider
{
    #region 公共属性定义
    public static readonly DirectProperty<TransferDataGridView, IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferDataGridView, IList<EntityKey>?>(nameof(SelectedKeys), 
            o => o.SelectedKeys,
            (o, v) => o.SelectedKeys = v);
    
    public static readonly DirectProperty<TransferDataGridView, TransferViewType> ViewTypeProperty =
        AvaloniaProperty.RegisterDirect<TransferDataGridView, TransferViewType>(nameof(ViewType), 
            o => o.ViewType,
            (o, v) => o.ViewType = v);
    
    public static readonly StyledProperty<bool> IsPaginationEnabledProperty =
        AbstractTransfer.IsPaginationEnabledProperty.AddOwner<TransferDataGridView>();
    
    public static readonly DirectProperty<TransferDataGridView, int> ItemCountProperty =
        AvaloniaProperty.RegisterDirect<TransferDataGridView, int>(nameof(ItemCount), 
            o => o.ItemCount);
    
    public static readonly DirectProperty<TransferDataGridView, PathIcon?> SelectionsIconProperty =
        AvaloniaProperty.RegisterDirect<TransferDataGridView, PathIcon?>(nameof(SelectionsIcon),
            o => o.SelectionsIcon,
            (o, v) => o.SelectionsIcon = v);
    
    public static readonly StyledProperty<bool> IsOneWayProperty =
        AbstractTransfer.IsOneWayProperty.AddOwner<TransferDataGridView>();
    
    private IList<EntityKey>? _selectedKeys;
    public IList<EntityKey>? SelectedKeys
    {
        get => _selectedKeys;
        set => SetAndRaise(SelectedKeysProperty, ref _selectedKeys, value);
    }
    
    private TransferViewType _viewType;
    public TransferViewType ViewType
    {
        get => _viewType;
        set => SetAndRaise(ViewTypeProperty, ref _viewType, value);
    }
    
    public bool IsPaginationEnabled
    {
        get => GetValue(IsPaginationEnabledProperty);
        set => SetValue(IsPaginationEnabledProperty, value);
    }
    
    private int _itemCount;
    public int ItemCount
    {
        get => _itemCount;
        set => SetAndRaise(ItemCountProperty, ref _itemCount, value);
    }
    
    private PathIcon? _selectionsIcon;
    internal PathIcon? SelectionsIcon
    {
        get => _selectionsIcon;
        set => SetAndRaise(SelectionsIconProperty, ref _selectionsIcon, value);
    }
    
    public bool IsOneWay
    {
        get => GetValue(IsOneWayProperty);
        set => SetValue(IsOneWayProperty, value);
    }

    public bool IsSupportItemTemplate => false;
    public bool IsSupportPagination => true;
    
    #endregion
    
    #region 公共事件定义

    public event EventHandler<TransferItemRemovedEventArgs>? ItemRemoved;
    public event EventHandler? SelectedKeyChanged;
    public event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;

    #endregion

    protected override void NotifyDataCollectionViewChanged(DataGridCollectionView view, NotifyCollectionChangedEventArgs args)
    {
        ItemCount = view.TotalItemCount;
        ItemCountChanged?.Invoke(this, new ItemCountChangedEventArgs(ItemCount));
    }

    public void DeselectAll() => ClearRowSelection(true);

    void ITransferView.NotifyAboutToTransfer(TransferDirection transferDirection)
    {
        
    }

    void ITransferView.NotifyTransferCompleted(TransferDirection transferDirection)
    {
    }
    
    void ITransferView.SetSelectionEnabled(bool enabled)
    {
        if (enabled)
        {
            SetCurrentValue(SelectionModeProperty, DataGridSelectionMode.Extended);
        }
        else
        {
            SetCurrentValue(SelectionModeProperty, DataGridSelectionMode.None);
        }
    }

    void ITransferView.SetPageSize(int pageSize)
    {
        SetCurrentValue(PageSizeProperty, pageSize);
    }

    void ITransferView.SetPaginationEnabled(bool enabled)
    {
        SetCurrentValue(IsPaginationEnabledProperty, enabled);
    }

    void ITransferView.SetItemsSource(IEnumerable? itemsSource)
    {
        SetCurrentValue(ItemsSourceProperty, itemsSource);
    }

    void ITransferView.SetSelectionsIcon(PathIcon? icon)
    {
        SetCurrentValue(SelectionsIconProperty, icon);
    }
    
    void ITransferView.NotifyIsOneWay(bool isOneWay)
    {
        SetCurrentValue(IsOneWayProperty, isOneWay);
    }

    void ITransferDecoratorProvider.ProvideTransferDecorator(TransferItemDecorator decorator)
    {
        decorator.IsShowSelectAllCheckbox  = false;
        decorator.IsShowSelectDropdownMenu = false;
    }

    public void NotifySelectAction(TransferSelectAction selectAction)
    {
    }
}