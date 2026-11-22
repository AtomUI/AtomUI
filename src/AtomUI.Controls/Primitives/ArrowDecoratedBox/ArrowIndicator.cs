using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// The arrow of an arrow decorated box: a right-angled isosceles triangle <see cref="ArrowSize"/> wide and half as
/// high, pointing up in its own coordinates (the box rotates it). The tip is rounded with a true arc of
/// <see cref="TipRadius"/> and the base corners flare into the box edge with arcs of <see cref="BaseRadius"/>, the
/// construction of the antd rounded arrow; both radii 0 give the plain border triangle of CSS frameworks.
/// </summary>
public class ArrowIndicator : Control
{
    public static readonly StyledProperty<double> ArrowSizeProperty =
        AvaloniaProperty.Register<ArrowIndicator, double>(nameof(ArrowSize));

    /// <summary>Radius of the rounded tip.</summary>
    public static readonly StyledProperty<double> TipRadiusProperty =
        AvaloniaProperty.Register<ArrowIndicator, double>(nameof(TipRadius));

    /// <summary>Radius of the concave arcs joining the two sides to the base edge.</summary>
    public static readonly StyledProperty<double> BaseRadiusProperty =
        AvaloniaProperty.Register<ArrowIndicator, double>(nameof(BaseRadius));

    public static readonly StyledProperty<IBrush?> FilledColorProperty =
        AvaloniaProperty.Register<ArrowIndicator, IBrush?>(nameof(FilledColor));

    public static readonly StyledProperty<IBrush?> StrokeBrushProperty =
        AvaloniaProperty.Register<ArrowIndicator, IBrush?>(nameof(StrokeBrush));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<ArrowIndicator, double>(nameof(StrokeThickness));

    public double ArrowSize
    {
        get => GetValue(ArrowSizeProperty);
        set => SetValue(ArrowSizeProperty, value);
    }

    public double TipRadius
    {
        get => GetValue(TipRadiusProperty);
        set => SetValue(TipRadiusProperty, value);
    }

    public double BaseRadius
    {
        get => GetValue(BaseRadiusProperty);
        set => SetValue(BaseRadiusProperty, value);
    }

    public IBrush? FilledColor
    {
        get => GetValue(FilledColorProperty);
        set => SetValue(FilledColorProperty, value);
    }

