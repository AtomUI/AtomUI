using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using FlyoutControl = Flyout;

public partial class DropdownButton : Button
{
    #region 公共属性定义

    public static readonly StyledProperty<MenuFlyout?> DropdownFlyoutProperty =
        AvaloniaProperty.Register<DropdownButton, MenuFlyout?>(nameof(DropdownFlyout));

    public static readonly StyledProperty<FlyoutTriggerType> TriggerTypeProperty =
        FlyoutStateHelper.TriggerTypeProperty.AddOwner<DropdownButton>();

    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<DropdownButton>();

    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        FlyoutControl.IsPointAtCenterProperty.AddOwner<DropdownButton>();

    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        Avalonia.Controls.Primitives.Popup.PlacementProperty.AddOwner<DropdownButton>();

    public static readonly StyledProperty<PopupAnchor> PlacementAnchorProperty =
        Avalonia.Controls.Primitives.Popup.PlacementAnchorProperty.AddOwner<DropdownButton>();

    public static readonly StyledProperty<PopupGravity> PlacementGravityProperty =
        Avalonia.Controls.Primitives.Popup.PlacementGravityProperty.AddOwner<DropdownButton>();

    public static readonly StyledProperty<double> MarginToAnchorProperty =
        Popup.MarginToAnchorProperty.AddOwner<DropdownButton>();

    public static readonly StyledProperty<int> MouseEnterDelayProperty =
        FlyoutStateHelper.MouseEnterDelayProperty.AddOwner<DropdownButton>();

    public static readonly StyledProperty<int> MouseLeaveDelayProperty =
        FlyoutStateHelper.MouseLeaveDelayProperty.AddOwner<DropdownButton>();

    public static readonly StyledProperty<bool> IsShowOpenIndicatorProperty =
        AvaloniaProperty.Register<DropdownButton, bool>(nameof(IsShowOpenIndicator), true);

    public static readonly StyledProperty<PathIcon?> OpenIndicatorProperty =
        AvaloniaProperty.Register<DropdownButton, PathIcon?>(nameof(OpenIndicator));

    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<DropdownButton, bool>(nameof(ShouldUseOverlayPopup), true);

    /// <summary>
    /// 弹层最小宽度是否匹配 DropdownButton 宽度，默认 false；命名与其余弹层类控件
    /// （AbstractSelect 等）的 IsPopupMatchSelectWidth 保持一致。
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupMatchSelectWidthProperty =
        AvaloniaProperty.Register<DropdownButton, bool>(nameof(IsPopupMatchSelectWidth));

    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        FlyoutControl.IsPopupPinnedOpenProperty.AddOwner<DropdownButton>();

    public MenuFlyout? DropdownFlyout
    {
        get => GetValue(DropdownFlyoutProperty);
        set => SetValue(DropdownFlyoutProperty, value);
    }

