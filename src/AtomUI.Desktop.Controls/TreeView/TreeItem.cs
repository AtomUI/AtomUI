using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.MotionScene;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using AvaloniaTreeItem = Avalonia.Controls.TreeViewItem;

[PseudoClasses(TreeViewPseudoClass.NodeToggleTypeCheckBox, TreeViewPseudoClass.NodeToggleTypeRadio)]
public class TreeItem : AvaloniaTreeItem, IRadioButton, ITreeItemNode
{
    #region 公共属性定义
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<TreeItem, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<bool?> IsCheckedProperty =
        AvaloniaProperty.Register<TreeItem, bool?>(nameof(IsChecked), false);

    public static readonly DirectProperty<TreeItem, bool> IsLeafProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(nameof(IsLeaf),
            o => o.IsLeaf);

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<TreeItem, bool>(nameof(IsLoading), false);
    
    public static readonly StyledProperty<string?> GroupNameProperty =
        RadioButton.GroupNameProperty.AddOwner<TreeItem>();
    
    public static readonly DirectProperty<TreeItem, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, object?>(nameof(Value),
            o => o.Value,
           (o, v) => o.Value = v);
    
    public static readonly StyledProperty<bool> IsIndicatorEnabledProperty =
        AvaloniaProperty.Register<TreeItem, bool>(nameof(IsIndicatorEnabled), true);

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
    
    IList<ITreeItemNode> ITreeNode<ITreeItemNode>.Children => Items.OfType<ITreeItemNode>().ToList();
    ITreeNode<ITreeItemNode>? ITreeNode<ITreeItemNode>.ParentNode => Parent as ITreeNode<ITreeItemNode>;
    
