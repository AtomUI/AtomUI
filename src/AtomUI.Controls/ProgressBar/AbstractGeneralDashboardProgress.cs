using AtomUI.Media;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

public abstract class AbstractGeneralDashboardProgress : AbstractCircleProgress
{
    public const double DEFAULT_GAP_DEGREE = 75;
    public const double MIN_GAP_DEGREE = 0;
    public const double MAX_GAP_DEGREE = 295;

    #region 公共属性定义

    public static readonly StyledProperty<DashboardGapPosition> DashboardGapPositionProperty =
        AvaloniaProperty.Register<AbstractGeneralDashboardProgress, DashboardGapPosition>(
            nameof(DashboardGapPosition), DashboardGapPosition.Bottom);

    public static readonly StyledProperty<double> GapDegreeProperty =
        AvaloniaProperty.Register<AbstractGeneralDashboardProgress, double>(
            nameof(GapDegree), DEFAULT_GAP_DEGREE,
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
    private IPen? _groovePen;
    private IPen? _indicatorPen;
    private IPen? _successPen;

    static AbstractGeneralDashboardProgress()
    {
        AffectsRender<AbstractGeneralDashboardProgress>(DashboardGapPositionProperty,
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
        PenUtils.TryModifyOrCreate(ref _groovePen,
            GrooveBrush,
            StrokeThickness,
            lineCap: StrokeLineCap);
        context.DrawArc(_groovePen, _currentGrooveRect, _anglePair.Item1, _anglePair.Item2);
    }

    private void DrawGrooveStep(DrawingContext context)
    {
        PenUtils.TryModifyOrCreate(ref _groovePen,
            GrooveBrush,
            StrokeThickness,
            lineCap: PenLineCap.Flat);
        var spanAngle  = (360 - GapDegree - StepGap * StepCount) / StepCount;
        var startAngle = _anglePair.Item1;
        for (var i = 0; i < StepCount; ++i)
        {
            context.DrawArc(_groovePen, _currentGrooveRect, startAngle, spanAngle);
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
        PenUtils.TryModifyOrCreate(ref _indicatorPen,
            StrokeBrush,
            StrokeThickness,
            lineCap: StrokeLineCap);
        context.DrawArc(_indicatorPen, _currentGrooveRect, _anglePair.Item1, IndicatorAngle);

        if (!double.IsNaN(SuccessThreshold))
        {
            PenUtils.TryModifyOrCreate(ref _successPen,
                SuccessStrokeBrush,
                StrokeThickness,
                lineCap: StrokeLineCap);
            context.DrawArc(_successPen, _currentGrooveRect, _anglePair.Item1, CalculateAngle(SuccessThreshold));
        }
    }

    private void DrawIndicatorBarStep(DrawingContext context)
    {
        PenUtils.TryModifyOrCreate(ref _indicatorPen,
            StrokeBrush,
            StrokeThickness,
            lineCap: PenLineCap.Flat);
        var spanAngle  = (360 - GapDegree - StepGap * StepCount) / StepCount;
        var startAngle = _anglePair.Item1;

        var   filledSteps  = (int)Math.Round(StepCount * Percentage / 100);
        int?  successSteps = null;

        if (!double.IsNaN(SuccessThreshold))
        {
            PenUtils.TryModifyOrCreate(ref _successPen,
                SuccessStrokeBrush,
                StrokeThickness,
                lineCap: PenLineCap.Flat);
            successSteps = (int)Math.Round(StepCount * CalculateProgressRatio(SuccessThreshold));
        }

        IPen? currentPen;
        for (var i = 0; i < filledSteps; ++i)
        {
            currentPen = _indicatorPen;
            if (successSteps.HasValue)
            {
                if (i < successSteps)
                {
                    currentPen = _successPen;
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
            RefreshTemplateProgressVisuals();
        }
    }

    protected override void NotifyUpdateProgress()
    {
        base.NotifyUpdateProgress();
        IndicatorAngle = CalculateAngle(Value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _anglePair = CalculateAngle(DashboardGapPosition, GapDegree);
        base.OnApplyTemplate(e);
    }

    private double CalculateAngle(double value)
    {
        return (360 - GapDegree) * CalculateProgressRatio(value);
    }

    private protected override Geometry BuildRailGeometry(Rect grooveRect)
    {
        if (StepCount <= 0 || StepGap <= 0)
        {
            return CommonShapeBuilder.BuildArc(grooveRect, _anglePair.Item1, _anglePair.Item2);
        }

        return BuildStepGeometry(grooveRect, StepCount);
    }

    private protected override Geometry BuildTrackGeometry(Rect grooveRect)
    {
        if (StepCount <= 0 || StepGap <= 0)
        {
            return CommonShapeBuilder.BuildArc(grooveRect, _anglePair.Item1, IndicatorAngle);
        }

        var filledSteps = (int)Math.Round(StepCount * Percentage / 100);
        return BuildStepGeometry(grooveRect, filledSteps);
    }

    private protected override Geometry BuildSuccessGeometry(Rect grooveRect)
    {
        var threshold = Math.Clamp(SuccessThreshold, Minimum, Maximum);
        if (StepCount <= 0 || StepGap <= 0)
        {
            return CommonShapeBuilder.BuildArc(grooveRect, _anglePair.Item1, CalculateAngle(threshold));
        }

        var filledSteps = (int)Math.Round(StepCount * Percentage / 100);
        var successSteps = (int)Math.Round(StepCount * CalculateProgressRatio(threshold));
        return BuildStepGeometry(grooveRect, Math.Min(filledSteps, successSteps));
    }

    private Geometry BuildStepGeometry(Rect grooveRect, int visibleSteps)
    {
        var geometry = new GeometryGroup();
        var count = Math.Clamp(visibleSteps, 0, StepCount);
        var spanAngle = (360 - GapDegree - StepGap * StepCount) / StepCount;
        var startAngle = _anglePair.Item1;
        for (var index = 0; index < count; index++)
        {
            geometry.Children.Add(CommonShapeBuilder.BuildArc(grooveRect, startAngle, spanAngle));
            startAngle += StepGap + spanAngle;
        }

        return geometry;
    }
}
