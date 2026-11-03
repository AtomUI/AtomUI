using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling; 
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

public abstract class AbstractMarqueeLabel : TextBlock
{
    #region 公共属性定义

    public static readonly StyledProperty<double> CycleSpaceProperty =
        AvaloniaProperty.Register<AbstractMarqueeLabel, double>(nameof(CycleSpace));

    public static readonly StyledProperty<double> MoveSpeedProperty =
        AvaloniaProperty.Register<AbstractMarqueeLabel, double>(nameof(MoveSpeed), 150);

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
    private Animation? _animation;
    private bool _animationRunning;
    private double _lastDesiredWidth;
    private double _lastTextWidth;
    private double _pivotOffsetStartValue;

    static AbstractMarqueeLabel()
    {
        AffectsRender<AbstractMarqueeLabel>(PivotOffsetProperty, CycleSpaceProperty, MoveSpeedProperty);
        HorizontalAlignmentProperty.OverrideDefaultValue<AbstractMarqueeLabel>(HorizontalAlignment.Stretch);
        VerticalAlignmentProperty.OverrideDefaultValue<AbstractMarqueeLabel>(VerticalAlignment.Top);
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
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsPointerOverProperty)
            {
                if (IsPointerOver)
                {
                    _pivotOffsetStartValue = PivotOffset;
                    HandleCleanupMarqueeAnimation();
                }
                else
                {
                    if (DesiredSize.Width < _lastDesiredWidth ||
                        MathUtils.AreClose(DesiredSize.Width, _lastDesiredWidth))
                    {
                        ReConfigureAnimation();
                        HandleStartupMarqueeAnimation();
                    }
                }
                // 这里处理暂停事件
            }
            else if (change.Property == CycleSpaceProperty || change.Property == MoveSpeedProperty)
            {
                var originRunning = _animationRunning;
                ReConfigureAnimation();
                if (originRunning)
                {
                    HandleStartupMarqueeAnimation();
                }
            }
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

    private void HandleStartupMarqueeAnimation()
    {
        if (_animation is not null)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            var cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource = cancellationTokenSource;
            var animation = _animation;
            var animationTask = animation.RunAsync(this, cancellationTokenSource.Token);
            _animationRunning = true;
            this.Dispatcher.InvokeAsync(async () =>
            {
                await animationTask;
                if (ReferenceEquals(_animation, animation) &&
                    ReferenceEquals(_cancellationTokenSource, cancellationTokenSource))
                {
                    _animationRunning = false;
                    _animation = null;
                    _cancellationTokenSource = null;
                    cancellationTokenSource.Dispose();
                }
            });
        }
    }

    private void HandleCleanupMarqueeAnimation()
    {
        _animationRunning = false;
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void HandleLayoutUpdated(Size size, Size availableSize)
    {
        if (availableSize.Width > size.Width || MathUtils.AreClose(availableSize.Width, size.Width))
        {
            HandleCleanupMarqueeAnimation();
            _animation = null;
            PivotOffset            = 0;
            _lastDesiredWidth      = 0;
            _lastTextWidth         = 0;
            _pivotOffsetStartValue = 0;
        }
        else
        {
            _lastDesiredWidth = size.Width;
            var textWidthChanged = !MathUtils.AreClose(_lastTextWidth, TextLayout.Width);
            _lastTextWidth = TextLayout.Width;

            if (textWidthChanged || !_animationRunning)
            {
                ReConfigureAnimation();
                if (!IsPointerOver)
                {
                    HandleStartupMarqueeAnimation();
                }
            }
        }
    }

    private void ReConfigureAnimation()
    {
        if (_animation is not null && _animationRunning)
        {
            HandleCleanupMarqueeAnimation();
        }

        var cycleWidth = _lastTextWidth + CycleSpace;
        if (_pivotOffsetStartValue < -cycleWidth)
        {
            _pivotOffsetStartValue %= cycleWidth;
        }

        var delta = _pivotOffsetStartValue - cycleWidth - _pivotOffsetStartValue;
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
        var offset = TextLayout.OverhangLeading + PivotOffset;
        TextLayout.Draw(context, origin + new Point(offset, 0));
        if (PivotOffset < 0)
        {
            TextLayout.Draw(context, origin + new Point(offset + _lastTextWidth + CycleSpace, 0));
        }
    }
}
