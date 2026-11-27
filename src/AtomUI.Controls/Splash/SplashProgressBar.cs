using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.Composition;

namespace AtomUI.Controls;

/// <summary>
/// Progress bar of the splash. It has no template: <see cref="Render" /> paints the track and the determinate
/// progress, while the indeterminate progress is advanced frame by frame on the render thread by a custom
/// composition visual attached to this control, so it keeps moving while the UI thread is blocked by the
/// commit. The Offset of a layout child cannot be animated instead: <c>Visual.SynchronizeCompositionProperties</c>
/// rewrites properties such as the Offset of the composition visual of a layout element on every synchronization
/// and removes the animations on it, so the indicator would stop at the layout origin. The composition visual
/// can only be created and sized in code: it depends on the compositor and on the layout result that are available
/// after attachment, which a ControlTheme cannot express.
/// </summary>
public class SplashProgressBar : Control
{
    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<SplashProgressBar, double>(nameof(Value), coerce: static (_, v) => Math.Clamp(v, 0d, 1d));

    /// <summary>
    /// Identifies the <see cref="IsIndeterminate" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<SplashProgressBar, bool>(nameof(IsIndeterminate), true);

    /// <summary>
    /// Identifies the <see cref="TrackBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TrackBrushProperty =
        AvaloniaProperty.Register<SplashProgressBar, IBrush?>(nameof(TrackBrush));

    /// <summary>
    /// Identifies the <see cref="IndicatorBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IndicatorBrushProperty =
        AvaloniaProperty.Register<SplashProgressBar, IBrush?>(nameof(IndicatorBrush));

    /// <summary>
    /// Identifies the <see cref="CornerRadius" /> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<SplashProgressBar, CornerRadius>(nameof(CornerRadius));

    private const double IndeterminateSegmentRatio = 0.3;
    private static readonly TimeSpan IndeterminateCycle = TimeSpan.FromSeconds(1.2);
    // Upper bound of the advance per frame, two frames at about 60 Hz. The render thread stalls during startup:
    // about 200 ms for the cold first frame of the splash, and the first frame of the main window shares the
    // render thread. Positioning by the absolute clock would make the frame after a stall jump over the whole
    // stall. With the bound, a stall shows as a short pause, after which the indicator moves on.
    private static readonly TimeSpan MaxFrameAdvance = TimeSpan.FromSeconds(1.0 / 30);

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

