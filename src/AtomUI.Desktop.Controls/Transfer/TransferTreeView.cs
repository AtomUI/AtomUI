using System.Collections;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The tree view of one panel of a tree transfer. It shows the nodes of the panel as
/// <see cref="TransferTreeViewItem" /> containers, keeps <see cref="SelectedKeys" /> in step with the checked
/// nodes, and leaves the nodes masked by <see cref="MaskKeys" /> out of checking. The item decorator of the
/// panel drives the view through <see cref="ITransferView" />. The view has neither pagination nor node removal,
/// and the decorator shows no select menu.
/// </summary>
public partial class TransferTreeView : TreeView, ITransferTreeView, ITransferDecoratorProvider
{
    internal override object DragIndicatorLineWidthKey => TransferTreeViewTokenKind.DragIndicatorLineWidth;

    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="SelectedKeys" /> property.
    /// </summary>
    public static readonly DirectProperty<TransferTreeView, IList<EntityKey>?> SelectedKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferTreeView, IList<EntityKey>?>(nameof(SelectedKeys), 
            o => o.SelectedKeys,
            (o, v) => o.SelectedKeys = v,
            defaultBindingMode: BindingMode.TwoWay);
    
    /// <summary>
    /// Identifies the <see cref="MaskKeys" /> property.
    /// </summary>
    public static readonly DirectProperty<TransferTreeView, ISet<EntityKey>?> MaskKeysProperty =
        AvaloniaProperty.RegisterDirect<TransferTreeView, ISet<EntityKey>?>(nameof(MaskKeys), 
            o => o.MaskKeys,
            (o, v) => o.MaskKeys = v);
    
    /// <summary>
    /// Identifies the <see cref="ViewType" /> property.
    /// </summary>
    public static readonly DirectProperty<TransferTreeView, TransferViewType> ViewTypeProperty =
        AvaloniaProperty.RegisterDirect<TransferTreeView, TransferViewType>(nameof(ViewType), 
            o => o.ViewType,
            (o, v) => o.ViewType = v);
    
    private IList<EntityKey>? _selectedKeys;
    /// <summary>
    /// The keys of the nodes checked in this panel. The owning transfer reads and sets the value through
    /// <see cref="ITransferView" />. The value and the checked nodes follow each other: a new value replaces the
    /// checked nodes with the nodes whose keys it contains, and a change of the checked nodes replaces the value with
    /// a new list of the keys of the checked nodes that are enabled and not masked, or with <see langword="null" />
    /// when no node is checked. While the view is attached, a collection that implements
    /// <see cref="INotifyCollectionChanged" /> has its change notifications applied to the checked nodes as well.
    /// </summary>
    public IList<EntityKey>? SelectedKeys
    {
        get => _selectedKeys;
        set => SetAndRaise(SelectedKeysProperty, ref _selectedKeys, value);
    }
    
    private ISet<EntityKey>? _maskKeys;
    /// <summary>
    /// The keys of the nodes that are masked. The owning <c>TreeTransfer</c> passes its target keys through
    /// <c>ITransferTreeView.SetMaskedItems</c>, which replaces the value with a set built from them. A masked node is
    /// drawn as disabled. Clicking it does not change the checked nodes, it is neither checked nor unchecked with
    /// an ancestor's subtree, and it is left out of <see cref="SelectedKeys" /> and of the count reported through
    /// <see cref="ItemCountChanged" />. <see langword="null" /> masks nothing.
    /// </summary>
    public ISet<EntityKey>? MaskKeys
    {
        get => _maskKeys;
        set => SetAndRaise(MaskKeysProperty, ref _maskKeys, value);
    }
    
    private TransferViewType _viewType;
    /// <summary>
    /// The panel the view belongs to. The item decorator of the panel sets it, and the view adds the class of that
    /// panel, <see cref="TreeTransferSemanticParts.SourceItemClass" /> or
    /// <see cref="TreeTransferSemanticParts.TargetItemClass" />, to every node container. The default is
    /// <see cref="TransferViewType.Source" />.
    /// </summary>
    public TransferViewType ViewType
    {
        get => _viewType;
        set => SetAndRaise(ViewTypeProperty, ref _viewType, value);
    }
    
    /// <summary>
    /// Always <see langword="true" />: the view renders an item template, so the item decorator forwards its own
    /// template through <c>ITransferView.SetItemTemplate</c>.
    /// </summary>
    public bool IsSupportItemTemplate => true;
    /// <summary>
    /// Always <see langword="false" />: the item decorator forwards neither a pagination setting nor a page size to
    /// the view, and the methods that would apply them do nothing.
    /// </summary>
    public bool IsSupportPagination => false;
    #endregion
    
