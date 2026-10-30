using AtomUI.Desktop.Controls.Internal.DateViewer;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using CalendarControl = AtomUI.Desktop.Controls.Calendar;

namespace AtomUI.Desktop.Controls.Tests.Calendar;

public class CalendarViewTests
{
    static CalendarViewTests() => AvaloniaTestApp.EnsureInitialized();

    [Theory]
    [InlineData(CalendarMode.Month, false, 42)]
    [InlineData(CalendarMode.Month, true, 48)]
    [InlineData(CalendarMode.Year, false, 12)]
    [InlineData(CalendarMode.Year, true, 12)]
    public void SharedPanel_UsesCalendarTopology(CalendarMode mode, bool showWeek, int count)
    {
        var owner = new CalendarControl { Value = new DateTime(2026, 7, 15), Mode = mode, ShowWeek = showWeek };
        var session = new DatePanelSession(owner);
        session.Models.Single().Cells.Count.ShouldBe(count);
        for (var index = 0; index < 20; index++)
        {
            owner.Value = owner.Value.AddMonths(1);
            session.UpdateInput();
            session.Models.Single().Cells.Count.ShouldBe(count);
        }
    }

    [Theory]
    [InlineData(CalendarMode.Month, "Right", 16, 7)]
    [InlineData(CalendarMode.Month, "Left", 14, 7)]
    [InlineData(CalendarMode.Month, "Down", 22, 7)]
    [InlineData(CalendarMode.Month, "Up", 8, 7)]
    [InlineData(CalendarMode.Year, "Right", 15, 8)]
    [InlineData(CalendarMode.Year, "Left", 15, 6)]
    [InlineData(CalendarMode.Year, "Down", 15, 10)]
    [InlineData(CalendarMode.Year, "Up", 15, 4)]
    public void FocusNavigation_DoesNotWriteCalendarValue(CalendarMode mode, string direction, int day, int month)
    {
        var owner = new CalendarControl { Value = new DateTime(2026, 7, 15), Mode = mode };
        var session = new DatePanelSession(owner);
        session.Apply(new DatePanelAction.MoveFocus(Enum.Parse<DateFocusDirection>(direction)));
        session.FocusedValue.ShouldBe(new DateTime(2026, month, day));
        owner.Value.ShouldBe(new DateTime(2026, 7, 15));
    }

    [Theory]
    [InlineData(CalendarMode.Month, 7, 18)]
    [InlineData(CalendarMode.Year, 10, 15)]
    public void FocusNavigation_SkipsDisabledBusinessCells(CalendarMode mode, int month, int day)
    {
        var owner = new CalendarControl
        {
            Value = new DateTime(2026, 7, 15), Mode = mode,
            DisabledDate = date => mode == CalendarMode.Month ? date.Day is 16 or 17 : date.Month is 8 or 9
        };
        var session = new DatePanelSession(owner);
        session.Apply(new DatePanelAction.MoveFocus(DateFocusDirection.Right));
        session.FocusedValue.ShouldBe(new DateTime(2026, month, day));
    }

    [Theory]
    [InlineData(1, 1, 1, "Left")]
    [InlineData(1, 1, 1, "Up")]
    [InlineData(9999, 12, 31, "Right")]
    [InlineData(9999, 12, 31, "Down")]
    public void FocusNavigation_ExtremeDatesDoNotOverflow(int year, int month, int day, string direction)
    {
        var owner = new CalendarControl { Value = new DateTime(year, month, day) };
        var session = new DatePanelSession(owner);
        session.Apply(new DatePanelAction.MoveFocus(Enum.Parse<DateFocusDirection>(direction)));
        session.FocusedValue.ShouldBe(owner.Value);
    }

    [Fact]
    public void SwitchingToYearMode_ReleasesInactiveCellContextsAndTemplates()
    {
        var owner = new CalendarControl
        {
            Value = new DateTime(2026, 7, 15),
            CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object?>((_, _) => new Border())
        };
        var window = new Avalonia.Controls.Window { Width = 700, Height = 600, Content = owner };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var previous = owner.GetVisualDescendants().OfType<DateViewerCell>().ToArray();
            owner.Mode = CalendarMode.Year;
            Dispatcher.UIThread.RunJobs();
            owner.GetVisualDescendants().OfType<DateViewerCell>().Count().ShouldBe(12);
            foreach (var cell in previous.Skip(12))
            {
                cell.Model.ShouldBeNull();
                cell.Context.ShouldBeNull();
                cell.CellTemplate.ShouldBeNull();
                cell.Session.ShouldBeNull();
            }
        }
        finally { window.Close(); }
    }
}
