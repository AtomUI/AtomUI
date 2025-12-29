using System.Diagnostics;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls.Primitives;

internal class WaveSpiritDecorator : Control
{
    public const string WaveSpiritPart = "PART_WaveSpirit";
    #region 公共属性定义
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, CornerRadius>(nameof(CornerRadius));

    public static readonly StyledProperty<TimeSpan> SizeMotionDurationProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, TimeSpan>(nameof(SizeMotionDuration));
    
    public static readonly StyledProperty<TimeSpan> OpacityMotionDurationProperty =
        AvaloniaProperty.Register<WaveSpiritDecorator, TimeSpan>(nameof(SizeMotionDuration));
    
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
        get => GetValue(SizeMotionDurationProperty);
        set => SetValue(SizeMotionDurationProperty, value);
    }

    public TimeSpan OpacityMotionDuration
    {
        get => GetValue(OpacityMotionDurationProperty);
        set => SetValue(OpacityMotionDurationProperty, value);
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
        get => GetValue(OriginOpacityProperty);
        set => SetValue(OriginOpacityProperty, value);
    }
    
    public double WaveRange
    {
        get => GetValue(WaveRangeProperty);
        set => SetValue(WaveRangeProperty, value);
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
    #endregion

    #region 内部属性定义

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
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isPlaying;

    static WaveSpiritDecorator()
    {
        AffectsRender<WaveSpiritDecorator>(LastWaveSizeProperty, LastWaveRadiusProperty, LastWaveOpacityProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == WaveTypeProperty)
            {
                BuildWavePainter(true);
            }
        }
        if (change.Property == WaveBrushProperty ||
                 change.Property == OriginOpacityProperty ||
                 change.Property == SizeMotionDurationProperty ||
                 change.Property == OpacityMotionDurationProperty ||
                 change.Property == SizeEasingCurveProperty ||
                 change.Property == OpacityEasingCurveProperty ||
                 change.Property == CornerRadiusProperty)
        {
            _cancellationTokenSource?.Cancel();
            ConfigureWavePainter();
        }
        else if (change.Property == BoundsProperty)
        {
            ConfigurePainterSize(Bounds.Size);
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

            ConfigureWavePainter();
            ConfigurePainterSize(Bounds.Size);
        }
    }

    private void ConfigureWavePainter()
    {
        if (_wavePainter != null)
        {
            if (_wavePainter is RoundRectWavePainter roundWavePainter)
            {
                roundWavePainter.CornerRadius = CornerRadius;
            }
            _wavePainter.SizeEasingCurve       = new CubicEaseOut();
            _wavePainter.OpacityEasingCurve    = new CubicEaseOut();
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
        base.OnDetachedFromVisualTree(e);
        _cancellationTokenSource?.Cancel();
        _isPlaying = false;
    }
    
    public void Play()
    {
        if (_isPlaying || !this.IsAttachedToVisualTree())
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

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        var sizeAnimationTask    = sizeAnimation.RunAsync(this, _cancellationTokenSource.Token);
        var opacityAnimationTask = opacityAnimation.RunAsync(this, _cancellationTokenSource.Token);
        _isPlaying = true;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.WhenAll(sizeAnimationTask, opacityAnimationTask);
            _isPlaying = false;
        });
    }
}