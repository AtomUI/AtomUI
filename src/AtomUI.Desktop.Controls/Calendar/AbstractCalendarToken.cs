using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Calendar 控件的 Design Token，承载产品视觉语义。
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractCalendarToken : AbstractControlDesignToken
{

    /// <summary>Background behind the full-size calendar grid.</summary>
    public partial Color FullBg { get; set; }

    /// <summary>
    /// 完整 Calendar Panel 背景，派生自容器背景。
    /// </summary>
    public partial Color FullPanelBg { get; set; }

    /// <summary>
    /// 完整模式选中日期/月单元背景，派生自 active item 背景。
    /// </summary>
    public partial Color ItemActiveBg { get; set; }

    /// <summary>
    /// The text colour of the selected date / month cell in fullscreen mode, drawn on <see cref="ItemActiveBg"/>.
    /// Defaults to the primary colour for Calendar. LunarCalendar uses this colour for its fullscreen month cells
    /// and for both lines of fullscreen date-cell text in dark appearance.
    /// </summary>
    public partial Color ItemActiveColor { get; set; }

    /// <summary>
    /// Year Select 最小宽度。
    /// </summary>
    public partial double YearControlWidth { get; set; }

    /// <summary>
    /// Month Select 最小宽度。
    /// </summary>
    public partial double MonthControlWidth { get; set; }

    /// <summary>Height of the weekday header row in the full-size calendar.</summary>


    public partial double WeekHeaderHeight { get; set; }
    /// <summary>Inset of the weekday labels in the full-size header.</summary>
    public partial Thickness WeekHeaderPadding { get; set; }

    /// <summary>Outer spacing between day or month cells in the full-size grid.</summary>

    public partial Thickness FullCellMargin { get; set; }

    /// <summary>Inset of the value and content of a full-size day or month cell.</summary>
    public partial Thickness FullCellPadding { get; set; }

    /// <summary>Thickness of the separator above a day or month cell in the full-size grid.</summary>
    public partial Thickness FullCellBorderThickness { get; set; }

    /// <summary>Height of a range bar inside a full-size date cell.</summary>
    public partial double RangeBarHeight { get; set; }

    /// <summary>Vertical gap between range bars stacked in the same date cell.</summary>
    public partial double RangeBarGap { get; set; }

    /// <summary>Horizontal inset of the label inside a range bar.</summary>
    public partial Thickness RangeBarLabelPadding { get; set; }
}
