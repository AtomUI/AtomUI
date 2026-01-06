using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.MotionScene;
using AtomUI.Theme.Styling;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;

public class Popup : AvaloniaPopup, IMotionAwareControl
{
    #region 公共属性定义

    public event EventHandler<PopupFlippedEventArgs>? PositionFlipped;

    public static readonly StyledProperty<BoxShadows> MaskShadowsProperty =
        AvaloniaProperty.Register<Popup, BoxShadows>(nameof(MaskShadows));

    public static readonly StyledProperty<double> MarginToAnchorProperty =
        AvaloniaProperty.Register<Popup, double>(nameof(MarginToAnchor));

    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<Popup>();
    
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty = 
        AvaloniaProperty.Register<Popup, AbstractMotion?>(nameof(OpenMotion));
        
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty = 
        AvaloniaProperty.Register<Popup, AbstractMotion?>(nameof(CloseMotion));

    public static readonly DirectProperty<Popup, bool> IsFlippedProperty =
        AvaloniaProperty.RegisterDirect<Popup, bool>(nameof(IsFlipped),
            o => o.IsFlipped,
            (o, v) => o.IsFlipped = v);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Popup>();

    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty = 
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<Popup>();
    
    public static readonly StyledProperty<bool> IsMotionAwareOpenProperty =
        AvaloniaProperty.Register<Popup, bool>(nameof (IsMotionAwareOpen));
    
    public static readonly StyledProperty<bool> ConfigureBlankMaskWhenMotionAwareOpenProperty =
        AvaloniaProperty.Register<Popup, bool>(nameof (ConfigureBlankMaskWhenMotionAwareOpen), false);

    public BoxShadows MaskShadows
    {
        get => GetValue(MaskShadowsProperty);
        set => SetValue(MaskShadowsProperty, value);
    }

    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    private bool _isFlipped;