    public IBrush? StrokeBrush
    {
        get => GetValue(StrokeBrushProperty);
        set => SetValue(StrokeBrushProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    private ArrowOutline? _outline;
    private Geometry? _fillGeometry;
    private Geometry? _strokeGeometry;
    private IPen? _cachedPen;

    static ArrowIndicator()
    {
        AffectsMeasure<ArrowIndicator>(ArrowSizeProperty, TipRadiusProperty, BaseRadiusProperty);
        AffectsRender<ArrowIndicator>(ArrowSizeProperty, TipRadiusProperty, BaseRadiusProperty, FilledColorProperty,
            StrokeBrushProperty, StrokeThicknessProperty);
        HorizontalAlignmentProperty.OverrideDefaultValue<ArrowIndicator>(HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<ArrowIndicator>(VerticalAlignment.Top);
    }

    private ArrowOutline EnsureOutline()
    {
        if (_outline is { } outline)
        {
            return outline;
        }

        outline         = ArrowOutline.Create(ArrowSize, TipRadius, BaseRadius);
        _outline        = outline;
        _fillGeometry   = outline.BuildGeometry(true);
        _strokeGeometry = outline.BuildGeometry(false);
        return outline;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ArrowSizeProperty ||
            change.Property == TipRadiusProperty ||
            change.Property == BaseRadiusProperty)
        {
            _outline        = null;
            _fillGeometry   = null;
            _strokeGeometry = null;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return EnsureOutline().Size;
    }

    /// <summary>The arrow's outline in this control's coordinates (base on the bottom edge, tip at the top).</summary>
    internal bool TryGetOutline(out ArrowOutline outline)
    {
        if (ArrowSize <= 0)
        {
            outline = default;
            return false;
        }

        outline = EnsureOutline();
        return true;
    }

    public sealed override void Render(DrawingContext context)
    {
        if (ArrowSize <= 0)
        {
            return;
        }

        EnsureOutline();
        // The fill covers the whole triangle; the outline strokes only the two sides, so no line runs along the base
        // where the arrow meets the box.
        context.DrawGeometry(FilledColor, null, _fillGeometry!);
        if (StrokeThickness > 0 && StrokeBrush is not null)
        {
            PenUtils.TryModifyOrCreate(ref _cachedPen, StrokeBrush, StrokeThickness);
            context.DrawGeometry(null, _cachedPen, _strokeGeometry!);
        }
    }
}

/// <summary>
/// The outline of an <see cref="ArrowIndicator"/>, from the left base point to the right one: the concave base arc
/// from <see cref="BaseStart"/> to <see cref="SideStart"/>, the left side to <see cref="TipStart"/>, the tip arc to
/// <see cref="TipEnd"/>, the right side to <see cref="SideEnd"/> and the base arc to <see cref="BaseEnd"/>. Rotations
/// keep the arcs' sweep directions, so rotated points describe the same shape.
/// </summary>
internal readonly record struct ArrowOutline(
    Point BaseStart,
    Point SideStart,
    Point TipStart,
    Point TipEnd,
    Point SideEnd,
    Point BaseEnd,
    double TipRadius,
    double BaseRadius,
    Size Size)
{
    private static readonly double Sqrt2 = Math.Sqrt(2);

    /// <summary>
    /// Builds the outline of an arrow <paramref name="width"/> wide with its base <c>width / 2</c> below the unrounded
    /// tip, moved up so the top of the rounded tip is at y = 0 (the layout size is the visible shape).
    /// </summary>
    public static ArrowOutline Create(double width, double tipRadius, double baseRadius)
    {
        width = Math.Max(0, width);
        var half = width / 2;
        // The sides keep a non-negative length while (baseRadius + tipRadius) / √2 <= half; larger radii would fold
        // the outline, so the base arc gives way first, then the tip.
        baseRadius = Math.Clamp(baseRadius, 0, half * Sqrt2);
        tipRadius  = Math.Clamp(tipRadius, 0, half * Sqrt2 - baseRadius);

        // antd genRoundedArrow: the sides run at 45°, tangent to both base arcs and to the tip arc.
        var b   = new Point(baseRadius / Sqrt2, half - baseRadius * (1 - 1 / Sqrt2));
        var c   = new Point(half - tipRadius / Sqrt2, baseRadius * (Sqrt2 - 1) + tipRadius / Sqrt2);
        var top = (baseRadius + tipRadius) * (Sqrt2 - 1);
        var shift = new Vector(0, -top);
        return new ArrowOutline(
            new Point(0, half) + shift,
            b + shift,
            c + shift,
            new Point(width - c.X, c.Y) + shift,
            new Point(width - b.X, b.Y) + shift,
            new Point(width, half) + shift,
            tipRadius,
            baseRadius,
            new Size(width, half - top));
    }

    public ArrowOutline Transform(Matrix matrix) => this with
    {
        BaseStart = BaseStart.Transform(matrix),
        SideStart = SideStart.Transform(matrix),
        TipStart = TipStart.Transform(matrix),
        TipEnd = TipEnd.Transform(matrix),
        SideEnd = SideEnd.Transform(matrix),
        BaseEnd = BaseEnd.Transform(matrix)
    };

    /// <summary>The closed fill shape (<paramref name="closed"/>) or the open stroke path along the two sides.</summary>
    public Geometry BuildGeometry(bool closed)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(BaseStart, closed);
            AppendSides(context, false, BaseEnd);
            context.EndFigure(closed);
        }
        return geometry;
    }

    /// <summary>
    /// Appends the path after the first base point up to <paramref name="end"/>: from <see cref="BaseStart"/> to the
    /// right, or from <see cref="BaseEnd"/> to the left when <paramref name="reversed"/>. The caller passes the last
    /// base point, which may sit on another line than the arrow's own base.
    /// </summary>
    public void AppendSides(StreamGeometryContext context, bool reversed, Point end)
    {
        // Left to right the base arcs turn counter-clockwise and the tip clockwise; backwards they swap.
        var baseSweep = reversed ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;
        var tipSweep  = reversed ? SweepDirection.CounterClockwise : SweepDirection.Clockwise;
        AppendArc(context, reversed ? SideEnd : SideStart, BaseRadius, baseSweep);
        context.LineTo(reversed ? TipEnd : TipStart);
        AppendArc(context, reversed ? TipStart : TipEnd, TipRadius, tipSweep);
        context.LineTo(reversed ? SideStart : SideEnd);
        AppendArc(context, end, BaseRadius, baseSweep);
    }

    private static void AppendArc(StreamGeometryContext context, Point to, double radius, SweepDirection sweep)
    {
        if (radius > 0)
        {
            context.ArcTo(to, new Size(radius, radius), 0, false, sweep);
        }
        else
        {
            context.LineTo(to);
        }
    }
}
