using System.Globalization;

namespace AtomUI.Desktop.Controls.Internal.DateViewer;

internal enum DateRangeActivePart { Start, End }
internal enum DateWeekNumbering { Culture, Iso }
internal enum DatePanelConstraintMode { Unit, Picker, Calendar }

internal readonly record struct DateUnitRange(DateTime Start, DateTime End)
{
    public bool Contains(DateTime value) => value.Date >= Start && value.Date <= End;
}

/// <summary>Effective rendering inputs. This is a projection, not a writable selection owner.</summary>
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

internal sealed record DatePanelModel(
    DateTime Anchor,
    DateViewerPanelKind Kind,
    int Rows,
    int Columns,
    IReadOnlyList<DateViewerCellModel> Cells)
{
    public IReadOnlyList<string> ColumnHeaders { get; init; } = Array.Empty<string>();
}
