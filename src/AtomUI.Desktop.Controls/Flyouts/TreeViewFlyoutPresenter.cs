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

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<FlyoutTreeViewItemClickedEventArgs> TreeViewItemClickedEvent =
        RoutedEvent.Register<TreeViewFlyoutPresenter, FlyoutTreeViewItemClickedEventArgs>(
            nameof(TreeViewItemClicked),
            RoutingStrategies.Bubble);

    #endregion

    #region 公共属性定义

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

    public event EventHandler<FlyoutTreeViewItemClickedEventArgs>? TreeViewItemClicked
    {
        add => AddHandler(TreeViewItemClickedEvent, value);
        remove => RemoveHandler(TreeViewItemClickedEvent, value);
    }

    #endregion

    #region 运行时字段

    private ArrowDecoratedBox? _arrowDecoratedBox;

    #endregion

    #region 构造与初始化

    static TreeViewFlyoutPresenter()
    {
        // Tree items expose one bubbling Click event. Observe it at this presenter
        // instead of retaining per-item MenuItem delegates on the wrong type.
        TreeViewItem.ClickEvent.AddClassHandler<TreeViewFlyoutPresenter>((presenter, args) =>
            presenter.HandleTreeViewItemClicked(args.Source, args));
    }

    public TreeViewFlyoutPresenter()
        : base(new DefaultTreeViewInteractionHandler(true))
    {
    }

    public TreeViewFlyoutPresenter(ITreeViewInteractionHandler menuInteractionHandler)
        : base(menuInteractionHandler)
    {
    }

    #endregion

    #region 公共与受保护 API

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

    #endregion

    #region 私有实现

    private void HandleTreeViewItemClicked(object? sender, RoutedEventArgs args)
    {
        if (sender is TreeViewItem treeViewItem)
        {
            var ev = new FlyoutTreeViewItemClickedEventArgs(TreeViewItemClickedEvent, treeViewItem);
            RaiseEvent(ev);
        }
    }

    #endregion

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
