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

namespace AtomUI.Desktop.Controls;

using AvaloniaTreeItem = Avalonia.Controls.TreeViewItem;

[PseudoClasses(TreeViewPseudoClass.NodeToggleTypeCheckBox, TreeViewPseudoClass.NodeToggleTypeRadio)]
public partial class TreeViewItem : AvaloniaTreeItem, IRadioButton, ITreeItemNode
{
    #region 公共属性定义
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<TreeViewItem, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<bool?> IsCheckedProperty =
        AvaloniaProperty.Register<TreeViewItem, bool?>(nameof(IsChecked), false);

    public static readonly DirectProperty<TreeViewItem, bool> IsLeafProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, bool>(nameof(IsLeaf),
            o => o.IsLeaf);

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<TreeViewItem, bool>(nameof(IsLoading), false);
    
    public static readonly StyledProperty<string?> GroupNameProperty =
        RadioButton.GroupNameProperty.AddOwner<TreeViewItem>();
    
    public static readonly DirectProperty<TreeViewItem, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<TreeViewItem, object?>(nameof(Value),
            o => o.Value,
           (o, v) => o.Value = v);
    
    public static readonly StyledProperty<bool> IsIndicatorEnabledProperty =
        AvaloniaProperty.Register<TreeViewItem, bool>(nameof(IsIndicatorEnabled), true);

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    private bool _isLeaf;

    public bool IsLeaf
    {
        get => _isLeaf;
        internal set => SetAndRaise(IsLeafProperty, ref _isLeaf, value);
    }
    
    private object? _value;

    public object? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string? GroupName
    {
        get => GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }
    
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

