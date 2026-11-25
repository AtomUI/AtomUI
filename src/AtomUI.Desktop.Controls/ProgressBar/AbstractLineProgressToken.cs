using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens of the line-shaped progress controls (<see cref="ProgressBar"/> and <see cref="StepsProgressBar"/>): the
/// percentage / status-icon area next to the bar.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractLineProgressToken : AbstractProgressToken
{
    /// <summary>
    /// 额外信息和进度条之间的间距
    /// Gap between the progress bar and the percentage / status icon
    /// </summary>
    public partial double LineExtraInfoMargin { get; set; }

    /// <summary>
    /// 状态图标大小
    /// Size of the status icon
    /// </summary>
    public partial double LineInfoIconSize { get; set; }

    /// <summary>
    /// 小号状态图标大小
    /// Size of the status icon of small and middle progress bars
    /// </summary>
    public partial double LineInfoIconSizeSM { get; set; }
}
