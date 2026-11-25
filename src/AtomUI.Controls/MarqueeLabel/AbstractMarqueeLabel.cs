using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

public abstract class AbstractMarqueeLabel : TextBlock
{
    #region 公共属性定义

    public static readonly StyledProperty<double> CycleSpaceProperty =
        AvaloniaProperty.Register<AbstractMarqueeLabel, double>(nameof(CycleSpace));

    public static readonly StyledProperty<double> MoveSpeedProperty =
        AvaloniaProperty.Register<AbstractMarqueeLabel, double>(nameof(MoveSpeed), 150);

    public static readonly StyledProperty<TimeSpan> StartDelayProperty =
        AvaloniaProperty.Register<AbstractMarqueeLabel, TimeSpan>(nameof(StartDelay), TimeSpan.Zero);

    public static readonly StyledProperty<bool> IsPausedOnPointerOverProperty =
        AvaloniaProperty.Register<AbstractMarqueeLabel, bool>(nameof(IsPausedOnPointerOver), true);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractMarqueeLabel>();

    /// <summary>Usually bound to the <c>EnableMotion</c> token; when false overflowing text stays where it stopped.</summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// 默认的间隔
    /// </summary>
    public double CycleSpace
    {
        get => GetValue(CycleSpaceProperty);
        set => SetValue(CycleSpaceProperty, value);
    }

    /// <summary>
    /// 移动速度
    /// </summary>
    public double MoveSpeed
    {
        get => GetValue(MoveSpeedProperty);
        set => SetValue(MoveSpeedProperty, value);
    }

    /// <summary>
    /// Wait before every (re)start of the scrolling animation; the text stays still meanwhile.
    /// </summary>
    public TimeSpan StartDelay
    {
        get => GetValue(StartDelayProperty);
        set => SetValue(StartDelayProperty, value);
    }

