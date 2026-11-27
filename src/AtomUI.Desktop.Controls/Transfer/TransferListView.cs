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

/// <summary>
/// The list view of one panel of a transfer. It shows the items of the panel as <see cref="TransferListItem" />
/// rows, keeps <see cref="SelectedKeys" /> in step with the checked rows, and reports rows to remove through
/// <see cref="ItemsRemoved" /> without removing anything itself. The item decorator of the panel drives the view
/// through <see cref="ITransferView" />.
/// </summary>
public partial class TransferListView : ListView, ITransferView
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="SelectedKeys" /> property.
    /// </summary>
    public static readonly DirectProperty<TransferListView, IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferListView, IList<EntityKey>?>(nameof(SelectedKeys), 
            o => o.SelectedKeys,
            (o, v) => o.SelectedKeys = v,
            defaultBindingMode: BindingMode.TwoWay);
    
    /// <summary>
    /// Identifies the <see cref="ViewType" /> property.
    /// </summary>
    public static readonly DirectProperty<TransferListView, TransferViewType> ViewTypeProperty =
        AvaloniaProperty.RegisterDirect<TransferListView, TransferViewType>(nameof(ViewType), 
            o => o.ViewType,
            (o, v) => o.ViewType = v);
    
    /// <summary>
    /// Identifies the <see cref="IsPaginationEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPaginationEnabledProperty =
        AbstractTransfer.IsPaginationEnabledProperty.AddOwner<TransferListView>();

    private IList<EntityKey>? _selectedKeys;
    /// <summary>
    /// The keys of the rows checked in this panel. The owning transfer reads and sets the value through
    /// <see cref="ITransferView" />. The value and the row selection follow each other: a new value replaces the row
    /// selection with the rows whose items carry these keys, and a selection made in the view replaces the value with
    /// a new list of the keys of the selected rows whose item is enabled, or with <see langword="null" /> when no row
    /// is selected. Changing the items source drops the keys that the new source does not contain. While the view is
    /// attached, a collection that implements <see cref="INotifyCollectionChanged" /> has its change notifications
    /// applied to the rows as well.
    /// </summary>
    public IList<EntityKey>? SelectedKeys
    {
        get => _selectedKeys;
        set => SetAndRaise(SelectedKeysProperty, ref _selectedKeys, value);
    }
    
    private TransferViewType _viewType;
    /// <summary>
    /// The panel the view belongs to. The item decorator of the panel sets it, and the view adds the class of that
    /// panel, <see cref="ListTransferSemanticParts.SourceItemClass" /> or
    /// <see cref="ListTransferSemanticParts.TargetItemClass" />, to every row. The default is
    /// <see cref="TransferViewType.Source" />.
    /// </summary>
    public TransferViewType ViewType
    {
        get => _viewType;
        set => SetAndRaise(ViewTypeProperty, ref _viewType, value);
    }
    
    /// <summary>
    /// Whether the rows are paged. The item decorator of the panel sets it through
    /// <c>ITransferView.SetPaginationEnabled</c>. When it is <see langword="false" />, the theme hides the pagination
    /// presenters and the current page actions of <see cref="NotifySelectAction" /> cover every row.
    /// </summary>
    public bool IsPaginationEnabled
    {
        get => GetValue(IsPaginationEnabledProperty);
        set => SetValue(IsPaginationEnabledProperty, value);
    }
    
    /// <summary>
    /// Always <see langword="true" />: the view renders an item template, so the item decorator forwards its own
    /// template through <c>ITransferView.SetItemTemplate</c>.
    /// </summary>
    public bool IsSupportItemTemplate => true;
    /// <summary>
    /// Always <see langword="true" />: the item decorator forwards its pagination setting and page size to the view
    /// through <c>ITransferView.SetPaginationEnabled</c> and <c>ITransferView.SetPageSize</c>.
    /// </summary>
    public bool IsSupportPagination => true;
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when the remove button of a row is clicked, with the item of that row, and when
    /// <see cref="NotifySelectAction" /> receives <see cref="TransferSelectAction.RemoveCurrentPage" />, with the
    /// items of the current page. The view removes nothing itself; the owning transfer removes the reported keys from
    /// its target keys.
    /// </summary>
    public event EventHandler<TransferItemsRemovedEventArgs>? ItemsRemoved;
    /// <summary>
    /// Raised after <see cref="SelectedKeys" /> changed, before the rows are made to follow it. Also raised
    /// when the current collection raises <c>CollectionChanged</c>.
    /// </summary>
    public event EventHandler? SelectedKeyChanged;
    /// <summary>
    /// Raised right after <see cref="SelectedKeyChanged" />, with the number of keys <see cref="SelectedKeys" />
    /// holds.
    /// </summary>
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

    /// <summary>
    /// Drops the selected keys that a new items source does not contain. Call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            HandleItemsSourceChange();
        }
    }

    /// <summary>
    /// Applies <see cref="SelectedKeys" /> to the rows once the view is attached. Call the base member first; row
    /// selection changes raised while it runs are not written back to <see cref="SelectedKeys" />.
    /// </summary>
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

    /// <summary>
    /// Stops following change notifications of the <see cref="SelectedKeys" /> collection while the view is
    /// detached, then calls the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isVisualTreeAttached = false;
        ReleaseSelectedKeysCollectionChangedSource();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Creates the row container, a <see cref="TransferListItem" /> that carries the list view item class.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var listItem = new TransferListItem();
        listItem.Classes.Add(ListViewSemanticParts.ItemClass);
        return listItem;
    }

    /// <summary>
    /// Tells whether an item needs a container; an item that is already a
    /// <see cref="TransferListItem" /> does not.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TransferListItem>(item, out recycleKey);
    }

    /// <summary>
    /// Adds the list view item class and the source or target item class chosen by <see cref="ViewType" /> to a
    /// <see cref="TransferListItem" />, and binds its <c>IsCheckable</c> property to the
    /// <c>IsSelectable</c> property of the view, so that a row is checkable only while the view is selectable. The base
    /// member is called first.
    /// </summary>
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

    /// <summary>
    /// Clears the row selection. This replaces <see cref="SelectedKeys" /> with <see langword="null" /> unless it is
    /// already empty.
    /// </summary>
    public void DeselectAll() => Selection.Clear();

    /// <summary>
    /// Selects every row, which replaces <see cref="SelectedKeys" /> with the keys of the selected rows whose item is
    /// enabled.
    /// </summary>
    public void SelectAll() => Selection.SelectAll();

    /// <summary>
    /// Replaces the bottom pagination with a new small <see cref="SimplePagination" /> whose <c>IsReadOnly</c> is
    /// <see langword="false" />. The base member is called first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        SetCurrentValue(BottomPaginationProperty, new SimplePagination()
        {
            IsReadOnly = false,
            SizeType = CustomizableSizeType.Small
        });
    }

    /// <summary>
    /// Applies a selection action of the select menu of the panel to the rows.
    /// <see cref="TransferSelectAction.SelectCurrentPage" /> selects the rows of the current page, or every row when
    /// <see cref="IsPaginationEnabled" /> is <see langword="false" />;
    /// <see cref="TransferSelectAction.InvertSelectCurrentPage" /> toggles the rows of the current page;
    /// <see cref="TransferSelectAction.RemoveCurrentPage" /> raises <see cref="ItemsRemoved" /> with the items of
    /// the current page and removes nothing itself. This method does nothing for the other actions.
    /// </summary>
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
