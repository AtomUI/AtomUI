using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Controls.Utils;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaTreeView = Avalonia.Controls.TreeView;

public enum TreeItemHoverMode
{
    Default,
    Block,
    WholeLine
}

[Flags]
public enum TreeFilterStrategy
{
    None = 0,
    HighlightedMatch = 0x01,
    HighlightedWhole = 0x02,
    BoldedMatch = 0x04,
    ExpandPath = 0x08,
    HideUnMatched = 0x10,
    MatchedOnly = HighlightedMatch | BoldedMatch | ExpandPath | HideUnMatched,
    FullTree = HighlightedMatch | BoldedMatch | ExpandPath,
    All = MatchedOnly
}

[PseudoClasses(StdPseudoClass.Draggable)]
public partial class TreeView : AvaloniaTreeView, 
                                IMotionAwareControl,
                                IFormItemAware
{
    #region 公共属性定义
    public static readonly new DirectProperty<TreeView, IList> SelectedItemsProperty =
        AvaloniaTreeView.SelectedItemsProperty.AddOwner<TreeView>(
            o => o.SelectedItems,
            (o, v) => o.SelectedItems = v);

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

    public static readonly StyledProperty<bool> IsSelectOnRightClickProperty =
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsSelectOnRightClick), true);

    public static readonly StyledProperty<bool> IsCheckStrictlyProperty = 
        AvaloniaProperty.Register<TreeView, bool>(nameof(IsCheckStrictly), false);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TreeView>();
    
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        AvaloniaProperty.Register<TreeView, AbstractMotion?>(nameof(OpenMotion));
        
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        AvaloniaProperty.Register<TreeView, AbstractMotion?>(nameof(CloseMotion));
    
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
    
    public static readonly StyledProperty<ITreeItemNodeLoader?> DataLoaderProperty =
        AvaloniaProperty.Register<TreeView, ITreeItemNodeLoader?>(nameof(DataLoader));
    
    public static readonly StyledProperty<IValueFilter?> FilterProperty =
        AvaloniaProperty.Register<TreeView, IValueFilter?>(nameof(Filter));

    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<TreeView, object?>(nameof(FilterValue));

    public static readonly StyledProperty<DefaultFilterValueSelector?> FilterValueSelectorProperty =
        AvaloniaProperty.Register<TreeView, DefaultFilterValueSelector?>(nameof(FilterValueSelector));
    
    public static readonly StyledProperty<TreeFilterStrategy> FilterStrategyProperty =
        AvaloniaProperty.Register<TreeView, TreeFilterStrategy>(nameof(FilterStrategy), TreeFilterStrategy.All);
    
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
    
    [AllowNull]
    public new IList SelectedItems
    {
        get => base.SelectedItems;
        set
        {
            var oldValue = base.SelectedItems;
            SyncingSelectedItems = true;
            _syncingSelectedItemsTarget = value;
            try
            {
                base.SelectedItems = value;
            }
            finally
            {
                _syncingSelectedItemsTarget = null;
                SyncingSelectedItems = false;
            }
            RaisePropertyChanged(SelectedItemsProperty, oldValue, base.SelectedItems);
        }
    }

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

    public bool IsSelectOnRightClick
    {
        get => GetValue(IsSelectOnRightClickProperty);
        set => SetValue(IsSelectOnRightClickProperty, value);
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
    
    public ITreeItemNodeLoader? DataLoader
    {
        get => GetValue(DataLoaderProperty);
        set => SetValue(DataLoaderProperty, value);
    }
    
    public IValueFilter? Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    
    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }

    public DefaultFilterValueSelector? FilterValueSelector
    {
        get => GetValue(FilterValueSelectorProperty);
        set => SetValue(FilterValueSelectorProperty, value);
    }

    public TreeFilterStrategy FilterStrategy
    {
        get => GetValue(FilterStrategyProperty);
        set => SetValue(FilterStrategyProperty, value);
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
            RecordExplicitCheckedUpdateDuringReplay(true);
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

    internal static readonly DirectProperty<TreeView, bool> IsDefaultEmptyIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<TreeView, bool>(
            nameof(IsDefaultEmptyIndicatorVisible),
            o => o.IsDefaultEmptyIndicatorVisible,
            (o, v) => o.IsDefaultEmptyIndicatorVisible = v);

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Flyout.IsPopupPinnedOpenProperty.AddOwner<TreeView>();
    
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

    private bool _isDefaultEmptyIndicatorVisible;
    internal bool IsDefaultEmptyIndicatorVisible
    {
        get => _isDefaultEmptyIndicatorVisible;
        set => SetAndRaise(IsDefaultEmptyIndicatorVisibleProperty, ref _isDefaultEmptyIndicatorVisible, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }
    
    protected internal ITreeViewInteractionHandler InteractionHandler { get; }
    
    #endregion
    
    private static readonly IList Empty = Array.Empty<object>();
    private IList? _checkedItems;
    private IList? _syncingSelectedItemsTarget;
    private readonly TreeDataController _treeDataController;
    private bool _isTreeDataControllerAttached;
    internal bool SyncingSelectedItems;
    internal bool SyncingCheckedItems;
    
    internal bool IsExpandAllProcess { get; set; }
    internal bool IsCollapseAllProcess { get; set; }

    static TreeView()
    {
        ConfigureDragAndDrop();
        TreeViewItem.ExpandedEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemExpanded(args));
        TreeViewItem.CollapsedEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemCollapsed(args));
        TreeViewItem.ContextMenuRequestEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemContextMenuRequest(args));
        TreeViewItem.ClickEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleTreeItemClicked(args));
        RequestBringIntoViewEvent.AddClassHandler<TreeView>((treeView, args) => treeView.HandleDescendantRequestBringIntoView(args));
        Avalonia.Controls.Primitives.SelectingItemsControl.SelectionChangedEvent.AddClassHandler<TreeView>(
            (treeView, args) => treeView.HandleSelectionChangedDuringItemsSourceReplay(args));
        ConfigureFilter();

        SelectedItemProperty.Changed.AddClassHandler<TreeView>((treeView, args) =>
        {
            if (!treeView.IsItemsSourceReplayPending)
            {
                treeView.NotifyFormValueChanged(args.NewValue);
            }
        });
        SelectedItemsProperty.Changed.AddClassHandler<TreeView>((treeView, args) =>
        {
            if (!treeView.IsItemsSourceReplayPending)
            {
                treeView.NotifyFormValueChanged(args.NewValue);
            }
        });
    }

    public TreeView()
        : this(new DefaultTreeViewInteractionHandler(false))
    {
    }
    
    protected TreeView(ITreeViewInteractionHandler interactionHandler)
    {
        InteractionHandler = interactionHandler ?? throw new ArgumentNullException(nameof(interactionHandler));
        _treeDataController = new TreeDataController(this);
        Items.CollectionChanged           += HandleCollectionChanged;
    }

    internal bool ShouldPreserveSelectedContainerDuringSelectedItemsSync(TreeViewItem treeViewItem)
    {
        if (!SyncingSelectedItems ||
            _syncingSelectedItemsTarget is null)
        {
            return false;
        }

        var item = TreeItemFromContainer(treeViewItem);
        return item != null && _syncingSelectedItemsTarget.Contains(item);
    }

    private void HandleDescendantRequestBringIntoView(RequestBringIntoViewEventArgs args)
    {
        if (!ReferenceEquals(args.Source, this))
        {
            args.Handled = true;
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Filter ??= ValueFilterFactory.BuildFilter(ValueFilterMode.Contains);
        FilterValueSelector ??= DefaultTreeFilterValueSelector;
        ConfigureEmptyIndicator();
    }

    internal static readonly DefaultFilterValueSelector DefaultTreeFilterValueSelector = value =>
    {
        if (value is TreeViewItem treeViewItem)
        {
            if (treeViewItem.Header is ITreeItemNode treeItemData)
            {
                return treeItemData.Header?.ToString();
            }
            if (treeViewItem.Header is string header)
            {
                return header;
            }
            return treeViewItem.Header?.ToString();
        }
        return null;
    };
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigureEmptyIndicator();
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                var oldItems = e.OldItems!;
                for (var i = 0; i < oldItems.Count; i++)
                {
                    if (!ShouldPreserveRemovedTreeItem(oldItems[i]))
                    {
                        CheckedItems.Remove(oldItems[i]);
                    }
                }
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

            foreach (var item in Items)
            {
                if (item != null)
                {
                    if (TreeContainerFromItem(item) is TreeViewItem treeItem)
                    {
                        ExpandSubTree(treeItem);
                    }
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
                if (ContainerFromIndex(i) is TreeViewItem treeItem)
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

    public void CheckedSubTree(TreeViewItem viewItem)
    {
        ApplyCheckedSubTree(viewItem);
    }

    protected virtual bool RecursiveCheckNodePredicate(TreeViewItem treeViewItem)
    {
        return true;
    }
    
    protected virtual bool RecursiveUnCheckNodePredicate(TreeViewItem treeViewItem)
    {
        return true;
    }

    public void UnCheckedSubTree(TreeViewItem viewItem)
    {
        ApplyUnCheckedSubTree(viewItem);
    }

    public ISet<object> DoUnCheckedSubTree(TreeViewItem treeViewItem)
    {
        return CollectUnCheckedSubTreeItems(treeViewItem);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.Draggable, IsDraggable);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var treeViewItem = new TreeViewItem();
        treeViewItem.Classes.Add(TreeViewSemanticParts.ItemClass);
        return treeViewItem;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TreeViewItem>(item, out recycleKey);
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is TreeViewItem treeViewItem)
        {
            treeViewItem.Classes.Add(TreeViewSemanticParts.ItemClass);
            treeViewItem.OwnerTreeView = this;
            
            if (item != null && item is not Visual && item is ITreeItemNode treeViewItemData)
            {
                treeViewItem.PrepareTreeItemNodeData(treeViewItemData, this);
            }
            
            if (ItemTemplate != null)
            {
                treeViewItem[!TreeViewItem.HeaderTemplateProperty] = this[!ItemTemplateProperty];
            }
            
            SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherExpandIconProperty, SwitcherExpandIcon);
            SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherCollapseIconProperty, SwitcherCollapseIcon);
            SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherRotationIconProperty, SwitcherRotationIcon);
            SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherLoadingIconProperty, SwitcherLoadingIcon);
            SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherLeafIconProperty, SwitcherLeafIcon);
            
            treeViewItem[!TreeViewItem.FilterStrategyProperty]           = this[!FilterStrategyProperty];
            treeViewItem[!TreeViewItem.IsMotionEnabledProperty]           = this[!IsMotionEnabledProperty];
            treeViewItem[!TreeViewItem.NodeHoverModeProperty]             = this[!NodeHoverModeProperty];
            treeViewItem[!TreeViewItem.IsShowLineProperty]                = this[!IsShowLineProperty];
            treeViewItem[!TreeViewItem.IsShowIconProperty]                = this[!IsShowIconProperty];
            treeViewItem[!TreeViewItem.IsShowLeafIconProperty]            = this[!IsShowLeafIconProperty];
            treeViewItem[!TreeViewItem.IsSwitcherRotationProperty]        = this[!IsSwitcherRotationProperty];
            treeViewItem[!TreeViewItem.ToggleTypeProperty]                = this[!ToggleTypeProperty];
            treeViewItem[!TreeViewItem.IsSelectableProperty]              = this[!IsSelectableProperty];
            treeViewItem[!TreeViewItem.FilterHighlightForegroundProperty] = this[!FilterHighlightForegroundProperty];
            treeViewItem[!TreeViewItem.HasTreeItemDataLoaderProperty]     = this[!HasTreeItemDataLoaderProperty];
            treeViewItem[!TreeViewItem.IsAutoExpandParentProperty]        = this[!IsAutoExpandParentProperty];
            
            PrepareTreeViewItem(treeViewItem, item, index);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TreeItem.");
        }
    }

    private void SetTreeViewItemIcon(TreeViewItem treeViewItem, AvaloniaProperty iconProperty, IIconTemplate? iconTemplate)
    {
        if (iconTemplate == null)
        {
            treeViewItem.SetValue(iconProperty, null);
        }
        else
        {
            treeViewItem.SetValue(iconProperty, iconTemplate.Build());
        }
    }
    
    protected virtual void PrepareTreeViewItem(TreeViewItem treeViewItem, object? item, int index)
    {
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is TreeViewItem treeViewItem)
        {
            var shouldReleaseTreeDataTemplateBinding = treeViewItem.Header is BindableTreeItemNode;
            treeViewItem.ClearPreparedTreeItemNodeData();
            base.ClearContainerForItemOverride(container);
            if (shouldReleaseTreeDataTemplateBinding)
            {
                // HeaderedItemsControl keeps TreeDataTemplate children binding in an internal disposable.
                // Preparing once with a null item lets Avalonia release that binding before recycling.
                base.PrepareContainerForItemOverride(container, null, -1);
                base.ClearContainerForItemOverride(container);
            }
            return;
        }

        base.ClearContainerForItemOverride(container);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isTreeDataControllerAttached = true;
        _treeDataController.SetRootSource(ItemsSource);
        InteractionHandler.Attach(this);
        UpdatePseudoClasses();
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        FinishDrag();
        base.OnDetachedFromVisualTree(e);
        CancelItemsSourceReplay();
        InteractionHandler.Detach(this);
        
        // 清理所有待处理的异步加载操作
        _asyncLoadCoordinator.CancelAll();
        _treeDataController.ClearRootSource();
        _isTreeDataControllerAttached = false;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ReplayLoadedState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var isItemsSourceChange = change.Property == ItemsSourceProperty;
        var replayRevision = isItemsSourceChange ? BeginItemsSourceReplay() : 0;
        if (isItemsSourceChange)
        {
            _itemsSourceReplayInternalDepth++;
        }
        try
        {
            base.OnPropertyChanged(change);
            HandlePropertyChangedCore(change, replayRevision);
        }
        catch
        {
            if (isItemsSourceChange)
            {
                CancelItemsSourceReplay();
            }
            throw;
        }
        finally
        {
            if (isItemsSourceChange)
            {
                _itemsSourceReplayInternalDepth--;
            }
        }
    }

    internal ITreeItemNode? ResolveTreeItemNode(TreeViewItem treeViewItem)
    {
        return TreeItemFromContainer(treeViewItem) as ITreeItemNode ??
               treeViewItem.Header as ITreeItemNode ??
               treeViewItem;
    }

    internal bool ShouldPreserveRemovedTreeItem(object? item)
    {
        return item is ITreeItemNode node &&
               _treeDataController.IsMovingNode(node);
    }

    protected void HandleSwitcherRotationIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeViewItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherRotationIconProperty, SwitcherRotationIcon);
            }
        }
    }
    
    protected void HandleSwitcherExpandIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeViewItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherExpandIconProperty, SwitcherExpandIcon);
            }
        }
    }
    
    protected void HandleSwitcherCollapseIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeViewItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherCollapseIconProperty, SwitcherCollapseIcon);
            }
        }
    }
    
    protected void HandleSwitcherLoadingIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeViewItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherLoadingIconProperty, SwitcherLoadingIcon);
            }
        }
    }
    
    protected void HandleSwitcherLeafIconChanged()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is TreeViewItem treeViewItem)
            {
                SetTreeViewItemIcon(treeViewItem, TreeViewItem.SwitcherLeafIconProperty, SwitcherLeafIcon);
            }
        }
    }

    private void HandleTreeItemExpanded(RoutedEventArgs args)
    {
        if (args.Source is TreeViewItem item)
        {
            ItemExpanded?.Invoke(this, new TreeItemExpandedEventArgs(item));
        }
    }
    
    private void HandleTreeItemCollapsed(RoutedEventArgs args)
    {
        if (args.Source is TreeViewItem item)
        {
            ItemCollapsed?.Invoke(this, new TreeItemCollapsedEventArgs(item));
        }
    }

    private void HandleTreeItemContextMenuRequest(RoutedEventArgs args)
    {
        if (args.Source is TreeViewItem item)
        {
            ItemContextMenuRequest?.Invoke(this, new TreeItemContextMenuEventArgs(item));
        }
    }
    
    private void HandleTreeItemClicked(RoutedEventArgs args)
    {
        if (args.Source is TreeViewItem item)
        {
            NotifyTreeItemClicked(item);
            ItemClicked?.Invoke(this, new TreeItemClickedEventArgs(item));
        }
    }

    protected virtual void NotifyTreeItemClicked(TreeViewItem viewItem)
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
        IsEffectiveEmptyVisible        = IsShowEmptyIndicator && isEmpty;
        IsDefaultEmptyIndicatorVisible = IsEffectiveEmptyVisible && EmptyIndicator is null && EmptyIndicatorTemplate is null;
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var container = DefaultTreeViewInteractionHandler.GetTreeViewItemCore(e.Source as Control);
        if (container is not null && e.Source is Visual source)
        {
            var point = e.GetCurrentPoint(source);
            if (point.Properties.IsLeftButtonPressed || point.Properties.IsRightButtonPressed)
            {
                if (IsSelectable)
                {
                    UpdateSelectionFromEvent(container, e);
                }
            }

            if (point.Properties.IsLeftButtonPressed && IsDraggable)
            {
                _lastPoint = e.GetPosition(this);
                e.PreventGestureRecognition();
            }
        }
    }

    public override bool UpdateSelectionFromEvent(Control container, RoutedEventArgs eventArgs)
    {
        if (!IsSelectOnRightClick &&
            eventArgs is PointerEventArgs pointerEvent &&
            pointerEvent.GetCurrentPoint(container).Properties.PointerUpdateKind is
                PointerUpdateKind.RightButtonPressed or PointerUpdateKind.RightButtonReleased)
        {
            return false;
        }

        return base.UpdateSelectionFromEvent(container, eventArgs);
    }

    #region 实现 FormItem 接口
    
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(object? value)
    {
        if ((SelectionMode & SelectionMode.Multiple) == SelectionMode.Multiple)
        {
            SelectedItems = value as IList;
        }
        else
        {
            SelectedItem = value;
        }
    }

    protected virtual object? NotifyGetFormValue()
    {
        if ((SelectionMode & SelectionMode.Multiple) == SelectionMode.Multiple)
        {
            return SelectedItems;
        }
        return SelectedItem;
    }

    protected virtual void NotifyClearFormValue()
    {
        if ((SelectionMode & SelectionMode.Multiple) == SelectionMode.Multiple)
        {
            SelectedItems = null;
        }
        else
        {
            SelectedItem = null;
        }
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion

    private void HandlePropertyChangedCore(AvaloniaPropertyChangedEventArgs change, int replayRevision)
    {
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
                 change.Property == FilterStrategyProperty ||
                 change.Property == ItemsSourceProperty ||
                 change.Property == FilterValueProperty)
        {
            FilterTreeNode();
        }

        if (change.Property == IsShowEmptyIndicatorProperty ||
            change.Property == ItemsSourceProperty ||
            change.Property == FilterResultCountProperty ||
            change.Property == IsFilterModeProperty ||
            change.Property == EmptyIndicatorProperty ||
            change.Property == EmptyIndicatorTemplateProperty)
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

        if (change.Property == ItemsSourceProperty)
        {
            if (_isTreeDataControllerAttached)
            {
                _treeDataController.SetRootSource(ItemsSource);
            }
            Dispatcher.Post(() => CompleteItemsSourceReplay(replayRevision));
        }
        else if (change.Property == SelectedItemProperty || change.Property == SelectedItemsProperty)
        {
            RecordExplicitSelectionUpdateDuringReplay(true);
        }

    }
}
