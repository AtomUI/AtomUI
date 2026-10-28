using System.Collections.Specialized;
using AtomUI.Animations;
using AtomUI.Controls.Utils;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class BorderBeamPresenter : Control
{
    #region 内部属性定义

    internal static readonly StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, Color?>(nameof(Color));

    internal static readonly StyledProperty<AvaloniaList<BorderBeamColorStop>?> ColorStopsProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, AvaloniaList<BorderBeamColorStop>?>(nameof(ColorStops));

    internal static readonly StyledProperty<Thickness?> OutsetProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, Thickness?>(nameof(Outset));

    internal static readonly StyledProperty<BorderBeamGeometry> BorderBeamGeometryProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, BorderBeamGeometry>(nameof(BorderBeamGeometry));

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, bool>(nameof(IsMotionEnabled), true);

    internal static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, TimeSpan>(nameof(Duration), TimeSpan.FromSeconds(6));

    internal static readonly StyledProperty<double> BeamSizeProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, double>(nameof(BeamSize), 100d);

    internal static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, int>(nameof(Count), 1);

    internal static readonly StyledProperty<double> BeamOpacityProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, double>(nameof(BeamOpacity), 0.95d);

    internal static readonly StyledProperty<double> MaxVisibleStopPercentProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, double>(nameof(MaxVisibleStopPercent), 70d);

    internal static readonly StyledProperty<IBrush?> DefaultStartColorProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, IBrush?>(nameof(DefaultStartColor));

    internal static readonly StyledProperty<IBrush?> DefaultEndColorProperty =
        AvaloniaProperty.Register<BorderBeamPresenter, IBrush?>(nameof(DefaultEndColor));

    internal static readonly DirectProperty<BorderBeamPresenter, double> ProgressProperty =
        AvaloniaProperty.RegisterDirect<BorderBeamPresenter, double>(
            nameof(Progress),
            o => o.Progress,
            (o, v) => o.Progress = v);

    internal Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    internal AvaloniaList<BorderBeamColorStop>? ColorStops
    {
        get => GetValue(ColorStopsProperty);
        set => SetValue(ColorStopsProperty, value);
    }

    internal Thickness? Outset
    {
        get => GetValue(OutsetProperty);
        set => SetValue(OutsetProperty, value);
    }

    internal BorderBeamGeometry BorderBeamGeometry
    {
        get => GetValue(BorderBeamGeometryProperty);
        set => SetValue(BorderBeamGeometryProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    internal double BeamSize
    {
        get => GetValue(BeamSizeProperty);
        set => SetValue(BeamSizeProperty, value);
    }

    internal int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    internal double BeamOpacity
    {
        get => GetValue(BeamOpacityProperty);
        set => SetValue(BeamOpacityProperty, value);
    }

    internal double MaxVisibleStopPercent
    {
        get => GetValue(MaxVisibleStopPercentProperty);
        set => SetValue(MaxVisibleStopPercentProperty, value);
    }

    internal IBrush? DefaultStartColor
    {
        get => GetValue(DefaultStartColorProperty);
        set => SetValue(DefaultStartColorProperty, value);
    }

    internal IBrush? DefaultEndColor
    {
        get => GetValue(DefaultEndColorProperty);
        set => SetValue(DefaultEndColorProperty, value);
    }

    private double _progress;

    internal double Progress
    {
        get => _progress;
        set => SetAndRaise(ProgressProperty, ref _progress, BorderBeamPathSampler.NormalizeProgress(value));
    }

    #endregion

    private Animation? _animation;
    private IDisposable? _animationSubscription;
    private Rect? _effectiveViewport;
    private AvaloniaList<BorderBeamColorStop>? _subscribedColorStops;

    static BorderBeamPresenter()
    {
        AffectsRender<BorderBeamPresenter>(
            ColorProperty,
            ColorStopsProperty,
            OutsetProperty,
            BorderBeamGeometryProperty,
            IsMotionEnabledProperty,
            BeamSizeProperty,
            CountProperty,
            BeamOpacityProperty,
            MaxVisibleStopPercentProperty,
            DefaultStartColorProperty,
            DefaultEndColorProperty,
            ProgressProperty,
            UseLayoutRoundingProperty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _effectiveViewport = null;
        EffectiveViewportChanged += HandleEffectiveViewportChanged;
        ConfigureColorStopsSubscription(ColorStops);
        UpdateAnimationState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        EffectiveViewportChanged -= HandleEffectiveViewportChanged;
        _effectiveViewport = null;
        StopAnimation();
        ConfigureColorStopsSubscription(null);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        InvalidateVisual();
        UpdateAnimationState();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ColorStopsProperty)
        {
            ConfigureColorStopsSubscription(change.GetNewValue<AvaloniaList<BorderBeamColorStop>?>());
        }

        if (change.Property == IsMotionEnabledProperty ||
            change.Property == DurationProperty ||
            change.Property == OutsetProperty ||
            change.Property == IsVisibleProperty)
        {
            RestartAnimation();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!ShouldRenderBeam())
        {
            return;
        }

        var borderThickness = BorderUtils.BuildRenderScaleAwareThickness(this, BorderBeamGeometry.BorderThickness);
        var renderBounds    = GetRenderBounds();
        var borderGeometry  = CreateBorderGeometry(renderBounds, borderThickness, BorderBeamGeometry.CornerRadius);
        if (borderGeometry is null)
        {
            return;
        }

        var effectiveCount = Math.Max(1, Count);
        var pathMetrics    = BorderBeamPathSampler.CreateMetrics(renderBounds, BeamSize);
        var beamBrush      = CreateBeamBrush();
        var beamBounds     = CreateBeamBounds();

        using var opacityState = context.PushOpacity(Math.Clamp(BeamOpacity, 0d, 1d));
        using var clipState    = context.PushGeometryClip(borderGeometry);
        for (var index = 0; index < effectiveCount; index++)
        {
            var beamProgress = BorderBeamPathSampler.GetBeamProgress(Progress, index, effectiveCount);
            var pathPoint    = BorderBeamPathSampler.GetPointAtProgress(pathMetrics, beamProgress);
            var transform    = BorderBeamPathSampler.CreateBeamTransform(pathPoint, BeamSize);
            using var transformState = context.PushTransform(transform);
            context.DrawRectangle(beamBrush, null, beamBounds);
        }
    }

    private void RestartAnimation()
    {
        StopAnimation();
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (!ShouldRunAnimation())
        {
            StopAnimation();
            return;
        }

        if (_animationSubscription is not null)
        {
            return;
        }

        _animation ??= new Animation
        {
            Duration = Duration,
            Easing   = new LinearEasing(),
            Children =
            {
                new KeyFrame
                {
                    Cue     = new Cue(0d),
                    Setters = { new Setter(ProgressProperty, 0d) }
                },
                new KeyFrame
                {
                    Cue     = new Cue(1d),
                    Setters = { new Setter(ProgressProperty, 1d) }
                }
            }
        };
        _animation.Duration = Duration;

        // Running depends on the actual visible viewport and a runtime duration
        // snapshot, which ControlTheme cannot express without duplicate state.
        var subscription = _animation.PlayLoop(this, ProgressProperty);
        if (ShouldRunAnimation() && _animationSubscription is null)
        {
            _animationSubscription = subscription;
        }
        else
        {
            subscription.Dispose();
        }
    }

    private void StopAnimation()
    {
        var subscription = _animationSubscription;
        _animationSubscription = null;
        subscription?.Dispose();
    }

    private void HandleEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        _effectiveViewport = e.EffectiveViewport;
        UpdateAnimationState();
    }

    private bool ShouldRunAnimation()
    {
        return IsMotionEnabled &&
               IsVisible &&
               this.IsAttachedToVisualTree() &&
               Duration > TimeSpan.Zero &&
               Bounds.Width > 0 &&
               Bounds.Height > 0 &&
               _effectiveViewport is { } viewport && viewport.Intersects(GetRenderBounds());
    }

    private bool ShouldRenderBeam()
    {
        return IsMotionEnabled &&
               Bounds.Width > 0 &&
               Bounds.Height > 0 &&
               BeamSize > 0 &&
               BeamOpacity > 0 &&
               HasVisibleBorder(BorderBeamGeometry.BorderThickness);
    }

    private Rect GetRenderBounds()
    {
        var renderBounds = new Rect(Bounds.Size);
        // Keep the default beam on the presenter's boundary. Outset is a
        // paint-only offset and must not expand the control's layout bounds.
        var outset = Outset.HasValue
            ? BorderUtils.BuildRenderScaleAwareThickness(this, Outset.Value)
            : default;
        return renderBounds.Inflate(outset);
    }

    private Geometry? CreateBorderGeometry(Rect bounds, Thickness borderThickness, CornerRadius cornerRadius)
    {
        if (bounds.Width <= 0 ||
            bounds.Height <= 0 ||
            !HasVisibleBorder(borderThickness))
        {
            return null;
        }

        var innerGeometry = CreateRoundedRectGeometry(
            bounds,
            borderThickness,
            cornerRadius,
            BackgroundSizing.InnerBorderEdge);
        var outerGeometry = CreateRoundedRectGeometry(
            bounds,
            borderThickness,
            cornerRadius,
            BackgroundSizing.OuterBorderEdge);

        return outerGeometry is null
            ? null
            : innerGeometry is null
                ? outerGeometry
                : new CombinedGeometry(GeometryCombineMode.Exclude, outerGeometry, innerGeometry);
    }

    private static Geometry? CreateRoundedRectGeometry(
        Rect bounds,
        Thickness borderThickness,
        CornerRadius cornerRadius,
        BackgroundSizing backgroundSizing)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return null;
        }

        if (backgroundSizing == BackgroundSizing.InnerBorderEdge)
        {
            var innerBounds = bounds.Deflate(borderThickness);
            if (innerBounds.Width <= 0 || innerBounds.Height <= 0)
            {
                return null;
            }
        }

        var keypoints = RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI(
            bounds,
            borderThickness,
            cornerRadius,
            backgroundSizing);

        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        RoundRectGeometryBuilder.DrawRoundedCornersRectangle(ctx, ref keypoints);
        return geometry;
    }

    private LinearGradientBrush CreateBeamBrush()
    {
        var halfSize = BeamSize / 2d;
        var brush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(new Point(BeamSize, halfSize), RelativeUnit.Absolute),
            EndPoint   = new RelativePoint(new Point(0d, halfSize), RelativeUnit.Absolute),
            SpreadMethod = GradientSpreadMethod.Pad
        };

        var defaultStartColor = ResolveDefaultColor(DefaultStartColor, Colors.Transparent);
        var defaultEndColor   = ResolveDefaultColor(DefaultEndColor, defaultStartColor);
        foreach (var stop in BorderBeamColorStops.Normalize(
                     ColorStops,
                     Color,
                     defaultStartColor,
                     defaultEndColor,
                     MaxVisibleStopPercent))
        {
            brush.GradientStops.Add(new GradientStop(stop.Color, stop.Offset));
        }

        return brush;
    }

    private Rect CreateBeamBounds()
    {
        return new Rect(0d, 0d, BeamSize, BeamSize);
    }

    private static bool HasVisibleBorder(Thickness thickness)
    {
        return thickness.Left > 0 ||
               thickness.Top > 0 ||
               thickness.Right > 0 ||
               thickness.Bottom > 0;
    }

    private static Color ResolveDefaultColor(IBrush? brush, Color fallback)
    {
        return brush is ISolidColorBrush solidColorBrush
            ? solidColorBrush.Color
            : fallback;
    }

    private void ConfigureColorStopsSubscription(AvaloniaList<BorderBeamColorStop>? colorStops)
    {
        if (ReferenceEquals(_subscribedColorStops, colorStops))
        {
            return;
        }

        if (_subscribedColorStops is not null)
        {
            _subscribedColorStops.CollectionChanged -= HandleColorStopsChanged;
        }

        _subscribedColorStops = colorStops;
        if (_subscribedColorStops is not null)
        {
            _subscribedColorStops.CollectionChanged += HandleColorStopsChanged;
        }
    }

    private void HandleColorStopsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }
}

