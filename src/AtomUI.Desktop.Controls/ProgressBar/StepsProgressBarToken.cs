using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens of the <see cref="StepsProgressBar" />: the gap between two steps and the minimum width of a step.
/// </summary>
[ControlDesignToken]
public sealed partial class StepsProgressBarToken : AbstractLineProgressToken
{
    /// <summary>
    /// Gap between two adjacent steps of a steps progress bar (antd Progress progressStepMarginInlineEnd).
    /// </summary>
    public partial double ProgressStepMarginInlineEnd { get; set; }

    /// <summary>
    /// Minimum width of a step (antd Progress progressStepMinWidth).
    /// </summary>
    public partial double ProgressStepMinWidth { get; set; }
}
