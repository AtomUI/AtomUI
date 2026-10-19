using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Calendar;

public class LunarCalendarContractTests
{

    [Fact]
    public void DateInfo_NormalizesSolarDateAndCopiesFestivalCollection()
    {
        var festivals = new List<ChineseTraditionalFestival>
        {
            ChineseTraditionalFestival.SpringFestival
        };

        var info = new LunarCalendarDateInfo(
            new DateTime(2024, 2, 10, 18, 30, 0),
            2024,
            1,
            1,
            false,
            ChineseHeavenlyStem.Jia,
            ChineseEarthlyBranch.Chen,
            ChineseZodiac.Dragon,
            null,
            festivals);
        festivals.Clear();

        info.SolarDate.ShouldBe(new DateTime(2024, 2, 10));
        info.TraditionalFestivals.ShouldBe([ChineseTraditionalFestival.SpringFestival]);
        Should.Throw<NotSupportedException>(() =>
            ((IList<ChineseTraditionalFestival>)info.TraditionalFestivals).Add(
                ChineseTraditionalFestival.LanternFestival));
    }

    [Fact]
    public void Holiday_NormalizesDate()
    {
        var holiday = new LunarCalendarHoliday(
            new DateTime(2026, 10, 1, 12, 0, 0),
            "National Day",
            LunarCalendarHolidayKind.Holiday);

        holiday.Date.ShouldBe(new DateTime(2026, 10, 1));
    }

}
