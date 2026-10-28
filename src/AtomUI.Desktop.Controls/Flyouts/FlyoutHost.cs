using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using FlyoutControl = Flyout;

public enum FlyoutTriggerType
{
    Hover,
    Click,
    Focus,
}

public partial class FlyoutHost : ContentControl, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<BoxShadows> PopupRootShadowProperty =
        Popup.PopupRootShadowProperty.AddOwner<FlyoutHost>();
    
    public static readonly StyledProperty<BoxShadows> OverlayHostShadowProperty =
        Popup.OverlayHostShadowProperty.AddOwner<FlyoutHost>();
    
    public static readonly StyledProperty<Flyout?> FlyoutProperty =
        AvaloniaProperty.Register<FlyoutHost, Flyout?>(nameof(Flyout));

    public static readonly StyledProperty<FlyoutTriggerType> TriggerProperty =
        FlyoutStateHelper.TriggerTypeProperty.AddOwner<FlyoutHost>();
    
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<FlyoutHost>();
    
    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        FlyoutControl.IsPointAtCenterProperty.AddOwner<FlyoutHost>();

    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        Popup.PlacementProperty.AddOwner<FlyoutHost>();

    public static readonly StyledProperty<PopupAnchor> PlacementAnchorProperty =
        Popup.PlacementAnchorProperty.AddOwner<FlyoutHost>();

    public static readonly StyledProperty<PopupGravity> PlacementGravityProperty =
        Popup.PlacementGravityProperty.AddOwner<FlyoutHost>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FlyoutHost>();
    
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<FlyoutHost>();
    
    public static readonly StyledProperty<Motion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<FlyoutHost>();
        
    public static readonly StyledProperty<Motion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<FlyoutHost>();
    
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        Flyout.ShouldUseOverlayPopupProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Gets or sets whether the flyout popup remains open while its placement target is available.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        FlyoutControl.IsPopupPinnedOpenProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// 距离 anchor 的边距，根据垂直和水平进行设置
    /// 但是对某些组合无效，比如跟随鼠标的情况
    /// 还有些 anchor 和 gravity 的组合也没有用
    /// </summary>
    public static readonly StyledProperty<double> MarginToAnchorProperty =
        Popup.MarginToAnchorProperty.AddOwner<FlyoutHost>();

    public static readonly StyledProperty<int> MouseEnterDelayProperty =
        FlyoutStateHelper.MouseEnterDelayProperty.AddOwner<FlyoutHost>();

    public static readonly StyledProperty<int> MouseLeaveDelayProperty =
        FlyoutStateHelper.MouseLeaveDelayProperty.AddOwner<FlyoutHost>();
    
    public BoxShadows PopupRootShadow
    {
        get => GetValue(PopupRootShadowProperty);
        set => SetValue(PopupRootShadowProperty, value);
    }
    
    public BoxShadows OverlayHostShadow
    {
        get => GetValue(OverlayHostShadowProperty);
        set => SetValue(OverlayHostShadowProperty, value);
    }

    public Flyout? Flyout
    {
        get => GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
    }

    public FlyoutTriggerType Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
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
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }
    
    public Motion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    public Motion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the flyout popup remains open while its placement target is available.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }
    #endregion
    
    private readonly FlyoutStateHelper _flyoutStateHelper;
    private CompositeDisposable? _flyoutDisposables;
    private CompositeDisposable? _pinnedOpenHostSubscriptions;
    private int _pinnedOpenGeneration;
    private Flyout? _registeredFlyout;
    
    static FlyoutHost()
    {
        PlacementProperty.OverrideDefaultValue<FlyoutHost>(PlacementMode.Top);
    }
    
    public FlyoutHost()
    {
        _flyoutStateHelper                                             = new FlyoutStateHelper();
        _flyoutStateHelper[!FlyoutStateHelper.AnchorTargetProperty]    = this[!ContentProperty];
        _flyoutStateHelper[!FlyoutStateHelper.FlyoutProperty]          = this[!FlyoutProperty];
        _flyoutStateHelper[!FlyoutStateHelper.MouseEnterDelayProperty] = this[!MouseEnterDelayProperty];
        _flyoutStateHelper[!FlyoutStateHelper.MouseLeaveDelayProperty] = this[!MouseLeaveDelayProperty];
        _flyoutStateHelper[!FlyoutStateHelper.TriggerTypeProperty]     = this[!TriggerProperty];
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RegisterFlyoutProperties(Flyout);
        _flyoutStateHelper.NotifyAttachedToVisualTree();
        if (IsPopupPinnedOpen)
        {
            SubscribeToPinnedOpenHostState();
            ReconcilePinnedOpenHostState();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ++_pinnedOpenGeneration;
        ReleasePinnedOpenHostSubscriptions();
        base.OnDetachedFromVisualTree(e);
        _flyoutStateHelper.NotifyDetachedFromVisualTree();
        UnregisterFlyoutProperties(_registeredFlyout);
    }

    protected virtual void RegisterFlyoutProperties(Flyout? flyout)
    {
        if (flyout is null || ReferenceEquals(_registeredFlyout, flyout))
        {
            return;
        }

        UnregisterFlyoutProperties(_registeredFlyout);
        _registeredFlyout  = flyout;
        _flyoutDisposables = new CompositeDisposable(14);
        _flyoutDisposables.Add(BindUtils.RelayBind(this, PlacementProperty, flyout, FlyoutControl.RequestedPlacementProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, PlacementAnchorProperty, flyout, FlyoutControl.PlacementAnchorProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, PlacementGravityProperty, flyout, FlyoutControl.PlacementGravityProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, IsArrowVisibleProperty, flyout, FlyoutControl.IsArrowVisibleProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, IsPointAtCenterProperty, flyout, FlyoutControl.IsPointAtCenterProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, MarginToAnchorProperty, flyout, FlyoutControl.MarginToAnchorProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, flyout, FlyoutControl.IsMotionEnabledProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, MotionDurationProperty, flyout, FlyoutControl.MotionDurationProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, OpenMotionProperty, flyout, FlyoutControl.OpenMotionProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, CloseMotionProperty, flyout, FlyoutControl.CloseMotionProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, PopupRootShadowProperty, flyout, FlyoutControl.PopupRootShadowProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, OverlayHostShadowProperty, flyout, FlyoutControl.OverlayHostShadowProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, ShouldUseOverlayPopupProperty, flyout, FlyoutControl.ShouldUseOverlayPopupProperty));
        _flyoutDisposables.Add(BindUtils.RelayBind(this, IsPopupPinnedOpenProperty, flyout, FlyoutControl.IsPopupPinnedOpenProperty));
        ConfigureMotion(Placement);
    }

    protected virtual void UnregisterFlyoutProperties(Flyout? flyout)
    {
        if (flyout is null || !ReferenceEquals(_registeredFlyout, flyout))
        {
            return;
        }

        flyout.CloseForLifecycle();
        _registeredFlyout = null;
        _flyoutDisposables?.Dispose();
        _flyoutDisposables = null;
    }

    private void ConfigureMotion(PlacementMode placement)
    {
        (OpenMotion, CloseMotion) = PopupUtils.CreateMotionForPlacement(placement);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FlyoutProperty)
        {
            var (oldFlyout, newFlyout) = change.GetOldAndNewValue<Flyout?>();
            UnregisterFlyoutProperties(oldFlyout);
            if (this.IsAttachedToVisualTree())
            {
                RegisterFlyoutProperties(newFlyout);
                if (IsPopupPinnedOpen)
                {
                    QueuePinnedOpen();
                }
            }
        }
        else if (change.Property == PlacementProperty && this.IsAttachedToVisualTree())
        {
            ConfigureMotion(Placement);
        }
        else if (change.Property == IsPopupPinnedOpenProperty &&
                 this.IsAttachedToVisualTree())
        {
            if (change.GetNewValue<bool>())
            {
                SubscribeToPinnedOpenHostState();
                ReconcilePinnedOpenHostState();
            }
            else
            {
                ++_pinnedOpenGeneration;
                ReleasePinnedOpenHostSubscriptions();
            }
        }
    }

    private void SubscribeToPinnedOpenHostState()
    {
        ReleasePinnedOpenHostSubscriptions();
        if (!IsPopupPinnedOpen || !this.IsAttachedToVisualTree())
        {
            return;
        }

        _pinnedOpenHostSubscriptions = new CompositeDisposable();
        this.GetObservable(IsVisibleProperty)
            .Subscribe(_ => ReconcilePinnedOpenHostState())
            .DisposeWith(_pinnedOpenHostSubscriptions);
        this.GetObservable(IsEffectivelyEnabledProperty)
            .Subscribe(_ => ReconcilePinnedOpenHostState())
            .DisposeWith(_pinnedOpenHostSubscriptions);
        this.SubscribeAncestorIsVisible(
            _ => ReconcilePinnedOpenHostState(),
            _pinnedOpenHostSubscriptions);
    }

    private void ReleasePinnedOpenHostSubscriptions()
    {
        _pinnedOpenHostSubscriptions?.Dispose();
        _pinnedOpenHostSubscriptions = null;
    }

    private void ReconcilePinnedOpenHostState()
    {
        if (!IsPopupPinnedOpen || !this.IsAttachedToVisualTree())
        {
            return;
        }

        if (IsEffectivelyVisible && IsEffectivelyEnabled)
        {
            QueuePinnedOpen();
        }
        else
        {
            ++_pinnedOpenGeneration;
            _registeredFlyout?.CloseForLifecycle();
        }
    }

    private void QueuePinnedOpen()
    {
        var generation = ++_pinnedOpenGeneration;
        var flyout     = _registeredFlyout;
        Dispatcher.Post(() =>
        {
            if (generation == _pinnedOpenGeneration &&
                IsPopupPinnedOpen &&
                this.IsAttachedToVisualTree() &&
                IsEffectivelyVisible &&
                IsEffectivelyEnabled &&
                ReferenceEquals(flyout, _registeredFlyout))
            {
                _flyoutStateHelper.ShowFlyout(immediately: true);
            }
        });
    }

    public void ShowFlyout(bool immediately)
    {
        _flyoutStateHelper.ShowFlyout(immediately);
    }

    public void HideFlyout(bool immediately)
    {
        _flyoutStateHelper.HideFlyout(immediately);
    }
}
