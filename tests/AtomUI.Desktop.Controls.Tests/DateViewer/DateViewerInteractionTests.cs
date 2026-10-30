// Public panel input behavior must stay aligned across pointer, keyboard and nested content.
using System.Reflection;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DateViewers;

public class DateViewerInteractionTests
{
    static DateViewerInteractionTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Releasing_Outside_The_Cell_Cancels_Activation()
    {
        WithCell((window, cell, host) =>
        {
            var inside = cell.TranslatePoint(new Point(30, 20), window)!.Value;
            window.MouseDown(inside, MouseButton.Left);
            window.MouseUp(inside + new Vector(150, 0), MouseButton.Left);
            host.Activations.ShouldBe(0);
            cell.Classes.Contains(":pressed").ShouldBeFalse("release outside must cancel the press state as well as selection");
        });
    }

    [Fact]
    public void A_Normal_Press_And_Release_Activates_Once()
    {
        WithCell((window, cell, host) =>
        {
            var point = cell.TranslatePoint(new Point(30, 20), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            host.Activations.ShouldBe(1);
            host.Value.ShouldBe(new DateTime(2026, 7, 15));
        });
    }

    [Fact]
    public void Changed_Constraint_After_Press_Is_Revalidated()
    {
        WithCell((window, cell, host) =>
        {
            var point = cell.TranslatePoint(new Point(30, 20), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            host.Disabled = date => date.Day == 15;
            window.MouseUp(point, MouseButton.Left);
            host.Activations.ShouldBe(0);
        });
    }

    [Fact]
    public void Nested_Button_Click_Does_Not_Select_The_Date()
    {
        var clicks = 0;
        var button = new Avalonia.Controls.Button
        {
            Template = new FuncControlTemplate<Avalonia.Controls.Button>((_, _) => new Border { Background = Brushes.Transparent }),
            Width = 100, Height = 40
        };
        button.Click += (_, _) => clicks++;
        WithCell((window, cell, host) =>
        {
            var point = button.TranslatePoint(new Point(20, 20), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            clicks.ShouldBe(1);
            host.Activations.ShouldBe(0);
        }, button);
    }

    [Fact]
    public void Enter_And_Space_Use_One_Activation_Path()
    {
        WithCell((window, cell, host) =>
        {
            cell.Focus(); Dispatcher.UIThread.RunJobs();
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            host.Activations.ShouldBe(1);
            window.KeyPress(Key.Space, RawInputModifiers.None, PhysicalKey.Space, null);
            host.Activations.ShouldBe(1);
            window.KeyRelease(Key.Space, RawInputModifiers.None, PhysicalKey.Space, null);
            host.Activations.ShouldBe(2);
        });
    }

    [Fact]
    public void A_Nested_Focusable_Content_Does_Not_Move_The_Grid_Cursor()
    {
        var viewer = new DateViewer { DisplayDate = new DateTime(2026, 7, 15) };
        var button = new Avalonia.Controls.Button { Content = "nested action" };
        viewer.FullCellTemplate = new FuncDataTemplate<DateViewerCellContext>((context, _) => context?.Value == new DateTime(2026, 7, 15) ? button : new Border());
        var window = new Avalonia.Controls.Window { Width = 500, Height = 400, Content = viewer };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs();
            var before = viewer.FocusedValue;
            button.Focus();
            button.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Right });
            viewer.FocusedValue.ShouldBe(before);
            viewer.Value.ShouldBeNull();
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Moving_Into_The_Second_Panel_Keeps_The_Shared_Hover_Value()
    {
        var viewer = new RangeDateViewer { DisplayDate = new DateTime(2026, 7, 15) };
        var observed = new List<DateTime?>();
        viewer.HoveredValueChanged += (_, args) => observed.Add(args.Value);
        var window = new Avalonia.Controls.Window { Width = 700, Height = 400, Content = viewer };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs();
            DateViewerCell Cell(DateTime value, int panel) => viewer.GetVisualDescendants().OfType<DateViewerCell>().Single(cell =>
                cell.Model?.Value == value && cell.GetVisualAncestors().OfType<DatePanel>().First().PanelIndex == panel);
            Point Center(DateViewerCell cell) => cell.TranslatePoint(
                new Point(cell.Bounds.Width / 2, cell.Bounds.Height / 2), window).ShouldNotBeNull();

            window.MouseMove(Center(Cell(new DateTime(2026, 7, 20), 0)));
            Dispatcher.UIThread.RunJobs();
            observed.Clear();
            window.MouseMove(Center(Cell(new DateTime(2026, 8, 20), 1)));
            Dispatcher.UIThread.RunJobs();

            observed.ShouldNotContain((DateTime?)null,
                "both panels belong to one hover surface, so crossing their seam must not clear the preview");
            viewer.HoveredValue.ShouldBe(new DateTime(2026, 8, 20));
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private static void WithCell(Action<Avalonia.Controls.Window, TemplatedControl, Host> action, Control? nested = null)
    {
        var type = RequiredType("DateViewerCell");
        var cell = (TemplatedControl)Activator.CreateInstance(type, nonPublic: true)!;
        cell.Width = 120;
        cell.Height = 60;
        cell.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        cell.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
        cell.Template = new FuncControlTemplate((_, _) => new Border { Background = Brushes.Transparent, Child = nested });
        var host = new Host();
        var session = new DatePanelSession(host);
        var model = session.Models[0].Cells.Single(c => c.Value == new DateTime(2026, 7, 15));
        type.GetMethod("Bind", [typeof(DatePanelSession), typeof(DateViewerCellModel)])!.Invoke(cell, [session, model]);
        var window = new Avalonia.Controls.Window { Width = 320, Height = 240, Content = cell };
        try { window.Show(); Dispatcher.UIThread.RunJobs(); action(window, cell, host); Dispatcher.UIThread.RunJobs(); }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private sealed class Host : IDatePanelHost
    {
        public Func<DateTime, bool>? Disabled { get; set; }
        public int Activations { get; private set; }
        public DateTime? Value { get; private set; }
        public DateTime? Previewed { get; private set; }
        public DatePanelInput ReadInput() => new() { DisplayDate = new DateTime(2026, 7, 15), DisabledDate = Disabled, SelectedDate = Value };
        public void Activate(DateCellSelection selection) { Value = selection.Value; Activations++; }
        public void NavigateTo(DateTime displayDate, DateViewerPanelKind panelKind) { }
        public void Preview(DateTime? hoveredValue) => Previewed = hoveredValue;
    }

    [Fact]
    public void Host_Contract_Accepts_A_Projected_Input_And_Reports_Intent()
    {
        var host = RequiredType("IDatePanelHost");
        host.IsInterface.ShouldBeTrue();
        host.GetMethod("ReadInput")!.ReturnType.FullName.ShouldBe("AtomUI.Desktop.Controls.Internal.DateViewer.DatePanelInput");
        host.GetMethod("Activate").ShouldNotBeNull();
        host.GetMethod("NavigateTo").ShouldNotBeNull();
        host.GetMethod("Preview").ShouldNotBeNull();
    }

    [Fact]
    public void Cell_Uses_The_Common_Templated_Interaction_Container()
    {
        var type = RequiredType("DateViewerCell");
        typeof(TemplatedControl).IsAssignableFrom(type).ShouldBeTrue();
        typeof(Avalonia.Controls.Button).IsAssignableFrom(type).ShouldBeFalse();
        type.GetMethod("Activate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ShouldNotBeNull();
    }

    private static Type RequiredType(string name)
    {
        var type = typeof(DatePicker).Assembly.GetType("AtomUI.Desktop.Controls.Internal.DateViewer." + name);
        type.ShouldNotBeNull($"shared input contract {name} must exist");
        return type!;
    }
}
