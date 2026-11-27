using AtomUI.Desktop.Controls.Internal.Calendar;
using AtomUI.Desktop.Controls.Internal.Calendar.Lunar;
using Avalonia;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A <see cref="Calendar" /> whose cells also show lunar information: the lunar day, solar terms, traditional
/// festivals and, through a <see cref="HolidayProvider" />, holidays and adjusted workdays.
/// </summary>
public partial class LunarCalendar : Calendar
{
    /// <summary>
    /// The range from 1900-01-01 to 2100-12-31, the dates the lunar calendar has lunar information for.
    /// <see cref="Calendar.Value" /> is coerced into this range with its time of day dropped, and date cells
    /// outside it show no lunar text.
    /// </summary>
    public static CalendarDateRange SupportedRange { get; } =
        new(new DateTime(1900, 1, 1), new DateTime(2100, 12, 31));

    /// <summary>
    /// Identifies the <see cref="ShowSolarTerms" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowSolarTermsProperty =
        AvaloniaProperty.Register<LunarCalendar, bool>(nameof(ShowSolarTerms), true);

    /// <summary>
    /// Identifies the <see cref="ShowTraditionalFestivals" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowTraditionalFestivalsProperty =
        AvaloniaProperty.Register<LunarCalendar, bool>(nameof(ShowTraditionalFestivals), true);

    /// <summary>
    /// Identifies the <see cref="ShowHolidays" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowHolidaysProperty =
        AvaloniaProperty.Register<LunarCalendar, bool>(nameof(ShowHolidays), true);

    /// <summary>
    /// Identifies the <see cref="HighlightWeekends" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> HighlightWeekendsProperty =
        AvaloniaProperty.Register<LunarCalendar, bool>(nameof(HighlightWeekends), true);

    /// <summary>
    /// Identifies the <see cref="HolidayProvider" /> property.
    /// </summary>
    public static readonly StyledProperty<ILunarCalendarHolidayProvider?> HolidayProviderProperty =
        AvaloniaProperty.Register<LunarCalendar, ILunarCalendarHolidayProvider?>(nameof(HolidayProvider));

    /// <summary>
    /// Identifies the <see cref="SelectedLunarDateInfo" /> property.
    /// </summary>
    public static readonly DirectProperty<LunarCalendar, LunarCalendarDateInfo> SelectedLunarDateInfoProperty =
        AvaloniaProperty.RegisterDirect<LunarCalendar, LunarCalendarDateInfo>(
            nameof(SelectedLunarDateInfo),
            owner => owner.SelectedLunarDateInfo);

    /// <summary>
    /// Gets or sets whether the solar term of a date is shown as the second line of its cell instead of the lunar
    /// day name. A holiday name or a traditional festival still takes precedence. The default is
    /// <see langword="true" />.
    /// </summary>
    public bool ShowSolarTerms
    {
        get => GetValue(ShowSolarTermsProperty);
        set => SetValue(ShowSolarTermsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the first traditional festival of a date is shown as the second line of its cell
    /// instead of the lunar day name. A holiday name still takes precedence. The default is
    /// <see langword="true" />.
    /// </summary>
    public bool ShowTraditionalFestivals
    {
        get => GetValue(ShowTraditionalFestivalsProperty);
        set => SetValue(ShowTraditionalFestivalsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the date panel asks <see cref="HolidayProvider" /> for holidays and adjusted workdays.
    /// While it is <see langword="true" />, a holiday with a non-empty name is the second line of the date cell,
    /// ahead of a traditional festival, a solar term or the lunar day name. While it is <see langword="false" />,
    /// the provider is not asked and no cell is a holiday or an adjusted workday. The default is
    /// <see langword="true" />.
    /// </summary>
    public bool ShowHolidays
    {
        get => GetValue(ShowHolidaysProperty);
        set => SetValue(ShowHolidaysProperty, value);
    }

    /// <summary>
    /// Gets or sets whether date cells on a Saturday or Sunday get the <c>:weekend</c> state. A date that the
    /// holiday provider marks as an adjusted workday is excluded. The theme forwards the value to the cell content
    /// of every date cell. The default is <see langword="true" />.
    /// </summary>
    public bool HighlightWeekends
    {
        get => GetValue(HighlightWeekendsProperty);
        set => SetValue(HighlightWeekendsProperty, value);
    }

    /// <summary>
    /// Gets or sets the source of the holidays and adjusted workdays shown in the date cells, or
    /// <see langword="null" /> to show none. It is asked only while <see cref="ShowHolidays" /> is
    /// <see langword="true" />. Call <see cref="RefreshHolidayData" /> when its data changes while the instance
    /// stays the same. The default is <see langword="null" />.
    /// </summary>
    public ILunarCalendarHolidayProvider? HolidayProvider
    {
        get => GetValue(HolidayProviderProperty);
        set => SetValue(HolidayProviderProperty, value);
    }

    private LunarCalendarDateInfo _selectedLunarDateInfo =
        LunarCalendarDateProjector.Create(ClampToSupportedRange(DateTime.Today));

    /// <summary>
    /// Gets the lunar information of the date of <see cref="Calendar.Value" />. It is updated whenever
    /// <see cref="Calendar.Value" /> changes, before that change reaches the base class and the change listeners.
    /// </summary>
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

    /// <summary>
    /// Discards the holiday data obtained from <see cref="HolidayProvider" /> and rebuilds the date panel and the
    /// header, so the provider is asked again. Call it when the provider returns different data without being
    /// replaced; assigning the property rebuilds the calendar by itself.
    /// </summary>
    public void RefreshHolidayData()
    {
        _lunarPresentationAdapter?.RefreshHolidayData();
        RefreshPresentation();
    }

    /// <summary>
    /// Drops the cached lunar panel data when the control leaves the visual tree, so that the data is rebuilt with
    /// the current holiday provider when the control is attached again. The base implementation is called
    /// afterwards.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _lunarPresentationAdapter?.InvalidatePanelData();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Updates <see cref="SelectedLunarDateInfo" /> before the base class is told about a change of
    /// <see cref="Calendar.Value" />, and after that call rebuilds the presentation when
    /// <see cref="ShowSolarTerms" />, <see cref="ShowTraditionalFestivals" />, <see cref="ShowHolidays" />,
    /// <see cref="HighlightWeekends" /> or <see cref="HolidayProvider" /> changed. An override must call the base
    /// implementation to keep both.
    /// </summary>
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