    /// <summary>
    /// Whether scrolling pauses while the pointer is over the label. Defaults to true.
    /// </summary>
    public bool IsPausedOnPointerOver
    {
        get => GetValue(IsPausedOnPointerOverProperty);
        set => SetValue(IsPausedOnPointerOverProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> PivotOffsetProperty =
        AvaloniaProperty.Register<AbstractMarqueeLabel, double>(nameof(PivotOffset));

    /// <summary>
    /// 内部动画使用，当前焦点，也就是文字最左侧
    /// </summary>
    internal double PivotOffset
    {
        get => GetValue(PivotOffsetProperty);
        set => SetValue(PivotOffsetProperty, value);
    }
    
    #endregion
    
    private CancellationTokenSource? _cancellationTokenSource;
    private DispatcherTimer? _startDelayTimer;
    private Animation? _animation;
    private MarqueeTextVisual? _compositedText;
    private TopLevel? _compositedTopLevel;
    private bool _animationRunning;
    private double _lastDesiredWidth;
    private double _lastTextWidth;
    private double _pivotOffsetStartValue;
    private readonly MotionGate _motionGate;

    // Composition has no client-side readback of the current translation.
    // Opt in only for labels that restart from the beginning when the gate reopens.
    internal virtual bool UseCompositedMarquee => false;

    /// <summary>True while a scrolling animation is running.</summary>
    internal bool IsMarqueeAnimating => _cancellationTokenSource is not null || _compositedText is not null;

    /// <summary>True while a delayed start is waiting for its timer.</summary>
    internal bool HasPendingStartDelay => _startDelayTimer is not null;

    static AbstractMarqueeLabel()
    {
        // Desktop labels keep scrolling unless a theme binds this to EnableMotion.
        IsMotionEnabledProperty.OverrideDefaultValue<AbstractMarqueeLabel>(true);
        AffectsRender<AbstractMarqueeLabel>(PivotOffsetProperty, CycleSpaceProperty, MoveSpeedProperty);
        HorizontalAlignmentProperty.OverrideDefaultValue<AbstractMarqueeLabel>(HorizontalAlignment.Stretch);
        VerticalAlignmentProperty.OverrideDefaultValue<AbstractMarqueeLabel>(VerticalAlignment.Top);
    }

    protected AbstractMarqueeLabel()
    {
        _motionGate = new MotionGate(this, HandleShouldRunChanged);
    }

    private void HandleShouldRunChanged(bool shouldRun)
    {
        if (shouldRun)
        {
            // Resume from where the text stopped, as after a pointer-over pause.
            if (CanStartMarquee())
            {
                ReConfigureAnimation();
                HandleStartupMarqueeAnimation();
            }
        }
        else if (IsMarqueeAnimating)
        {
            _pivotOffsetStartValue = PivotOffset;
            HandleCleanupMarqueeAnimation();
        }
        else
        {
            // A closed gate keeps no timer; reopening starts a full delay again.
            CancelStartDelay();
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        HandleStartupMarqueeAnimation();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        HandleCleanupMarqueeAnimation();
        _pivotOffsetStartValue = 0;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsMotionEnabledProperty)
        {
            _motionGate.IsMotionEnabled = IsMotionEnabled;
        }
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsPointerOverProperty)
            {
                if (IsPausedOnPointerOver)
                {
                    HandlePointerOverChanged();
                }
            }
            else if (change.Property == IsPausedOnPointerOverProperty)
            {
                // Turning the pause off while hovering resumes; turning it on while hovering pauses.
                if (IsPointerOver)
                {
                    HandlePointerOverChanged();
                }
            }
            else if (change.Property == CycleSpaceProperty || change.Property == MoveSpeedProperty)
            {
                var originRunning = _animationRunning || _compositedText is not null;
                ReConfigureAnimation();
                if (originRunning)
                {
                    HandleStartupMarqueeAnimation();
                }
            }
            else if (_compositedText is not null && change.Property == BoundsProperty)
            {
                _compositedText.InvalidateVisual();
            }
            else if (UseCompositedMarquee && change.Property == TextWrappingProperty && TextWrapping != TextWrapping.NoWrap)
            {
                HandleCleanupMarqueeAnimation();
            }
        }
    }

    private void HandlePointerOverChanged()
    {
        if (IsPausedByPointerOver)
        {
            _pivotOffsetStartValue = PivotOffset;
            HandleCleanupMarqueeAnimation();
        }
        else if (DesiredSize.Width < _lastDesiredWidth ||
                 MathUtils.AreClose(DesiredSize.Width, _lastDesiredWidth))
        {
            ReConfigureAnimation();
            HandleStartupMarqueeAnimation();
        }
    }

    public sealed override void ApplyTemplate()
    {
        base.ApplyTemplate();
        // 为了强制
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        HandleLayoutUpdated(size, availableSize);
        return size;
    }

    private double CalculateDuration(double distance)
    {
        // 计算持续时间，确保至少有一毫秒的持续时间以避免除以零的错误
        return 4 * Math.Max(1, distance / MoveSpeed * 1000);
    }

    // The single start condition shared by layout, the motion gate, pointer changes and the delay timer:
    // the text overflows, the label is in the tree, motion may run and no hover pause is in effect.
    private bool CanStartMarquee()
    {
        return _lastDesiredWidth > 0 &&
               (!UseCompositedMarquee || TextWrapping == TextWrapping.NoWrap) &&
               this.IsAttachedToVisualTree() &&
               _motionGate.ShouldRun &&
               !IsPausedByPointerOver;
    }

    private bool IsPausedByPointerOver => IsPointerOver && IsPausedOnPointerOver;

