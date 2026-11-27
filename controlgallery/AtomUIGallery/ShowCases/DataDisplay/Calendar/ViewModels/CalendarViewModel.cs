using System.Globalization;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Data.Converters;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Calendar;

public class CalendarViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Calendar";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    /// <summary>
    /// Anchor date of the examples. It follows the Calendar default.
    /// </summary>
    public DateTime SampleDate { get; } = DateTime.Today;

    /// <summary>
    /// The lunar example is pinned to the Spring Festival so that the holiday and the lunar
    /// secondary text are visible.
    /// </summary>
    public DateTime LunarCalendarSampleDate { get; } = new(2024, 2, 10);

    /// <summary>
    /// Application-level holiday data provider used by the full lunar example.
    /// </summary>
    public ILunarCalendarHolidayProvider LunarCalendarHolidayProvider { get; } =
        new GalleryLunarCalendarHolidayProvider();

    /// <summary>
    /// The cross-date event example always shows January 2026.
    /// </summary>
    public DateTime CrossDateEventsSampleDate { get; } = new(2026, 1, 1);

    private DateTime _selectableCalendarValue = new(2017, 1, 25);

    /// <summary>
    /// The Calendar value that the selectable calendar currently displays and has selected.
    /// </summary>
    public DateTime SelectableCalendarValue
    {
        get => _selectableCalendarValue;
        set => this.RaiseAndSetIfChanged(ref _selectableCalendarValue, value.Date);
    }

    private DateTime _selectableCalendarSelectedValue = new(2017, 1, 25);

    /// <summary>
    /// The last value the user actually selected in the selectable calendar. It feeds the Alert feedback.
    /// </summary>
    public DateTime SelectableCalendarSelectedValue
    {
        get => _selectableCalendarSelectedValue;
        private set
        {
            value = value.Date;
            if (_selectableCalendarSelectedValue != value)
            {
                this.RaiseAndSetIfChanged(ref _selectableCalendarSelectedValue, value);
                this.RaisePropertyChanged(nameof(SelectableCalendarSelectedText));
            }
        }
    }

    public string SelectableCalendarSelectedText => GalleryLocalization.Format(
        CalendarShowCaseLangResourceKind.SelectableCalendarSelectedMessage,
        "You selected date: {0:yyyy-MM-dd}",
        SelectableCalendarSelectedValue);

    public CalendarViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

    public void SelectSelectableCalendarDate(DateTime value)
    {
        SelectableCalendarValue = value;
        SelectableCalendarSelectedValue = value;
    }

    public void RefreshSelectableCalendarText()
    {
        this.RaisePropertyChanged(nameof(SelectableCalendarSelectedText));
    }

    private static string Lang(CalendarShowCaseLangResourceKind kind)
    {
        return Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)?.Get(kind) ?? kind.ToString()
            : kind.ToString();
    }
}

internal sealed class GalleryLunarCalendarHolidayProvider : ILunarCalendarHolidayProvider
{
    private static readonly HolidayDefinition[] Definitions =
    [
        new(new DateTime(2024, 2, 9), LunarCalendarHolidayKind.Holiday, "除夕"),
        new(new DateTime(2024, 2, 10), LunarCalendarHolidayKind.Holiday, "春节假期"),
        new(new DateTime(2024, 2, 18), LunarCalendarHolidayKind.Workday, "调休工作日"),
        new(new DateTime(2024, 4, 4), LunarCalendarHolidayKind.Holiday, "清明假期"),
        new(new DateTime(2024, 5, 1), LunarCalendarHolidayKind.Holiday, "劳动节假期")
    ];

    public bool TryGetHolidays(
        CalendarDateRange visibleRange,
        CultureInfo _,
        out IReadOnlyList<LunarCalendarHoliday> holidays)
    {
        var start = visibleRange.Start.Date;
        var end = visibleRange.End.Date;
        var rows = Definitions
            .Where(definition => definition.Date >= start && definition.Date <= end)
            .Select(definition => new LunarCalendarHoliday(
                definition.Date,
                definition.Name,
                definition.Kind))
            .ToArray();

        holidays = rows;
        return rows.Length > 0;
    }

    private sealed record HolidayDefinition(
        DateTime Date,
        LunarCalendarHolidayKind Kind,
        string Name);
}

public sealed class NoticeCalendarDateEventVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CalendarCellContext { CellType: DateViewerCellType.Date } context ||
            parameter is not string days)
        {
            return false;
        }

        foreach (var dayText in days.Split(
                     new[] { ',', '|' },
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(dayText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var day) &&
                day == context.Value.Day)
            {
                return true;
            }
        }

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class NoticeCalendarDateCellVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is CalendarCellContext { CellType: DateViewerCellType.Date };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class NoticeCalendarMonthBacklogVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is CalendarCellContext { CellType: DateViewerCellType.Month, Value.Month: 9 };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class CustomCalendarHeaderYearOptionsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime date)
        {
            return Array.Empty<int>();
        }

        const int optionCount = 20;
        var startYear = Math.Clamp(date.Year - 10, DateTime.MinValue.Year, DateTime.MaxValue.Year - optionCount + 1);
        return Enumerable.Range(startYear, optionCount).ToArray();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class CustomCalendarHeaderMonthOptionsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var displayCulture = Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)?.Current.FormattingCulture ?? culture
            : culture;
        return Enumerable.Range(1, 12)
            .Select(displayCulture.DateTimeFormat.GetAbbreviatedMonthName)
            .ToArray();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class CustomCalendarHeaderSelectionIndexConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            CalendarMode.Month => 0,
            CalendarMode.Year => 1,
            DateTime date => date.Month - 1,
            _ => -1
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