    public TreeNodeKey? ItemKey { get; set; }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<TreeItem, RoutedEventArgs>(
            nameof(Click),
            RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> ContextMenuRequestEvent =
        RoutedEvent.Register<TreeItem, RoutedEventArgs>(nameof(ContextMenuRequest), RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    
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
        AvaloniaProperty.Register<TreeItem, PathIcon?>(nameof(SwitcherExpandIcon));

    internal static readonly StyledProperty<PathIcon?> SwitcherCollapseIconProperty =
        AvaloniaProperty.Register<TreeItem, PathIcon?>(nameof(SwitcherCollapseIcon));

    internal static readonly StyledProperty<PathIcon?> SwitcherRotationIconProperty =
        AvaloniaProperty.Register<TreeItem, PathIcon?>(nameof(SwitcherRotationIcon));

    internal static readonly StyledProperty<PathIcon?> SwitcherLoadingIconProperty =
        AvaloniaProperty.Register<TreeItem, PathIcon?>(nameof(SwitcherLoadingIcon));

    internal static readonly StyledProperty<PathIcon?> SwitcherLeafIconProperty =
        AvaloniaProperty.Register<TreeItem, PathIcon?>(nameof(SwitcherLeafIcon));

    internal static readonly DirectProperty<TreeItem, TreeItemHoverMode> NodeHoverModeProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, TreeItemHoverMode>(nameof(NodeHoverMode),
            o => o.NodeHoverMode,
            (o, v) => o.NodeHoverMode = v);

    internal static readonly DirectProperty<TreeItem, bool> IsShowLineProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(nameof(IsShowLine),
            o => o.IsShowLine,
            (o, v) => o.IsShowLine = v);

    internal static readonly DirectProperty<TreeItem, bool> IsShowIconProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(nameof(IsShowIcon),
            o => o.IsShowIcon,
            (o, v) => o.IsShowIcon = v);

    internal static readonly DirectProperty<TreeItem, bool> IsDraggingProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(nameof(IsDragging),
            o => o.IsDragging,
            (o, v) => o.IsDragging = v);

    internal static readonly DirectProperty<TreeItem, bool> IsDragOverProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(nameof(IsDragOver),
            o => o.IsDragOver,
            (o, v) => o.IsDragOver = v);

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<TreeItem>();

    internal static readonly DirectProperty<TreeItem, bool> IsSwitcherRotationProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(
            nameof(IsSwitcherRotation),
            o => o.IsSwitcherRotation,
            (o, v) => o.IsSwitcherRotation = v);
    
    internal static readonly StyledProperty<ItemToggleType> ToggleTypeProperty =
        TreeView.ToggleTypeProperty.AddOwner<TreeItem>();
    
    internal static readonly StyledProperty<bool> IsShowLeafIconProperty =
        AvaloniaProperty.Register<TreeItem, bool>(nameof(IsShowLeafIcon));
    
    internal static readonly DirectProperty<TreeItem, bool> HasTreeItemDataLoaderProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(nameof(HasTreeItemDataLoader),
            o => o.HasTreeItemDataLoader,
            (o, v) => o.HasTreeItemDataLoader = v);
    
    internal static readonly DirectProperty<TreeItem, bool> IsAutoExpandParentProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(nameof(IsAutoExpandParent),
            o => o.IsAutoExpandParent,
            (o, v) => o.IsAutoExpandParent = v);
    
    internal static readonly DirectProperty<TreeItem, bool> IsFilterModeProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(nameof(IsFilterMode),
            o => o.IsFilterMode,
            (o, v) => o.IsFilterMode = v);
    
    internal static readonly DirectProperty<TreeItem, bool> IsFilterMatchProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(nameof(IsFilterMatch),
            o => o.IsFilterMatch,
            (o, v) => o.IsFilterMatch = v);
    
    internal static readonly DirectProperty<TreeItem, string?> FilterHighlightWordsProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, string?>(nameof(FilterHighlightWords),
            o => o.FilterHighlightWords,
            (o, v) => o.FilterHighlightWords = v);

    internal static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        TreeView.FilterHighlightForegroundProperty.AddOwner<TreeItem>();
    
    internal static readonly DirectProperty<TreeItem, TreeFilterHighlightStrategy> FilterHighlightStrategyProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, TreeFilterHighlightStrategy>(
            nameof(FilterHighlightStrategy),
            o => o.FilterHighlightStrategy,
            (o, v) => o.FilterHighlightStrategy = v);
    
    internal static readonly DirectProperty<TreeItem, bool> IsSelectableProperty =
        AvaloniaProperty.RegisterDirect<TreeItem, bool>(
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
    
    private TreeFilterHighlightStrategy _filterHighlightStrategy = TreeFilterHighlightStrategy.All;
    
    internal TreeFilterHighlightStrategy FilterHighlightStrategy
    {
        get => _filterHighlightStrategy;
        set => SetAndRaise(FilterHighlightStrategyProperty, ref _filterHighlightStrategy, value);
    }

    private bool _isSelectable = true;
    
    internal bool IsSelectable
    {
        get => _isSelectable;
        set => SetAndRaise(IsSelectableProperty, ref _isSelectable, value);
    }
    
    internal TreeView? OwnerTreeView { get; set; }

    private ITreeViewInteractionHandler? TreeViewInteractionHandler => this.FindLogicalAncestorOfType<TreeView>()?.InteractionHandler;
    
    #endregion
    
    private bool _animating;
    private bool _isRealExpanded;
    private BaseMotionActor? _itemsPresenterMotionActor;
    private readonly BorderRenderHelper _borderRenderHelper;
    private TreeViewItemHeader? _header;
    private readonly Dictionary<TreeItem, CompositeDisposable> _itemsBindingDisposables = new();
    internal bool AsyncLoaded;
    private FilterContextBackup? _filterContextBackup;

    static TreeItem()
    {
        AffectsRender<TreeItem>(
            IsShowLineProperty,
            IsShowLeafIconProperty,
            IsDraggingProperty,
            IsDragOverProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            NodeHoverModeProperty,
            BackgroundProperty);
    }

    public TreeItem()
    {
        _borderRenderHelper               =  new BorderRenderHelper();
        LogicalChildren.CollectionChanged += HandleLogicalChildrenChanged;
        Items.CollectionChanged           += HandleCollectionChanged;
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
                foreach (var i in e.OldItems!)
                {
                    OwnerTreeView.CheckedItems.Remove(i);
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                OwnerTreeView.CheckedItems.Clear();
                break;
        }
    }
    
    private void HandleLogicalChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is TreeItem treeViewItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(treeViewItem, out var disposable))
                        {
                            disposable.Dispose();
                        }
                        _itemsBindingDisposables.Remove(treeViewItem);
                    }
                }
            }
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OwnerTreeView = this.GetLogicalAncestors().OfType<TreeView>().FirstOrDefault();
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
        else if (change.Property == IsExpandedProperty)
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

    private void HandleExpandedChanged()
    {
        if (IsExpanded)
        {
            ExpandChildren();
        }
        else
        {
            CollapseChildren();
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

    private void ExpandChildren()
    {
        if (_itemsPresenterMotionActor is null ||
            _animating ||
            OwnerTreeView is null ||
            _isRealExpanded)
        {
            return;
        }

        if (!IsMotionEnabled || OwnerTreeView.IsExpandAllProcess)
        {
            _itemsPresenterMotionActor.Opacity   = 1.0;
            _itemsPresenterMotionActor.IsVisible = true;
            _isRealExpanded                      = true;
            return;
        }

        _animating = true;
        _header?.NotifyAnimating(true);

        var motion = OwnerTreeView.OpenMotion ?? new ExpandMotion(Direction.Top, null, new CubicEaseOut());
        motion.Duration = OwnerTreeView.MotionDuration;

        motion.Run(_itemsPresenterMotionActor, () => { _itemsPresenterMotionActor.IsVisible = true; },
            () =>
            {
                _animating = false;
                _header?.NotifyAnimating(false);
                _isRealExpanded = true;
                if (IsAutoExpandParent)
                {
                    if (Parent is TreeItem parentTreeItem)
                    {
                        parentTreeItem.SetCurrentValue(IsExpandedProperty, true);
                    }
                }
            });
    }

    private void CollapseChildren()
    {
        if (_itemsPresenterMotionActor is null ||
            _animating ||
            OwnerTreeView is null ||
            !_isRealExpanded)
        {
            return;
        }

        if (!IsMotionEnabled || OwnerTreeView.IsExpandAllProcess)
        {
            _itemsPresenterMotionActor.Opacity   = 0.0;
            _itemsPresenterMotionActor.IsVisible = false;
            _isRealExpanded                      = false;
            return;
        }

        _animating = true;
        _header?.NotifyAnimating(true);

        var motion = OwnerTreeView.CloseMotion ?? new CollapseMotion(Direction.Top, null, new CubicEaseIn());
        motion.Duration = OwnerTreeView.MotionDuration;

        motion.Run(_itemsPresenterMotionActor, null, () =>
        {
            _itemsPresenterMotionActor.IsVisible = false;
            _animating                           = false;
            _header?.NotifyAnimating(false);
            _isRealExpanded = false;
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _header             = e.NameScope.Find<TreeViewItemHeader>(TreeViewItemThemeConstants.HeaderPart);
        _itemsPresenterMotionActor =
            e.NameScope.Find<BaseMotionActor>(TreeViewItemThemeConstants.ItemsPresenterMotionActorPart);
        
        ConfigureIsLeaf();
        
        var originIsMotionEnabled = IsMotionEnabled;
        try
        {
            SetCurrentValue(IsMotionEnabledProperty, false);
            HandleExpandedChanged();
        }
        finally
        {
            SetCurrentValue(IsMotionEnabledProperty, originIsMotionEnabled);
        }
        
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

        var penWidth = BorderThickness.Top;
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
        
        if (!IsLeaf && !isLastChild && _itemsPresenterMotionActor?.IsVisible == true)
        {
            var switcherMiddleBottom = new Point(switcherButtonRect.X + switcherButtonRect.Width / 2, switcherButtonRect.Bottom);
            var blockStartPoint = new Point(switcherMiddleBottom.X, switcherMiddleBottom.Y);
            var blockEndPoint   = new Point(blockStartPoint.X, DesiredSize.Height);
            context.DrawLine(new Pen(BorderBrush, penWidth), blockStartPoint, blockEndPoint);
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

                context.DrawLine(new Pen(BorderBrush, penWidth), childStartPoint.WithY(0), childEndPoint);
            }

            {
                // 横向
                var childStartPoint =  new Point(switcherButtonRect.X + switcherButtonRect.Width / 2 - penWidth / 2, switcherButtonRect.Top +  switcherButtonRect.Height / 2 - penWidth / 2);
                var childEndPoint = new Point(switcherButtonRect.Right, switcherButtonRect.Top +  switcherButtonRect.Height / 2 - penWidth / 2);

                context.DrawLine(new Pen(BorderBrush, penWidth), childStartPoint, childEndPoint);
            }
        }
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
    
    protected virtual void NotifyHeaderClick()
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
        return new TreeItem();
    }
    
    protected override bool NeedsContainerOverride(
        object? item,
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
            treeViewItem.OwnerTreeView = OwnerTreeView;
            var disposables = new CompositeDisposable(8);
            
            if (item != null && item is not Visual && item is ITreeItemNode treeViewItemData)
            {
                ApplyNodeData(treeViewItem, treeViewItemData, disposables);
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, treeViewItem, HeaderTemplateProperty));
            }
            disposables.Add(BindUtils.RelayBind(this, FilterHighlightStrategyProperty, treeViewItem, FilterHighlightStrategyProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, treeViewItem, IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, NodeHoverModeProperty, treeViewItem, NodeHoverModeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowLineProperty, treeViewItem, IsShowLineProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowIconProperty, treeViewItem, IsShowIconProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowLeafIconProperty, treeViewItem,
                    IsShowLeafIconProperty));
            disposables.Add(BindUtils.RelayBind(this, IsSwitcherRotationProperty, treeViewItem, IsSwitcherRotationProperty));
            disposables.Add(BindUtils.RelayBind(this, ToggleTypeProperty, treeViewItem, ToggleTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsSelectableProperty, treeViewItem, IsSelectableProperty));
            disposables.Add(BindUtils.RelayBind(this, FilterHighlightForegroundProperty, treeViewItem, FilterHighlightForegroundProperty));
            disposables.Add(BindUtils.RelayBind(this, HasTreeItemDataLoaderProperty, treeViewItem,
                HasTreeItemDataLoaderProperty));
            disposables.Add(BindUtils.RelayBind(this, IsAutoExpandParentProperty, treeViewItem,
                IsAutoExpandParentProperty));
            
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
    
    protected virtual void PrepareTreeViewItem(TreeItem treeItem, object? item, int index, CompositeDisposable disposables)
    {
    }

    internal static void ApplyNodeData(TreeItem treeItem, ITreeItemNode treeItemData, CompositeDisposable disposables)
    {
        treeItem.SetCurrentValue(IconProperty, treeItemData.Icon);
        treeItem.SetCurrentValue(IsCheckedProperty, treeItemData.IsChecked);
        treeItem.SetCurrentValue(IsSelectedProperty, treeItemData.IsSelected);
        treeItem.SetCurrentValue(IsEnabledProperty, treeItemData.IsEnabled);
        treeItem.SetCurrentValue(IsExpandedProperty, treeItemData.IsExpanded);
        treeItem.SetCurrentValue(IsIndicatorEnabledProperty, treeItemData.IsIndicatorEnabled);
        
        treeItem.ItemKey = treeItemData.ItemKey;
        treeItem.IsLeaf  = treeItemData.IsLeaf;
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
        
        SetCurrentValue(IsExpandedProperty, OwnerTreeView?.SelectedItemsClosure.Contains(this));
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