using AtomUI.Media;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of the circle progress control with a closed ring. The ring starts at the top and runs
/// clockwise; in step mode it is divided into <see cref="AbstractCircleProgress.StepCount" /> arcs separated by
/// <see cref="AbstractCircleProgress.StepGap" /> degrees each.
/// </summary>
public abstract class AbstractGeneralCircleProgress : AbstractCircleProgress
{
    private Rect _currentGrooveRect;
    private IPen? _groovePen;
    private IPen? _indicatorPen;
    private IPen? _successPen;

    /// <summary>
    /// Draws the rail with <see cref="AbstractProgressBar.GrooveBrush" /> and flat line ends, and stores the groove
    /// rectangle that <see cref="RenderIndicatorBar" /> uses for the indicator.
    /// </summary>
    protected override void RenderGroove(DrawingContext context)
    {
        var controlRect = new Rect(new Point(0, 0), Bounds.Size);

        _currentGrooveRect = GetProgressBarRect(controlRect).Deflate(EffectiveStrokeThickness / 2);
        _currentGrooveRect = new Rect(_currentGrooveRect.Position, new Size(Math.Floor(_currentGrooveRect.Size.Width),
            Math.Floor(_currentGrooveRect.Size.Height)));
        // The rail and the success arc come from the shared geometry cache, so progress changes and angle
        // transitions do not rebuild them
        PenUtils.TryModifyOrCreate(ref _groovePen,
            GrooveBrush,
            EffectiveStrokeThickness,
            lineCap: PenLineCap.Flat);
        context.DrawGeometry(null, _groovePen, GetRailGeometry(_currentGrooveRect));
    }

    /// <summary>
    /// Draws the filled arc with <see cref="AbstractProgressBar.StrokeBrush" /> and, when <see
    /// cref="AbstractProgressBar.SuccessThreshold" /> is not <see langword="NaN" />, the success arc with <see
    /// cref="AbstractProgressBar.SuccessStrokeBrush" />. Both use the effective stroke thickness, with flat line ends
    /// in step mode and <see cref="AbstractProgressBar.StrokeLineCap" /> otherwise.
    /// </summary>
    protected override void RenderIndicatorBar(DrawingContext context)
    {
        var lineCap = IsStepMode ? PenLineCap.Flat : StrokeLineCap;
        PenUtils.TryModifyOrCreate(ref _indicatorPen,
            StrokeBrush,
            EffectiveStrokeThickness,
            lineCap: lineCap);
        context.DrawGeometry(null, _indicatorPen, GetTrackGeometry(_currentGrooveRect));

        if (!double.IsNaN(SuccessThreshold))
        {
            PenUtils.TryModifyOrCreate(ref _successPen,
                SuccessStrokeBrush,
                EffectiveStrokeThickness,
                lineCap: lineCap);
            context.DrawGeometry(null, _successPen, GetSuccessGeometry(_currentGrooveRect));
        }
    }

    /// <summary>
    /// Calls the base implementation, then sets <see cref="AbstractCircleProgress.IndicatorAngle" /> from the current
    /// value, so that the indicator fills the ring clockwise from the top.
    /// </summary>
    protected override void NotifyUpdateProgress()
    {
        base.NotifyUpdateProgress();
        IndicatorAngle = CalculateAngle(Value);
    }

    private double CalculateAngle(double value)
    {
        return 360 * CalculateProgressRatio(value);
    }

    /// <summary>
    /// Builds the rail as a full circle or, in step mode, as all
    /// <see cref="AbstractCircleProgress.StepCount" /> steps of equal length that start at the top of the ring.
    /// </summary>
    /// <remarks>
    /// A step spans <c>(360 - StepGap * StepCount) / StepCount</c> degrees and is followed by a gap of
    /// <see cref="AbstractCircleProgress.StepGap" /> degrees.
    /// </remarks>
    private protected override Geometry BuildRailGeometry(Rect grooveRect)
    {
        if (StepCount <= 0 || StepGap <= 0)
        {
            return new EllipseGeometry(grooveRect);
        }

        return BuildStepGeometry(grooveRect, StepCount, -90, GetStepSpanAngle(), StepCount);
    }

    /// <summary>
    /// Builds the indicator as the arc that starts at the top of the ring and spans
    /// <see cref="AbstractCircleProgress.IndicatorAngle" /> degrees clockwise. In step mode it covers the
    /// filled steps only, rounded to the nearest step.
    /// </summary>
    private protected override Geometry BuildTrackGeometry(Rect grooveRect)
    {
        if (StepCount <= 0 || StepGap <= 0)
        {
            return CommonShapeBuilder.BuildArc(grooveRect, -90, IndicatorAngle);
        }

        var filledSteps = (int)Math.Round(StepCount * Percentage / 100);
        return BuildStepGeometry(grooveRect, StepCount, -90, GetStepSpanAngle(), filledSteps);
    }

    /// <summary>
    /// Builds the success arc from the top of the ring up to the angle of
    /// <see cref="AbstractProgressBar.SuccessThreshold" />, clamped to the value range. In step mode it
    /// covers the smaller of the filled step count and the number of steps below the threshold.
    /// </summary>
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
