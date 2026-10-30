using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.DatePicker;

public class DatePickerViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "DatePicker";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private CustomizableSizeType _pickerSizeType = CustomizableSizeType.Middle;

    public CustomizableSizeType PickerSizeType
    {
        get => _pickerSizeType;
        set => this.RaiseAndSetIfChanged(ref _pickerSizeType, value);
    }

    private PlacementMode _pickerPlacement;

    public PlacementMode PickerPlacement
    {
        get => _pickerPlacement;
        set => this.RaiseAndSetIfChanged(ref _pickerPlacement, value);
    }

    private List<ISelectOption>? _pickerTypeOptions;

    public List<ISelectOption>? PickerTypeOptions
    {
        get => _pickerTypeOptions;
        set => this.RaiseAndSetIfChanged(ref _pickerTypeOptions, value);
    }

    private ISelectOption? _selectedPickerOption;

    public ISelectOption? SelectedPickerOption
    {
        get => _selectedPickerOption;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPickerOption, value);
            this.RaisePropertyChanged(nameof(IsTimePickerVisible));
            this.RaisePropertyChanged(nameof(IsDatePickerVisible));
            this.RaisePropertyChanged(nameof(SelectedPickerMode));
            this.RaisePropertyChanged(nameof(SelectedPickerPlaceholderText));
        }
    }

    public bool IsTimePickerVisible => SelectedPickerType == PickerTypeTime;

    public bool IsDatePickerVisible => !IsTimePickerVisible;

    public DatePickerMode SelectedPickerMode => SelectedPickerType switch
    {
        PickerTypeWeek => DatePickerMode.Week,
        PickerTypeMonth => DatePickerMode.Month,
        PickerTypeQuarter => DatePickerMode.Quarter,
        PickerTypeYear => DatePickerMode.Year,
        _ => DatePickerMode.Date
    };

    public string SelectedPickerPlaceholderText => SelectedPickerType switch
    {
        PickerTypeTime => DatePickerShowCaseLanguage.Get(DatePickerShowCaseLangResourceKind.P2PlaceholderTextSelectTime, "Select time"),
        PickerTypeDate => DatePickerShowCaseLanguage.Get(DatePickerShowCaseLangResourceKind.P2PlaceholderTextSelectDate, "Select date"),
        PickerTypeWeek => DatePickerShowCaseLanguage.Get(DatePickerShowCaseLangResourceKind.P2PlaceholderTextSelectWeek, "Select week"),
        PickerTypeMonth => DatePickerShowCaseLanguage.Get(DatePickerShowCaseLangResourceKind.P2PlaceholderTextSelectMonth, "Select month"),
        PickerTypeQuarter => DatePickerShowCaseLanguage.Get(DatePickerShowCaseLangResourceKind.P2PlaceholderTextSelectQuarter, "Select quarter"),
        PickerTypeYear => DatePickerShowCaseLanguage.Get(DatePickerShowCaseLangResourceKind.P2PlaceholderTextSelectYear, "Select year"),
        _ => DatePickerShowCaseLanguage.Get(DatePickerShowCaseLangResourceKind.P2PlaceholderTextSelectTime, "Select time")
    };

    private DateTime? _boundSelectedDateTime = new DateTime(2026, 7, 5);

    public DateTime? BoundSelectedDateTime
    {
        get => _boundSelectedDateTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _boundSelectedDateTime, value);
            this.RaisePropertyChanged(nameof(BoundSelectedDateTimeText));
        }
    }

    public string BoundSelectedDateTimeText =>
        BoundSelectedDateTime?.ToString("yyyy-MM-dd", GalleryLocalization.GetFormattingCulture()) ?? "-";

    private DateTime? _boundRangeStartSelectedDate = new DateTime(2026, 7, 6);

    public DateTime? BoundRangeStartSelectedDate
    {
        get => _boundRangeStartSelectedDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _boundRangeStartSelectedDate, value);
            this.RaisePropertyChanged(nameof(BoundRangeSelectedDateText));
        }
    }

    private DateTime? _boundRangeEndSelectedDate = new DateTime(2026, 7, 12);

    public DateTime? BoundRangeEndSelectedDate
    {
        get => _boundRangeEndSelectedDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _boundRangeEndSelectedDate, value);
            this.RaisePropertyChanged(nameof(BoundRangeSelectedDateText));
        }
    }

    public string BoundRangeSelectedDateText
    {
        get
        {
            var culture = GalleryLocalization.GetFormattingCulture();
            var startText = BoundRangeStartSelectedDate?.ToString("yyyy-MM-dd", culture) ?? "-";
            var endText   = BoundRangeEndSelectedDate?.ToString("yyyy-MM-dd", culture) ?? "-";
            return $"{startText} → {endText}";
        }
    }

    public DatePickerViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

    private DateTime? _semanticPreviewRangeStart = DateTime.Today;
    private DateTime? _semanticPreviewRangeEnd = DateTime.Today.AddDays(16);

    public DateTime? SemanticPreviewRangeStart
    {
        get => _semanticPreviewRangeStart;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewRangeStart, value);
    }

    public DateTime? SemanticPreviewRangeEnd
    {
        get => _semanticPreviewRangeEnd;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewRangeEnd, value);
    }

    public void HandlePickerSizeTypeOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        PickerSizeType = args.Index switch
        {
            0 => CustomizableSizeType.Large,
            2 => CustomizableSizeType.Small,
            3 => CustomizableSizeType.Custom,
            _ => CustomizableSizeType.Middle
        };
    }

    public void HandlePickerPlacementCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (args.Index == 0)
        {
            PickerPlacement = PlacementMode.TopEdgeAlignedLeft;
        }
        else if (args.Index == 1)
        {
            PickerPlacement = PlacementMode.TopEdgeAlignedRight;
        }
        else if (args.Index == 2)
        {
            PickerPlacement = PlacementMode.BottomEdgeAlignedLeft;
        }
        else
        {
            PickerPlacement = PlacementMode.BottomEdgeAlignedRight;
        }
    }

    internal const string PickerTypeTime = "time";
    internal const string PickerTypeDate = "date";
    internal const string PickerTypeWeek = "week";
    internal const string PickerTypeMonth = "month";
    internal const string PickerTypeQuarter = "quarter";
    internal const string PickerTypeYear = "year";

    private string SelectedPickerType => SelectedPickerOption?.Content?.ToString() ?? PickerTypeTime;
}
