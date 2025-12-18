using AtomUI.Media;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public enum DashboardGapPosition
{
    Left,
    Top,
    Right,
    Bottom
}

public class DashboardProgress : AbstractCircleProgress
{
    public const double DEFAULT_GAP_DEGREE = 75;
    public const double MIN_GAP_DEGREE = 0;
    public const double MAX_GAP_DEGREE = 295;

    #region 公共属性定义

    public static readonly StyledProperty<DashboardGapPosition> DashboardGapPositionProperty =
        AvaloniaProperty.Register<DashboardProgress, DashboardGapPosition>(
            nameof(DashboardGapPosition), DashboardGapPosition.Bottom);

    public static readonly StyledProperty<double> GapDegreeProperty =
        AvaloniaProperty.Register<DashboardProgress, double>(
            nameof(DashboardGapPosition), DEFAULT_GAP_DEGREE,
            coerce: (o, value) => Math.Clamp(value, MIN_GAP_DEGREE, MAX_GAP_DEGREE));

    public DashboardGapPosition DashboardGapPosition
    {
        get => GetValue(DashboardGapPositionProperty);
        set => SetValue(DashboardGapPositionProperty, value);
    }

    public double GapDegree
    {
        get => GetValue(GapDegreeProperty);
        set => SetValue(GapDegreeProperty, value);
    }

    #endregion
    
    private Rect _currentGrooveRect;
    private (double, double) _anglePair;

    static DashboardProgress()
    {
        AffectsRender<DashboardProgress>(DashboardGapPositionProperty,
            GapDegreeProperty);
    }

    protected override void RenderGroove(DrawingContext context)
    {
        var controlRect = new Rect(new Point(0, 0), Bounds.Size);
        _currentGrooveRect = GetProgressBarRect(controlRect).Deflate(StrokeThickness / 2);
        _currentGrooveRect = new Rect(_currentGrooveRect.Position, new Size(Math.Floor(_currentGrooveRect.Size.Width),
            Math.Floor(_currentGrooveRect.Size.Height)));
        if (StepCount > 0 && StepGap > 0)
        {
            DrawGrooveStep(context);
        }
        else
        {
            DrawGrooveNormal(context);
        }
    }

    private void DrawGrooveNormal(DrawingContext context)
    {
        var pen = new Pen(GrooveBrush, StrokeThickness)
        {
            LineCap = StrokeLineCap
        };
        context.DrawArc(pen, _currentGrooveRect, _anglePair.Item1, _anglePair.Item2);
    }

    private void DrawGrooveStep(DrawingContext context)
    {
        var pen = new Pen(GrooveBrush, StrokeThickness)
        {
            LineCap = PenLineCap.Flat
        };
        var spanAngle  = (360 - GapDegree - StepGap * StepCount) / StepCount;
        var startAngle = _anglePair.Item1;
        for (var i = 0; i < StepCount; ++i)
        {
            context.DrawArc(pen, _currentGrooveRect, startAngle, spanAngle);
            startAngle += StepGap + spanAngle;
        }
    }

    protected override void RenderIndicatorBar(DrawingContext context)
    {
        if (StepCount > 0 && StepGap > 0)
        {
            DrawIndicatorBarStep(context);
        }
        else
        {
            DrawIndicatorBarNormal(context);
        }
    }

    private void DrawIndicatorBarNormal(DrawingContext context)
    {
        var pen = new Pen(StrokeBrush, StrokeThickness)
        {
            LineCap = StrokeLineCap
        };
        context.DrawArc(pen, _currentGrooveRect, _anglePair.Item1, IndicatorAngle);

        if (!double.IsNaN(SuccessThreshold))
        {
            var successPen = new Pen(SuccessStrokeBrush, StrokeThickness)
            {
                LineCap = StrokeLineCap
            };
            context.DrawArc(successPen, _currentGrooveRect, _anglePair.Item1, CalculateAngle(SuccessThreshold));
        }
    }

    private void DrawIndicatorBarStep(DrawingContext context)
    {
        var pen = new Pen(StrokeBrush, StrokeThickness)
        {
            LineCap = PenLineCap.Flat
        };
        var spanAngle  = (360 - GapDegree - StepGap * StepCount) / StepCount;
        var startAngle = _anglePair.Item1;

        var   filledSteps  = (int)Math.Round(StepCount * Percentage / 100);
        int?  successSteps = null;
        IPen? successPen   = null;

        if (!double.IsNaN(SuccessThreshold))
        {
            successPen = new Pen(SuccessStrokeBrush, StrokeThickness)
            {
                LineCap = PenLineCap.Flat
            };
            successSteps = (int)Math.Round(StepCount * SuccessThreshold / (Maximum - Minimum));
        }

        IPen? currentPen;
        for (var i = 0; i < filledSteps; ++i)
        {
            currentPen = pen;
            if (successSteps.HasValue)
            {
                if (i < successSteps)
                {
                    currentPen = successPen;
                }
            }

            context.DrawArc(currentPen, _currentGrooveRect, startAngle, spanAngle);
            startAngle += StepGap + spanAngle;
        }
    }

    private (double, double) CalculateAngle(DashboardGapPosition position, double gapDegree)
    {
        double startAngle    = 0;
        double spanAngle     = 0;
        var    halfGapDegree = gapDegree / 2;
        if (position == DashboardGapPosition.Bottom)
        {
            startAngle = 90 + halfGapDegree;
        }
        else if (position == DashboardGapPosition.Left)
        {
            startAngle = 180 + halfGapDegree;
        }
        else if (position == DashboardGapPosition.Top)
        {
            startAngle = 270 + halfGapDegree;
        }
        else
        {
            startAngle = halfGapDegree;
        }

        spanAngle = 360 - gapDegree;
        return (startAngle, spanAngle);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == GapDegreeProperty || change.Property == DashboardGapPositionProperty)
        {
            _anglePair = CalculateAngle(DashboardGapPosition, GapDegree);
        }
    }

    protected override void NotifyUpdateProgress()
    {
        base.NotifyUpdateProgress();
        IndicatorAngle = CalculateAngle(Value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _anglePair = CalculateAngle(DashboardGapPosition, GapDegree);
    }

    private double CalculateAngle(double value)
    {
        return (360 - GapDegree) * value / (Maximum - Minimum);
    }
}