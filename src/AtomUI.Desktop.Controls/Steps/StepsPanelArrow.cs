using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaFlowDirection = Avalonia.Media.FlowDirection;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Renders the stretchable wedge between adjacent panel steps.
/// </summary>
internal sealed class StepsPanelArrow : Control
{
    #region 公共属性定义

    public static readonly StyledProperty<IBrush?> FillBrushProperty =
        AvaloniaProperty.Register<StepsPanelArrow, IBrush?>(nameof(FillBrush));

    public static readonly StyledProperty<IBrush?> StrokeBrushProperty =
        AvaloniaProperty.Register<StepsPanelArrow, IBrush?>(nameof(StrokeBrush));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<StepsPanelArrow, double>(nameof(StrokeThickness), 1d);

    public IBrush? FillBrush
    {
        get => GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
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

    static StepsPanelArrow()
    {
        AffectsRender<StepsPanelArrow>(
            FillBrushProperty,
            StrokeBrushProperty,
            StrokeThicknessProperty,
            FlowDirectionProperty);
        IsHitTestVisibleProperty.OverrideDefaultValue<StepsPanelArrow>(false);
    }

    public override void Render(DrawingContext context)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        var thickness = Math.Max(0, StrokeThickness);
        if (width <= 0 || height <= 0 || (FillBrush is null && StrokeBrush is null))
        {
            return;
        }

        var geometry = new StreamGeometry();
        using (var geometryContext = geometry.Open())
        {
            if (FlowDirection == AvaloniaFlowDirection.RightToLeft)
            {
                geometryContext.BeginFigure(new Point(width, 0), true);
                geometryContext.LineTo(new Point(0, height / 2));
                geometryContext.LineTo(new Point(width, height));
            }
            else
            {
                geometryContext.BeginFigure(new Point(0, 0), true);
                geometryContext.LineTo(new Point(width, height / 2));
                geometryContext.LineTo(new Point(0, height));
            }

            // Match the SVG path: fill closes the wedge, while stroke only draws its two sloped edges.
            geometryContext.EndFigure(false);
        }

        var pen = StrokeBrush is not null && thickness > 0
            ? new Pen(StrokeBrush, thickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round)
            : null;
        context.DrawGeometry(FillBrush, pen, geometry);
    }
}