    public bool IsFlipped
    {
        get => _isFlipped;
        private set => SetAndRaise(IsFlippedProperty, ref _isFlipped, value);
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
    
    public bool IsMotionAwareOpen
    {
        get => GetValue(IsMotionAwareOpenProperty);
        set => SetValue(IsMotionAwareOpenProperty, value);
    }
    
    /// <summary>
    /// When the animation is turned on and off, whether to not make a complete mask after the animation ends, including the content
    /// </summary>
    public bool ConfigureBlankMaskWhenMotionAwareOpen
    {
        get => GetValue(ConfigureBlankMaskWhenMotionAwareOpenProperty);
        set => SetValue(ConfigureBlankMaskWhenMotionAwareOpenProperty, value);
    }

    #endregion
    
    public Func<IPopupHostProvider, RawPointerEventArgs, bool>? ClickHidePredicate;
    public Action<Popup>? CloseAction;

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsDetectMouseClickEnabledProperty =
        AvaloniaProperty.Register<Popup, bool>(nameof(IsDetectMouseClickEnabled), true);
    
    internal static readonly DirectProperty<Popup, Direction> HostDecoratorDirectionProperty =
        AvaloniaProperty.RegisterDirect<Popup, Direction>(nameof(HostDecoratorDirection),
            o => o.HostDecoratorDirection,
            (o, v) => o.HostDecoratorDirection = v);

    internal bool IsDetectMouseClickEnabled
    {
        get => GetValue(IsDetectMouseClickEnabledProperty);
        set => SetValue(IsDetectMouseClickEnabledProperty, value);
    }
    
    private Direction _hostDecoratorDirection;

    internal Direction HostDecoratorDirection
    {
        get => _hostDecoratorDirection;
        private set => SetAndRaise(HostDecoratorDirectionProperty, ref _hostDecoratorDirection, value);
    }
    
    #endregion

    internal BaseMotionActor? MotionActor;
    private PlacementAwareDecorator? _placementAwareDecorator;
    private PopupBuddyLayer? _buddyLayer;
    private IDisposable? _selfLightDismissDisposable;
    private IManagedPopupPositionerPopup? _managedPopupPositioner;
    private bool _isNeedDetectFlip = true;
    private bool _ignoreIsOpenChanged;
    private CompositeDisposable? _hostDecoratorDisposable;

    // 在翻转之后或者恢复正常，会有属性的变动，在变动之后捕捉动画需要等一个事件循环，保证布局已经生效
    private bool _openAnimating;
    private bool _closeAnimating;
    private bool _motionAwareOpened;
    
    // 用于保证动画状态最终一致性
    private IDisposable? _overlayPopupHostOpenMotionDisposable;
    private IDisposable? _overlayPopupHostCloseMotionDisposable;

    // 当鼠标移走了，但是打开动画还没完成，我们需要记录下来这个信号
    internal bool RequestCloseWhereAnimationCompleted { get; set; }

    static Popup()
    {
        AffectsMeasure<Popup>(PlacementProperty);
        AffectsMeasure<Popup>(PlacementAnchorProperty);
        AffectsMeasure<Popup>(PlacementGravityProperty);
        IsLightDismissEnabledProperty.OverrideDefaultValue<Popup>(false);
    }

    public Popup()
    {
        this.ConfigureMotionBindingStyle();
        Closed += HandleClosed;
        Opened += HandleOpened;
        if (this is IPopupHostProvider popupHostProvider)
        {
            if (popupHostProvider.PopupHost != null)
            {
                HandlePopupHostChanged(popupHostProvider.PopupHost);
            }
            else
            {
                popupHostProvider.PopupHostChanged += HandlePopupHostChanged; 
            }
        }

        ConfigureInstanceStyles();
    }

    private void ConfigureInstanceStyles()
    {
        var style = new Style();
        style.Add(MaskShadowsProperty, SharedTokenKey.BoxShadowsSecondary);
        style.Add(MotionDurationProperty, SharedTokenKey.MotionDurationMid);
    }
    
    private void HandlePopupHostChanged(IPopupHost? popupHost)
    {
        _hostDecoratorDisposable?.Dispose();
        _placementAwareDecorator = null;
        if (popupHost is PopupRoot popupRoot)
        {
            MotionActor              = popupRoot.FindDescendantOfType<MotionActor>();
            _placementAwareDecorator = popupRoot.FindDescendantOfType<PlacementAwareDecorator>();
        }
        else if (popupHost is OverlayPopupHost overlayPopupHost)
        {
            MotionActor              = overlayPopupHost.FindDescendantOfType<MotionActor>();
            _placementAwareDecorator = overlayPopupHost.FindDescendantOfType<PlacementAwareDecorator>();
            var popupContent = overlayPopupHost.FindDescendantOfType<OverlayPopupContent>();
            Debug.Assert(popupContent != null);
            BindUtils.RelayBind(this, IsFlippedProperty, popupContent, OverlayPopupContent.IsFlippedProperty);
        }
        if (_placementAwareDecorator != null)
        {
            _hostDecoratorDisposable = new CompositeDisposable(2);
            _hostDecoratorDisposable.Add(BindUtils.RelayBind(this, MarginToAnchorProperty, _placementAwareDecorator, PlacementAwareDecorator.MarginToAnchorProperty));
            _hostDecoratorDisposable.Add(BindUtils.RelayBind(this, HostDecoratorDirectionProperty, _placementAwareDecorator, PlacementAwareDecorator.MarginPlacementProperty));
        }
    }
    
    protected Control? GetEffectivePlacementTarget()
    {
        return PlacementTarget ?? this.FindLogicalAncestorOfType<Control>();
    }

    private void HandleClosed(object? sender, EventArgs? args)
    {
        _selfLightDismissDisposable?.Dispose();
        if (IgnoreFirstDetected)
        {
            _firstDetected = true;
        }
    }

    private void HandleOpened(object? sender, EventArgs? args)
    {
        if (Host is PopupRoot popupRoot)
        {
            var popupPositioner = popupRoot.PlatformImpl?.PopupPositioner;
            if (popupPositioner is ManagedPopupPositioner managedPopupPositioner)
            {
                _managedPopupPositioner = managedPopupPositioner.GetManagedPopupPositionerPopup();
            }
#if DEBUG
            if (popupRoot is TopLevel topLevel)
            {
                topLevel.AttachDevTools();
            }
#endif
        }

        var placementTarget = GetEffectivePlacementTarget();
        if (placementTarget is not null)
        {
            if (_isNeedDetectFlip)
            {
                if (Placement != PlacementMode.Pointer && Placement != PlacementMode.Center)
                {
                    if (Host is PopupRoot)
                    {
                        AdjustPopupHostPosition(placementTarget);
                    }
                    else if (Host is OverlayPopupHost overlayPopupHost)
                    {
                        overlayPopupHost.PropertyChanged += HandleOverlayPopupHostPropertyChanged;
                    }
                }
            }

            // 如果没有启动，我们使用自己的处理函数，一般是为了增加我们自己的动画效果
            if (!IsLightDismissEnabled)
            {
                var inputManager = AvaloniaLocator.Current.GetService<IInputManager>()!;
                _selfLightDismissDisposable = inputManager.Process.Subscribe(HandleMouseClick);
            }
        }
    }

    private void HandleOverlayPopupHostPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (Host is OverlayPopupHost overlayPopupHost)
        {
            if (change.Property == Canvas.LeftProperty ||
                change.Property == Canvas.TopProperty)
            {
                var left = Canvas.GetLeft(overlayPopupHost);
                var top = Canvas.GetTop(overlayPopupHost);
                if (!double.IsNaN(left) && !double.IsNaN(top))
                {
                    overlayPopupHost.PropertyChanged -= HandleOverlayPopupHostPropertyChanged;
                    var placementTarget = GetEffectivePlacementTarget();
                    if (placementTarget is not null)
                    {
                        AdjustPopupHostPosition(placementTarget);
                    }
                }
            }
        }
    }
    
