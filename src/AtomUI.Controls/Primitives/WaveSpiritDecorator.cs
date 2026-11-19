using System.Diagnostics;
using System.Globalization;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Primitives;

internal class WaveSpiritDecorator : Control
{
    public const string WaveSpiritPart = "PART_WaveSpirit";
    
    #region 公共属性定义
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, CornerRadius>(nameof(CornerRadius));

    public static readonly StyledProperty<TimeSpan> SizeMotionDurationProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, TimeSpan>(nameof(SizeMotionDuration));
    
    public static readonly StyledProperty<TimeSpan> OpacityMotionDurationProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, TimeSpan>(nameof(OpacityMotionDuration));
    
    public static readonly StyledProperty<Easing> SizeEasingCurveProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, Easing>(nameof(SizeEasingCurve));
    
    public static readonly StyledProperty<Easing> OpacityEasingCurveProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, Easing>(nameof(OpacityEasingCurve));
    
    public static readonly StyledProperty<double> OriginOpacityProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, double>(nameof(OriginOpacity));
    
    public static readonly StyledProperty<double> WaveRangeProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, double>(nameof(WaveRange));
    
    public static readonly StyledProperty<IBrush?> WaveBrushProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, IBrush?>(nameof(WaveBrush));
    
    public static readonly StyledProperty<WaveSpiritType> WaveTypeProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, WaveSpiritType>(nameof(WaveType));
    
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    
    public TimeSpan SizeMotionDuration
    {
        get => GetWaveMetric(SizeMotionDurationProperty, _themeDuration);
        set => SetWaveMetric(SizeMotionDurationProperty, value);
    }

    public TimeSpan OpacityMotionDuration
    {
        get => GetWaveMetric(OpacityMotionDurationProperty, _themeDuration);
        set => SetWaveMetric(OpacityMotionDurationProperty, value);
    }
    
    public Easing SizeEasingCurve
    {
        get => GetValue(SizeEasingCurveProperty);
        set => SetValue(SizeEasingCurveProperty, value);
    }
    
    public Easing OpacityEasingCurve
    {
        get => GetValue(OpacityEasingCurveProperty);
        set => SetValue(OpacityEasingCurveProperty, value);
    }
    
    public double OriginOpacity
    {
        get => GetWaveMetric(OriginOpacityProperty, _themeOriginOpacity);
        set => SetWaveMetric(OriginOpacityProperty, value);
    }
    
    public double WaveRange
    {
        get => GetWaveMetric(WaveRangeProperty, _themeRange);
        set => SetWaveMetric(WaveRangeProperty, value);
    }
    
    public IBrush? WaveBrush
    {
        get => GetValue(WaveBrushProperty);
        set => SetValue(WaveBrushProperty, value);
    }
    
    public WaveSpiritType WaveType
    {
        get => GetValue(WaveTypeProperty);
        set => SetValue(WaveTypeProperty, value);
    }

    public static IBrush? ResolveWaveSpiritBrush(IBrush? borderBrush, IBrush? background)
    {
        if (IsValidWaveSpiritBrush(borderBrush))
        {
            return borderBrush;
        }

        if (IsValidWaveSpiritBrush(background))
        {
            return background;
        }

        return null;
    }

    private static bool IsValidWaveSpiritBrush(IBrush? brush)
    {
        if (brush is null || !(brush.Opacity > 0))
        {
            return false;
        }

        if (brush is not ISolidColorBrush solidBrush)
        {
            return false;
        }

        var color = solidBrush.Color;
        var effectiveAlpha = color.A / 255d * brush.Opacity;
        return effectiveAlpha > 0 &&
               (color.R != byte.MaxValue ||
                color.G != byte.MaxValue ||
                color.B != byte.MaxValue ||
                effectiveAlpha < 1);
    }
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<WaveSpiritDecorator>();

    internal static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<WaveSpiritDecorator>();

    internal static readonly StyledProperty<bool> UseOwnerWaveTokenResourcesProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, bool>(nameof(UseOwnerWaveTokenResources));