    public FlyoutTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
    }

    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public PopupGravity PlacementGravity
    {
        get => GetValue(PlacementGravityProperty);
        set => SetValue(PlacementGravityProperty, value);
    }

    public PopupAnchor PlacementAnchor
    {
        get => GetValue(PlacementAnchorProperty);
        set => SetValue(PlacementAnchorProperty, value);
    }

    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    public int MouseEnterDelay
    {
        get => GetValue(MouseEnterDelayProperty);
        set => SetValue(MouseEnterDelayProperty, value);
    }

    public int MouseLeaveDelay
    {
        get => GetValue(MouseLeaveDelayProperty);
        set => SetValue(MouseLeaveDelayProperty, value);
    }
    
    public bool IsShowOpenIndicator
    {
        get => GetValue(IsShowOpenIndicatorProperty);
        set => SetValue(IsShowOpenIndicatorProperty, value);
    }
    
    public PathIcon? OpenIndicator
    {
        get => GetValue(OpenIndicatorProperty);
        set => SetValue(OpenIndicatorProperty, value);
    }

    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    public bool IsPopupMatchSelectWidth
    {
        get => GetValue(IsPopupMatchSelectWidthProperty);
        set => SetValue(IsPopupMatchSelectWidthProperty, value);
    }

    /// <summary>
    /// 钉住弹层：开启后弹层忽略 light-dismiss 关闭请求，保持强制打开。
    /// 典型场景是 Gallery 语义部件预览需要持续高亮 popup.* 部件。
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<FlyoutMenuItemClickedEventArgs> MenuItemClickedEvent =
        RoutedEvent.Register<DropdownButton, FlyoutMenuItemClickedEventArgs>(
            nameof(MenuItemClicked),
            RoutingStrategies.Bubble);
    
    public event EventHandler<FlyoutMenuItemClickedEventArgs>? MenuItemClicked
    {
        add => AddHandler(MenuItemClickedEvent, value);
        remove => RemoveHandler(MenuItemClickedEvent, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<DropdownButton, bool> IsContentVisibleProperty =
        AvaloniaProperty.RegisterDirect<DropdownButton, bool>(nameof(IsContentVisible),
            o => o.IsContentVisible,
            (o, v) => o.IsContentVisible = v);

    private bool _isContentVisible;

    internal bool IsContentVisible
    {
        get => _isContentVisible;
        set => SetAndRaise(IsContentVisibleProperty, ref _isContentVisible, value);
    }

    #endregion
    
    private readonly FlyoutStateHelper _flyoutStateHelper;
    private CompositeDisposable? _flyoutBindingDisposables;
    private int _pinnedOpenGeneration;
    private MenuFlyout? _registeredDropdownFlyout;
    static DropdownButton()
    {
        PlacementProperty.OverrideDefaultValue<DropdownButton>(PlacementMode.BottomEdgeAlignedLeft);
        IsArrowVisibleProperty.OverrideDefaultValue<DropdownButton>(false);
    }

    public DropdownButton()
    {
        _flyoutStateHelper = new FlyoutStateHelper
        {
            AnchorTarget = this
        };
        _flyoutStateHelper[!FlyoutStateHelper.FlyoutProperty]          = this[!DropdownFlyoutProperty];
        _flyoutStateHelper[!FlyoutStateHelper.MouseEnterDelayProperty] = this[!MouseEnterDelayProperty];
        _flyoutStateHelper[!FlyoutStateHelper.MouseLeaveDelayProperty] = this[!MouseLeaveDelayProperty];
        _flyoutStateHelper[!FlyoutStateHelper.TriggerTypeProperty]     = this[!TriggerTypeProperty];
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (OpenIndicator == null)
        {
            SetCurrentValue(OpenIndicatorProperty, new DownOutlined());
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _flyoutStateHelper.NotifyAttachedToVisualTree();
        RegisterDropdownFlyout(DropdownFlyout);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _flyoutStateHelper.NotifyDetachedFromVisualTree();
        UnregisterDropdownFlyout(_registeredDropdownFlyout);
    }
    
    private void RegisterDropdownFlyout(MenuFlyout? menuFlyout)
    {
        if (menuFlyout == null || ReferenceEquals(_registeredDropdownFlyout, menuFlyout))
        {
            return;
        }

        UnregisterDropdownFlyout(_registeredDropdownFlyout);

        _registeredDropdownFlyout = menuFlyout;
        menuFlyout.MenuItemClicked += HandleMenuItemClicked;
        menuFlyout.Opening += HandleMenuFlyoutOpening;

        _flyoutBindingDisposables?.Dispose();
        _flyoutBindingDisposables = new CompositeDisposable(9);
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, PlacementProperty, menuFlyout, FlyoutControl.RequestedPlacementProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, PlacementAnchorProperty, menuFlyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, PlacementGravityProperty, menuFlyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsArrowVisibleProperty, menuFlyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsPointAtCenterProperty, menuFlyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, MarginToAnchorProperty, menuFlyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, menuFlyout));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, ShouldUseOverlayPopupProperty, menuFlyout, MenuFlyout.ShouldUseOverlayPopupProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsPopupPinnedOpenProperty, menuFlyout, FlyoutControl.IsPopupPinnedOpenProperty));

        if (IsPopupPinnedOpen && this.IsAttachedToVisualTree())
        {
            QueuePinnedOpen();
        }
    }

    private void UnregisterDropdownFlyout(MenuFlyout? menuFlyout)
    {
        if (menuFlyout == null || !ReferenceEquals(_registeredDropdownFlyout, menuFlyout))
        {
            return;
        }

        menuFlyout.MenuItemClicked -= HandleMenuItemClicked;
        menuFlyout.Opening -= HandleMenuFlyoutOpening;
        ++_pinnedOpenGeneration;
        menuFlyout.CloseForLifecycle();
        _registeredDropdownFlyout = null;
        _flyoutBindingDisposables?.Dispose();
        _flyoutBindingDisposables = null;
    }
    
    private void HandleMenuItemClicked(object? sender, FlyoutMenuItemClickedEventArgs args)
    {
        var eventArgs = new FlyoutMenuItemClickedEventArgs(MenuItemClickedEvent, args.Item);
        RaiseEvent(eventArgs);
    }

    private void HandleMenuFlyoutOpening(object? sender, EventArgs e)
    {
        if (!IsPopupMatchSelectWidth ||
            sender is not MenuFlyout menuFlyout ||
            menuFlyout.Popup?.Child is not Control popupChild ||
            !(Bounds.Width > 0))
        {
            return;
        }

        popupChild.MinWidth = Bounds.Width;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DropdownFlyoutProperty)
        {
            if (change.OldValue is MenuFlyout oldMenuFlyout)
            {
                UnregisterDropdownFlyout(oldMenuFlyout);
            }

            if (change.NewValue is MenuFlyout newMenuFlyout && this.IsAttachedToVisualTree())
            {
                RegisterDropdownFlyout(newMenuFlyout);
            }
        }
        else if (change.Property == IsPopupPinnedOpenProperty && this.IsAttachedToVisualTree())
        {
            if (change.GetNewValue<bool>())
            {
                QueuePinnedOpen();
            }
            else
            {
                ++_pinnedOpenGeneration;
            }
        }

        if (change.Property == ContentProperty ||
            change.Property == ContentTemplateProperty ||
            change.Property == IsLoadingProperty ||
            change.Property == IconProperty)
        {
            ConfigureContentVisible();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (RightExtraContent == null)
        {
            SetCurrentValue(RightExtraContentProperty, new Border());
        }

        ConfigureContentVisible();
    }

    private void ConfigureContentVisible()
    {
        SetCurrentValue(IsContentVisibleProperty, IsLoading || Content != null || ContentTemplate != null || Icon != null);
    }

    private void QueuePinnedOpen()
    {
        var generation = ++_pinnedOpenGeneration;
        var flyout     = _registeredDropdownFlyout;
        Dispatcher.Post(() =>
        {
            if (generation == _pinnedOpenGeneration &&
                IsPopupPinnedOpen &&
                this.IsAttachedToVisualTree() &&
                ReferenceEquals(flyout, _registeredDropdownFlyout))
            {
                _flyoutStateHelper.ShowFlyout(immediately: true);
            }
        });
    }
}
