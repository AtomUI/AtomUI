using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.MotionScene;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTreeView = Avalonia.Controls.TreeView;

public enum TreeItemHoverMode
{
    Default,
    Block,
    WholeLine
}

[PseudoClasses(StdPseudoClass.Draggable)]
public partial class TreeView : AvaloniaTreeView, IMotionAwareControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsDraggableProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsDraggable));

    public static readonly StyledProperty<bool> IsShowIconProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsShowIcon));

    public static readonly StyledProperty<bool> IsShowLineProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsShowLine));

    public static readonly StyledProperty<TreeItemHoverMode> NodeHoverModeProperty =
        AvaloniaProperty.Register<TreeView, TreeItemHoverMode>(nameof(NodeHoverMode), TreeItemHoverMode.Default);

    public static readonly StyledProperty<bool> IsShowLeafIconProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsShowLeafIcon));
    
    public static readonly StyledProperty<bool> IsSwitcherRotationProperty = 
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsSwitcherRotation), true);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TreeView>();
    
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty = 
        Popup.OpenMotionProperty.AddOwner<TreeView>();
        
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty = 
        Popup.CloseMotionProperty.AddOwner<TreeView>();
    
    public static readonly StyledProperty<ItemToggleType> ToggleTypeProperty =
        AvaloniaProperty.Register<TreeView, ItemToggleType>(nameof(ToggleType), ItemToggleType.None);
    
    public static readonly DirectProperty<TreeView, IList<TreeNodePath>?> DefaultCheckedPathsProperty =
        AvaloniaProperty.RegisterDirect<TreeView, IList<TreeNodePath>?>(
            nameof(DefaultCheckedPaths),
            o => o.DefaultCheckedPaths,
            (o, v) => o.DefaultCheckedPaths = v);
    
    public static readonly DirectProperty<TreeView, IList<TreeNodePath>?> DefaultSelectedPathsProperty =
        AvaloniaProperty.RegisterDirect<TreeView, IList<TreeNodePath>?>(
            nameof(DefaultSelectedPaths),
            o => o.DefaultSelectedPaths,
            (o, v) => o.DefaultSelectedPaths = v);
    
    public static readonly DirectProperty<TreeView, IList<TreeNodePath>?> DefaultExpandedPathsProperty =
        AvaloniaProperty.RegisterDirect<TreeView, IList<TreeNodePath>?>(
            nameof(DefaultExpandedPaths),
            o => o.DefaultExpandedPaths,
            (o, v) => o.DefaultExpandedPaths = v);

    public bool IsDraggable
    {
        get => GetValue(IsDraggableProperty);
        set => SetValue(IsDraggableProperty, value);
    }
    
    public bool IsShowIcon
    {
        get => GetValue(IsShowIconProperty);
        set => SetValue(IsShowIconProperty, value);
    }

    public bool IsShowLine
    {
        get => GetValue(IsShowLineProperty);
        set => SetValue(IsShowLineProperty, value);
    }

    public TreeItemHoverMode NodeHoverMode
    {
        get => GetValue(NodeHoverModeProperty);
        set => SetValue(NodeHoverModeProperty, value);
    }

    public bool IsShowLeafIcon
    {
        get => GetValue(IsShowLeafIconProperty);
        set => SetValue(IsShowLeafIconProperty, value);
    }
    
    public bool IsSwitcherRotation
    {
        get => GetValue(IsSwitcherRotationProperty);
        set => SetValue(IsSwitcherRotationProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }
    
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }
    
    public ItemToggleType ToggleType
    {
        get => GetValue(ToggleTypeProperty);
        set => SetValue(ToggleTypeProperty, value);
    }
    
    private IList<TreeNodePath>? _defaultCheckedPaths;
    
    public IList<TreeNodePath>? DefaultCheckedPaths
    {
        get => _defaultCheckedPaths;
        set => SetAndRaise(DefaultCheckedPathsProperty, ref _defaultCheckedPaths, value);
    }
    
    private IList<TreeNodePath>? _defaultSelectedPaths;
    
    public IList<TreeNodePath>? DefaultSelectedPaths
    {
        get => _defaultSelectedPaths;
        set => SetAndRaise(DefaultSelectedPathsProperty, ref _defaultSelectedPaths, value);
    }
    
    private IList<TreeNodePath>? _defaultExpandedPaths;
    
    public IList<TreeNodePath>? DefaultExpandedPaths
    {
        get => _defaultExpandedPaths;
        set => SetAndRaise(DefaultExpandedPathsProperty, ref _defaultExpandedPaths, value);
    }
    
    public bool IsDefaultExpandAll { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the selected items.
    /// </summary>
    [AllowNull]
    public IList<ITreeViewItemData> CheckedItems
    {
        get
        {
            if (_checkedItems == null)
            {
                _checkedItems = new AvaloniaList<ITreeViewItemData>();
                SubscribeToCheckedItems();
            }

            return _checkedItems;
        }
        set
        {
            if (value?.IsReadOnly == true)
            {
                throw new NotSupportedException(
                    "Cannot use a fixed size or read-only collection as CheckedItems.");
            }

            UnsubscribeFromSelectedItems();
            _checkedItems = value ?? new AvaloniaList<ITreeViewItemData>();
            SubscribeToCheckedItems();
        }
    }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<TreeViewCheckedItemsChangedEventArgs> CheckedItemsChangedEvent =
        RoutedEvent.Register<TreeView, TreeViewCheckedItemsChangedEventArgs>(
            nameof(CheckedItemsChangedEvent),
            RoutingStrategies.Bubble);
    
    public event EventHandler<TreeViewCheckedItemsChangedEventArgs>? CheckedItemsChanged
    {
        add => AddHandler(CheckedItemsChangedEvent, value);
        remove => RemoveHandler(CheckedItemsChangedEvent, value);
    }

    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<TreeView>();
    
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TreeViewToken.ID;
    
    protected internal ITreeViewInteractionHandler InteractionHandler { get; }
    
    #endregion
    
    private readonly Dictionary<TreeViewItem, CompositeDisposable> _itemsBindingDisposables = new();
    private static readonly IList<ITreeViewItemData> Empty = Array.Empty<ITreeViewItemData>();
    private IList<ITreeViewItemData>? _checkedItems;
    private bool _syncingCheckedItems;
    
    internal bool IsExpandAllProcess { get; set; }
    internal bool IsCollapseAllProcess { get; set; }

    public TreeView()
        : this(new DefaultTreeViewInteractionHandler(false))
    {
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
    }
    
    protected TreeView(ITreeViewInteractionHandler interactionHandler)
    {
        InteractionHandler = interactionHandler ?? throw new ArgumentNullException(nameof(interactionHandler));
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
    }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is TreeViewItem treeViewItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(treeViewItem, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(treeViewItem);
                        }
                    }
                }
            }
        }
    }
    
    public void ExpandAll(bool? motionEnabled = null)
    {
        var originMotionEnabled = IsMotionEnabled;
        try
        {
            IsExpandAllProcess = true;
            if (motionEnabled.HasValue)
            {
                SetCurrentValue(IsMotionEnabledProperty, motionEnabled.Value);
            }
            foreach (var item in Items)
            {
                if (item is TreeViewItem treeItem)
                {
                    ExpandSubTree(treeItem);
                }
            }
        }
        finally
        {
            IsExpandAllProcess = false;
            if (motionEnabled.HasValue)
            {
                SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
            }
        }
    }

    public void CollapseAll(bool? motionEnabled = null)
    {
        var originMotionEnabled = IsMotionEnabled;
        try
        {
            IsCollapseAllProcess = true;
            if (motionEnabled.HasValue)
            {
                SetCurrentValue(IsMotionEnabledProperty, motionEnabled.Value);
            }
            foreach (var item in Items)
            {
                if (item is TreeViewItem treeItem)
                {
                    CollapseSubTree(treeItem);
                }
            }
        }
        finally
        {
            IsCollapseAllProcess = false;
            if (motionEnabled.HasValue)
            {
                SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
            }
        }
    }

    public void CheckedSubTree(TreeViewItem item)
    {
        if (!item.IsEffectiveCheckable())
        {
            return;
        }

        var checkedItems = DoCheckedSubTree(item);
        try
        {
            _syncingCheckedItems = true;
            foreach (var checkedItem in checkedItems)
            {
                if (!CheckedItems.Contains(checkedItem))
                {
                    CheckedItems.Add(checkedItem);
                }
            }
        }
        finally
        {
            _syncingCheckedItems = false; 
        }
    }

    private ISet<TreeViewItem> DoCheckedSubTree(TreeViewItem item)
    {
        var checkedItems = new HashSet<TreeViewItem>();
        item.SetCurrentValue(TreeViewItem.IsCheckedProperty, true);
        checkedItems.Add(item);
        if (item.Presenter?.Panel == null && this.GetVisualRoot() is ILayoutRoot visualRoot)
        {
            var layoutManager = visualRoot.GetLayoutManager();
            layoutManager.ExecuteLayoutPass();
        }

        foreach (var childItem in item.Items)
        {
            if (childItem != null)
            {
                var control = TreeContainerFromItem(childItem);
                if (control is TreeViewItem treeViewItem && treeViewItem.IsEffectiveCheckable())
                {
                    var childCheckedItems = DoCheckedSubTree(treeViewItem);
                    checkedItems.UnionWith(childCheckedItems);
                }
            }
        }

        var (checkedParentItems, _) = SetupParentNodeCheckedStatus(item);
        checkedItems.UnionWith(checkedParentItems);
        return checkedItems;
    }
    
    public void UnCheckedSubTree(TreeViewItem item)
    {
        if (!item.IsEffectiveCheckable())
        {
            return;
        }

        var unCheckedItems = DoUnCheckedSubTree(item);
        try
        {
            _syncingCheckedItems = true;
            foreach (var unCheckedItem in unCheckedItems)
            {
                CheckedItems.Remove(unCheckedItem);
            }
        }
        finally
        {
            _syncingCheckedItems = false; 
        }
    }

    public ISet<TreeViewItem> DoUnCheckedSubTree(TreeViewItem item)
    {
        var unCheckedItems = new HashSet<TreeViewItem>();
        if (item.IsChecked == true)
        {
            unCheckedItems.Add(item);
        }
        item.SetCurrentValue(TreeViewItem.IsCheckedProperty, false);
        if (item.Presenter?.Panel == null && this.GetVisualRoot() is ILayoutRoot visualRoot)
        {
            var layoutManager = visualRoot.GetLayoutManager();
            layoutManager.ExecuteLayoutPass();
        }

        foreach (var childItem in item.Items)
        {
            if (childItem != null)
            {
                var control = TreeContainerFromItem(childItem);
                if (control is TreeViewItem treeViewItem && treeViewItem.IsEffectiveCheckable())
                {
                    var childUnCheckedItems = DoUnCheckedSubTree(treeViewItem);
                    unCheckedItems.UnionWith(childUnCheckedItems);
                }
            }
        }
        var (_, unCheckedParentItems) = SetupParentNodeCheckedStatus(item);
        unCheckedItems.UnionWith(unCheckedParentItems);
        return unCheckedItems;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Draggable, IsDraggable);
    }

    protected override Control CreateContainerForItemOverride(
        object? item,
        int index,
        object? recycleKey)
    {
        return new TreeViewItem();
    }

    protected override bool NeedsContainerOverride(
        object? item,
        int index,
        out object? recycleKey)
    {
        return NeedsContainer<TreeViewItem>(item, out recycleKey);
    }

    protected override void ContainerForItemPreparedOverride(
        Control container,
        object? item,
        int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is TreeViewItem treeViewItem)
        {
            treeViewItem.OwnerTreeView = this;
            var disposables = new CompositeDisposable(8);
            
            if (item != null && item is not Visual && item is ITreeViewItemData treeViewItemData)
            {
                TreeViewItem.ApplyNodeData(treeViewItem, treeViewItemData);
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, treeViewItem, TreeViewItem.HeaderTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, treeViewItem, TreeViewItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, NodeHoverModeProperty, treeViewItem, TreeViewItem.NodeHoverModeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowLineProperty, treeViewItem, TreeViewItem.IsShowLineProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowIconProperty, treeViewItem, TreeViewItem.IsShowIconProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowLeafIconProperty, treeViewItem,
                TreeViewItem.IsShowLeafIconProperty));
            disposables.Add(BindUtils.RelayBind(this, IsSwitcherRotationProperty, treeViewItem, TreeViewItem.IsSwitcherRotationProperty));
            disposables.Add(BindUtils.RelayBind(this, ToggleTypeProperty, treeViewItem, TreeViewItem.ToggleTypeProperty));
            
            PrepareTreeViewItem(treeViewItem, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(treeViewItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(treeViewItem);
            }
            _itemsBindingDisposables.Add(treeViewItem, disposables);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TreeViewItem.");
        }
    }
    
    protected virtual void PrepareTreeViewItem(TreeViewItem treeViewItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        InteractionHandler.Attach(this);
        UpdatePseudoClasses();
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        InteractionHandler.Detach(this);
    }

    private TreeViewItem? GetTreeViewItemContainer(ITreeViewItemData childNode, ItemsControl current)
    {
        if (current.Presenter?.Panel == null && this.GetVisualRoot() is ILayoutRoot visualRoot)
        {
            var layoutManager = visualRoot.GetLayoutManager();
            layoutManager.ExecuteLayoutPass();
        }
        return current.ContainerFromItem(childNode) as TreeViewItem;
    }
    
    private void SubscribeToCheckedItems()
    {
        if (_checkedItems is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged += HandleCheckedItemsCollectionChanged;
        }

        HandleCheckedItemsCollectionChanged(
            _checkedItems,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
    
    private void UnsubscribeFromSelectedItems()
    {
        if (_checkedItems is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged -= HandleCheckedItemsCollectionChanged;
        }
    }
    
    private void HandleCheckedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_syncingCheckedItems)
        {
            return;
        }
        IList<ITreeViewItemData>? added   = null;
        IList<ITreeViewItemData>? removed = null;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:

            {
                var newItems = e.NewItems!.Cast<ITreeViewItemData>().ToArray();
                CheckedItemsAdded(newItems);
                added = newItems;
            }
                break;
            case NotifyCollectionChangedAction.Remove:
                
                foreach (var item in e.OldItems!)
                {
                    if (item is ITreeViewItemData treeItemData)
                    {
                        MarkItemChecked(treeItemData, false);
                    }
                }

                removed = e.OldItems!.Cast<ITreeViewItemData>().ToArray();

                break;
            case NotifyCollectionChangedAction.Reset:

                foreach (var container in GetRealizedTreeContainers())
                {
                    MarkContainerChecked(container, false);
                }
                if (CheckedItems.Count > 0)
                {
                    CheckedItemsAdded(CheckedItems);
                    added = CheckedItems;
                }

                break;
            case NotifyCollectionChangedAction.Replace:
            {
                var newItems = e.NewItems!.Cast<ITreeViewItemData>().ToArray();
                var oldItems = e.OldItems!.Cast<ITreeViewItemData>().ToArray();
                foreach (var item in oldItems)
                {
                    MarkItemChecked(item, false);
                }

                foreach (var item in newItems)
                {
                    MarkItemChecked(item, true);
                }

                added   = newItems;
                removed = oldItems;
            }

                break;
        }

        if (added?.Count > 0 || removed?.Count > 0)
        {
            var changed = new TreeViewCheckedItemsChangedEventArgs(
                CheckedItemsChangedEvent,
                removed ?? Empty,
                added ?? Empty);
            RaiseEvent(changed);
        }
    }
    
    private void CheckedItemsAdded(IList<ITreeViewItemData> items)
    {
        if (items.Count == 0)
        {
            return;
        }
        foreach (ITreeViewItemData item in items)
        {
            MarkItemChecked(item, true);
        }
    }
    
    private void MarkItemChecked(ITreeViewItemData item, bool isChecked)
    {
        if (TreeContainerFromItem(item) is Control container)
        {
            MarkContainerChecked(container, isChecked);
        }
        else if (item is TreeViewItem treeViewItem)
        {
            MarkContainerChecked(treeViewItem, isChecked);
        }
    }
    
    private void MarkContainerChecked(Control container, bool isChecked)
    {
        container.SetCurrentValue(TreeViewItem.IsCheckedProperty, isChecked);
    }

    #region 默认展开选中

    private IList<ITreeViewItemData> FindTreeItemByPath(TreeNodePath treeNodePath)
    {
        if (treeNodePath.Length == 0)
        {
            return [];
        }
        var                      segments  = treeNodePath.Segments;
        IList<ITreeViewItemData> items     = Items.OfType<ITreeViewItemData>().ToList();
        IList<ITreeViewItemData> pathNodes = new List<ITreeViewItemData>();
        foreach (var segment in segments)
        {
            bool childFound = false;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.ItemKey != null && item.ItemKey.Value == segment)
                {
                    items      = item.Children;
                    childFound = true;
                    pathNodes.Add(item);
                    break;
                }
            }

            if (!childFound)
            {
                return [];
            }
        }
        return pathNodes;
    }
    
    private List<TreeViewItem> ExpandTreeViewPaths(IList<ITreeViewItemData> pathNodes, bool expandLastRecursively = false)
    {
        if (pathNodes.Count == 0)
        {
            return [];
        }
        List<TreeViewItem> items = new List<TreeViewItem>();
        try
        {
            ItemsControl current = this;
            for (var i = 0; i < pathNodes.Count; i++)
            {
                var pathNode = pathNodes[i];
                var child    = GetTreeViewItemContainer(pathNode, current);
                if (child != null)
                {
                    items.Add(child);
                    current               = child;
                    child.IsMotionEnabled = false;
                    if (i == pathNodes.Count - 1 && expandLastRecursively)
                    {
                        ExpandSubTree(child);
                    }
                    else
                    {
                        child.SetCurrentValue(TreeViewItem.IsExpandedProperty, true);
                    }
                }
            }
            return items;
        }
        finally
        {
            foreach (var item in items)
            {
                item.IsMotionEnabled = true;
            }
        }
    }
    
    private List<TreeViewItem> CollapseTreeViewPaths(IList<ITreeViewItemData> pathNodes, bool collapseLastRecursively = false)
    {
        if (pathNodes.Count == 0)
        {
            return [];
        }
        List<TreeViewItem> items = new List<TreeViewItem>();
        try
        {
            ItemsControl current = this;
            for (var i = 0; i < pathNodes.Count; i++)
            {
                var pathNode = pathNodes[i];
                var child    = GetTreeViewItemContainer(pathNode, current);
                if (child != null)
                {
                    items.Add(child);
                    current               = child;
                    child.IsMotionEnabled = false;
                    if (i == pathNodes.Count - 1 && collapseLastRecursively)
                    {
                        CollapseSubTree(child);
                    }
                    else
                    {
                        child.SetCurrentValue(TreeViewItem.IsExpandedProperty, false);
                    }
                }
            }
            return items;
        }
        finally
        {
            foreach (var item in items)
            {
                item.IsMotionEnabled = true;
            }
        }
    }
    
    private (ISet<TreeViewItem>, ISet<TreeViewItem>) SetupParentNodeCheckedStatus(TreeViewItem item)
    {
        var parent         = item.Parent;
        var checkedParents =  new HashSet<TreeViewItem>();
        var unCheckedParents =  new HashSet<TreeViewItem>();
        while (parent is TreeViewItem parentTreeItem && parentTreeItem.IsEnabled)
        {
            var isAllChecked = false;
            var isAnyChecked = false;

            if (parentTreeItem.Items.Count > 0)
            {
                isAllChecked = parentTreeItem.Items.All(childItem =>
                {
                    if (childItem != null)
                    {
                        var container = TreeContainerFromItem(childItem);
                        if (container is TreeViewItem treeViewItem)
                        {
                            return !treeViewItem.IsEffectiveCheckable() || treeViewItem.IsChecked.HasValue && treeViewItem.IsChecked.Value;
                        }
                    }
                    
                    return false;
                });

                isAnyChecked = parentTreeItem.Items.Any(childItem =>
                {
                    if (childItem != null)
                    {
                        var container = TreeContainerFromItem(childItem);
                        if (container is TreeViewItem treeViewItem)
                        {
                            return treeViewItem.IsEffectiveCheckable() && (!treeViewItem.IsChecked.HasValue || treeViewItem.IsChecked.HasValue && treeViewItem.IsChecked.Value);
                        }
                    }
                    return false;
                });
            }

            if (parentTreeItem.IsChecked == true && !isAllChecked)
            {
                unCheckedParents.Add(parentTreeItem);
            }
            
            var originMotionEnabled = parentTreeItem.IsMotionEnabled;
            try
            {
                parentTreeItem.SetCurrentValue(TreeViewItem.IsMotionEnabledProperty, false);
                if (isAllChecked)
                {
                    parentTreeItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, true);
                }
                else if (isAnyChecked)
                {
                    parentTreeItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, null);
                }
                else
                {
                    parentTreeItem.SetCurrentValue(TreeViewItem.IsCheckedProperty, false);
                }
            }
            finally
            {
                parentTreeItem.SetCurrentValue(TreeViewItem.IsMotionEnabledProperty, originMotionEnabled);
            }
       
            if (parentTreeItem.IsChecked == true)
            {
                checkedParents.Add(parentTreeItem);
            }
            parent = parent.Parent;
        }

        return (checkedParents, unCheckedParents);
    }

    private void ConfigureDefaultCheckedPaths()
    {
        if (DefaultCheckedPaths != null)
        {
            foreach (var defaultCheckedPath in DefaultCheckedPaths)
            {
                var pathNodes = FindTreeItemByPath(defaultCheckedPath);
                if (pathNodes.Count > 0)
                {
                    try
                    {
                        var items = ExpandTreeViewPaths(pathNodes, true);
                        if (items.Count > 0)
                        {
                            var target              = items.Last();
                            var originMotionEnabled = target.IsMotionEnabled;
                            try
                            {
                                target.SetCurrentValue(TreeViewItem.IsMotionEnabledProperty, false);
                                target.SetCurrentValue(TreeViewItem.IsCheckedProperty, true);
                            }
                            finally
                            {
                                target.SetCurrentValue(TreeViewItem.IsMotionEnabledProperty, originMotionEnabled);
                            }
                        }
                    }
                    finally
                    {
                        CollapseTreeViewPaths(pathNodes, false);
                    }
                }
            }
        }
    }
        
    private void ConfigureDefaultExpandedPaths()
    {
        if (DefaultExpandedPaths != null)
        {
            foreach (var defaultExpandedPath in DefaultExpandedPaths)
            {
                var pathNodes = FindTreeItemByPath(defaultExpandedPath);
                ExpandTreeViewPaths(pathNodes);
            }
        }
    }
    
    private void ConfigureDefaultSelectedPaths()
    {
        if (DefaultSelectedPaths != null)
        {
            foreach (var defaultSelectedPath in DefaultSelectedPaths)
            {
                var pathNodes = FindTreeItemByPath(defaultSelectedPath);
              
                if (pathNodes.Count > 0)
                {
                    var targetNode = pathNodes.Last();
                    if (!SelectedItems.Contains(targetNode))
                    {
                        SelectedItems.Add(targetNode);
                    }
                }
            }
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ConfigureDefaultSelectedPaths();
        ConfigureDefaultCheckedPaths();
        
        if (IsDefaultExpandAll)
        {
            ExpandAll(true);
        }
        else
        {
            ConfigureDefaultExpandedPaths();
        }
    }

    #endregion
}