    internal bool UseOwnerWaveTokenResources
    {
        get => GetValue(UseOwnerWaveTokenResourcesProperty);
        set => SetValue(UseOwnerWaveTokenResourcesProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    protected static readonly DirectProperty<WaveSpiritDecorator, double> LastWaveOpacityProperty =
        AvaloniaProperty.RegisterDirect<WaveSpiritDecorator, double>(
            nameof(LastWaveOpacity),
            o => o.LastWaveOpacity,
            (o, v) => o.LastWaveOpacity = v);
    
    protected static readonly DirectProperty<WaveSpiritDecorator, Size> LastWaveSizeProperty =
        AvaloniaProperty.RegisterDirect<WaveSpiritDecorator, Size>(
            nameof(LastWaveSize),
            o => o.LastWaveSize,
            (o, v) => o.LastWaveSize = v);
    
    protected static readonly DirectProperty<WaveSpiritDecorator, double> LastWaveRadiusProperty =
        AvaloniaProperty.RegisterDirect<WaveSpiritDecorator, double>(
            nameof(LastWaveRadius),
            o => o.LastWaveRadius,
            (o, v) => o.LastWaveRadius = v);
    
    private double _lastWaveOpacity;
    protected double LastWaveOpacity
    {
        get => _lastWaveOpacity;
        set => SetAndRaise(LastWaveOpacityProperty, ref _lastWaveOpacity, value);
    }
    
    private Size _lastWaveSize;
    protected Size LastWaveSize
    {
        get => _lastWaveSize;
        set => SetAndRaise(LastWaveSizeProperty, ref _lastWaveSize, value);
    }
    
    private double _lastWaveRadiusSize;
    protected double LastWaveRadius
    {
        get => _lastWaveRadiusSize;
        set => SetAndRaise(LastWaveRadiusProperty, ref _lastWaveRadiusSize, value);
    }
    #endregion
    
    private AbstractWavePainter? _wavePainter;
    private OwnerTokenSet? _waveMetricTokens;
    private double _themeRange;
    private double _themeOriginOpacity;
    private TimeSpan _themeDuration;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isPlaying;
    private static readonly Easing DefaultEasingCurve = new CubicEaseOut();

    internal bool IsPlaying => _isPlaying;

    static WaveSpiritDecorator()
    {
        AffectsRender<WaveSpiritDecorator>(LastWaveSizeProperty, LastWaveRadiusProperty, LastWaveOpacityProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if ((change.Property == IsMotionEnabledProperty ||
             change.Property == IsWaveSpiritEnabledProperty) &&
            (!IsMotionEnabled || !IsWaveSpiritEnabled))
        {
            CancelAnimation();
        }
        
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == WaveTypeProperty)
            {
                BuildWavePainter(true);
            }
        }
        if (change.Property == UseOwnerWaveTokenResourcesProperty)
        {
            UpdateWaveTokenSubscription();
        }
        else if (change.Property == WaveBrushProperty ||
            change.Property == OriginOpacityProperty ||
            change.Property == WaveRangeProperty ||
            change.Property == SizeMotionDurationProperty ||
            change.Property == OpacityMotionDurationProperty ||
            change.Property == SizeEasingCurveProperty ||
            change.Property == OpacityEasingCurveProperty ||
            change.Property == CornerRadiusProperty)
        {
            CancelAnimation();
            ConfigureWavePainter();
        }
        else if (change.Property == BoundsProperty)
        {
            ConfigurePainterSize(Bounds.Size);
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_waveMetricTokens is null && UseOwnerWaveTokenResources)
        {
            UpdateWaveTokenSubscription();
        }
    }

    private T GetWaveMetric<T>(StyledProperty<T> property, T themeValue) =>
        _waveMetricTokens is not null && !IsSet(property) ? themeValue : GetValue(property);

    private void SetWaveMetric<T>(StyledProperty<T> property, T value)
    {
        // An explicit value equal to the registration default can still replace the scoped default.
        var needsRefresh = _waveMetricTokens is not null && !IsSet(property) &&
                           EqualityComparer<T>.Default.Equals(GetValue(property), value);
        SetValue(property, value);
        if (needsRefresh)
        {
            CancelAnimation();
            ConfigureWavePainter();
        }
    }

    private void UpdateWaveTokenSubscription()
    {
        _waveMetricTokens?.Dispose();
        _waveMetricTokens = null;
        if (UseOwnerWaveTokenResources && this.IsAttachedToVisualTree())
        {
            _waveMetricTokens = new OwnerTokenSet(this, 3, RefreshWaveMetrics);
            _waveMetricTokens.SetKind(0, SharedTokenKind.WaveAnimationRange);
            _waveMetricTokens.SetKind(1, SharedTokenKind.WaveStartOpacity);
            _waveMetricTokens.SetKind(2, SharedTokenKind.MotionDurationSlow);
        }
        ReadWaveMetrics();
        CancelAnimation();
        ConfigureWavePainter();
    }

    private void RefreshWaveMetrics()
    {
        var previous = (WaveRange, OriginOpacity, SizeMotionDuration, OpacityMotionDuration);
        ReadWaveMetrics();
        if (!previous.Equals((WaveRange, OriginOpacity, SizeMotionDuration, OpacityMotionDuration)))
        {
            CancelAnimation();
            ConfigureWavePainter();
        }
    }

    private void ReadWaveMetrics()
    {
        _themeRange = ReadMetric(_waveMetricTokens?.GetValue(0), _themeRange);
        _themeOriginOpacity = ReadMetric(_waveMetricTokens?.GetValue(1), _themeOriginOpacity);
        _themeDuration = _waveMetricTokens?.GetValue(2) switch
        {
            TimeSpan duration => duration,
            string text when TimeSpan.TryParse(text, CultureInfo.CurrentCulture, out var parsed) => parsed,
            var value when ReferenceEquals(value, BindingOperations.DoNothing) => _themeDuration,
            _ => TimeSpan.Zero
        };
    }

    private static double ReadMetric(object? value, double previous)
    {
        if (value is double metric)
        {
            return metric;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    private void BuildWavePainter(bool force)
    {
        if (_wavePainter == null || force)
        {
            if (WaveType == WaveSpiritType.CircleWave)
            {
                _wavePainter = new CircleWavePainter();
            }
            else if (WaveType == WaveSpiritType.PillWave)
            {
                _wavePainter = new PillWavePainter();
            }
            else
            {
                _wavePainter = new RoundRectWavePainter();
            }

            ConfigurePainterSize(Bounds.Size);
        }
        ConfigureWavePainter();
    }

    private void ConfigureWavePainter()
    {
        if (_wavePainter != null)
        {
            if (_wavePainter is RoundRectWavePainter roundWavePainter)
            {
                roundWavePainter.CornerRadius = CornerRadius;
            }
            _wavePainter.SizeEasingCurve       = DefaultEasingCurve;
            _wavePainter.OpacityEasingCurve    = DefaultEasingCurve;
            _wavePainter.OriginOpacity         = Math.Clamp(OriginOpacity, 0.0, 1.0);
            _wavePainter.SizeMotionDuration    = SizeMotionDuration;
            _wavePainter.OpacityMotionDuration = OpacityMotionDuration.Add(TimeSpan.FromMilliseconds(50));
            _wavePainter.WaveRange             = Math.Min(WaveRange, 8);
            _wavePainter.WaveBrush             = WaveBrush;
        }
    }

    private void ConfigurePainterSize(Size size)
    {
        if (_wavePainter is RoundRectWavePainter roundRectWavePainter)
        {
            roundRectWavePainter.OriginSize  = size;
        }
        else if (_wavePainter is PillWavePainter pillWavePainter)
        {
            pillWavePainter.OriginSize  = size;
        }
        else if (_wavePainter is CircleWavePainter circleWavePainter)
        {
            circleWavePainter.OriginPoint  = new Point(size.Width / 2, size.Height / 2);
            circleWavePainter.OriginRadius = size.Width / 2;
        }
    }

    public sealed override void Render(DrawingContext context)
    {
        if (!_isPlaying || _wavePainter == null)
        {
            return;
        }
        object currentSize;
        if (_wavePainter.WaveType == WaveSpiritType.CircleWave)
        {
            currentSize = LastWaveRadius;
        }
        else
        {
            currentSize = LastWaveSize;
        }
        _wavePainter.Paint(context, currentSize, LastWaveOpacity);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _waveMetricTokens?.Dispose();
        _waveMetricTokens = null;
        base.OnDetachedFromVisualTree(e);
        CancelAnimation();
    }
    
    public void Play()
    {
        if (_isPlaying ||
            !IsMotionEnabled ||
            !IsWaveSpiritEnabled ||
            !this.IsAttachedToVisualTree())
        {
            return;
        }
        
        if (SizeMotionDuration <= TimeSpan.Zero || OpacityMotionDuration <= TimeSpan.Zero)
        {
            return; // Skip animation if durations are invalid
        }
        
        BuildWavePainter(false);
        
        Debug.Assert(_wavePainter != null);
        
        var              sizeAnimation    = new Animation();
        var              opacityAnimation = new Animation();
        AvaloniaProperty targetProperty;
        if (_wavePainter.WaveType == WaveSpiritType.CircleWave)
        {
            targetProperty = LastWaveRadiusProperty;
        }
        else
        {
            targetProperty = LastWaveSizeProperty;
        }

        _wavePainter.NotifyBuildSizeAnimation(sizeAnimation, targetProperty);
        _wavePainter.NotifyBuildOpacityAnimation(opacityAnimation, LastWaveOpacityProperty);

        var cancellation = new CancellationTokenSource();
        _cancellationTokenSource = cancellation;

        var sizeAnimationTask    = sizeAnimation.RunAsync(this, cancellation.Token);
        var opacityAnimationTask = opacityAnimation.RunAsync(this, cancellation.Token);
        _isPlaying = true;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                await Task.WhenAll(sizeAnimationTask, opacityAnimationTask);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (ReferenceEquals(_cancellationTokenSource, cancellation))
                {
                    cancellation.Dispose();
                    _cancellationTokenSource = null;
                    _isPlaying = false;
                }
            }
        });
    }

    private void CancelAnimation()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _isPlaying               = false;
    }
}
