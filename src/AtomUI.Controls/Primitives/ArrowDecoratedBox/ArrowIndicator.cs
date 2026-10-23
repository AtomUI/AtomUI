using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls;

public class ArrowIndicator : Control
{
    #region 公共属性定义

    public static readonly StyledProperty<double> ArrowSizeProperty =
        AvaloniaProperty.Register<ArrowIndicator, double>(nameof(ArrowSize));

    public static readonly StyledProperty<double> TipRadiusProperty =
        AvaloniaProperty.Register<ArrowIndicator, double>(nameof(TipRadius), 1.5,
            validate: static value => double.IsFinite(value) && value >= 0);

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

    /// <summary>The rounded arrow tip radius in DIPs. Zero produces a sharp tip.</summary>
    public double TipRadius
    {
        get => GetValue(TipRadiusProperty);
        set => SetValue(TipRadiusProperty, value);
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

    #endregion

    private Geometry? _arrowGeometry;
    private IPen? _cachedPen;

    static ArrowIndicator()
    {
        AffectsMeasure<ArrowIndicator>(ArrowSizeProperty, TipRadiusProperty);
        AffectsRender<ArrowIndicator>(ArrowSizeProperty, TipRadiusProperty, FilledColorProperty, StrokeBrushProperty, StrokeThicknessProperty);
        HorizontalAlignmentProperty.OverrideDefaultValue<ArrowIndicator>(HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<ArrowIndicator>(VerticalAlignment.Top);
    }

    private void BuildGeometry(bool force = false)
    {
        if (_arrowGeometry is null || force)
        {
            _arrowGeometry = CommonShapeBuilder.BuildArrow(ArrowSize, TipRadius);
            var matrix = Matrix.CreateTranslation(_arrowGeometry.Bounds.X, -_arrowGeometry.Bounds.Y);
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
        if ((change.Property == ArrowSizeProperty || change.Property == TipRadiusProperty) && _arrowGeometry is not null)
        {
            BuildGeometry(true);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return _arrowGeometry?.Bounds.Size ?? new Size(ArrowSize, ArrowSize);
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
