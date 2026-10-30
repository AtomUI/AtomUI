using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal enum NodeSwitcherButtonIconMode
{
    Default,
    Rotation,
    Leaf,
    Loading
}

internal class NodeSwitcherButton : ToggleButton
{
    #region 公共属性定义
    
    public static readonly StyledProperty<PathIcon?> ExpandIconProperty = 
        AvaloniaProperty.Register<NodeSwitcherButton, PathIcon?>(nameof(ExpandIcon));

    public static readonly StyledProperty<PathIcon?> CollapseIconProperty = 
        AvaloniaProperty.Register<NodeSwitcherButton, PathIcon?>(nameof(CollapseIcon));

    public static readonly StyledProperty<PathIcon?> LoadingIconProperty = 
        AvaloniaProperty.Register<NodeSwitcherButton, PathIcon?>(nameof(LoadingIcon));

    public static readonly StyledProperty<PathIcon?> LeafIconProperty =
        AvaloniaProperty.Register<NodeSwitcherButton, PathIcon?>(nameof(LeafIcon));
    
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<NodeSwitcherButton, bool>(nameof(IsLoading), false);
    
    public static readonly StyledProperty<PathIcon?> RotationIconProperty =
        AvaloniaProperty.Register<NodeSwitcherButton, PathIcon?>(nameof(RotationIcon));

    public PathIcon? LoadingIcon
    {
        get => GetValue(LoadingIconProperty);
        set => SetValue(LoadingIconProperty, value);
    }

    public PathIcon? LeafIcon
    {
        get => GetValue(LeafIconProperty);
        set => SetValue(LeafIconProperty, value);
    }
    
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    
    public PathIcon? RotationIcon
    {
        get => GetValue(RotationIconProperty);
        set => SetValue(RotationIconProperty, value);
    }
    
