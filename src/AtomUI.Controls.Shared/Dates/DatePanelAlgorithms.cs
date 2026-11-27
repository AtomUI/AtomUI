using System.Globalization;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Internal.DateViewer;

namespace AtomUI.Controls;

/// <summary>
/// Gregorian topology and unit comparisons without control or host dependencies.
/// </summary>
internal static class DatePanelAlgorithms
{
    internal static DateMonthLayout GetMonthLayout(DateOnly anchor, DayOfWeek firstDayOfWeek)
    {
        var first = new DateOnly(anchor.Year, anchor.Month, 1);
        var leading = DateUnitMath.DayOffset(first.DayOfWeek, firstDayOfWeek);
        var days = DateUnitMath.DaysInMonth(first.Year, first.Month);
        return new DateMonthLayout(leading, days, (leading + days + 6) / 7);
    }

    internal static DateCellState GetCellState(in DateCellStateInput input, DateOnly date)
    {
        var weekday = DateUnitMath.DayOffset(date.DayOfWeek, input.FirstDayOfWeek);
        var disabled = date < input.MinDate || date > input.MaxDate || input.DisabledDate?.Invoke(date) == true;
        var selected = !disabled && (date == input.SelectedDate || date == input.RangeStart || date == input.RangeEnd);
        var range = GetRangeFlags(date.DayNumber, input.RangeStart?.DayNumber, input.RangeEnd?.DayNumber);
        return GetCellState(date == input.Today, disabled, selected, range,
            weekday == 0 || date.Day == 1,
            weekday == 6 || date.Day == DateUnitMath.DaysInMonth(date.Year, date.Month));
    }

    private static DateCellState GetCellState(bool isToday, bool isDisabled, bool isSelected,
        DateCellState range, bool isRowStart = false, bool isRowEnd = false)
    {
        var state = range;
        if (isToday)
        {
            state |= DateCellState.Today;
        }
        if (isDisabled)
        {
            state |= DateCellState.Disabled;
        }
        if (isSelected)
        {
            state |= DateCellState.Selected;
        }
        if (isRowStart)
        {
            state |= DateCellState.RowStart;
        }
        if (isRowEnd)
        {
            state |= DateCellState.RowEnd;
        }
        return state;
    }

    private static DateCellState GetRangeFlags(int key, int? start, int? end)
    {
        if (start is null || end is null)
        {
            return DateCellState.None;
        }
        if (start > end)
        {
            (start, end) = (end, start);
        }

        var state = DateCellState.None;
        if (start == key)
        {
            state |= DateCellState.RangeStart;
        }
        if (end == key)
        {
            state |= DateCellState.RangeEnd;
        }
        if (start < key && key < end)
        {
            state |= DateCellState.RangeMiddle;
        }
        return state;
    }

    internal static DayOfWeek GetWeekFirstDay(CultureInfo culture) => DateUnitMath.GetWeekFirstDay(culture);

    internal static (int Year, int Week) GetWeekIdentity(DateTime value, CultureInfo culture, DayOfWeek firstDay) =>
        DateUnitMath.GetWeekIdentity(value, culture, firstDay);

    public static DateTime Normalize(DateTime value, DateViewerSelectionUnit unit, DayOfWeek firstDayOfWeek) =>
        DateUnitMath.Normalize(value, unit, firstDayOfWeek);

    public static DateUnitRange GetUnitRange(DateTime value, DateViewerSelectionUnit unit, DayOfWeek firstDayOfWeek) =>
        DateUnitMath.GetUnitRange(value, unit, firstDayOfWeek);

