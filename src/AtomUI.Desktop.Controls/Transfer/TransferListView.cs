using System.Collections;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using VirtualizingStackPanel = Avalonia.Controls.VirtualizingStackPanel;

namespace AtomUI.Desktop.Controls;

public partial class TransferListView : ListView, ITransferView
{
    #region 公共属性定义
    public static readonly DirectProperty<TransferListView, IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferListView, IList<EntityKey>?>(nameof(SelectedKeys), 
            o => o.SelectedKeys,
            (o, v) => o.SelectedKeys = v,
            defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly DirectProperty<TransferListView, TransferViewType> ViewTypeProperty =
        AvaloniaProperty.RegisterDirect<TransferListView, TransferViewType>(nameof(ViewType), 
            o => o.ViewType,
            (o, v) => o.ViewType = v);
    
    public static readonly StyledProperty<bool> IsPaginationEnabledProperty =
        AbstractTransfer.IsPaginationEnabledProperty.AddOwner<TransferListView>();

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

    public event EventHandler<TransferItemsRemovedEventArgs>? ItemsRemoved;
    public event EventHandler? SelectedKeyChanged;
    public event EventHandler<SelectionCountChangedEventArgs>? SelectionCountChanged;

    #endregion
    
    private bool _isApplyingSelectionToSelectedKeys;
    private bool _isApplyingSelectedKeysToSelection;
    private IList<EntityKey>? _selectedKeysBackup;
    private int _currentPageSizeBackup;
    private INotifyCollectionChanged? _selectedKeysCollectionChangedSource;
    private bool _isVisualTreeAttached;
    
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            HandleItemsSourceChange();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var wasApplyingSelectedKeysToSelection = _isApplyingSelectedKeysToSelection;
        _isApplyingSelectedKeysToSelection = true;
        try
        {
            base.OnAttachedToVisualTree(e);
        }
        finally
        {
            _isApplyingSelectedKeysToSelection = wasApplyingSelectedKeysToSelection;
        }
        _isVisualTreeAttached = true;
        ConfigureSelectedKeysCollectionChangedSource(SelectedKeys);
        HandleSelectedKeysChanged();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isVisualTreeAttached = false;
        ReleaseSelectedKeysCollectionChangedSource();
        base.OnDetachedFromVisualTree(e);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var listItem = new TransferListItem();
        listItem.Classes.Add(ListViewSemanticParts.ItemClass);
        return listItem;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TransferListItem>(item, out recycleKey);
    }

    protected override void PrepareListViewItem(ListViewItem listItem, object? item, int index)
    {
        base.PrepareListViewItem(listItem, item, index);
        if (listItem is TransferListItem transferListItem)
        {
            transferListItem.Classes.Add(ListViewSemanticParts.ItemClass);
            transferListItem.Classes.Add(GetDirectionItemClass());
            transferListItem[!TransferListItem.IsCheckableProperty] = this[!IsSelectableProperty];
        }
    }

    private string GetDirectionItemClass()
    {
        return ViewType == TransferViewType.Source
            ? ListTransferSemanticParts.SourceItemClass
            : ListTransferSemanticParts.TargetItemClass;
    }

    public void DeselectAll() => Selection.Clear();

