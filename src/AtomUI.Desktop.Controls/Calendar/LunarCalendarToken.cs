using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class LunarCalendarToken : AbstractCalendarToken
{
    /// <summary>Height of the calendar content area in compact presentation.</summary>
    public partial double MiniContentHeight { get; set; }
    /// <summary>Width of the inner date cell and height of the month cell in compact presentation.</summary>
    public partial double MiniDateCellSize { get; set; }
    /// <summary>Height of the date cell in compact presentation.</summary>
    public partial double MiniDateCellHeight { get; set; }
    /// <summary>Insets around the date cell content in compact presentation.</summary>
    public partial Thickness MiniDateCellPadding { get; set; }
    /// <summary>Corner radius of the date cell's fill and outline in compact presentation.</summary>
    public partial CornerRadius MiniDateCellCornerRadius { get; set; }
    /// <summary>Width of the inner month cell in compact presentation.</summary>
    public partial double MiniMonthCellWidth { get; set; }
    /// <summary>Font size of the secondary text in a date cell.</summary>
    public partial double SecondaryTextFontSize { get; set; }
    /// <summary>Line height of the secondary text in a date cell.</summary>
    public partial double SecondaryTextLineHeight { get; set; }
    /// <summary>Foreground color of the secondary text in a date cell.</summary>
    public partial Color SecondaryTextColor { get; set; }
    /// <summary>Foreground color of the primary date text for an enabled, unselected weekend in the displayed month.</summary>
    public partial Color WeekendTextColor { get; set; }
    /// <summary>Fill color of the holiday marker dot.</summary>
    public partial Color HolidayMarkerColor { get; set; }
    /// <summary>Fill color of the make-up workday marker dot.</summary>
    public partial Color WorkdayMarkerColor { get; set; }

    /// <summary>Diameter of the holiday and make-up workday marker dots.</summary>
    public partial double MarkerSize { get; set; }

    /// <summary>Margin around the marker dot next to the secondary text.</summary>
    public partial Thickness MarkerMargin { get; set; }

    /// <summary>Corner radius of the marker dot. A radius of half <see cref="MarkerSize"/> makes the dot circular.</summary>
    public partial CornerRadius MarkerCornerRadius { get; set; }
    /// <summary>Minimum date cell height in full-size presentation.</summary>
    public partial double FullCellMinHeight { get; set; }
    /// <summary>Vertical offset of a range bar from the top of its date row.</summary>
    public partial double RangeBarTopOffset { get; set; }
}
