using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public class TreeViewFlyoutPresenter : FloatableTreeView,
                                       IShadowMaskInfoProvider,
                                       IMotionAwareControl,
                                       IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsShowArrowProperty =
        ArrowDecoratedBox.IsShowArrowProperty.AddOwner<TreeViewFlyoutPresenter>();

    public static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<TreeViewFlyoutPresenter>();

    /// <summary>
    /// 是否显示指示箭头
    /// </summary>
    public bool IsShowArrow
    {
        get => GetValue(IsShowArrowProperty);
        set => SetValue(IsShowArrowProperty, value);
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
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => MenuToken.ID;
    
    private ArrowDecoratedBox? _arrowDecoratedBox;

    #endregion
    
    private readonly Dictionary<MenuItem, CompositeDisposable> _itemsBindingDisposables = new();
    
    public TreeViewFlyoutPresenter()
        : base(new DefaultTreeViewInteractionHandler(true))
    {
        this.RegisterResources();
        Items.CollectionChanged += HandleCollectionChanged;
    }

    public TreeViewFlyoutPresenter(ITreeViewInteractionHandler menuInteractionHandler)
        : base(menuInteractionHandler)
    {
        this.RegisterResources();
        Items.CollectionChanged += HandleCollectionChanged;
    }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is MenuItem menuItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(menuItem, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(menuItem);
                        }
                    }
                }
            }
        }
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
            var disposables = new CompositeDisposable(1);
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, menuItem, MenuItem.IsMotionEnabledProperty));
            if (_itemsBindingDisposables.TryGetValue(menuItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(menuItem);
            }
            _itemsBindingDisposables.Add(menuItem, disposables);
        }

        base.PrepareContainerForItemOverride(container, item, index);
    }
}

public class FlyoutTreeViewItemClickedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// 当前鼠标点击的树节点
    /// </summary>
    public TreeViewItem Item { get; }

    public FlyoutTreeViewItemClickedEventArgs(RoutedEvent routedEvent, TreeViewItem treeViewItem)
        : base(routedEvent)
    {
        Item = treeViewItem;
    }
}