    public void SelectAll() => Selection.SelectAll();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SetCurrentValue(BottomPaginationProperty, new SimplePagination()
        {
            IsReadOnly = false,
            SizeType = CustomizableSizeType.Small
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
        else if (selectAction == TransferSelectAction.RemoveCurrentPage)
        {
            ItemsRemoved?.Invoke(this, new TransferItemsRemovedEventArgs(BuildItemsList(Items)));
        }
    }

    #region 实现 ITransferView

    void ITransferView.SetPaginationEnabled(bool enabled)
    {
        SetCurrentValue(IsPaginationEnabledProperty, enabled);
    }

    void ITransferView.SetItemsSource(IEnumerable? itemsSource)
    {
        SetCurrentValue(ItemsSourceProperty, itemsSource);
    }

    void ITransferView.SetItemTemplate(IDataTemplate? itemTemplate)
    {
        SetCurrentValue(ItemTemplateProperty, itemTemplate);
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

    #endregion

    private void HandleItemsSourceChange()
    {
        if (SelectedKeys != null)
        {
            var newSelectedKeys = new List<EntityKey>(SelectedKeys.Count);
            if (SelectedKeys.Count > 0 && ItemsSource != null)
            {
                var allItems = BuildItemKeySet(ItemsSource);
                foreach (var selectedKey in SelectedKeys)
                {
                    if (allItems.Contains(selectedKey))
                    {
                        newSelectedKeys.Add(selectedKey);
                    }
                }
            }
            if (!AreKeyCollectionsEquivalent(SelectedKeys, newSelectedKeys))
            {
                SetCurrentValue(SelectedKeysProperty, newSelectedKeys);
            }
        }
    }

    private void HandleSelectionChanged(ListViewSelectionChangedEventArgs e)
    {
        if (_isApplyingSelectedKeysToSelection)
        {
            return;
        }

        _isApplyingSelectionToSelectedKeys = true;
        try
        {
            if (e.SelectedItems.Count == 0 && Selection.SelectedItems.Count == 0)
            {
                if (!AreKeyCollectionsEquivalent(SelectedKeys, null))
                {
                    SetCurrentValue(SelectedKeysProperty, null);
                }
            }
            else
            {
                var selectedItems = Selection.SelectedItems;
                var selectedKeys = new List<EntityKey>(selectedItems.Count);
                foreach (var item in selectedItems)
                {
                    if (item is IListItemData listItemData && listItemData.IsEnabled)
                    {
                        selectedKeys.Add(listItemData.ItemKey ?? default);
                    }
                }
                if (!AreKeyCollectionsEquivalent(SelectedKeys, selectedKeys))
                {
                    SetCurrentValue(SelectedKeysProperty, selectedKeys);
                }
            }
        }
        finally
        {
            _isApplyingSelectionToSelectedKeys = false;
        }
    }

    private void HandleSelectedKeysChanged()
    {
        ConfigureSelectedKeysCollectionChangedSource(SelectedKeys);
        SelectedKeyChanged?.Invoke(this, EventArgs.Empty);
        SelectionCountChanged?.Invoke(this, new SelectionCountChangedEventArgs(SelectedKeys?.Count ?? 0));
        if (_isApplyingSelectionToSelectedKeys)
        {
            return;
        }
        var selectedItems = BuildSelectedItemsList(ItemsSource, SelectedKeys);
        _isApplyingSelectedKeysToSelection = true;
        try
        {
            ApplySelectedItemsToSelection(selectedItems);
        }
        finally
        {
            _isApplyingSelectedKeysToSelection = false;
        }
    }

    private void ApplySelectedItemsToSelection(IEnumerable? selectedItems)
    {
        var selectedKeys = selectedItems is null
            ? null
            : BuildItemKeySet(selectedItems);

        var currentKeys = BuildItemKeySet(Selection.SelectedItems);
        if (currentKeys.SetEquals(selectedKeys ?? []))
        {
            return;
        }

        Selection.Clear();
        if (selectedKeys is null || selectedKeys.Count == 0)
        {
            return;
        }

        for (var sourceIndex = 0; TryGetSourceItem(sourceIndex, out var item); sourceIndex++)
        {
            if (item is IItemKey itemKey && selectedKeys.Contains(itemKey.ItemKey))
            {
                Selection.Select(sourceIndex);
            }
        }
    }

    private void ConfigureSelectedKeysCollectionChangedSource(IList<EntityKey>? selectedKeys)
    {
        if (!_isVisualTreeAttached)
        {
            ReleaseSelectedKeysCollectionChangedSource();
            return;
        }

        if (ReferenceEquals(_selectedKeysCollectionChangedSource, selectedKeys))
        {
            return;
        }

        ReleaseSelectedKeysCollectionChangedSource();

        _selectedKeysCollectionChangedSource = selectedKeys as INotifyCollectionChanged;
        if (_selectedKeysCollectionChangedSource != null)
        {
            _selectedKeysCollectionChangedSource.CollectionChanged += HandleSelectedKeysCollectionChanged;
        }
    }

    private void ReleaseSelectedKeysCollectionChangedSource()
    {
        if (_selectedKeysCollectionChangedSource != null)
        {
            _selectedKeysCollectionChangedSource.CollectionChanged -= HandleSelectedKeysCollectionChanged;
            _selectedKeysCollectionChangedSource = null;
        }
    }

    private void HandleSelectedKeysCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (!ReferenceEquals(sender, _selectedKeysCollectionChangedSource))
        {
            return;
        }

        HandleSelectedKeysChanged();
    }

    private void HandleRemoveButtonClicked(RoutedEventArgs e)
    {
        if (e.Source is TransferRemoveItemButton && GetContainerFromEventSource(e.Source) is TransferListItem listItem)
        {
            if (listItem.DataContext is IItemKey itemKey)
            {
                ItemsRemoved?.Invoke(this, new TransferItemsRemovedEventArgs([itemKey]));
            }
        }
    }

    private void HandleCheckBoxClicked(RoutedEventArgs e)
    {
        if (e.Source is CheckBox checkBox && GetContainerFromEventSource(e.Source) is TransferListItem listItem)
        {
            var index = SelectionIndexFromContainer(listItem);
            if (index != -1)
            {
                if (checkBox.IsChecked == true)
                {
                    Selection.Select(index);
                }
                else
                {
                    Selection.Deselect(index);
                }
            }
        }
    }

    private static HashSet<EntityKey?> BuildItemKeySet(IEnumerable source)
    {
        var keySet = source switch
        {
            ICollection collection => new HashSet<EntityKey?>(collection.Count),
            IReadOnlyCollection<IItemKey> collection => new HashSet<EntityKey?>(collection.Count),
            _ => new HashSet<EntityKey?>()
        };
        foreach (var item in source)
        {
            keySet.Add(((IItemKey)item!).ItemKey);
        }
        return keySet;
    }

    private static List<IItemKey>? BuildSelectedItemsList(IEnumerable? source, ICollection<EntityKey>? selectedKeys)
    {
        if (source == null)
        {
            return null;
        }

        if (selectedKeys == null || selectedKeys.Count == 0)
        {
            return new List<IItemKey>(0);
        }

        var selectedKeySet = BuildEntityKeySet(selectedKeys);
        var selectedItems  = new List<IItemKey>(selectedKeys.Count);
        foreach (var item in source)
        {
            var itemKey = (IItemKey)item!;
            if (selectedKeySet!.Contains(itemKey.ItemKey ?? default))
            {
                selectedItems.Add(itemKey);
            }
        }
        return selectedItems;
    }

    private static HashSet<EntityKey>? BuildEntityKeySet(ICollection<EntityKey>? keys)
    {
        if (keys == null || keys.Count == 0)
        {
            return null;
        }

        var keySet = new HashSet<EntityKey>(keys.Count);
        foreach (var key in keys)
        {
            keySet.Add(key);
        }
        return keySet;
    }

    private static bool AreKeyCollectionsEquivalent(ICollection<EntityKey>? currentKeys, ICollection<EntityKey>? nextKeys)
    {
        if (currentKeys == null || currentKeys.Count == 0)
        {
            return nextKeys == null || nextKeys.Count == 0;
        }

        if (nextKeys == null || currentKeys.Count != nextKeys.Count)
        {
            return false;
        }

        var currentKeySet = new HashSet<EntityKey>(currentKeys.Count);
        foreach (var key in currentKeys)
        {
            currentKeySet.Add(key);
        }

        foreach (var key in nextKeys)
        {
            if (!currentKeySet.Contains(key))
            {
                return false;
            }
        }

        return true;
    }

    private static List<IItemKey> BuildItemsList(IEnumerable source)
    {
        var items = source switch
        {
            ICollection collection => new List<IItemKey>(collection.Count),
            IReadOnlyCollection<IItemKey> collection => new List<IItemKey>(collection.Count),
            _ => new List<IItemKey>()
        };
        foreach (var item in source)
        {
            items.Add((IItemKey)item!);
        }
        return items;
    }
}
