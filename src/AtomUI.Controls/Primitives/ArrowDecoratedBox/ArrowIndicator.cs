using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls;

public class ArrowIndicator : Control
{
    public static readonly StyledProperty<double> ArrowSizeProperty =
        AvaloniaProperty.Register<ArrowIndicator, double>(nameof(ArrowSize));

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

    // CommonShapeBuilder.BuildArrow tip rounding; the outline points below follow the same construction.
    private const double TipRadius = 1.5;

    private Geometry? _arrowGeometry;
    private Vector _geometryOffset;
    private IPen? _cachedPen;

    static ArrowIndicator()
    {
        AffectsMeasure<ArrowIndicator>(ArrowSizeProperty);
        AffectsRender<ArrowIndicator>(ArrowSizeProperty, FilledColorProperty, StrokeBrushProperty, StrokeThicknessProperty);
        HorizontalAlignmentProperty.OverrideDefaultValue<ArrowIndicator>(HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<ArrowIndicator>(VerticalAlignment.Top);
    }

    private void BuildGeometry(bool force = false)
    {
        if (_arrowGeometry is null || force)
        {
            _arrowGeometry  = CommonShapeBuilder.BuildArrow(ArrowSize, TipRadius);
            _geometryOffset = new Vector(_arrowGeometry.Bounds.X, -_arrowGeometry.Bounds.Y);
            var matrix = Matrix.CreateTranslation(_geometryOffset.X, _geometryOffset.Y);
            _arrowGeometry.Transform =  new MatrixTransform(matrix);
        }
    }

    public sealed override void ApplyTemplate()
    {
        base.ApplyTemplate();
        BuildGeometry();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ArrowSizeProperty)
        {
            BuildGeometry(true);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return _arrowGeometry?.Bounds.Size ?? new Size(ArrowSize, ArrowSize);
    }

    /// <summary>
    /// The arrow's outline in this control's coordinates, in the order the geometry is drawn: the base start, the
    /// start of the rounded tip, the tip's control point, the end of the rounded tip and the base end. Returns
    /// <see langword="false"/> before the geometry exists.
    /// </summary>
    internal bool TryGetOutline(out ArrowOutline outline)
    {
        if (_arrowGeometry is null)
        {
            outline = default;
            return false;
        }

        // Same points as CommonShapeBuilder.BuildArrow, moved by the translation applied to the geometry.
        var size = ArrowSize;
        var tip  = new Point(size / 2.0, size / 2.0 + 1);
        outline = new ArrowOutline(
            new Point(0, size) + _geometryOffset,
            new Point(tip.X - TipRadius, tip.Y) + _geometryOffset,
            new Point(tip.X, tip.Y - TipRadius) + _geometryOffset,
            new Point(tip.X + TipRadius, tip.Y) + _geometryOffset,
            new Point(size, size) + _geometryOffset);
        return true;
    }

    public sealed override void Render(DrawingContext context)
    {
        if (_arrowGeometry is not null)
        {
            PenUtils.TryModifyOrCreate(ref _cachedPen, StrokeBrush, StrokeThickness);
            context.DrawGeometry(FilledColor, _cachedPen, _arrowGeometry);
        }
    }
}

/// <summary>
/// The outline of an <see cref="ArrowIndicator"/>: the two base points, the rounded tip (a quadratic curve from
/// <see cref="TipStart"/> through <see cref="TipControl"/> to <see cref="TipEnd"/>).
/// </summary>
internal readonly record struct ArrowOutline(Point BaseStart, Point TipStart, Point TipControl, Point TipEnd, Point BaseEnd);
