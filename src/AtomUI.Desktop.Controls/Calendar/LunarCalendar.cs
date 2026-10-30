using AtomUI.Desktop.Controls.Internal.Calendar;
using AtomUI.Desktop.Controls.Internal.Calendar.Lunar;
using Avalonia;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

public partial class LunarCalendar : Calendar
{
    public static CalendarDateRange SupportedRange { get; } =
        new(new DateTime(1900, 1, 1), new DateTime(2100, 12, 31));

    public static readonly StyledProperty<bool> ShowSolarTermsProperty =
        AvaloniaProperty.Register<LunarCalendar, bool>(nameof(ShowSolarTerms), true);

    public static readonly StyledProperty<bool> ShowTraditionalFestivalsProperty =
        AvaloniaProperty.Register<LunarCalendar, bool>(nameof(ShowTraditionalFestivals), true);

    public static readonly StyledProperty<bool> ShowHolidaysProperty =
        AvaloniaProperty.Register<LunarCalendar, bool>(nameof(ShowHolidays), true);

    public static readonly StyledProperty<bool> HighlightWeekendsProperty =
        AvaloniaProperty.Register<LunarCalendar, bool>(nameof(HighlightWeekends), true);

    public static readonly StyledProperty<ILunarCalendarHolidayProvider?> HolidayProviderProperty =
        AvaloniaProperty.Register<LunarCalendar, ILunarCalendarHolidayProvider?>(nameof(HolidayProvider));

    public static readonly DirectProperty<LunarCalendar, LunarCalendarDateInfo> SelectedLunarDateInfoProperty =
        AvaloniaProperty.RegisterDirect<LunarCalendar, LunarCalendarDateInfo>(
            nameof(SelectedLunarDateInfo),
            owner => owner.SelectedLunarDateInfo);

    public bool ShowSolarTerms
    {
        get => GetValue(ShowSolarTermsProperty);
        set => SetValue(ShowSolarTermsProperty, value);
    }

    public bool ShowTraditionalFestivals
    {
        get => GetValue(ShowTraditionalFestivalsProperty);
        set => SetValue(ShowTraditionalFestivalsProperty, value);
    }

    public bool ShowHolidays
    {
        get => GetValue(ShowHolidaysProperty);
        set => SetValue(ShowHolidaysProperty, value);
    }

    public bool HighlightWeekends
    {
        get => GetValue(HighlightWeekendsProperty);
        set => SetValue(HighlightWeekendsProperty, value);
    }

    public ILunarCalendarHolidayProvider? HolidayProvider
    {
        get => GetValue(HolidayProviderProperty);
        set => SetValue(HolidayProviderProperty, value);
    }

    private LunarCalendarDateInfo _selectedLunarDateInfo =
        LunarCalendarDateProjector.Create(ClampToSupportedRange(DateTime.Today));

    public LunarCalendarDateInfo SelectedLunarDateInfo
    {
        get => _selectedLunarDateInfo;
        private set => SetAndRaise(SelectedLunarDateInfoProperty, ref _selectedLunarDateInfo, value);
    }

    private LunarCalendarPresentationAdapter? _lunarPresentationAdapter;

    static LunarCalendar()
    {
        ValueProperty.OverrideMetadata<LunarCalendar>(
            new StyledPropertyMetadata<DateTime>(
                defaultBindingMode: BindingMode.TwoWay,
                coerce: static (_, value) => ClampToSupportedRange(value)));
    }

    public LunarCalendar()
    {
        _lunarPresentationAdapter = new LunarCalendarPresentationAdapter(this);
        SetPresentationAdapter(_lunarPresentationAdapter);
        UpdateSelectedLunarDateInfo();
    }

    public void RefreshHolidayData()
    {
        _lunarPresentationAdapter?.RefreshHolidayData();
        RefreshPresentation();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _lunarPresentationAdapter?.InvalidatePanelData();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ValueProperty && _lunarPresentationAdapter is not null)
        {
            UpdateSelectedLunarDateInfo();
        }

        base.OnPropertyChanged(change);

        if (_lunarPresentationAdapter is null)
        {
            return;
        }

        if (change.Property == ShowSolarTermsProperty ||
            change.Property == ShowTraditionalFestivalsProperty ||
            change.Property == ShowHolidaysProperty ||
            change.Property == HighlightWeekendsProperty ||
            change.Property == HolidayProviderProperty)
        {
            _lunarPresentationAdapter.InvalidatePanelData();
            RefreshPresentation();
        }
    }

    private void UpdateSelectedLunarDateInfo()
    {
        SelectedLunarDateInfo = LunarCalendarDateProjector.Create(Value.Date);
    }

    private static DateTime ClampToSupportedRange(DateTime value)
    {
        var date = value.Date;
        if (date < SupportedRange.Start)
        {
            return SupportedRange.Start;
        }

        return date > SupportedRange.End ? SupportedRange.End : date;
    }
}
