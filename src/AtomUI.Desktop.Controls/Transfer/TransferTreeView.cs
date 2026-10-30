using System.Collections;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public partial class TransferTreeView : TreeView, ITransferTreeView, ITransferDecoratorProvider
{
    #region 公共属性定义
    public static readonly DirectProperty<TransferTreeView, IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferTreeView, IList<EntityKey>?>(nameof(SelectedKeys), 
            o => o.SelectedKeys,
            (o, v) => o.SelectedKeys = v,
            defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly DirectProperty<TransferTreeView, ISet<EntityKey>?> MaskKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferTreeView, ISet<EntityKey>?>(nameof(MaskKeys), 
            o => o.MaskKeys,
            (o, v) => o.MaskKeys = v);
    
    public static readonly DirectProperty<TransferTreeView, TransferViewType> ViewTypeProperty =
        AvaloniaProperty.RegisterDirect<TransferTreeView, TransferViewType>(nameof(ViewType), 
            o => o.ViewType,
            (o, v) => o.ViewType = v);
    
    private IList<EntityKey>? _selectedKeys;
    public IList<EntityKey>? SelectedKeys
    {
        get => _selectedKeys;
        set => SetAndRaise(SelectedKeysProperty, ref _selectedKeys, value);
    }
    
    private ISet<EntityKey>? _maskKeys;
    public ISet<EntityKey>? MaskKeys
    {
        get => _maskKeys;
        set => SetAndRaise(MaskKeysProperty, ref _maskKeys, value);
    }
    
    private TransferViewType _viewType;
    public TransferViewType ViewType
    {
        get => _viewType;
        set => SetAndRaise(ViewTypeProperty, ref _viewType, value);
    }
    
    public bool IsSupportItemTemplate => true;
    public bool IsSupportPagination => false;
    #endregion
    
    #region 公共事件定义
#pragma warning disable CS0067
    public event EventHandler<TransferItemsRemovedEventArgs>? ItemsRemoved;
#pragma warning restore CS0067
    public event EventHandler? SelectedKeyChanged;
    public event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;
    public event EventHandler<SelectionCountChangedEventArgs>? SelectionCountChanged;

    #endregion
    
    private bool _isApplyingCheckedItemsToSelectedKeys;
    private bool _isApplyingSelectedKeysToCheckedItems;
    private INotifyCollectionChanged? _selectedKeysCollectionChangedSource;
    private bool _isVisualTreeAttached;
    
    static TransferTreeView()
    {
        SelectedKeysProperty.Changed.AddClassHandler<TransferTreeView>((view, args) => view.HandleSelectedKeysChanged());
        TransferTreeViewItem.ClickEvent.AddClassHandler<TransferTreeView>((view, args) => view.HandleItemClicked(args));
    }

    public TransferTreeView()
    {
        CheckedItemsChanged += HandleCheckedItemsChanged;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MaskKeysProperty ||
            change.Property == ItemsSourceProperty)
        {
            MaskNodes();
            HandleItemCountChanged();
        }

        if (change.Property == MaskKeysProperty)
        {
            ConfigureSelectedKeys();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var wasApplyingSelectedKeysToCheckedItems = _isApplyingSelectedKeysToCheckedItems;
        _isApplyingSelectedKeysToCheckedItems = true;
        try
        {
            base.OnAttachedToVisualTree(e);
        }
        finally
        {
            _isApplyingSelectedKeysToCheckedItems = wasApplyingSelectedKeysToCheckedItems;
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
        var treeViewItem = new TransferTreeViewItem();
        treeViewItem.Classes.Add(TreeViewSemanticParts.ItemClass);
        return treeViewItem;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TransferTreeViewItem>(item, out recycleKey);
    }

    protected override void PrepareTreeViewItem(TreeViewItem treeViewItem, object? item, int index)
    {
        base.PrepareTreeViewItem(treeViewItem, item, index);
        if (treeViewItem is TransferTreeViewItem transferTreeViewItem)
        {
            transferTreeViewItem.Classes.Add(GetDirectionItemClass());
            PrepareTransferTreeViewItem(transferTreeViewItem, item);
        }
    }

    private string GetDirectionItemClass()
    {
        return ViewType == TransferViewType.Source
            ? TreeTransferSemanticParts.SourceItemClass
            : TreeTransferSemanticParts.TargetItemClass;
    }

    protected override bool RecursiveCheckNodePredicate(TreeViewItem treeViewItem)
    {
        if (treeViewItem is TransferTreeViewItem transferTreeViewItem)
        {
            return !transferTreeViewItem.IsMasked;
        }
        return true;
    }

    protected override bool RecursiveUnCheckNodePredicate(TreeViewItem treeViewItem)
    {
        if (treeViewItem is TransferTreeViewItem transferTreeViewItem)
        {
            return !transferTreeViewItem.IsMasked;
        }
        return true;
    }

    public new void SelectAll()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TreeViewItem treeItem)
            {
                CheckedSubTree(treeItem);
            }
        }
    }

    public void DeselectAll()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is TreeViewItem treeItem)
            {
                UnCheckedSubTree(treeItem);
            }
        }
    }

    #region 实现 ITransferView

    void ITransferView.SetItemsSource(IEnumerable? itemsSource)
    {
        SetCurrentValue(ItemsSourceProperty, itemsSource);
    }

    void ITransferView.NotifyAboutToTransfer(TransferDirection transferDirection)
    {
    }

    void ITransferView.NotifyTransferCompleted(TransferDirection transferDirection)
    {
    }

    void ITransferView.SetSelectionEnabled(bool enabled)
    {
    }

    void ITransferView.SetPageSize(int pageSize)
    {
    }

    void ITransferView.SetItemTemplate(IDataTemplate? itemTemplate)
    {
        if (itemTemplate is ITreeDataTemplate treeDataTemplate)
        {
            SetCurrentValue(ItemTemplateProperty, treeDataTemplate);
        }
        else
        {
            SetCurrentValue(ItemTemplateProperty, itemTemplate);
        }
    }

    void ITransferView.NotifySelectAction(TransferSelectAction selectAction)
    {
    }

    #endregion

    #region 实现 ITransferTreeView

    void ITransferTreeView.SetMaskedItems(IList<EntityKey>? maskedItems)
    {
        SetCurrentValue(MaskKeysProperty, BuildEntityKeySet(maskedItems));
    }

    #endregion

    #region 实现 ITransferDecoratorProvider

    void ITransferDecoratorProvider.ProvideTransferDecorator(TransferItemDecorator decorator)
    {
        decorator.IsShowSelectDropdownMenu = false;
    }

    #endregion

    internal void PrepareTransferTreeViewItem(TransferTreeViewItem treeViewItem, object? item)
    {
        treeViewItem.IsMasked = IsMaskedItem(item);
    }

    private void HandleItemCountChanged()
    {
        var totalCount = 0;
        foreach (var item in Items)
        {
            var node = (ITreeItemNode)item!;
            totalCount += CalculateAllNodesCount(node);
        }
        ItemCountChanged?.Invoke(this, new ItemCountChangedEventArgs(totalCount));
    }

    private void HandleSelectedKeysChanged()
    {
        ConfigureSelectedKeysCollectionChangedSource(SelectedKeys);
        SelectedKeyChanged?.Invoke(this, EventArgs.Empty);
        if (_isApplyingCheckedItemsToSelectedKeys)
        {
            return;
        }
        var checkedItems = BuildCheckedItemsList(ItemsSource, SelectedKeys);
        _isApplyingSelectedKeysToCheckedItems = true;
        try
        {
            CheckedItems.Clear();
            if (checkedItems != null)
            {
                foreach (var item in checkedItems)
                {
                    CheckedItems.Add(item);
                }
            }
        }
        finally
        {
            _isApplyingSelectedKeysToCheckedItems = false;
        }
        SelectionCountChanged?.Invoke(this, new SelectionCountChangedEventArgs(SelectedKeys?.Count ?? 0));
    }
    
    private void HandleCheckedItemsChanged(object? sender, EventArgs e)
    {
        if (_isApplyingSelectedKeysToCheckedItems)
        {
            return;
        }

        ConfigureSelectedKeys();
    }

    private void ConfigureSelectedKeys()
    {
        _isApplyingCheckedItemsToSelectedKeys = true;
        try
        {
            if (CheckedItems.Count == 0)
            {
                if (!AreKeyCollectionsEquivalent(SelectedKeys, null))
                {
                    SetCurrentValue(SelectedKeysProperty, null);
                }
            }
            else
            {
                var selectedKeys = new List<EntityKey>(CheckedItems.Count);
                foreach (var item in CheckedItems)
                {
                    if (item is ITreeItemNode treeItemNode && treeItemNode.IsEnabled)
                    {
                        var itemKey = treeItemNode.ItemKey ?? default;
                        if (MaskKeys == null || MaskKeys?.Contains(itemKey) == false)
                        {
                            selectedKeys.Add(treeItemNode.ItemKey ?? default);
                        }
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
            _isApplyingCheckedItemsToSelectedKeys = false;
        }
        SelectionCountChanged?.Invoke(this, new SelectionCountChangedEventArgs(SelectedKeys?.Count ?? 0));
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

    private int CalculateAllNodesCount(ITreeItemNode treeNode)
    {
        var count = 0;
        if (MaskKeys == null || MaskKeys?.Contains(treeNode.ItemKey ?? default) == false)
        {
            count += 1;
        }
        foreach (var child in treeNode.Children)
        {
            count += CalculateAllNodesCount(child);
        }
        return count;
    }

    private void MaskNodes()
    {
        foreach (var item in Items)
        {
            var node = (ITreeItemNode)item!;
            MaskNodeRecursively(node);
        }
    }

    private void MaskNodeRecursively(ITreeItemNode item)
    {
        var container = TreeContainerFromItem(item);
        if (container is TransferTreeViewItem treeItem)
        {
            PrepareTransferTreeViewItem(treeItem, item);
        }
        foreach (var child in item.Children)
        {
            MaskNodeRecursively(child);
        }
    }

    private void HandleItemClicked(RoutedEventArgs e)
    {
        if (e.Source is TransferTreeViewItem treeViewItem)
        {
            if (!treeViewItem.IsMasked)
            {
                var item = TreeItemFromContainer(treeViewItem);
                if (treeViewItem.IsChecked != true)
                {
                    CheckedItems.Add(item);
                }
                else
                {
                    CheckedItems.Remove(item);
                }
            }
        }
    }

    private static List<ITreeItemNode> BuildTreeNodeList(IEnumerable source)
    {
        var nodes = source switch
        {
            ICollection collection => new List<ITreeItemNode>(collection.Count),
            IReadOnlyCollection<ITreeItemNode> collection => new List<ITreeItemNode>(collection.Count),
            _ => new List<ITreeItemNode>()
        };
        foreach (var item in source)
        {
            nodes.Add((ITreeItemNode)item!);
        }
        return nodes;
    }

    private static List<ITreeItemNode>? BuildCheckedItemsList(
        IEnumerable? source,
        ICollection<EntityKey>? selectedKeys)
    {
        if (source == null)
        {
            return null;
        }

        if (selectedKeys == null || selectedKeys.Count == 0)
        {
            return new List<ITreeItemNode>(0);
        }

        var selectedKeySet = BuildEntityKeySet(selectedKeys);
        var checkedItems   = new List<ITreeItemNode>(selectedKeys.Count);
        CollectCheckedItems(source, selectedKeySet!, checkedItems);
        return checkedItems;
    }

    private bool IsMaskedItem(object? item)
    {
        return item is ITreeItemNode treeItemNode &&
               MaskKeys?.Contains(treeItemNode.ItemKey ?? default) == true;
    }

    private static void CollectCheckedItems(
        IEnumerable source,
        ISet<EntityKey> selectedKeySet,
        IList<ITreeItemNode> checkedItems)
    {
        foreach (var item in source)
        {
            var treeItem = (ITreeItemNode)item!;
            if (selectedKeySet.Contains(treeItem.ItemKey ?? default))
            {
                checkedItems.Add(treeItem);
            }

            CollectCheckedItems(treeItem.Children, selectedKeySet, checkedItems);
        }
    }

    private static HashSet<EntityKey>? BuildEntityKeySet(ICollection<EntityKey>? keys)
    {
        if (keys == null)
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
}