    public EntityKey? ItemKey { get; set; }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<TreeViewItem, RoutedEventArgs>(
            nameof(Click),
            RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> ContextMenuRequestEvent =
        RoutedEvent.Register<TreeViewItem, RoutedEventArgs>(nameof(ContextMenuRequest), RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }
    
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

    private ITreeViewInteractionHandler? TreeViewInteractionHandler => OwnerTreeView?.InteractionHandler;
    
    #endregion
    
    private static readonly AbstractMotion DefaultOpenMotion = new ExpandMotion(Direction.Top, easing: new CubicEaseOut());
    private static readonly AbstractMotion DefaultCloseMotion = new CollapseMotion(Direction.Top, easing: new CubicEaseIn());
    private ContentViewport? _childrenViewport;
    private long _childrenRequestVersion;
    private readonly BorderRenderHelper _borderRenderHelper;
    private TreeViewItemHeader? _header;
    internal bool AsyncLoaded;
    private FilterContextBackup? _filterContextBackup;
    private Pen? _treeNodeLinePen;
    private IBrush? _treeNodeLinePenBrush;
    private double _treeNodeLinePenWidth;
    private CompositeDisposable? _treeItemNodeBindingDisposables;

    static TreeViewItem()
    {
        AffectsRender<TreeViewItem>(
            IsShowLineProperty,
            IsShowLeafIconProperty,
            IsDraggingProperty,
            IsDragOverProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            UseLayoutRoundingProperty,
            NodeHoverModeProperty,
            BackgroundProperty);
        IsSelectedProperty.OverrideMetadata<TreeViewItem>(
            new StyledPropertyMetadata<bool>(coerce: CoerceIsSelected));
        SelectingItemsControl.IsSelectedChangedEvent.AddClassHandler<TreeViewItem>(
            (treeViewItem, args) => treeViewItem.HandleIsSelectedChanged(args));
    }

    public TreeViewItem()
    {
        _borderRenderHelper     =  new BorderRenderHelper();
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
        if (!this.GetLogicalAncestors().OfType<TreeView>().Any())
        {
            return;
        }

        ClearTreeItemNodeBindingDisposables();

        if (treeItemData is BindableTreeItemNode bindableTreeItemNode)
        {
            var disposables = new CompositeDisposable();
            _treeItemNodeBindingDisposables = disposables;
            disposables.Add(bindableTreeItemNode.AttachResourceHost(resourceHost));

            ApplyNodeData(this, treeItemData);
            BindBindableTreeItemNode(bindableTreeItemNode, disposables);
        }
        else
        {
            ApplyNodeData(this, treeItemData);
        }
    }

    internal void ClearTreeItemNodeBindingDisposables()
    {
        _treeItemNodeBindingDisposables?.Dispose();
        _treeItemNodeBindingDisposables = null;
    }

    internal void ClearPreparedTreeItemNodeData()
    {
        ClearTreeItemNodeBindingDisposables();
        // Only template-bound BindableTreeItemNode containers may drop ItemsSource here: the
        // resulting ItemsView reset synchronizes removed children out of TreeView.CheckedItems
        // and SelectedItems, which would destroy state that outlives the container otherwise.
        if (Header is BindableTreeItemNode)
        {
            ClearValue(ItemsSourceProperty);
        }
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (OwnerTreeView is null)
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
                    if (!OwnerTreeView.ShouldPreserveRemovedTreeItem(oldItems[i]))
                    {
                        OwnerTreeView.CheckedItems.Remove(oldItems[i]);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                OwnerTreeView.CheckedItems.Clear();
                break;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ++_childrenRequestVersion;
        base.OnDetachedFromVisualTree(e);
        OwnerTreeView = null;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OwnerTreeView = this.FindAncestorOfType<TreeView>();
        HandleExpandedChanged(true);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

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
            base.PrepareContainerForItemOverride(this, bindableTreeItemNode, -1);
            var resourceHost = this.GetLogicalAncestors().OfType<TreeViewItem>().FirstOrDefault() as IResourceHost ?? treeView;
            PrepareTreeItemNodeData(bindableTreeItemNode, resourceHost);
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ClearPreparedTreeItemNodeData();
        base.OnDetachedFromLogicalTree(e);
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
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
        Motion.SetEnter(viewport, owner.OpenMotion ?? DefaultOpenMotion);
        Motion.SetLeave(viewport, owner.CloseMotion ?? DefaultCloseMotion);
        Motion.SetDurationOverride(viewport, owner.MotionDuration);
        Motion.SetIsEnabled(viewport, IsMotionEnabled && !owner.IsExpandAllProcess && !forceDisabledMotion);
        Motion.SetIsVisible(viewport, IsExpanded);
        if (forceDisabledMotion)
        {
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

    private void HandleGroupNameChanged(AvaloniaPropertyChangedEventArgs change)
    {
        (TreeViewInteractionHandler as DefaultTreeViewInteractionHandler)?.OnGroupOrTypeChanged(this, change.GetOldValue<string>());
    }
    
    private void HandleToggleTypeChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newValue = change.GetNewValue<ItemToggleType>();
        PseudoClasses.Set(TreeViewPseudoClass.NodeToggleTypeRadio, newValue == ItemToggleType.Radio);
        PseudoClasses.Set(TreeViewPseudoClass.NodeToggleTypeCheckBox, newValue == ItemToggleType.CheckBox);

        (TreeViewInteractionHandler as DefaultTreeViewInteractionHandler)?.OnGroupOrTypeChanged(this, GroupName);
    }
    
    private void HandleIsCheckedChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newValue = change.GetNewValue<bool?>();
        PseudoClasses.Set(StdPseudoClass.Checked, newValue == true);

        (TreeViewInteractionHandler as DefaultTreeViewInteractionHandler)?.OnCheckedChanged(this);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ++_childrenRequestVersion;
        base.OnApplyTemplate(e);
        _header = e.NameScope.Find<TreeViewItemHeader>("Header");
        _childrenViewport = e.NameScope.Find<ContentViewport>("PART_ChildrenViewport");

        ConfigureIsLeaf();
        HandleExpandedChanged(true);
    }

    internal bool IsEffectiveCheckable()
    {
        if (!IsEnabled || !IsIndicatorEnabled || ToggleType == ItemToggleType.None)
        {
            return false;
        }

        return true;
    }

    public override void Render(DrawingContext context)
    {
        if (NodeHoverMode == TreeItemHoverMode.WholeLine && _header != null)
        {
            using var state = context.PushTransform(Matrix.CreateTranslation(_header.Bounds.X, 0));
            _borderRenderHelper.Render(context,
                _header.Bounds.Size,
                new Thickness(),
                CornerRadius,
                BackgroundSizing.InnerBorderEdge,
                Background,
                null);
        }
   
        if (IsShowLine)
        {
            RenderTreeNodeLine(context);
        }
    }

    private void RenderTreeNodeLine(DrawingContext context)
    {
        if (_header is null)
        {
            return;
        }

        var penWidth = BorderUtils.BuildRenderScaleAwareThickness(this, BorderThickness.Top);
        var linePen  = GetTreeNodeLinePen(penWidth);
        using var state = context.PushRenderOptions(new RenderOptions
        {
            EdgeMode = EdgeMode.Aliased
        });
        
        var isLastChild = false;
        if (Parent is ItemsControl parentTreeItem)
        {
            if (parentTreeItem.ContainerFromIndex(parentTreeItem.ItemCount - 1) == this)
            {
                isLastChild = true;
            }
        }

        var switcherButtonRect = _header.SwitcherButtonRect(this);
        
        if (!IsLeaf && !isLastChild && _childrenViewport?.IsVisible == true)
        {
            var switcherMiddleBottom = new Point(switcherButtonRect.X + switcherButtonRect.Width / 2, switcherButtonRect.Bottom);
            var blockStartPoint = new Point(switcherMiddleBottom.X, switcherMiddleBottom.Y);
            var blockEndPoint   = new Point(blockStartPoint.X, DesiredSize.Height);
            context.DrawLine(linePen, blockStartPoint, blockEndPoint);
        }

        // 画孩子线条
        if (!IsShowLeafIcon && IsLeaf)
        {
            {
                // 纵向
                var childStartPoint = new Point(switcherButtonRect.X + switcherButtonRect.Width / 2, switcherButtonRect.Top);
                var childEndPoint = new Point(switcherButtonRect.X + switcherButtonRect.Width / 2, isLastChild ? switcherButtonRect.Bottom : switcherButtonRect.Top + DesiredSize.Height);
                if (isLastChild)
                {
                    childEndPoint = childEndPoint.WithY(childEndPoint.Y / 2);
                }

                context.DrawLine(linePen, childStartPoint.WithY(0), childEndPoint);
            }

            {
                // 横向
                var childStartPoint =  new Point(switcherButtonRect.X + switcherButtonRect.Width / 2 - penWidth / 2, switcherButtonRect.Top +  switcherButtonRect.Height / 2 - penWidth / 2);
                var childEndPoint = new Point(switcherButtonRect.Right, switcherButtonRect.Top +  switcherButtonRect.Height / 2 - penWidth / 2);

                context.DrawLine(linePen, childStartPoint, childEndPoint);
            }
        }
    }

    private Pen GetTreeNodeLinePen(double penWidth)
    {
        if (_treeNodeLinePen is null ||
            !ReferenceEquals(_treeNodeLinePenBrush, BorderBrush) ||
            !MathUtils.AreClose(_treeNodeLinePenWidth, penWidth))
        {
            _treeNodeLinePenBrush = BorderBrush;
            _treeNodeLinePenWidth = penWidth;
            _treeNodeLinePen      = new Pen(BorderBrush, penWidth);
        }

        return _treeNodeLinePen!;
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

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (PointInHeaderBounds(e))
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
    
    protected internal virtual void NotifyHeaderClick()
    {
    }
    
    internal void RaiseClick() => RaiseEvent(new RoutedEventArgs(ClickEvent));
    internal void RaiseContextMenuRequest() => RaiseEvent(new RoutedEventArgs
    {
        Source = this,
        RoutedEvent = ContextMenuRequestEvent
    });
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var treeViewItem = new TreeViewItem();
        treeViewItem.Classes.Add(TreeViewItemSemanticParts.ItemClass);
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
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TreeItem.");
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

    internal static void ApplyNodeData(TreeViewItem treeViewItem, ITreeItemNode treeItemData)
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

    private void BindBindableTreeItemNode(BindableTreeItemNode node, CompositeDisposable disposables)
    {
        void NodePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            SyncContainerFromBindableTreeItemNode(node, e.Property);
        }

        void ContainerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            SyncBindableTreeItemNodeFromContainer(node, e.Property);
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

    private void SyncBindableTreeItemNodeFromContainer(BindableTreeItemNode node, AvaloniaProperty property)
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

    // 用户保存在树处于过滤状态结束后的节点原始上下文属性的恢复
    private record FilterContextBackup
    {
        public bool IsVisible { get; set; }
    }
}
