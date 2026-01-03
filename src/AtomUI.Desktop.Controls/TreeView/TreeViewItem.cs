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
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTreeItem = Avalonia.Controls.TreeViewItem;

[PseudoClasses(TreeViewPseudoClass.NodeToggleTypeCheckBox, TreeViewPseudoClass.NodeToggleTypeRadio)]
public class TreeViewItem : AvaloniaTreeItem, IRadioButton, ITreeViewItemData
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
    
    IList<ITreeViewItemData> ITreeNode<ITreeViewItemData>.Children => Items.OfType<ITreeViewItemData>().ToList();
    ITreeNode<ITreeViewItemData>? ITreeNode<ITreeViewItemData>.ParentNode => Parent as ITreeNode<ITreeViewItemData>;
    
    public TreeNodeKey? ItemKey { get; set; }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<MenuItem, RoutedEventArgs>(
            nameof(Click),
            RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
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
        AvaloniaProperty.Register<TreeViewItem, PathIcon?>(nameof(SwitcherRotationIcon));

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
    internal TreeView? OwnerTreeView { get; set; }

    private ITreeViewInteractionHandler? TreeViewInteractionHandler => this.FindLogicalAncestorOfType<TreeView>()?.InteractionHandler;
    
    #endregion
    
    private bool _animating;
    private BaseMotionActor? _itemsPresenterMotionActor;
    private readonly BorderRenderHelper _borderRenderHelper;
    private TreeViewItemHeader? _header;
    private readonly Dictionary<TreeViewItem, CompositeDisposable> _itemsBindingDisposables = new();
    internal bool AsyncLoaded;

    static TreeViewItem()
    {
        AffectsRender<TreeViewItem>(
            IsShowLineProperty,
            IsShowLeafIconProperty,
            IsDraggingProperty,
            IsDragOverProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            NodeHoverModeProperty,
            BackgroundProperty);
    }

    public TreeViewItem()
    {
        _borderRenderHelper               =  new BorderRenderHelper();
        LogicalChildren.CollectionChanged += HandleItemsCollectionChanged;
    }
    
    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is TreeViewItem treeViewItem)
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

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsExpandedProperty)
            {
                HandleExpandedChanged();
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
            OwnerTreeView is null)
        {
            return;
        }

        if (!IsMotionEnabled || OwnerTreeView.IsExpandAllProcess)
        {
            _itemsPresenterMotionActor.Opacity   = 1.0;
            _itemsPresenterMotionActor.IsVisible = true;
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
            });
    }

    private void CollapseChildren()
    {
        if (_itemsPresenterMotionActor is null ||
            _animating ||
            OwnerTreeView is null)
        {
            return;
        }

        if (!IsMotionEnabled || OwnerTreeView.IsExpandAllProcess)
        {
            _itemsPresenterMotionActor.Opacity   = 0.0;
            _itemsPresenterMotionActor.IsVisible = false;
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
        var bounds = new Rect(new Point(0, 0), _header?.DesiredSize ?? default);
        var point  = e.GetPosition(_header);
        return bounds.Contains(point);
    }
    
    protected virtual void NotifyHeaderClick()
    {
    }
    
    internal void RaiseClick() => RaiseEvent(new RoutedEventArgs(ClickEvent));
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
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
            treeViewItem.OwnerTreeView = OwnerTreeView;
            var disposables = new CompositeDisposable(8);
            
            if (item != null && item is not Visual && item is ITreeViewItemData treeViewItemData)
            {
                ApplyNodeData(treeViewItem, treeViewItemData);
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, treeViewItem, HeaderTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, treeViewItem, IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, NodeHoverModeProperty, treeViewItem, NodeHoverModeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowLineProperty, treeViewItem, IsShowLineProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowIconProperty, treeViewItem, IsShowIconProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowLeafIconProperty, treeViewItem,
                    IsShowLeafIconProperty));
            disposables.Add(BindUtils.RelayBind(this, IsSwitcherRotationProperty, treeViewItem, IsSwitcherRotationProperty));
            disposables.Add(BindUtils.RelayBind(this, ToggleTypeProperty, treeViewItem, ToggleTypeProperty));
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
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TreeViewItem.");
        }
    }
    
    protected virtual void PrepareTreeViewItem(TreeViewItem treeViewItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    internal static void ApplyNodeData(TreeViewItem treeViewItem, ITreeViewItemData treeViewItemData)
    {
        if (treeViewItemData is not Visual)
        {
            if (!treeViewItem.IsSet(HeaderProperty))
            {
                treeViewItem.SetCurrentValue(HeaderProperty, treeViewItem);
            }
                    
            if (!treeViewItem.IsSet(IconProperty))
            {
                treeViewItem.SetCurrentValue(IconProperty, treeViewItemData.Icon);
            }
                    
            if (treeViewItem.ItemKey == null)
            {
                treeViewItem.ItemKey = treeViewItemData.ItemKey;
            }
                    
            if (!treeViewItem.IsSet(IsCheckedProperty))
            {
                treeViewItem.SetCurrentValue(IsCheckedProperty, treeViewItemData.IsChecked);
            }
                    
            if (!treeViewItem.IsSet(IsSelectedProperty))
            {
                treeViewItem.SetCurrentValue(IsSelectedProperty, treeViewItemData.IsSelected);
            }
                    
            if (!treeViewItem.IsSet(IsEnabledProperty))
            {
                treeViewItem.SetCurrentValue(IsEnabledProperty, treeViewItemData.IsEnabled);
            }
                    
            if (!treeViewItem.IsSet(IsExpandedProperty))
            {
                treeViewItem.SetCurrentValue(IsExpandedProperty, treeViewItemData.IsExpanded);
            }
            if (!treeViewItem.IsSet(IsIndicatorEnabledProperty))
            {
                treeViewItem.SetCurrentValue(IsIndicatorEnabledProperty, treeViewItemData.IsIndicatorEnabled);
            }
            
            if (!treeViewItem.IsSet(IsLeafProperty))
            {
                treeViewItem.IsLeaf = treeViewItemData.IsLeaf;
            }
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
}