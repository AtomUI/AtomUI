using AtomUI.Desktop.Controls.Internal.Calendar.Lunar;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Calendar;

public class LunarCalendarResolverTests
{
    [Fact]
    public void SolarTermTable_ReturnsKnown2024Dates()
    {
        (ChineseSolarTerm Term, int Month, int Day)[] expectedDates =
        [
            (ChineseSolarTerm.MinorCold, 1, 6),
            (ChineseSolarTerm.MajorCold, 1, 20),
            (ChineseSolarTerm.StartOfSpring, 2, 4),
            (ChineseSolarTerm.RainWater, 2, 19),
            (ChineseSolarTerm.AwakeningOfInsects, 3, 5),
            (ChineseSolarTerm.SpringEquinox, 3, 20),
            (ChineseSolarTerm.PureBrightness, 4, 4),
            (ChineseSolarTerm.GrainRain, 4, 19),
            (ChineseSolarTerm.StartOfSummer, 5, 5),
            (ChineseSolarTerm.GrainBuds, 5, 20),
            (ChineseSolarTerm.GrainInEar, 6, 5),
            (ChineseSolarTerm.SummerSolstice, 6, 21),
            (ChineseSolarTerm.MinorHeat, 7, 6),
            (ChineseSolarTerm.MajorHeat, 7, 22),
            (ChineseSolarTerm.StartOfAutumn, 8, 7),
            (ChineseSolarTerm.EndOfHeat, 8, 22),
            (ChineseSolarTerm.WhiteDew, 9, 7),
            (ChineseSolarTerm.AutumnEquinox, 9, 22),
            (ChineseSolarTerm.ColdDew, 10, 8),
            (ChineseSolarTerm.FrostDescent, 10, 23),
            (ChineseSolarTerm.StartOfWinter, 11, 7),
            (ChineseSolarTerm.MinorSnow, 11, 22),
            (ChineseSolarTerm.MajorSnow, 12, 6),
            (ChineseSolarTerm.WinterSolstice, 12, 21)
        ];

        foreach (var (term, month, day) in expectedDates)
        {
            var date = SolarTermResolver.GetDate(2024, term);

            date.ShouldBe(new DateTime(2024, month, day), $"solar term {term}");
            SolarTermResolver.GetSolarTerm(date).ShouldBe(term, $"solar term {term} on {date:yyyy-MM-dd}");
        }
    }

    [Fact]
    public void SolarTermTable_ContainsEveryTermForEverySupportedYear()
    {
        for (var year = 1900; year <= 2100; year++)
        {
            foreach (var term in Enum.GetValues<ChineseSolarTerm>())
            {
                var date = SolarTermResolver.GetDate(year, term);
                date.Year.ShouldBe(year);
                date.Month.ShouldBe((int)term / 2 + 1);
                SolarTermResolver.GetSolarTerm(date).ShouldBe(term);
            }
        }
    }

    [Fact]
    public void TraditionalFestivalResolver_ReturnsFixedLunarFestivals()
    {
        (int Month, int Day, ChineseTraditionalFestival Festival)[] expectedFestivals =
        [
            (1, 1, ChineseTraditionalFestival.SpringFestival),
            (1, 15, ChineseTraditionalFestival.LanternFestival),
            (2, 2, ChineseTraditionalFestival.DragonHeadFestival),
            (5, 5, ChineseTraditionalFestival.DragonBoatFestival),
            (7, 7, ChineseTraditionalFestival.QixiFestival),
            (7, 15, ChineseTraditionalFestival.ZhongyuanFestival),
            (8, 15, ChineseTraditionalFestival.MidAutumnFestival),
            (9, 9, ChineseTraditionalFestival.DoubleNinthFestival),
            (12, 8, ChineseTraditionalFestival.LabaFestival)
        ];

        foreach (var (month, day, festival) in expectedFestivals)
        {
            TraditionalFestivalResolver.Resolve(new ChineseLunarDate(2024, month, day, false), null)
                .ShouldContain(festival, $"lunar date 2024-{month:D2}-{day:D2}");
        }
    }

    [Fact]
    public void TraditionalFestivalResolver_DoesNotRepeatFixedFestivalsInLeapMonth()
    {
        TraditionalFestivalResolver.Resolve(new ChineseLunarDate(2023, 2, 2, true), null)
            .ShouldBeEmpty();
    }

    [Fact]
    public void TraditionalFestivalResolver_DetectsDynamicNewYearsEveAndQingming()
    {
        var newYearsEve = ChineseLunarCalendarEngine.FromSolar(new DateTime(2025, 1, 28));
        TraditionalFestivalResolver.Resolve(newYearsEve, null)
            .ShouldContain(ChineseTraditionalFestival.LunarNewYearsEve);

        var qingming = ChineseLunarCalendarEngine.FromSolar(new DateTime(2024, 4, 4));
        TraditionalFestivalResolver.Resolve(qingming, ChineseSolarTerm.PureBrightness)
            .ShouldBe([ChineseTraditionalFestival.QingmingFestival]);
    }

    [Fact]
    public void DateProjector_CreatesStructuredDateInfo()
    {
        var info = LunarCalendarDateProjector.Create(new DateTime(2024, 2, 10, 23, 0, 0));

        info.SolarDate.ShouldBe(new DateTime(2024, 2, 10));
        info.LunarYear.ShouldBe(2024);
        info.LunarMonth.ShouldBe(1);
        info.LunarDay.ShouldBe(1);
        info.HeavenlyStem.ShouldBe(ChineseHeavenlyStem.Jia);
        info.EarthlyBranch.ShouldBe(ChineseEarthlyBranch.Chen);
        info.Zodiac.ShouldBe(ChineseZodiac.Dragon);
        info.TraditionalFestivals.ShouldBe([ChineseTraditionalFestival.SpringFestival]);
    }

    [Fact]
    public void MonthIntersectionResolver_CollectsAllIntersectingLunarMonthsInOrder()
    {
        LunarCalendarMonthIntersectionResolver.Resolve(2023, 3).ShouldBe([
            new LunarCalendarMonthInfo(2023, 2, false),
            new LunarCalendarMonthInfo(2023, 2, true)
        ]);
        LunarCalendarMonthIntersectionResolver.Resolve(2024, 2).ShouldBe([
            new LunarCalendarMonthInfo(2023, 12, false),
            new LunarCalendarMonthInfo(2024, 1, false)
        ]);
    }

    [Fact]
    public void Formatter_UsesFixedChineseLunarTerms()
    {
        var info = LunarCalendarDateProjector.Create(new DateTime(2024, 2, 10));

        LunarCalendarFormatter.FormatLunarDay(info).ShouldBe("初一");
        LunarCalendarFormatter.FormatFestival(ChineseTraditionalFestival.SpringFestival).ShouldBe("春节");
        LunarCalendarFormatter.FormatSolarTerm(ChineseSolarTerm.PureBrightness).ShouldBe("清明");
        LunarCalendarFormatter.FormatStemBranch(info.HeavenlyStem, info.EarthlyBranch).ShouldBe("甲辰");
        LunarCalendarFormatter.FormatZodiac(info.Zodiac).ShouldBe("龙");
    }
}
