using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.DateViewer;

public class DateViewerViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "DateViewer";
    public IScreen HostScreen { get; }
    public string? UrlPathSegment => ID.ToString();

    public DateTime SampleDate => new(2026, 10, 15);
    public DateTime MinDate => new(2026, 10, 5);
    public DateTime MaxDate => new(2026, 11, 20);
    public Func<DateTime, bool> DisableWeekends { get; } =
        value => value.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private DateTime? _value;
    public DateTime? Value
    {
        get => _value;
        set
        {
            this.RaiseAndSetIfChanged(ref _value, value);
        }
    }

    private DateViewerRange? _rangeValue = new(new DateTime(2026, 10, 12), new DateTime(2026, 10, 20));
    public DateViewerRange? RangeValue
    {
        get => _rangeValue;
        set
        {
            this.RaiseAndSetIfChanged(ref _rangeValue, value);
            this.RaisePropertyChanged(nameof(RangeValueText));
        }
    }

    private DateViewerSelectionUnit _selectionUnit;
    public DateViewerSelectionUnit SelectionUnit
    {
        get => _selectionUnit;
        set => this.RaiseAndSetIfChanged(ref _selectionUnit, value);
    }

    private DateTime? _emptyValue;
    public DateTime? EmptyValue
    {
        get => _emptyValue;
        set => this.RaiseAndSetIfChanged(ref _emptyValue, value);
    }

    private DateTime _browseDate = new(2026, 10, 15);
    public DateTime BrowseDate
    {
        get => _browseDate;
        set => this.RaiseAndSetIfChanged(ref _browseDate, value);
    }

    public string RangeValueText => $"{Format(RangeValue?.Start)} → {Format(RangeValue?.End)}";
    private static string Format(DateTime? value) =>
        value?.ToString("yyyy-MM-dd", GalleryLocalization.GetFormattingCulture()) ?? "—";

    public DateViewerViewModel(IScreen screen) => HostScreen = screen;
}
