// Reimplementation reference: https://github.com/irihitech/Irihi.Iconica.IconPark

using System.Diagnostics;
using System.Runtime.InteropServices;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

public abstract class Icon : PathIcon, ICustomHitTest, IMotionAwareControl
{
    protected override Type StyleKeyOverride { get; } = typeof(Icon);

    public static readonly StyledProperty<IconAnimation> LoadingAnimationProperty =
        AvaloniaProperty.Register<Icon, IconAnimation>(
            nameof(LoadingAnimation), IconAnimation.None);
    
    public static readonly StyledProperty<IBrush?> StrokeBrushProperty =
        AvaloniaProperty.Register<Icon, IBrush?>(
            nameof(StrokeBrush));
    
    public static readonly StyledProperty<IBrush?> FillBrushProperty =
        AvaloniaProperty.Register<Icon, IBrush?>(
            nameof(FillBrush));
    
    public static readonly StyledProperty<IBrush?> SecondaryStrokeBrushProperty =
        AvaloniaProperty.Register<Icon, IBrush?>(
            nameof(SecondaryStrokeBrush));
    
    public static readonly StyledProperty<IBrush?> SecondaryFillBrushProperty =
        AvaloniaProperty.Register<Icon, IBrush?>(
            nameof(SecondaryFillBrush));
    
    public static readonly StyledProperty<IBrush?> FallbackBrushProperty =
        AvaloniaProperty.Register<Icon, IBrush?>(
            nameof(Icon), defaultValue: Brushes.White);
    
    public static readonly StyledProperty<IconThemeType> IconThemeProperty =
        AvaloniaProperty.Register<Icon, IconThemeType>(
            nameof(IconTheme), IconThemeType.Filled);
    
    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<Icon, double>(
            nameof(StrokeWidth), 4);

    public static readonly StyledProperty<PenLineCap> StrokeLineCapProperty =
        AvaloniaProperty.Register<Icon, PenLineCap>(
            nameof(StrokeLineCap), PenLineCap.Round);

    public static readonly StyledProperty<PenLineJoin> StrokeLineJoinProperty =
        AvaloniaProperty.Register<Icon, PenLineJoin>(
            nameof(StrokeLineJoin), PenLineJoin.Round);

    public static readonly StyledProperty<TimeSpan> LoadingAnimationDurationProperty =
        AvaloniaProperty.Register<Icon, TimeSpan>(
            nameof(LoadingAnimationDuration), TimeSpan.FromSeconds(1));

    public static readonly StyledProperty<TimeSpan> FillAnimationDurationProperty =
        AvaloniaProperty.Register<Icon, TimeSpan>(
            nameof(FillAnimationDuration), TimeSpan.FromMilliseconds(200));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Icon>();
    
    public IBrush? StrokeBrush
    {
        get => GetValue(StrokeBrushProperty);
        set => SetValue(StrokeBrushProperty, value);
    }
    
    public IBrush? FillBrush
    {
        get => GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }
    
    public IBrush? SecondaryStrokeBrush
    {
        get => GetValue(SecondaryStrokeBrushProperty);
        set => SetValue(SecondaryStrokeBrushProperty, value);
    }
    
    public IBrush? SecondaryFillBrush
    {
        get => GetValue(SecondaryFillBrushProperty);
        set => SetValue(SecondaryFillBrushProperty, value);
    }
    
    public IBrush? FallbackBrush
    {
        get => GetValue(FallbackBrushProperty);
        set => SetValue(FallbackBrushProperty, value);
    }
    
    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    public PenLineCap StrokeLineCap
    {
        get => GetValue(StrokeLineCapProperty);
        set => SetValue(StrokeLineCapProperty, value);
    }

    public PenLineJoin StrokeLineJoin
    {
        get => GetValue(StrokeLineJoinProperty);
        set => SetValue(StrokeLineJoinProperty, value);
    }
    
