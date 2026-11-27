using System.Globalization;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Draws the scroll progress ring along the edge of the BackTop float button, matching the progress ring of
/// antd FloatButton.BackTop. <see cref="Progress" /> is a fraction from 0 to 1 that fills the ring clockwise
/// from 12 o'clock. The track color, the progress color and the stroke thickness come from the theme
/// (ColorBorderSecondary, ColorPrimary and LineWidthBold). The indicator is a sector fill clipped to the
/// ring band, which follows the angle semantics of the antd conic gradient.
/// The sweep angle is a continuous runtime value that no ControlTheme selector can express, so the ring is
/// drawn in <see cref="Render" /> instead of through theme bindings, in the same way as
/// <c>AbstractGeneralCircleProgress</c>.
/// </summary>
internal class BackTopProgressRing : Control
{
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<BackTopProgressRing, double>(nameof(Progress));

    internal static readonly StyledProperty<BackTopProgressRingTokenResourceKeys?> ProgressRingTokenResourceKeysProperty =
        AvaloniaProperty.Register<BackTopProgressRing, BackTopProgressRingTokenResourceKeys?>(nameof(ProgressRingTokenResourceKeys));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<BackTopProgressRing, CornerRadius>(nameof(CornerRadius));

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>
    /// Resource keys of the stroke thickness, the track brush and the indicator brush of the ring. While the ring is
    /// attached, it observes the keys and repaints when a value changes. <see langword="null" />, the default,
    /// observes nothing: the thickness falls back to 2 and the brushes stay unset. The theme of the back-top button
    /// sets the keys.
    /// </summary>
    internal BackTopProgressRingTokenResourceKeys? ProgressRingTokenResourceKeys
    {
        get => GetValue(ProgressRingTokenResourceKeysProperty);
        set => SetValue(ProgressRingTokenResourceKeysProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    static BackTopProgressRing()
    {
        AffectsRender<BackTopProgressRing>(ProgressProperty, CornerRadiusProperty);
    }

    private IPen? _trackPen;
    private OwnerTokenSet? _paintTokens;
    private (double Thickness, IBrush? Track, IBrush? Indicator) _paint = (2d, null, null);

    internal (double Thickness, IBrush? Track, IBrush? Indicator) Paint => _paint;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePaintTokenSubscription();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _paintTokens?.Dispose();
        _paintTokens = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ProgressRingTokenResourceKeysProperty && this.IsAttachedToVisualTree())
        {
            UpdatePaintTokenSubscription();
        }
    }

    private void UpdatePaintTokenSubscription()
    {
        _paintTokens?.Dispose();
        _paintTokens = null;
        if (ProgressRingTokenResourceKeys is { } keys)
        {
            _paintTokens = new OwnerTokenSet(this, 3, RefreshPaint);
            _paintTokens.SetResourceKey(0, keys.Thickness);
            _paintTokens.SetResourceKey(1, keys.Track);
            _paintTokens.SetResourceKey(2, keys.Indicator);
        }
        RefreshPaint();
    }

    private void RefreshPaint()
    {
        var paint = (
            ReadThickness(_paintTokens?.GetValue(0)),
            ReadBrush(_paintTokens?.GetValue(1), _paint.Track),
            ReadBrush(_paintTokens?.GetValue(2), _paint.Indicator));
        if (!_paint.Equals(paint))
        {
            _paint = paint;
            InvalidateVisual();
        }
    }

    private static IBrush? ReadBrush(object? value, IBrush? previous) => value switch
    {
        IBrush brush => brush,
        Color color => new ImmutableSolidColorBrush(color),
        string text when Color.TryParse(text, out var color) => new ImmutableSolidColorBrush(color),
        _ when ReferenceEquals(value, BindingOperations.DoNothing) => previous,
        _ => null
    };

    private double ReadThickness(object? value)
    {
        if (value is double thickness)
        {
            return thickness;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return _paint.Thickness;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 2d;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 2d;
        }
    }

    /// <summary>
    /// Computes the geometry of the ring: the outer, centerline and inner boundary rectangles with their corner
    /// radii. The outer radius must equal the corner radius of the button itself so that the outer edge of the
    /// ring coincides exactly with the outline of the button; the centerline radius is the outer radius minus half
    /// the stroke thickness, and the inner radius is the outer radius minus the stroke thickness.
    /// </summary>
    internal static BackTopProgressRingMetrics CalculateRingMetrics(
        Size size, double cornerRadius, double thickness)
    {
        var outerRect      = new Rect(size);
        var centerlineRect = outerRect.Deflate(thickness / 2);
        var innerRect      = outerRect.Deflate(thickness);
        var halfThickness  = thickness / 2;

        var outerLimit    = Math.Min(outerRect.Width, outerRect.Height) / 2;
        var outerRadius   = Math.Clamp(cornerRadius, 0d, outerLimit);
        var centerlineRadius = Math.Max(0d, outerRadius - halfThickness);
        var innerRadius      = Math.Max(0d, outerRadius - thickness);

        return new BackTopProgressRingMetrics(outerRect, centerlineRect, innerRect,
            outerRadius, centerlineRadius, innerRadius);
    }

