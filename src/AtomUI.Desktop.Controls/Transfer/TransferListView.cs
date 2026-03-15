using System.Collections;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Data;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using VirtualizingStackPanel = Avalonia.Controls.VirtualizingStackPanel;

namespace AtomUI.Desktop.Controls;

public class TransferListView : ListBox, ITransferView
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
    
    public static readonly StyledProperty<int> PageSizeProperty =
        Transfer.PageSizeProperty.AddOwner<TransferListView>();

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

    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
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
    private IListCollectionView? _listCollectionView;
    private SimplePagination? _pagination;
    
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
    
    protected override void PrepareListBoxItem(ListBoxItem listItem, object? item, int index,
                                               CompositeDisposable disposables)
    {
        base.PrepareListBoxItem(listItem, item, index, disposables);
        if (listItem is TransferListItem transferListItem)
        {
            disposables.Add(BindUtils.RelayBind(this, IsItemSelectableProperty, transferListItem, TransferListItem.IsCheckableProperty));
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            Selection.Clear();
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
    
    public void DeselectAll()
    {
        Selection.Clear();
    }

    void ITransferView.SetPaginationEnabled(bool enabled)
    {
        SetCurrentValue(IsPaginationEnabledProperty, enabled);
    }

    void ITransferView.SetItemsSource(IEnumerable? itemsSource)
    {
        if (!IsPaginationEnabled)
        {
            SetCurrentValue(ItemsSourceProperty, itemsSource);
            _listCollectionView = null;
        }
        else
        {
            var oldCollectionView = _listCollectionView;
            var newItemsSource = itemsSource;
        
            _listCollectionView = null;
            IListCollectionView? newCollectionView;
            if (newItemsSource is IListCollectionView)
            {
                newCollectionView   = (IListCollectionView)newItemsSource;
            }
            else
            {
                newCollectionView = newItemsSource is not null
                    ? CreateView(newItemsSource)
                    : default;
            }
            if (oldCollectionView != null)
            {
                if (oldCollectionView is ListCollectionView oldDataGridCollectionView)
                {
                    oldDataGridCollectionView.PageChanging -= HandlePageChanging;
                }
            }
            if (newCollectionView != null)
            {
                if (newCollectionView is ListCollectionView newDataGridCollectionView)
                {
                    newDataGridCollectionView.PageChanging += HandlePageChanging;
                }
            }
       
            _listCollectionView = newCollectionView;
            
            if (oldCollectionView != newCollectionView)
            {
                SetCurrentValue(ItemsSourceProperty, _listCollectionView);
            }
            ReConfigurePagination();
        }
    }
    
    internal static IListCollectionView CreateView(IEnumerable source)
    {
        Debug.Assert(source != null, "source unexpectedly null");
        Debug.Assert(!(source is IListCollectionView), "source is an IListCollectionView");

        IListCollectionView? collectionView = null;

        if (source is IListCollectionViewFactory collectionViewFactory)
        {
            // If the source is a collection view factory, give it a chance to produce a custom collection view.
            collectionView = collectionViewFactory.CreateView();
            // Intentionally not catching potential exception thrown by ICollectionViewFactory.CreateView().
        }
        if (collectionView == null)
        {
            // If we still do not have a collection view, default to a PagedCollectionView.
            collectionView = new ListCollectionView(source);
        }
        return collectionView;
    }
    
    private void HandlePageChanging(object? sender, PageChangingEventArgs args)
    {
        var targetPage = args.NewPageIndex + 1;
        if (_pagination != null && _pagination.CurrentPage != targetPage)
        {
            _pagination.CurrentPage = targetPage;
        }
    }
    
    private void ReConfigurePagination()
    {
        if (IsSupportPagination && IsPaginationEnabled)
        {
            if (_listCollectionView is ListCollectionView collectionView)
            {
                collectionView.PageSize = PageSize;
                if (_pagination != null)
                {
                    _pagination.Total       = collectionView.ItemCount;
                    _pagination.PageSize    = PageSize;
                    _pagination.CurrentPage = Pagination.DefaultCurrentPage;
                }
            }
        }
    }
    
    void ITransferView.NotifyAboutToTransfer(TransferDirection transferDirection)
    {
        _selectedKeysBackup = SelectedKeys;
        _currentPageSizeBackup = _pagination?.CurrentPage ?? 1;
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

        if (IsPaginationEnabled && _pagination != null)
        {
            var pageCount = _pagination.PageCount;
            var newPageCount = _currentPageSizeBackup <= pageCount ? _currentPageSizeBackup : pageCount;
            _pagination.CurrentPage = newPageCount;
        }
        _selectedKeysBackup    = null;
        _currentPageSizeBackup = 1;
    }

    void ITransferView.SetSelectionEnabled(bool enabled)
    {
        SetCurrentValue(IsItemSelectableProperty, enabled);
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
            listItem.SetCurrentValue(IsSelectedProperty, checkBox.IsChecked == true);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _pagination = e.NameScope.Find<SimplePagination>("Pagination");
        if (_pagination != null)
        {
            _pagination.CurrentPageChanged += HandlePageChangeRequest;
        }
    }
    
    private void HandlePageChangeRequest(object? sender, PageChangedEventArgs args)
    {
        if (_listCollectionView is ListCollectionView collectionView)
        {
            collectionView.MoveToPage(args.PageIndex - 1);
        }
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        var selection = this.GetSelection();
        if (selection == null)
        {
            selection = new TransferListViewSelectionModel
            {
                SingleSelect = !SelectionMode.HasAllFlags(SelectionMode.Multiple),
            }; 
            this.InitializeSelectionModel(selection);
        }
        Console.WriteLine(selection.Count);
 
        base.PrepareContainerForItemOverride(container, item, index);
    }
}