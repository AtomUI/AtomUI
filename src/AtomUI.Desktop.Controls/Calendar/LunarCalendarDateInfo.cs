using System.Collections.ObjectModel;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The Chinese lunar calendar facts of one solar date: the lunar year, month and day, the stem, the branch and the
/// zodiac of the lunar year, the solar term and the traditional festivals.
/// <see cref="LunarCalendar.SelectedLunarDateInfo" /> holds the information of the selected date.
/// </summary>
public sealed record LunarCalendarDateInfo
{
    /// <summary>
    /// Creates the lunar information of one date.
    /// </summary>
    /// <param name="solarDate">
    /// The date the information describes. Its time of day is dropped.
    /// </param>
    /// <param name="lunarYear">
    /// The number of the lunar year.
    /// </param>
    /// <param name="lunarMonth">
    /// The number of the lunar month, from 1 to 12.
    /// </param>
    /// <param name="lunarDay">
    /// The day of the lunar month, from 1 to 30.
    /// </param>
    /// <param name="isLeapMonth">
    /// Whether the date lies in the leap month of <paramref name="lunarMonth" />.
    /// </param>
    /// <param name="heavenlyStem">
    /// The stem of the lunar year.
    /// </param>
    /// <param name="earthlyBranch">
    /// The branch of the lunar year.
    /// </param>
    /// <param name="zodiac">
    /// The zodiac of the lunar year.
    /// </param>
    /// <param name="solarTerm">
    /// The solar term that begins on the date, or <see langword="null" /> when none does.
    /// </param>
    /// <param name="traditionalFestivals">
    /// The traditional festivals of the date. The list is copied, so later changes to the argument are not visible.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="traditionalFestivals" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="lunarMonth" /> is outside 1 to 12, or <paramref name="lunarDay" /> is outside 1 to 30.
    /// </exception>
    public LunarCalendarDateInfo(
        DateTime solarDate,
        int lunarYear,
        int lunarMonth,
        int lunarDay,
        bool isLeapMonth,
        ChineseHeavenlyStem heavenlyStem,
        ChineseEarthlyBranch earthlyBranch,
        ChineseZodiac zodiac,
        ChineseSolarTerm? solarTerm,
        IReadOnlyList<ChineseTraditionalFestival> traditionalFestivals)
    {
        ArgumentNullException.ThrowIfNull(traditionalFestivals);
        if (lunarMonth is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(lunarMonth));
        }

        if (lunarDay is < 1 or > 30)
        {
            throw new ArgumentOutOfRangeException(nameof(lunarDay));
        }

        SolarDate = solarDate.Date;
        LunarYear = lunarYear;
        LunarMonth = lunarMonth;
        LunarDay = lunarDay;
        IsLeapMonth = isLeapMonth;
        HeavenlyStem = heavenlyStem;
        EarthlyBranch = earthlyBranch;
        Zodiac = zodiac;
        SolarTerm = solarTerm;
        TraditionalFestivals = new ReadOnlyCollection<ChineseTraditionalFestival>(traditionalFestivals.ToArray());
    }

    /// <summary>
    /// The date the information describes, without a time of day.
    /// </summary>
    public DateTime SolarDate { get; }
    /// <summary>
    /// The number of the lunar year. It is the number of the Gregorian year in which the lunar year begins.
    /// </summary>
    public int LunarYear { get; }
    /// <summary>
    /// The number of the lunar month, from 1 to 12.
    /// </summary>
    public int LunarMonth { get; }
    /// <summary>
    /// The day of the lunar month, from 1 to 30.
    /// </summary>
    public int LunarDay { get; }
    /// <summary>
    /// Whether the date lies in a leap month. A leap month carries the number of the lunar month it follows.
    /// </summary>
    public bool IsLeapMonth { get; }
    /// <summary>
    /// The heavenly stem of the lunar year. Together with <see cref="EarthlyBranch" /> it forms the
    /// stem-branch name of the lunar year.
    /// </summary>
    public ChineseHeavenlyStem HeavenlyStem { get; }
    /// <summary>
    /// The earthly branch of the lunar year.
    /// </summary>
    public ChineseEarthlyBranch EarthlyBranch { get; }
    /// <summary>
    /// The zodiac of the lunar year.
    /// </summary>
    public ChineseZodiac Zodiac { get; }
    /// <summary>
    /// The solar term that begins on the date, or <see langword="null" /> when none does.
    /// </summary>
    public ChineseSolarTerm? SolarTerm { get; }
    /// <summary>
    /// The traditional festivals of the date; empty when there are none. While traditional festivals are shown,
    /// a date cell shows the first of them as its second line. Record equality compares this list by reference,
    /// so two instances created separately are not equal.
    /// </summary>
    public IReadOnlyList<ChineseTraditionalFestival> TraditionalFestivals { get; }
}

/// <summary>
/// A lunar month that intersects a solar month. A <see cref="LunarCalendar" /> lists the lunar months of a
/// displayed month with these records, to name them in month cells, in the header and in the month options.
/// </summary>
public sealed record LunarCalendarMonthInfo(int LunarYear, int LunarMonth, bool IsLeapMonth)
{
    /// <summary>
    /// The number of the lunar month, from 1 to 12. Creating the record with any other value throws an
    /// <see cref="ArgumentOutOfRangeException" />.
    /// </summary>
    public int LunarMonth { get; } = LunarMonth is >= 1 and <= 12
        ? LunarMonth
        : throw new ArgumentOutOfRangeException(nameof(LunarMonth));
}
