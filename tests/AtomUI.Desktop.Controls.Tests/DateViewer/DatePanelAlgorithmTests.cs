// Shared Gregorian topology and selection-unit invariants used by three public control families.
using System.Collections;
using System.Globalization;
using System.Reflection;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DateViewers;

public class DatePanelAlgorithmTests
{
    private const string InternalNamespace = "AtomUI.Desktop.Controls.Internal.DateViewer.";

    [Theory]
    [InlineData("Date", false, 6, 7, 42)]
    [InlineData("Date", true, 6, 8, 48)]
    [InlineData("Month", false, 4, 3, 12)]
    [InlineData("Quarter", false, 1, 4, 4)]
    [InlineData("Year", false, 4, 3, 12)]
    public void Builds_Actual_Unit_Grid(string panel, bool showWeek, int rows, int columns, int count)
    {
        var model = Build(panel, new DateTime(2026, 7, 15), showWeek: showWeek);
        Get<int>(model, "Rows").ShouldBe(rows);
        Get<int>(model, "Columns").ShouldBe(columns);
        Cells(model).Count.ShouldBe(count);
        Cells(model).All(c => Get<int>(c, "Row") < rows && Get<int>(c, "Column") < columns).ShouldBeTrue();
    }

    [Theory]
    [InlineData("Date", 2026, 8, 1)]
    [InlineData("Month", 2027, 1, 1)]
    [InlineData("Quarter", 2027, 1, 1)]
    [InlineData("Year", 2030, 1, 1)]
    public void Secondary_Panel_Uses_The_Panel_Cycle(string panel, int year, int month, int day)
    {
        var model = Build(panel, new DateTime(2026, 7, 15), panelIndex: 1);
        Get<DateTime>(model, "Anchor").ShouldBe(new DateTime(year, month, day));
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(9999, 12, 31)]
    public void Extreme_Date_Grid_Does_Not_Wrap_Or_Overflow(int year, int month, int day)
    {
        var cells = Cells(Build("Date", new DateTime(year, month, day)));
        cells.Count.ShouldBe(42);
        var dates = cells.Select(c => Get<DateTime?>(c, "Value")).Where(d => d.HasValue).Select(d => d!.Value).ToList();
        dates.Distinct().Count().ShouldBe(dates.Count);
        dates.ShouldBe(dates.OrderBy(d => d).ToList());
        cells.Where(c => Get<DateTime?>(c, "Value") is null).ShouldAllBe(c => Get<bool>(c, "IsDisabled"));
    }

    [Fact]
    public void Display_Anchor_Is_Not_A_Selection()
    {
        Cells(Build("Date", new DateTime(2026, 7, 15))).ShouldAllBe(c => !Get<bool>(c, "IsSelected"));
    }

    [Fact]
    public void Selection_And_Hover_Are_Independent()
    {
        var cells = Cells(Build("Date", new DateTime(2026, 7, 15), selected: new DateTime(2026, 7, 20), hover: new DateTime(2026, 7, 21)));
        Get<bool>(Find(cells, new DateTime(2026, 7, 20)), "IsSelected").ShouldBeTrue();
        Get<bool>(Find(cells, new DateTime(2026, 7, 21)), "IsSelected").ShouldBeFalse();
        Get<bool>(Find(cells, new DateTime(2026, 7, 21)), "IsHovered").ShouldBeTrue();
    }

    [Fact]
    public void Disabled_External_Value_Is_Not_Highlighted()
    {
        var cells = Cells(Build("Date", new DateTime(2026, 7, 15), selected: new DateTime(2026, 7, 20),
            disabled: date => date == new DateTime(2026, 7, 20)));
        var cell = Find(cells, new DateTime(2026, 7, 20));
        Get<bool>(cell, "IsDisabled").ShouldBeTrue();
        Get<bool>(cell, "IsSelected").ShouldBeFalse();
    }

    [Fact]
    public void Month_Selection_Uses_The_Whole_Unit()
    {
        var cells = Cells(Build("Month", new DateTime(2026, 7, 15), selected: new DateTime(2026, 8, 20), unit: "Month"));
        Get<bool>(Find(cells, new DateTime(2026, 8, 1)), "IsSelected").ShouldBeTrue();
    }

    [Fact]
    public void Date_Grid_Uses_The_Resolved_Week_Start()
    {
        var cells = Cells(Build("Date", new DateTime(2026, 7, 15)));
        Get<DateTime?>(cells[0], "Value").ShouldBe(new DateTime(2026, 6, 29));
    }

