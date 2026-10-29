using System.ComponentModel;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Media;
using AtomUI.Animations;
using AtomUI.Native;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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

    private PopupCloseState _closeMotionState;
    private bool _isOpenMotionActive;
    private bool _hasRaisedOpened;
    private bool _isLogicallyAttachedAtOpen;
    private long _motionVersion;
    private bool _closeRequested;
    private IDisposable? _entryPreparation;
    private Control? _motionTarget;
    private TopLevel? _openTopLevel;
    private PopupLightDismissRegistration? _lightDismissRegistration;

    #endregion

    // 窗口型 Popup 阴影区域的输入穿透，随一次打开会话挂接与释放
    private IPopupShadowHitTest? _shadowHitTest;

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
        _closeMotionState = PopupCloseState.Idle;
        _closeRequested = false;
        AttachWheelGuard();
        UpdatePlacementTransformTracker();
        StartOpenMotion();
        AttachShadowHitTest();
    }

    private void StartOpenMotion()
    {
        var version = ++_motionVersion;
        _isOpenMotionActive = false;
        if (_motionTarget is not { } target)
        {
            return;
        }
        if (!IsMotionEnabled || OpenMotion is not { } motion)
        {
            MotionPlayback.CancelAndReset(target);
            ClearEntryPreparation();
            return;
        }

        // A single layout boundary prepares the physical host. Playback itself
        // always follows Avalonia's clock, with no periodic Dispatcher work.
        _isOpenMotionActive = true;
        _shadowHitTest?.Invalidate();
        Dispatcher.Post(() => _ = PlayOpenMotionAsync(version, target, motion), DispatcherPriority.Loaded);
    }

    private async Task PlayOpenMotionAsync(long version, Control target, AbstractMotion motion)
    {
        try
        {
            await PlayOpenMotionCoreAsync(version, target, motion);
        }
        finally
        {
            if (version == _motionVersion)
            {
                _isOpenMotionActive = false;
                _shadowHitTest?.Invalidate();
            }
        }
    }

    private async Task PlayOpenMotionCoreAsync(long version, Control target, AbstractMotion motion)
    {
        if (!IsCurrentMotion(version, target) || !IsOpen || _closeMotionState != PopupCloseState.Idle)
        {
            return;
        }
        var preparation = _entryPreparation;
        _entryPreparation = null;
        try
        {
            target.UpdateLayout();
            Task<MotionResult> playback;
            try
            {
                playback = motion.PlayAsync(target, MotionDuration, CancellationToken.None);
            }
            finally
            {
                preparation?.Dispose();
            }
            var result = await playback;
            if (IsCurrentMotion(version, target) && result.Outcome == MotionOutcome.NotReady)
            {
                MotionPlayback.CancelAndReset(target);
            }
        }
        catch (Exception exception)
        {
            Avalonia.Logging.Logger.TryGet(Avalonia.Logging.LogEventLevel.Error, AtomUILogArea.Control)?.Log(
                this, "Popup entry motion failed: {Exception}", exception);
            if (IsCurrentMotion(version, target))
            {
                MotionPlayback.CancelAndReset(target);
            }
        }
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        DetachShadowHitTest();
        _hasRaisedOpened = false;
        _isOpenMotionActive = false;
        var lightDismiss = _lightDismissRegistration;
        _lightDismissRegistration = null;
        var previousVersion = _motionVersion;
        lightDismiss?.Dispose();
        if (previousVersion != _motionVersion)
        {
            return;
        }
        var version = CancelMotion();
        if (version != _motionVersion)
        {
            return;
        }
        _closeMotionState             = PopupCloseState.Idle;
        _closeRequested              = false;
        _isLogicallyAttachedAtOpen    = false;
        _motionTarget                  = null;
        _openTopLevel                 = null;
        ClearPlacementTransformTracker();
        DetachWheelGuard();
    }

    private long CancelMotion()
    {
        var version = ++_motionVersion;
        var target = _motionTarget;
        var preparation = _entryPreparation;
        _motionTarget = null;
        _entryPreparation = null;
        _isOpenMotionActive = false;
        Exception? failure = null;
        try
        {
            if (target is not null)
            {
                MotionPlayback.CancelAndReset(target);
            }
        }
        catch (Exception exception)
        {
            failure = exception;
        }
        try
        {
            preparation?.Dispose();
        }
        catch (Exception cleanup) when (failure is not null)
        {
            throw new AggregateException("Popup motion cleanup failed.", failure, cleanup);
        }
        if (failure is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }
        return version;
    }

    private void ClearEntryPreparation()
    {
        var preparation = _entryPreparation;
        _entryPreparation = null;
        preparation?.Dispose();
    }

    private bool IsCurrentMotion(long version, Control target) =>
        version == _motionVersion && ReferenceEquals(target, _motionTarget);

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

        if (_closeMotionState == PopupCloseState.Completing)
        {
            _closeMotionState = PopupCloseState.Idle;
            return;
        }

        if (!IsMotionEnabled ||
            CloseMotion is not { } closeMotion ||
            _motionTarget is not { } motionActor)
        {
            return;
        }

        e.Cancel = true;
        if (_closeMotionState != PopupCloseState.Idle)
        {
            return;
        }

        _closeMotionState = PopupCloseState.Pending;
        _isOpenMotionActive = false;
        _closeRequested = true;
        var version = ++_motionVersion;
        ClearEntryPreparation();
        _shadowHitTest?.Invalidate();
        Dispatcher.Post(() => _ = PlayCloseMotionAndCloseAsync(version, motionActor, closeMotion), DispatcherPriority.Loaded);
    }

    private async Task PlayCloseMotionAndCloseAsync(long version, Control target, AbstractMotion motion)
    {
        if (!IsCurrentMotion(version, target) || _closeMotionState != PopupCloseState.Pending)
        {
            return;
        }
        _closeMotionState = PopupCloseState.Playing;
        try
        {
            target.UpdateLayout();
            var result = await motion.PlayAsync(target, MotionDuration, CancellationToken.None);
            if (!IsCurrentMotion(version, target) || _closeMotionState != PopupCloseState.Playing)
            {
                return;
            }
            if (result.Outcome != MotionOutcome.NotReady && (!result.IsCompleted || !Motion.IsCurrent(target, result)))
            {
                _closeMotionState = PopupCloseState.Idle;
                _shadowHitTest?.Invalidate();
                return;
            }
        }
        catch (Exception exception)
        {
            Avalonia.Logging.Logger.TryGet(Avalonia.Logging.LogEventLevel.Error, AtomUILogArea.Control)?.Log(
                this, "Popup exit motion failed: {Exception}", exception);
        }
        if (!IsCurrentMotion(version, target) || _closeMotionState != PopupCloseState.Playing)
        {
            return;
        }
        QueueFinalClose(version, target);
    }

    private void QueueFinalClose(long version, Control target)
    {
        _closeMotionState = PopupCloseState.Completing;
        Dispatcher.Post(() =>
        {
            if (IsCurrentMotion(version, target) && _closeMotionState == PopupCloseState.Completing && _closeRequested)
            {
                Close();
            }
        });
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
        _closeMotionState is PopupCloseState.Pending or PopupCloseState.Playing or PopupCloseState.Completing;

    /// <summary>
    /// 打开（含等待首帧准备）或关闭动效进行中。
    /// </summary>
    internal bool IsPlayingMotion => _isOpenMotionActive || IsPlayingCloseMotion;

    private void AttachShadowHitTest()
    {
        DetachShadowHitTest();
        // 打开后 Child 挂在宿主下：窗口型 Popup 的 TopLevel 即 PopupRoot，Overlay Popup 则是所属窗口
        if (Child is null || TopLevel.GetTopLevel(Child) is not PopupRoot popupRoot)
        {
            return;
        }

        // 逐点命中（Windows）直接使用 Avalonia 的命中测试：frame renderer 的圆角矩形与箭头几何可命中，
        // 绘制列表命中不包含 BoxShadow，阴影区域因此不命中。禁用的内容仍属于内容形状。
        // 预先提交输入区域的平台（X11）无法逐点查询，提供同一圆角矩形与箭头几何，由 AtomUI.Native 光栅化。
        _shadowHitTest = NativePopupShadowHitTest.TryAttach(
            popupRoot,
            () => IsPlayingMotion,
            point => popupRoot.InputHitTest(point, enabledElementsOnly: false) is not null,
            () => popupRoot.GetVisualDescendants().OfType<ShadowsAwareContainer>().FirstOrDefault()
                           ?.GetContentShape(popupRoot));
    }

    // 窗口型 Popup 的 Child 在 PopupRoot 首次布局时挂入视觉树，早于原生窗口映射（WindowBase.Show 先布局再 PlatformImpl.Show）。
    // 这是 Opened 之前唯一能拿到尚未映射的 PopupRoot 的时机，Avalonia 未提供其他公开回调。
    private void HandleChildAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // Avalonia 12 的视觉根是包裹 TopLevel 的 TopLevelHost，按所属 TopLevel 判定
        if (_shadowHitTest is null && sender is Visual child && TopLevel.GetTopLevel(child) is PopupRoot popupRoot)
        {
            NativePopupShadowHitTest.SuspendInputBeforeShow(popupRoot);
        }
    }

    private void DetachShadowHitTest()
    {
        var shadowHitTest = _shadowHitTest;
        _shadowHitTest = null;
        shadowHitTest?.Dispose();
    }

    internal void CancelCloseAnimation()
    {
        if (!IsPlayingCloseMotion)
        {
            return;
        }

        _closeMotionState = PopupCloseState.Idle;
        _closeRequested = false;
        SetCurrentValue(IsOpenProperty, true);
        StartOpenMotion();
        _shadowHitTest?.Invalidate();
    }

    internal void CloseForLifecycle()
    {
        ++_pinnedOpenGeneration;
        _isPinnedOpenSuspended = true;
        _closeMotionState      = PopupCloseState.Idle;
        _closeRequested       = false;
        CancelMotion();
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

    internal void NotifySurfaceReady(Control target)
    {
        var changed = !ReferenceEquals(_motionTarget, target);
        var closing = _closeRequested && IsPlayingCloseMotion;
        var version = _motionVersion;
        if (changed)
        {
            version = CancelMotion();
            if (version != _motionVersion)
            {
                return;
            }
            _motionTarget = target;
        }
        if (changed && closing)
        {
            // The old template is gone. Keep the replacement hidden and finish
            // the current close intent; a re-open invalidates this queued close.
            PrepareSurface(target);
            if (IsCurrentMotion(version, target) && _closeRequested)
            {
                QueueFinalClose(version, target);
            }
            return;
        }
        if (IsMotionEnabled && OpenMotion is not null && _entryPreparation is null && (changed || !_hasRaisedOpened))
        {
            // The host calls this during logical attach, before its first Show.
            // A scoped override preserves the application's underlying opacity.
            PrepareSurface(target);
        }
        if (!IsCurrentMotion(version, target))
        {
            return;
        }
        if (_hasRaisedOpened && IsOpen && _closeMotionState == PopupCloseState.Idle)
        {
            StartOpenMotion();
        }
    }

    private void PrepareSurface(Control target)
    {
        var preparation = new CompositeDisposable();
        _entryPreparation = preparation;
        preparation.Add(target.SetValue(OpacityProperty, 0, Avalonia.Data.BindingPriority.Animation) ?? Disposable.Empty);
    }

    private enum PopupCloseState
    {
        Idle,
        Pending,
        Playing,
        Completing
    }

    #region 自定义定位逻辑

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ChildProperty)
        {
            if (change.OldValue is Control oldChild)
            {
                oldChild.AttachedToVisualTree -= HandleChildAttachedToVisualTree;
            }
            if (change.NewValue is Control newChild)
            {
                newChild.AttachedToVisualTree += HandleChildAttachedToVisualTree;
            }
        }

        if (change.Property == IsOpenProperty && change.GetNewValue<bool>() && _closeRequested)
        {
            CancelCloseAnimation();
        }
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

            // 箭头隐藏或部件不可用时保持空 bounds：窗口型 Popup 在朝向 anchor 一侧保留完整阴影，
            // ApplyCustomPlacement 按完整阴影厚度回退（见 PopupUtils.GetAnchorSideResidualShadow）
            var arrowIndicatorLayoutBounds = default(Rect);
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
