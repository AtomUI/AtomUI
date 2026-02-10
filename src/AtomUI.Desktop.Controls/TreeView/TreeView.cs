using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
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
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTreeView = Avalonia.Controls.TreeView;

public enum TreeItemHoverMode
{
    Default,
    Block,
    WholeLine
}

[Flags]
public enum TreeFilterHighlightStrategy
{
    HighlightedMatch = 0x01,
    HighlightedWhole = 0x02,
    BoldedMatch = 0x04,
    ExpandPath = 0x08,
    HideUnMatched = 0x10,
    All = HighlightedMatch | BoldedMatch | ExpandPath | HideUnMatched
}

[PseudoClasses(StdPseudoClass.Draggable)]
public partial class TreeView : AvaloniaTreeView, IMotionAwareControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsAutoExpandParentProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsAutoExpandParent), true);
    
    public static readonly StyledProperty<bool> IsDraggableProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsDraggable));

    public static readonly StyledProperty<bool> IsShowIconProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsShowIcon));

    public static readonly StyledProperty<bool> IsShowLineProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsShowLine));
    
    public static readonly StyledProperty<bool> IsDefaultExpandAllProperty =
        AvaloniaProperty.Register<TreeView, bool>(
            nameof(IsDefaultExpandAll));

    public static readonly StyledProperty<TreeItemHoverMode> NodeHoverModeProperty =
        AvaloniaProperty.Register<TreeView, TreeItemHoverMode>(nameof(NodeHoverMode), TreeItemHoverMode.Default);
    
    public static readonly StyledProperty<IconTemplate?> SwitcherExpandIconProperty =
        AvaloniaProperty.Register<TreeView, IconTemplate?>(nameof(SwitcherExpandIcon));

    public static readonly StyledProperty<IconTemplate?> SwitcherCollapseIconProperty =
        AvaloniaProperty.Register<TreeView, IconTemplate?>(nameof(SwitcherCollapseIcon));

    public static readonly StyledProperty<IconTemplate?> SwitcherRotationIconProperty =
        AvaloniaProperty.Register<TreeView, IconTemplate?>(nameof(SwitcherRotationIcon));

    public static readonly StyledProperty<IconTemplate?> SwitcherLoadingIconProperty =
        AvaloniaProperty.Register<TreeView, IconTemplate?>(nameof(SwitcherLoadingIcon));

    public static readonly StyledProperty<IconTemplate?> SwitcherLeafIconProperty =
        AvaloniaProperty.Register<TreeView, IconTemplate?>(nameof(SwitcherLeafIcon));

    public static readonly StyledProperty<bool> IsShowLeafIconProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsShowLeafIcon));
    
    public static readonly StyledProperty<bool> IsSwitcherRotationProperty = 
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsSwitcherRotation), true);
    
    public static readonly StyledProperty<bool> IsSelectableProperty = 
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsSelectable), true);
    
    public static readonly StyledProperty<bool> IsCheckStrictlyProperty = 
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsCheckStrictly), false);

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
    
    public static readonly StyledProperty<ITreeItemDataLoader?> DataLoaderProperty =
        AvaloniaProperty.Register<TreeView, ITreeItemDataLoader?>(nameof(DataLoader));
    
    public static readonly StyledProperty<ITreeItemFilter?> FilterProperty =
        AvaloniaProperty.Register<TreeView, ITreeItemFilter?>(nameof(Filter), new DefaultTreeItemFilter());
    
    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<TreeView, object?>(nameof(FilterValue));
    
    
    public static readonly StyledProperty<TreeFilterHighlightStrategy> FilterHighlightStrategyProperty =
        AvaloniaProperty.Register<TreeView, TreeFilterHighlightStrategy>(nameof(FilterHighlightStrategy), TreeFilterHighlightStrategy.All);
    
    public static readonly DirectProperty<TreeView, int> FilterResultCountProperty =
        AvaloniaProperty.RegisterDirect<TreeView, int>(nameof(FilterResultCount),
            o => o.FilterResultCount);
    
    public static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        AvaloniaProperty.Register<TreeView, IBrush?>(nameof(FilterHighlightForeground));
    
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<TreeView, object?>(nameof(EmptyIndicator));
    
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<TreeView, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsShowEmptyIndicator), true);
    
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<TreeView, Thickness>(nameof(EmptyIndicatorPadding));
    
    public bool IsAutoExpandParent
    {
        get => GetValue(IsAutoExpandParentProperty);
        set => SetValue(IsAutoExpandParentProperty, value);
    }

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
    
    public bool IsDefaultExpandAll
    {
        get => GetValue(IsDefaultExpandAllProperty);
        set => SetValue(IsDefaultExpandAllProperty, value);
    }

    public TreeItemHoverMode NodeHoverMode
    {
        get => GetValue(NodeHoverModeProperty);
        set => SetValue(NodeHoverModeProperty, value);
    }
    
    public IconTemplate? SwitcherExpandIcon
    {
        get => GetValue(SwitcherExpandIconProperty);
        set => SetValue(SwitcherExpandIconProperty, value);
    }

    public IconTemplate? SwitcherCollapseIcon
    {
        get => GetValue(SwitcherCollapseIconProperty);
        set => SetValue(SwitcherCollapseIconProperty, value);
    }

    public IconTemplate? SwitcherRotationIcon
    {
        get => GetValue(SwitcherRotationIconProperty);
        set => SetValue(SwitcherRotationIconProperty, value);
    }

    public IconTemplate? SwitcherLoadingIcon
    {
        get => GetValue(SwitcherLoadingIconProperty);
        set => SetValue(SwitcherLoadingIconProperty, value);
    }

    public IconTemplate? SwitcherLeafIcon
    {
        get => GetValue(SwitcherLeafIconProperty);
        set => SetValue(SwitcherLeafIconProperty, value);
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
    
    public bool IsSelectable
    {
        get => GetValue(IsSelectableProperty);
        set => SetValue(IsSelectableProperty, value);
    }
    
    public bool IsCheckStrictly
    {
        get => GetValue(IsCheckStrictlyProperty);
        set => SetValue(IsCheckStrictlyProperty, value);
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
    
    public ITreeItemDataLoader? DataLoader
    {
        get => GetValue(DataLoaderProperty);
        set => SetValue(DataLoaderProperty, value);
    }
    
    public ITreeItemFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    
    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }

    public TreeFilterHighlightStrategy FilterHighlightStrategy
    {
        get => GetValue(FilterHighlightStrategyProperty);
        set => SetValue(FilterHighlightStrategyProperty, value);
    }

    private int _filterResultCount;
    
    public int FilterResultCount
    {
        get => _filterResultCount;
        private set => SetAndRaise(FilterResultCountProperty, ref _filterResultCount, value);
    }
    
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }
    
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }
    
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }
    
    public IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the selected items.
    /// </summary>
    [AllowNull]
    public IList CheckedItems
    {
        get
        {
            if (_checkedItems == null)
            {
                _checkedItems = new AvaloniaList<object>();
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

            UnsubscribeFromCheckedItems();
            _checkedItems = value ?? new AvaloniaList<object>();
            SubscribeToCheckedItems();
        }
    }

    #endregion

    #region 公共事件定义
    
    public event EventHandler<TreeViewCheckedItemsChangedEventArgs>? CheckedItemsChanged;
    public event EventHandler<TreeViewItemLoadedEventArgs>? TreeItemLoaded;
    public event EventHandler<TreeViewDragStartedEventArgs>? ItemDragStarted;
    public event EventHandler<TreeViewDragCompletedEventArgs>? ItemDragCompleted;
    public event EventHandler<TreeViewDragEnterEventArgs>? ItemDragEnter;
    public event EventHandler<TreeViewDragLeaveEventArgs>? ItemDragLeave;
    public event EventHandler<TreeViewDragOverEventArgs>? ItemDragOver;
    public event EventHandler<TreeViewDroppedEventArgs>? ItemDropped;
    public event EventHandler<TreeItemExpandedEventArgs>? ItemExpanded;
    public event EventHandler<TreeItemCollapsedEventArgs>? ItemCollapsed;
    public event EventHandler<TreeItemClickedEventArgs>? ItemClicked;
    public event EventHandler<TreeItemContextMenuEventArgs>? ItemContextMenuRequest;
    
    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<TreeView>();
    
    internal static readonly DirectProperty<TreeView, bool> IsEffectiveEmptyVisibleProperty =
        AvaloniaProperty.RegisterDirect<TreeView, bool>(
            nameof(IsEffectiveEmptyVisible),
            o => o.IsEffectiveEmptyVisible,
            (o, v) => o.IsEffectiveEmptyVisible = v);
    
    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }
    
    private bool _isEffectiveEmptyVisible = false;
    internal bool IsEffectiveEmptyVisible
    {
        get => _isEffectiveEmptyVisible;
        set => SetAndRaise(IsEffectiveEmptyVisibleProperty, ref _isEffectiveEmptyVisible, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TreeViewToken.ID;
    
    protected internal ITreeViewInteractionHandler InteractionHandler { get; }
    
    #endregion
    
    private readonly Dictionary<TreeItem, CompositeDisposable> _itemsBindingDisposables = new();
    private static readonly IList Empty = Array.Empty<object>();
    private IList? _checkedItems;
    private bool _syncingCheckedItems;
    
    internal bool IsExpandAllProcess { get; set; }
    internal bool IsCollapseAllProcess { get; set; }

    static TreeView()
    {
        ConfigureDragAndDrop();
        TreeItem.ExpandedEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemExpanded(args));
        TreeItem.CollapsedEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemCollapsed(args));
        TreeItem.ContextMenuRequestEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemContextMenuRequest(args));
        TreeItem.ClickEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemClicked(args));
        ConfigureFilter();
    }

    public TreeView()
        : this(new DefaultTreeViewInteractionHandler(false))
    {
    }
    
    protected TreeView(ITreeViewInteractionHandler interactionHandler)
    {
        InteractionHandler = interactionHandler ?? throw new ArgumentNullException(nameof(interactionHandler));
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleLogicalChildrenCollectionChanged;
        Items.CollectionChanged           += HandleCollectionChanged;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Filter ??= new DefaultTreeItemFilter();
        ConfigureEmptyIndicator();
    }

    private void HandleLogicalChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is TreeItem treeViewItem)
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
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigureEmptyIndicator();
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                foreach (var i in e.OldItems!)
                    CheckedItems.Remove(i);
                break;
            case NotifyCollectionChangedAction.Reset:
                CheckedItems.Clear();
                break;
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

            for (var i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is TreeItem treeItem)
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

            for (var i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is TreeItem treeItem)
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

    public void CheckedSubTree(TreeItem item)
    {
        if (!item.IsEffectiveCheckable())
        {
            return;
        }
        
        var originIsMotionEnabled = IsMotionEnabled;
        try
        {
            SetCurrentValue(IsMotionEnabledProperty, false);
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
        finally
        {
            SetCurrentValue(IsMotionEnabledProperty, originIsMotionEnabled);
        }
    }

    private ISet<object> DoCheckedSubTree(TreeItem treeItem)
    {
        var checkedItems = new HashSet<object>();
        treeItem.SetCurrentValue(TreeItem.IsCheckedProperty, true);
        var treeItemData = TreeItemFromContainer(treeItem);
        Debug.Assert(treeItemData != null);
        checkedItems.Add(treeItemData);
        var originIsExpanded = treeItem.IsExpanded;
        try
        {
            if (treeItem.Presenter?.Panel == null && !originIsExpanded && this.GetVisualRoot() is ILayoutRoot visualRoot)
            {
                treeItem.SetCurrentValue(TreeItem.IsExpandedProperty, true);
                var layoutManager = visualRoot.GetLayoutManager();
                layoutManager.ExecuteLayoutPass();
            }

            foreach (var childItem in treeItem.Items)
            {
                if (childItem != null)
                {
                    var container = TreeContainerFromItem(childItem);
                    if (container is TreeItem treeViewItem && treeViewItem.IsEffectiveCheckable())
                    {
                        var childCheckedItems = DoCheckedSubTree(treeViewItem);
                        checkedItems.UnionWith(childCheckedItems);
                    }
                }
            }

            var (checkedParentItems, _) = SetupParentNodeCheckedStatus(treeItem);
            checkedItems.UnionWith(checkedParentItems);
        }
        finally
        {
            treeItem.SetCurrentValue(TreeItem.IsExpandedProperty, originIsExpanded);
        }
        
        return checkedItems;
    }
    
    public void UnCheckedSubTree(TreeItem item)
    {
        if (!item.IsEffectiveCheckable())
        {
            return;
        }

        var originIsMotionEnabled = IsMotionEnabled;
        try
        {
            SetCurrentValue(IsMotionEnabledProperty, false);
            var unCheckedItems = DoUnCheckedSubTree(item);
            try
            {
                _syncingCheckedItems = true;
                foreach (var unCheckedItem in unCheckedItems)
                {
                    CheckedItems.Remove(unCheckedItem);
                }

                var treeItemData = TreeItemFromContainer(item);
                Debug.Assert(treeItemData != null);
                CheckedItems.Remove(treeItemData);
            }
            finally
            {
                _syncingCheckedItems = false;
            }
        }
        finally
        {
            SetCurrentValue(IsMotionEnabledProperty, originIsMotionEnabled);
        }
    }

    public ISet<object> DoUnCheckedSubTree(TreeItem treeItem)
    {
        var unCheckedItems = new HashSet<object>();
        if (treeItem.IsChecked == true)
        {
            var treeItemData = TreeItemFromContainer(treeItem);
            Debug.Assert(treeItemData != null);
            unCheckedItems.Add(treeItemData);
        }
        treeItem.SetCurrentValue(TreeItem.IsCheckedProperty, false);
        var originIsExpanded = treeItem.IsExpanded;

        try
        {
            if (treeItem.Presenter?.Panel == null && !originIsExpanded && this.GetVisualRoot() is ILayoutRoot visualRoot)
            {
                treeItem.SetCurrentValue(TreeItem.IsExpandedProperty, true);
                var layoutManager = visualRoot.GetLayoutManager();
                layoutManager.ExecuteLayoutPass();
            }

            foreach (var childItem in treeItem.Items)
            {
                if (childItem != null)
                {
                    var control = TreeContainerFromItem(childItem);
                    if (control is TreeItem treeViewItem && treeViewItem.IsEffectiveCheckable())
                    {
                        var childUnCheckedItems = DoUnCheckedSubTree(treeViewItem);
                        unCheckedItems.UnionWith(childUnCheckedItems);
                    }
                }
            }
            var (_, unCheckedParentItems) = SetupParentNodeCheckedStatus(treeItem);
            unCheckedItems.UnionWith(unCheckedParentItems);
            return unCheckedItems;
        }
        finally
        {
            treeItem.SetCurrentValue(TreeItem.IsExpandedProperty, originIsExpanded);
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Draggable, IsDraggable);
    }

    protected override Control CreateContainerForItemOverride(object? item,
                                                              int index,
                                                              object? recycleKey)
    {
        return new TreeItem();
    }

    protected override bool NeedsContainerOverride(object? item,
                                                   int index,
                                                   out object? recycleKey)
    {
        return NeedsContainer<TreeItem>(item, out recycleKey);
    }

    protected override void ContainerForItemPreparedOverride(Control container,
                                                             object? item,
                                                             int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is TreeItem treeViewItem)
        {
            treeViewItem.OwnerTreeView = this;
            var disposables = new CompositeDisposable(8);
            
            if (item != null && item is not Visual && item is ITreeItemData treeViewItemData)
            {
                TreeItem.ApplyNodeData(treeViewItem, treeViewItemData, disposables);
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, treeViewItem, TreeItem.HeaderTemplateProperty));
            }
            
            SetTreeViewItemIcon(treeViewItem, TreeItem.SwitcherExpandIconProperty, SwitcherExpandIcon);
            SetTreeViewItemIcon(treeViewItem, TreeItem.SwitcherCollapseIconProperty, SwitcherCollapseIcon);
            SetTreeViewItemIcon(treeViewItem, TreeItem.SwitcherRotationIconProperty, SwitcherRotationIcon);
            SetTreeViewItemIcon(treeViewItem, TreeItem.SwitcherLoadingIconProperty, SwitcherLoadingIcon);
            SetTreeViewItemIcon(treeViewItem, TreeItem.SwitcherLeafIconProperty, SwitcherLeafIcon);
            
            disposables.Add(BindUtils.RelayBind(this, FilterHighlightStrategyProperty, treeViewItem, TreeItem.FilterHighlightStrategyProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, treeViewItem, TreeItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, NodeHoverModeProperty, treeViewItem, TreeItem.NodeHoverModeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowLineProperty, treeViewItem, TreeItem.IsShowLineProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowIconProperty, treeViewItem, TreeItem.IsShowIconProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowLeafIconProperty, treeViewItem,
                TreeItem.IsShowLeafIconProperty));
            disposables.Add(BindUtils.RelayBind(this, IsSwitcherRotationProperty, treeViewItem, TreeItem.IsSwitcherRotationProperty));
            disposables.Add(BindUtils.RelayBind(this, ToggleTypeProperty, treeViewItem, TreeItem.ToggleTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsSelectableProperty, treeViewItem, TreeItem.IsSelectableProperty));
            disposables.Add(BindUtils.RelayBind(this, FilterHighlightForegroundProperty, treeViewItem, TreeItem.FilterHighlightForegroundProperty));
            disposables.Add(BindUtils.RelayBind(this, HasTreeItemDataLoaderProperty, treeViewItem,
                TreeItem.HasTreeItemDataLoaderProperty));
            disposables.Add(BindUtils.RelayBind(this, IsAutoExpandParentProperty, treeViewItem,
                TreeItem.IsAutoExpandParentProperty));
            
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
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TreeItem.");
        }
    }

    private void SetTreeViewItemIcon(TreeItem treeItem, AvaloniaProperty iconProperty, IIconTemplate? iconTemplate)
    {
        if (iconTemplate == null)
        {
            treeItem.SetValue(iconProperty, null);
        }
        else
        {
            treeItem.SetValue(iconProperty, iconTemplate.Build());
        }
    }
    
    protected virtual void PrepareTreeViewItem(TreeItem treeItem, object? item, int index, CompositeDisposable disposables)
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

    private TreeItem? GetTreeViewItemContainer(object childNode, ItemsControl current)
    {
        if (current.Presenter?.Panel == null && this.GetVisualRoot() is ILayoutRoot visualRoot)
        {
            var layoutManager = visualRoot.GetLayoutManager();
            layoutManager.ExecuteLayoutPass();
        }
        if (current.Presenter?.Panel is { } panel)
        {
            return current.ContainerFromItem(childNode) as TreeItem;
        }
        return null;
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
    
    private void UnsubscribeFromCheckedItems()
    {
        if (_checkedItems is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged -= HandleCheckedItemsCollectionChanged;
        }
    }
    
    private void HandleCheckedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        IList? added   = null;
        IList? removed = null;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:

            {
                if (e.NewItems != null)
                {
                    if (!_syncingCheckedItems)
                    {
                        CheckedItemsAdded(e.NewItems); 
                    }
                    added = e.NewItems;
                }
               
                break;
            }
            case NotifyCollectionChangedAction.Remove:
                if (!_syncingCheckedItems)
                {
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            MarkItemChecked(item, false);
                        }
                    }
                }

                removed = e.OldItems;

                break;
            case NotifyCollectionChangedAction.Reset:
                if (!_syncingCheckedItems)
                {
                    foreach (var container in GetRealizedTreeContainers())
                    {
                        MarkContainerChecked(container, false);
                    }
                    if (CheckedItems.Count > 0)
                    {
                        CheckedItemsAdded(CheckedItems);
                    }
                }

                if (CheckedItems.Count > 0)
                {
                    added = CheckedItems;
                }

                break;
            case NotifyCollectionChangedAction.Replace:
            {
                if (!_syncingCheckedItems)
                {
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            MarkItemChecked(item, false);
                        }
                    }

                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            MarkItemChecked(item, true);
                        }
                    }
                }
                
                added   = e.NewItems;
                removed = e.OldItems;
                break;
            }
        }
        if (added?.Count > 0 || removed?.Count > 0)
        {
            CheckedItemsChanged?.Invoke(this, new TreeViewCheckedItemsChangedEventArgs(
                removed ?? Empty,
                added ?? Empty));
        }
    }
    
    private void CheckedItemsAdded(IList items)
    {
        if (items.Count == 0)
        {
            return;
        }
        foreach (var item in items)
        {
            MarkItemChecked(item, true);
        }
    }
    
    private void MarkItemChecked(object item, bool isChecked)
    {
        var container = TreeContainerFromItem(item);
        if (container != null)
        {
            MarkContainerChecked(container, isChecked);
        }
    }
    
    private void MarkContainerChecked(Control container, bool isChecked)
    {
        container.SetCurrentValue(TreeItem.IsCheckedProperty, isChecked);
    }

    #region 默认展开选中

    private List<TreeItem>? ExpandTreeViewPath(TreeNodePath treeNodePath)
    {
        if (treeNodePath.Length == 0)
        {
            return null;
        }

        var originIsMotionEnabled = IsMotionEnabled;
        try
        {
            SetCurrentValue(IsMotionEnabledProperty, false);
            var   segments  = treeNodePath.Segments;
            IList items     = Items.ToList();
            var   pathNodes = new List<TreeItem>();
            foreach (var segment in segments)
            {
                bool childFound = false;
                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item != null)
                    {
                        var treeViewItem = TreeContainerFromItem(item) as TreeItem;
                        if (treeViewItem == null)
                        {
                            return null;
                        }
                        if (treeViewItem.ItemKey != null && treeViewItem.ItemKey.Value == segment)
                        {
                            treeViewItem.SetCurrentValue(TreeItem.IsExpandedProperty, true);
                            if (treeViewItem.Presenter?.Panel == null &&
                                this.GetVisualRoot() is ILayoutRoot visualRoot)
                            {
                                var layoutManager = visualRoot.GetLayoutManager();
                                layoutManager.ExecuteLayoutPass();
                            }
                            items      = treeViewItem.Items;
                            childFound = true;
                            pathNodes.Add(treeViewItem);
                            break;
                        }
                    }
                }

                if (!childFound)
                {
                    return null;
                }
            }

            return pathNodes;
        }
        finally
        {
            SetCurrentValue(IsMotionEnabledProperty, originIsMotionEnabled);
        }
    }

    private List<TreeItem>? CollapseTreeViewPath(TreeNodePath treeNodePath)
    {
        if (treeNodePath.Length == 0)
        {
            return null;
        }
        var originIsMotionEnabled = IsMotionEnabled;
        try
        {
            var   segments  = treeNodePath.Segments;
            IList items     = Items.ToList();
            var   pathNodes = new List<TreeItem>();
            foreach (var segment in segments)
            {
                bool childFound = false;
                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item != null)
                    {
                        var treeViewItem = TreeContainerFromItem(item) as TreeItem;
                        if (treeViewItem == null)
                        {
                            return null;
                        }
                        if (treeViewItem.ItemKey != null && treeViewItem.ItemKey.Value == segment)
                        {
                            treeViewItem.SetCurrentValue(TreeItem.IsExpandedProperty, true);
                            if (treeViewItem.Presenter?.Panel == null &&
                                this.GetVisualRoot() is ILayoutRoot visualRoot)
                            {
                                var layoutManager = visualRoot.GetLayoutManager();
                                layoutManager.ExecuteLayoutPass();
                            }
                            items      = treeViewItem.Items;
                            childFound = true;
                            pathNodes.Add(treeViewItem);
                            break;
                        }
                    }
                }

                if (!childFound)
                {
                    return null;
                }
            }

            foreach (var treeViewItem in pathNodes)
            {
                treeViewItem.SetCurrentValue(TreeItem.IsExpandedProperty, false);
            }
            return pathNodes;
        }
        finally
        {
            SetCurrentValue(IsMotionEnabledProperty, originIsMotionEnabled);
        }
    }
    
    private List<TreeItem>? TraverTreeViewPath(TreeNodePath treeNodePath, Action<TreeItem>? action)
    {
        if (treeNodePath.Length == 0)
        {
            return null;
        }

        var originIsMotionEnabled = IsMotionEnabled;
        try
        {
            SetCurrentValue(IsMotionEnabledProperty, false);
            var   segments             = treeNodePath.Segments;
            IList items                = Items.ToList();
            var   pathNodes            = new List<TreeItem>();
            var   pathNodeExpandStatus = new List<bool>();
            try
            {
                foreach (var segment in segments)
                {
                    bool childFound = false;
                    for (var i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        if (item != null)
                        {
                            var treeViewItem = TreeContainerFromItem(item) as TreeItem;
                            if (treeViewItem == null)
                            {
                                return null;
                            }

                            if (treeViewItem.ItemKey != null && treeViewItem.ItemKey.Value == segment)
                            {
                                pathNodeExpandStatus.Add(treeViewItem.IsExpanded);
                                treeViewItem.SetCurrentValue(TreeItem.IsExpandedProperty, true);
                                if (treeViewItem.Presenter?.Panel == null &&
                                    this.GetVisualRoot() is ILayoutRoot visualRoot)
                                {
                                    var layoutManager = visualRoot.GetLayoutManager();
                                    layoutManager.ExecuteLayoutPass();
                                }

                                items      = treeViewItem.Items;
                                childFound = true;
                                pathNodes.Add(treeViewItem);
                                action?.Invoke(treeViewItem);
                                break;
                            }
                        }
                    }

                    if (!childFound)
                    {
                        return null;
                    }
                }

                return pathNodes;
            }
            finally
            {
                for (var i = pathNodes.Count - 1; i >= 0; --i)
                {
                    var treeViewItem = pathNodes[i];
                    var expandStatus = pathNodeExpandStatus[i];
                    treeViewItem.SetCurrentValue(TreeItem.IsExpandedProperty, expandStatus);
                }
            }
        }
        finally
        {
            SetCurrentValue(IsMotionEnabledProperty, originIsMotionEnabled);
        }
    }
    
    private (ISet<object>, ISet<object>) SetupParentNodeCheckedStatus(TreeItem item)
    {
        var parent           = item.Parent;
        var checkedParents   =  new HashSet<object>();
        var unCheckedParents =  new HashSet<object>();
        while (parent is TreeItem parentTreeItem && parentTreeItem.IsEnabled)
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
                        if (container is TreeItem treeViewItem)
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
                        if (container is TreeItem treeViewItem)
                        {
                            return treeViewItem.IsEffectiveCheckable() && (!treeViewItem.IsChecked.HasValue || treeViewItem.IsChecked.HasValue && treeViewItem.IsChecked.Value);
                        }
                    }
                    return false;
                });
            }

            if (parentTreeItem.IsChecked == true && !isAllChecked)
            {
                var parentTreeItemData = TreeItemFromContainer(parentTreeItem);
                Debug.Assert(parentTreeItemData != null);
                unCheckedParents.Add(parentTreeItemData);
            }
            
            var originMotionEnabled = parentTreeItem.IsMotionEnabled;
            try
            {
                parentTreeItem.SetCurrentValue(TreeItem.IsMotionEnabledProperty, false);
                if (isAllChecked)
                {
                    parentTreeItem.SetCurrentValue(TreeItem.IsCheckedProperty, true);
                }
                else if (isAnyChecked)
                {
                    parentTreeItem.SetCurrentValue(TreeItem.IsCheckedProperty, null);
                }
                else
                {
                    parentTreeItem.SetCurrentValue(TreeItem.IsCheckedProperty, false);
                }
            }
            finally
            {
                parentTreeItem.SetCurrentValue(TreeItem.IsMotionEnabledProperty, originMotionEnabled);
            }
       
            if (parentTreeItem.IsChecked == true)
            {
                var parentTreeItemData = TreeItemFromContainer(parentTreeItem);
                Debug.Assert(parentTreeItemData != null);
                checkedParents.Add(parentTreeItemData);
            }
            parent = parent.Parent;
        }

        return (checkedParents, unCheckedParents);
    }

    private void ConfigureDefaultCheckedPaths()
    {
        if (DefaultCheckedPaths != null)
        {
            foreach (var checkedPath in DefaultCheckedPaths)
            {
                TraverTreeViewPath(checkedPath, treeViewItem =>
                {
                    treeViewItem.SetCurrentValue(TreeItem.IsCheckedProperty, true);
                });
            }
        }
    }
        
    private void ConfigureDefaultExpandedPaths()
    {
        if (DefaultExpandedPaths != null)
        {
            foreach (var path in DefaultExpandedPaths)
            {
                ExpandTreeViewPath(path);
            }
        }
    }
    
    private void ConfigureDefaultSelectedPaths()
    {
        if (IsSelectable)
        {
            if (SelectedItems.Count == 0 && SelectedItem == null)
            {
                if (DefaultSelectedPaths != null)
                {
                    foreach (var selectedPath in DefaultSelectedPaths)
                    {
                        TraverTreeViewPath(selectedPath, treeViewItem =>
                        {
                            treeViewItem.SetCurrentValue(TreeItem.IsSelectedProperty, true);
                        });
                    }
                }
            }
            else
            {
                if (SelectedItem != null)
                {
                    var paths = GetTreePathFromItem(SelectedItem);
                    SelectTreeItemByPath(paths);
                }
            }
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureDefaultSelectedPaths();
        ConfigureDefaultCheckedPaths();
        
        FilterTreeNode();
        
        if (IsDefaultExpandAll)
        {
            ExpandAll(false);
        }
        else
        {
            ConfigureDefaultExpandedPaths();
        }
    }

    #endregion
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DefaultSelectedPathsProperty)
        {
            ConfigureDefaultSelectedPaths();
        }
        else if (change.Property == SwitcherRotationIconProperty)
        {
            HandleSwitcherRotationIconChanged();
        }
        else if (change.Property == SwitcherExpandIconProperty)
        {
            HandleSwitcherExpandIconChanged();
        }
        else if (change.Property == SwitcherCollapseIconProperty)
        {
            HandleSwitcherCollapseIconChanged();
        }
        else if (change.Property == SwitcherLoadingIconProperty)
        {
            HandleSwitcherLoadingIconChanged();
        }
        else if (change.Property == SwitcherLeafIconProperty)
        {
            HandleSwitcherLeafIconChanged();
        }
        else if (change.Property == DataLoaderProperty)
        {
            HasTreeItemDataLoader = DataLoader != null;
        }
        else if (change.Property == FilterProperty ||
                 change.Property == FilterHighlightStrategyProperty ||
                 change.Property == ItemsSourceProperty ||
                 change.Property == FilterValueProperty)
        {
            FilterTreeNode();
        }

        if (change.Property == IsShowEmptyIndicatorProperty ||
            change.Property == ItemsSourceProperty ||
            change.Property == FilterResultCountProperty ||
            change.Property == IsFilterModeProperty)
        {
            ConfigureEmptyIndicator();
        }
        else if (change.Property == IsSelectableProperty)
        {
            if (!IsSelectable)
            {
                SetCurrentValue(SelectedItemProperty, null);
                SelectedItems.Clear();
            }
        }
    }

    protected void HandleSwitcherRotationIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeItem.SwitcherRotationIconProperty, SwitcherRotationIcon);
            }
        }
    }
    
    protected void HandleSwitcherExpandIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeItem.SwitcherExpandIconProperty, SwitcherExpandIcon);
            }
        }
    }
    
    protected void HandleSwitcherCollapseIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeItem.SwitcherCollapseIconProperty, SwitcherCollapseIcon);
            }
        }
    }
    
    protected void HandleSwitcherLoadingIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeItem.SwitcherLoadingIconProperty, SwitcherLoadingIcon);
            }
        }
    }
    
    protected void HandleSwitcherLeafIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeItem.SwitcherLeafIconProperty, SwitcherLeafIcon);
            }
        }
    }

    private void HandleTreeItemExpanded(RoutedEventArgs args)
    {
        if (args.Source is TreeItem item)
        {
            ItemExpanded?.Invoke(this, new TreeItemExpandedEventArgs(item));
        }
    }
    
    private void HandleTreeItemCollapsed(RoutedEventArgs args)
    {
        if (args.Source is TreeItem item)
        {
            ItemCollapsed?.Invoke(this, new TreeItemCollapsedEventArgs(item));
        }
    }

    private void HandleTreeItemContextMenuRequest(RoutedEventArgs args)
    {
        if (args.Source is TreeItem item)
        {
            ItemContextMenuRequest?.Invoke(this, new TreeItemContextMenuEventArgs(item));
        }
    }
    
    private void HandleTreeItemClicked(RoutedEventArgs args)
    {
        if (args.Source is TreeItem item)
        {
            NotifyTreeItemClicked(item);
            ItemClicked?.Invoke(this, new TreeItemClickedEventArgs(item));
        }
    }

    protected virtual void NotifyTreeItemClicked(TreeItem item)
    {
    }
    
    protected virtual void ConfigureEmptyIndicator()
    {
        var isEmpty = false;
        if (IsFilterMode)
        {
            isEmpty = FilterResultCount == 0;
        }
        else
        {
            if (ItemsSource != null)
            {
                var enumerator = ItemsSource.GetEnumerator();
                isEmpty = !enumerator.MoveNext();
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            else
            {
                isEmpty = Items.Count == 0;
            }
        }
        IsEffectiveEmptyVisible = IsShowEmptyIndicator && isEmpty;
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Source is Visual source)
        {
            var point = e.GetCurrentPoint(source);
            if (point.Properties.IsLeftButtonPressed || point.Properties.IsRightButtonPressed)
            {
                if (IsSelectable)
                {
                    var keymap = Application.Current!.PlatformSettings!.HotkeyConfiguration;
                    e.Handled = UpdateSelectionFromEventSource(
                        e.Source,
                        true,
                        e.KeyModifiers.HasAllFlags(KeyModifiers.Shift),
                        e.KeyModifiers.HasAllFlags(keymap.CommandModifiers),
                        point.Properties.IsRightButtonPressed);
                }
            }
        }
        
        if (IsDraggable)
        {
            e.Handled  = true;
            _lastPoint = e.GetPosition(this);
            e.PreventGestureRecognition();
        }
    }

    private List<object> GetTreePathFromItem(object item)
    {
        var paths = new List<object>();
        if (item is ITreeItemData itemData)
        {
            var current = itemData;
            while (current != null)
            {
                paths.Add(current);
                current = current.ParentNode as ITreeItemData;
            }
        }
        else if (item is TreeItem treeViewItem)
        {
            var current = treeViewItem;
            while (current != null)
            {
                paths.Add(current);
                current = current.Parent as TreeItem;
            }
        }
        else
        {
            throw new ArgumentException("Invalid item type, Must ITreeItemData or TreeItem.");
        }

        paths.Reverse();
        return paths;
    }

    private void SelectTreeItemByPath(IList paths)
    {
        if (paths.Count == 0)
        {
            return;
        }
        ItemsControl current             = this;
        bool         originMotionEnabled = IsMotionEnabled;
        try
        {
            SetCurrentValue(IsMotionEnabledProperty, false);
            for (var i = 0; i < paths.Count; i++)
            {
                var pathNode = paths[i];
                if (pathNode != null)
                {
                    TreeItem? child          = null;
                    bool?         originExpanded = null;
                    try
                    {

                        if (current is TreeItem item)
                        {
                            originExpanded = item.IsExpanded;
                            item.SetCurrentValue(TreeItem.IsExpandedProperty, true);
                        }

                        child = GetTreeViewItemContainer(pathNode, current);
                    }
                    finally
                    {
                        if (current is TreeItem item)
                        {
                            if (originExpanded != null)
                            {
                                item.SetCurrentValue(TreeItem.IsExpandedProperty, originExpanded.Value);
                            }
                        }
                    }

                    if (child != null)
                    {
                        current = child;
                    }
                }
            }

            if (current is TreeItem treeViewItem)
            {
                treeViewItem.SetCurrentValue(TreeItem.IsSelectedProperty, true);
            }
        }
        finally
        {
            SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
        }
    }
}