using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class StepsProgressBarToken : AbstractLineProgressToken
{
    /// <summary>
    /// 步骤之间的间距
    /// Gap between two steps (antd Progress progressStepMarginInlineEnd)
    /// </summary>
    public partial double ProgressStepMarginInlineEnd { get; set; }

    /// <summary>
    /// 步骤最小宽度
    /// Minimum width of a step (antd Progress progressStepMinWidth)
    /// </summary>
    public partial double ProgressStepMinWidth { get; set; }
}