    public PathIcon? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }

    public PathIcon? CollapseIcon
    {
        get => GetValue(CollapseIconProperty);
        set => SetValue(CollapseIconProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<TreeViewItemLoadRequestEventArgs> NodeLoadRequestEvent =
        RoutedEvent.Register<NodeSwitcherButton, TreeViewItemLoadRequestEventArgs>(nameof(NodeLoadRequest), RoutingStrategies.Bubble);
    
    public event EventHandler<TreeViewItemLoadRequestEventArgs>? NodeLoadRequest
    {
        add => AddHandler(NodeLoadRequestEvent, value);
        remove => RemoveHandler(NodeLoadRequestEvent, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsLeafIconVisibleProperty =
        AvaloniaProperty.Register<NodeSwitcherButton, bool>(nameof(IsLeafIconVisible), true);
    
    internal static readonly StyledProperty<ITransform?> RotationIconRenderTransformProperty =
        AvaloniaProperty.Register<NodeSwitcherButton, ITransform?>(nameof(RotationIconRenderTransform));
    
    internal static readonly DirectProperty<NodeSwitcherButton, NodeSwitcherButtonIconMode> IconModeProperty =
        AvaloniaProperty.RegisterDirect<NodeSwitcherButton, NodeSwitcherButtonIconMode>(
            nameof(IconMode),
            o => o.IconMode,
            (o, v) => o.IconMode = v);

    internal static readonly DirectProperty<NodeSwitcherButton, PathIcon?> CurrentIconProperty =
        AvaloniaProperty.RegisterDirect<NodeSwitcherButton, PathIcon?>(
            nameof(CurrentIcon),
            o => o.CurrentIcon,
            (o, v) => o.CurrentIcon = v);

    internal static readonly DirectProperty<NodeSwitcherButton, bool> IsCurrentIconVisibleProperty =
        AvaloniaProperty.RegisterDirect<NodeSwitcherButton, bool>(
            nameof(IsCurrentIconVisible),
            o => o.IsCurrentIconVisible,
            (o, v) => o.IsCurrentIconVisible = v);
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NodeSwitcherButton>();

    internal bool IsLeafIconVisible
    {
        get => GetValue(IsLeafIconVisibleProperty);
        set => SetValue(IsLeafIconVisibleProperty, value);
    }
    
    internal ITransform? RotationIconRenderTransform
    {
        get => GetValue(RotationIconRenderTransformProperty);
        set => SetValue(RotationIconRenderTransformProperty, value);
    }
    
    private NodeSwitcherButtonIconMode _iconMode;

    internal NodeSwitcherButtonIconMode IconMode
    {
        get => _iconMode;
        set => SetAndRaise(IconModeProperty, ref _iconMode, value);
    }

    private PathIcon? _currentIcon;

    internal PathIcon? CurrentIcon
    {
        get => _currentIcon;
        private set => SetAndRaise(CurrentIconProperty, ref _currentIcon, value);
    }

    private bool _isCurrentIconVisible;

    internal bool IsCurrentIconVisible
    {
        get => _isCurrentIconVisible;
        private set => SetAndRaise(IsCurrentIconVisibleProperty, ref _isCurrentIconVisible, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    
    #endregion

    private readonly BorderRenderHelper _borderRenderHelper;
    private TreeViewItem? _owningTreeItem;

    static NodeSwitcherButton()
    {
        AffectsRender<NodeSwitcherButton>(BackgroundProperty);
        AffectsMeasure<NodeSwitcherButton>(ExpandIconProperty, CollapseIconProperty, IsCheckedProperty,
            LoadingIconProperty, LeafIconProperty, RotationIconProperty, IconModeProperty, IsLeafIconVisibleProperty);
    }

    public NodeSwitcherButton()
    {
        _borderRenderHelper = new BorderRenderHelper();
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupDefaultIcons();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CollapseIconProperty ||
            change.Property == ExpandIconProperty ||
            change.Property == LoadingIconProperty ||
            change.Property == RotationIconProperty)
        {
            SetupDefaultIcons();
        }
        if (change.Property == CollapseIconProperty ||
            change.Property == ExpandIconProperty ||
            change.Property == LoadingIconProperty ||
            change.Property == RotationIconProperty ||
            change.Property == LeafIconProperty ||
            change.Property == IconModeProperty ||
            change.Property == IsCheckedProperty ||
            change.Property == IsLeafIconVisibleProperty)
        {
            UpdateCurrentIcon();
        }
    }

    public override void Render(DrawingContext context)
    {
        if (_iconMode != NodeSwitcherButtonIconMode.Leaf)
        {
            _borderRenderHelper.Render(context,
                Bounds.Size,
                new Thickness(),
                CornerRadius,
                BackgroundSizing.InnerBorderEdge,
                Background,
                null,
                default);
        }
    }
    
    private void SetupDefaultIcons()
    {
        if (ExpandIcon == null)
        {
            ClearValue(ExpandIconProperty);
            SetValue(ExpandIconProperty, new PlusSquareOutlined(), BindingPriority.Template);
        }
        
        if (CollapseIcon == null)
        {
            ClearValue(CollapseIconProperty);
            SetValue(CollapseIconProperty, new MinusSquareOutlined(), BindingPriority.Template);
        }
        
        if (RotationIcon == null)
        {
            ClearValue(RotationIconProperty);
            SetValue(RotationIconProperty, new CaretRightOutlined(), BindingPriority.Template);
        }
        
        if (LeafIcon == null)
        {
            ClearValue(LeafIconProperty);
            SetValue(LeafIconProperty, new FileOutlined(), BindingPriority.Template);
        }

        if (LoadingIcon == null)
        {
            ClearValue(LoadingIconProperty);
            SetValue(LoadingIconProperty, new LoadingOutlined()
            {
                LoadingAnimation = IconAnimation.Spin
            }, BindingPriority.Template);
        }
        UpdateCurrentIcon();
    }

    private void UpdateCurrentIcon()
    {
        var icon = IconMode switch
        {
            NodeSwitcherButtonIconMode.Default => IsChecked == true ? CollapseIcon : ExpandIcon,
            NodeSwitcherButtonIconMode.Rotation => RotationIcon,
            NodeSwitcherButtonIconMode.Loading => LoadingIcon,
            NodeSwitcherButtonIconMode.Leaf => IsLeafIconVisible ? LeafIcon : null,
            _ => null
        };

        CurrentIcon = icon;
        IsCurrentIconVisible = icon is not null;
    }

    protected override void Toggle()
    {
        _owningTreeItem ??= this.FindAncestorOfType<TreeViewItem>();
        Debug.Assert(_owningTreeItem != null);
        if (_owningTreeItem.HasTreeItemDataLoader && !_owningTreeItem.AsyncLoaded)
        {
            RaiseEvent(new TreeViewItemLoadRequestEventArgs(_owningTreeItem)
            {
                RoutedEvent = NodeLoadRequestEvent
            });
            return;
        }
        base.Toggle();
    }

}
