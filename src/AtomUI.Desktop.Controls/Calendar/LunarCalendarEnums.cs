namespace AtomUI.Desktop.Controls;

/// <summary>
/// One of the 24 solar terms of the traditional Chinese calendar. The underlying value is the position of the term
/// in the year, starting with <see cref="MinorCold" /> as 0: each Gregorian month holds two terms, at positions
/// 2 * (month - 1) and 2 * (month - 1) + 1. The lunar calendar casts a position to this type and indexes its
/// name table by the value, so the members must keep this order and count.
/// </summary>
public enum ChineseSolarTerm
{
    /// <summary>
    /// The first solar term of January.
    /// </summary>
    MinorCold,
    /// <summary>
    /// The second solar term of January.
    /// </summary>
    MajorCold,
    /// <summary>
    /// The first solar term of February.
    /// </summary>
    StartOfSpring,
    /// <summary>
    /// The second solar term of February.
    /// </summary>
    RainWater,
    /// <summary>
    /// The first solar term of March.
    /// </summary>
    AwakeningOfInsects,
    /// <summary>
    /// The second solar term of March.
    /// </summary>
    SpringEquinox,
    /// <summary>
    /// The first solar term of April, the day of the
    /// <see cref="ChineseTraditionalFestival.QingmingFestival" />.
    /// </summary>
    PureBrightness,
    /// <summary>
    /// The second solar term of April.
    /// </summary>
    GrainRain,
    /// <summary>
    /// The first solar term of May.
    /// </summary>
    StartOfSummer,
    /// <summary>
    /// The second solar term of May.
    /// </summary>
    GrainBuds,
    /// <summary>
    /// The first solar term of June.
    /// </summary>
    GrainInEar,
    /// <summary>
    /// The second solar term of June.
    /// </summary>
    SummerSolstice,
    /// <summary>
    /// The first solar term of July.
    /// </summary>
    MinorHeat,
    /// <summary>
    /// The second solar term of July.
    /// </summary>
    MajorHeat,
    /// <summary>
    /// The first solar term of August.
    /// </summary>
    StartOfAutumn,
    /// <summary>
    /// The second solar term of August.
    /// </summary>
    EndOfHeat,
    /// <summary>
    /// The first solar term of September.
    /// </summary>
    WhiteDew,
    /// <summary>
    /// The second solar term of September.
    /// </summary>
    AutumnEquinox,
    /// <summary>
    /// The first solar term of October.
    /// </summary>
    ColdDew,
    /// <summary>
    /// The second solar term of October.
    /// </summary>
    FrostDescent,
    /// <summary>
    /// The first solar term of November.
    /// </summary>
    StartOfWinter,
    /// <summary>
    /// The second solar term of November.
    /// </summary>
    MinorSnow,
    /// <summary>
    /// The first solar term of December.
    /// </summary>
    MajorSnow,
    /// <summary>
    /// The second solar term of December.
    /// </summary>
    WinterSolstice
}

/// <summary>
/// A traditional Chinese festival that the lunar calendar reports for its date. Nine festivals fall on a fixed
/// lunar month and day and are not reported when that month is a leap month;
/// <see cref="LunarNewYearsEve" /> falls on the last day of the twelfth lunar month and
/// <see cref="QingmingFestival" /> on a solar term. The lunar calendar indexes its name table by the underlying
/// value, so the members must keep this order and count.
/// </summary>
public enum ChineseTraditionalFestival
{
    /// <summary>
    /// Day 1 of lunar month 1.
    /// </summary>
    SpringFestival,
    /// <summary>
    /// Day 15 of lunar month 1.
    /// </summary>
    LanternFestival,
    /// <summary>
    /// Day 2 of lunar month 2.
    /// </summary>
    DragonHeadFestival,
    /// <summary>
    /// Day 5 of lunar month 5.
    /// </summary>
    DragonBoatFestival,
    /// <summary>
    /// Day 7 of lunar month 7.
    /// </summary>
    QixiFestival,
    /// <summary>
    /// Day 15 of lunar month 7.
    /// </summary>
    ZhongyuanFestival,
    /// <summary>
    /// Day 15 of lunar month 8.
    /// </summary>
    MidAutumnFestival,
    /// <summary>
    /// Day 9 of lunar month 9.
    /// </summary>
    DoubleNinthFestival,
    /// <summary>
    /// Day 8 of lunar month 12.
    /// </summary>
    LabaFestival,
    /// <summary>
    /// The last day of the twelfth lunar month, whichever day count that month has. A leap twelfth month has no
    /// New Year's Eve.
    /// </summary>
    LunarNewYearsEve,
    /// <summary>
    /// The day of the <see cref="ChineseSolarTerm.PureBrightness" /> solar term, so its date does not depend on
    /// the lunar month or day.
    /// </summary>
    QingmingFestival
}

