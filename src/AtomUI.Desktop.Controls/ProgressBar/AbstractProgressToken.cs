using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens shared by every progress control (antd Progress <c>defaultColor</c> / <c>remainingColor</c>). Each
/// progress control has its own terminal Token, so <c>Controls[CircleProgress]</c> and <c>Controls[ProgressBar]</c>
/// compute and override these values independently.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractProgressToken : AbstractControlDesignToken
{
    /// <summary>
    /// Color of the filled part of a progress bar: the track of a line bar and the arc of a circle or dashboard.
    /// </summary>
    public partial Color DefaultColor { get; set; }

    /// <summary>
    /// Color of the unfilled part of a progress bar: the rail behind the track of a line bar and the ring behind the
    /// arc of a circle or dashboard.
    /// </summary>
    public partial Color RemainingColor { get; set; }
}