    public static DateTime GetPanelAnchor(DateTime value, DateViewerPanelKind kind) => kind switch
    {
        DateViewerPanelKind.Date => DateUnitMath.GetMonthStart(value),
        DateViewerPanelKind.Month or DateViewerPanelKind.Quarter => new DateTime(value.Year, 1, 1),
        DateViewerPanelKind.Year => new DateTime(Math.Max(1, value.Year / 10 * 10), 1, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static DateTime Navigate(DateTime value, DateViewerPanelKind kind, int periods)
    {
        var anchor = GetPanelAnchor(value, kind);
        if (kind == DateViewerPanelKind.Date)
        {
            var month = Math.Clamp((long)(anchor.Year - 1) * 12 + anchor.Month - 1 + periods, 0, 9999L * 12 - 1);
            return new DateTime((int)(month / 12) + 1, (int)(month % 12) + 1, 1);
        }

        var baseYear = kind == DateViewerPanelKind.Year ? value.Year / 10 * 10 : anchor.Year;
        var years = kind == DateViewerPanelKind.Year ? periods * 10L : periods;
        var year = (int)Math.Clamp(baseYear + years, 1, 9999);
        return GetPanelAnchor(new DateTime(year, 1, 1), kind);
    }

    public static DatePanelModel Build(DatePanelInput input, int panelIndex)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (panelIndex is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(panelIndex));
        }

        var anchor = Navigate(input.DisplayDate, input.PanelKind, panelIndex);
        var firstDay = input.FirstDayOfWeek ?? GetWeekFirstDay(input.Culture);
        var availability = new Dictionary<DateTime, bool>();
        var bounds = GetBounds(input, firstDay);
        bool IsDateEnabled(DateTime date)
        {
            date = date.Date;
            if (bounds.Empty || bounds.Start is { } start && date < start || bounds.End is { } end && date > end)
            {
                return false;
            }

            if (!availability.TryGetValue(date, out var enabled))
            {
                enabled = !(input.DisabledDate?.Invoke(date) ?? false);
                availability.Add(date, enabled);
            }
            return enabled;
        }

        bool IsPeriodEnabled(DateUnitRange period)
        {
            if (input.ConstraintMode == DatePanelConstraintMode.Calendar)
            {
                return IsDateEnabled(period.Start) || IsDateEnabled(period.End);
            }

            if (bounds.Empty || bounds.Start is { } start && period.End < start ||
                bounds.End is { } end && period.Start > end)
            {
                return false;
            }

            if (input.DisabledDate is null)
            {
                return true;
            }

            var first = bounds.Start is { } minimum && minimum > period.Start ? minimum : period.Start;
            var last = bounds.End is { } maximum && maximum < period.End ? maximum : period.End;
            for (var ticks = first.Ticks; ticks <= last.Ticks; ticks += TimeSpan.TicksPerDay)
            {
                if (IsDateEnabled(new DateTime(ticks)))
                {
                    return true;
                }
            }
            return false;
        }

        return input.PanelKind == DateViewerPanelKind.Date
            ? BuildDatePanel(input, anchor, firstDay, IsDateEnabled, IsPeriodEnabled)
            : BuildPeriodPanel(input, anchor, firstDay, IsDateEnabled, IsPeriodEnabled);
    }

    /// <summary>
    /// Project interaction state onto cached topology and availability.
    /// </summary>
    internal static DatePanelModel ProjectState(DatePanelInput input, DatePanelModel topology)
    {
        var firstDay = input.FirstDayOfWeek ?? GetWeekFirstDay(input.Culture);
        List<DateViewerCellModel>? changed = null;
        for (var index = 0; index < topology.Cells.Count; index++)
        {
            var original = topology.Cells[index];
            var projected = BuildCell(input, original.Value, original.Kind, original.Row, original.Column,
                original.DisplayText, original.IsInView, firstDay, topology.Columns, cachedDisabled: original.IsDisabled);
            if (projected == original)
            {
                continue;
            }

            changed ??= new List<DateViewerCellModel>(topology.Cells);
            changed[index] = projected;
        }
        return changed is null ? topology : topology with { Cells = changed.AsReadOnly() };
    }

    private static DatePanelModel BuildDatePanel(DatePanelInput input, DateTime anchor, DayOfWeek firstDay,
        Func<DateTime, bool> isDateEnabled, Func<DateUnitRange, bool> isPeriodEnabled)
    {
        var showWeek = input.ShowWeek || input.SelectionUnit == DateViewerSelectionUnit.Week;
        var columns = showWeek ? 8 : 7;
        var layout = GetMonthLayout(DateOnly.FromDateTime(anchor), firstDay);
        var startTicks = anchor.Ticks - layout.LeadingDays * TimeSpan.TicksPerDay;
        var monthEnd = anchor.AddDays(layout.DaysInMonth - 1);
        var cells = new List<DateViewerCellModel>(6 * columns);
        for (var row = 0; row < 6; row++)
        {
            if (showWeek)
            {
                var rawStart = startTicks + row * 7L * TimeSpan.TicksPerDay;
                var weekDate = FromDateTicks(Math.Max(0, rawStart));
                var number = weekDate is { } date
                    ? (input.WeekNumbering == DateWeekNumbering.Iso
                        ? DateUnitMath.GetIsoWeek(date).Week
                        : GetWeekIdentity(date, input.Culture, firstDay).Week)
                        .ToString(CultureInfo.InvariantCulture)
                    : string.Empty;
                var inView = weekDate is { } weekStart && DateUnitMath.GetWeekRange(weekStart, firstDay) is { } weekPeriod &&
                             weekPeriod.End >= anchor && weekPeriod.Start <= monthEnd;
                cells.Add(BuildCell(input, weekDate, DateViewerCellType.Week, row, 0, number, inView,
                    firstDay, columns, isDateEnabled, isPeriodEnabled));
            }

            for (var column = 0; column < 7; column++)
            {
                var date = FromDateTicks(startTicks + (row * 7L + column) * TimeSpan.TicksPerDay);
                var inView = date is { } d && d.Year == anchor.Year && d.Month == anchor.Month;
                cells.Add(BuildCell(input, date, DateViewerCellType.Date, row, column + (showWeek ? 1 : 0),
                    date?.Day.ToString(input.DateNumberFormat, input.Culture) ?? string.Empty, inView, firstDay, columns, isDateEnabled, isPeriodEnabled));
            }
        }
        var headers = new List<string>(columns);
        if (showWeek)
        {
            headers.Add(string.Empty);
        }

        var names = input.AbbreviatedWeekdays
            ? input.Culture.DateTimeFormat.AbbreviatedDayNames
            : input.Culture.DateTimeFormat.ShortestDayNames;
        for (var index = 0; index < 7; index++)
        {
            headers.Add(names[((int)firstDay + index) % 7]);
        }

        return new DatePanelModel(anchor, input.PanelKind, 6, columns, cells.AsReadOnly())
        {
            ColumnHeaders = headers.AsReadOnly()
        };
    }