/// <summary>
/// One of the twelve zodiac signs that name a year in the traditional Chinese calendar. The underlying value is
/// the position in the twelve-year cycle, starting with <see cref="Rat" /> as 0. It equals the value of the
/// year's <see cref="ChineseEarthlyBranch" />: the lunar calendar casts the branch position to this type and
/// indexes its name table by the value, so the members must keep this order and count.
/// </summary>
public enum ChineseZodiac
{
    /// <summary>
    /// The first sign, for the branch <see cref="ChineseEarthlyBranch.Zi" />.
    /// </summary>
    Rat,
    /// <summary>
    /// The second sign, for the branch <see cref="ChineseEarthlyBranch.Chou" />.
    /// </summary>
    Ox,
    /// <summary>
    /// The third sign, for the branch <see cref="ChineseEarthlyBranch.Yin" />.
    /// </summary>
    Tiger,
    /// <summary>
    /// The fourth sign, for the branch <see cref="ChineseEarthlyBranch.Mao" />.
    /// </summary>
    Rabbit,
    /// <summary>
    /// The fifth sign, for the branch <see cref="ChineseEarthlyBranch.Chen" />.
    /// </summary>
    Dragon,
    /// <summary>
    /// The sixth sign, for the branch <see cref="ChineseEarthlyBranch.Si" />.
    /// </summary>
    Snake,
    /// <summary>
    /// The seventh sign, for the branch <see cref="ChineseEarthlyBranch.Wu" />.
    /// </summary>
    Horse,
    /// <summary>
    /// The eighth sign, for the branch <see cref="ChineseEarthlyBranch.Wei" />.
    /// </summary>
    Goat,
    /// <summary>
    /// The ninth sign, for the branch <see cref="ChineseEarthlyBranch.Shen" />.
    /// </summary>
    Monkey,
    /// <summary>
    /// The tenth sign, for the branch <see cref="ChineseEarthlyBranch.You" />.
    /// </summary>
    Rooster,
    /// <summary>
    /// The eleventh sign, for the branch <see cref="ChineseEarthlyBranch.Xu" />.
    /// </summary>
    Dog,
    /// <summary>
    /// The twelfth sign, for the branch <see cref="ChineseEarthlyBranch.Hai" />.
    /// </summary>
    Pig
}

/// <summary>
/// One of the ten heavenly stems that name a year in the traditional Chinese calendar together with an
/// <see cref="ChineseEarthlyBranch" />. The underlying value is the position in the ten-stem cycle, starting with
/// <see cref="Jia" /> as 0; the lunar calendar takes it as the lunar year minus 4, modulo 10, casts it to this
/// type and indexes its name table by it, so the members must keep this order and count.
/// </summary>
public enum ChineseHeavenlyStem
{
    /// <summary>
    /// The first of the ten stems, at position 0.
    /// </summary>
    Jia,
    /// <summary>
    /// The second of the ten stems, at position 1.
    /// </summary>
    Yi,
    /// <summary>
    /// The third of the ten stems, at position 2.
    /// </summary>
    Bing,
    /// <summary>
    /// The fourth of the ten stems, at position 3.
    /// </summary>
    Ding,
    /// <summary>
    /// The fifth of the ten stems, at position 4. It is not
    /// <see cref="ChineseEarthlyBranch.Wu" />, the seventh branch.
    /// </summary>
    Wu,
    /// <summary>
    /// The sixth of the ten stems, at position 5.
    /// </summary>
    Ji,
    /// <summary>
    /// The seventh of the ten stems, at position 6.
    /// </summary>
    Geng,
    /// <summary>
    /// The eighth of the ten stems, at position 7.
    /// </summary>
    Xin,
    /// <summary>
    /// The ninth of the ten stems, at position 8.
    /// </summary>
    Ren,
    /// <summary>
    /// The tenth of the ten stems, at position 9.
    /// </summary>
    Gui
}