    [Fact]
    public void Bounds_Are_Inclusive_And_Do_Not_Highlight_An_Invalid_Value()
    {
        var input = Input("Date", new DateTime(2026, 7, 15));
        Set(input, "MinDate", new DateTime(2026, 7, 10));
        Set(input, "MaxDate", new DateTime(2026, 7, 20));
        Set(input, "SelectedDate", new DateTime(2026, 7, 5));
        var cells = Cells(BuildInput(input));
        Get<bool>(Find(cells, new DateTime(2026, 7, 10)), "IsDisabled").ShouldBeFalse();
        Get<bool>(Find(cells, new DateTime(2026, 7, 20)), "IsDisabled").ShouldBeFalse();
        Get<bool>(Find(cells, new DateTime(2026, 7, 9)), "IsDisabled").ShouldBeTrue();
        Get<bool>(Find(cells, new DateTime(2026, 7, 21)), "IsDisabled").ShouldBeTrue();
        cells.ShouldAllBe(c => !Get<bool>(c, "IsSelected"));
    }

    [Fact]
    public void Reversed_Unit_Bounds_Collapse_To_The_Minimum_Month()
    {
        var input = Input("Month", new DateTime(2026, 7, 15), unit: "Month");
        Set(input, "MinDate", new DateTime(2026, 9, 20));
        Set(input, "MaxDate", new DateTime(2026, 8, 1));
        var cells = Cells(BuildInput(input));
        cells.Where(c => !Get<bool>(c, "IsDisabled")).Select(c => Get<DateTime?>(c, "Value"))
            .ShouldBe([new DateTime(2026, 9, 1)]);
    }

    [Fact]
    public void Range_Preview_Replaces_Committed_Selected_And_Range_States()
    {
        foreach (var (panel, unit, start, end, hover) in new[]
        {
            ("Date", "Date", new DateTime(2026, 7, 10), new DateTime(2026, 7, 20), new DateTime(2026, 7, 25)),
            ("Date", "Week", new DateTime(2026, 7, 6), new DateTime(2026, 7, 13), new DateTime(2026, 7, 20)),
            ("Month", "Month", new DateTime(2026, 1, 1), new DateTime(2026, 5, 1), new DateTime(2026, 8, 1)),
            ("Quarter", "Quarter", new DateTime(2026, 1, 1), new DateTime(2026, 4, 1), new DateTime(2026, 10, 1)),
            ("Year", "Year", new DateTime(2022, 1, 1), new DateTime(2024, 1, 1), new DateTime(2027, 1, 1))
        })
        {
            var input = Input(panel, start, unit);
            var range = new DateViewerRange(start, end);
            Set(input, "Range", range);
            Set(input, "ActiveRangePart", Enum.Parse(RequiredType(InternalNamespace + "DateRangeActivePart"), "End"));
            Set(input, "HoveredValue", hover);
            var cells = Cells(BuildInput(input)).Where(cell => Get<object>(cell, "Kind").ToString() != "Week").ToList();
            cells.ShouldAllBe(cell => !Get<bool>(cell, "IsSelected"));
            cells.ShouldAllBe(cell => !Get<bool>(cell, "IsRangeStart") &&
                                      !Get<bool>(cell, "IsRangeEnd") && !Get<bool>(cell, "IsRangeMiddle"));
            Get<bool>(Find(cells, hover), "IsRangePreviewEnd").ShouldBeTrue();
            Get<bool>(Find(cells, start), "IsRangePreviewStart").ShouldBeTrue();
            range.ShouldBe(new DateViewerRange(start, end));

            Set(input, "ActiveRangePart", Enum.Parse(RequiredType(InternalNamespace + "DateRangeActivePart"), "Start"));
            cells = Cells(BuildInput(input)).Where(cell => Get<object>(cell, "Kind").ToString() != "Week").ToList();
            Get<bool>(Find(cells, start), "IsVisualEndpoint").ShouldBeFalse();
            Get<bool>(Find(cells, end), "IsRangePreviewStart").ShouldBeTrue();
            Get<bool>(Find(cells, hover), "IsRangePreviewEnd").ShouldBeTrue();

            Set(input, "HoveredValue", end);
            cells = Cells(BuildInput(input)).Where(cell => Get<object>(cell, "Kind").ToString() != "Week").ToList();
            Get<bool>(Find(cells, end), "IsRangePreviewStart").ShouldBeTrue();
            Get<bool>(Find(cells, end), "IsRangePreviewEnd").ShouldBeTrue();
            Get<bool>(Find(cells, start), "IsVisualEndpoint").ShouldBeFalse();
        }
    }

    [Fact]
    public void Focus_Is_Not_A_Selection()
    {
        var input = Input("Date", new DateTime(2026, 7, 15));
        Set(input, "FocusedValue", new DateTime(2026, 7, 16));
        var cells = Cells(BuildInput(input));
        Get<bool>(Find(cells, new DateTime(2026, 7, 16)), "IsFocused").ShouldBeTrue();
        cells.ShouldAllBe(c => !Get<bool>(c, "IsSelected"));
    }