internal readonly record struct BorderBeamPathPoint(Point Point, Vector Tangent);

internal readonly record struct BorderBeamPathMetrics(
    RoundRectGeometryBuilder.RoundedRectKeypoints Keypoints,
    double Perimeter,
    Point FallbackPoint);

internal static class BorderBeamPathSampler
{
    private const double QuarterTurn               = Math.PI / 2d;
    private const int    ArcLengthIntegrationSteps = 12;
    private const int    ArcLengthSearchIterations = 12;

    internal static double GetBeamProgress(double progress, int index, int count)
    {
        var effectiveCount = Math.Max(1, count);
        return NormalizeProgress(progress + index / (double)effectiveCount);
    }

    internal static BorderBeamPathPoint GetPointAtProgress(Rect bounds, double pathCornerRadius, double progress)
    {
        return GetPointAtProgress(CreateMetrics(bounds, pathCornerRadius), progress);
    }

    internal static BorderBeamPathMetrics CreateMetrics(Rect bounds, double pathCornerRadius)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return new BorderBeamPathMetrics(default, 0d, bounds.TopLeft);
        }

        var keypoints = RoundRectGeometryBuilder.CalculateRoundedCornersRectangleWinUI(
            bounds,
            default,
            new CornerRadius(Math.Max(pathCornerRadius, 0d)),
            BackgroundSizing.OuterBorderEdge);
        var perimeter = GetPerimeter(ref keypoints);
        return new BorderBeamPathMetrics(keypoints, perimeter, bounds.TopLeft);
    }

    internal static BorderBeamPathPoint GetPointAtProgress(BorderBeamPathMetrics metrics, double progress)
    {
        if (MathUtils.IsZero(metrics.Perimeter))
        {
            return new BorderBeamPathPoint(metrics.FallbackPoint, new Vector(1, 0));
        }

        var keypoints = metrics.Keypoints;
        var distance  = NormalizeProgress(progress) * metrics.Perimeter;
        return GetPointAtDistance(ref keypoints, distance);
    }

    internal static Matrix CreateBeamTransform(BorderBeamPathPoint pathPoint, double beamSize)
    {
        var tangent = Normalize(pathPoint.Tangent);
        var normal  = new Vector(-tangent.Y, tangent.X);
        var anchorX = beamSize * 0.9d;
        var anchorY = beamSize * 0.5d;
        var origin  = pathPoint.Point - tangent * anchorX - normal * anchorY;
        return new Matrix(
            tangent.X,
            tangent.Y,
            normal.X,
            normal.Y,
            origin.X,
            origin.Y);
    }

    internal static double NormalizeProgress(double progress)
    {
        if (double.IsNaN(progress) || double.IsInfinity(progress))
        {
            return 0d;
        }

        progress %= 1d;
        return progress < 0d ? progress + 1d : progress;
    }

    private static BorderBeamPathPoint GetPointAtDistance(
        ref RoundRectGeometryBuilder.RoundedRectKeypoints keypoints,
        double distance)
    {
        if (TryConsumeLine(ref distance, keypoints.TopLeft, keypoints.TopRight, out var point))
        {
            return point;
        }

        if (TryConsumeArc(
                ref distance,
                keypoints.TopRight,
                keypoints.RightTop,
                new Point(keypoints.TopRight.X, keypoints.RightTop.Y),
                -QuarterTurn,
                0d,
                out point))
        {
            return point;
        }

        if (TryConsumeLine(ref distance, keypoints.RightTop, keypoints.RightBottom, out point))
        {
            return point;
        }

        if (TryConsumeArc(
                ref distance,
                keypoints.RightBottom,
                keypoints.BottomRight,
                new Point(keypoints.BottomRight.X, keypoints.RightBottom.Y),
                0d,
                QuarterTurn,
                out point))
        {
            return point;
        }

        if (TryConsumeLine(ref distance, keypoints.BottomRight, keypoints.BottomLeft, out point))
        {
            return point;
        }

        if (TryConsumeArc(
                ref distance,
                keypoints.BottomLeft,
                keypoints.LeftBottom,
                new Point(keypoints.BottomLeft.X, keypoints.LeftBottom.Y),
                QuarterTurn,
                Math.PI,
                out point))
        {
            return point;
        }

        if (TryConsumeLine(ref distance, keypoints.LeftBottom, keypoints.LeftTop, out point))
        {
            return point;
        }

        if (TryConsumeArc(
                ref distance,
                keypoints.LeftTop,
                keypoints.TopLeft,
                new Point(keypoints.TopLeft.X, keypoints.LeftTop.Y),
                Math.PI,
                Math.PI + QuarterTurn,
                out point))
        {
            return point;
        }

        return new BorderBeamPathPoint(keypoints.TopLeft, new Vector(1, 0));
    }

    private static double GetPerimeter(ref RoundRectGeometryBuilder.RoundedRectKeypoints keypoints)
    {
        return GetLineLength(keypoints.TopLeft, keypoints.TopRight) +
               GetArcLength(
                   keypoints.TopRight,
                   keypoints.RightTop,
                   new Point(keypoints.TopRight.X, keypoints.RightTop.Y)) +
               GetLineLength(keypoints.RightTop, keypoints.RightBottom) +
               GetArcLength(
                   keypoints.RightBottom,
                   keypoints.BottomRight,
                   new Point(keypoints.BottomRight.X, keypoints.RightBottom.Y)) +
               GetLineLength(keypoints.BottomRight, keypoints.BottomLeft) +
               GetArcLength(
                   keypoints.BottomLeft,
                   keypoints.LeftBottom,
                   new Point(keypoints.BottomLeft.X, keypoints.LeftBottom.Y)) +
               GetLineLength(keypoints.LeftBottom, keypoints.LeftTop) +
               GetArcLength(
                   keypoints.LeftTop,
                   keypoints.TopLeft,
                   new Point(keypoints.TopLeft.X, keypoints.LeftTop.Y));
    }

    private static bool TryConsumeLine(
        ref double distance,
        Point start,
        Point end,
        out BorderBeamPathPoint point)
    {
        var vector = new Vector(end.X - start.X, end.Y - start.Y);
        var length = vector.Length;
        if (MathUtils.IsZero(length))
        {
            point = default;
            return false;
        }

        if (distance <= length)
        {
            var ratio = distance / length;
            point = new BorderBeamPathPoint(
                new Point(start.X + vector.X * ratio, start.Y + vector.Y * ratio),
                vector / length);
            return true;
        }

        distance -= length;
        point = default;
        return false;
    }

    private static bool TryConsumeArc(
        ref double distance,
        Point start,
        Point end,
        Point center,
        double startAngle,
        double endAngle,
        out BorderBeamPathPoint point)
    {
        var radiusX = GetArcRadius(start.X, end.X, center.X);
        var radiusY = GetArcRadius(start.Y, end.Y, center.Y);
        var length  = GetQuarterEllipseLength(radiusX, radiusY);
        if (MathUtils.IsZero(length))
        {
            point = default;
            return false;
        }

        if (distance <= length)
        {
            var angle   = GetAngleAtArcDistance(radiusX, radiusY, startAngle, endAngle, distance);
            var tangent = Normalize(new Vector(-radiusX * Math.Sin(angle), radiusY * Math.Cos(angle)));
            point = new BorderBeamPathPoint(
                new Point(center.X + radiusX * Math.Cos(angle), center.Y + radiusY * Math.Sin(angle)),
                tangent);
            return true;
        }

        distance -= length;
        point = default;
        return false;
    }

    private static double GetLineLength(Point start, Point end)
    {
        return new Vector(end.X - start.X, end.Y - start.Y).Length;
    }

    private static double GetArcLength(Point start, Point end, Point center)
    {
        return GetQuarterEllipseLength(
            GetArcRadius(start.X, end.X, center.X),
            GetArcRadius(start.Y, end.Y, center.Y));
    }

    private static double GetArcRadius(double start, double end, double center)
    {
        return Math.Max(Math.Abs(start - center), Math.Abs(end - center));
    }

    private static double GetQuarterEllipseLength(double radiusX, double radiusY)
    {
        if (MathUtils.IsZero(radiusX) || MathUtils.IsZero(radiusY))
        {
            return 0d;
        }

        return GetEllipseArcLength(radiusX, radiusY, 0d, QuarterTurn);
    }

    private static double GetAngleAtArcDistance(
        double radiusX,
        double radiusY,
        double startAngle,
        double endAngle,
        double distance)
    {
        if (distance <= 0d)
        {
            return startAngle;
        }

        var totalLength = GetEllipseArcLength(radiusX, radiusY, startAngle, endAngle);
        if (distance >= totalLength)
        {
            return endAngle;
        }

        var low  = startAngle;
        var high = endAngle;
        for (var i = 0; i < ArcLengthSearchIterations; i++)
        {
            var middle = (low + high) / 2d;
            var length = GetEllipseArcLength(radiusX, radiusY, startAngle, middle);
            if (length < distance)
            {
                low = middle;
            }
            else
            {
                high = middle;
            }
        }

        return (low + high) / 2d;
    }

    private static double GetEllipseArcLength(
        double radiusX,
        double radiusY,
        double startAngle,
        double endAngle)
    {
        var angleDelta = endAngle - startAngle;
        if (MathUtils.IsZero(angleDelta))
        {
            return 0d;
        }

        var step = angleDelta / ArcLengthIntegrationSteps;
        var sum  = GetEllipseArcSpeed(radiusX, radiusY, startAngle) +
                   GetEllipseArcSpeed(radiusX, radiusY, endAngle);
        for (var i = 1; i < ArcLengthIntegrationSteps; i++)
        {
            var angle  = startAngle + step * i;
            var factor = i % 2 == 0 ? 2d : 4d;
            sum += factor * GetEllipseArcSpeed(radiusX, radiusY, angle);
        }

        return Math.Abs(step * sum / 3d);
    }

    private static double GetEllipseArcSpeed(double radiusX, double radiusY, double angle)
    {
        var x = radiusX * Math.Sin(angle);
        var y = radiusY * Math.Cos(angle);
        return Math.Sqrt(x * x + y * y);
    }

    private static Vector Normalize(Vector vector)
    {
        var length = vector.Length;
        return MathUtils.IsZero(length)
            ? new Vector(1, 0)
            : vector / length;
    }
}