    #region 公共事件定义
#pragma warning disable CS0067
    /// <summary>
    /// Never raised: the tree view has no control that removes nodes. It exists only to satisfy
    /// <see cref="ITransferView.ItemsRemoved" />.
    /// </summary>
    public event EventHandler<TransferItemsRemovedEventArgs>? ItemsRemoved;
#pragma warning restore CS0067
    /// <summary>
    /// Raised after <see cref="SelectedKeys" /> changed, before the checked nodes are made to follow it. Also raised
    /// when the current collection raises <c>CollectionChanged</c>.
    /// </summary>
    public event EventHandler? SelectedKeyChanged;
    /// <summary>
    /// Raised when <see cref="MaskKeys" /> or the items source changes, with the number of nodes at every depth that
    /// are not masked.
    /// </summary>
    public event EventHandler<ItemCountChangedEventArgs>? ItemCountChanged;
    /// <summary>
    /// Raised after <see cref="SelectedKeys" /> changed or was rebuilt from the checked nodes, with the
    /// number of keys it holds.
    /// </summary>
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

    /// <summary>
    /// Updates the masks of the node containers and reports the item count when the items source or
    /// <see cref="MaskKeys" /> changes, and rebuilds <see cref="SelectedKeys" /> from the checked nodes when
    /// <see cref="MaskKeys" /> changes. Call the base member first.
    /// </summary>
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

    /// <summary>
    /// Applies <see cref="SelectedKeys" /> to the checked nodes once the view is attached.
    /// Call the base member first; changes of the checked nodes raised while it runs are not written back to
    /// <see cref="SelectedKeys" />.
    /// </summary>
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
    /// Creates the node container, a <see cref="TransferTreeViewItem" /> that carries the tree view item class.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var treeViewItem = new TransferTreeViewItem();
        treeViewItem.Classes.Add(TreeViewSemanticParts.ItemClass);
        return treeViewItem;
    }

    /// <summary>
    /// Tells whether an item needs a container; an item that is already a
    /// <see cref="TransferTreeViewItem" /> does not.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TransferTreeViewItem>(item, out recycleKey);
    }

    /// <summary>
    /// Adds the source or target item class selected by <see cref="ViewType" /> to a
    /// <see cref="TransferTreeViewItem" /> and sets its <see cref="TransferTreeViewItem.IsMasked" /> from
    /// <see cref="MaskKeys" />. The base member is called first.
    /// </summary>
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

    /// <summary>
    /// Decides whether a realized node container takes part in a check that is applied recursively to a subtree:
    /// <see langword="false" /> for a masked <see cref="TransferTreeViewItem" />, <see langword="true" /> for any
    /// other container.
    /// </summary>
    protected override bool RecursiveCheckNodePredicate(TreeViewItem treeViewItem)
    {
        if (treeViewItem is TransferTreeViewItem transferTreeViewItem)
        {
            return !transferTreeViewItem.IsMasked;
        }
        return true;
    }

    /// <summary>
    /// Decides whether a realized node container takes part in an uncheck that is applied recursively to a subtree:
    /// <see langword="false" /> for a masked <see cref="TransferTreeViewItem" />, <see langword="true" /> for any
    /// other container.
    /// </summary>
    protected override bool RecursiveUnCheckNodePredicate(TreeViewItem treeViewItem)
    {
        if (treeViewItem is TransferTreeViewItem transferTreeViewItem)
        {
            return !transferTreeViewItem.IsMasked;
        }
        return true;
    }

    /// <summary>
    /// Decides whether a node whose container is not realized takes part in a check that is applied recursively to a
    /// subtree: <see langword="false" /> for an item whose key is in <c>MaskKeys</c> (see
    /// <see cref="IsMaskedItem" />), <see langword="true" /> for any other item.
    /// </summary>
    private protected override bool RecursiveCheckNodeDataPredicate(object item) => !IsMaskedItem(item);

    /// <summary>
    /// Decides whether a node whose container is not realized takes part in an uncheck that is applied recursively to
    /// a subtree: <see langword="false" /> for an item whose key is in <c>MaskKeys</c> (see
    /// <see cref="IsMaskedItem" />), <see langword="true" /> for any other item.
    /// </summary>
    private protected override bool RecursiveUnCheckNodeDataPredicate(object item) => !IsMaskedItem(item);

    // Goes by the root data rather than the realized root containers, which a virtualizing panel limits.
    /// <summary>
    /// Checks every node at every depth. It walks the root items and their subtrees by data, so nodes whose container
    /// is not realized are checked too. It hides the <c>SelectAll</c> of the base class, which runs instead when
    /// the caller is typed as <see cref="TreeView" />.
    /// </summary>
    public new void SelectAll()
    {
        foreach (var item in Items)
        {
            if (item != null)
            {
                SetSubTreeChecked(item, true);
            }
        }
    }

    /// <summary>
    /// Unchecks every node at every depth by walking the root items and their subtrees by data.
    /// </summary>
    public void DeselectAll()
    {
        foreach (var item in Items)
        {
            if (item != null)
            {
                SetSubTreeChecked(item, false);
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
