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

/// <summary>
/// The user input that opens and closes the flyout of a control.
/// </summary>
public enum FlyoutTriggerType
{
    /// <summary>
    /// Opens the flyout after the pointer has rested on the anchor for the enter delay of the control, and closes it
    /// after the pointer has left the anchor and the flyout for its leave delay.
    /// </summary>
    Hover,
    /// <summary>
    /// Toggles the flyout when the anchor is pressed, or when Enter or Space is pressed while the anchor has keyboard
    /// focus. A click outside the flyout closes it.
    /// </summary>
    Click,
    /// <summary>
    /// Opens the flyout when the anchor receives focus or is pressed while the flyout is closed, and closes it when
    /// focus leaves the anchor and the flyout. A pointer press outside the flyout closes it at once.
    /// </summary>
    Focus,
}

/// <summary>
/// Hosts the anchor of its <see cref="Flyout" /> and opens the flyout at it in response to <see cref="Trigger" />.
/// The anchor is the <see cref="ContentControl.Content" /> of this control. While attached to the visual tree, this
/// host forwards its placement, motion and shadow properties to the flyout.
/// </summary>
public partial class FlyoutHost : ContentControl, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="PopupRootShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> PopupRootShadowProperty =
        Popup.PopupRootShadowProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="OverlayHostShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> OverlayHostShadowProperty =
        Popup.OverlayHostShadowProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="Flyout" /> property.
    /// </summary>
    public static readonly StyledProperty<Flyout?> FlyoutProperty =
        AvaloniaProperty.Register<FlyoutHost, Flyout?>(nameof(Flyout));

    /// <summary>
    /// Identifies the <see cref="Trigger" /> property.
    /// </summary>
    public static readonly StyledProperty<FlyoutTriggerType> TriggerProperty =
        FlyoutStateHelper.TriggerTypeProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="IsArrowVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="IsPointAtCenter" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        FlyoutControl.IsPointAtCenterProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        Popup.PlacementProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="PlacementAnchor" /> property.
    /// </summary>
    public static readonly StyledProperty<PopupAnchor> PlacementAnchorProperty =
        Popup.PlacementAnchorProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="PlacementGravity" /> property.
    /// </summary>
    public static readonly StyledProperty<PopupGravity> PlacementGravityProperty =
        Popup.PlacementGravityProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="MotionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        Flyout.ShouldUseOverlayPopupProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        FlyoutControl.IsPopupPinnedOpenProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="MarginToAnchor" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MarginToAnchorProperty =
        Popup.MarginToAnchorProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="MouseEnterDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MouseEnterDelayProperty =
        FlyoutStateHelper.MouseEnterDelayProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Identifies the <see cref="MouseLeaveDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MouseLeaveDelayProperty =
        FlyoutStateHelper.MouseLeaveDelayProperty.AddOwner<FlyoutHost>();

    /// <summary>
    /// Shadow of the flyout while its popup is a native top-level window rather than the overlay layer, where
    /// <see cref="OverlayHostShadow" /> applies. Without an explicit value the theme supplies the PopupRootShadow
    /// token.
    /// </summary>
    public BoxShadows PopupRootShadow
    {
        get => GetValue(PopupRootShadowProperty);
        set => SetValue(PopupRootShadowProperty, value);
    }

    /// <summary>
    /// Shadow of the flyout while its popup is shown in the overlay layer rather than as a native top-level window,
    /// where <see cref="PopupRootShadow" /> applies. Without an explicit value the theme supplies the OverlayHostShadow
    /// token.
    /// </summary>
    public BoxShadows OverlayHostShadow
    {
        get => GetValue(OverlayHostShadowProperty);
        set => SetValue(OverlayHostShadowProperty, value);
    }

    /// <summary>
    /// The flyout to open at the <see cref="ContentControl.Content" /> of this host. While this host is attached to
    /// the visual tree, the flyout receives the placement, arrow, motion, shadow and pinned-open values of this host,
    /// and changing the value closes the flyout that was registered before, if it is open. <see langword="null" />
    /// shows no flyout.
    /// </summary>
    public Flyout? Flyout
    {
        get => GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
    }

    /// <summary>
    /// The user input that opens and closes <see cref="Flyout" />. The enter and leave delays apply to
    /// <see cref="FlyoutTriggerType.Hover" /> only. The default is <see cref="FlyoutTriggerType.Click" />.
    /// </summary>
    public FlyoutTriggerType Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    /// <summary>
    /// Draws an arrow that points from the flyout to this host. The arrow is drawn only when <see cref="Placement" />,
    /// <see cref="PlacementAnchor" /> and <see cref="PlacementGravity" /> also allow one. The default is
    /// <see langword="false" />.
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// Moves the flyout so that its arrow points at the center of this host. It has an effect only while the arrow is
    /// drawn. The default is <see langword="false" />.
    /// </summary>
    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    /// <summary>
    /// Where the flyout is placed relative to this host. The value selects the AtomUI positioning of the flyout, which
    /// adds <see cref="MarginToAnchor" />, the arrow and flipping. The default is <see cref="PlacementMode.Top" />.
    /// </summary>
    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// The anchor point on this host that the flyout is placed at, as a refinement of <see cref="Placement" />. It is
    /// also one of the values that decide whether the arrow can be drawn.
    /// </summary>
    public PopupAnchor PlacementAnchor
    {
        get => GetValue(PlacementAnchorProperty);
        set => SetValue(PlacementAnchorProperty, value);
    }

    /// <summary>
    /// The direction in which the flyout opens from <see cref="PlacementAnchor" />, as a refinement of
    /// <see cref="Placement" />. It is also one of the values that decide whether the arrow can be drawn.
    /// </summary>
    public PopupGravity PlacementGravity
    {
        get => GetValue(PlacementGravityProperty);
        set => SetValue(PlacementGravityProperty, value);
    }

    /// <summary>
    /// Switches the open and close motions of the flyout on or off; when it is <see langword="false" />, the flyout is
    /// removed at once instead of playing <see cref="CloseMotion" />. Without an explicit value the theme supplies the
    /// EnableMotion token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Duration of the open and close motions of the flyout. Without an explicit value the theme supplies the
    /// MotionDurationMid token.
    /// </summary>
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    /// <summary>
    /// Motion played when the flyout opens. Without an explicit value the theme supplies the popover open motion.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// Motion played when the flyout closes, only while <see cref="IsMotionEnabled" /> is <see langword="true" />.
    /// Without an explicit value the theme supplies the popover close motion.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// Shows the flyout in the overlay layer of the window instead of a native top-level popup window. The overlay
    /// layer is also used on platforms without native popup windows. <see cref="OverlayHostShadow" /> applies in the
    /// overlay layer and <see cref="PopupRootShadow" /> in a native window. The default is <see langword="true" />.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Shows the flyout without waiting for <see cref="Trigger" /> and keeps it open: while this host is attached to
    /// the visual tree, effectively visible and effectively enabled, the flyout is opened at once, and it is closed as
    /// soon as one of these conditions no longer holds. The value is also forwarded to the flyout.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    /// <summary>
    /// Gap between the flyout and this host, in device-independent pixels, applied vertically or horizontally as the
    /// placement requires. It is ignored for placements that follow the pointer and for some combinations of
    /// <see cref="PlacementAnchor" /> and <see cref="PlacementGravity" />. Without an explicit value the theme supplies
    /// the MarginToAnchor token.
    /// </summary>
    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    /// <summary>
    /// Time in milliseconds before the flyout opens, unless it is opened immediately. The
    /// <see cref="FlyoutTriggerType.Hover" /> trigger and <see cref="ShowFlyout" /> called with
    /// <see langword="false" /> wait for it; <c>0</c> opens the flyout at once. The default is <c>200</c>.
    /// </summary>
    public int MouseEnterDelay
    {
        get => GetValue(MouseEnterDelayProperty);
        set => SetValue(MouseEnterDelayProperty, value);
    }

    /// <summary>
    /// Time in milliseconds before the flyout closes after the pointer has left it, unless it is closed immediately.
    /// The <see cref="FlyoutTriggerType.Hover" /> trigger and <see cref="HideFlyout" /> called with
    /// <see langword="false" /> wait for it; <c>0</c> closes the flyout at once. The default is <c>200</c>.
    /// </summary>
    public int MouseLeaveDelay
    {
        get => GetValue(MouseLeaveDelayProperty);
        set => SetValue(MouseLeaveDelayProperty, value);
    }

    #endregion

    #region 内部属性定义

    #endregion

    #region 运行时字段

    private static readonly (AvaloniaProperty Source, AvaloniaProperty Target)[] FlyoutPropertyBindings =
    [
        (PlacementProperty, FlyoutControl.RequestedPlacementProperty),
        (PlacementAnchorProperty, FlyoutControl.PlacementAnchorProperty),
        (PlacementGravityProperty, FlyoutControl.PlacementGravityProperty),
        (IsArrowVisibleProperty, FlyoutControl.IsArrowVisibleProperty),
        (IsPointAtCenterProperty, FlyoutControl.IsPointAtCenterProperty),
        (MarginToAnchorProperty, FlyoutControl.MarginToAnchorProperty),
        (IsMotionEnabledProperty, FlyoutControl.IsMotionEnabledProperty),
        (MotionDurationProperty, FlyoutControl.MotionDurationProperty),
        (OpenMotionProperty, FlyoutControl.OpenMotionProperty),
        (CloseMotionProperty, FlyoutControl.CloseMotionProperty),
        (PopupRootShadowProperty, FlyoutControl.PopupRootShadowProperty),
        (OverlayHostShadowProperty, FlyoutControl.OverlayHostShadowProperty),
        (ShouldUseOverlayPopupProperty, FlyoutControl.ShouldUseOverlayPopupProperty),
        (IsPopupPinnedOpenProperty, FlyoutControl.IsPopupPinnedOpenProperty)
    ];

    private readonly FlyoutStateHelper _flyoutStateHelper;

    private CompositeDisposable? _flyoutDisposables;

    private CompositeDisposable? _pinnedOpenHostSubscriptions;

    private int _pinnedOpenGeneration;

    private Flyout? _registeredFlyout;

    #endregion

    #region 构造函数

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

    #endregion

    #region 控件 API

    /// <summary>
    /// Registers <see cref="Flyout" /> for the property forwarding, starts the tracking of <see cref="Trigger" /> and
    /// shows the flyout at once when <see cref="IsPopupPinnedOpen" /> is set. The base implementation is called first;
    /// an override must keep calling it.
    /// </summary>
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

    /// <summary>
    /// Ends the pinned-open tracking, calls the base implementation, then stops the tracking of <see cref="Trigger" />
    /// and the property forwarding, which closes a flyout that is still open. An override must keep calling the base
    /// implementation.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ++_pinnedOpenGeneration;
        ReleasePinnedOpenHostSubscriptions();
        base.OnDetachedFromVisualTree(e);
        _flyoutStateHelper.NotifyDetachedFromVisualTree();
        UnregisterFlyoutProperties(_registeredFlyout);
    }

    /// <summary>
    /// Makes <paramref name="flyout" /> the registered flyout of this host until
    /// <see cref="UnregisterFlyoutProperties" /> is called: this host becomes the motion configuration owner of the
    /// flyout, and the placement, arrow, motion, shadow and pinned-open values of this host are relayed to it. Called
    /// when this host attaches to the visual tree and whenever <see cref="Flyout" /> changes while it is attached.
    /// </summary>
    /// <param name="flyout">
    /// The flyout to register. A flyout other than the current <see cref="Flyout" /> is replaced by the current one.
    /// <see langword="null" />, a flyout that is already registered or a registration that is being disposed ends the
    /// call without effect.
    /// </param>
    protected virtual void RegisterFlyoutProperties(Flyout? flyout)
    {
        // Disposal marks the lease before raising Closed. Reentrant property
        // changes are reconciled from the latest Flyout after that lease ends.
        if (flyout is null || ReferenceEquals(_registeredFlyout, flyout) || _flyoutDisposables?.IsDisposed == true)
        {
            return;
        }

        UnregisterFlyoutProperties(_registeredFlyout);
        if (!ReferenceEquals(flyout, Flyout))
        {
            RegisterFlyoutProperties(Flyout);
            return;
        }

        var bindings = new CompositeDisposable(15);
        var registration = new CompositeDisposable(1);
        registration.Add(Disposable.Create(() =>
        {
            // Positioning relays must remain valid while the physical host
            // closes. The outer lease is already retired during user callbacks.
            try
            {
                flyout.CloseForLifecycle();
            }
            finally
            {
                try
                {
                    bindings.Dispose();
                }
                finally
                {
                    flyout.ClearMotionConfigurationOwner(this);
                }
            }
        }));
        _registeredFlyout = flyout;
        _flyoutDisposables = registration;
        try
        {
            flyout.SetMotionConfigurationOwner(this);
            if (registration.IsDisposed)
            {
                return;
            }
            // This replaceable non-visual target is outside the ControlTemplate.
            // Configuration relays follow this replaceable registration's lifetime.
            foreach (var (source, target) in FlyoutPropertyBindings)
            {
                if (registration.IsDisposed)
                {
                    return;
                }
                bindings.Add(BindUtils.RelayBind(this, source, flyout, target));
            }
        }
        catch
        {
            if (ReferenceEquals(_flyoutDisposables, registration))
            {
                UnregisterFlyoutProperties(flyout);
            }
            else
            {
                registration.Dispose();
            }
            throw;
        }
    }

    /// <summary>
    /// Ends the registration that <see cref="RegisterFlyoutProperties" /> made for <paramref name="flyout" />: the
    /// relays are dropped, the flyout is closed in a way that cannot be canceled, and this host stops being its
    /// motion configuration owner.
    /// </summary>
    /// <param name="flyout">
    /// The flyout whose registration ends. <see langword="null" /> or a flyout that is not the registered one ends the
    /// call without effect.
    /// </param>
    protected virtual void UnregisterFlyoutProperties(Flyout? flyout)
    {
        if (flyout is null || !ReferenceEquals(_registeredFlyout, flyout))
        {
            return;
        }

        var registration = _flyoutDisposables!;
        _registeredFlyout = null;
        try
        {
            registration.Dispose();
        }
        finally
        {
            if (ReferenceEquals(_flyoutDisposables, registration))
            {
                _flyoutDisposables = null;
            }
        }
    }

    /// <summary>
    /// Calls the base implementation, then keeps the flyout registration and the pinned-open state in step with
    /// <see cref="Flyout" /> and <see cref="IsPopupPinnedOpen" /> while this host is attached to the visual tree. An
    /// override must keep calling the base implementation.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FlyoutProperty)
        {
            var (oldFlyout, _) = change.GetOldAndNewValue<Flyout?>();
            UnregisterFlyoutProperties(oldFlyout);
            if (this.IsAttachedToVisualTree())
            {
                RegisterFlyoutProperties(Flyout);
                if (IsPopupPinnedOpen)
                {
                    QueuePinnedOpen();
                }
            }
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

    /// <summary>
    /// Opens <see cref="Flyout" /> at this host's content without waiting for <see cref="Trigger" /> and cancels a
    /// pending close delay. A flyout that is open is shown again, unless it is playing its close motion, which is
    /// canceled so that it stays open.
    /// </summary>
    /// <param name="immediately">
    /// <see langword="true" /> opens the flyout at once; <see langword="false" /> opens it after
    /// <see cref="MouseEnterDelay" /> milliseconds.
    /// </param>
    public void ShowFlyout(bool immediately)
    {
        _flyoutStateHelper.ShowFlyout(immediately);
    }

    /// <summary>
    /// Closes <see cref="Flyout" /> regardless of <see cref="Trigger" /> and cancels a pending open delay.
    /// </summary>
    /// <param name="immediately">
    /// <see langword="true" /> starts closing the flyout at once; <see langword="false" /> starts closing it after
    /// <see cref="MouseLeaveDelay" /> milliseconds.
    /// </param>
    public void HideFlyout(bool immediately)
    {
        _flyoutStateHelper.HideFlyout(immediately);
    }

    #endregion

    #region 内部实现

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

    #endregion
}