    public IconThemeType IconTheme
    {
        get => GetValue(IconThemeProperty);
        set => SetValue(IconThemeProperty, value);
    }

    public TimeSpan LoadingAnimationDuration
    {
        get => GetValue(LoadingAnimationDurationProperty);
        set => SetValue(LoadingAnimationDurationProperty, value);
    }

    public TimeSpan FillAnimationDuration
    {
        get => GetValue(FillAnimationDurationProperty);
        set => SetValue(FillAnimationDurationProperty, value);
    }

    public IconAnimation LoadingAnimation
    {
        get => GetValue(LoadingAnimationProperty);
        set => SetValue(LoadingAnimationProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #region 内部属性定义

    internal static readonly StyledProperty<double> AngleAnimationRotateProperty =
        AvaloniaProperty.Register<Icon, double>(
            nameof(AngleAnimationRotate));
    
    internal static readonly DirectProperty<Icon, bool> IsSupportSimpleTransitionProperty =
        AvaloniaProperty.RegisterDirect<Icon, bool>(nameof(IsSupportSimpleTransition),
            o => o.IsSupportSimpleTransition,
            (o, v) => o.IsSupportSimpleTransition = v);

    internal double AngleAnimationRotate
    {
        get => GetValue(AngleAnimationRotateProperty);
        set => SetValue(AngleAnimationRotateProperty, value);
    }
    
    private bool _isSupportSimpleTransition;

    internal bool IsSupportSimpleTransition
    {
        get => _isSupportSimpleTransition;
        set => SetAndRaise(IsSupportSimpleTransitionProperty, ref _isSupportSimpleTransition, value);
    }

    #endregion
    
    protected virtual IList<DrawingInstruction> DrawingInstructions { get; } = Array.Empty<DrawingInstruction>();
    protected Rect ViewBox;

    private Animation? _animation;
    private CancellationTokenSource? _animationCancellationTokenSource;
    protected readonly IBrush?[] DrawBrushes = new IBrush[5];
    protected readonly Pen?[] DrawPens = new Pen?[5];

    static Icon()
    {
        AffectsMeasure<Icon>(HeightProperty, WidthProperty);
        AffectsRender<Icon>(
            StrokeBrushProperty,
            FillBrushProperty,
            SecondaryStrokeBrushProperty,
            SecondaryFillBrushProperty,
            FallbackBrushProperty,
            StrokeLineCapProperty,
            StrokeLineJoinProperty,
            StrokeWidthProperty);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        var strokeIndex          = (int)IconBrushType.Fallback;
        var fillIndex            = (int)IconBrushType.Fill;
        var secondaryStrokeIndex = (int)IconBrushType.SecondaryStroke;
        var secondaryFillIndex   = (int)IconBrushType.SecondaryFill;
        var fallbackIndex        = (int)IconBrushType.Fallback;
        
        var strokeBrush          = ProcessBrush(StrokeBrush);
        var fillBrush            = ProcessBrush(FillBrush);
        var secondaryStrokeBrush = ProcessBrush(SecondaryStrokeBrush);
        var secondaryFillBrush   = ProcessBrush(SecondaryFillBrush);
        var fallbackBrush        = ProcessBrush(FallbackBrush);
        
        DrawBrushes[strokeIndex]          = strokeBrush;
        DrawBrushes[fillIndex]            = fillBrush;
        DrawBrushes[secondaryStrokeIndex] = secondaryStrokeBrush;
        DrawBrushes[secondaryFillIndex]   = secondaryFillBrush;
        DrawBrushes[fallbackIndex]        = fallbackBrush;
        
        DrawPens[strokeIndex]          = new Pen(strokeBrush, StrokeWidth, lineCap: StrokeLineCap, lineJoin: StrokeLineJoin);
        DrawPens[fillIndex]            = new Pen(fillBrush, StrokeWidth, lineCap: StrokeLineCap, lineJoin: StrokeLineJoin);
        DrawPens[secondaryStrokeIndex] = new Pen(secondaryStrokeBrush, StrokeWidth, lineCap: StrokeLineCap, lineJoin: StrokeLineJoin);
        DrawPens[secondaryFillIndex]   = new Pen(secondaryFillBrush, StrokeWidth, lineCap: StrokeLineCap, lineJoin: StrokeLineJoin);
        DrawPens[fallbackIndex]        = new Pen(fallbackBrush, StrokeWidth, lineCap: StrokeLineCap, lineJoin: StrokeLineJoin);
    }

    protected virtual IBrush? ProcessBrush(IBrush? brush)
    {
        return brush;
    }

    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    BaseTransitionUtils.CreateTransition<SolidColorBrushTransition>(StrokeBrushProperty,
                        FillAnimationDuration),
                    BaseTransitionUtils.CreateTransition<SolidColorBrushTransition>(FillBrushProperty,
                        FillAnimationDuration),
                    BaseTransitionUtils.CreateTransition<SolidColorBrushTransition>(SecondaryFillBrushProperty,
                        FillAnimationDuration),
                    BaseTransitionUtils.CreateTransition<SolidColorBrushTransition>(SecondaryStrokeBrushProperty,
                        FillAnimationDuration),
                    BaseTransitionUtils.CreateTransition<SolidColorBrushTransition>(FallbackBrushProperty,
                        FillAnimationDuration)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == AngleAnimationRotateProperty)
        {
            SetCurrentValue(RenderTransformProperty, new RotateTransform(AngleAnimationRotate));
        }
        
        else if (change.Property == FillAnimationDurationProperty)
        {
            ConfigureTransitions(true);
        }

        if (change.Property == StrokeBrushProperty)
        {
            HandleBrushChanged(IconBrushType.Stroke, StrokeBrush, change.Priority);
        }
        else if (change.Property == FillBrushProperty)
        {
            HandleBrushChanged(IconBrushType.Fill, FillBrush, change.Priority);
        }
        else if (change.Property == SecondaryStrokeBrushProperty)
        {
            HandleBrushChanged(IconBrushType.SecondaryStroke, SecondaryStrokeBrush, change.Priority);
        }
        else if (change.Property == SecondaryFillBrushProperty)
        {
            HandleBrushChanged(IconBrushType.SecondaryFill, SecondaryFillBrush, change.Priority);
        }
        else if (change.Property == FallbackBrushProperty)
        {
            HandleBrushChanged(IconBrushType.Fallback, FallbackBrush, change.Priority);
        }
        else if (change.Property == StrokeWidthProperty)
        {
            HandleStrokeWidthChanged(StrokeWidth);
        }
        else if (change.Property == StrokeLineCapProperty)
        {
            HandleLineCapChanged(StrokeLineCap);
        }
        else if (change.Property == StrokeLineJoinProperty)
        {
            HandleLineJoinChanged(StrokeLineJoin);
        }
        else if (change.Property == IconThemeProperty)
        {
            if (IconTheme == IconThemeType.Filled ||
                IconTheme == IconThemeType.Outlined ||
                IconTheme == IconThemeType.Rounded ||
                IconTheme == IconThemeType.Sharp)
            {
                IsSupportSimpleTransition = true;
            }
            else
            {
                IsSupportSimpleTransition = false;
            }
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsVisibleProperty)
            {
                if (IsVisible)
                {
                    StartLoadingAnimationDelay();
                }
                else
                {
                    _animationCancellationTokenSource?.Cancel();
                    _animationCancellationTokenSource = null;
                }
            }
            else if (change.Property == LoadingAnimationProperty)
            {
                SetupRotateAnimation();
                if (_animation != null && IsVisible)
                {
                    StartLoadingAnimationDelay();
                }
            }
        }
        
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }

    private void StartLoadingAnimationDelay()
    {
        DispatcherTimer.RunOnce(() =>
        {
            Dispatcher.UIThread.InvokeAsync(StartLoadingAnimationAsync);
        }, TimeSpan.FromMilliseconds(200));
    }

    private void HandleBrushChanged(IconBrushType brushType, IBrush? brush, BindingPriority priority)
    {
        if (brushType == IconBrushType.None)
        {
            return;
        }
        var brushIndex = (int)brushType;
        if (priority > BindingPriority.Animation)
        {
            DrawBrushes[brushIndex] = ProcessBrush(brush);
        }
        else
        {
            DrawBrushes[brushIndex] = brush;
        }
   
        DrawPens[brushIndex]    = new Pen(DrawBrushes[brushIndex], StrokeWidth, lineCap: StrokeLineCap, lineJoin: StrokeLineJoin);
    }

    private void HandleStrokeWidthChanged(double strokeWidth)
    {
        foreach (var pen in DrawPens)
        {
            if (pen != null)
            {
                pen.Thickness = strokeWidth;
            }
        }
    }
    
    private void HandleLineCapChanged(PenLineCap lineCap)
    {
        foreach (var pen in DrawPens)
        {
            if (pen != null)
            {
                pen.LineCap = lineCap;
            }
        }
    }
    
    private void HandleLineJoinChanged(PenLineJoin lineJoin)
    {
        foreach (var pen in DrawPens)
        {
            if (pen != null)
            {
                pen.LineJoin = lineJoin;
            }
        }
    }
    
    private void SetupRotateAnimation()
    {
        if (_animation is not null)
        {
            _animationCancellationTokenSource?.Cancel();
            _animation = null;
        }

        if (LoadingAnimation == IconAnimation.Spin || LoadingAnimation == IconAnimation.Pulse)
        {
            _animation = new Animation
            {
                Duration       = LoadingAnimationDuration,
                IterationCount = new IterationCount(ulong.MaxValue),
                Children =
                {
                    new KeyFrame
                    {
                        Cue     = new Cue(0d),
                        Setters = { new Setter(AngleAnimationRotateProperty, 0d) }
                    },
                    new KeyFrame
                    {
                        Cue     = new Cue(1d),
                        Setters = { new Setter(AngleAnimationRotateProperty, 360d) }
                    }
                }
            };
            if (LoadingAnimation == IconAnimation.Pulse)
            {
                _animation.Easing = new PulseEasing();
            }
        }
    }
    
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        SetupRotateAnimation();
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (_animation is not null && IsVisible)
        {
            StartLoadingAnimationDelay();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _animationCancellationTokenSource?.Cancel();
        _animationCancellationTokenSource = null;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }

    public async Task StartLoadingAnimationAsync()
    {
        if (_animation is not null && _animationCancellationTokenSource == null)
        {
            _animationCancellationTokenSource = new CancellationTokenSource();
            await _animation.RunAsync(this, _animationCancellationTokenSource.Token);
        }
    }

    public void StartLoadingAnimation()
    {
        Dispatcher.UIThread.InvokeAsync(StartLoadingAnimationAsync);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(Background ?? Brushes.Transparent, Bounds);
        if (DrawingInstructions.Count == 0)
        {
            return;
        }
        var       realSize             = DesiredSize.Deflate(Margin);
        var       scale                = new Vector(realSize.Width / ViewBox.Width, realSize.Height / ViewBox.Height);
        var       globalGeometryMatrix = CalculateGlobalGeometryMatrix();
        using var transformState       = context.PushTransform(Matrix.CreateScale(scale));
        foreach (var instruction in DrawingInstructions)
        {
            instruction.Draw(context, in globalGeometryMatrix, this);
        }
    }

    protected virtual Matrix CalculateGlobalGeometryMatrix()
    {
        return Matrix.Identity;
    }
    
    protected Rect CalculateGeometryBounds()
    {
        var group = new GeometryGroup();
        foreach (var instruction in DrawingInstructions)
        {
            if (instruction is RectDrawingInstruction rectDrawingInstruction)
            {
                var rectangleGeometry = new RectangleGeometry(rectDrawingInstruction.Rect,
                    rectDrawingInstruction.RadiusX, rectDrawingInstruction.RadiusY);
                if (rectDrawingInstruction.Transform != null)
                {
                    rectangleGeometry.Transform = new MatrixTransform(rectDrawingInstruction.Transform.Value);
                }

                group.Children.Add(rectangleGeometry);
            }
            else if (instruction is CircleDrawingInstruction circleDrawingInstruction)
            {
                var circleGeometry = new EllipseGeometry
                {
                    Center  = circleDrawingInstruction.Center,
                    RadiusX = circleDrawingInstruction.Radius,
                    RadiusY = circleDrawingInstruction.Radius
                };
                if (circleDrawingInstruction.Transform != null)
                {
                    circleGeometry.Transform = new MatrixTransform(circleDrawingInstruction.Transform.Value);
                }

                group.Children.Add(circleGeometry);
            }
            else if (instruction is EllipseDrawingInstruction ellipseDrawingInstruction)
            {
                var ellipseGeometry = new EllipseGeometry
                {
                    Center  = ellipseDrawingInstruction.Center,
                    RadiusX = ellipseDrawingInstruction.RadiusX,
                    RadiusY = ellipseDrawingInstruction.RadiusY
                };
                if (ellipseDrawingInstruction.Transform != null)
                {
                    ellipseGeometry.Transform = new MatrixTransform(ellipseDrawingInstruction.Transform.Value);
                }

                group.Children.Add(ellipseGeometry);
            }
            else if (instruction is LineDrawingInstruction lineDrawingInstruction)
            {
                var lineGeometry = new LineGeometry()
                {
                    StartPoint = lineDrawingInstruction.StartPoint,
                    EndPoint   = lineDrawingInstruction.EndPoint
                };
                if (lineDrawingInstruction.Transform != null)
                {
                    lineGeometry.Transform = new MatrixTransform(lineDrawingInstruction.Transform.Value);
                }

                group.Children.Add(lineGeometry);
            }
            else if (instruction is PolylineDrawingInstruction polylineDrawingInstruction)
            {
                var polylineGeometry = new PolylineGeometry()
                {
                    Points   = polylineDrawingInstruction.Points,
                    IsFilled = false
                };
                if (polylineDrawingInstruction.Transform != null)
                {
                    polylineGeometry.Transform = new MatrixTransform(polylineDrawingInstruction.Transform.Value);
                }

                group.Children.Add(polylineGeometry);
            }
            else if (instruction is PolygonDrawingInstruction polygonDrawingInstruction)
            {
                var polygonGeometry = new PolylineGeometry()
                {
                    Points   = polygonDrawingInstruction.Points,
                    IsFilled = true
                };
                if (polygonDrawingInstruction.Transform != null)
                {
                    polygonGeometry.Transform = new MatrixTransform(polygonDrawingInstruction.Transform.Value);
                }

                group.Children.Add(polygonGeometry);
            }
            else if (instruction is PathDrawingInstruction pathDrawingInstruction)
            {
                var geometry = pathDrawingInstruction.Data?.Clone();
                if (geometry != null)
                {
                    if (pathDrawingInstruction.Transform != null)
                    {
                        geometry.Transform = new MatrixTransform(pathDrawingInstruction.Transform.Value);
                    }

                    group.Children.Add(geometry);
                }
            }
        }

        return group.Bounds;
    }

    protected virtual void ValidateThemeChange()
    {
        throw new InvalidOleVariantTypeException("Icon theme type switching is not supported.");
    }

    public bool HitTest(Point point)
    {
        return true;
    }

    public virtual IBrush? FindIconBrush(IconBrushType brushType)
    {
        if (brushType == IconBrushType.None)
        {
            return null;
        }
        var index = (int)brushType;
        Debug.Assert(index >= 0 && index < DrawBrushes.Length);
        return DrawBrushes[index];
    }
}