    [Fact]
    public void Iso_Week_Number_And_Selection_Agree_Across_The_Year()
    {
        var input = Input("Date", new DateTime(2026, 1, 1), unit: "Week");
        Set(input, "WeekNumbering", Enum.Parse(RequiredType(InternalNamespace + "DateWeekNumbering"), "Iso"));
        Set(input, "SelectedDate", new DateTime(2026, 1, 1));
        var cells = Cells(BuildInput(input));
        Get<string>(cells[0], "DisplayText").ShouldBe("1");
        Get<bool>(cells[0], "IsWeekSelectionStart").ShouldBeTrue();
        Get<bool>(Find(cells.Skip(1), new DateTime(2026, 1, 4)), "IsWeekSelectionEnd").ShouldBeTrue();
        Get<bool>(Find(cells, new DateTime(2026, 1, 1)), "IsSelected").ShouldBeTrue();
        foreach (var (name, day, week) in new[]
        {
            ("en-US", DayOfWeek.Sunday, 41), ("zh-CN", DayOfWeek.Monday, 40),
            ("zh-TW", DayOfWeek.Sunday, 41), ("pt-BR", DayOfWeek.Sunday, 41)
        })
        {
            var culture = CultureInfo.GetCultureInfo(name);
            AtomUI.Desktop.Controls.Internal.DateViewer.DatePanelAlgorithms.GetWeekFirstDay(culture).ShouldBe(day);
            AtomUI.Desktop.Controls.Internal.DateViewer.DatePanelAlgorithms
                .GetWeekIdentity(new DateTime(2026, 10, 4), culture, day).Week.ShouldBe(week);
            AtomUI.Desktop.Controls.Internal.DateViewer.DatePanelAlgorithms
                .GetWeekIdentity(new DateTime(2025, 12, 31), culture, day).ShouldBe((2026, 1));
        }
    }

    [Fact]
    public void Predicate_Is_Not_Repeated_For_The_Selected_Visible_Date()
    {
        var calls = new Dictionary<DateTime, int>();
        Build("Date", new DateTime(2026, 7, 15), selected: new DateTime(2026, 7, 20), disabled: date =>
        {
            calls[date] = calls.GetValueOrDefault(date) + 1;
            return false;
        });
        calls.Count.ShouldBe(42);
        calls.Values.ShouldAllBe(count => count == 1);
    }

    [Fact]
    public void The_First_Decade_Advances_To_The_Natural_Second_Decade()
    {
        Get<DateTime>(Build("Year", DateTime.MinValue, panelIndex: 1), "Anchor").ShouldBe(new DateTime(10, 1, 1));
    }

    [Fact]
    public void Calendar_Month_Uses_Boundary_Availability_And_Preserves_Selection_Day()
    {
        var input = Input("Month", new DateTime(2026, 1, 31));
        Set(input, "ConstraintMode", Enum.Parse(RequiredType(InternalNamespace + "DatePanelConstraintMode"), "Calendar"));
        Set(input, "SelectedDate", new DateTime(2026, 1, 31));
        Set(input, "DisabledDate", (Func<DateTime, bool>)(date => date.Day == 31));
        var january = Find(Cells(BuildInput(input)), new DateTime(2026, 1, 1));
        Get<bool>(january, "IsDisabled").ShouldBeFalse();
        Get<bool>(january, "IsSelected").ShouldBeTrue();
    }

    [Fact]
    public void A_Month_Is_Not_Disabled_Only_Because_Its_First_Day_Is_Disabled()
    {
        var model = Build("Month", new DateTime(2026, 7, 1), unit: "Month", disabled: date => date.Day == 1);
        Cells(model).ShouldAllBe(c => !Get<bool>(c, "IsDisabled"));
    }

    [Fact]
    public void Reverse_Candidate_Preview_Uses_The_Unmodified_Fixed_Endpoint()
    {
        var committedStart = new DateTime(2026, 7, 12);
        var fixedEnd = new DateTime(2026, 7, 24);
        var hoveredStart = new DateTime(2026, 8, 26);
        var input = Input("Date", new DateTime(2026, 7, 1));
        var range = new DateViewerRange(committedStart, fixedEnd);
        Set(input, "Range", range);
        Set(input, "PanelCount", 2);
        Set(input, "ActiveRangePart", Enum.Parse(RequiredType(InternalNamespace + "DateRangeActivePart"), "Start"));
        Set(input, "HoveredValue", hoveredStart);
        var cells = Cells(BuildInput(input, 0)).Concat(Cells(BuildInput(input, 1))).ToArray();

        cells.ShouldAllBe(cell => !Get<bool>(cell, "IsSelected"));
        cells.ShouldAllBe(cell => !Get<bool>(cell, "IsRangeStart") && !Get<bool>(cell, "IsRangeEnd"));
        Get<bool>(Find(cells, committedStart), "IsRangePreviewStart").ShouldBeFalse();
        Get<bool>(Find(cells, fixedEnd), "IsRangePreviewStart").ShouldBeTrue();
        Get<bool>(Find(cells, hoveredStart), "IsRangePreviewEnd").ShouldBeTrue();
        cells.Count(cell => Get<bool>(cell, "IsRangePreviewStart") || Get<bool>(cell, "IsRangePreviewEnd"))
             .ShouldBe(2);
        range.ShouldBe(new DateViewerRange(committedStart, fixedEnd));
    }

