using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design token shared by <see cref="AbstractStatisticToken" /> and <see cref="StatisticCountUpToken" />. It
/// sets the font size of the value a statistic displays.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractStatisticValueToken : AbstractControlDesignToken
{
    /// <summary>
    /// Font size of the displayed statistic value.
    /// </summary>
    public partial double ContentFontSize { get; set; }
}
