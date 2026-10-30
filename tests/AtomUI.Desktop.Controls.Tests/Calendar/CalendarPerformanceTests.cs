using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUICalendar = AtomUI.Desktop.Controls.Calendar;
using CalendarCellControl = AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell;

namespace AtomUI.Desktop.Controls.Tests.Calendar;

public class CalendarPerformanceTests
{
    static CalendarPerformanceTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void DefaultMonthInstance_Realizes42CellContainers()
    {
        var calendar = new AtomUICalendar { Value = new DateTime(2026, 7, 15) };
        var window = Show(calendar);
        try
        {
            CellCount(calendar).ShouldBe(42);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RepeatedModeSwitch_KeepsContainerCountBounded()
    {
        var calendar = new AtomUICalendar { Value = new DateTime(2026, 7, 15) };
        var window = Show(calendar);
        try
        {
            for (var i = 0; i < 20; i++)
            {
                calendar.Mode = i % 2 == 0 ? CalendarMode.Year : CalendarMode.Month;
                Dispatcher.UIThread.RunJobs();
            }

            // 结束于 Month 模式(第 20 次 i=19 为奇数 → Month),容器上界 42
            calendar.Mode.ShouldBe(CalendarMode.Month);
            CellCount(calendar).ShouldBe(42);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RepeatedValueChange_KeepsContainerCountBounded()
    {
        var calendar = new AtomUICalendar { Value = new DateTime(2026, 1, 15) };
        var window = Show(calendar);
        try
        {
            for (var i = 0; i < 24; i++)
            {
                calendar.Value = new DateTime(2026, 1, 15).AddMonths(i);
                Dispatcher.UIThread.RunJobs();
            }

            CellCount(calendar).ShouldBe(42);
        }
        finally
        {
            window.Close();
        }
    }

    private static int CellCount(AtomUICalendar calendar) =>
        calendar.GetVisualDescendants().OfType<CalendarCellControl>().Count();

    private static Avalonia.Controls.Window Show(Control content)
    {
        var window = new Avalonia.Controls.Window { Width = 400, Height = 400, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
