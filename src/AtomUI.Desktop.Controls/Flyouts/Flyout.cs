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

public class Flyout : PopupFlyoutBase, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<BoxShadows> PopupRootShadowProperty =
        PopupControl.PopupRootShadowProperty.AddOwner<Flyout>();

    public static readonly StyledProperty<BoxShadows> OverlayHostShadowProperty =
        PopupControl.OverlayHostShadowProperty.AddOwner<Flyout>();

    /// <summary>
    /// 是否显示指示箭头
    /// </summary>
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<Flyout>();

    public static readonly StyledProperty<double> MarginToAnchorProperty =
        PopupControl.MarginToAnchorProperty.AddOwner<Flyout>();

    /// <summary>
    /// 如果要启用 AtomUI 的自定义定位功能（MarginToAnchor、箭头、翻转等），
    /// 请使用该属性而不是 <see cref="PopupFlyoutBase.Placement"/>。
    /// 为 null 时 Flyout 走 Avalonia 默认定位逻辑。
    /// </summary>
    public static readonly StyledProperty<PlacementMode?> RequestedPlacementProperty =
        PopupControl.RequestedPlacementProperty.AddOwner<Flyout>();

    /// <summary>
    /// 箭头是否始终指向中心
    /// </summary>
    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        AvaloniaProperty.Register<Flyout, bool>(nameof(IsPointAtCenter));

    public static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<Flyout, object>(nameof(Content));

    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        PopupControl.OpenMotionProperty.AddOwner<Flyout>();

    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        PopupControl.CloseMotionProperty.AddOwner<Flyout>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Flyout>();

    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<Flyout>();

    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<Flyout, bool>(nameof(ShouldUseOverlayPopup), true);

    public static readonly StyledProperty<bool> IsLightDismissEnabledProperty =
        AvaloniaPopup.IsLightDismissEnabledProperty.AddOwner<Flyout>();

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

    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    public PlacementMode? RequestedPlacement
    {
        get => GetValue(RequestedPlacementProperty);
        set => SetValue(RequestedPlacementProperty, value);
    }

    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    [Content]
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
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

    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

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

    protected override bool ShowAtCore(Control placementTarget, bool showAtPointer = false)
    {
        EnsureGlobalResourceBindings(placementTarget);
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
        return base.ShowAtCore(placementTarget, showAtPointer);
    }

    protected override void OnClosed()
    {
        base.OnClosed();
        ReleaseGlobalResourceBindings();
    }

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
            ConfigurePointerPlacementOffsets();
        }
    }

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
    /// Bind <see cref="PopupFlyoutBase.HorizontalOffsetProperty"/> / <see cref="PopupFlyoutBase.VerticalOffsetProperty"/>
    /// to the given tokens only while <see cref="RequestedPlacement"/> is <see cref="PlacementMode.Pointer"/>.
    /// For non-pointer placements the offsets stay at their default values so edge-aligned placements
    /// (e.g. <c>BottomEdgeAlignedRight</c>) aren't shifted off anchor.
    /// </summary>
    protected void BindPointerPlacementOffsets(object horizontalOffsetTokenKey, object verticalOffsetTokenKey)
    {
        _pointerHorizontalOffsetTokenKey = horizontalOffsetTokenKey;
        _pointerVerticalOffsetTokenKey   = verticalOffsetTokenKey;
        ConfigurePointerPlacementOffsets();
    }

    #endregion

    #region 内部协作 API

    internal virtual AbstractMotion GetDefaultOpenMotion() => PopupMotionDefaults.Create(PopupMotionDefaults.PopoverOpen);

    internal virtual AbstractMotion GetDefaultCloseMotion() => PopupMotionDefaults.Create(PopupMotionDefaults.PopoverClose);

    internal void ReleaseGlobalResourceBindings()
    {
        _pointerHorizontalOffsetBinding?.Dispose();
        _pointerVerticalOffsetBinding?.Dispose();
        _pointerHorizontalOffsetBinding = null;
        _pointerVerticalOffsetBinding   = null;

        _globalResourceBindingDisposables?.Dispose();
        _globalResourceBindingDisposables = null;
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

        ReleaseGlobalResourceBindings();
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

    private void EnsureGlobalResourceBindings(Control context)
    {
        if (_globalResourceBindingDisposables is not null)
        {
            return;
        }

        _globalResourceBindingDisposables = new CompositeDisposable(3)
        {
            TokenResourceBinder.CreateControlTokenBinding(
                typeof(FlyoutHost),
                context,
                this,
                PopupRootShadowProperty,
                FlyoutHostTokenKind.PopupRootShadow),
            TokenResourceBinder.CreateControlTokenBinding(
                typeof(FlyoutHost),
                context,
                this,
                OverlayHostShadowProperty,
                FlyoutHostTokenKind.OverlayHostShadow),
            TokenResourceBinder.CreateControlTokenBinding(
                typeof(FlyoutHost),
                context,
                this,
                MotionDurationProperty,
                SharedTokenKind.MotionDurationMid)
        };
        ConfigurePointerPlacementOffsets();
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

    /// <remarks>
    /// <see cref="PopupFlyoutBase.ShowAtCore"/> 内部的 <c>PositionPopup</c> 会在每次打开时把
    /// <see cref="AvaloniaPopup.Placement"/> 和 <see cref="AvaloniaPopup.CustomPopupPlacementCallback"/>
    /// 强制覆盖为 Flyout 侧的值，导致 AtomUI 自定义定位链路失效。这里在 <see cref="RequestedPlacement"/>
    /// 非空时把 Popup 的相关属性重新拉回 Custom 模式。
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
        ReleaseGlobalResourceBindings();
    }

    private void ApplyLightDismissConfiguration(PopupControl popup)
    {
        popup.SetCurrentValue(
            AvaloniaPopup.IsLightDismissEnabledProperty,
            IsLightDismissEnabled);
    }

    private void ConfigurePointerPlacementOffsets()
    {
        _pointerHorizontalOffsetBinding?.Dispose();
        _pointerVerticalOffsetBinding?.Dispose();
        _pointerHorizontalOffsetBinding = null;
        _pointerVerticalOffsetBinding   = null;

        if (_globalResourceBindingDisposables is null)
        {
            return;
        }

        if (RequestedPlacement == PlacementMode.Pointer &&
            _pointerHorizontalOffsetTokenKey != null &&
            _pointerVerticalOffsetTokenKey != null)
        {
            _pointerHorizontalOffsetBinding = TokenResourceBinder.CreateGlobalResourceBinding(
                this, HorizontalOffsetProperty, _pointerHorizontalOffsetTokenKey, BindingPriority.Style);
            _pointerVerticalOffsetBinding = TokenResourceBinder.CreateGlobalResourceBinding(
                this, VerticalOffsetProperty, _pointerVerticalOffsetTokenKey, BindingPriority.Style);
        }
    }

    #endregion
}
