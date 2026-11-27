using System.Globalization;
using AtomUI.Controls;

namespace AtomUI.Desktop.Controls.Internal.DateViewer;

/// <summary>
/// The end of a date range that is currently active: <c>Start</c> or <c>End</c>. Used by the date viewer and by
/// the date picker presenters and edit session of AtomUI.Desktop.Controls.
/// </summary>
internal enum DateRangeActivePart { Start, End }
internal enum DateWeekNumbering { Culture, Iso }
internal enum DatePanelConstraintMode { Unit, Picker, Calendar }

/// <summary>
/// Effective rendering inputs. This is a projection, not a writable selection owner.
/// </summary>
internal sealed record DatePanelInput
{
    public DateTime DisplayDate { get; init; } = DateTime.Today;
    public DateTime Today { get; init; } = DateTime.Today;
    public DateViewerPanelKind PanelKind { get; init; }
    public DateViewerSelectionUnit SelectionUnit { get; init; }
    public CultureInfo Culture { get; init; } = CultureInfo.CurrentCulture;
    public DayOfWeek? FirstDayOfWeek { get; init; }
    public bool ShowWeek { get; init; }
    public bool AbbreviatedWeekdays { get; init; }
    public string? DateNumberFormat { get; init; }
    public int PanelCount { get; init; } = 1;
    public bool IsRangeSelection { get; init; }
    public bool IsSelectionRequired { get; init; }
    public bool AllowWeekActivation { get; init; }
    public DateTime? SelectedDate { get; init; }
    public DateTime? HoveredValue { get; init; }
    public DateTime? FocusedValue { get; init; }
    public DateTime? MinDate { get; init; }
    public DateTime? MaxDate { get; init; }
    public DateViewerRange? Range { get; init; }
    public DateRangeActivePart ActiveRangePart { get; init; }
    public DateWeekNumbering WeekNumbering { get; init; }
    public DatePanelConstraintMode ConstraintMode { get; init; }
    public Func<DateTime, bool>? DisabledDate { get; init; }
}

/// <summary>
/// The display state of one cell of a date panel grid: its value, cell kind and period, the text to show, its
/// row and column, and its view, today, selection, hover and range flags. It is a projection built by the date
/// panel algorithms for <c>DatePanelSession</c>, which hands it to its content factory and to the calendar
/// presentation adapters of AtomUI.Desktop.Controls; it does not own the selection.
/// </summary>
internal sealed record DateViewerCellModel(
    DateTime? Value,
    DateViewerCellType Kind,
    DateUnitRange? Period,
    string DisplayText,
    int Row,
    int Column,
    bool IsInView,
    bool IsToday,
    bool IsDisabled,
    bool IsSelected,
    bool IsHovered)
{
    public bool IsFocusable => Value.HasValue && !IsDisabled && Kind != DateViewerCellType.Week;
    public bool IsFocused { get; init; }
    public bool IsVisualEndpoint { get; init; }
    public bool IsNavigationCurrent { get; init; }
    public bool IsRangeStart { get; init; }
    public bool IsRangeEnd { get; init; }
    public bool IsRangeMiddle { get; init; }
    public bool HasRangePreview { get; init; }
    public bool IsRangePreviewStart { get; init; }
    public bool IsRangePreviewEnd { get; init; }
    public bool IsRangePreviewMiddle { get; init; }
    public bool IsWeekSelectionStart { get; init; }
    public bool IsWeekSelectionMiddle { get; init; }
    public bool IsWeekSelectionEnd { get; init; }
}

/// <summary>
/// One panel of a date viewer as built for rendering: the anchor date, the panel kind, the number of rows and
/// columns and the cell models, with optional column headers. <c>DatePanelSession</c> exposes the panels of
/// the current layout as these models for the date viewer and calendar code of AtomUI.Desktop.Controls.
/// </summary>
internal sealed record DatePanelModel(
    DateTime Anchor,
    DateViewerPanelKind Kind,
    int Rows,
    int Columns,
    IReadOnlyList<DateViewerCellModel> Cells)
{
    public IReadOnlyList<string> ColumnHeaders { get; init; } = Array.Empty<string>();
}
