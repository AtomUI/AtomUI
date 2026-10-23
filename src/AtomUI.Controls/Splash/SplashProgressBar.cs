using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.Composition;

namespace AtomUI.Controls;

/// <summary>
/// Splash 专用进度条：无模板，在 Render 中绘制轨道与确定进度；不确定进度由挂在本控件上的自定义合成视觉在渲染线程按时间绘制，
/// 提交期间 UI 线程被占用时照常移动。不能给布局子元素做 Offset 动画：Visual.SynchronizeCompositionProperties 每次同步都会改写
/// 布局元素合成视觉的 Offset 等属性并移除其上的动画，进度块会停在布局原点。合成视觉的创建与尺寸依赖挂接后的合成器与布局结果，
/// ControlTheme 无法表达，因此写在代码中。
/// </summary>
public class SplashProgressBar : Control, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<SplashProgressBar, double>(nameof(Value), coerce: static (_, v) => Math.Clamp(v, 0d, 1d));

    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<SplashProgressBar, bool>(nameof(IsIndeterminate), true);

    public static readonly StyledProperty<IBrush?> TrackBrushProperty =
        AvaloniaProperty.Register<SplashProgressBar, IBrush?>(nameof(TrackBrush));

    public static readonly StyledProperty<IBrush?> IndicatorBrushProperty =
        AvaloniaProperty.Register<SplashProgressBar, IBrush?>(nameof(IndicatorBrush));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<SplashProgressBar, CornerRadius>(nameof(CornerRadius));

    public static readonly StyledProperty<TimeSpan> CycleDurationProperty =
        AvaloniaProperty.Register<SplashProgressBar, TimeSpan>(nameof(CycleDuration), TimeSpan.FromSeconds(1.2),
            coerce: static (_, value) => value < TimeSpan.Zero ? TimeSpan.Zero : value);

    public static readonly StyledProperty<double> SegmentRatioProperty =
        AvaloniaProperty.Register<SplashProgressBar, double>(nameof(SegmentRatio), 0.3,
            coerce: static (_, value) => double.IsFinite(value) ? Math.Clamp(value, 0, 1) : 0);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SplashProgressBar>(
            new StyledPropertyMetadata<bool>(true));

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public IBrush? TrackBrush
    {
        get => GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public IBrush? IndicatorBrush
    {
        get => GetValue(IndicatorBrushProperty);
        set => SetValue(IndicatorBrushProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public TimeSpan CycleDuration
    {
        get => GetValue(CycleDurationProperty);
        set => SetValue(CycleDurationProperty, value);
    }

    public double SegmentRatio
    {
        get => GetValue(SegmentRatioProperty);
        set => SetValue(SegmentRatioProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    private CompositionCustomVisual? _indicatorVisual;
    private CompositeDisposable? _effectiveVisibilitySubscriptions;
    private bool _isEffectivelyVisible;

    static SplashProgressBar()
    {
        AffectsRender<SplashProgressBar>(ValueProperty, IsIndeterminateProperty, TrackBrushProperty,
            IndicatorBrushProperty, CornerRadiusProperty);
    }

    public SplashProgressBar()
    {
        ClipToBounds = true;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (ElementComposition.GetElementVisual(this)?.Compositor is not { } compositor)
        {
            return;
        }
        _indicatorVisual = compositor.CreateCustomVisual(new IndeterminateIndicatorHandler());
        _indicatorVisual.Size = new Vector(Bounds.Width, Bounds.Height);
        ElementComposition.SetElementChildVisual(this, _indicatorVisual);
        var subscriptions = new CompositeDisposable();
        _effectiveVisibilitySubscriptions = subscriptions;
        this.TrackEffectiveVisibility(HandleEffectiveVisibilityChanged, subscriptions);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _effectiveVisibilitySubscriptions?.Dispose();
        _effectiveVisibilitySubscriptions = null;
        _isEffectivelyVisible = false;
        SyncIndicator();
        if (_indicatorVisual is not null)
        {
            ElementComposition.SetElementChildVisual(this, null);
            _indicatorVisual = null;
        }
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsIndeterminateProperty ||
            change.Property == IndicatorBrushProperty ||
            change.Property == CornerRadiusProperty ||
            change.Property == CycleDurationProperty ||
            change.Property == SegmentRatioProperty ||
            change.Property == IsMotionEnabledProperty)
        {
            SyncIndicator();
        }
    }

    protected override Size MeasureOverride(Size availableSize) => new(0, 0);

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_indicatorVisual is not null)
        {
            _indicatorVisual.Size = new Vector(finalSize.Width, finalSize.Height);
        }
        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);
        if (TrackBrush is { } track)
        {
            context.DrawRectangle(track, null, new RoundedRect(bounds, CornerRadius));
        }
        if (!IsIndeterminate && IndicatorBrush is { } indicator && Value > 0)
        {
            context.DrawRectangle(indicator, null,
                new RoundedRect(new Rect(0, 0, bounds.Width * Value, bounds.Height), CornerRadius));
        }
    }

    private void SyncIndicator()
    {
        _indicatorVisual?.SendHandlerMessage(
            new IndicatorState(IsIndeterminate && _isEffectivelyVisible,
                IndicatorBrush?.ToImmutable(), CornerRadius, IsMotionEnabled, CycleDuration, SegmentRatio));
    }

    private void HandleEffectiveVisibilityChanged(bool visible)
    {
        if (_isEffectivelyVisible != visible)
        {
            _isEffectivelyVisible = visible;
            SyncIndicator();
        }
    }

    private sealed record IndicatorState(bool IsActive, IImmutableBrush? Brush, CornerRadius Radius,
        bool IsMotionEnabled, TimeSpan CycleDuration, double SegmentRatio);

    /// <summary>运行在渲染线程：按合成时钟计算进度块位置并逐帧重绘，不依赖 UI 线程。</summary>
    private sealed class IndeterminateIndicatorHandler : CompositionCustomVisualHandler
    {
        private bool _isActive;
        private IImmutableBrush? _brush;
        private CornerRadius _radius;
        private TimeSpan _cycleDuration;
        private double _segmentRatio;
        private bool _isAnimating;

        public override void OnMessage(object message)
        {
            if (message is not IndicatorState state)
            {
                return;
            }
            var wasAnimating = _isAnimating;
            _isActive = state.IsActive;
            _brush = state.Brush;
            _radius = state.Radius;
            _cycleDuration = state.CycleDuration;
            _segmentRatio = state.SegmentRatio;
            _isAnimating = _isActive && _brush is not null && state.IsMotionEnabled && _cycleDuration > TimeSpan.Zero && _segmentRatio > 0;
            Invalidate();
            if (_isAnimating && !wasAnimating)
            {
                RegisterForNextAnimationFrameUpdate();
            }
        }

        public override void OnAnimationFrameUpdate()
        {
            if (!_isAnimating)
            {
                return;
            }
            Invalidate();
            RegisterForNextAnimationFrameUpdate();
        }

        public override void OnRender(ImmediateDrawingContext context)
        {
            var size = EffectiveSize;
            if (!_isActive || _brush is null || size.X <= 0 || size.Y <= 0)
            {
                return;
            }
            var segmentWidth = size.X * _segmentRatio;
            // Keep an indeterminate status visible without requesting animation frames
            // when motion is disabled or the configured cycle is zero.
            var phase = _isAnimating
                ? CompositionNow.Ticks % _cycleDuration.Ticks / (double)_cycleDuration.Ticks : 0.5;
            var x = -segmentWidth + (size.X + segmentWidth) * phase;
            using (context.PushClip(new Rect(0, 0, size.X, size.Y)))
            {
                // ImmediateDrawingContext's convenience overload accepts uniform radii only.
                // The public platform context preserves all four corners without geometry
                // allocations on each composition frame (Avalonia 12.1.4).
                context.PlatformImpl.DrawRectangle(_brush, null,
                    new RoundedRect(new Rect(x, 0, segmentWidth, size.Y), _radius));
            }
        }
    }
}
