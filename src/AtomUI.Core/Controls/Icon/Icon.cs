// Reimplementation reference: https://github.com/irihitech/Irihi.Iconica.IconPark

using System.Diagnostics;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

public abstract class Icon : PathIcon, ICustomHitTest
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
            nameof(FallbackBrush), defaultValue: Brushes.White);
    
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

    public IconAnimation LoadingAnimation
    {
        get => GetValue(LoadingAnimationProperty);
        set => SetValue(LoadingAnimationProperty, value);
    }

    private const float FULL_ROTATION_RADIANS = (float)(Math.PI * 2);
    private const string ROTATION_PROPERTY    = "RotationAngle";

    private CompositeDisposable? _effectiveVisibilitySubscriptions;

    protected virtual IList<DrawingInstruction> DrawingInstructions { get; } = Array.Empty<DrawingInstruction>();
    protected Rect ViewBox;
    
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
        var strokeIndex          = (int)IconBrushType.Stroke;
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

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

        if (IsLoaded)
        {
            if (change.Property == LoadingAnimationProperty)
            {
                ConfigureLoadingAnimationVisibilityTracking();
            }
            else if (change.Property == LoadingAnimationDurationProperty)
            {
                if (IsLoadingAnimationConfigured())
                {
                    RestartLoadingAnimation();
                }
            }
            else if (change.Property == BoundsProperty)
            {
                if (CanRunLoadingAnimation())
                {
                    UpdateLoadingAnimationCenterPoint();
                }
            }
        }
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

        DrawPens[brushIndex] = new Pen(DrawBrushes[brushIndex], StrokeWidth, lineCap: StrokeLineCap,
            lineJoin: StrokeLineJoin);
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
    
    private void StartLoadingAnimation()
    {
        if (!CanRunLoadingAnimation())
        {
            return;
        }

        var visual = ElementComposition.GetElementVisual(this);
        if (visual?.Compositor is null)
        {
            return;
        }

        visual.StopAnimation(ROTATION_PROPERTY);
        UpdateLoadingAnimationCenterPoint(visual);

        Easing easing = LoadingAnimation == IconAnimation.Pulse
            ? new PulseEasing()
            : new LinearEasing();
        var rotationAnimation = visual.Compositor.CreateScalarKeyFrameAnimation();
        rotationAnimation.Target            = ROTATION_PROPERTY;
        rotationAnimation.Duration          = GetCompositionAnimationDuration();
        rotationAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
        rotationAnimation.StopBehavior      = AnimationStopBehavior.SetToInitialValue;
        rotationAnimation.InsertKeyFrame(0, 0, easing);
        rotationAnimation.InsertKeyFrame(1, FULL_ROTATION_RADIANS, easing);
        visual.StartAnimation(ROTATION_PROPERTY, rotationAnimation);
    }

    private void StopLoadingAnimation()
    {
        var visual = ElementComposition.GetElementVisual(this);
        visual?.StopAnimation(ROTATION_PROPERTY);
        if (visual is not null)
        {
            visual.RotationAngle = 0;
        }
    }

    private void RestartLoadingAnimation()
    {
        StopLoadingAnimation();
        StartLoadingAnimation();
    }

    private bool CanRunLoadingAnimation()
    {
        return IsEffectivelyVisible &&
               IsLoaded &&
               IsLoadingAnimationConfigured();
    }

    private void ConfigureLoadingAnimationVisibilityTracking()
    {
        _effectiveVisibilitySubscriptions?.Dispose();
        _effectiveVisibilitySubscriptions = null;

        if (!this.IsAttachedToVisualTree() || !IsLoadingAnimationConfigured())
        {
            StopLoadingAnimation();
            return;
        }

        var subscriptions = new CompositeDisposable();
        _effectiveVisibilitySubscriptions = subscriptions;
        this.TrackEffectiveVisibility(HandleEffectiveVisibilityChanged, subscriptions);
    }

    private void HandleEffectiveVisibilityChanged(bool isEffectivelyVisible)
    {
        if (isEffectivelyVisible)
        {
            StartLoadingAnimation();
        }
        else
        {
            StopLoadingAnimation();
        }
    }

    private bool IsLoadingAnimationConfigured()
    {
        return LoadingAnimation == IconAnimation.Spin ||
               LoadingAnimation == IconAnimation.Pulse;
    }

    private void UpdateLoadingAnimationCenterPoint()
    {
        var visual = ElementComposition.GetElementVisual(this);
        if (visual is not null)
        {
            UpdateLoadingAnimationCenterPoint(visual);
        }
    }

    private void UpdateLoadingAnimationCenterPoint(CompositionVisual visual)
    {
        var size = Bounds.Size;
        visual.CenterPoint = new Vector3D(size.Width / 2, size.Height / 2, 0);
    }

    private TimeSpan GetCompositionAnimationDuration()
    {
        return LoadingAnimationDuration < TimeSpan.FromMilliseconds(1)
            ? TimeSpan.FromMilliseconds(1)
            : LoadingAnimationDuration;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureLoadingAnimationVisibilityTracking();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _effectiveVisibilitySubscriptions?.Dispose();
        _effectiveVisibilitySubscriptions = null;
        StopLoadingAnimation();
        base.OnUnloaded(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureLoadingAnimationVisibilityTracking();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _effectiveVisibilitySubscriptions?.Dispose();
        _effectiveVisibilitySubscriptions = null;
        StopLoadingAnimation();
        base.OnDetachedFromVisualTree(e);
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

    public virtual Icon CreateInstance()
    {
        throw new NotSupportedException(
            $"{GetType().FullName} must override {nameof(CreateInstance)} for AOT-safe cloning.");
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
