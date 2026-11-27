using AtomUI.Animations;
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

/// <summary>
/// A button that opens the <see cref="MenuFlyout" /> set as <see cref="DropdownFlyout" /> when the user
/// hovers over it, clicks it or focuses it, as selected by <see cref="TriggerType" />. Its placement,
/// arrow, motion and popup settings are forwarded to that flyout, and a click on one of its items raises
/// <see cref="MenuItemClicked" />.
/// </summary>
public partial class DropdownButton : Button
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Identifies the <see cref="DropdownFlyout" /> property.
    /// </summary>
    public static readonly StyledProperty<MenuFlyout?> DropdownFlyoutProperty =
        AvaloniaProperty.Register<DropdownButton, MenuFlyout?>(nameof(DropdownFlyout));

    /// <summary>
    /// Identifies the <see cref="TriggerType" /> property.
    /// </summary>
    public static readonly StyledProperty<FlyoutTriggerType> TriggerTypeProperty =
        FlyoutStateHelper.TriggerTypeProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Identifies the <see cref="IsArrowVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Identifies the <see cref="IsPointAtCenter" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        FlyoutControl.IsPointAtCenterProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        Avalonia.Controls.Primitives.Popup.PlacementProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Identifies the <see cref="PlacementAnchor" /> property.
    /// </summary>
    public static readonly StyledProperty<PopupAnchor> PlacementAnchorProperty =
        Avalonia.Controls.Primitives.Popup.PlacementAnchorProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Identifies the <see cref="PlacementGravity" /> property.
    /// </summary>
    public static readonly StyledProperty<PopupGravity> PlacementGravityProperty =
        Avalonia.Controls.Primitives.Popup.PlacementGravityProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Identifies the <see cref="MarginToAnchor" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MarginToAnchorProperty =
        Popup.MarginToAnchorProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Identifies the <see cref="MouseEnterDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MouseEnterDelayProperty =
        FlyoutStateHelper.MouseEnterDelayProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Identifies the <see cref="MouseLeaveDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MouseLeaveDelayProperty =
        FlyoutStateHelper.MouseLeaveDelayProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Identifies the <see cref="IsShowOpenIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowOpenIndicatorProperty =
        AvaloniaProperty.Register<DropdownButton, bool>(nameof(IsShowOpenIndicator), true);

    /// <summary>
    /// Identifies the <see cref="OpenIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> OpenIndicatorProperty =
        AvaloniaProperty.Register<DropdownButton, PathIcon?>(nameof(OpenIndicator));

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<DropdownButton, bool>(nameof(ShouldUseOverlayPopup), true);

    /// <summary>
    /// Identifies the <see cref="IsPopupMatchSelectWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupMatchSelectWidthProperty =
        AvaloniaProperty.Register<DropdownButton, bool>(nameof(IsPopupMatchSelectWidth));

    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        FlyoutControl.IsPopupPinnedOpenProperty.AddOwner<DropdownButton>();

    /// <summary>
    /// Motion played when the dropdown flyout opens. The value is forwarded to the flyout. The theme sets it to
    /// <c>PopupMotionDefaults.DropdownOpen</c>; without a value the flyout falls back to the popover open motion.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// Motion played when the dropdown flyout closes. The value is forwarded to the flyout, which plays it only
    /// while <c>IsMotionEnabled</c> is <see langword="true" />. Without a close motion the flyout is removed at
    /// once. The theme sets it to <c>PopupMotionDefaults.DropdownClose</c>.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// The menu flyout that the trigger opens for this button. While the button is attached to the visual
    /// tree, the flyout takes its motion, placement, arrow and popup settings from the properties of this
    /// button. Leave it <see langword="null" /> to keep the button from opening a menu. When the value changes
    /// or the button detaches, the previous flyout is closed and its bindings to this button are removed.
    /// </summary>
    public MenuFlyout? DropdownFlyout
    {
        get => GetValue(DropdownFlyoutProperty);
        set => SetValue(DropdownFlyoutProperty, value);
    }

    /// <summary>
    /// The user action that opens the flyout: hovering the button, clicking it or giving it focus. Only the click
    /// trigger lets a click outside the flyout close it. The default is <see cref="FlyoutTriggerType.Click" />.
    /// </summary>
    public FlyoutTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
    }

    /// <summary>
    /// Draws an arrow on the flyout that points at the button. The value is forwarded to the flyout, which
    /// draws the arrow only when <see cref="Placement" />, <see cref="PlacementAnchor" /> and
    /// <see cref="PlacementGravity" /> allow one. The default is <see langword="false" />.
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// Moves the flyout so that its arrow points at the center of the button. The value is forwarded to the flyout
    /// and has an effect only while the flyout draws an arrow.
    /// </summary>
    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    /// <summary>
    /// The position of the flyout relative to the button. The value is forwarded to the flyout as its requested
    /// placement. The default is <see cref="PlacementMode.BottomEdgeAlignedLeft" />.
    /// </summary>
    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// The direction in which the flyout extends from its anchor point on the button. The value is forwarded to the
    /// flyout.
    /// </summary>
    public PopupGravity PlacementGravity
    {
        get => GetValue(PlacementGravityProperty);
        set => SetValue(PlacementGravityProperty, value);
    }

    /// <summary>
    /// The point on the button at which the flyout is anchored. The value is forwarded to the flyout.
    /// </summary>
    public PopupAnchor PlacementAnchor
    {
        get => GetValue(PlacementAnchorProperty);
        set => SetValue(PlacementAnchorProperty, value);
    }

    /// <summary>
    /// The gap between the flyout and the button, in device-independent pixels. The value is forwarded to
    /// the flyout. The theme supplies the <c>MarginToAnchor</c> token of the dropdown button; without it the
    /// value is <c>0</c>.
    /// </summary>
    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    /// <summary>
    /// The time, in milliseconds, that the pointer must rest on the button before the flyout opens. <c>0</c> opens
    /// the flyout at once. It takes effect with the hover trigger. The default is <c>200</c>.
    /// </summary>
    public int MouseEnterDelay
    {
        get => GetValue(MouseEnterDelayProperty);
        set => SetValue(MouseEnterDelayProperty, value);
    }

    /// <summary>
    /// The time, in milliseconds, between the pointer leaving the button and the flyout closing. <c>0</c> closes
    /// the flyout at once. It takes effect with the hover trigger. The default is <c>200</c>.
    /// </summary>
    public int MouseLeaveDelay
    {
        get => GetValue(MouseLeaveDelayProperty);
        set => SetValue(MouseLeaveDelayProperty, value);
    }
    
    /// <summary>
    /// Shows the <see cref="OpenIndicator" /> icon in the button. The default is <see langword="true" />.
    /// </summary>
    public bool IsShowOpenIndicator
    {
        get => GetValue(IsShowOpenIndicatorProperty);
        set => SetValue(IsShowOpenIndicatorProperty, value);
    }
    
    /// <summary>
    /// The icon that marks the button as one that opens a menu. It is visible only while
    /// <see cref="IsShowOpenIndicator" /> is <see langword="true" />. When it is left unset, the button sets it to
    /// <c>DownOutlined</c> when the button is initialized.
    /// </summary>
    public PathIcon? OpenIndicator
    {
        get => GetValue(OpenIndicatorProperty);
        set => SetValue(OpenIndicatorProperty, value);
    }

    /// <summary>
    /// Shows the flyout in the overlay layer of the button's window instead of a native popup window. The value is
    /// forwarded to the flyout. The default is <see langword="true" />.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Makes the flyout at least as wide as the button. The name follows <c>IsPopupMatchSelectWidth</c> of the other
    /// popup controls such as <c>AbstractSelect</c>. The default is <see langword="false" />.
    /// </summary>
    public bool IsPopupMatchSelectWidth
    {
        get => GetValue(IsPopupMatchSelectWidthProperty);
        set => SetValue(IsPopupMatchSelectWidthProperty, value);
    }

    /// <summary>
    /// Keeps the dropdown flyout open. While it is <see langword="true" />, the flyout ignores light-dismiss close
    /// requests, and setting it while the button is attached opens the flyout without delay. Setting it to
    /// <see langword="false" /> cancels an open that is still queued. The Gallery uses it to keep the
    /// <c>popup.*</c> semantic parts of a preview highlighted.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="MenuItemClicked" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<FlyoutMenuItemClickedEventArgs> MenuItemClickedEvent =
        RoutedEvent.Register<DropdownButton, FlyoutMenuItemClickedEventArgs>(
            nameof(MenuItemClicked),
            RoutingStrategies.Bubble);
    
    /// <summary>
    /// Raised when an item of the dropdown flyout is clicked. The event bubbles up from the button, and
    /// <see cref="FlyoutMenuItemClickedEventArgs.Item" /> holds the clicked item.
    /// </summary>
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

    /// <summary>
    /// Called when the button is initialized. It sets <see cref="OpenIndicator" /> to <c>DownOutlined</c> when the
    /// property has no value. Call the base member first.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (OpenIndicator == null)
        {
            SetCurrentValue(OpenIndicatorProperty, new DownOutlined());
        }
    }
    
    /// <summary>
    /// Called when the button is attached to the visual tree. It hands the attachment to the flyout state helper
    /// that drives the trigger and registers <see cref="DropdownFlyout" />. Call the base member first; an
    /// override that skips this method leaves the flyout unregistered and no trigger active.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _flyoutStateHelper.NotifyAttachedToVisualTree();
        RegisterDropdownFlyout(DropdownFlyout);
    }

    /// <summary>
    /// Called when the button is detached from the visual tree. It hands the detachment to the flyout state helper
    /// and unregisters the current <see cref="DropdownFlyout" />, which closes the flyout and removes its bindings
    /// to this button. Call the base member first.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _flyoutStateHelper.NotifyDetachedFromVisualTree();
        UnregisterDropdownFlyout(_registeredDropdownFlyout);
    }
    
    private void RegisterDropdownFlyout(MenuFlyout? menuFlyout)
    {
        if (menuFlyout is null || ReferenceEquals(_registeredDropdownFlyout, menuFlyout) || _flyoutBindingDisposables?.IsDisposed == true)
        {
            return;
        }
        UnregisterDropdownFlyout(_registeredDropdownFlyout);
        if (!ReferenceEquals(menuFlyout, DropdownFlyout))
        {
            RegisterDropdownFlyout(DropdownFlyout);
            return;
        }
        var bindings = new CompositeDisposable(14);
        var registration = new CompositeDisposable(1);
        registration.Add(Disposable.Create(() =>
        {
            try
            {
                menuFlyout.CloseForLifecycle();
            }
            finally
            {
                try { bindings.Dispose(); }
                finally { menuFlyout.ClearMotionConfigurationOwner(this); }
            }
        }));
        _registeredDropdownFlyout = menuFlyout;
        _flyoutBindingDisposables = registration;
        try
        {
            menuFlyout.SetMotionConfigurationOwner(this);
            if (registration.IsDisposed)
            {
                return;
            }
            menuFlyout.MenuItemClicked += HandleMenuItemClicked;
            menuFlyout.Opening += HandleMenuFlyoutOpening;
            menuFlyout.Opened += HandleMenuFlyoutOpened;
            menuFlyout.Closed += HandleMenuFlyoutClosed;
            Bind(() => BindUtils.RelayBind(this, OpenMotionProperty, menuFlyout, AtomUI.Desktop.Controls.Flyout.OpenMotionProperty));
            Bind(() => BindUtils.RelayBind(this, CloseMotionProperty, menuFlyout, AtomUI.Desktop.Controls.Flyout.CloseMotionProperty));
            Bind(() => BindUtils.RelayBind(this, PlacementProperty, menuFlyout, FlyoutControl.RequestedPlacementProperty));
            Bind(() => BindUtils.RelayBind(this, PlacementAnchorProperty, menuFlyout));
            Bind(() => BindUtils.RelayBind(this, PlacementGravityProperty, menuFlyout));
            Bind(() => BindUtils.RelayBind(this, IsArrowVisibleProperty, menuFlyout));
            Bind(() => BindUtils.RelayBind(this, IsPointAtCenterProperty, menuFlyout));
            Bind(() => BindUtils.RelayBind(this, MarginToAnchorProperty, menuFlyout));
            Bind(() => BindUtils.RelayBind(this, IsMotionEnabledProperty, menuFlyout));
            Bind(() => BindUtils.RelayBind(this, ShouldUseOverlayPopupProperty, menuFlyout, MenuFlyout.ShouldUseOverlayPopupProperty));
            Bind(() => BindUtils.RelayBind(this, IsPopupPinnedOpenProperty, menuFlyout, FlyoutControl.IsPopupPinnedOpenProperty));
            if (!registration.IsDisposed && IsPopupPinnedOpen && this.IsAttachedToVisualTree())
            {
                QueuePinnedOpen();
            }
        }
        catch
        {
            if (ReferenceEquals(_flyoutBindingDisposables, registration)) { UnregisterDropdownFlyout(menuFlyout); }
            else { registration.Dispose(); }
            throw;
        }

        // Only replaceable runtime targets need a scoped code relay. A retired
        // lease cannot acquire more bindings after a synchronous user callback.
        void Bind(Func<IDisposable> acquire)
        {
            if (!registration.IsDisposed)
            {
                bindings.Add(acquire());
            }
        }
    }

    private void UnregisterDropdownFlyout(MenuFlyout? menuFlyout)
    {
        if (menuFlyout is null || !ReferenceEquals(_registeredDropdownFlyout, menuFlyout))
        {
            return;
        }
        menuFlyout.MenuItemClicked -= HandleMenuItemClicked;
        menuFlyout.Opening -= HandleMenuFlyoutOpening;
        menuFlyout.Opened -= HandleMenuFlyoutOpened;
        menuFlyout.Closed -= HandleMenuFlyoutClosed;
        ++_pinnedOpenGeneration;
        var registration = _flyoutBindingDisposables!;
        _registeredDropdownFlyout = null;
        SetCurrentValue(IsExternalPopupOpenProperty, false);
        try
        {
            registration.Dispose();
        }
        finally
        {
            if (ReferenceEquals(_flyoutBindingDisposables, registration))
            {
                _flyoutBindingDisposables = null;
            }
        }
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

    private void HandleMenuFlyoutOpened(object? sender, EventArgs e)
    {
        if (sender is MenuFlyout menuFlyout && ReferenceEquals(menuFlyout.Target, this))
        {
            SetCurrentValue(IsExternalPopupOpenProperty, true);
        }
    }

    private void HandleMenuFlyoutClosed(object? sender, EventArgs e)
    {
        SetCurrentValue(IsExternalPopupOpenProperty, false);
    }

    /// <summary>
    /// Called when a property of the button changes. It replaces the registered flyout when
    /// <see cref="DropdownFlyout" /> changes, opens or unpins the flyout when <see cref="IsPopupPinnedOpen" />
    /// changes, and updates the content visibility when <c>Content</c>, <c>ContentTemplate</c>, <c>IsLoading</c>
    /// or <c>Icon</c> changes. Call the base member first.
    /// </summary>
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

    /// <summary>
    /// Called when the control template is applied. It fills an unset <c>RightExtraContent</c> with an empty
    /// <see cref="Border" /> and updates the content visibility. Call the base member first.
    /// </summary>
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
