using AtomUI.Media;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

public abstract class AbstractGeneralCircleProgress : AbstractCircleProgress
{
    private Rect _currentGrooveRect;
    private IPen? _groovePen;
    private IPen? _indicatorPen;
    private IPen? _successPen;

    protected override void RenderGroove(DrawingContext context)
    {
        var controlRect = new Rect(new Point(0, 0), Bounds.Size);

        _currentGrooveRect = GetProgressBarRect(controlRect).Deflate(StrokeThickness / 2);
        _currentGrooveRect = new Rect(_currentGrooveRect.Position, new Size(Math.Floor(_currentGrooveRect.Size.Width),
            Math.Floor(_currentGrooveRect.Size.Height)));
        // 轨道与成功弧取自共享缓存，进度与角度过渡期间不重新构建
        PenUtils.TryModifyOrCreate(ref _groovePen,
            GrooveBrush,
            StrokeThickness,
            lineCap: PenLineCap.Flat);
        context.DrawGeometry(null, _groovePen, GetRailGeometry(_currentGrooveRect));
    }

    protected override void RenderIndicatorBar(DrawingContext context)
    {
        var lineCap = IsStepMode ? PenLineCap.Flat : StrokeLineCap;
        PenUtils.TryModifyOrCreate(ref _indicatorPen,
            StrokeBrush,
            StrokeThickness,
            lineCap: lineCap);
        context.DrawGeometry(null, _indicatorPen, GetTrackGeometry(_currentGrooveRect));

        if (!double.IsNaN(SuccessThreshold))
        {
            PenUtils.TryModifyOrCreate(ref _successPen,
                SuccessStrokeBrush,
                StrokeThickness,
                lineCap: lineCap);
            context.DrawGeometry(null, _successPen, GetSuccessGeometry(_currentGrooveRect));
        }
    }

    protected override void NotifyUpdateProgress()
    {
        base.NotifyUpdateProgress();
        IndicatorAngle = CalculateAngle(Value);
    }

    private double CalculateAngle(double value)
    {
        return 360 * CalculateProgressRatio(value);
    }

    private protected override Geometry BuildRailGeometry(Rect grooveRect)
    {
        if (StepCount <= 0 || StepGap <= 0)
        {
            return new EllipseGeometry(grooveRect);
        }

        return BuildStepGeometry(grooveRect, StepCount, -90, GetStepSpanAngle(), StepCount);
    }

    private protected override Geometry BuildTrackGeometry(Rect grooveRect)
    {
        if (StepCount <= 0 || StepGap <= 0)
        {
            return CommonShapeBuilder.BuildArc(grooveRect, -90, IndicatorAngle);
        }

        var filledSteps = (int)Math.Round(StepCount * Percentage / 100);
        return BuildStepGeometry(grooveRect, StepCount, -90, GetStepSpanAngle(), filledSteps);
    }

    private protected override Geometry BuildSuccessGeometry(Rect grooveRect)
    {
        var threshold = Math.Clamp(SuccessThreshold, Minimum, Maximum);
        if (StepCount <= 0 || StepGap <= 0)
        {
            return CommonShapeBuilder.BuildArc(grooveRect, -90, CalculateAngle(threshold));
        }

        var filledSteps = (int)Math.Round(StepCount * Percentage / 100);
        var successSteps = (int)Math.Round(StepCount * CalculateProgressRatio(threshold));
        return BuildStepGeometry(
            grooveRect,
            StepCount,
            -90,
            GetStepSpanAngle(),
            Math.Min(filledSteps, successSteps));
    }

    private double GetStepSpanAngle()
    {
        return (360 - StepGap * StepCount) / StepCount;
    }

    private Geometry BuildStepGeometry(
        Rect grooveRect,
        int stepCount,
        double startAngle,
        double spanAngle,
        int visibleSteps)
    {
        var geometry = new GeometryGroup();
        var count = Math.Clamp(visibleSteps, 0, stepCount);
        for (var index = 0; index < count; index++)
        {
            geometry.Children.Add(CommonShapeBuilder.BuildArc(grooveRect, startAngle, spanAngle));
            startAngle += StepGap + spanAngle;
        }

        return geometry;
    }
}