    private void HandleStartupMarqueeAnimation()
    {
        if ((!UseCompositedMarquee && _animation is null) || !CanStartMarquee())
        {
            return;
        }
        if (_startDelayTimer is not null)
        {
            // A delayed start is already pending; the timer starts whatever configuration is current by then.
            return;
        }
        var delay = StartDelay;
        if (delay <= TimeSpan.Zero)
        {
            StartMarqueeAnimation();
            return;
        }
        // The text rests during the delay, so a still-running run must stop now.
        HandleCleanupMarqueeAnimation();
        var timer = new DispatcherTimer { Interval = delay };
        timer.Tick += (_, _) =>
        {
            if (!ReferenceEquals(_startDelayTimer, timer))
            {
                return;
            }
            CancelStartDelay();
            if (!CanStartMarquee())
            {
                return;
            }
            // The completion of the run stopped above may have discarded the animation while the delay was pending.
            if (!UseCompositedMarquee && _animation is null)
            {
                ReConfigureAnimation();
            }
            StartMarqueeAnimation();
        };
        _startDelayTimer = timer;
        timer.Start();
    }

    private void StartMarqueeAnimation()
    {
        if (UseCompositedMarquee)
        {
            StartCompositedMarquee();
            return;
        }

        if (_animation is null)
        {
            return;
        }
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        var cancellation = new CancellationTokenSource();
        _cancellationTokenSource = cancellation;
        var animationTask = _animation.RunAsync(this, cancellation.Token);
        _animationRunning = true;
        this.Dispatcher.InvokeAsync(async () =>
        {
            await animationTask;
            // A run stopped and immediately replaced completes after its successor started; it must not clear
            // the successor's state.
            if (_cancellationTokenSource is null || ReferenceEquals(_cancellationTokenSource, cancellation))
            {
                _animationRunning = false;
                _animation        = null;
            }
        });
    }

    private void CancelStartDelay()
    {
        if (_startDelayTimer is not null)
        {
            _startDelayTimer.Stop();
            _startDelayTimer = null;
        }
    }

    private void HandleCleanupMarqueeAnimation()
    {
        CancelStartDelay();
        StopCompositedMarquee();
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void HandleLayoutUpdated(Size size, Size availableSize)
    {
        if ((UseCompositedMarquee && TextWrapping != TextWrapping.NoWrap) ||
            availableSize.Width > size.Width || MathUtils.AreClose(availableSize.Width, size.Width))
        {
            HandleCleanupMarqueeAnimation();
            PivotOffset            = 0;
            _lastDesiredWidth      = 0;
            _pivotOffsetStartValue = 0;
        }
        else
        {
            if (UseCompositedMarquee || !MathUtils.AreClose(_lastDesiredWidth, size.Width))
            {
                _lastDesiredWidth = size.Width;
                if (!MathUtils.AreClose(_lastTextWidth, TextLayout.Width))
                {
                    _lastTextWidth = TextLayout.Width;
                    StopCompositedMarquee();
                }
            }

            if (_compositedText is not null)
            {
                _compositedText.InvalidateVisual();
            }
            else if (!_animationRunning && _startDelayTimer is null)
            {
                ReConfigureAnimation();
                HandleStartupMarqueeAnimation();
            }
        }
    }

    private void ReConfigureAnimation()
    {
        if (UseCompositedMarquee)
        {
            StopCompositedMarquee();
            return;
        }

        if (_animation is not null && _animationRunning)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        var cycleWidth = _lastTextWidth + CycleSpace;
        if (_pivotOffsetStartValue < -cycleWidth)
        {
            _pivotOffsetStartValue %= cycleWidth;
        }

        _animation = new Animation
        {
            IterationCount    = new IterationCount(long.MaxValue),
            PlaybackBehavior = PlaybackBehavior.OnlyIfVisible,
            Children =
            {
                new KeyFrame
                {
                    Cue     = new Cue(0.0d),
                    Setters = { new Setter(PivotOffsetProperty, _pivotOffsetStartValue) }
                },
                new KeyFrame
                {
                    Cue     = new Cue(1.0d),
                    Setters = { new Setter(PivotOffsetProperty, _pivotOffsetStartValue - cycleWidth) }
                }
            },
            FillMode = FillMode.Both,
            Duration = TimeSpan.FromMilliseconds(CalculateDuration(_lastTextWidth + CycleSpace))
        };
    }

    protected override void RenderTextLayout(DrawingContext context, Point origin)
    {
        if (_compositedText is not null)
        {
            context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));
            return;
        }

