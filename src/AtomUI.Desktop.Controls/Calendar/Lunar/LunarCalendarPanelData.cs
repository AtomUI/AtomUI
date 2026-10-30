namespace AtomUI.Desktop.Controls.Internal.Calendar.Lunar;

internal sealed class LunarCalendarPanelData
{
    internal LunarCalendarPanelData(
        IReadOnlyDictionary<DateTime, LunarCalendarDateInfo> dates,
        IReadOnlyDictionary<int, IReadOnlyList<LunarCalendarMonthInfo>> months,
        IReadOnlyDictionary<DateTime, LunarCalendarHoliday> holidays)
    {
        Dates = dates;
        Months = months;
        Holidays = holidays;
    }

    internal IReadOnlyDictionary<DateTime, LunarCalendarDateInfo> Dates { get; }
    internal IReadOnlyDictionary<int, IReadOnlyList<LunarCalendarMonthInfo>> Months { get; }
    internal IReadOnlyDictionary<DateTime, LunarCalendarHoliday> Holidays { get; }
}

internal sealed class LunarCalendarPanelDataKey
{
    internal LunarCalendarPanelDataKey(
        DateViewerPanelKind panelKind,
        int year,
        int month,
        DateTime? visibleStart,
        DateTime? visibleEnd,
        string cultureName,
        bool showSolarTerms,
        bool showTraditionalFestivals,
        bool showHolidays,
        bool highlightWeekends,
        ILunarCalendarHolidayProvider? provider,
        long providerRevision)
    {
        PanelKind = panelKind;
        VisibleStart = visibleStart;
        VisibleEnd = visibleEnd;
        Year = year;
        Month = month;
        CultureName = cultureName;
        ShowSolarTerms = showSolarTerms;
        ShowTraditionalFestivals = showTraditionalFestivals;
        ShowHolidays = showHolidays;
        HighlightWeekends = highlightWeekends;
        Provider = provider;
        ProviderRevision = providerRevision;
    }

    private DateViewerPanelKind PanelKind { get; }
    private DateTime? VisibleStart { get; }
    private DateTime? VisibleEnd { get; }
    private int Year { get; }
    private int Month { get; }
    private string CultureName { get; }
    private bool ShowSolarTerms { get; }
    private bool ShowTraditionalFestivals { get; }
    private bool ShowHolidays { get; }
    private bool HighlightWeekends { get; }
    private ILunarCalendarHolidayProvider? Provider { get; }
    private long ProviderRevision { get; }

    internal bool Matches(LunarCalendarPanelDataKey other)
    {
        if (PanelKind == DateViewerPanelKind.Month || other.PanelKind == DateViewerPanelKind.Month)
        {
            return PanelKind == other.PanelKind &&
                   Year == other.Year &&
                   CultureName == other.CultureName;
        }

        return PanelKind == other.PanelKind &&
               Year == other.Year &&
               Month == other.Month &&
               VisibleStart == other.VisibleStart &&
               VisibleEnd == other.VisibleEnd &&
               CultureName == other.CultureName &&
               ShowSolarTerms == other.ShowSolarTerms &&
               ShowTraditionalFestivals == other.ShowTraditionalFestivals &&
               ShowHolidays == other.ShowHolidays &&
               HighlightWeekends == other.HighlightWeekends &&
               ReferenceEquals(Provider, other.Provider) &&
               ProviderRevision == other.ProviderRevision;
    }
}
