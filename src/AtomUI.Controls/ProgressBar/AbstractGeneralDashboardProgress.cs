using AtomUI.Media;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of the dashboard progress control: an arc that leaves out a gap of <see cref="GapDegree" />
/// degrees centered on the side selected by <see cref="DashboardGapPosition" />. The indicator starts at the
/// clockwise end of the gap; in step mode the arc is divided into <see cref="AbstractCircleProgress.StepCount" /> steps.
/// </summary>
public abstract class AbstractGeneralDashboardProgress : AbstractCircleProgress
{
    /// <summary>
    /// Width of the gap, in degrees, used when <see cref="GapDegree" /> is not set.
    /// </summary>
    public const double DEFAULT_GAP_DEGREE = 75;
    /// <summary>
    /// Smallest gap width, in degrees, that <see cref="GapDegree" /> accepts; smaller values are coerced to it.
    /// </summary>
    public const double MIN_GAP_DEGREE = 0;
    /// <summary>
    /// Largest gap width, in degrees, that <see cref="GapDegree" /> accepts; larger values are coerced to it.
    /// </summary>
    public const double MAX_GAP_DEGREE = 295;

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="DashboardGapPosition" /> property.
    /// </summary>
    public static readonly StyledProperty<DashboardGapPosition> DashboardGapPositionProperty =
        AvaloniaProperty.Register<AbstractGeneralDashboardProgress, DashboardGapPosition>(
            nameof(DashboardGapPosition), DashboardGapPosition.Bottom);

    /// <summary>
    /// Identifies the <see cref="GapDegree" /> property.
    /// </summary>
    public static readonly StyledProperty<double> GapDegreeProperty =
        AvaloniaProperty.Register<AbstractGeneralDashboardProgress, double>(
            nameof(GapDegree), DEFAULT_GAP_DEGREE,
            coerce: (o, value) => Math.Clamp(value, MIN_GAP_DEGREE, MAX_GAP_DEGREE));

    /// <summary>
    /// Side of the circle the gap is centered on. The indicator starts at the clockwise end of the gap. The
    /// default is Bottom.
    /// </summary>
    public DashboardGapPosition DashboardGapPosition
    {
        get => GetValue(DashboardGapPositionProperty);
        set => SetValue(DashboardGapPositionProperty, value);
    }

    /// <summary>
    /// Width of the gap in degrees. The arc spans 360 degrees minus this value, and values are coerced to the
    /// range from <see cref="MIN_GAP_DEGREE" /> to <see cref="MAX_GAP_DEGREE" />. The default is
    /// <see cref="DEFAULT_GAP_DEGREE" />.
    /// </summary>
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

    /// <summary>
    /// Draws the rail arc with <see cref="AbstractProgressBar.GrooveBrush" />, and stores the groove rectangle that
    /// <see cref="RenderIndicatorBar" /> uses for the indicator. The line ends are flat in step mode and follow
    /// <see cref="AbstractProgressBar.StrokeLineCap" /> otherwise.
    /// </summary>
    protected override void RenderGroove(DrawingContext context)
    {
        var controlRect = new Rect(new Point(0, 0), Bounds.Size);
        _currentGrooveRect = GetProgressBarRect(controlRect).Deflate(EffectiveStrokeThickness / 2);
        _currentGrooveRect = new Rect(_currentGrooveRect.Position, new Size(Math.Floor(_currentGrooveRect.Size.Width),
            Math.Floor(_currentGrooveRect.Size.Height)));
        // The rail and the success arc come from the shared geometry cache, so progress changes and angle
        // transitions do not rebuild them
        var lineCap = IsStepMode ? PenLineCap.Flat : StrokeLineCap;
        PenUtils.TryModifyOrCreate(ref _groovePen,
            GrooveBrush,
            EffectiveStrokeThickness,
            lineCap: lineCap);
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

    /// <summary>
    /// Calls the base implementation, then recalculates the start and span angles when
    /// <see cref="DashboardGapPosition" /> or <see cref="GapDegree" /> changes, drops the cached geometries and
    /// refreshes the template path data.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == GapDegreeProperty || change.Property == DashboardGapPositionProperty)
        {
            _anglePair = CalculateAngle(DashboardGapPosition, GapDegree);
            InvalidateGeometryCache();
            RefreshTemplateProgressVisuals();
        }
    }

    /// <summary>
    /// Calls the base implementation, then sets <see cref="AbstractCircleProgress.IndicatorAngle" /> from the current
    /// value, so that the indicator fills the arc left by the gap clockwise from its start.
    /// </summary>
    protected override void NotifyUpdateProgress()
    {
        base.NotifyUpdateProgress();
        IndicatorAngle = CalculateAngle(Value);
    }

    /// <summary>
    /// Recalculates the start and span angles before calling the base implementation, which builds the template
    /// path data from them.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _anglePair = CalculateAngle(DashboardGapPosition, GapDegree);
        base.OnApplyTemplate(e);
    }

    private double CalculateAngle(double value)
    {
        return (360 - GapDegree) * CalculateProgressRatio(value);
    }

    /// <summary>
    /// Builds the rail as the arc between the two ends of the gap, which leaves out <see cref="GapDegree" />
    /// degrees, or, in step mode, as all <see cref="AbstractCircleProgress.StepCount" /> steps of that arc.
    /// </summary>
    private protected override Geometry BuildRailGeometry(Rect grooveRect)
    {
        if (StepCount <= 0 || StepGap <= 0)
        {
            return CommonShapeBuilder.BuildArc(grooveRect, _anglePair.Item1, _anglePair.Item2);
        }

        return BuildStepGeometry(grooveRect, StepCount);
    }

    /// <summary>
    /// Builds the indicator as the arc that starts at the clockwise end of the gap and spans
    /// <see cref="AbstractCircleProgress.IndicatorAngle" /> degrees. In step mode it covers the filled steps
    /// only, rounded to the nearest step.
    /// </summary>
    private protected override Geometry BuildTrackGeometry(Rect grooveRect)
    {
        if (StepCount <= 0 || StepGap <= 0)
        {
            return CommonShapeBuilder.BuildArc(grooveRect, _anglePair.Item1, IndicatorAngle);
        }

        var filledSteps = (int)Math.Round(StepCount * Percentage / 100);
        return BuildStepGeometry(grooveRect, filledSteps);
    }

    /// <summary>
    /// Builds the success arc from the start of the indicator up to the angle of
    /// <see cref="AbstractProgressBar.SuccessThreshold" />, clamped to the value range. In step mode it
    /// covers the smaller of the filled step count and the number of steps below the threshold.
    /// </summary>
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