        DrawMarqueeText(context, origin, PivotOffset, PivotOffset < 0);
    }

    private void DrawMarqueeText(DrawingContext context, Point origin, double pivot, bool drawSecondCopy)
    {
        var textLayout = TextLayout;
        var offset = textLayout.OverhangLeading + pivot;
        textLayout.Draw(context, origin + new Point(offset, 0));
        if (drawSecondCopy)
        {
            textLayout.Draw(context, origin + new Point(offset + _lastTextWidth + CycleSpace, 0));
        }
    }

    private void StartCompositedMarquee()
    {
        var cycleWidth = _lastTextWidth + CycleSpace;
        if (_compositedText is not null || !(cycleWidth > 0) || ElementComposition.GetElementVisual(this) is null)
        {
            return;
        }

        var text = new MarqueeTextVisual(this);
        VisualChildren.Add(text);
        var visual = ElementComposition.GetElementVisual(text)!;
        var animation = visual.Compositor.CreateVector3DKeyFrameAnimation();
        animation.Duration = TimeSpan.FromMilliseconds(CalculateDuration(cycleWidth));
        animation.IterationBehavior = AnimationIterationBehavior.Forever;
        var linear = new LinearEasing();
        animation.InsertKeyFrame(0, default, linear);
        animation.InsertKeyFrame(1, new Vector3D(-cycleWidth, 0, 0), linear);
        visual.StartAnimation(nameof(CompositionVisual.Translation), animation);
        _compositedText = text;
        _compositedTopLevel = TopLevel.GetTopLevel(this);
        if (_compositedTopLevel is not null)
        {
            _compositedTopLevel.ScalingChanged += HandleCompositedScalingChanged;
        }
        InvalidateVisual();
    }

    private void HandleCompositedScalingChanged(object? sender, EventArgs e) => _compositedText?.InvalidateVisual();

    private void StopCompositedMarquee()
    {
        if (_compositedText is { } text)
        {
            _compositedText = null;
            if (_compositedTopLevel is not null)
            {
                _compositedTopLevel.ScalingChanged -= HandleCompositedScalingChanged;
                _compositedTopLevel = null;
            }
            if (ElementComposition.GetElementVisual(text) is { } visual)
            {
                // A hidden ancestor can defer the tree update. Queue a direct value on the compositor
                // to cancel both an installed and a pending animation. The client value is already zero,
                // so change it first to make the zero assignment part of the batch.
                visual.Translation = new Vector3D(1, 0, 0);
                visual.Translation = default;
            }
            VisualChildren.Remove(text);
            InvalidateVisual();
        }
    }

    private sealed class MarqueeTextVisual(AbstractMarqueeLabel owner) : Visual
    {
        public override void Render(DrawingContext context)
        {
            // The child uses the owner's shaped text and the same origin as TextBlock.Render.
            var padding = owner.Padding;
            if (owner.UseLayoutRounding)
            {
                padding = LayoutHelper.RoundLayoutThickness(padding, LayoutHelper.GetLayoutScale(owner));
            }

            var top = padding.Top;
            var heightDifference = owner.Bounds.Height - owner.TextLayout.Height;
            if (heightDifference < 0)
            {
                top += owner.VerticalAlignment switch
                {
                    VerticalAlignment.Center => heightDifference / 2,
                    VerticalAlignment.Bottom => heightDifference,
                    _ => 0
                };
            }

            owner.DrawMarqueeText(context, new Point(padding.Left, top), 0, true);
        }
    }
}
