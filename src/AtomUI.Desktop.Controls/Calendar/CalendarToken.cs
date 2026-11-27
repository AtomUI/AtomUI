using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The design tokens of the <see cref="Calendar" />: the sizes of its cells and content area in the compact
/// layout and in the full layout.
/// </summary>
[ControlDesignToken]
public sealed partial class CalendarToken : AbstractCalendarToken
{
    /// <summary>
    /// Width of the inner month cell in the compact calendar's year view.
    /// </summary>
    public partial double YearMonthCellWidth { get; set; }
    /// <summary>
    /// Height of the compact calendar content area.
    /// </summary>
    public partial double MiniContentHeight { get; set; }
    /// <summary>
    /// Minimum desired height of a calendar cell in the content presentation.
    /// </summary>
    public partial double FullCellMinHeight { get; set; }
}
