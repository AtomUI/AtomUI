using System.ComponentModel;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Animations;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using PopupControl = Popup;
using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;

/// <summary>
/// A flyout that shows <see cref="Content" /> in a <see cref="FlyoutPresenter" /> hosted by an AtomUI
/// <c>Popup</c>, which adds an anchor arrow, AtomUI placement with flipping, open and close motions and shadows.
/// </summary>
public class Flyout : PopupFlyoutBase, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="PopupRootShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> PopupRootShadowProperty =
        PopupControl.PopupRootShadowProperty.AddOwner<Flyout>();

    /// <summary>
    /// Identifies the <see cref="OverlayHostShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> OverlayHostShadowProperty =
        PopupControl.OverlayHostShadowProperty.AddOwner<Flyout>();

    /// <summary>
    /// Identifies the <see cref="IsArrowVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<Flyout>();

    /// <summary>
    /// Identifies the <see cref="MarginToAnchor" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MarginToAnchorProperty =
        PopupControl.MarginToAnchorProperty.AddOwner<Flyout>();

    /// <summary>
    /// Identifies the <see cref="RequestedPlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<PlacementMode?> RequestedPlacementProperty =
        PopupControl.RequestedPlacementProperty.AddOwner<Flyout>();

    /// <summary>
    /// Identifies the <see cref="IsPointAtCenter" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        AvaloniaProperty.Register<Flyout, bool>(nameof(IsPointAtCenter));

    /// <summary>
    /// Identifies the <see cref="Content" /> property.
    /// </summary>
    public static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<Flyout, object>(nameof(Content));

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        PopupControl.OpenMotionProperty.AddOwner<Flyout>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        PopupControl.CloseMotionProperty.AddOwner<Flyout>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Flyout>();

    /// <summary>
    /// Identifies the <see cref="MotionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<Flyout>();

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<Flyout, bool>(nameof(ShouldUseOverlayPopup), true);

    /// <summary>
    /// Identifies the <see cref="IsLightDismissEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLightDismissEnabledProperty =
        AvaloniaPopup.IsLightDismissEnabledProperty.AddOwner<Flyout>();

    /// <summary>
    /// Shadow of the flyout while its popup is a native top-level window rather than the overlay layer, where
    /// <see cref="OverlayHostShadow" /> applies. While the flyout is shown, the value follows the PopupRootShadow
    /// token of <see cref="FlyoutHost" /> in the placement target's theme scope unless it is set explicitly.
    /// </summary>
    public BoxShadows PopupRootShadow
    {
        get => GetValue(PopupRootShadowProperty);
        set => SetValue(PopupRootShadowProperty, value);
    }

    /// <summary>
    /// Shadow of the flyout while its popup is shown in the overlay layer rather than as a native top-level
    /// window, where <see cref="PopupRootShadow" /> applies. While the flyout is shown, the value follows the
    /// OverlayHostShadow token of <see cref="FlyoutHost" /> in the placement target's theme scope unless it is
    /// set explicitly.
    /// </summary>
    public BoxShadows OverlayHostShadow
    {
        get => GetValue(OverlayHostShadowProperty);
        set => SetValue(OverlayHostShadowProperty, value);
    }

    /// <summary>
    /// Draws an arrow that points from the flyout to its placement target. The arrow is drawn only when the
    /// placement also supports one: <see cref="RequestedPlacement" />, or else
    /// <see cref="PopupFlyoutBase.Placement" />, together with the placement anchor and gravity.
    /// The default is <see langword="false" />.
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// Gap between the flyout and its placement target, in device-independent pixels. It applies to the AtomUI
    /// positioning that <see cref="RequestedPlacement" /> enables; the default is <c>0</c>.
    /// </summary>
    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    /// <summary>
    /// Placement mode of the AtomUI positioning, which adds <see cref="MarginToAnchor" />, the anchor arrow and
    /// flipping. Use it instead of <see cref="PopupFlyoutBase.Placement" /> to enable that positioning; while it
    /// has a value, <see cref="PopupFlyoutBase.Placement" /> reads as <see cref="PlacementMode.Custom" />.
    /// <see langword="null" /> leaves the flyout on Avalonia's default positioning.
    /// </summary>
    public PlacementMode? RequestedPlacement
    {
        get => GetValue(RequestedPlacementProperty);
        set => SetValue(RequestedPlacementProperty, value);
    }

    /// <summary>
    /// Moves the flyout so that its arrow points at the center of the placement target. It has an effect only
    /// with the AtomUI positioning of <see cref="RequestedPlacement" /> and only while the arrow is drawn.
    /// </summary>
    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    /// <summary>
    /// The object the flyout displays. It is bound one-way to the presenter created by <see cref="CreatePresenter" />
    /// and is the XAML content property, so markup children of the flyout become its value.
    /// </summary>
    [Content]
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Motion played when the flyout opens. It is forwarded to the popup. Without an explicit value it falls
    /// back to the popover open motion, or to what <see cref="FlyoutHost" /> supplies when it hosts the flyout.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// Motion played when the flyout closes. It is forwarded to the popup and is played only while
    /// <see cref="IsMotionEnabled" /> is <see langword="true" />; without such a motion, hiding removes the
    /// flyout at once. Without an explicit value it falls back to the popover close motion, or to what
    /// <see cref="FlyoutHost" /> supplies when it hosts the flyout.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// Switches the open and close motions on or off. When it is <see langword="false" />, hiding the flyout
    /// removes it at once instead of starting <see cref="CloseMotion" />. The value is forwarded to the popup and
    /// to the presenter.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Duration used when the open and close motions are played. While the flyout is shown, the value follows
    /// the shared MotionDurationMid token in the placement target's theme scope unless it is set explicitly.
    /// </summary>
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    /// <summary>
    /// Shows the flyout in the overlay layer of the target's window instead of a native top-level popup window.
    /// The overlay layer is also used on platforms without native popup windows. The host also decides which
    /// shadow applies: <see cref="OverlayHostShadow" /> in the overlay layer and <see cref="PopupRootShadow" />
    /// in a native window. The default is <see langword="true" />.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Closes the flyout when the user clicks or taps outside of it. The value is pushed to the popup when the
    /// flyout is shown and whenever it changes while the popup exists. The default is <see langword="true" />;
    /// <see cref="FlyoutStateHelper" /> sets it to <see langword="true" /> only when its trigger is Click.
    /// </summary>
    public bool IsLightDismissEnabled
    {
        get => GetValue(IsLightDismissEnabledProperty);
        set => SetValue(IsLightDismissEnabledProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Flyout, bool> IsArrowVisibleEffectiveProperty =
        AvaloniaProperty.RegisterDirect<Flyout, bool>(nameof(IsArrowVisibleEffective),
            o => o.IsArrowVisibleEffective,
            (o, v) => o.IsArrowVisibleEffective = v);

    internal static readonly DirectProperty<Flyout, bool> IsPopupHorizontalFlippedProperty =
        AvaloniaProperty.RegisterDirect<Flyout, bool>(nameof(IsPopupHorizontalFlipped),
            o => o.IsPopupHorizontalFlipped,
            (o, v) => o.IsPopupHorizontalFlipped = v);

    internal static readonly DirectProperty<Flyout, bool> IsPopupVerticalFlippedProperty =
        AvaloniaProperty.RegisterDirect<Flyout, bool>(nameof(IsPopupVerticalFlipped),
            o => o.IsPopupVerticalFlipped,
            (o, v) => o.IsPopupVerticalFlipped = v);

    internal static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<Flyout>();

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        PopupControl.IsPopupPinnedOpenProperty.AddOwner<Flyout>();

    private bool _isArrowVisibleEffective;

    internal bool IsArrowVisibleEffective
    {
        get => _isArrowVisibleEffective;
        private set => SetAndRaise(IsArrowVisibleEffectiveProperty, ref _isArrowVisibleEffective, value);
    }

    private bool _isPopupHorizontalFlipped;

    internal bool IsPopupHorizontalFlipped
    {
        get => _isPopupHorizontalFlipped;
        private set => SetAndRaise(IsPopupHorizontalFlippedProperty, ref _isPopupHorizontalFlipped, value);
    }

    private bool _isPopupVerticalFlipped;

    internal bool IsPopupVerticalFlipped
    {
        get => _isPopupVerticalFlipped;
        private set => SetAndRaise(IsPopupVerticalFlippedProperty, ref _isPopupVerticalFlipped, value);
    }

    internal ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion

    #region 运行时字段

    private object? _pointerHorizontalOffsetTokenKey;

    private object? _pointerVerticalOffsetTokenKey;

    private CompositeDisposable? _globalResourceBindingDisposables;
    private OwnerTokenPropertyBindings? _resourceTokens;
    private Control? _resourcePlacementTarget;

    private IDisposable? _pointerHorizontalOffsetBinding;

    private IDisposable? _pointerVerticalOffsetBinding;

    private bool _hasPendingPinnedOpen;

    private PopupControl? _popupControl;

    private AvaloniaObject? _motionConfigurationOwner;

    private CompositeDisposable? _defaultMotionValues;

    #endregion

    #region 构造函数

    static Flyout()
    {
        // RequestedPlacement projects onto Custom without overwriting the
        // application's base Placement; releasing a host must restore it.
        PlacementProperty.OverrideMetadata<Flyout>(new StyledPropertyMetadata<PlacementMode>(
            coerce: static (sender, value) => ((Flyout)sender).RequestedPlacement.HasValue ? PlacementMode.Custom : value));
        IsArrowVisibleProperty.OverrideDefaultValue<Flyout>(false);
        IsLightDismissEnabledProperty.OverrideDefaultValue<Flyout>(true);
    }

    public Flyout()
    {
        this.SetPopupLazy(new Lazy<AvaloniaPopup>(CreatePopup));
    }

    #endregion

    #region 控件 API

    /// <summary>
    /// Cancels the opening while the flyout is in the pinned-open state and the popup cannot host a pinned popup
    /// at that moment, and holds the open request as pending.
    /// </summary>
    /// <remarks>
    /// A pending request keeps the placement token bindings of <see cref="ShowAtCore" /> until the popup opens or
    /// the pinned state is released. Overrides call the base method first; an opening that is already canceled
    /// is not held as pending.
    /// </remarks>
    protected override void OnOpening(CancelEventArgs args)
    {
        base.OnOpening(args);
        if (!args.Cancel &&
            IsPopupPinnedOpen &&
            Popup is PopupControl popup &&
            !popup.CanOpenPinnedPopup())
        {
            _hasPendingPinnedOpen = true;
            args.Cancel = true;
        }
    }

    /// <summary>
    /// Hides the flyout. When a close motion applies, it only starts that motion and the flyout is removed when
    /// the motion ends.
    /// </summary>
    /// <param name="canCancel">
    /// With <see langword="true" /> the hide is refused while the flyout is pinned open
    /// (<c>IsPopupPinnedOpen</c>). With <see langword="false" /> the popup host is closed first and the flyout
    /// is removed without a close motion.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the hide was accepted, including when the close motion was only started.
    /// <see langword="false" /> when the hide was refused or when the base implementation did not hide the
    /// flyout.
    /// </returns>
    protected override bool HideCore(bool canCancel = true)
    {
        if (!canCancel && _popupControl is { IsHostOpen: true } physicalPopup)
        {
            // Base teardown clears placement and parent immediately. Retire the
            // physical host first; a false IsOpen may still own a leaving host.
            physicalPopup.CloseForLifecycle();
        }

        if (canCancel && IsPopupPinnedOpen)
        {
            return false;
        }

        if (canCancel && Popup is PopupControl popup &&
            (popup.IsPlayingCloseMotion || IsMotionEnabled && CloseMotion is not null && popup.IsOpen))
        {
            popup.IsOpen = false;
            return true;
        }

        return base.HideCore(canCancel);
    }

    /// <summary>
    /// Shows the flyout at <paramref name="placementTarget" />, or at the pointer position when
    /// <paramref name="showAtPointer" /> is <see langword="true" />, after the flyout's token bindings are attached
    /// to that target's theme scope.
    /// </summary>
    /// <param name="placementTarget">
    /// The control the flyout is placed at and whose theme scope supplies the flyout's token values.
    /// </param>
    /// <param name="showAtPointer">
    /// <see langword="true" /> to place the flyout at the pointer position instead of the control.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the flyout is open. <see langword="false" /> when the open was refused, for
    /// example because the flyout is already open at the same target or the opening was canceled.
    /// </returns>
    /// <remarks>
    /// A flyout that is open at another control is hidden without cancellation before it is shown at the new
    /// target. A flyout that is playing its close motion for the same target reopens instead. The token bindings
    /// of the placement session are acquired here; they are released when the open is refused without a pending
    /// pinned open, and on close.
    /// </remarks>
    protected override bool ShowAtCore(Control placementTarget, bool showAtPointer = false)
    {
        ArgumentNullException.ThrowIfNull(placementTarget);
        if (IsOpen && !ReferenceEquals(Target, placementTarget))
        {
            // Retire the old session before acquiring the new target's bindings; its OnClosed
            // cleanup must not dispose resources belonging to the next positioning session.
            _ = HideCore(canCancel: false);
        }
        EnsurePlacementTokenBindings(placementTarget);
        if (Popup is PopupControl popup)
        {
            popup.SetCurrentValue(PopupControl.IsPopupPinnedOpenProperty, IsPopupPinnedOpen);
            ApplyLightDismissConfiguration(popup);
            if (IsOpen && popup.IsPlayingCloseMotion && ReferenceEquals(popup.PlacementTarget, placementTarget))
            {
                popup.CancelCloseAnimation();
                return true;
            }
        }
        var opened = base.ShowAtCore(placementTarget, showAtPointer);
        if (!opened && !IsOpen && !_hasPendingPinnedOpen)
        {
            ReleasePlacementTokenBindings();
        }
        return opened;
    }

    /// <summary>
    /// Releases the placement token bindings and the pointer offset bindings of the closed session. Overrides
    /// call the base method so that these bindings are released.
    /// </summary>
    protected override void OnClosed()
    {
        base.OnClosed();
        ReleasePlacementTokenBindings();
    }

    /// <summary>
    /// Creates the <see cref="FlyoutPresenter" /> hosted by the popup. It carries the popup root semantic part
    /// class and is bound one-way to the flyout's <see cref="Content" />, <see cref="IsMotionEnabled" />,
    /// effective arrow visibility and arrow position.
    /// </summary>
    protected override Control CreatePresenter()
    {
        var presenter = new FlyoutPresenter();
        presenter.Classes.Add(FlyoutHostSemanticParts.PopupRootClass);
        presenter[!FlyoutPresenter.ContentProperty]         = this[!ContentProperty];
        presenter[!FlyoutPresenter.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        presenter[!FlyoutPresenter.IsArrowVisibleProperty]     = this[!IsArrowVisibleEffectiveProperty];
        presenter[!FlyoutPresenter.ArrowPositionProperty]   = this[!ArrowPositionProperty];
        ConfigureShowArrowEffective();
        ConfigureArrowPosition();
        return presenter;
    }

    /// <summary>
    /// Keeps the popup and the arrow in step with property changes: it pushes the light dismiss setting to the
    /// popup, updates the effective arrow visibility and the arrow position, and, when
    /// <see cref="RequestedPlacement" /> changes, re-evaluates <see cref="PopupFlyoutBase.Placement" /> and the
    /// pointer offset bindings. Overrides call the base method to keep this in place.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsPopupPinnedOpenProperty &&
            !change.GetNewValue<bool>())
        {
            CancelPendingPinnedOpen();
        }

        if ((change.Property == IsPopupPinnedOpenProperty ||
             change.Property == IsLightDismissEnabledProperty) &&
            Popup is PopupControl popup)
        {
            ApplyLightDismissConfiguration(popup);
        }

        if (change.Property == IsArrowVisibleProperty ||
            change.Property == PlacementProperty ||
            change.Property == RequestedPlacementProperty ||
            change.Property == PlacementAnchorProperty ||
            change.Property == PlacementGravityProperty)
        {
            ConfigureShowArrowEffective();
        }

        if (change.Property == PlacementProperty ||
            change.Property == RequestedPlacementProperty ||
            change.Property == IsPopupHorizontalFlippedProperty ||
            change.Property == IsPopupVerticalFlippedProperty)
        {
            ConfigureArrowPosition();
        }

        if (change.Property == RequestedPlacementProperty)
        {
            CoerceValue(PlacementProperty);
            UpdatePointerOffsetTokenBindings();
        }
    }

    /// <summary>
    /// Updates the effective arrow visibility, which is <see langword="true" /> only while
    /// <see cref="IsArrowVisible" /> is <see langword="true" /> and <see cref="RequestedPlacement" />, or else
    /// <see cref="PopupFlyoutBase.Placement" />, supports an arrow for the placement anchor and gravity. The
    /// result is bound to the presenter. Derived flyouts call it when they change one of these inputs themselves.
    /// </summary>
    protected void ConfigureShowArrowEffective()
    {
        if (!IsArrowVisible)
        {
            SetCurrentValue(IsArrowVisibleEffectiveProperty, false);
        }
        else
        {
            var placement = RequestedPlacement ?? Placement;
            SetCurrentValue(IsArrowVisibleEffectiveProperty, PopupUtils.CanEnabledArrow(placement, PlacementAnchor, PlacementGravity));
        }
    }

    /// <summary>
    /// Updates the side of the presenter that carries the arrow, derived from <see cref="RequestedPlacement" />,
    /// or else <see cref="PopupFlyoutBase.Placement" />, the placement anchor and gravity, and mirrored when the
    /// popup reports a horizontal or vertical flip. A placement without an arrow position leaves the value
    /// unchanged. The result is bound to the presenter. Derived flyouts call it when they change one of these
    /// inputs themselves.
    /// </summary>
    protected void ConfigureArrowPosition()
    {
        var placement     = RequestedPlacement ?? Placement;
        var arrowPosition = PopupUtils.CalculateArrowPosition(placement, PlacementAnchor, PlacementGravity);
        if (arrowPosition.HasValue)
        {
            arrowPosition = ArrowPositionUtils.FlipArrowPosition(arrowPosition.Value, IsPopupHorizontalFlipped, IsPopupVerticalFlipped);
            SetCurrentValue(ArrowPositionProperty, arrowPosition);
        }
    }

    /// <summary>
    /// Binds <see cref="PopupFlyoutBase.HorizontalOffsetProperty" /> and
    /// <see cref="PopupFlyoutBase.VerticalOffsetProperty" /> to the given resource keys only while
    /// <see cref="RequestedPlacement" /> is <see cref="PlacementMode.Pointer" />.
    /// FlyoutHost Own token keys are normalized to that control identity; Shared, custom and already-typed keys
    /// retain their meaning in the placement target's resource scope.
    /// For non-pointer placements the offsets stay at their default values so edge-aligned placements (for
    /// example <c>BottomEdgeAlignedRight</c>) are not shifted off anchor.
    /// </summary>
    protected void BindPointerPlacementOffsets(object horizontalOffsetTokenKey, object verticalOffsetTokenKey)
    {
        _pointerHorizontalOffsetTokenKey = horizontalOffsetTokenKey;
        _pointerVerticalOffsetTokenKey   = verticalOffsetTokenKey;
        UpdatePointerOffsetTokenBindings();
    }

    #endregion

    #region 内部协作 API

    internal virtual (object PopupRoot, object OverlayHost) GetPopupShadowTokenResourceKeys() =>
        (ControlTokenResourceKey.Own(typeof(FlyoutHost), FlyoutHostTokenKind.PopupRootShadow),
         ControlTokenResourceKey.Own(typeof(FlyoutHost), FlyoutHostTokenKind.OverlayHostShadow));

    /// <summary>
    /// Returns the popover open motion. The flyout installs it as the default <see cref="OpenMotion" />, below a
    /// value set directly on the property, when it creates its popup and no host has taken over the configuration of
    /// the popup. Derived flyouts override it to start with another motion; they need not call the base
    /// implementation.
    /// </summary>
    internal virtual AbstractMotion GetDefaultOpenMotion() => PopupMotionDefaults.Create(PopupMotionDefaults.PopoverOpen);

    /// <summary>
    /// Returns the popover close motion. The flyout installs it as the default <see cref="CloseMotion" />, below a
    /// value set directly on the property, when it creates its popup and no host has taken over the configuration of
    /// the popup. Derived flyouts override it to start with another motion; they need not call the base
    /// implementation.
    /// </summary>
    internal virtual AbstractMotion GetDefaultCloseMotion() => PopupMotionDefaults.Create(PopupMotionDefaults.PopoverClose);

    internal void ReleasePlacementTokenBindings()
    {
        if (_resourcePlacementTarget is { } target)
        {
            target.ResourcesChanged -= HandlePlacementTargetResourcesChanged;
        }
        _pointerHorizontalOffsetBinding?.Dispose();
        _pointerVerticalOffsetBinding?.Dispose();
        _pointerHorizontalOffsetBinding = null;
        _pointerVerticalOffsetBinding   = null;

        _globalResourceBindingDisposables?.Dispose();
        _globalResourceBindingDisposables = null;
        _resourceTokens?.Dispose();
        _resourceTokens = null;
        _resourcePlacementTarget = null;
    }

    internal void CloseForLifecycle()
    {
        if (_popupControl is { IsHostOpen: true } physicalPopup)
        {
            physicalPopup.CloseForLifecycle();
        }

        if (IsOpen)
        {
            base.HideCore(canCancel: false);
        }
        else
        {
            CancelPendingPinnedOpen();
        }

        ReleasePlacementTokenBindings();
    }

    internal void SetMotionConfigurationOwner(AvaloniaObject? owner)
    {
        if (ReferenceEquals(_motionConfigurationOwner, owner))
        {
            return;
        }
        _motionConfigurationOwner = owner;
        ConfigureDefaultMotionValues();
    }

    internal void ClearMotionConfigurationOwner(AvaloniaObject owner)
    {
        if (ReferenceEquals(_motionConfigurationOwner, owner))
        {
            if (_popupControl?.IsHostOpen != true)
            {
                this.ClearDefaultPlacementTarget();
                _popupControl?.ReleaseDetachedContentCompositionCaches();
            }
            SetMotionConfigurationOwner(null);
        }
    }

    #endregion

    #region 内部实现

    private void EnsurePlacementTokenBindings(Control context)
    {
        if (ReferenceEquals(_resourcePlacementTarget, context))
        {
            return;
        }

        ReleasePlacementTokenBindings();
        // The Flyout is non-visual. Its values follow the runtime placement target's theme scope.
        var tokens = new OwnerTokenPropertyBindings(context, 5, BindingPriority.Template);
        var bindings = new CompositeDisposable(3);
        _resourceTokens = tokens;
        _resourcePlacementTarget = context;
        _globalResourceBindingDisposables = bindings;
        try
        {
            var (popupRootKey, overlayHostKey) = GetPopupShadowTokenResourceKeys();
            var popupRoot = tokens.Observe(0, popupRootKey);
            var overlayHost = Equals(popupRootKey, overlayHostKey) ? popupRoot : tokens.Observe(1, overlayHostKey);
            bindings.Add(Bind(PopupRootShadowProperty, popupRoot, BindingPriority.Template));
            bindings.Add(Bind(OverlayHostShadowProperty, overlayHost, BindingPriority.Template));
            bindings.Add(Bind(MotionDurationProperty,
                tokens.Observe(2, ControlTokenResourceKey.Global(typeof(FlyoutHost), SharedTokenKind.MotionDurationMid)),
                BindingPriority.Template));
            context.ResourcesChanged += HandlePlacementTargetResourcesChanged;
            UpdatePointerOffsetTokenBindings();
        }
        catch
        {
            ReleasePlacementTokenBindings();
            throw;
        }
    }

    private void HandlePlacementTargetResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        // PopupFlyoutBase sets the placement target as the popup's logical parent without adding it to LogicalChildren.
        // Resource lookup follows that parent, but the popup needs the placement session to relay change notifications.
        if (_popupControl is { } popup && ReferenceEquals(popup.Parent, sender))
        {
            ((IResourceHost)popup).NotifyHostedResourcesChanged(e);
        }
    }

    private Popup CreatePopup()
    {
        var popup = new PopupControl
        {
            WindowManagerAddShadowHint = false,
        };
        _popupControl = popup;
        // This non-visual Flyout has no ControlTheme; install independently
        // created defaults at Style priority before forwarding configuration.
        ConfigureDefaultMotionValues();

        popup[!PopupControl.RequestedPlacementProperty]     = this[!RequestedPlacementProperty];
        popup[!PopupControl.PlacementAnchorProperty]        = this[!PlacementAnchorProperty];
        popup[!PopupControl.PlacementGravityProperty]       = this[!PlacementGravityProperty];
        popup[!PopupControl.PopupRootShadowProperty]        = this[!PopupRootShadowProperty];
        popup[!PopupControl.OverlayHostShadowProperty]      = this[!OverlayHostShadowProperty];
        popup[!PopupControl.MotionDurationProperty]         = this[!MotionDurationProperty];
        popup[!PopupControl.OpenMotionProperty]             = this[!OpenMotionProperty];
        popup[!PopupControl.CloseMotionProperty]            = this[!CloseMotionProperty];
        popup[!PopupControl.IsMotionEnabledProperty]        = this[!IsMotionEnabledProperty];
        popup[!PopupControl.MarginToAnchorProperty]         = this[!MarginToAnchorProperty];
        popup[!PopupControl.IsPointAtCenterProperty]        = this[!IsPointAtCenterProperty];
        popup[!PopupControl.IsPopupPinnedOpenProperty]      = this[!IsPopupPinnedOpenProperty];
        popup[!PopupControl.ShouldUseOverlayLayerProperty]  = this[!ShouldUseOverlayPopupProperty];
        this[!IsPopupHorizontalFlippedProperty]             = popup[!PopupControl.IsHorizontalFlippedProperty];
        this[!IsPopupVerticalFlippedProperty]               = popup[!PopupControl.IsVerticalFlippedProperty];
        ApplyLightDismissConfiguration(popup);

        popup.Opened += HandlePopupOpened;
        popup.Opened += this.OnPopupOpened;
        popup.Closed += this.OnPopupClosed;
        popup.CloseRequested += HandlePopupClosing;
        popup.KeyUp += this.OnPlacementTargetOrPopupKeyUp;
        popup.PropertyChanged += HandlePopupPropertyChanged;

        if (CustomPopupPlacementCallback == null)
        {
            CustomPopupPlacementCallback = popup.HandleCustomPlacement;
        }

        return popup;
    }

    private void ConfigureDefaultMotionValues()
    {
        var previous = _defaultMotionValues;
        _defaultMotionValues = null;
        previous?.Dispose();
        if (_motionConfigurationOwner is not null || _popupControl is null || _defaultMotionValues is not null)
        {
            return;
        }

        // Flyout is non-visual and has no ControlTheme. These Style-priority
        // defaults exist only while it owns configuration; a host takeover
        // disposes them before installing its one-way configuration relay.
        var bindings = new CompositeDisposable(2);
        _defaultMotionValues = bindings;
        try
        {
            bindings.Add(SetValue(OpenMotionProperty, GetDefaultOpenMotion(), BindingPriority.Style)!);
            if (ReferenceEquals(_defaultMotionValues, bindings))
            {
                bindings.Add(SetValue(CloseMotionProperty, GetDefaultCloseMotion(), BindingPriority.Style)!);
            }
        }
        catch
        {
            if (ReferenceEquals(_defaultMotionValues, bindings))
            {
                _defaultMotionValues = null;
            }
            bindings.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Restores the popup's custom placement callback when another change replaces it while
    /// <see cref="RequestedPlacement" /> is set.
    /// </summary>
    /// <remarks>
    /// <c>PositionPopup</c> inside <see cref="PopupFlyoutBase.ShowAtCore" /> overwrites
    /// <see cref="AvaloniaPopup.Placement" /> and <see cref="AvaloniaPopup.CustomPopupPlacementCallback" /> with
    /// the flyout-side values every time the flyout opens, which disables the AtomUI positioning. Here the
    /// callback is set back to the popup's own handler, which keeps the popup in Custom mode.
    /// </remarks>
    private void HandlePopupPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not PopupControl popup || !RequestedPlacement.HasValue)
        {
            return;
        }

        if (e.Property == AvaloniaPopup.CustomPopupPlacementCallbackProperty)
        {
            if (e.GetNewValue<CustomPopupPlacementCallback?>() != popup.HandleCustomPlacement)
            {
                popup.CustomPopupPlacementCallback = popup.HandleCustomPlacement;
            }
        }
    }

    private void HandlePopupClosing(object? sender, CancelEventArgs e)
    {
        if (!e.Cancel)
        {
            this.OnPopupClosing(sender, e);
        }
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        _hasPendingPinnedOpen = false;
    }

    private void CancelPendingPinnedOpen()
    {
        if (!_hasPendingPinnedOpen)
        {
            return;
        }

        _hasPendingPinnedOpen = false;
        if (Popup is PopupControl popup)
        {
            popup.CloseForLifecycle();
            popup.PlacementTarget = null;
            popup.SetPopupParent(null);
        }
        Target = null;
        ReleasePlacementTokenBindings();
    }

    private void ApplyLightDismissConfiguration(PopupControl popup)
    {
        popup.SetCurrentValue(
            AvaloniaPopup.IsLightDismissEnabledProperty,
            IsLightDismissEnabled);
    }

    private void UpdatePointerOffsetTokenBindings()
    {
        _pointerHorizontalOffsetBinding?.Dispose();
        _pointerVerticalOffsetBinding?.Dispose();
        _pointerHorizontalOffsetBinding = null;
        _pointerVerticalOffsetBinding   = null;

        if (_resourceTokens is not { } tokens)
        {
            return;
        }

        if (RequestedPlacement == PlacementMode.Pointer &&
            _pointerHorizontalOffsetTokenKey != null &&
            _pointerVerticalOffsetTokenKey != null)
        {
            // Pointer offsets are configured on this non-visual Flyout, so AXAML cannot bind them to
            // the runtime placement target. The open-session bindings are released on close / detach.
            var horizontalKey = _pointerHorizontalOffsetTokenKey is FlyoutHostTokenKind horizontalToken
                ? ControlTokenResourceKey.Own(typeof(FlyoutHost), horizontalToken) : _pointerHorizontalOffsetTokenKey;
            var verticalKey = _pointerVerticalOffsetTokenKey is FlyoutHostTokenKind verticalToken
                ? ControlTokenResourceKey.Own(typeof(FlyoutHost), verticalToken) : _pointerVerticalOffsetTokenKey;
            _pointerHorizontalOffsetBinding = this.Bind(HorizontalOffsetProperty,
                tokens.Observe(3, horizontalKey), BindingPriority.Style);
            _pointerVerticalOffsetBinding = this.Bind(VerticalOffsetProperty,
                tokens.Observe(4, verticalKey), BindingPriority.Style);
        }
        else
        {
            tokens.ClearKey(3);
            tokens.ClearKey(4);
        }
    }

    #endregion
}
