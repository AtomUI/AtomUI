using System.Windows.Input;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Internal.Calendar;
using Shouldly;
using Xunit;
using AtomUI.Desktop.Controls.Internal.DateViewer;

namespace AtomUI.Desktop.Controls.Tests.Calendar;

public class CalendarPresentationAdapterTests
{
    static CalendarPresentationAdapterTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void DefaultAdapter_PreservesExistingCellContextAndAutomationText()
    {
        var owner = new AtomUI.Desktop.Controls.Calendar { Value = new DateTime(2026, 7, 15) };
        var session = new DatePanelSession(owner);
        var model = session.Models.Single().Cells.Single(cell => cell.Value == owner.Value);
        var adapter = DefaultCalendarPresentationAdapter.Instance;

        adapter.Metrics.MiniContentHeightResourceKey.ShouldBe(CalendarTokenKind.MiniContentHeight);
        adapter.CreateCellContext(session, model).ShouldBe(new CalendarCellContext(
            model.Value!.Value,
            session.Input.Today,
            DateViewerCellType.Date,
            model.DisplayText,
            model.IsToday,
            model.IsInView,
            model.IsSelected,
            model.IsDisabled, model.IsFocused));
        adapter.GetAutomationName(session, model).ShouldBe(model.Value!.Value.ToString("D", session.Input.Culture));
    }

    [Fact]
    public void CalendarContexts_AreInheritableWithoutChangingExistingMembers()
    {
        var cell = new DerivedCellContext();
        cell.Value.ShouldBe(new DateTime(2026, 7, 15));

        var header = new DerivedHeaderContext();
        header.Value.ShouldBe(new DateTime(2026, 7, 15));
        header.Mode.ShouldBe(CalendarMode.Month);
    }

    private sealed record DerivedCellContext() : CalendarCellContext(
        new DateTime(2026, 7, 15),
        new DateTime(2026, 7, 30),
        DateViewerCellType.Date,
        "15",
        false,
        true,
        true,
        false);

    private sealed class DerivedHeaderContext : CalendarHeaderContext
    {
        public DerivedHeaderContext()
            : base(
                new DateTime(2026, 7, 15),
                CalendarMode.Month,
                new NoOpCommand(),
                new NoOpCommand())
        {
        }
    }

    private sealed class NoOpCommand : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