    // 正常的菜单项点击的时候第一次是需要忽略点击的探测的，不然按钮会被关闭
    // 这个可以优化动态探测 Popup 的 parent
    internal bool IgnoreFirstDetected { get; set; } = true;
    private bool _firstDetected = true;

    private void HandleMouseClick(RawInputEventArgs args)
    {
        if (!IsMotionAwareOpen)
        {
            return;
        }

        if (IsDetectMouseClickEnabled)
        {
            if (args is RawPointerEventArgs pointerEventArgs)
            {
                if (pointerEventArgs.Type == RawPointerEventType.LeftButtonUp)
                {
                    if ((IgnoreFirstDetected && _firstDetected) || _openAnimating)
                    {
                        if (IgnoreFirstDetected && _firstDetected)
                        {
                            _firstDetected = false;
                        }
                        return;
                    }
                    
                    if (this is IPopupHostProvider popupHostProvider)
                    {
                        if (ClickHidePredicate is not null)
                        {
                            if (ClickHidePredicate(popupHostProvider, pointerEventArgs))
                            {
                                if (CloseAction != null)
                                {
                                    CloseAction.Invoke(this);
                                }
                                else
                                {
                                    MotionAwareClose();
                                }
                            }
                        }
                        else
                        {
                            if (popupHostProvider.PopupHost is PopupRoot popupRoot)
                            {
                                if (popupRoot != pointerEventArgs.Root)
                                {
                                    if (CloseAction != null)
                                    {
                                        CloseAction.Invoke(this);
                                    }
                                    else
                                    {
                                        MotionAwareClose();
                                    }
                                }
                            }
                            else if (popupHostProvider.PopupHost is OverlayPopupHost overlayPopupHost)
                            {
                                if (args.Root is Control root)
                                {
                                    var offset = overlayPopupHost.TranslatePoint(default, root);
                                    if (offset.HasValue)
                                    {
                                        var bounds = new Rect(offset.Value, overlayPopupHost.Bounds.Size);
                                        if (!bounds.Contains(pointerEventArgs.Position))
                                        {
                                            if (CloseAction != null)
                                            {
                                                CloseAction.Invoke(this);
                                            }
                                            else
                                            {
                                                MotionAwareClose();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void CreatePopupRootBuddyLayer()
    {
        Debug.Assert(_buddyLayer == null);
        var topLevel = TopLevel.GetTopLevel(PlacementTarget ?? Parent as Visual);
        Debug.Assert(topLevel is not null);
        _buddyLayer         = new PopupBuddyLayer(this, topLevel);
        _buddyLayer.Topmost = true;
    }

    internal (bool, bool) CalculateFlipInfo(Size translatedSize, Rect anchorRect, PopupAnchor anchor,
                                            PopupGravity gravity,
                                            Point offset)
    {
        var bounds = GetBounds(anchorRect);
        return CalculateFlipInfo(bounds, translatedSize, anchorRect, anchor, gravity, offset);
    }

    internal static (bool, bool) CalculateFlipInfo(Rect bounds, Size translatedSize, Rect anchorRect,
                                                   PopupAnchor anchor,
                                                   PopupGravity gravity,
                                                   Point offset)
    {
        var result = (false, false);

        bool FitsInBounds(Rect rc, PopupAnchor edge = PopupAnchor.AllMask)
        {
            if ((edge.HasFlag(PopupAnchor.Left) && rc.X < bounds.X) ||
                (edge.HasFlag(PopupAnchor.Top) && rc.Y < bounds.Y) ||
                (edge.HasFlag(PopupAnchor.Right) && rc.Right > bounds.Right) ||
                (edge.HasFlag(PopupAnchor.Bottom) && rc.Bottom > bounds.Bottom))
            {
                return false;
            }

            return true;
        }

        Rect GetUnconstrained(PopupAnchor a, PopupGravity g)
        {
            return new Rect(PopupUtils.Gravitate(PopupUtils.GetAnchorPoint(anchorRect, a), translatedSize, g) + offset,
                translatedSize);
        }

        var geo = GetUnconstrained(anchor, gravity);
        // If flipping geometry and anchor is allowed and helps, use the flipped one,
        // otherwise leave it as is
        if (!FitsInBounds(geo, PopupAnchor.HorizontalMask))
        {
            result.Item1 = true;
        }

        if (!FitsInBounds(geo, PopupAnchor.VerticalMask))
        {
            result.Item2 = true;
        }

        return result;
    }

    private Rect GetBounds(Rect anchorRect)
    {
        IManagedPopupPositionerPopup? positionerPopup = null;
        if (!ShouldUseOverlayLayer)
        {
            positionerPopup = _managedPopupPositioner;
        }
        else
        {
            positionerPopup = Host as IManagedPopupPositionerPopup;
        }
        if (positionerPopup is null)
        {
            throw new InvalidOperationException("ManagedPopupPositioner is null");
        }
        var parentGeometry = positionerPopup.ParentClientAreaScreenGeometry;
        var screens        = positionerPopup.Screens;
        return GetBounds(anchorRect, parentGeometry, screens);
    }

    private static Rect GetBounds(Rect anchorRect, Rect parentGeometry,
                                  IReadOnlyList<ManagedPopupPositionerScreenInfo> screens)
    {
        var targetScreen = screens.FirstOrDefault(s => s.Bounds.ContainsExclusive(anchorRect.TopLeft))
                           ?? screens.FirstOrDefault(s => s.Bounds.Intersects(anchorRect))
                           ?? screens.FirstOrDefault(s => s.Bounds.ContainsExclusive(parentGeometry.TopLeft))
                           ?? screens.FirstOrDefault(s => s.Bounds.Intersects(parentGeometry))
                           ?? screens.FirstOrDefault();

        if (targetScreen != null &&
            targetScreen.WorkingArea.Width == 0 && targetScreen.WorkingArea.Height == 0)
        {
            return targetScreen.Bounds;
        }

        return targetScreen?.WorkingArea
               ?? new Rect(0, 0, double.MaxValue, double.MaxValue);
    }

    /// <summary>
    /// 在这里处理翻转问题
    /// </summary>
    internal void AdjustPopupHostPosition(Control placementTarget)
    {
        var topLevel  = TopLevel.GetTopLevel(placementTarget)!;

        Point location  = default;
        Size  popupSize = default;
        if (Host is PopupRoot popupRoot)
        {
            if (popupRoot.PlatformImpl?.Position is not null)
            {
                location = new Point(popupRoot.PlatformImpl.Position.X, popupRoot.PlatformImpl.Position.Y);
            }
            popupSize = popupRoot.ClientSize;
        }
        else if (Host is OverlayPopupHost overlayPopupHost)
        {
            location  = new Point(Canvas.GetLeft(overlayPopupHost), Canvas.GetTop(overlayPopupHost));
            popupSize = overlayPopupHost.Bounds.Size;
        }
        
        IManagedPopupPositionerPopup? positionerPopup = null;
        if (!ShouldUseOverlayLayer)
        {
            positionerPopup = _managedPopupPositioner;
        }
        else
        {
            positionerPopup = Host as IManagedPopupPositionerPopup;
        }
        Debug.Assert(positionerPopup != null);
        var scaling      = positionerPopup.Scaling;
        var parentOrigin = positionerPopup.ParentClientAreaScreenGeometry.TopLeft;

        if (Placement != PlacementMode.Center && Placement != PlacementMode.Pointer)
        {
            var effectiveOffsetX = location.X;
            var effectiveOffsetY = location.Y;
            // 计算是否 flip
            var parameters = new PopupPositionerParameters();
            var offset     = new Point(HorizontalOffset, VerticalOffset);
            parameters.ConfigurePosition(topLevel,
                placementTarget,
                Placement,
                offset,
                PlacementAnchor,
                PlacementGravity,
                PopupPositionerConstraintAdjustment.All,
                null,
                FlowDirection);
            
            var anchorRect = new Rect(
                parameters.AnchorRectangle.TopLeft * scaling,
                parameters.AnchorRectangle.Size * scaling);
            
            anchorRect = anchorRect.Translate(parentOrigin);

            var flipInfo = CalculateFlipInfo(popupSize * scaling,
                anchorRect,
                parameters.Anchor,
                parameters.Gravity,
                offset * scaling);
            
            if (flipInfo.Item1 || flipInfo.Item2)
            {
                var flipPlacement        = GetFlipPlacement(Placement, flipInfo.Item1, flipInfo.Item2);
                var flipAnchorAndGravity = PopupUtils.GetAnchorAndGravity(flipPlacement);

                Placement        = flipPlacement;
                PlacementAnchor  = flipAnchorAndGravity.Item1;
                PlacementGravity = flipAnchorAndGravity.Item2;
                
                IsFlipped           = true;
            }
            else
            {
                IsFlipped = false;
            }

            var effectivePosition = new Point(effectiveOffsetX, effectiveOffsetY);
            positionerPopup.MoveAndResize(effectivePosition, popupSize);
            PositionFlipped?.Invoke(this, new PopupFlippedEventArgs(IsFlipped));
        }
    }

    protected static PlacementMode GetFlipPlacement(PlacementMode placement, bool isHorizontalFlipped,
                                                    bool isVerticalFlipped)
    {
        return placement switch
        {
            PlacementMode.Left => isHorizontalFlipped ? PlacementMode.Right : PlacementMode.Left,
            PlacementMode.LeftEdgeAlignedTop => isHorizontalFlipped
                ? PlacementMode.RightEdgeAlignedTop
                : PlacementMode.LeftEdgeAlignedTop,
            PlacementMode.LeftEdgeAlignedBottom => isHorizontalFlipped
                ? PlacementMode.RightEdgeAlignedBottom
                : PlacementMode.LeftEdgeAlignedBottom,

            PlacementMode.Top => isVerticalFlipped ? PlacementMode.Bottom : PlacementMode.Top,
            PlacementMode.TopEdgeAlignedLeft => isVerticalFlipped
                ? PlacementMode.BottomEdgeAlignedLeft
                : PlacementMode.TopEdgeAlignedLeft,
            PlacementMode.TopEdgeAlignedRight => isVerticalFlipped
                ? PlacementMode.BottomEdgeAlignedRight
                : PlacementMode.TopEdgeAlignedRight,

            PlacementMode.Right => isHorizontalFlipped ? PlacementMode.Left : PlacementMode.Right,
            PlacementMode.RightEdgeAlignedTop => isHorizontalFlipped
                ? PlacementMode.LeftEdgeAlignedTop
                : PlacementMode.RightEdgeAlignedTop,
            PlacementMode.RightEdgeAlignedBottom => isHorizontalFlipped
                ? PlacementMode.LeftEdgeAlignedBottom
                : PlacementMode.RightEdgeAlignedBottom,

            PlacementMode.Bottom => isVerticalFlipped ? PlacementMode.Top : PlacementMode.Bottom,
            PlacementMode.BottomEdgeAlignedLeft => isVerticalFlipped
                ? PlacementMode.TopEdgeAlignedLeft
                : PlacementMode.BottomEdgeAlignedLeft,
            PlacementMode.BottomEdgeAlignedRight => isVerticalFlipped
                ? PlacementMode.TopEdgeAlignedRight
                : PlacementMode.BottomEdgeAlignedRight,

            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, "Invalid value for PlacementMode")
        };
    }

    public void MotionAwareOpen(Action? opened = null)
    {
        if (!IsMotionEnabled)
        {
            Open();
            if (!ShouldUseOverlayLayer)
            {
                CreatePopupRootBuddyLayer();
                _buddyLayer?.Attach();
                _buddyLayer?.Show();
            }
            opened?.Invoke();
            _motionAwareOpened = true;
            using (BeginIgnoringIsOpen())
            {
                SetCurrentValue(IsMotionAwareOpenProperty, true);
            }
            return;
        }
        
        if (_motionAwareOpened || _openAnimating || _closeAnimating)
        {
            opened?.Invoke();
            return;
        }
  
        Open();
        if (MotionActor == null)
        {
            opened?.Invoke();
            return;
        }
        _openAnimating = true;
        using (BeginIgnoringIsOpen())
        {
            SetCurrentValue(IsMotionAwareOpenProperty, true);
        }
        
        if (!ShouldUseOverlayLayer)
        {
            OpenPopupRootHost(opened);
        }
        else
        {
            OpenOverlayPopupHost(opened);
        }
    }

    private void OpenPopupRootHost(Action? opened = null)
    {
        CreatePopupRootBuddyLayer();
        _buddyLayer?.Attach();
        _buddyLayer?.Show();
        var shadowAwareLayer = _buddyLayer as IShadowAwareLayer;
        Debug.Assert(shadowAwareLayer != null);
        shadowAwareLayer.RunOpenMotion(null, () =>
        {
            opened?.Invoke();
            _openAnimating     = false;
            _motionAwareOpened = true;
        
            if (RequestCloseWhereAnimationCompleted)
            {
                RequestCloseWhereAnimationCompleted = false;
                Dispatcher.UIThread.Post(() => MotionAwareClose());
            }
        });
    }

    private void OpenOverlayPopupHost(Action? opened = null)
    {
        if (MotionActor == null)
        {
            opened?.Invoke();
            _openAnimating     = false;
            _motionAwareOpened = true;
            _overlayPopupHostOpenMotionDisposable?.Dispose();
            return;
        }
        var motion       = OpenMotion ?? new ZoomBigInMotion();
        if (MotionDuration != TimeSpan.Zero)
        {
            motion.Duration = MotionDuration;
        }
        
        var completedFuncCalled = false;
        
        _overlayPopupHostOpenMotionDisposable?.Dispose();
        _overlayPopupHostOpenMotionDisposable = DispatcherTimer.RunOnce(() =>
        {
            if (!completedFuncCalled)
            {
                opened?.Invoke();
                _openAnimating      = false;
                _motionAwareOpened  = true;
                completedFuncCalled = true;
            }
        
        }, motion.Duration * 1.2);
        
        motion.Run(MotionActor, null, () =>
        {
            _overlayPopupHostOpenMotionDisposable.Dispose();
            _overlayPopupHostOpenMotionDisposable = null;
            if (!completedFuncCalled)
            {
                opened?.Invoke();
                _openAnimating     = false;
                _motionAwareOpened = true;
        
                if (RequestCloseWhereAnimationCompleted)
                {
                    RequestCloseWhereAnimationCompleted = false;
                    Dispatcher.UIThread.Post(() => MotionAwareClose());
                }
                completedFuncCalled = true;
            }
        });
    }

    public async Task MotionAwareOpenAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        MotionAwareOpen(() => tcs.SetResult(true));
        await tcs.Task;
    }

    public void MotionAwareClose(Action? closed = null)
    {
        if ((!_motionAwareOpened && !_openAnimating) || _closeAnimating)
        {
            closed?.Invoke();
            return;
        }
        
        if (_openAnimating)
        {
            RequestCloseWhereAnimationCompleted = true;
            closed?.Invoke();
            return;
        }
        
        if (MotionActor == null)
        {
            closed?.Invoke();
            return;
        }
        
        if (!IsMotionEnabled || Host == null)
        {
            _isNeedDetectFlip = true;
            Close();
            if (!ShouldUseOverlayLayer)
            {
                _buddyLayer?.Detach();
                _buddyLayer = null;
            }
            closed?.Invoke();
            _motionAwareOpened = false;
            using (BeginIgnoringIsOpen())
            {
                SetCurrentValue(IsMotionAwareOpenProperty, false);
            }
            
            return;
        }
        
        using (BeginIgnoringIsOpen())
        {
            SetCurrentValue(IsMotionAwareOpenProperty, false);
        }
        
        _closeAnimating    = true;

        if (!ShouldUseOverlayLayer)
        {
            ClosePopupRootHost(closed);
        }
        else
        {
            CloseOverlayPopupHost(closed);
        }
    }

    private void ClosePopupRootHost(Action? closed = null)
    {
        var shadowAwareLayer = _buddyLayer as IShadowAwareLayer;
        Debug.Assert(shadowAwareLayer != null);
        shadowAwareLayer.RunCloseMotion(null, () =>
        {
            _buddyLayer?.Detach();
            _buddyLayer = null;
            Close();
            closed?.Invoke();
            _closeAnimating    = false;
            _motionAwareOpened = false;
            _isNeedDetectFlip  = true;
        });
    }

    private void CloseOverlayPopupHost(Action? closed = null)
    {
        if (MotionActor == null)
        {
            Close();
            closed?.Invoke();
            _closeAnimating    = false;
            _motionAwareOpened = false;
            _isNeedDetectFlip  = true;
            return;
        }

        var motion = CloseMotion ?? new ZoomBigOutMotion();
        if (MotionDuration != TimeSpan.Zero)
        {
            motion.Duration = MotionDuration;
        }
        
        var completedFuncCalled = false;
        
        _overlayPopupHostCloseMotionDisposable?.Dispose();
        _overlayPopupHostCloseMotionDisposable = DispatcherTimer.RunOnce(() =>
        {
            if (!completedFuncCalled)
            {
                Close();
                closed?.Invoke();
                _closeAnimating     = false;
                _motionAwareOpened  = false;
                _isNeedDetectFlip   = true;
                completedFuncCalled = true;
            }
        }, motion.Duration * 1.2);
        motion.Run(MotionActor, null, () =>
        {
            _overlayPopupHostCloseMotionDisposable.Dispose();
            _overlayPopupHostCloseMotionDisposable = null;
            
            if (!completedFuncCalled)
            {
                Close();
                closed?.Invoke();
                _closeAnimating     = false;
                _motionAwareOpened  = false;
                _isNeedDetectFlip   = true;
                completedFuncCalled = true;
            }
        });
    }

    public async Task MotionAwareCloseAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        MotionAwareClose(() => tcs.SetResult(true));
        await tcs.Task;
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsMotionAwareOpenProperty)
        {
            if (!_ignoreIsOpenChanged)
            {
                if (change.GetNewValue<bool>())
                {
                    MotionAwareOpen();
                }
                else
                {
                    MotionAwareClose();
                }
            }
        }
        else if (change.Property == PlacementProperty)
        {
            HostDecoratorDirection = GetHostDecoratorDirection(Placement);
        }
    }

    private Direction GetHostDecoratorDirection(PlacementMode placement)
    {
        var direction = PopupUtils.GetDirection(Placement);
        return direction switch
        {
            Direction.Left => Direction.Right,
            Direction.Top => Direction.Bottom,
            Direction.Right => Direction.Left,
            Direction.Bottom => Direction.Top,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid value for direction for Placement.")
        };
    }
    
    private IgnoreIsOpenScope BeginIgnoringIsOpen() => new IgnoreIsOpenScope(this);
    
    private readonly struct IgnoreIsOpenScope : IDisposable
    {
        private readonly Popup _owner;

        public IgnoreIsOpenScope(Popup owner)
        {
            _owner                      = owner;
            _owner._ignoreIsOpenChanged = true;
        }

        public void Dispose() => _owner._ignoreIsOpenChanged = false;
    }
}

public class PopupFlippedEventArgs : EventArgs
{
    public bool Flipped { get; set; }

    public PopupFlippedEventArgs(bool flipped)
    {
        Flipped = flipped;
    }
}
