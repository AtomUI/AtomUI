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
    
    private object? _pointerHorizontalOffsetTokenKey;
    private object? _pointerVerticalOffsetTokenKey;
    private CompositeDisposable? _globalResourceBindingDisposables;
    private IDisposable? _pointerHorizontalOffsetBinding;
    private IDisposable? _pointerVerticalOffsetBinding;
    private bool _hasPendingPinnedOpen;

    static Flyout()
    {
        IsArrowVisibleProperty.OverrideDefaultValue<Flyout>(false);
        IsLightDismissEnabledProperty.OverrideDefaultValue<Flyout>(true);
    }

    public Flyout()
    {
        this.SetPopupLazy(new Lazy<AvaloniaPopup>(CreatePopup));
    }

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
    
    private Popup CreatePopup()
    {
        var popup = new PopupControl
        {
            WindowManagerAddShadowHint = false,
        };

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
        popup.AddClosingEventHandler(HandlePopupClosing);
        popup.KeyUp += this.OnPlacementTargetOrPopupKeyUp;
        popup.PropertyChanged += HandlePopupPropertyChanged;
        
        if (CustomPopupPlacementCallback == null)
        {
            CustomPopupPlacementCallback = popup.HandleCustomPlacement;
        }
        
        return popup;
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

    protected override bool HideCore(bool canCancel = true)
    {
        if (canCancel && IsPopupPinnedOpen)
        {
            return false;
        }

        if (canCancel && IsMotionEnabled && CloseMotion is not null && Popup.IsOpen)
        {
            Popup.IsOpen = false;
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
            SetCurrentValue(PlacementProperty, PlacementMode.Custom);
            ConfigurePointerPlacementOffsets();
        }
    }

    private void ApplyLightDismissConfiguration(PopupControl popup)
    {
        popup.SetCurrentValue(
            AvaloniaPopup.IsLightDismissEnabledProperty,
            IsLightDismissEnabled);
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
}
