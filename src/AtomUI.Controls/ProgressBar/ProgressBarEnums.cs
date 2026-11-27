namespace AtomUI.Controls;

/// <summary>
/// Status a progress bar presents. The theme derives the indicator color and the status icon from it.
/// </summary>
public enum ProgressStatus
{
    /// <summary>
    /// The theme keeps the indicator color of the control.
    /// </summary>
    Normal,
    /// <summary>
    /// The theme paints the indicator with the success color.
    /// </summary>
    Success,
    /// <summary>
    /// The theme paints the indicator with the error color and shows the exception icon in place of the
    /// percentage text.
    /// </summary>
    Exception,
    /// <summary>
    /// Renders like <see cref="Normal" />. The shimmer animation over the track is not implemented.
    /// </summary>
    Active
}

/// <summary>
/// Side of the circle of a dashboard progress bar that the gap is centered on.
/// </summary>
public enum DashboardGapPosition
{
    /// <summary>
    /// The gap is centered on the left side of the circle.
    /// </summary>
    Left,
    /// <summary>
    /// The gap is centered on the top of the circle.
    /// </summary>
    Top,
    /// <summary>
    /// The gap is centered on the right side of the circle.
    /// </summary>
    Right,
    /// <summary>
    /// The gap is centered on the bottom of the circle. This is the default.
    /// </summary>
    Bottom
}

/// <summary>
/// Placement of the progress info of a line progress bar: inside or outside the bar, and at which alignment.
/// </summary>
public record struct PercentPosition
{
    public PercentPosition() {}
    /// <summary>
    /// <see langword="true" /> draws the progress info over the filled part of the bar and thickens the bar so
    /// the info fits. <see langword="false" /> places the info outside the bar, where it adds to the size of
    /// the control. Applies only while progress info is presented.
    /// </summary>
    public bool IsInner { get; set; } = false;
    /// <summary>
    /// Place of the progress info. Outside the bar, <see cref="LinePercentAlignment.Start" /> and
    /// <see cref="LinePercentAlignment.End" /> put it before and after the bar, and
    /// <see cref="LinePercentAlignment.Center" /> below a horizontal bar or beside a vertical one. Inside the bar
    /// it is drawn over the start, the center or the end of the filled part. Applies only while progress info is
    /// presented. The default is <see cref="LinePercentAlignment.End" />.
    /// </summary>
    public LinePercentAlignment Alignment { get; set; } = LinePercentAlignment.End;
}

/// <summary>
/// Place of the progress info relative to the bar of a line progress bar or of a step progress bar.
/// </summary>
public enum LinePercentAlignment
{
    /// <summary>
    /// Before the bar. Inside the bar, over the start of the filled part.
    /// </summary>
    Start,
    /// <summary>
    /// Below a horizontal bar or beside a vertical one. Inside the bar, over the center of the filled part.
    /// </summary>
    Center,
    /// <summary>
    /// After the bar. Inside the bar, over the end of the filled part. This is the default.
    /// </summary>
    End
}