    public override void Render(DrawingContext context)
    {
        var bounds    = new Rect(Bounds.Size);
        var (thickness, track, indicator) = Paint;
        if (bounds.Width <= 0 || bounds.Height <= 0 || thickness <= 0)
        {
            return;
        }

        var metrics = CalculateRingMetrics(bounds.Size, CornerRadius.TopLeft, thickness);

        PenUtils.TryModifyOrCreate(ref _trackPen, track, thickness);
        if (_trackPen is not null)
        {
            // The track stroke is centered on the centerline rectangle, so its outer edge coincides with the button
            // outline. The Circle and Square shapes share this path.
            context.DrawRectangle(null, _trackPen, metrics.CenterlineRect,
                metrics.CenterlineRadius, metrics.CenterlineRadius);
        }

        var sweepAngle = 360d * Math.Clamp(Progress, 0d, 1d);
        if (sweepAngle <= 0 || indicator is null)
        {
            return;
        }

        // The progress sector is clipped to the band left when the inner rounded rectangle is subtracted from the
        // outer one. The Circle and Square shapes share this path.
        // The indicator must be a filled sector, not an arc stroke inscribed in the ring rectangle. For the Square
        // shape, the inscribed arc has a smaller radius than the outer boundary in the corner direction, so it falls
        // into the inner hole and is clipped away, and the progress ring degenerates into four short segments on
        // the sides.
        var bandGeometry = new CombinedGeometry(GeometryCombineMode.Exclude,
            BuildRoundedRectGeometry(metrics.OuterRect, metrics.OuterRadius),
            BuildRoundedRectGeometry(metrics.InnerRect, metrics.InnerRadius));

        if (sweepAngle >= 360d)
        {
            // The start and end points of the sector coincide here, so fill the whole band with the progress color.
            context.DrawGeometry(indicator, null, bandGeometry);
            return;
        }

        // A sector with its vertex at the center of the ring. Its radius is half the diagonal, which covers the
        // outermost point of the band, and it sweeps clockwise by sweepAngle from -90 degrees (12 o'clock).
        // The angle-to-point conversion follows the convention of CommonShapeBuilder.GetRingPoint:
        // x = cx + r * cos(angle), y = cy + r * sin(angle).
        var center = metrics.CenterlineRect.Center;
        var sectorRadius = Math.Sqrt(metrics.CenterlineRect.Width * metrics.CenterlineRect.Width / 4
                                     + metrics.CenterlineRect.Height * metrics.CenterlineRect.Height / 4);

        var startPoint = new Point(
            center.X + sectorRadius * Math.Cos(MathUtils.Deg2Rad(-90d)),
            center.Y + sectorRadius * Math.Sin(MathUtils.Deg2Rad(-90d)));
        var endPoint = new Point(
            center.X + sectorRadius * Math.Cos(MathUtils.Deg2Rad(sweepAngle - 90d)),
            center.Y + sectorRadius * Math.Sin(MathUtils.Deg2Rad(sweepAngle - 90d)));

        var sectorGeometry = new StreamGeometry();
        using (var ctx = sectorGeometry.Open())
        {
            ctx.BeginFigure(center, true);
            ctx.LineTo(startPoint);
            ctx.ArcTo(endPoint, new Size(sectorRadius, sectorRadius), 0d,
                sweepAngle > 180d, SweepDirection.Clockwise);
            ctx.EndFigure(true);
        }

        using (context.PushGeometryClip(bandGeometry))
        {
            context.DrawGeometry(indicator, null, sectorGeometry);
        }
    }

    private static StreamGeometry BuildRoundedRectGeometry(Rect rect, double radius)
    {
        var geometry = new StreamGeometry();
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return geometry;
        }

        radius = Math.Clamp(radius, 0d, Math.Min(rect.Width, rect.Height) / 2);
        using var ctx = geometry.Open();
        if (radius <= 0)
        {
            ctx.BeginFigure(rect.TopLeft, true);
            ctx.LineTo(rect.TopRight);
            ctx.LineTo(rect.BottomRight);
            ctx.LineTo(rect.BottomLeft);
            ctx.EndFigure(true);
            return geometry;
        }

        ctx.BeginFigure(new Point(rect.Left + radius, rect.Top), true);
        ctx.LineTo(new Point(rect.Right - radius, rect.Top));
        ctx.ArcTo(new Point(rect.Right, rect.Top + radius),
            new Size(radius, radius), 0d, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(rect.Right, rect.Bottom - radius));
        ctx.ArcTo(new Point(rect.Right - radius, rect.Bottom),
            new Size(radius, radius), 0d, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(rect.Left + radius, rect.Bottom));
        ctx.ArcTo(new Point(rect.Left, rect.Bottom - radius),
            new Size(radius, radius), 0d, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(rect.Left, rect.Top + radius));
        ctx.ArcTo(new Point(rect.Left + radius, rect.Top),
            new Size(radius, radius), 0d, false, SweepDirection.Clockwise);
        ctx.EndFigure(true);
        return geometry;
    }
}

/// <summary>
/// Resource keys of the paint of the ring. The theme sets them through
/// <see cref="BackTopProgressRing.ProgressRingTokenResourceKeys" />, and the ring resolves them while attached.
/// </summary>
internal sealed record BackTopProgressRingTokenResourceKeys(object Thickness, object Track, object Indicator);

/// <summary>
/// The geometry of the progress ring. The outer boundary is the outline of the button, with a radius equal to
/// the corner radius of the button; the centerline and the inner boundary are inset from it by the stroke
/// thickness.
/// </summary>
internal readonly record struct BackTopProgressRingMetrics(
    Rect OuterRect,
    Rect CenterlineRect,
    Rect InnerRect,
    double OuterRadius,
    double CenterlineRadius,
    double InnerRadius);
