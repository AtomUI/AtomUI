namespace AtomUI.Controls;

/// <summary>
/// Pseudo-class names that the progress bar controls set to expose their state to style selectors.
/// </summary>
public static class ProgressBarPseudoClass
{
    /// <summary>
    /// Set while the progress bar is indeterminate.
    /// </summary>
    public const string Indeterminate = ":indeterminate";
    /// <summary>
    /// Set while <c>Value</c> is close to <c>Maximum</c>.
    /// </summary>
    public const string Completed = ":completed";
    /// <summary>
    /// Set while <c>Value</c> is close to 0. The minimum of the range does not matter.
    /// </summary>
    public const string ValueZero = ":value-zero";
    /// <summary>
    /// Set while the progress info is inside the bar (<see cref="PercentPosition.IsInner" />).
    /// </summary>
    public const string PercentLabelInner = ":labelinner";
    /// <summary>
    /// Set while the progress info is inside the bar and aligned with its start.
    /// </summary>
    public const string PercentLabelInnerStart = ":labelinner-start";
    /// <summary>
    /// Set while the progress info is inside the bar and aligned with its center.
    /// </summary>
    public const string PercentLabelInnerCenter = ":labelinner-center";
    /// <summary>
    /// Set while the progress info is inside the bar and aligned with its end.
    /// </summary>
    public const string PercentLabelInnerEnd = ":labelinner-end";
    /// <summary>
    /// Set while <c>Orientation</c> is <c>Orientation.Vertical</c>.
    /// </summary>
    public const string Vertical = ":vertical";
    /// <summary>
    /// Set while <c>Orientation</c> is <c>Orientation.Horizontal</c>.
    /// </summary>
    public const string Horizontal = ":horizontal";
}