/// <summary>
/// One of the twelve earthly branches that name a year in the traditional Chinese calendar together with a
/// <see cref="ChineseHeavenlyStem" />. The underlying value is the position in the twelve-branch cycle, starting
/// with <see cref="Zi" /> as 0; the lunar calendar takes it as the lunar year minus 4, modulo 12, casts it to
/// this type and indexes its name table by it, and uses the same position as the year's
/// <see cref="ChineseZodiac" /> sign. The members must keep this order and count.
/// </summary>
public enum ChineseEarthlyBranch
{
    /// <summary>
    /// The first of the twelve branches, at position 0.
    /// </summary>
    Zi,
    /// <summary>
    /// The second of the twelve branches, at position 1.
    /// </summary>
    Chou,
    /// <summary>
    /// The third of the twelve branches, at position 2.
    /// </summary>
    Yin,
    /// <summary>
    /// The fourth of the twelve branches, at position 3.
    /// </summary>
    Mao,
    /// <summary>
    /// The fifth of the twelve branches, at position 4.
    /// </summary>
    Chen,
    /// <summary>
    /// The sixth of the twelve branches, at position 5.
    /// </summary>
    Si,
    /// <summary>
    /// The seventh of the twelve branches, at position 6. It is not
    /// <see cref="ChineseHeavenlyStem.Wu" />, the fifth stem.
    /// </summary>
    Wu,
    /// <summary>
    /// The eighth of the twelve branches, at position 7.
    /// </summary>
    Wei,
    /// <summary>
    /// The ninth of the twelve branches, at position 8.
    /// </summary>
    Shen,
    /// <summary>
    /// The tenth of the twelve branches, at position 9.
    /// </summary>
    You,
    /// <summary>
    /// The eleventh of the twelve branches, at position 10.
    /// </summary>
    Xu,
    /// <summary>
    /// The twelfth of the twelve branches, at position 11.
    /// </summary>
    Hai
}

/// <summary>
/// Whether a <see cref="LunarCalendarHoliday" /> marks its date as a holiday or as an adjusted workday.
/// </summary>
public enum LunarCalendarHolidayKind
{
    /// <summary>
    /// The date is a day off.
    /// </summary>
    Holiday,
    /// <summary>
    /// The date is an adjusted workday, a working day that replaces a day off. A Saturday or Sunday of this kind
    /// is not treated as a weekend.
    /// </summary>
    Workday
}

/// <summary>
/// What the second line of a lunar calendar cell names, so that each kind can be styled differently. A date cell
/// takes the first of these that applies and is enabled: the holiday or adjusted workday, the traditional
/// festival, the solar term, the lunar day.
/// </summary>
public enum LunarCalendarSecondaryContentKind
{
    /// <summary>
    /// The name of the lunar day of the date, or the range of lunar months of a month cell. It is also the kind
    /// when the second line is empty.
    /// </summary>
    LunarDay,
    /// <summary>
    /// The name of the solar term of the date.
    /// </summary>
    SolarTerm,
    /// <summary>
    /// The name of the first traditional festival of the date.
    /// </summary>
    TraditionalFestival,
    /// <summary>
    /// The name of a <see cref="LunarCalendarHoliday" /> that marks the date as a holiday.
    /// </summary>
    Holiday,
    /// <summary>
    /// The name of a <see cref="LunarCalendarHoliday" /> that marks the date as an adjusted workday.
    /// </summary>
    Workday
}
