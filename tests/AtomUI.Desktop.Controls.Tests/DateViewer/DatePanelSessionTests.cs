// Interaction state remains separate from the selection owner and cached date availability.
using System.Reflection;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DateViewers;

public class DatePanelSessionTests
{
    [Fact]
    public void Focus_And_Hover_Do_Not_Select()
    {
        var host = new Host();
        var session = Create(host);
        Apply(session, new DatePanelAction.Focus(new DateTime(2026, 7, 15)));
        Apply(session, new DatePanelAction.MoveFocus(DateFocusDirection.Right));
        Read<DateTime?>(session, "FocusedValue").ShouldBe(new DateTime(2026, 7, 16));
        Apply(session, new DatePanelAction.Hover(new DateTime(2026, 7, 20)));
        Read<DateTime?>(session, "HoveredValue").ShouldBe(new DateTime(2026, 7, 20));
        host.Value.ShouldBeNull();
        host.LastPreview.ShouldBe(new DateTime(2026, 7, 20));
    }

    [Fact]
    public void Activation_Goes_Through_The_Actual_Owner()
    {
        var host = new Host();
        var session = Create(host);
        Apply(session, new DatePanelAction.Activate(new DateTime(2026, 7, 20), DateViewerCellType.Date));
        host.Value.ShouldBe(new DateTime(2026, 7, 20));
        host.Activations.ShouldBe(1);
    }

    [Fact]
    public void A_Previously_Enabled_Date_Is_Revalidated_Before_Activation()
    {
        var host = new Host();
        var session = Create(host);
        host.Input = host.Input with { DisabledDate = date => date.Day == 20 };
        Apply(session, new DatePanelAction.Activate(new DateTime(2026, 7, 20), DateViewerCellType.Date));
        host.Value.ShouldBeNull();
        host.Activations.ShouldBe(0);
    }

    [Fact]
    public void Focus_Skips_Disabled_Dates_And_Does_Not_Select()
    {
        var host = new Host { Input = new DatePanelInput { DisplayDate = new DateTime(2026, 7, 15), DisabledDate = date => date.Day is 16 or 17 } };
        var session = Create(host);
        Apply(session, new DatePanelAction.Focus(new DateTime(2026, 7, 15)));
        Apply(session, new DatePanelAction.MoveFocus(DateFocusDirection.Right));
        Read<DateTime?>(session, "FocusedValue").ShouldBe(new DateTime(2026, 7, 18));
        host.Value.ShouldBeNull();
    }

    [Fact]
    public void Hover_And_Focus_Do_Not_Requery_Date_Availability()
    {
        var calls = 0;
        var host = new Host { Input = new DatePanelInput { DisplayDate = new DateTime(2026, 7, 15), DisabledDate = _ => { calls++; return false; } } };
        var session = (DatePanelSession)Create(host);
        var baseline = calls;
        var changes = 0;
        session.Changed += (_, _) => changes++;
        Apply(session, new DatePanelAction.Hover(new DateTime(2026, 7, 20)));
        changes.ShouldBe(1, "one pointer move must not expose an intermediate stale hover projection");
        Apply(session, new DatePanelAction.Hover(new DateTime(2026, 7, 20)));
        changes.ShouldBe(1);
        Apply(session, new DatePanelAction.Focus(new DateTime(2026, 7, 18)));
        calls.ShouldBe(baseline, "selection-independent topology and constraints must be retained");
    }

    private static object Create(Host host)
    {
        var type = typeof(DatePicker).Assembly.GetType("AtomUI.Desktop.Controls.Internal.DateViewer.DatePanelSession");
        type.ShouldNotBeNull("date interaction session must exist");
        return Activator.CreateInstance(type!, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [host], null)!;
    }

    private static void Apply(object session, DatePanelAction action) => session.GetType().GetMethod("Apply")!.Invoke(session, [action]);
    private static T Read<T>(object session, string name) => (T)session.GetType().GetProperty(name)!.GetValue(session)!;

    private sealed class Host : IDatePanelHost
    {
        public DatePanelInput Input { get; set; } = new() { DisplayDate = new DateTime(2026, 7, 15) };
        public DateTime? Value { get; private set; }
        public DateTime? LastPreview { get; private set; }
        public int Activations { get; private set; }
        public DatePanelInput ReadInput() => Input with { SelectedDate = Value };
        public void NavigateTo(DateTime displayDate, DateViewerPanelKind panelKind) => Input = Input with { DisplayDate = displayDate, PanelKind = panelKind };
        public void Activate(DateCellSelection selection) { Value = selection.Value; Activations++; }
        public void Preview(DateTime? hoveredValue) => LastPreview = hoveredValue;
    }
}