    private static DatePanelModel BuildPeriodPanel(DatePanelInput input, DateTime anchor, DayOfWeek firstDay,
        Func<DateTime, bool> isDateEnabled, Func<DateUnitRange, bool> isPeriodEnabled)
    {
        var kind = input.PanelKind switch
        {
            DateViewerPanelKind.Month => DateViewerCellType.Month,
            DateViewerPanelKind.Quarter => DateViewerCellType.Quarter,
            DateViewerPanelKind.Year => DateViewerCellType.Year,
            _ => throw new ArgumentOutOfRangeException(nameof(input.PanelKind))
        };
        var quarters = kind == DateViewerCellType.Quarter;
        var columns = quarters ? 4 : 3;
        var cells = new List<DateViewerCellModel>(quarters ? 4 : 12);
        var decade = anchor.Year / 10 * 10;
        for (var index = 0; index < (quarters ? 4 : 12); index++)
        {
            DateTime? value;
            string text;
            var inView = true;
            if (kind == DateViewerCellType.Year)
            {
                var year = decade - 1 + index;
                value = year is >= 1 and <= 9999 ? new DateTime(year, 1, 1) : null;
                text = value.HasValue ? year.ToString(input.Culture) : string.Empty;
                inView = year >= decade && year <= decade + 9;
            }
            else
            {
                value = new DateTime(anchor.Year, quarters ? index * 3 + 1 : index + 1, 1);
                text = quarters ? $"Q{index + 1}" : input.Culture.DateTimeFormat.AbbreviatedMonthNames[index];
            }
            cells.Add(BuildCell(input, value, kind, quarters ? 0 : index / 3, quarters ? index : index % 3,
                text, inView, firstDay, columns, isDateEnabled, isPeriodEnabled));
        }
        return new DatePanelModel(anchor, input.PanelKind, quarters ? 1 : 4, columns, cells.AsReadOnly());
    }

    private static DateViewerCellModel BuildCell(DatePanelInput input, DateTime? value, DateViewerCellType kind,
        int row, int column, string text, bool inView, DayOfWeek firstDay, int columns,
        Func<DateTime, bool>? isDateEnabled = null, Func<DateUnitRange, bool>? isPeriodEnabled = null, bool? cachedDisabled = null)
    {
        var cellUnit = GetCellUnit(kind);
        var period = value is { } date ? GetUnitRange(date, cellUnit, firstDay) : (DateUnitRange?)null;
        var disabled = cachedDisabled ?? (period is null || (kind == DateViewerCellType.Date ||
            kind == DateViewerCellType.Week && input.ConstraintMode == DatePanelConstraintMode.Calendar
            ? !isDateEnabled!(value!.Value)
            : !isPeriodEnabled!(period.Value)));
        var comparisonUnit = kind == DateViewerCellType.Date && input.SelectionUnit == DateViewerSelectionUnit.Week
            ? DateViewerSelectionUnit.Week
            : cellUnit;
        bool Matches(DateTime? target) => value is { } candidate && target is { } selection &&
            DateUnitMath.IsSameUnit(candidate, selection, comparisonUnit, firstDay);
        var selectableWeek = kind != DateViewerCellType.Week || input.SelectionUnit == DateViewerSelectionUnit.Week;
        var rangeVisualEligible = IsRangeVisualEligible(input, inView);
        var rangeGrid = rangeVisualEligible && input.SelectionUnit == comparisonUnit && input.Range is not null;
        var state = rangeGrid && value is { } rangeValue
            ? GetRangeState(input, rangeValue, firstDay)
            : default;
        var selected = !state.HasPreview && !disabled && selectableWeek && (Matches(input.SelectedDate) ||
            rangeVisualEligible && input.Range is { } range && (Matches(range.Start) || Matches(range.End)));
        var hovered = !disabled && selectableWeek && Matches(input.HoveredValue);
        var visualEndpoint = state.HasPreview ? state.PreviewStart || state.PreviewEnd : selected;
        var weekVisual = input.SelectionUnit == DateViewerSelectionUnit.Week && input.PanelKind == DateViewerPanelKind.Date &&
                         !disabled && visualEndpoint;
        var flags = GetCellState(period?.Contains(input.Today) == true && kind != DateViewerCellType.Week,
            disabled, selected, state.Committed);
        return new DateViewerCellModel(value, kind, period, text, row, column, inView,
            (flags & DateCellState.Today) != 0, (flags & DateCellState.Disabled) != 0, (flags & DateCellState.Selected) != 0, hovered)
        {
            IsFocused = !disabled && kind != DateViewerCellType.Week && Matches(input.FocusedValue),
            IsVisualEndpoint = visualEndpoint,
            IsNavigationCurrent = false,
            IsRangeStart = (flags & DateCellState.RangeStart) != 0,
            IsRangeEnd = (flags & DateCellState.RangeEnd) != 0,
            IsRangeMiddle = (flags & DateCellState.RangeMiddle) != 0,
            HasRangePreview = state.HasPreview,
            IsRangePreviewStart = state.PreviewStart,
            IsRangePreviewEnd = state.PreviewEnd,
            IsRangePreviewMiddle = state.PreviewMiddle,
            IsWeekSelectionStart = weekVisual && column == 0,
            IsWeekSelectionMiddle = weekVisual && column > 0 && column < columns - 1,
            IsWeekSelectionEnd = weekVisual && column == columns - 1
        };
    }

