using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractDateViewerToken : AbstractControlDesignToken
{
    /// <summary>Background shared by the calendar panel and the range panels.</summary>
    public partial Color PanelBg { get; set; }
    /// <summary>Fill behind the cell value on pointer hover.</summary>
    public partial Color CellHoverBg { get; set; }
    /// <summary>Fill connecting cells inside the selected range.</summary>
    public partial Color CellRangeBg { get; set; }

    /// <summary>The corner radius of a date cell's value frame.</summary>
    public partial CornerRadius CellCornerRadius { get; set; }

    /// <summary>The background of today's date cell (transparent by default: today is outlined).</summary>
    public partial Color CellTodayBg { get; set; }

    /// <summary>The text colour of today's date cell.</summary>
    public partial Color CellTodayColor { get; set; }

    /// <summary>The outline of today's date cell.</summary>
    public partial Color CellTodayBorderColor { get; set; }

    /// <summary>The font weight of the panel header's period title.</summary>
    public partial FontWeight HeaderTitleFontWeight { get; set; }

    /// <summary>The minimum height of the week-day header row; 0 follows <see cref="CellRowHeight"/>.</summary>
    public partial double WeekHeaderHeight { get; set; }

    /// <summary>The font weight of the week-day header names.</summary>
    public partial FontWeight WeekHeaderFontWeight { get; set; }

    /// <summary>
    /// The text colour of today's and the selected cell in the content presentation, drawn on
    /// <c>ControlItemBgActive</c>. Defaults to the primary colour (Ant Design); a theme whose selected background is
    /// the solid primary sets the contrast colour.
    /// </summary>
    public partial Color CellActiveColor { get; set; }
    /// <summary>Width of one date viewer panel.</summary>
    public partial double PanelWidth { get; set; }
    /// <summary>Height of the cell value area.</summary>
    public partial double CellHeight { get; set; }
    /// <summary>Width of the cell value area.</summary>
    public partial double CellWidth { get; set; }
    /// <summary>Minimum row height for date cells and the weekday header.</summary>
    public partial double CellRowHeight { get; set; }
    /// <summary>Width of a value cell in the period selection view.</summary>
    public partial double PeriodCellWidth { get; set; }
    /// <summary>Padding between the edge and value of a period cell.</summary>
    public partial Thickness PeriodCellPadding { get; set; }
    /// <summary>Minimum height of a date cell that displays additional content.</summary>
    public partial double ContentCellMinHeight { get; set; }
    /// <summary>Inset of the painted surface inside the interactive content cell.</summary>
    public partial Thickness ContentCellSurfaceMargin { get; set; }
    /// <summary>Thickness of the separator at the top of a content cell.</summary>
    public partial Thickness ContentCellTopBorderThickness { get; set; }
    /// <summary>Space around the date value inside a content cell.</summary>
    public partial Thickness ContentCellValueMargin { get; set; }
    /// <summary>Space around the additional content below the date value.</summary>
    public partial Thickness ContentCellContentMargin { get; set; }
    /// <summary>Space between the panel edge and its header and body.</summary>
    public partial Thickness PanelPadding { get; set; }
    /// <summary>Horizontal inset of the header navigation and period label.</summary>
    public partial Thickness HeaderHorizontalPadding { get; set; }
    /// <summary>Padding around the weekday row and date grid.</summary>
    public partial Thickness DateBodyPadding { get; set; }

    /// <summary>
    /// Bottom divider of the default panel header (antd picker header borderBottom: lineWidth).
    /// </summary>
    public partial Thickness HeaderBorderThickness { get; set; }

    /// <summary>
    /// Top divider of the standalone footer (antd picker footer borderTop: lineWidth).
    /// </summary>

    /// <summary>
    /// Opacity of the week numbers in week mode (antd dims the selected row's week number to half alpha).
    /// </summary>
    public partial double WeekNumberOpacity { get; set; }
}
