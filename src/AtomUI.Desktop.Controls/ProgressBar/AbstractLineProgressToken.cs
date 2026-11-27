using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens of the line-shaped progress controls (<see cref="ProgressBar" /> and <see cref="StepsProgressBar" />):
/// the percentage / status-icon area next to the bar.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractLineProgressToken : AbstractProgressToken
{
    /// <summary>
    /// Gap between the progress bar and the percentage / status icon.
    /// </summary>
    public partial double LineExtraInfoMargin { get; set; }

    /// <summary>
    /// Size of the status icon of the large progress bar. The small and middle progress bars use
    /// <see cref="LineInfoIconSizeSM" />.
    /// </summary>
    public partial double LineInfoIconSize { get; set; }

    /// <summary>
    /// Size of the status icon of small and middle progress bars.
    /// </summary>
    public partial double LineInfoIconSizeSM { get; set; }
}