    [Fact]
    public void Empty_Quarter_Has_A_Navigation_Anchor_Without_A_Selection()
    {
        var cells = Cells(Build("Quarter", new DateTime(2026, 10, 2), unit: "Quarter"));
        Get<bool>(Find(cells, new DateTime(2026, 10, 1)), "IsNavigationCurrent").ShouldBeFalse();
        cells.ShouldAllBe(c => !Get<bool>(c, "IsSelected"));
    }

    [Fact]
    public void Picker_Weekday_Headers_Are_Two_Letter_Labels_And_Week_Prefix_Is_Empty()
    {
        var model = Build("Date", new DateTime(2026, 10, 2), unit: "Week");
        Get<IReadOnlyList<string>>(model, "ColumnHeaders").ShouldBe(["", "Mo", "Tu", "We", "Th", "Fr", "Sa", "Su"]);
    }

    private static object Build(string panel, DateTime anchor, bool showWeek = false, int panelIndex = 0,
        DateTime? selected = null, DateTime? hover = null, Func<DateTime, bool>? disabled = null, string unit = "Date")
    {
        var input = Input(panel, anchor, unit);
        Set(input, "DisplayDate", anchor);
        Set(input, "Today", new DateTime(2026, 7, 1));
        Set(input, "PanelKind", Enum.Parse(RequiredType("AtomUI.Desktop.Controls.DateViewerPanelKind"), panel));
        Set(input, "SelectionUnit", Enum.Parse(RequiredType("AtomUI.Desktop.Controls.DateViewerSelectionUnit"), unit));
        Set(input, "FirstDayOfWeek", DayOfWeek.Monday);
        Set(input, "Culture", CultureInfo.GetCultureInfo("en-US"));
        Set(input, "ShowWeek", showWeek);
        Set(input, "SelectedDate", selected);
        Set(input, "HoveredValue", hover);
        Set(input, "DisabledDate", disabled);
        return BuildInput(input, panelIndex);
    }

    private static object Input(string panel, DateTime anchor, string unit = "Date")
    {
        var input = Activator.CreateInstance(RequiredType(InternalNamespace + "DatePanelInput"), nonPublic: true)!;
        Set(input, "DisplayDate", anchor);
        Set(input, "Today", new DateTime(2026, 7, 1));
        Set(input, "PanelKind", Enum.Parse(RequiredType("AtomUI.Desktop.Controls.DateViewerPanelKind"), panel));
        Set(input, "SelectionUnit", Enum.Parse(RequiredType("AtomUI.Desktop.Controls.DateViewerSelectionUnit"), unit));
        Set(input, "FirstDayOfWeek", DayOfWeek.Monday);
        Set(input, "Culture", CultureInfo.GetCultureInfo("en-US"));
        return input;
    }

    private static object BuildInput(object input, int panelIndex = 0)
    {
        var method = RequiredType(InternalNamespace + "DatePanelAlgorithms").GetMethod("Build", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        return method!.Invoke(null, [input, panelIndex])!;
    }

    private static Type RequiredType(string name)
    {
        var type = typeof(DatePicker).Assembly.GetType(name);
        type.ShouldNotBeNull($"new shared date panel contract {name} must exist");
        return type!;
    }

    private static void Set(object target, string name, object? value)
    {
        var property = target.GetType().GetProperty(name);
        property.ShouldNotBeNull($"input contract {name} must exist");
        property!.SetValue(target, value);
    }
    private static T Get<T>(object target, string name)
    {
        var property = target.GetType().GetProperty(name);
        property.ShouldNotBeNull($"model contract {name} must exist");
        return (T)property!.GetValue(target)!;
    }
    private static List<object> Cells(object model) => ((IEnumerable)model.GetType().GetProperty("Cells")!.GetValue(model)!).Cast<object>().ToList();
    private static object Find(IEnumerable<object> cells, DateTime date) => cells.Single(c => Get<DateTime?>(c, "Value") == date);
}
