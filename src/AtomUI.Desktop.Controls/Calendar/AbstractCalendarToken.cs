using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design Tokens that carry the product visual semantics of the Calendar control.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractCalendarToken : AbstractControlDesignToken
{

    /// <summary>
    /// Background behind the full-size calendar grid.
    /// </summary>
    public partial Color FullBg { get; set; }

    /// <summary>
    /// Background of the date panel in the full-size calendar. Derived from the container background.
    /// </summary>
    public partial Color FullPanelBg { get; set; }

    /// <summary>
    /// Background of a selected date or month cell in the full-size calendar. Derived from the active item background.
    /// </summary>
    public partial Color ItemActiveBg { get; set; }

    /// <summary>
    /// The text color of the selected date / month cell in fullscreen mode, drawn on <see cref="ItemActiveBg" />.
    /// Defaults to the primary color for Calendar. LunarCalendar uses this color for its fullscreen month cells
    /// and for both lines of fullscreen date-cell text in dark appearance.
    /// </summary>
    public partial Color ItemActiveColor { get; set; }

    /// <summary>
    /// Minimum width of the year Select in the calendar header.
    /// </summary>
    public partial double YearControlWidth { get; set; }

    /// <summary>
    /// Minimum width of the month Select in the calendar header.
    /// </summary>
    public partial double MonthControlWidth { get; set; }

    /// <summary>
    /// Height of the weekday header row in the full-size calendar.
    /// </summary>


    public partial double WeekHeaderHeight { get; set; }
    /// <summary>
    /// Inset of the weekday labels in the full-size header.
    /// </summary>
    public partial Thickness WeekHeaderPadding { get; set; }

    /// <summary>
    /// Outer spacing between day or month cells in the full-size grid.
    /// </summary>

    public partial Thickness FullCellMargin { get; set; }

    /// <summary>
    /// Inset of the value and content of a full-size day or month cell.
    /// </summary>
    public partial Thickness FullCellPadding { get; set; }

    /// <summary>
    /// Thickness of the separator above a day or month cell in the full-size grid.
    /// </summary>
    public partial Thickness FullCellBorderThickness { get; set; }

    /// <summary>
    /// Height of a range bar inside a full-size date cell.
    /// </summary>
    public partial double RangeBarHeight { get; set; }

    /// <summary>
    /// Vertical gap between range bars stacked in the same date cell.
    /// </summary>
    public partial double RangeBarGap { get; set; }

    /// <summary>
    /// Horizontal inset of the label inside a range bar.
    /// </summary>
    public partial Thickness RangeBarLabelPadding { get; set; }
}
