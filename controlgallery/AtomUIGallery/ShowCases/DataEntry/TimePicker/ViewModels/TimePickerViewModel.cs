using AtomUI;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.TimePicker;

public class TimePickerViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "TimePicker";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private CustomizableSizeType _pickerSizeType = CustomizableSizeType.Middle;

    public CustomizableSizeType PickerSizeType
    {
        get => _pickerSizeType;
        set => this.RaiseAndSetIfChanged(ref _pickerSizeType, value);
    }

    private TimeSpan? _boundSelectedTime = new(10, 9, 20);

    public TimeSpan? BoundSelectedTime
    {
        get => _boundSelectedTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _boundSelectedTime, value);
            this.RaisePropertyChanged(nameof(BoundSelectedTimeText));
        }
    }

    public string BoundSelectedTimeText =>
        BoundSelectedTime?.ToString(@"hh\:mm\:ss", GalleryLocalization.GetFormattingCulture()) ?? "-";

    private TimeSpan? _confirmRequiredSelectedTime;

    public TimeSpan? ConfirmRequiredSelectedTime
    {
        get => _confirmRequiredSelectedTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _confirmRequiredSelectedTime, value);
            this.RaisePropertyChanged(nameof(ConfirmRequiredSelectedTimeText));
        }
    }

    public string ConfirmRequiredSelectedTimeText =>
        ConfirmRequiredSelectedTime?.ToString(@"hh\:mm\:ss", GalleryLocalization.GetFormattingCulture()) ?? "-";

    private TimeSpan? _noConfirmSelectedTime;

    public TimeSpan? NoConfirmSelectedTime
    {
        get => _noConfirmSelectedTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _noConfirmSelectedTime, value);
            this.RaisePropertyChanged(nameof(NoConfirmSelectedTimeText));
        }
    }

    public string NoConfirmSelectedTimeText =>
        NoConfirmSelectedTime?.ToString(@"hh\:mm\:ss", GalleryLocalization.GetFormattingCulture()) ?? "-";

    private TimeSpan? _boundRangeStartSelectedTime = new(9, 0, 0);

    public TimeSpan? BoundRangeStartSelectedTime
    {
        get => _boundRangeStartSelectedTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _boundRangeStartSelectedTime, value);
            this.RaisePropertyChanged(nameof(BoundRangeSelectedTimeText));
        }
    }

    private TimeSpan? _boundRangeEndSelectedTime = new(18, 0, 0);

    public TimeSpan? BoundRangeEndSelectedTime
    {
        get => _boundRangeEndSelectedTime;
        set
        {
            this.RaiseAndSetIfChanged(ref _boundRangeEndSelectedTime, value);
            this.RaisePropertyChanged(nameof(BoundRangeSelectedTimeText));
        }
    }

    public string BoundRangeSelectedTimeText
    {
        get
        {
            var culture = GalleryLocalization.GetFormattingCulture();
            var startText = BoundRangeStartSelectedTime?.ToString(@"hh\:mm\:ss", culture) ?? "-";
            var endText   = BoundRangeEndSelectedTime?.ToString(@"hh\:mm\:ss", culture) ?? "-";
            return $"{startText} → {endText}";
        }
    }

    public TimePickerViewModel(IScreen screen)
    {
        HostScreen = screen;
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

}
