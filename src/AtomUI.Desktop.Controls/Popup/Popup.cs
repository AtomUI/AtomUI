using System.ComponentModel;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Media;
using AtomUI.MotionScene;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;

public class Popup : AvaloniaPopup, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<BoxShadows> PopupRootShadowProperty =
        AvaloniaProperty.Register<Popup, BoxShadows>(nameof(PopupRootShadow));
    
    public static readonly StyledProperty<BoxShadows> OverlayHostShadowProperty =
        AvaloniaProperty.Register<Popup, BoxShadows>(nameof(OverlayHostShadow));

    public static readonly StyledProperty<IBrush?> SurfaceBackgroundProperty =
        AvaloniaProperty.Register<Popup, IBrush?>(nameof(SurfaceBackground), defaultValue: null);

    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<Popup>();

    public static readonly DirectProperty<Popup, bool> IsHorizontalFlippedProperty =
        AvaloniaProperty.RegisterDirect<Popup, bool>(nameof(IsHorizontalFlipped),
            o => o.IsHorizontalFlipped,
            (o, v) => o.IsHorizontalFlipped = v);
    
    public static readonly DirectProperty<Popup, bool> IsVerticalFlippedProperty =
        AvaloniaProperty.RegisterDirect<Popup, bool>(nameof(IsVerticalFlipped),
            o => o.IsVerticalFlipped,
            (o, v) => o.IsVerticalFlipped = v);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Popup>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<Popup>();

    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        AvaloniaProperty.Register<Popup, AbstractMotion?>(nameof(OpenMotion));

    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        AvaloniaProperty.Register<Popup, AbstractMotion?>(nameof(CloseMotion));

    public static readonly StyledProperty<PlacementMode?> RequestedPlacementProperty =
        AvaloniaProperty.Register<Popup, PlacementMode?>(nameof(RequestedPlacement));

    public static readonly StyledProperty<double> MarginToAnchorProperty =
        AvaloniaProperty.Register<Popup, double>(nameof(MarginToAnchor));

    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        AvaloniaProperty.Register<Popup, bool>(nameof(IsPointAtCenter));
    
    public static readonly StyledProperty<CustomPlacementCallback?> CustomPlacementCallbackProperty =
        AvaloniaProperty.Register<Popup, CustomPlacementCallback?>(nameof(CustomPlacementCallback));
    
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

    public IBrush? SurfaceBackground
    {
        get => GetValue(SurfaceBackgroundProperty);
        set => SetValue(SurfaceBackgroundProperty, value);
    }

    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    private bool _isHorizontalFlipped;

    public bool IsHorizontalFlipped
    {
        get => _isHorizontalFlipped;
        private set => SetAndRaise(IsHorizontalFlippedProperty, ref _isHorizontalFlipped, value);
    }
    
    private bool _isVerticalFlipped;

    public bool IsVerticalFlipped
    {
        get => _isVerticalFlipped;
        private set => SetAndRaise(IsVerticalFlippedProperty, ref _isVerticalFlipped, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
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

    public PlacementMode? RequestedPlacement
    {
        get => GetValue(RequestedPlacementProperty);
        set => SetValue(RequestedPlacementProperty, value);
    }

    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }
    
    public CustomPlacementCallback? CustomPlacementCallback
    {
        get => GetValue(CustomPlacementCallbackProperty);
        set => SetValue(CustomPlacementCallbackProperty, value);
    }
    #endregion

    #region 公共事件定义

    public event EventHandler<PopupFlippedEventArgs>? PositionFlipped;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<BoxShadows> FrameShadowProperty =
        AvaloniaProperty.Register<Popup, BoxShadows>(nameof(FrameShadow));    

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        AvaloniaProperty.Register<Popup, bool>(nameof(IsPopupPinnedOpen));

    internal BoxShadows FrameShadow
    {
        get => GetValue(FrameShadowProperty);
        set => SetValue(FrameShadowProperty, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }
    #endregion

    #region 动画相关字段

    private MotionExecutionState _closeMotionState;
    private bool _hasRaisedOpened;
    private bool _isLogicallyAttachedAtOpen;
    private CancellationTokenSource? _motionCts;
    private PopupMotionActor? _motionActor;
    private TopLevel? _openTopLevel;
    private PopupLightDismissRegistration? _lightDismissRegistration;

    #endregion

    // 避免 popup 内的 wheel 事件冒泡出去触发 PlacementTarget 所在祖先 ScrollViewer 的意外滚动。
    // 订阅对象要记录下来,保证 open/close 成对摘挂。
    private InputElement? _wheelGuardSubscribedOn;
    private int _ignoreRequestedPlacementChange;
    private int _lifecycleCloseDepth;
    private int _pinnedOpenGeneration;
    private bool _isPinnedOpenSuspended;
    private bool _isPinnedOpenReconcileRetried;
    private readonly List<Visual> _pinnedOpenTargetStateSubscriptions = [];
    private Control? _pinnedOpenTrackingTarget;
    private Control? _placementTransformTrackingTarget;
    private IDisposable? _placementTransformTracker;

    static Popup()
    {
        IsOpenProperty.OverrideMetadata<Popup>(
            new StyledPropertyMetadata<bool>(coerce: CoerceIsOpen));
        IsLightDismissEnabledProperty.OverrideMetadata<Popup>(
            new StyledPropertyMetadata<bool>(coerce: (sender, value) => value && !((Popup)sender).IsPopupPinnedOpen));
    }

    public Popup()
    {
        this.AddClosingEventHandler(HandlePopupClosing);
        Opened += HandlePopupOpened;
        Closed += HandlePopupClosed;
    }

    #region 动画逻辑

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        _hasRaisedOpened = true;
        _isPinnedOpenSuspended = false;
        _isLogicallyAttachedAtOpen = ((ILogical)this).IsAttachedToLogicalTree;
        _openTopLevel = ResolvePlacementTarget() is { } target
            ? TopLevel.GetTopLevel(target)
            : null;
        _lightDismissRegistration?.Dispose();
        _lightDismissRegistration = ResolvePlacementTarget() is { } placementTarget
            ? PopupLightDismissRegistration.Acquire(this, placementTarget)
            : null;
        _lightDismissRegistration?.Update(this);
        _closeMotionState = MotionExecutionState.Idle;
        AttachWheelGuard();
        UpdatePlacementTransformTracker();
        StartOpenMotion();
    }

    private void StartOpenMotion()
    {
        CancelMotion();
        if (_motionActor is not { } motionActor)
        {
            return;
        }

        motionActor.MotionTransform           = null;
        motionActor.MotionTransformOperations = null;
        if (!IsMotionEnabled || OpenMotion is null)
        {
            motionActor.Opacity = 1.0d;
            return;
        }

        _motionCts = new CancellationTokenSource();

        var motion = OpenMotion;
        motion.Duration      = MotionDuration;
        motionActor.Opacity = 0.0d;
        var cancellationToken = _motionCts.Token;
        Dispatcher.InvokeAsync(() => PlayMotionAsync(motion, motionActor, cancellationToken));
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        _hasRaisedOpened = false;
        _lightDismissRegistration?.Dispose();
        _lightDismissRegistration = null;
        CancelMotion();
        _closeMotionState             = MotionExecutionState.Idle;
        _isLogicallyAttachedAtOpen    = false;
        _motionActor                  = null;
        _openTopLevel                 = null;
        ClearPlacementTransformTracker();
        DetachWheelGuard();
    }

    private void CancelMotion()
    {
        _motionCts?.Cancel();
        _motionCts?.Dispose();
        _motionCts = null;
    }

    private void HandlePopupClosing(object? sender, CancelEventArgs e)
    {
        if (_lifecycleCloseDepth > 0 ||
            !CanDelayCloseForMotion() ||
            (IsPopupPinnedOpen && !HasValidPinnedOpenTarget()))
        {
            return;
        }

        if (IsPopupPinnedOpen)
        {
            e.Cancel = true;
            return;
        }

        if (_closeMotionState == MotionExecutionState.Completing)
        {
            _closeMotionState = MotionExecutionState.Idle;
            return;
        }

        if (!IsMotionEnabled ||
            CloseMotion is not { } closeMotion ||
            _motionActor is not { } motionActor)
        {
            return;
        }

        e.Cancel = true;
        if (_closeMotionState != MotionExecutionState.Idle)
        {
            return;
        }

        _closeMotionState = MotionExecutionState.Pending;
        Dispatcher.InvokeAsync(() => PlayCloseMotionAndCloseAsync(motionActor, closeMotion));
    }

    private async Task PlayMotionAsync(AbstractMotion motion, BaseMotionActor actor, CancellationToken cancellationToken)
    {
        try
        {
            await motion.RunAsync(actor, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // 动画被取消（快速开关场景），忽略
        }
    }

    private async Task PlayCloseMotionAndCloseAsync(BaseMotionActor actor, AbstractMotion motion)
    {
        if (_closeMotionState != MotionExecutionState.Pending)
        {
            return;
        }

        if (_motionCts is { } previousMotionCts)
        {
            await previousMotionCts.CancelAsync();
            previousMotionCts.Dispose();
            if (ReferenceEquals(_motionCts, previousMotionCts))
            {
                _motionCts = null;
            }
        }

        if (_closeMotionState != MotionExecutionState.Pending)
        {
            return;
        }

        var closeMotionCts = new CancellationTokenSource();
        _motionCts         = closeMotionCts;
        _closeMotionState  = MotionExecutionState.Playing;
        motion.Duration = MotionDuration;

        try
        {
            await motion.RunAsync(actor, cancellationToken: closeMotionCts.Token);
        }
        catch (OperationCanceledException)
        {
            if (_closeMotionState == MotionExecutionState.Playing)
            {
                _closeMotionState = MotionExecutionState.Idle;
            }
            return;
        }
        finally
        {
            if (ReferenceEquals(_motionCts, closeMotionCts))
            {
                _motionCts = null;
                closeMotionCts.Dispose();
            }
        }

        if (_closeMotionState != MotionExecutionState.Playing)
        {
            return;
        }

        _closeMotionState = MotionExecutionState.Completing;
        Dispatcher.Post(Close);
    }

    private bool CanDelayCloseForMotion()
    {
        if (_openTopLevel is null ||
            (_isLogicallyAttachedAtOpen && !((ILogical)this).IsAttachedToLogicalTree))
        {
            return false;
        }

        var target = ResolvePlacementTarget();
        return target is not null &&
               target.IsAttachedToVisualTree() &&
               ReferenceEquals(TopLevel.GetTopLevel(target), _openTopLevel) &&
               target.TransformToVisual(_openTopLevel) is not null;
    }

    private Control? ResolvePlacementTarget()
    {
        return PlacementTarget ?? this.FindLogicalAncestorOfType<Control>();
    }

    #endregion

    internal bool IsPlayingCloseMotion =>
        _closeMotionState is MotionExecutionState.Pending or MotionExecutionState.Playing;

    internal void CancelCloseAnimation()
    {
        if (!IsPlayingCloseMotion)
        {
            return;
        }

        _closeMotionState = MotionExecutionState.Idle;
        _motionCts?.Cancel();
        if (_motionActor is not null)
        {
            _motionActor.Opacity = 1.0d;
        }

        IsOpen = true;
    }

    internal void CloseForLifecycle()
    {
        ++_pinnedOpenGeneration;
        _isPinnedOpenSuspended = true;
        _closeMotionState      = MotionExecutionState.Idle;
        CancelMotion();
        if (_motionActor is not null)
        {
            _motionActor.Opacity = 1.0d;
        }

        try
        {
            ++_lifecycleCloseDepth;
            Close();
        }
        finally
        {
            --_lifecycleCloseDepth;
        }
    }

    internal void NotifyFlipped(bool horizontalFlipped,  bool verticalFlipped)
    {
        IsHorizontalFlipped = horizontalFlipped;
        IsVerticalFlipped   = verticalFlipped;
        PositionFlipped?.Invoke(this, new PopupFlippedEventArgs(horizontalFlipped, verticalFlipped));
    }

    internal void NotifyMotionActorReady(PopupMotionActor actor)
    {
        _motionActor = actor;
        // A lazily created popup host can apply its template after Popup.Opened.
        // PopupMotionActor pre-hides itself during attach, so it must enter the
        // same opening path here or the first popup remains physically open but
        // visually transparent until it is reopened.
        if (_hasRaisedOpened && IsOpen && _closeMotionState == MotionExecutionState.Idle)
        {
            StartOpenMotion();
        }
    }

    #region 自定义定位逻辑

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == RequestedPlacementProperty)
        {
            var requestedPlacement = change.GetNewValue<PlacementMode?>();
            if (requestedPlacement is not null)
            {
                var originalPlacement = Placement;
                CustomPopupPlacementCallback = HandleCustomPlacement;
                SetCurrentValue(PlacementProperty, PlacementMode.Custom);
                if (originalPlacement == PlacementMode.Custom)
                {
                    this.HandlePositionChange();
                }
            }
            else
            {
                CustomPopupPlacementCallback = null;
            }
        }
        else if (change.Property == PlacementProperty)
        {
            if (_ignoreRequestedPlacementChange > 0)
            {
                return;
            }
            try
            {
                _ignoreRequestedPlacementChange++;
                if (Placement != PlacementMode.Custom)
                {
                    CustomPopupPlacementCallback = HandleCustomPlacement;
                    SetCurrentValue(RequestedPlacementProperty, Placement);
                    SetCurrentValue(PlacementProperty, PlacementMode.Custom);
                    this.HandlePositionChange();
                }
            }
            finally
            {
                _ignoreRequestedPlacementChange--;
            }
        }
        else if (change.Property == MarginToAnchorProperty)
        {
            this.HandlePositionChange();
        }
        else if (change.Property == PopupRootShadowProperty ||
                 change.Property == OverlayHostShadowProperty ||
                 change.Property == IsUsingOverlayLayerProperty)
        {
            ConfigureFrameShadow();
        }
        else if (change.Property == IsLightDismissEnabledProperty ||
                 change.Property == OverlayInputPassThroughElementProperty)
        {
            _lightDismissRegistration?.Update(this);
        }
        else if (change.Property == IsPopupPinnedOpenProperty)
        {
            CoerceValue(IsLightDismissEnabledProperty);
            if (change.GetNewValue<bool>())
            {
                _isPinnedOpenSuspended = false;
                CancelCloseAnimation();
                UpdatePinnedOpenTargetSubscription();
                QueuePinnedOpen();
            }
            else
            {
                ++_pinnedOpenGeneration;
                ClearPinnedOpenTargetSubscription();
                if (!IsOpen)
                {
                    CloseForLifecycle();
                }
            }
        }
        else if (change.Property == ChildProperty && IsPopupPinnedOpen)
        {
            if (Child is null)
            {
                CloseForLifecycle();
            }
            else
            {
                _isPinnedOpenSuspended = false;
                QueuePinnedOpen();
            }
        }

        if (change.Property == PlacementTargetProperty ||
            change.Property == PlacementProperty ||
            change.Property == RequestedPlacementProperty)
        {
            UpdatePlacementTransformTracker();
            if (change.Property == PlacementTargetProperty && IsPopupPinnedOpen)
            {
                _isPinnedOpenSuspended = false;
                UpdatePinnedOpenTargetSubscription();
                QueuePinnedOpen();
            }
        }
    }

    internal void HandleCustomPlacement(CustomPopupPlacement placement)
    {
        var          requestedPlacement = RequestedPlacement!.Value;
        var          isUseOverlayHost   = ShouldUseOverlayLayer || !RuntimePlatform.Features.SupportsNativeWindow;
        var          shadowThickness    = GetPositioningShadowThickness(isUseOverlayHost);
        var          hOffset            = HorizontalOffset;
        var          vOffset            = VerticalOffset;
        var          marginToAnchor     = MarginToAnchor;
        PopupAnchor  anchor;
        PopupGravity gravity;
        var          target             = PlacementTarget ?? Parent as Control;
        var          topLevel           = TopLevel.GetTopLevel(target);
        if (requestedPlacement == PlacementMode.Center)
        {
            if (topLevel == null)
            {
                return;
            }

            if (isUseOverlayHost)
            {
                // Overlay 模式：popup 渲染在覆盖整个窗口的 OverlayLayer 中，坐标系为窗口客户区逻辑坐标。
                // 将 AnchorRectangle 设为整个客户区，使 Anchor=None / Gravity=None 的定位器
                // 自动将 popup 居中于窗口。
                placement.AnchorRectangle = new Rect(default, topLevel.ClientSize);
            }
            else
            {
                // Popup Root 模式：popup 是独立的 OS 窗口。
                // ManagedPopupPositioner 会将 AnchorRectangle（窗口客户区逻辑坐标）自动加上
                // parentGeometry.TopLeft 转换为屏幕坐标，flip 约束边界为 screen.WorkingArea。
                // 设为整个窗口客户区，popup 最终居中于父窗口所在屏幕区域内。
                placement.AnchorRectangle = new Rect(default, topLevel.ClientSize);
            }

            anchor  = PopupAnchor.None;
            gravity = PopupGravity.None;
        }
        else
        {
            if (target is null)
            {
                return;
            }

            if (requestedPlacement == PlacementMode.Pointer)
            {
                if (topLevel == null)
                {
                    return;
                }

                var position = topLevel.PointToClient(topLevel.GetLastPointerPosition() ?? default);
                placement.AnchorRectangle = new Rect(position, new Size(1, 1));
                anchor                    = PopupAnchor.TopLeft;
                gravity                   = PopupGravity.BottomRight;
            }
            else
            {
                (anchor, gravity) = PopupUtils.GetAnchorAndGravity(requestedPlacement);
            }

            var arrowIndicatorLayoutBounds = default(Rect);
            var arrowHiddenButCapable      = false;
            if (Child is IArrowAwareShadowMaskInfoProvider provider)
            {
                // If ArrowIndicatorLayoutBounds is not initialized yet, force a layout pass
                if (provider.IsArrowVisible() && PopupUtils.CanEnabledArrow(requestedPlacement))
                {
                    Child.Measure(Size.Infinity);
                    Child.Arrange(new Rect(Child.DesiredSize));
                    var arrowDecoratedBox = provider.GetArrowDecoratedBox();
                    if (arrowDecoratedBox is not null)
                    {
                        arrowIndicatorLayoutBounds = arrowDecoratedBox.ArrowIndicatorLayoutBounds;

                        if (IsPointAtCenter)
                        {
                            var delta = PopupUtils.CalculatePointAtCenterDelta(
                                target, arrowDecoratedBox, requestedPlacement, anchor, gravity);
                            hOffset += delta.X;
                            vOffset += delta.Y;
                        }
                    }
                    else
                    {
                        // 箭头部件不可用（模板未就绪或主题未提供），与箭头隐藏同等对待：
                        // 位置公式按无箭头计算，再由 arrowHiddenButCapable 做阴影厚度补偿
                        arrowHiddenButCapable = true;
                    }
                }
                else if (PopupUtils.CanEnabledArrow(requestedPlacement))
                {
                    // 箭头被隐藏但该 Placement 本来支持箭头：popup root 在箭头侧仍然预留了完整阴影厚度
                    // （ShadowsAwareContainer.GetEffectiveShadowThickness 在 IsArrowVisible=false 时返回完整阴影），
                    // 而位置公式默认阴影会被箭头抵消，因此此时内容离 target 会多出一个阴影厚度的空隙。
                    // 在 ApplyCustomPlacement 之后再对 placement.Offset 做一次朝 target 方向的补偿。
                    arrowHiddenButCapable = true;
                }
            }
            var (flipX, flipY) = PopupUtils.ApplyCustomPlacement(
                placement,
                requestedPlacement,
                isUseOverlayHost,
                hOffset,
                vOffset,
                marginToAnchor,
                shadowThickness,
                anchor,
                gravity,
                arrowIndicatorLayoutBounds);
            if (arrowHiddenButCapable && !isUseOverlayHost)
            {
                var arrowDirection = PopupUtils.FlipDirection(
                    PopupUtils.GetDirection(requestedPlacement), flipX, flipY);
                var dx = 0.0d;
                var dy = 0.0d;
                switch (arrowDirection)
                {
                    case Direction.Top:
                        dy = shadowThickness.Bottom;
                        break;
                    case Direction.Bottom:
                        dy = -shadowThickness.Top;
                        break;
                    case Direction.Left:
                        dx = shadowThickness.Right;
                        break;
                    case Direction.Right:
                        dx = -shadowThickness.Left;
                        break;
                }
                placement.Offset = new Point(placement.Offset.X + dx, placement.Offset.Y + dy);
            }

            NotifyFlipped(flipX, flipY);
            CustomPlacementCallback?.Invoke(placement, shadowThickness, marginToAnchor, isUseOverlayHost, flipX, flipY);
            return;
        }

        // Center 走到这里：AnchorRectangle 已设为整个窗口客户区，Anchor/Gravity=None 定位器自动居中。
        // 居中不存在翻转，直接设置 Offset 并通知 (false, false)。
        placement.Anchor  = anchor;
        placement.Gravity = gravity;
        placement.Offset  = new Point(hOffset, vOffset);
        NotifyFlipped(false, false);
        CustomPlacementCallback?.Invoke(placement, shadowThickness, marginToAnchor, isUseOverlayHost, false, false);
    }

    private Thickness GetPositioningShadowThickness(bool isUseOverlayHost)
    {
        return (isUseOverlayHost ? OverlayHostShadow : PopupRootShadow).Thickness();
    }

    #endregion

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureFrameShadow();
        if (IsPopupPinnedOpen)
        {
            _isPinnedOpenSuspended = false;
            UpdatePinnedOpenTargetSubscription();
            QueuePinnedOpen();
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ++_pinnedOpenGeneration;
        ClearPinnedOpenTargetSubscription();
        base.OnDetachedFromLogicalTree(e);
    }

    private void QueuePinnedOpen()
    {
        var generation = ++_pinnedOpenGeneration;
        Dispatcher.Post(() =>
        {
            if (generation != _pinnedOpenGeneration ||
                !IsPopupPinnedOpen ||
                _isPinnedOpenSuspended ||
                IsOpen ||
                Child is null)
            {
                return;
            }

            if (!CanOpenPinnedPopup())
            {
                return;
            }

            IsOpen = true;
            CoerceValue(IsOpenProperty);
        });
    }

    private void UpdatePinnedOpenTargetSubscription()
    {
        var target = IsPopupPinnedOpen ? ResolvePlacementTarget() : null;
        if (ReferenceEquals(_pinnedOpenTrackingTarget, target))
        {
            return;
        }

        ClearPinnedOpenTargetSubscription();
        if (target is null)
        {
            return;
        }

        _pinnedOpenTrackingTarget = target;
        target.AttachedToVisualTree += HandlePinnedOpenTargetAttached;
        target.DetachedFromVisualTree += HandlePinnedOpenTargetDetached;
        if (target.IsAttachedToVisualTree())
        {
            SubscribeToPinnedOpenTargetState(target);
        }
    }

    private void ClearPinnedOpenTargetSubscription()
    {
        if (_pinnedOpenTrackingTarget is not null)
        {
            _pinnedOpenTrackingTarget.AttachedToVisualTree -= HandlePinnedOpenTargetAttached;
            _pinnedOpenTrackingTarget.DetachedFromVisualTree -= HandlePinnedOpenTargetDetached;
            _pinnedOpenTrackingTarget = null;
        }
        ClearPinnedOpenTargetStateSubscriptions();
    }

    private void HandlePinnedOpenTargetAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control target && ReferenceEquals(target, _pinnedOpenTrackingTarget))
        {
            SubscribeToPinnedOpenTargetState(target);
            ReconcilePinnedOpenTargetState();
        }
    }

    private void HandlePinnedOpenTargetDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!ReferenceEquals(sender, _pinnedOpenTrackingTarget))
        {
            return;
        }

        ClearPinnedOpenTargetStateSubscriptions();
        CloseForLifecycle();
    }

    private void SubscribeToPinnedOpenTargetState(Control target)
    {
        ClearPinnedOpenTargetStateSubscriptions();
        Visual? current = target;
        while (current is not null)
        {
            current.PropertyChanged += HandlePinnedOpenTargetStatePropertyChanged;
            _pinnedOpenTargetStateSubscriptions.Add(current);
            current = current.GetVisualParent();
        }
    }

    private void ClearPinnedOpenTargetStateSubscriptions()
    {
        foreach (var visual in _pinnedOpenTargetStateSubscriptions)
        {
            visual.PropertyChanged -= HandlePinnedOpenTargetStatePropertyChanged;
        }
        _pinnedOpenTargetStateSubscriptions.Clear();
    }

    private void HandlePinnedOpenTargetStatePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Visual.IsVisibleProperty ||
            e.Property == Visual.BoundsProperty ||
            e.Property == InputElement.IsEffectivelyEnabledProperty)
        {
            ReconcilePinnedOpenTargetState();
        }
    }

    private void ReconcilePinnedOpenTargetState()
    {
        if (!IsPopupPinnedOpen || _pinnedOpenTrackingTarget is null)
        {
            return;
        }

        if (HasValidPinnedOpenTarget())
        {
            _isPinnedOpenReconcileRetried = false;
            _isPinnedOpenSuspended = false;
            QueuePinnedOpen();
            return;
        }

        // IsVisible 恢复的同帧里放置目标尚未重新布局（TransformToVisual 未就绪 / 尺寸为零），
        // 立即判无效会错过这次恢复信号——之后不再有属性变化触发 reconcile，弹层就再也打不开。
        // 无法呈现时延后一帧重试一次，重试仍无效才按目标失效关闭。
        if (!_isPinnedOpenReconcileRetried)
        {
            _isPinnedOpenReconcileRetried = true;
            var generation = _pinnedOpenGeneration;
            Dispatcher.Post(() =>
            {
                if (generation == _pinnedOpenGeneration)
                {
                    ReconcilePinnedOpenTargetState();
                }
            }, DispatcherPriority.Loaded);
            return;
        }

        _isPinnedOpenReconcileRetried = false;
        CloseForLifecycle();
    }

    private bool HasValidPinnedOpenTarget()
    {
        var target = ResolvePlacementTarget();
        if (target is not { IsEffectivelyVisible: true, IsEffectivelyEnabled: true } ||
            !target.IsAttachedToVisualTree() ||
            TopLevel.GetTopLevel(target) is not { } topLevel)
        {
            return false;
        }

        return IsPlacementTargetVisibleInTopLevel(target, target.TransformToVisual(topLevel));
    }

    internal bool CanOpenPinnedPopup()
    {
        return Child is not null && HasValidPinnedOpenTarget();
    }

    private static bool CoerceIsOpen(AvaloniaObject sender, bool value)
    {
        return value &&
               (sender is not Popup popup ||
                !popup.IsPopupPinnedOpen ||
                popup.CanOpenPinnedPopup());
    }

    private void ConfigureFrameShadow()
    {
        if (IsUsingOverlayLayer)
        {
            FrameShadow = OverlayHostShadow;
        }
        else
        {
            FrameShadow = PopupRootShadow;
        }
    }

    private void AttachWheelGuard()
    {
        if (_wheelGuardSubscribedOn is not null)
        {
            return;
        }
        if (Child is not InputElement child)
        {
            return;
        }
        child.AddHandler(InputElement.PointerWheelChangedEvent, HandleChildPointerWheelBubbled,
            RoutingStrategies.Bubble);
        _wheelGuardSubscribedOn = child;
    }

    private void DetachWheelGuard()
    {
        if (_wheelGuardSubscribedOn is null)
        {
            return;
        }
        _wheelGuardSubscribedOn.RemoveHandler(InputElement.PointerWheelChangedEvent, HandleChildPointerWheelBubbled);
        _wheelGuardSubscribedOn = null;
    }

    private static void HandleChildPointerWheelBubbled(object? sender, PointerWheelEventArgs e)
    {
        // AddHandler 未指定 handledEventsToo,内部 ScrollViewer/列表若已把事件标 Handled,到此不会被调用。
        // 走到这里意味着 popup 内没有消费者,在 popup 边界处吞掉,防止泄漏到外层 ScrollViewer 引发意外滚动。
        e.Handled = true;
    }

    private void UpdatePlacementTransformTracker()
    {
        var target = ShouldTrackPlacementTransform()
            ? PlacementTarget ?? this.FindLogicalAncestorOfType<Control>()
            : null;
        if (ReferenceEquals(_placementTransformTrackingTarget, target))
        {
            return;
        }

        ClearPlacementTransformTracker();
        if (target is null)
        {
            return;
        }

        _placementTransformTrackingTarget = target;
        _placementTransformTracker = TransformTrackingHelper.Track(
            target,
            true,
            HandlePlacementTransformChanged);
    }

    private bool ShouldTrackPlacementTransform()
    {
        if (!IsOpen)
        {
            return false;
        }

        var placement = RequestedPlacement ?? Placement;
        return placement is not PlacementMode.Pointer and not PlacementMode.Center;
    }

    private void ClearPlacementTransformTracker()
    {
        _placementTransformTracker?.Dispose();
        _placementTransformTracker       = null;
        _placementTransformTrackingTarget = null;
    }

    private void HandlePlacementTransformChanged(Visual target, Matrix? matrix)
    {
        if (IsOpen && ReferenceEquals(_placementTransformTrackingTarget, target))
        {
            if (!IsPlacementTargetVisibleInTopLevel(target, matrix))
            {
                if (IsPopupPinnedOpen)
                {
                    CloseForLifecycle();
                }
                else
                {
                    Close();
                }
                return;
            }

            this.HandlePositionChange();
        }
    }

    private static bool IsPlacementTargetVisibleInTopLevel(Visual target, Matrix? matrix)
    {
        if (matrix is null ||
            TopLevel.GetTopLevel(target) is not { } topLevel)
        {
            return false;
        }

        var targetBounds  = new Rect(default, target.Bounds.Size).TransformToAABB(matrix.Value);
        var visibleBounds = new Rect(default, topLevel.ClientSize);
        if (targetBounds.Width <= 0 || targetBounds.Height <= 0)
        {
            return visibleBounds.Contains(targetBounds.Position);
        }

        return visibleBounds.Intersects(targetBounds);
    }
}

public class PopupFlippedEventArgs : EventArgs
{
    public bool HorizontalFlipped { get; set; }
    public bool VerticalFlipped { get; set; }

    public PopupFlippedEventArgs(bool horizontalFlipped, bool verticalFlipped)
    {
        HorizontalFlipped = horizontalFlipped;
        VerticalFlipped   = verticalFlipped;
    }
}
