using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[global::System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
public class TreeViewFlyoutPresenter : FloatableTreeView, IShadowMaskInfoProvider
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<TreeViewFlyoutPresenter>();

    public static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<TreeViewFlyoutPresenter>();

    /// <summary>
    /// 是否显示指示箭头
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// 箭头渲染的位置
    /// </summary>
    public ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }
    
    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<FlyoutTreeViewItemClickedEventArgs> TreeViewItemClickedEvent =
        RoutedEvent.Register<TreeViewFlyoutPresenter, FlyoutTreeViewItemClickedEventArgs>(
            nameof(TreeViewItemClicked),
            RoutingStrategies.Bubble);
    
    public event EventHandler<FlyoutTreeViewItemClickedEventArgs>? TreeViewItemClicked
    {
        add => AddHandler(TreeViewItemClickedEvent, value);
        remove => RemoveHandler(TreeViewItemClickedEvent, value);
    }

    #endregion
        
    #region 内部属性定义
    
    private ArrowDecoratedBox? _arrowDecoratedBox;

    #endregion
    
    public TreeViewFlyoutPresenter()
        : base(new DefaultTreeViewInteractionHandler(true))
    {
    }

    public TreeViewFlyoutPresenter(ITreeViewInteractionHandler menuInteractionHandler)
        : base(menuInteractionHandler)
    {
    }
    
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is MenuItem menuItem)
        {
            BindTreeViewItemClickedRecursive(menuItem);
        }
    }
    
    protected override void ClearContainerForItemOverride(Control container)
    {
        base.ClearContainerForItemOverride(container);
        if (container is MenuItem menuItem)
        {
            ClearMenuItemClickedRecursive(menuItem);
        }
    }
    
    private void BindTreeViewItemClickedRecursive(MenuItem menuItem)
    {
        foreach (var childItem in menuItem.Items)
        {
            if (childItem is MenuItem childMenuItem)
            {
                BindTreeViewItemClickedRecursive(childMenuItem);
            }
        }

        // 绑定自己
        menuItem.Click += HandleTreeViewItemClicked;
    }

    private void ClearMenuItemClickedRecursive(MenuItem menuItem)
    {
        foreach (var childItem in menuItem.Items)
        {
            if (childItem is MenuItem childMenuItem)
            {
                ClearMenuItemClickedRecursive(childMenuItem);
            }
        }

        // 绑定自己
        menuItem.Click -= HandleTreeViewItemClicked;
    }

    private void HandleTreeViewItemClicked(object? sender, RoutedEventArgs args)
    {
        if (sender is TreeViewItem treeViewItem)
        {
            var ev = new FlyoutTreeViewItemClickedEventArgs(TreeViewItemClickedEvent, treeViewItem);
            RaiseEvent(ev);
        }
    }
    
    public CornerRadius GetMaskCornerRadius()
    {
        if (_arrowDecoratedBox is not null)
        {
            return _arrowDecoratedBox.CornerRadius;
        }

        return new CornerRadius(0);
    }
    
    public Rect GetMaskBounds()
    {
        if (_arrowDecoratedBox is not null)
        {
            var contentRect = _arrowDecoratedBox.GetMaskBounds();
            var adjustedPos = _arrowDecoratedBox.TranslatePoint(contentRect.Position, this) ?? default;
            return new Rect(adjustedPos, contentRect.Size);
        }

        return Bounds;
    }

    public IBrush? GetMaskBackground()
    {
        return _arrowDecoratedBox?.Background;
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _arrowDecoratedBox = e.NameScope.Find<ArrowDecoratedBox>(ArrowDecoratedBox.ArrowDecoratorPart);
    } 
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (container is MenuItem menuItem)
        {
            menuItem[!MenuItem.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        }

        base.PrepareContainerForItemOverride(container, item, index);
    }
}

public class FlyoutTreeViewItemClickedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// 当前鼠标点击的树节点
    /// </summary>
    public TreeViewItem ViewItem { get; }

    public FlyoutTreeViewItemClickedEventArgs(RoutedEvent routedEvent, TreeViewItem treeViewItem)
        : base(routedEvent)
    {
        ViewItem = treeViewItem;
    }
}
