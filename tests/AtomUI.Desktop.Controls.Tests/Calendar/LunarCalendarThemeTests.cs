using System.Xml.Linq;
using AtomUI.Desktop.Controls.Internal.Calendar;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Shouldly;
using Xunit;
using LunarCalendarControl = AtomUI.Desktop.Controls.LunarCalendar;

namespace AtomUI.Desktop.Controls.Tests.Calendar;

public class LunarCalendarThemeTests
{
    static LunarCalendarThemeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void LunarCalendarAndDedicatedCell_HaveExactRegisteredThemes()
    {
        var application = Application.Current;
        application.ShouldNotBeNull();

        application!.TryFindResource(typeof(LunarCalendarControl), out var calendarResource).ShouldBeTrue();
        calendarResource.ShouldNotBeNull();
        ((ControlTheme)calendarResource!).TargetType.ShouldBe(typeof(LunarCalendarControl));

        application!.TryFindResource(typeof(LunarCalendarCellContent), out var cellResource).ShouldBeTrue();
        cellResource.ShouldNotBeNull();
        ((ControlTheme)cellResource!).TargetType.ShouldBe(typeof(LunarCalendarCellContent));
    }

    [Fact]
    public void LunarCalendarToken_DerivesLayoutMetricsFromSharedTokens()
    {
        var shared = new DesignToken
        {
            ControlHeightLG = 40,
            ControlHeightSM = 32,
            FontSizeSM = 12,
            FontHeightSM = 20,
            UniformlyMarginXS = 8,
            UniformlyMarginXXS = 4,
            UniformlyPaddingXS = 8,
            LineWidth = 1,
            LineWidthBold = 2,
            ColorTextTertiary = Colors.Gray,
            ColorError = Colors.Red
        };
        var token = new LunarCalendarToken();
        token.AssignEffectiveGlobalToken(shared);

        token.CalculateTokenValues(isDarkMode: false);

        token.MiniDateCellSize.ShouldBe(40);
        token.MiniContentHeight.ShouldBe(308);
        token.MiniMonthCellWidth.ShouldBe(80);
        token.SecondaryTextFontSize.ShouldBe(12);
        token.SecondaryTextLineHeight.ShouldBe(20);
        token.SecondaryTextColor.ShouldBe(Colors.Gray);
        token.WeekendTextColor.ShouldBe(Colors.Red);
        token.HolidayMarkerColor.ShouldBe(Colors.Red);
        token.WorkdayMarkerColor.ShouldBe(Colors.Gray);
        token.RangeBarTopOffset.ShouldBe(56);
        token.FullCellMinHeight.ShouldBe(148);
    }

    [Fact]
    public void LunarRootTheme_ReusesCalendarThemeWithoutEnteringChildTemplates()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/Calendar/Themes/LunarCalendarTheme.axaml");

        source.ShouldContain("CalendarControlTheme");
        XDocument.Parse(source).Elements().Single().Elements().Where(element => element.Name.LocalName == "Style").ShouldBeEmpty();
        source.ShouldNotContain("CalendarHeader#");
        source.ShouldNotContain("CalendarView#");
        source.ShouldNotContain("ComboBox#");
        source.ShouldNotContain("OptionButtonGroup#");
    }

    [Fact]
    public void DedicatedCellTheme_OnlyCrossesItsOwnTemplateBoundaryOncePerSelector()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/Calendar/Themes/LunarCalendarCellContentTheme.axaml");
        var document = XDocument.Parse(source);
        var avalonia = XNamespace.Get("https://github.com/avaloniaui");

        foreach (var selector in document.Descendants(avalonia + "Style")
                     .Select(style => (string?)style.Attribute("Selector"))
                     .Where(selector => selector is not null))
        {
            foreach (var branch in selector!.Split(','))
                branch.Split("/template/", StringSplitOptions.None).Length.ShouldBeLessThanOrEqualTo(2);
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
        {
            directory = directory.Parent;
        }

        directory.ShouldNotBeNull();
        return File.ReadAllText(Path.Combine(directory!.FullName, relativePath));
    }
}
