using System.Collections;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using VirtualizingStackPanel = Avalonia.Controls.VirtualizingStackPanel;

namespace AtomUI.Desktop.Controls;

public class TransferListView : ListView, ITransferView
{
    #region 公共属性定义
    public static readonly DirectProperty<TransferListView, IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferListView, IList<EntityKey>?>(nameof(SelectedKeys), 
            o => o.SelectedKeys,
            (o, v) => o.SelectedKeys = v);
    
    public static readonly DirectProperty<TransferListView, TransferViewType> ViewTypeProperty =
        AvaloniaProperty.RegisterDirect<TransferListView, TransferViewType>(nameof(ViewType), 
            o => o.ViewType,
            (o, v) => o.ViewType = v);
    
    public static readonly StyledProperty<bool> IsPaginationEnabledProperty =
        Transfer.IsPaginationEnabledProperty.AddOwner<TransferListView>();

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
    
    public bool IsSupportItemTemplate => true;
    public bool IsSupportPagination => true;
    #endregion

    #region 公共事件定义

    public event EventHandler<TransferItemRemovedEventArgs>? ItemRemoved;
    public event EventHandler? SelectedKeyChanged;

    #endregion
    
    private bool _ignoreSyncSelection;
    private IList<EntityKey>? _selectedKeysBackup;
    private int _currentPageSizeBackup;
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new VirtualizingStackPanel());

    static TransferListView()
    {
        SelectionModeProperty.OverrideDefaultValue<TransferListView>(SelectionMode.Multiple | SelectionMode.Toggle);
        ItemsPanelProperty.OverrideDefaultValue<TransferListView>(DefaultPanel);
        SelectedKeysProperty.Changed.AddClassHandler<TransferListView>((view, args) => view.HandleSelectedKeysChanged());
        SelectionChangedEvent.AddClassHandler<TransferListView>((view, args) => view.HandleSelectionChanged(args));
        TransferRemoveItemButton.ClickEvent.AddClassHandler<TransferListView>((view, args) => view.HandleRemoveButtonClicked(args));
        CheckBox.ClickEvent.AddClassHandler<TransferListView>((view, args) => view.HandleCheckBoxClicked(args));
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<TransferListView>(false);
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TransferListItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TransferListItem>(item, out recycleKey);
    }
    
    protected override void PrepareListViewItem(ListViewItem listItem, object? item, int index, CompositeDisposable disposables)
    {
        base.PrepareListViewItem(listItem, item, index, disposables);
        if (listItem is TransferListItem transferListItem)
        {
            disposables.Add(BindUtils.RelayBind(this, IsSelectableProperty, transferListItem, TransferListItem.IsCheckableProperty));
        }
    }

    private void HandleSelectionChanged(SelectionChangedEventArgs e)
    {
        _ignoreSyncSelection = true;
        if (SelectedItems == null || SelectedItems.Count == 0)
        {
            SetCurrentValue(SelectedKeysProperty, null);
        }
        else
        {
            var selectedKeys = new List<EntityKey>();
            foreach (var item in SelectedItems)
            {
                if (item is IListItemData listItemData && listItemData.IsEnabled)
                {
                    selectedKeys.Add(listItemData.ItemKey ?? default);
                }
            }
            SetCurrentValue(SelectedKeysProperty, selectedKeys);
        }
    }

    private void HandleSelectedKeysChanged()
    {
        SelectedKeyChanged?.Invoke(this, EventArgs.Empty);
        if (_ignoreSyncSelection)
        {
            _ignoreSyncSelection = false;
            return;
        }
        var selectedItems = ItemsSource?.Cast<IItemKey>().Where(item => SelectedKeys?.Contains(item.ItemKey ?? default) ?? false).ToList();
        SetCurrentValue(SelectedItemsProperty, selectedItems);
    }
    
    public void DeselectAll() => Selection.Clear();

    void ITransferView.SetPaginationEnabled(bool enabled)
    {
        SetCurrentValue(IsPaginationEnabledProperty, enabled);
    }

    void ITransferView.SetItemsSource(IEnumerable? itemsSource)
    {
        SetCurrentValue(ItemsSourceProperty, itemsSource);
    }
    
    void ITransferView.NotifyAboutToTransfer(TransferDirection transferDirection)
    {
        _selectedKeysBackup    = SelectedKeys;
        if (IsPaginationEnabled && BottomPagination != null)
        {
            if (ItemsSource is IListCollectionView listCollectionView)
            {
                _currentPageSizeBackup = listCollectionView.PageIndex;
            }
        }
    }
    
    void ITransferView.NotifyTransferCompleted(TransferDirection transferDirection)
    {
        if (ViewType == TransferViewType.Source && transferDirection == TransferDirection.ToSource)
        {
            SetCurrentValue(SelectedKeysProperty, _selectedKeysBackup);
        }
        else if (ViewType == TransferViewType.Target && transferDirection == TransferDirection.ToTarget)
        {
            SetCurrentValue(SelectedKeysProperty, _selectedKeysBackup);
        }
        else
        {
            SetCurrentValue(SelectedKeysProperty, null);
        }
        
        if (IsPaginationEnabled && BottomPagination != null)
        {
            if (ItemsSource is IListCollectionView listCollectionView)
            {
                var pageCount    = listCollectionView.TotalItemCount / listCollectionView.PageSize;
                var newPageCount = _currentPageSizeBackup <= pageCount ? _currentPageSizeBackup : pageCount;
                listCollectionView.MoveToPage(newPageCount);
            }
        }
        
        _selectedKeysBackup    = null;
        _currentPageSizeBackup = 0;
    }

    void ITransferView.SetSelectionEnabled(bool enabled)
    {
        SetCurrentValue(IsSelectableProperty, enabled);
        if (enabled)
        {
            SetCurrentValue(SelectionModeProperty, SelectionMode.Multiple | SelectionMode.Toggle);
        }
    }

    void ITransferView.SetPageSize(int pageSize)
    {
        SetCurrentValue(PageSizeProperty, pageSize);
    }

    private void HandleRemoveButtonClicked(RoutedEventArgs e)
    {
        if (e.Source is TransferRemoveItemButton && GetContainerFromEventSource(e.Source) is TransferListItem listItem)
        {
            if (listItem.DataContext is IItemKey itemKey)
            {
                ItemRemoved?.Invoke(this, new TransferItemRemovedEventArgs(itemKey));
            }
        }
    }

    private void HandleCheckBoxClicked(RoutedEventArgs e)
    {
        if (e.Source is CheckBox checkBox && GetContainerFromEventSource(e.Source) is TransferListItem listItem)
        {
            UpdateSelection(listItem, checkBox.IsChecked == true, false, true);
        }
    }

    public void SelectAll() => Selection.SelectAll();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SetCurrentValue(BottomPaginationProperty, new SimplePagination()
        {
            IsReadOnly = false,
            SizeType = SizeType.Small
        });
    }

    public void NotifySelectAction(TransferSelectAction selectAction)
    {
        if (selectAction == TransferSelectAction.SelectCurrentPage)
        {
            if (IsPaginationEnabled)
            {
                var startIndex = GlobalIndex(0);
                var endIndex = GlobalIndex(ItemCount - 1);
                Selection.SelectRange(startIndex, endIndex);
            }
            else
            {
                Selection.SelectAll();   
            }
        }
        else if (selectAction == TransferSelectAction.InvertSelectCurrentPage)
        {
            for (var i = 0; i < ItemCount; i++)
            {
                var globalIndex = GlobalIndex(i);
                if (Selection.IsSelected(globalIndex))
                {
                    Selection.Deselect(globalIndex);
                }
                else
                {
                    Selection.Select(globalIndex);
                }
            }
        }
    }
}