    private static bool IsRangeVisualEligible(DatePanelInput input, bool isInView) =>
        input is not { IsRangeSelection: true, PanelCount: > 1 } || isInView;

    private static DateViewerSelectionUnit GetCellUnit(DateViewerCellType kind) => kind switch
    {
        DateViewerCellType.Date => DateViewerSelectionUnit.Date,
        DateViewerCellType.Week => DateViewerSelectionUnit.Week,
        DateViewerCellType.Month => DateViewerSelectionUnit.Month,
        DateViewerCellType.Quarter => DateViewerSelectionUnit.Quarter,
        DateViewerCellType.Year => DateViewerSelectionUnit.Year,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private static (DateTime? Start, DateTime? End, bool Empty) GetBounds(DatePanelInput input, DayOfWeek firstDay)
    {
        var start = input.MinDate?.Date;
        var end = input.MaxDate?.Date;
        if (input.ConstraintMode != DatePanelConstraintMode.Calendar)
        {
            start = start.HasValue ? Normalize(start.Value, input.SelectionUnit, firstDay) : null;
            end = end.HasValue ? (input.ConstraintMode == DatePanelConstraintMode.Picker
                ? Normalize(end.Value, input.SelectionUnit, firstDay)
                : GetUnitRange(end.Value, input.SelectionUnit, firstDay).End) : null;
            if (start > end)
            {
                end = input.ConstraintMode == DatePanelConstraintMode.Picker
                    ? start
                    : GetUnitRange(start!.Value, input.SelectionUnit, firstDay).End;
            }
        }
        return (start, end, start > end);
    }

    private readonly record struct RangeState(DateCellState Committed, bool HasPreview,
        bool PreviewStart, bool PreviewEnd, bool PreviewMiddle);

    private static RangeState GetRangeState(DatePanelInput input, DateTime value, DayOfWeek firstDay)
    {
        int? Key(DateTime? date) => date is { } day
            ? DateOnly.FromDateTime(Normalize(day, input.SelectionUnit, firstDay)).DayNumber
            : null;
        var key = Key(value)!.Value;
        var start = Key(input.Range!.Start);
        var end = Key(input.Range.End);
        var previewStart = input.ActiveRangePart == DateRangeActivePart.Start ? Key(input.HoveredValue) : start;
        var previewEnd = input.ActiveRangePart == DateRangeActivePart.End ? Key(input.HoveredValue) : end;
        if (input.HoveredValue is not null && previewStart is not null && previewEnd is not null)
        {
            var preview = GetRangeFlags(key, previewStart, previewEnd);
            return new RangeState(DateCellState.None, true, (preview & DateCellState.RangeStart) != 0,
                (preview & DateCellState.RangeEnd) != 0, (preview & DateCellState.RangeMiddle) != 0);
        }

        return new RangeState(GetRangeFlags(key, start, end), false, false, false, false);
    }

    private static DateTime? FromDateTicks(long ticks) => ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Date.Ticks
        ? new DateTime(ticks)
        : null;
}