    /// <summary>
    /// Completed fraction of the bar, from 0 to 1; values outside that range are clamped to it. While
    /// <see cref="IsIndeterminate" /> is false, the indicator is painted from the left edge up to this fraction of
    /// the width, as long as the value is greater than 0. The default theme binds the value to
    /// <see cref="Splash.Progress" />.
    /// </summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// While true, the default, an indicator loops over the track and <see cref="Value" /> is ignored; while false,
    /// the bar shows the indicator for <see cref="Value" />. The loop runs only while the control is effectively
    /// visible and continues from the position where it stopped once the control is visible again. The default
    /// theme binds the value to <see cref="Splash.IsIndeterminate" />.
    /// </summary>
    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>
    /// Brush of the track, the whole area of the control. The default theme sets it to the <c>ColorFillSecondary</c>
    /// Token; <see langword="null" /> paints no track.
    /// </summary>
    public IBrush? TrackBrush
    {
        get => GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    /// <summary>
    /// Brush of the indicator: the filled part of the track while <see cref="IsIndeterminate" /> is false, and the
    /// moving indicator while it is true. The default theme sets it to the <c>ColorPrimary</c> Token;
    /// <see langword="null" /> paints no indicator in either mode.
    /// </summary>
    public IBrush? IndicatorBrush
    {
        get => GetValue(IndicatorBrushProperty);
        set => SetValue(IndicatorBrushProperty, value);
    }

    /// <summary>
    /// Corner radius of the track, of the indicator and of the moving indicator. Only the top-left corner is used.
    /// The default splash theme sets it to 2.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Creates the custom composition visual of the moving indicator, attaches it to this control as its element
    /// child visual and starts tracking the effective visibility that pauses the loop while the control is hidden.
    /// Nothing is created when the element visual has no compositor yet. Overrides must call the base member.
    /// </summary>
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

    /// <summary>
    /// Stops the loop, disposes the effective-visibility subscription and removes the custom composition visual
    /// when the control is detached from a visual tree. The visual is created again on the next attachment.
    /// Overrides must call the base member.
    /// </summary>
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

    /// <summary>
    /// Sends the current state of the moving indicator to the render thread whenever <see cref="IsIndeterminate" />,
    /// <see cref="IndicatorBrush" /> or <see cref="CornerRadius" /> changes. Overrides must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsIndeterminateProperty ||
            change.Property == IndicatorBrushProperty ||
            change.Property == CornerRadiusProperty)
        {
            SyncIndicator();
        }
    }

    /// <summary>
    /// Reports a desired size of zero: the bar draws nothing unless its parent or a style property such as
    /// <c>Height</c> gives it a size.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize) => new(0, 0);

    /// <summary>
    /// Sizes the custom composition visual of the moving indicator to the arranged size and returns that size
    /// unchanged.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_indicatorVisual is not null)
        {
            _indicatorVisual.Size = new Vector(finalSize.Width, finalSize.Height);
        }
        return finalSize;
    }

    /// <summary>
    /// Paints the track with <see cref="TrackBrush" /> and, while <see cref="IsIndeterminate" /> is false, the
    /// indicator with <see cref="IndicatorBrush" /> from the left edge up to <see cref="Value" />. The moving
    /// indicator of the indeterminate mode is painted on the render thread and does not pass through here.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);
        var radius = CornerRadius.TopLeft;
        if (TrackBrush is { } track)
        {
            context.DrawRectangle(track, null, bounds, radius, radius);
        }
        if (!IsIndeterminate && IndicatorBrush is { } indicator && Value > 0)
        {
            context.DrawRectangle(indicator, null, new Rect(0, 0, bounds.Width * Value, bounds.Height), radius, radius);
        }
    }

    private void SyncIndicator()
    {
        _indicatorVisual?.SendHandlerMessage(
            new IndicatorState(IsIndeterminate && _isEffectivelyVisible,
                IndicatorBrush?.ToImmutable(), CornerRadius.TopLeft));
    }

    private void HandleEffectiveVisibilityChanged(bool visible)
    {
        if (_isEffectivelyVisible != visible)
        {
            _isEffectivelyVisible = visible;
            SyncIndicator();
        }
    }

    private sealed record IndicatorState(bool IsActive, IImmutableBrush? Brush, double Radius);

    /// <summary>
    /// Runs on the render thread: advances the moving indicator by the composition clock difference between two
    /// adjacent frames and repaints it on every frame, without the UI thread. A frame advances the indicator by at
    /// most <see cref="MaxFrameAdvance" />. The first frame after an activation only records the clock. When the
    /// loop starts, the indicator enters from beyond the left end of the track.
    /// </summary>
    private sealed class IndeterminateIndicatorHandler : CompositionCustomVisualHandler
    {
        private bool _isActive;
        private IImmutableBrush? _brush;
        private double _radius;
        private double _phase;
        private TimeSpan? _lastFrameTime;

        public override void OnMessage(object message)
        {
            if (message is not IndicatorState state)
            {
                return;
            }
            var wasActive = _isActive;
            _isActive = state.IsActive;
            _brush = state.Brush;
            _radius = state.Radius;
            Invalidate();
            if (_isActive && !wasActive)
            {
                // The time spent inactive does not advance the indicator: after a reactivation it continues
                // from where it stopped.
                _lastFrameTime = null;
                RegisterForNextAnimationFrameUpdate();
            }
        }

        public override void OnAnimationFrameUpdate()
        {
            if (!_isActive)
            {
                return;
            }
            var now = CompositionNow;
            if (_lastFrameTime is { } last && now > last)
            {
                var advance = now - last;
                if (advance > MaxFrameAdvance)
                {
                    advance = MaxFrameAdvance;
                }
                _phase = (_phase + advance.Ticks / (double)IndeterminateCycle.Ticks) % 1d;
            }
            _lastFrameTime = now;
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
            var segmentWidth = size.X * IndeterminateSegmentRatio;
            var x = -segmentWidth + (size.X + segmentWidth) * _phase;
            using (context.PushClip(new Rect(0, 0, size.X, size.Y)))
            {
                context.DrawRectangle(_brush, null, new Rect(x, 0, segmentWidth, size.Y), _radius, _radius);
            }
        }
    }
}
