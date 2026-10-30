using AtomUI.Desktop.Controls.Internal.DateViewer;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUICalendar = AtomUI.Desktop.Controls.Calendar;
using CalendarCellControl = AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell;
using CalendarViewControl = AtomUI.Desktop.Controls.Internal.DateViewer.DatePanel;

namespace AtomUI.Desktop.Controls.Tests.Calendar;

public class CalendarAutomationTests
{
    static CalendarAutomationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void ViewPeer_ExposesSingleRequiredSelection()
    {
        var calendar = new AtomUICalendar { Value = new DateTime(2026, 7, 15) };
        var window = Show(calendar);
        try
        {
            var view = calendar.GetVisualDescendants().OfType<CalendarViewControl>().Single();
            var peer = ControlAutomationPeer.CreatePeerForElement(view)
                .ShouldBeOfType<DatePanelAutomationPeer>();
            var provider = peer.ShouldBeAssignableTo<ISelectionProvider>();

            provider.CanSelectMultiple.ShouldBeFalse();
            provider.IsSelectionRequired.ShouldBeTrue();
            provider.GetSelection().Count.ShouldBe(1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void CellPeer_UsesFullLocalizedNameAndViewSelectionContainer()
    {
        var calendar = new AtomUICalendar { Value = new DateTime(2026, 7, 15) };
        var window = Show(calendar);
        try
        {
            var view = calendar.GetVisualDescendants().OfType<CalendarViewControl>().Single();
            var cell = calendar.GetVisualDescendants().OfType<CalendarCellControl>()
                .Single(item => item.Model is { IsSelected: true });
            var viewPeer = ControlAutomationPeer.CreatePeerForElement(view)
                .ShouldBeOfType<DatePanelAutomationPeer>();
            var viewProvider = viewPeer.ShouldBeAssignableTo<ISelectionProvider>();
            var cellPeer = ControlAutomationPeer.CreatePeerForElement(cell)
                .ShouldBeOfType<DateViewerCellAutomationPeer>();

            cellPeer.GetName().ShouldContain("2026");
            cellPeer.GetName().ShouldNotBe(cell.DisplayText);
            cellPeer.SelectionContainer.ShouldBeSameAs(viewProvider);
        }
        finally
        {
            window.Close();
        }
    }

    private static Avalonia.Controls.Window Show(Avalonia.Controls.Control content)
    {
        var window = new Avalonia.Controls.Window { Width = 400, Height = 400, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
