using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Animations;
using AtomUI.Controls.Primitives;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

using AvaloniaTreeItem = Avalonia.Controls.TreeViewItem;

// Token-owner transparent item container: the item, header and switcher themes read the hosting tree's Tokens (TreeView,
// or the host of a derived tree such as TreeSelect), so Controls[TreeView] also reaches the nodes and their transitions.
/// <summary>
/// The container of one node in a <see cref="TreeView" />: it hosts the node header and the children viewport,
/// mirrors the state of the item data it was prepared from, and routes checking, selection and expansion
/// through the interaction handler of the owning tree.
/// </summary>
[TokenOwnerTransparent]
[PseudoClasses(TreeViewPseudoClass.NodeToggleTypeCheckBox, TreeViewPseudoClass.NodeToggleTypeRadio, TreeViewPseudoClass.KeyboardActive)]
public partial class TreeViewItem : AvaloniaTreeItem, IRadioButton, ITreeItemNode
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<TreeViewItem, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="IsChecked" /> property.
    /// </summary>
    public static readonly StyledProperty<bool?> IsCheckedProperty =
        AvaloniaProperty.Register<TreeViewItem, bool?>(nameof(IsChecked), false);

    /// <summary>
    /// Identifies the <see cref="IsLeaf" /> property.
    /// </summary>
    public static readonly DirectProperty<TreeViewItem, bool> IsLeafProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsLeaf),
            o => o.IsLeaf);

    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<TreeViewItem, bool>(nameof(IsLoading), false);
    
    /// <summary>
    /// Identifies the <see cref="GroupName" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> GroupNameProperty =
        RadioButton.GroupNameProperty.AddOwner<TreeViewItem>();
    
    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly DirectProperty<TreeViewItem, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, object?>(nameof(Value),
            o => o.Value,
           (o, v) => o.Value = v);
    
    /// <summary>
    /// Identifies the <see cref="IsIndicatorEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndicatorEnabledProperty =
        AvaloniaProperty.Register<TreeViewItem, bool>(nameof(IsIndicatorEnabled), true);

    /// <summary>
    /// The icon shown in the node row. It is visible only while <see cref="TreeView.IsShowIcon" /> is
    /// <see langword="true" />; <see langword="null" /> shows no icon. The value is copied from the node data when
    /// the container is prepared and follows later changes of a <see cref="BindableTreeItemNode" />.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// The check state of the node: <see langword="true" /> for checked and <see langword="false" /> for unchecked;
    /// <see langword="null" /> reads as <see langword="false" /> through <see cref="AtomUI.Controls.IRadioButton.IsChecked" />.
    /// A change that is not part of applying node data is reported to the interaction handler of the owning tree
    /// as a check toggle. The default is <see langword="false" />.
    /// </summary>
    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    private bool _isLeaf;

    /// <summary>
    /// Whether the node has no children. The header uses it to present the node as a leaf, with the leaf icon in
    /// place of the switcher when the tree shows leaf icons. The value comes from the node data and is recomputed
    /// from <c>ItemCount</c> when the item count changes: without a data loader a node is a leaf when it has no
    /// children, with a data loader only once its asynchronous load has finished.
    /// </summary>
    public bool IsLeaf
    {
        get => _isLeaf;
        internal set => SetAndRaise(IsLeafProperty, ref _isLeaf, value);
    }
    
    private object? _value;

    /// <summary>
    /// A value the application stores with the node. It is copied from the node data when the container is prepared
    /// and, while <see cref="ItemKey" /> is <see langword="null" />, its string form identifies the node in node
    /// paths.
    /// </summary>
    public object? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    /// <summary>
    /// Whether the children of the node are being loaded asynchronously. The owning tree sets it while a data
    /// loader request runs, the header is bound to it, and <see cref="RequestToggleExpand" /> returns
    /// <see langword="false" /> while it is <see langword="true" />.
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// The radio group of the node's check indicator, shared with <see cref="Avalonia.Controls.RadioButton.GroupName" />. Nodes that
    /// share a group name accept only one checked node among them. Set it on the item or on its node data. A change
    /// moves the item from its previous group to the new one in the group manager of the owning tree; a change to
    /// <see langword="null" /> or an empty string only removes the item from its previous group.
    /// </summary>
    public string? GroupName
    {
        get => GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }
    
    /// <summary>
    /// Whether the check indicator of the node accepts input. With <see langword="false" /> the indicator is
    /// disabled and the node is not checkable, whatever <see cref="ToggleType" /> is. Set it on the item or on its
    /// node data; the default is <see langword="true" />.
    /// </summary>
    public bool IsIndicatorEnabled
    {
        get => GetValue(IsIndicatorEnabledProperty);
        set => SetValue(IsIndicatorEnabledProperty, value);
    }
    
    IEnumerable<ITreeItemNode> ITreeNode<ITreeItemNode>.Children => GetTreeItemChildren();
    ITreeNode<ITreeItemNode>? ITreeNode<ITreeItemNode>.ParentNode => Parent as ITreeNode<ITreeItemNode>;

    private IEnumerable<ITreeItemNode> GetTreeItemChildren()
    {
        foreach (var item in Items)
        {
            if (item is ITreeItemNode treeItemNode)
            {
                yield return treeItemNode;
            }
        }
    }

    // Explicit interface implementation for IRadioButton
    bool IRadioButton.IsChecked
    {
        get => IsChecked ?? false;
        set => IsChecked = value;
    }

    /// <summary>
    /// The key that identifies the node in node paths. A path segment matches this key, or the string form of
    /// <see cref="Value" /> when this is <see langword="null" />. Set it on the item or on its node data.
    /// </summary>
    public EntityKey? ItemKey { get; set; }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="Click" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<TreeViewItem, RoutedEventArgs>(
            nameof(Click),
            RoutingStrategies.Bubble);
    
    /// <summary>
    /// Identifies the <see cref="ContextMenuRequest" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ContextMenuRequestEvent =
        RoutedEvent.Register<TreeViewItem, RoutedEventArgs>(nameof(ContextMenuRequest), RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    
    /// <summary>
    /// Raised when the left mouse button is released inside the node header. The default interaction handler of the
    /// tree calls <see cref="NotifyHeaderClick" /> first and then raises this event, which carries a plain
    /// <see cref="RoutedEventArgs" />.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }
    
    /// <summary>
    /// Raised when the right mouse button is released inside the node header. The default interaction handler
    /// raises it after it closed the context menus open in the tree and, when <c>IsSelectOnRightClick</c> is on,
    /// after it selected the node. The event tunnels and bubbles; the owning <see cref="TreeView" /> handles it
    /// and raises <see cref="TreeView.ItemContextMenuRequest" />.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? ContextMenuRequest
    {
        add => AddHandler(ContextMenuRequestEvent, value);
        remove => RemoveHandler(ContextMenuRequestEvent, value);
    }
    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<PathIcon?> SwitcherExpandIconProperty =
        AvaloniaProperty.Register<TreeViewItem, PathIcon?>(nameof(SwitcherExpandIcon));

    internal static readonly StyledProperty<PathIcon?> SwitcherCollapseIconProperty =
        AvaloniaProperty.Register<TreeViewItem, PathIcon?>(nameof(SwitcherCollapseIcon));

    internal static readonly StyledProperty<PathIcon?> SwitcherRotationIconProperty =
        AvaloniaProperty.Register<TreeViewItem, PathIcon?>(nameof(SwitcherRotationIcon));

    internal static readonly StyledProperty<PathIcon?> SwitcherLoadingIconProperty =
        AvaloniaProperty.Register<TreeViewItem, PathIcon?>(nameof(SwitcherLoadingIcon));

    internal static readonly StyledProperty<PathIcon?> SwitcherLeafIconProperty =
        AvaloniaProperty.Register<TreeViewItem, PathIcon?>(nameof(SwitcherLeafIcon));

    internal static readonly DirectProperty<TreeViewItem, TreeItemHoverMode> NodeHoverModeProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, TreeItemHoverMode>(nameof(NodeHoverMode),
            o => o.NodeHoverMode,
            (o, v) => o.NodeHoverMode = v);

    internal static readonly DirectProperty<TreeViewItem, bool> IsShowLineProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsShowLine),
            o => o.IsShowLine,
            (o, v) => o.IsShowLine = v);

    internal static readonly DirectProperty<TreeViewItem, bool> IsShowIconProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsShowIcon),
            o => o.IsShowIcon,
            (o, v) => o.IsShowIcon = v);

    internal static readonly DirectProperty<TreeViewItem, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsDragging),
            o => o.IsDragging,
            (o, v) => o.IsDragging = v);

    internal static readonly DirectProperty<TreeViewItem, bool> IsKeyboardActiveProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsKeyboardActive),
            o => o.IsKeyboardActive,
            (o, v) => o.IsKeyboardActive = v);

    internal static readonly DirectProperty<TreeViewItem, bool> IsKeyboardFocusOutlineVisibleProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsKeyboardFocusOutlineVisible),
            o => o.IsKeyboardFocusOutlineVisible,
            (o, v) => o.IsKeyboardFocusOutlineVisible = v);

    internal static readonly DirectProperty<TreeViewItem, bool> IsDragOverProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsDragOver),
            o => o.IsDragOver,
            (o, v) => o.IsDragOver = v);

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TreeViewItem>();

    internal static readonly DirectProperty<TreeViewItem, bool> IsSwitcherRotationProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(
            nameof(IsSwitcherRotation),
            o => o.IsSwitcherRotation,
            (o, v) => o.IsSwitcherRotation = v);
    
    internal static readonly StyledProperty<ItemToggleType> ToggleTypeProperty =
        TreeView.ToggleTypeProperty.AddOwner<TreeViewItem>();
    
    internal static readonly StyledProperty<bool> IsShowLeafIconProperty =
        AvaloniaProperty.Register<TreeViewItem, bool>(nameof(IsShowLeafIcon));
    
    internal static readonly DirectProperty<TreeViewItem, bool> HasTreeItemDataLoaderProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(HasTreeItemDataLoader),
            o => o.HasTreeItemDataLoader,
            (o, v) => o.HasTreeItemDataLoader = v);
    
    internal static readonly DirectProperty<TreeViewItem, bool> IsAutoExpandParentProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsAutoExpandParent),
            o => o.IsAutoExpandParent,
            (o, v) => o.IsAutoExpandParent = v);
    
    internal static readonly DirectProperty<TreeViewItem, bool> IsFilterModeProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsFilterMode),
            o => o.IsFilterMode,
            (o, v) => o.IsFilterMode = v);
    
    internal static readonly DirectProperty<TreeViewItem, bool> IsFilterMatchProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsFilterMatch),
            o => o.IsFilterMatch,
            (o, v) => o.IsFilterMatch = v);
    
    internal static readonly DirectProperty<TreeViewItem, string?> FilterHighlightWordsProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, string?>(nameof(FilterHighlightWords),
            o => o.FilterHighlightWords,
            (o, v) => o.FilterHighlightWords = v);

    internal static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        TreeView.FilterHighlightForegroundProperty.AddOwner<TreeViewItem>();
    
    internal static readonly DirectProperty<TreeViewItem, TreeFilterStrategy> FilterStrategyProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, TreeFilterStrategy>(
            nameof(FilterStrategy),
            o => o.FilterStrategy,
            (o, v) => o.FilterStrategy = v);
    
    internal static readonly DirectProperty<TreeViewItem, bool> IsSelectableProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(
            nameof(IsSelectable),
            o => o.IsSelectable,
            (o, v) => o.IsSelectable = v);
    
    internal PathIcon? SwitcherExpandIcon
    {
        get => GetValue(SwitcherExpandIconProperty);
        set => SetValue(SwitcherExpandIconProperty, value);
    }

    internal PathIcon? SwitcherCollapseIcon
    {
        get => GetValue(SwitcherCollapseIconProperty);
        set => SetValue(SwitcherCollapseIconProperty, value);
    }

    internal PathIcon? SwitcherRotationIcon
    {
        get => GetValue(SwitcherRotationIconProperty);
        set => SetValue(SwitcherRotationIconProperty, value);
    }

    internal PathIcon? SwitcherLoadingIcon
    {
        get => GetValue(SwitcherLoadingIconProperty);
        set => SetValue(SwitcherLoadingIconProperty, value);
    }

    internal PathIcon? SwitcherLeafIcon
    {
        get => GetValue(SwitcherLeafIconProperty);
        set => SetValue(SwitcherLeafIconProperty, value);
    }
    
    private TreeItemHoverMode _nodeHoverMode;

    internal TreeItemHoverMode NodeHoverMode
    {
        get => _nodeHoverMode;
        set => SetAndRaise(NodeHoverModeProperty, ref _nodeHoverMode, value);
    }

    private bool _isShowLine;

    internal bool IsShowLine
    {
        get => _isShowLine;
        set => SetAndRaise(IsShowLineProperty, ref _isShowLine, value);
    }

    private bool _isShowIcon = true;

    internal bool IsShowIcon
    {
        get => _isShowIcon;
        set => SetAndRaise(IsShowIconProperty, ref _isShowIcon, value);
    }

    private bool _isDragging;

    internal bool IsDragging
    {
        get => _isDragging;
        set => SetAndRaise(IsDraggingProperty, ref _isDragging, value);
    }

    private bool _isKeyboardActive;

    internal bool IsKeyboardActive
    {
        get => _isKeyboardActive;
        set => SetAndRaise(IsKeyboardActiveProperty, ref _isKeyboardActive, value);
    }

    private bool _isKeyboardFocusOutlineVisible;

    /// <summary>
    /// The item is the keyboard active item while the tree itself owns keyboard focus, so its header
    /// shows the focus outline (Ant Design: list <c>:focus-visible</c> outlines the active node).
    /// </summary>
    internal bool IsKeyboardFocusOutlineVisible
    {
        get => _isKeyboardFocusOutlineVisible;
        set => SetAndRaise(IsKeyboardFocusOutlineVisibleProperty, ref _isKeyboardFocusOutlineVisible, value);
    }

    private bool _isDragOver;

    internal bool IsDragOver
    {
        get => _isDragOver;
        set => SetAndRaise(IsDragOverProperty, ref _isDragOver, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    private bool _isSwitcherRotation;

    internal bool IsSwitcherRotation
    {
        get => _isSwitcherRotation;
        set => SetAndRaise(IsSwitcherRotationProperty, ref _isSwitcherRotation, value);
    }
    
    /// <summary>
    /// The kind of check indicator the node shows: none, a check box or a radio button. The owning
    /// <see cref="TreeView" /> forwards its value to every item container, so set it there and not on a single
    /// node. A change updates the pseudo-classes of the item and re-registers it with its radio group.
    /// </summary>
    public ItemToggleType ToggleType
    {
        get => GetValue(ToggleTypeProperty);
        set => SetValue(ToggleTypeProperty, value);
    }

    internal bool IsShowLeafIcon
    {
        get => GetValue(IsShowLeafIconProperty);
        set => SetValue(IsShowLeafIconProperty, value);
    }
    
    private bool _hasTreeItemDataLoader;

    internal bool HasTreeItemDataLoader
    {
        get => _hasTreeItemDataLoader;
        set => SetAndRaise(HasTreeItemDataLoaderProperty, ref _hasTreeItemDataLoader, value);
    }
        
    private bool _isAutoExpandParent;

    internal bool IsAutoExpandParent
    {
        get => _isAutoExpandParent;
        set => SetAndRaise(IsAutoExpandParentProperty, ref _isAutoExpandParent, value);
    }
    
    private bool _isFilterMode;

    internal bool IsFilterMode
    {
        get => _isFilterMode;
        set => SetAndRaise(IsFilterModeProperty, ref _isFilterMode, value);
    }
    
    private bool _isFilterMatch;

    internal bool IsFilterMatch
    {
        get => _isFilterMatch;
        set => SetAndRaise(IsFilterMatchProperty, ref _isFilterMatch, value);
    }
    
    private string? _filterHighlightWords;

    internal string? FilterHighlightWords
    {
        get => _filterHighlightWords;
        set => SetAndRaise(FilterHighlightWordsProperty, ref _filterHighlightWords, value);
    }
    
    internal IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    
    private TreeFilterStrategy _filterStrategy = TreeFilterStrategy.All;
    
    internal TreeFilterStrategy FilterStrategy
    {
        get => _filterStrategy;
        set => SetAndRaise(FilterStrategyProperty, ref _filterStrategy, value);
    }

    private bool _isSelectable = true;
    
    internal bool IsSelectable
    {
        get => _isSelectable;
        set => SetAndRaise(IsSelectableProperty, ref _isSelectable, value);
    }
    
    internal TreeView? OwnerTreeView { get; set; }

    
    #endregion
    
    private static readonly AbstractMotion DefaultOpenMotion = new ExpandMotion(Direction.Top, easing: new CubicEaseOut());
    private static readonly AbstractMotion DefaultCloseMotion = new CollapseMotion(Direction.Top, easing: new CubicEaseIn());
    private ContentViewport? _childrenViewport;
    private long _childrenRequestVersion;
    private TreeViewItemHeader? _header;
    private TreeViewItemDecoration? _decoration;
    internal TreeViewItemHeader? HeaderForDecoration => _header;
    internal bool AreChildrenVisibleForDecoration => _childrenViewport?.IsVisible == true;
    internal bool AsyncLoaded;
    private FilterContextBackup? _filterContextBackup;
    private CompositeDisposable? _treeItemNodeBindingDisposables;
    // Plain ITreeItemNode data has no change notification, so the container writes its interactive state
    // back to it. A virtualizing panel prepares recycled containers from the node data again, which then
    // carries the latest state; BindableTreeItemNode gets the same through its two-way sync.
    private ITreeItemNode? _stateWriteBackTreeItemNode;
    private bool _isApplyingNodeData;
    // In checkbox mode the owning TreeView reconciles the node's checked state with CheckedItems once the node
    // data is applied, so the IsChecked change of the apply is not handled as a toggle.
    private bool _isCheckedStateReconciledByOwner;
    // Prepare, clear and logical (re)attach swap ItemsSource for the container's own reasons; such resets and
    // replacements do not remove nodes from the data tree.
    private int _containerLifecycleDepth;
    private bool _isItemsSourceChanging;

    static TreeViewItem()
    {
        IsSelectedProperty.OverrideMetadata<TreeViewItem>(
            new StyledPropertyMetadata<bool>(coerce: CoerceIsSelected));
        SelectingItemsControl.IsSelectedChangedEvent.AddClassHandler<TreeViewItem>(
            (treeViewItem, args) => treeViewItem.HandleIsSelectedChanged(args));
    }

    public TreeViewItem()
    {
        Items.CollectionChanged += HandleCollectionChanged;
    }

    internal void PrepareTreeItemNodeData(ITreeItemNode treeItemData, IResourceHost resourceHost)
    {
        // Node bindings are only valid while this container belongs to a TreeView's logical
        // tree: that membership is what guarantees Avalonia's EnsureTreeView cannot throw and
        // what keeps the container collectable once discarded. Membership can genuinely be
        // absent here because Avalonia re-realizes containers from a still-live ItemsView
        // while a detached parent is being torn down (RefreshContainers triggered by the
        // ItemTemplate clear in ClearContainerForItemOverride). All three call sites — the
        // TreeView-level prepare (also reached for nested containers through Avalonia's
        // TreeViewItem._treeView forwarding, which is never reset after detach), the
        // TreeViewItem-level prepare, and logical re-attach — funnel through this method, so
        // the membership invariant is enforced here rather than at each caller.
        if (this.GetLogicalAncestors().OfType<TreeView>().FirstOrDefault() is not { } treeView)
        {
            return;
        }

        ClearTreeItemNodeBindingDisposables();

        var isCheckedStateReconciledByOwner = treeView.ToggleType == ItemToggleType.CheckBox;
        if (treeItemData is BindableTreeItemNode bindableTreeItemNode)
        {
            var disposables = new CompositeDisposable();
            _treeItemNodeBindingDisposables = disposables;
            disposables.Add(bindableTreeItemNode.AttachResourceHost(resourceHost));

            ApplyNodeData(this, treeItemData, isCheckedStateReconciledByOwner);
            BindBindableTreeItemNode(bindableTreeItemNode, disposables);
        }
        else
        {
            ApplyNodeData(this, treeItemData, isCheckedStateReconciledByOwner);
            _stateWriteBackTreeItemNode = treeItemData;
        }

        treeView.ReconcilePreparedCheckedState(this, treeItemData);
    }

    internal ContainerLifecycleScope BeginContainerLifecycle()
    {
        return new ContainerLifecycleScope(this);
    }

    // A struct scope: prepare and clear run for every container a virtualizing panel recycles.
    internal readonly struct ContainerLifecycleScope : IDisposable
    {
        private readonly TreeViewItem _owner;

        public ContainerLifecycleScope(TreeViewItem owner)
        {
            _owner = owner;
            ++owner._containerLifecycleDepth;
        }

        public void Dispose()
        {
            --_owner._containerLifecycleDepth;
        }
    }

    internal void ClearTreeItemNodeBindingDisposables()
    {
        _treeItemNodeBindingDisposables?.Dispose();
        _treeItemNodeBindingDisposables = null;
        _stateWriteBackTreeItemNode     = null;
    }

    internal void ClearPreparedTreeItemNodeData()
    {
        ClearTreeItemNodeBindingDisposables();
        // Only template-bound BindableTreeItemNode containers may drop ItemsSource here: the
        // resulting ItemsView reset synchronizes removed children out of TreeView.CheckedItems
        // and SelectedItems, which would destroy state that outlives the container otherwise.
        if (Header is BindableTreeItemNode)
        {
            using (BeginContainerLifecycle())
            {
                ClearValue(ItemsSourceProperty);
            }
        }
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Swapping ItemsSource replays the old children as removed (or a Reset); the swap itself decides, with
        // both collections known, whether nodes left the data tree.
        if (OwnerTreeView is null || _isItemsSourceChanging)
        {
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                var oldItems = e.OldItems!;
                for (var i = 0; i < oldItems.Count; i++)
                {
                    OwnerTreeView.RemoveCheckedItemOfRemovedNode(oldItems[i]);
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                OwnerTreeView.RemoveDetachedCheckedItems();
                break;
        }
    }

    /// <summary>
    /// Invalidates a pending expansion animation, calls the base method, tells the owning tree that the item left
    /// the tree, and clears <see cref="OwnerTreeView" />.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ++_childrenRequestVersion;
        base.OnDetachedFromVisualTree(e);
        OwnerTreeView?.NotifyTreeViewItemDetached(this);
        OwnerTreeView = null;
    }

    /// <summary>
    /// Calls the base method, resolves <see cref="OwnerTreeView" /> from the visual ancestors and brings the
    /// children viewport in line with the current expanded state without motion.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OwnerTreeView = this.FindAncestorOfType<TreeView>();
        HandleExpandedChanged(true);
    }

    /// <summary>
    /// Calls the base method inside a container lifecycle scope, so that the children it binds are not treated as
    /// nodes removed from the data tree. A container whose header is a <see cref="BindableTreeItemNode" /> and
    /// has no node bindings yet is then prepared again while a <see cref="TreeView" /> is a logical ancestor.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        // HeaderedItemsControl may bind the children it deferred while detached.
        using (BeginContainerLifecycle())
        {
            base.OnAttachedToLogicalTree(e);
        }

        // Node bindings follow logical-tree membership: this is the same ancestor chain Avalonia
        // uses for TreeViewItem._treeView (EnsureTreeView), so base.PrepareContainerForItemOverride
        // is guaranteed not to throw here. A visual-tree attach handler cannot provide that
        // guarantee: during container teardown a container can still be visually attached while
        // its logical chain is already cut.
        var treeView = this.GetLogicalAncestors().OfType<TreeView>().FirstOrDefault();
        if (_treeItemNodeBindingDisposables is null &&
            Header is BindableTreeItemNode bindableTreeItemNode &&
            treeView is not null)
        {
            using (BeginContainerLifecycle())
            {
                base.PrepareContainerForItemOverride(this, bindableTreeItemNode, -1);
            }
            var resourceHost = this.GetLogicalAncestors().OfType<TreeViewItem>().FirstOrDefault() as IResourceHost ?? treeView;
            PrepareTreeItemNodeData(bindableTreeItemNode, resourceHost);
        }
    }

    /// <summary>
    /// Releases the prepared node data before the base method detaches the item, inside a container lifecycle scope.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        using (BeginContainerLifecycle())
        {
            ClearPreparedTreeItemNodeData();
            base.OnDetachedFromLogicalTree(e);
        }
    }
    
    /// <summary>
    /// Calls the base method and then keeps the node in step with the change: it writes check, selection and
    /// expansion changes back to plain node data, recomputes <see cref="IsLeaf" />, updates the pseudo-classes,
    /// re-runs the expand or collapse of the children viewport and reports check, group and toggle type changes to
    /// the interaction handler of the owning tree. A swap of <c>ItemsSource</c> is guarded so that the replaced
    /// children are not treated as nodes removed from the data tree.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The item becomes selected while <see cref="IsSelectable" /> is <see langword="false" />. The selection is
    /// withdrawn before the exception is thrown.
    /// </exception>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ItemsSourceProperty)
        {
            // ItemsControl answers the swap with a Reset of Items from inside base.OnPropertyChanged.
            var wasItemsSourceChanging = _isItemsSourceChanging;
            _isItemsSourceChanging = true;
            try
            {
                base.OnPropertyChanged(change);
            }
            finally
            {
                _isItemsSourceChanging = wasItemsSourceChanging;
            }
            HandleItemsSourceChanged(change);
        }
        else
        {
            base.OnPropertyChanged(change);
        }

        if (change.Property == IsShowLineProperty || change.Property == IsShowLeafIconProperty ||
            change.Property == IsDraggingProperty || change.Property == IsDragOverProperty ||
            change.Property == BorderBrushProperty || change.Property == BorderThicknessProperty ||
            change.Property == UseLayoutRoundingProperty || change.Property == NodeHoverModeProperty ||
            change.Property == BackgroundProperty || change.Property == IsExpandedProperty ||
            change.Property == IsLeafProperty)
        {
            _decoration?.InvalidateVisual();
        }

        if (_stateWriteBackTreeItemNode is { } stateWriteBackTreeItemNode)
        {
            SyncTreeItemNodeFromContainer(stateWriteBackTreeItemNode, change.Property);
        }
        
        if (change.Property == ItemCountProperty ||
            change.Property == HasTreeItemDataLoaderProperty)
        {
            ConfigureIsLeaf();
        }
        else if (change.Property == GroupNameProperty)
        {
            HandleGroupNameChanged(change);
        }
        else if (change.Property == IsCheckedProperty)
        {
            HandleIsCheckedChanged(change);
        }
        else if (change.Property == ToggleTypeProperty)
        {
            HandleToggleTypeChanged(change);
        }
        else if (change.Property == IsExpandedProperty || change.Property == IsMotionEnabledProperty)
        {
            HandleExpandedChanged();
            if (change.Property == IsExpandedProperty && !IsExpanded)
            {
                OwnerTreeView?.NotifyTreeViewItemCollapsed(this);
            }
        }
        else if (change.Property == IsKeyboardActiveProperty)
        {
            PseudoClasses.Set(TreeViewPseudoClass.KeyboardActive, IsKeyboardActive);
        }

        if (change.Property == IsSelectedProperty)
        {
            if (IsSelected && !IsSelectable)
            {
                SetCurrentValue(IsSelectedProperty, false);
                throw new InvalidOperationException("The IsSelectable property value is False, meaning selection is not supported.");
            }
        }

        if (change.Property == IsSelectableProperty)
        {
            if (!IsSelectable)
            {
                SetCurrentValue(IsSelectedProperty, false);
            }
        }
    }

    private void HandleIsSelectedChanged(RoutedEventArgs args)
    {
        if (OwnerTreeView?.SyncingSelectedItems == true)
        {
            args.Handled = true;
        }
    }

    private static bool CoerceIsSelected(AvaloniaObject sender, bool value)
    {
        if (!value &&
            sender is TreeViewItem treeViewItem &&
            treeViewItem.OwnerTreeView?.ShouldPreserveSelectedContainerDuringSelectedItemsSync(treeViewItem) == true)
        {
            return true;
        }
        return value;
    }

    private void HandleExpandedChanged(bool forceDisabledMotion = false)
    {
        var version = ++_childrenRequestVersion;
        if (_childrenViewport is not { } viewport || OwnerTreeView is not { } owner)
        {
            return;
        }

        // OwnerTreeView is assigned during runtime container preparation. The one-request
        // ExpandAll/initialization flag has no equivalent ControlTheme binding source.
        // Configure the request once; Motion owns the clock and the target lifecycle.
        Motion.SetEnter(viewport, ComponentMotionSelection.TryResolve(owner, TreeView.OpenMotionProperty,
            MotionSlot.InlineExpand, out var opening) ? opening : DefaultOpenMotion);
        Motion.SetLeave(viewport, ComponentMotionSelection.TryResolve(owner, TreeView.CloseMotionProperty,
            MotionSlot.InlineCollapse, out var closing) ? closing : DefaultCloseMotion);
        Motion.SetDurationOverride(viewport, owner.MotionDuration);
        var animate = IsMotionEnabled && !owner.IsExpandAllProcess && !forceDisabledMotion;
        Motion.SetIsEnabled(viewport, animate);
        Motion.SetIsVisible(viewport, IsExpanded);
        if (!animate)
        {
            // A deferred visibility request could otherwise play after the tree restores motion.
            Motion.SnapVisibility(viewport);
        }
        _ = CompleteExpansionAsync(version, viewport, owner, IsExpanded);
    }

    private async Task CompleteExpansionAsync(long version, ContentViewport viewport, TreeView owner, bool expanded)
    {
        var result = await Motion.GetVisibilityTask(viewport);
        if (version != _childrenRequestVersion || !ReferenceEquals(viewport, _childrenViewport) ||
            !ReferenceEquals(owner, OwnerTreeView) || IsExpanded != expanded ||
            !this.IsAttachedToVisualTree() || !result.IsCompleted)
        {
            return;
        }
        if (expanded && IsAutoExpandParent && Parent is TreeViewItem parentTreeItem)
        {
            parentTreeItem.SetCurrentValue(IsExpandedProperty, true);
        }
    }

    // Outside the container's own lifecycle a new ItemsSource means the node's children collection was replaced,
    // e.g. BindableTreeItemNode.Children assigned: the old children the new collection does not carry are gone.
    private void HandleItemsSourceChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (_containerLifecycleDepth == 0 &&
            OwnerTreeView is { } owner &&
            change.GetOldValue<IEnumerable?>() is { } oldChildren)
        {
            owner.RemoveCheckedItemsOfReplacedChildren(oldChildren, change.GetNewValue<IEnumerable?>());
        }
    }

    private void HandleGroupNameChanged(AvaloniaPropertyChangedEventArgs change)
    {
        OwnerTreeView?.OnGroupOrTypeChanged(this, change.GetOldValue<string>());
    }
    
    private void HandleToggleTypeChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newValue = change.GetNewValue<ItemToggleType>();
        PseudoClasses.Set(TreeViewPseudoClass.NodeToggleTypeRadio, newValue == ItemToggleType.Radio);
        PseudoClasses.Set(TreeViewPseudoClass.NodeToggleTypeCheckBox, newValue == ItemToggleType.CheckBox);

        OwnerTreeView?.OnGroupOrTypeChanged(this, GroupName);
    }
    
    private void HandleIsCheckedChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newValue = change.GetNewValue<bool?>();
        PseudoClasses.Set(StdPseudoClass.Checked, newValue == true);

        // Node data applied on prepare restores state, it is not a toggle. In checkbox mode the owner reconciles it
        // with CheckedItems afterwards; otherwise a container a virtualizing panel is still realizing has no item
        // index yet for the handler to resolve.
        if (_isApplyingNodeData &&
            (_isCheckedStateReconciledByOwner || (Parent as ItemsControl)?.ItemFromContainer(this) is null))
        {
            return;
        }

        OwnerTreeView?.OnCheckedChanged(this);
    }

    /// <summary>
    /// Calls the base method, lets the owning tree apply the initial expansion state, looks up the
    /// <see cref="TreeViewItemHeader" /> named <c>Header</c> and the <see cref="ContentViewport" /> named
    /// <c>PART_ChildrenViewport</c>, recomputes <see cref="IsLeaf" /> and brings the children viewport in line
    /// with the expanded state without motion.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ++_childrenRequestVersion;
        base.OnApplyTemplate(e);
        OwnerTreeView?.ApplyInitialExpansion(this);
        _decoration = e.NameScope.Find<TreeViewItemDecoration>("PART_Decoration");
        _header = e.NameScope.Find<TreeViewItemHeader>("Header");
        _childrenViewport = e.NameScope.Find<ContentViewport>("PART_ChildrenViewport");

        ConfigureIsLeaf();
        HandleExpandedChanged(true);
    }

    /// <summary>
    /// Shared expand entry for the switcher and keyboard: requests an async load for an
    /// unloaded data-loader node, otherwise toggles <see cref="AvaloniaTreeItem.IsExpanded" />.
    /// </summary>
    internal bool RequestToggleExpand()
    {
        if (IsLoading)
        {
            return false;
        }

        if (HasTreeItemDataLoader && !AsyncLoaded)
        {
            RaiseEvent(new TreeViewItemLoadRequestEventArgs(this)
            {
                RoutedEvent = NodeSwitcherButton.NodeLoadRequestEvent
            });
            return true;
        }

        SetCurrentValue(IsExpandedProperty, !IsExpanded);
        return true;
    }

    internal void BringHeaderIntoView()
    {
        if (_header is not null)
        {
            _header.BringIntoView();
        }
        else
        {
            this.BringIntoView();
        }
    }

    internal bool IsHeaderEffectivelyEnabled => _header?.IsEffectivelyEnabled ?? IsEffectivelyEnabled;

    internal bool IsExpandable() => ItemCount > 0 || (HasTreeItemDataLoader && !AsyncLoaded);

    internal bool IsEffectiveCheckable()
    {
        if (!IsEnabled || !IsIndicatorEnabled || ToggleType == ItemToggleType.None)
        {
            return false;
        }

        return true;
    }

    internal Rect GetDragBounds(bool includeChildren = false)
    {
        var dragOffset = _header?.GetDragIndicatorOffset(this) ?? default;
        var offsetX    = dragOffset.X;
        return new Rect(new Point(offsetX, 0),
            new Size(Bounds.Width - offsetX,
                includeChildren ? Bounds.Height : _header?.Bounds.Height ?? default));
    }

    internal bool IsInDragBounds(Point point)
    {
        return GetDragBounds(true).Contains(point);
    }

    internal bool IsInDragHeaderBounds(Point point)
    {
        return GetDragBounds().Contains(point);
    }

    internal bool IsDragOverForOffsetY(Point point)
    {
        if (OwnerTreeView is null)
        {
            return false;
        }

        return new Rect(Bounds.Size).Contains(point);
    }

    internal DragPreviewAdorner BuildPreviewAdorner()
    {
        return new DragPreviewAdorner(_header!);
    }

    /// <summary>
    /// Does nothing and does not call the base class, which skips the pointer-press handling of
    /// <see cref="Avalonia.Controls.TreeViewItem" />. Press handling is done by the header, which tracks the
    /// pressed state, and by the interaction handler of the owning tree.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
    }

    /// <summary>
    /// Does nothing and does not call the base class, so that key presses bubble to the owning tree, which owns
    /// keyboard navigation.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        // Keyboard navigation is owned by TreeView; let keys bubble without Avalonia's
        // focus-based item handling.
    }

    /// <summary>
    /// Calls the base method and then <see cref="NotifyHeaderClick" /> when the pointer is released inside the
    /// header.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if ((OwnerTreeView is null || OwnerTreeView.InteractionHandler is not null) && PointInHeaderBounds(e))
        {
            NotifyHeaderClick();
        }
    }
    
    internal bool PointInHeaderBounds(PointerReleasedEventArgs e)
    {
        var bounds = new Rect(new Point(0, 0), _header?.Bounds.Size ?? default);
        var point  = e.GetPosition(_header);
        return bounds.Contains(point);
    }
    
    /// <summary>
    /// Called when the mouse pointer is released inside the node header: by the default interaction handler for
    /// the left button, right before it raises <see cref="Click" />, and by this item from
    /// <see cref="OnPointerReleased" />. The default implementation does nothing, so an override does not need to
    /// call the base member.
    /// </summary>
    protected internal virtual void NotifyHeaderClick()
    {
    }
    
    internal void RaiseClick() => RaiseEvent(new RoutedEventArgs(ClickEvent));
    internal void RaiseContextMenuRequest() => RaiseEvent(new RoutedEventArgs
    {
        Source = this,
        RoutedEvent = ContextMenuRequestEvent
    });
    
    /// <summary>
    /// Creates the container for a child item: a new <see cref="TreeViewItem" /> that carries the semantic item
    /// class <see cref="TreeViewItemSemanticParts.ItemClass" />.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var treeViewItem = new TreeViewItem();
        treeViewItem.Classes.Add(TreeViewItemSemanticParts.ItemClass);
        return treeViewItem;
    }
    
    /// <summary>
    /// Decides whether a child item needs a container: an item that is itself a <see cref="TreeViewItem" /> is
    /// its own container, any other item gets one from <see cref="CreateContainerForItemOverride" />.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when a container must be created, <see langword="false" /> when the item is its own
    /// container.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TreeViewItem>(item, out recycleKey);
    }

    /// <summary>
    /// Prepares a child container after the base preparation: it adds the semantic item class, assigns
    /// <see cref="OwnerTreeView" />, applies the node data of an item that is an <see cref="ITreeItemNode" /> but
    /// not a control, forwards the header template and the per-node options of this item to the child, calls
    /// <see cref="PrepareTreeViewItem" /> and then tells the owning tree about the prepared container.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The container is not a <see cref="TreeViewItem" />.
    /// </exception>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is TreeViewItem treeViewItem)
        {
            treeViewItem.Classes.Add(TreeViewItemSemanticParts.ItemClass);
            treeViewItem.OwnerTreeView = OwnerTreeView;

            if (item != null && item is not Visual && item is ITreeItemNode treeViewItemData)
            {
                treeViewItem.PrepareTreeItemNodeData(treeViewItemData, this);
            }
            
            if (ItemTemplate != null)
            {
                treeViewItem[!HeaderTemplateProperty] = this[!ItemTemplateProperty];
            }
            treeViewItem[!FilterStrategyProperty]           = this[!FilterStrategyProperty];
            treeViewItem[!IsMotionEnabledProperty]           = this[!IsMotionEnabledProperty];
            treeViewItem[!NodeHoverModeProperty]             = this[!NodeHoverModeProperty];
            treeViewItem[!IsShowLineProperty]                = this[!IsShowLineProperty];
            treeViewItem[!IsShowIconProperty]                = this[!IsShowIconProperty];
            treeViewItem[!IsShowLeafIconProperty]            = this[!IsShowLeafIconProperty];
            treeViewItem[!IsSwitcherRotationProperty]        = this[!IsSwitcherRotationProperty];
            treeViewItem[!ToggleTypeProperty]                = this[!ToggleTypeProperty];
            treeViewItem[!IsSelectableProperty]              = this[!IsSelectableProperty];
            treeViewItem[!FilterHighlightForegroundProperty] = this[!FilterHighlightForegroundProperty];
            treeViewItem[!HasTreeItemDataLoaderProperty]     = this[!HasTreeItemDataLoaderProperty];
            treeViewItem[!IsAutoExpandParentProperty]        = this[!IsAutoExpandParentProperty];
            
            PrepareTreeViewItem(treeViewItem, item, index);
            OwnerTreeView?.NotifyTreeViewItemPrepared(treeViewItem, item);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TreeItem.");
        }
    }
    
    /// <summary>
    /// Hook for derived items, called from <see cref="ContainerForItemPreparedOverride" /> after a child container
    /// received the default preparation and before the owning tree is told about the container. The default
    /// implementation does nothing, so an override does not need to call the base member.
    /// </summary>
    protected virtual void PrepareTreeViewItem(TreeViewItem treeViewItem, object? item, int index)
    {
    }

    /// <summary>
    /// Clears a child container for reuse inside a container lifecycle scope: it releases the prepared node data,
    /// including the <c>ItemsSource</c> of a header bound to a <see cref="BindableTreeItemNode" />, and calls the
    /// base method. When the header is a <see cref="BindableTreeItemNode" />, a second prepare and clear pass with
    /// a <see langword="null" /> item follows, which makes the base class release its children binding.
    /// </summary>
    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is TreeViewItem treeViewItem)
        {
            using (treeViewItem.BeginContainerLifecycle())
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
            }
            return;
        }

        base.ClearContainerForItemOverride(container);
    }

    internal static void ApplyNodeData(TreeViewItem treeViewItem, ITreeItemNode treeItemData,
                                       bool isCheckedStateReconciledByOwner)
    {
        treeViewItem._isApplyingNodeData              = true;
        treeViewItem._isCheckedStateReconciledByOwner = isCheckedStateReconciledByOwner;
        try
        {
            treeViewItem.SetCurrentValue(IconProperty, treeItemData.Icon);
            treeViewItem.SetCurrentValue(IsCheckedProperty, treeItemData.IsChecked);
            treeViewItem.SetCurrentValue(IsSelectedProperty, treeItemData.IsSelected);
            treeViewItem.SetCurrentValue(IsEnabledProperty, treeItemData.IsEnabled);
            treeViewItem.SetCurrentValue(IsExpandedProperty, treeItemData.IsExpanded);
            treeViewItem.SetCurrentValue(IsIndicatorEnabledProperty, treeItemData.IsIndicatorEnabled);
            treeViewItem.SetCurrentValue(GroupNameProperty, treeItemData.GroupName);
            treeViewItem.SetCurrentValue(ValueProperty, treeItemData.Value);

            treeViewItem.ItemKey = treeItemData.ItemKey;
            treeViewItem.IsLeaf  = treeItemData.IsLeaf;
        }
        finally
        {
            treeViewItem._isApplyingNodeData              = false;
            treeViewItem._isCheckedStateReconciledByOwner = false;
        }
    }

    private void BindBindableTreeItemNode(BindableTreeItemNode node, CompositeDisposable disposables)
    {
        void NodePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            SyncContainerFromBindableTreeItemNode(node, e.Property);
        }

        void ContainerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            SyncTreeItemNodeFromContainer(node, e.Property);
        }

        node.PropertyChanged += NodePropertyChanged;
        PropertyChanged      += ContainerPropertyChanged;

        disposables.Add(Disposable.Create(() =>
        {
            node.PropertyChanged -= NodePropertyChanged;
            PropertyChanged      -= ContainerPropertyChanged;
        }));
    }

    private void SyncContainerFromBindableTreeItemNode(BindableTreeItemNode node, AvaloniaProperty property)
    {
        if (property == BindableTreeItemNode.IconProperty &&
            !Equals(Icon, node.Icon))
        {
            SetCurrentValue(IconProperty, node.Icon);
        }
        else if (property == BindableTreeItemNode.IsCheckedProperty &&
                 IsChecked != node.IsChecked)
        {
            SetCurrentValue(IsCheckedProperty, node.IsChecked);
        }
        else if (property == BindableTreeItemNode.IsSelectedProperty &&
                 IsSelected != node.IsSelected)
        {
            SetCurrentValue(IsSelectedProperty, node.IsSelected);
        }
        else if (property == BindableTreeItemNode.IsEnabledProperty &&
                 IsEnabled != node.IsEnabled)
        {
            SetCurrentValue(IsEnabledProperty, node.IsEnabled);
        }
        else if (property == BindableTreeItemNode.IsExpandedProperty &&
                 IsExpanded != node.IsExpanded)
        {
            SetCurrentValue(IsExpandedProperty, node.IsExpanded);
        }
        else if (property == BindableTreeItemNode.IsIndicatorEnabledProperty &&
                 IsIndicatorEnabled != node.IsIndicatorEnabled)
        {
            SetCurrentValue(IsIndicatorEnabledProperty, node.IsIndicatorEnabled);
        }
        else if (property == BindableTreeItemNode.GroupNameProperty &&
                 GroupName != node.GroupName)
        {
            SetCurrentValue(GroupNameProperty, node.GroupName);
        }
        else if (property == BindableTreeItemNode.ValueProperty &&
                 !Equals(Value, node.Value))
        {
            SetCurrentValue(ValueProperty, node.Value);
        }
        else if (property == BindableTreeItemNode.ItemKeyProperty &&
                 ItemKey != node.ItemKey)
        {
            ItemKey = node.ItemKey;
        }
        else if (property == BindableTreeItemNode.IsLeafProperty &&
                 IsLeaf != node.IsLeaf)
        {
            IsLeaf = node.IsLeaf;
        }
    }

    private void SyncTreeItemNodeFromContainer(ITreeItemNode node, AvaloniaProperty property)
    {
        if (property == IsCheckedProperty &&
            node.IsChecked != IsChecked)
        {
            node.IsChecked = IsChecked;
        }
        else if (property == IsSelectedProperty &&
                 node.IsSelected != IsSelected)
        {
            node.IsSelected = IsSelected;
        }
        else if (property == IsExpandedProperty &&
                 node.IsExpanded != IsExpanded)
        {
            node.IsExpanded = IsExpanded;
        }
    }

    private void ConfigureIsLeaf()
    {
        if (HasTreeItemDataLoader)
        {
            if (ItemCount > 0)
            {
                IsLeaf = false;
            }
            else if (AsyncLoaded)
            {
                IsLeaf = true;
            }
        }
        else
        {
            IsLeaf = ItemCount == 0;
        }
    }

    internal void CreateFilterContextBackup()
    {
        _filterContextBackup = new FilterContextBackup()
        {
            IsVisible = IsVisible,
        };
    }

    internal void ClearFilterMode()
    {
        if (_filterContextBackup != null)
        {
            SetCurrentValue(IsVisibleProperty, _filterContextBackup.IsVisible);
        }
        
        SetCurrentValue(IsExpandedProperty, OwnerTreeView?.SelectedItemsClosure.Contains(this) == true);
        IsFilterMatch        = false;
        IsFilterMode         = false;
        FilterHighlightWords = null;
        _filterContextBackup = null;
    }

    // The node's original context, saved so that it can be restored when the tree leaves filter mode.
    private record FilterContextBackup
    {
        public bool IsVisible { get; set; }
    }
}
