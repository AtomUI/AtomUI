using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design token shared by <see cref="StatisticToken" /> and <see cref="TimerStatisticToken" />. It adds the
/// title font size to the value settings of <see cref="AbstractStatisticValueToken" />.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractStatisticToken : AbstractStatisticValueToken
{

    /// <summary>
    /// Font size of the statistic title, the header above the value.
    /// </summary>
    public partial double TitleFontSize { get; set; }

}
