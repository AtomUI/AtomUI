// Standalone public DateViewer and RangeDateViewer ownership contracts.
using System.Reflection;
using AtomUI.Desktop.Controls.Internal.DateViewer;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DateViewers;

public class DateViewerTests
{
    [Fact]
    public void A_Decorative_Week_Number_Does_Not_Select_A_Date()
    {
        AvaloniaTestApp.EnsureInitialized();
        var viewer = new DateViewer { DisplayDate = new DateTime(2026, 7, 1), ShowWeek = true };
        var window = new Avalonia.Controls.Window { Width = 360, Height = 400, Content = viewer };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs();
            var week = viewer.GetVisualDescendants().OfType<AtomUI.Desktop.Controls.Internal.DateViewer.DateViewerCell>().First(c => c.Model!.Kind == DateViewerCellType.Week);
            week.Activate();
            viewer.Value.ShouldBeNull();
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }
    static DateViewerTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Empty_Value_Browses_Without_Selecting_And_Realizes_Date_Cells()
    {
        var viewer = Create("DateViewer");
        Read<DateTime?>(viewer, "Value").ShouldBeNull();
        Set(viewer, "DisplayDate", new DateTime(2027, 3, 15));
        var window = new Avalonia.Controls.Window { Width = 360, Height = 360, Content = viewer };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs();
            viewer.GetVisualDescendants().OfType<DateViewerCell>().Count().ShouldBe(42);
            Read<DateTime?>(viewer, "Value").ShouldBeNull();
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void A_Real_Cell_Selection_Writes_The_Public_Value()
    {
        var viewer = Create("DateViewer");
        Set(viewer, "DisplayDate", new DateTime(2026, 7, 15));
        var window = new Avalonia.Controls.Window { Width = 360, Height = 360, Content = viewer };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs();
            var cell = viewer.GetVisualDescendants().OfType<DateViewerCell>().Single(c => c.Model?.Value == new DateTime(2026, 7, 20));
            cell.Activate(); Dispatcher.UIThread.RunJobs();
            Read<DateTime?>(viewer, "Value").ShouldBe(new DateTime(2026, 7, 20));
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Range_Uses_Two_Panels_With_One_Atomic_Public_Value()
    {
        var viewer = Create("RangeDateViewer");
        Set(viewer, "DisplayDate", new DateTime(2026, 7, 15));
        var window = new Avalonia.Controls.Window { Width = 600, Height = 360, Content = viewer };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs();
            var cells = viewer.GetVisualDescendants().OfType<DateViewerCell>().ToList();
            cells.Count.ShouldBe(84);
            cells.First(c => c.Model?.Value == new DateTime(2026, 7, 20)).Activate();
            Dispatcher.UIThread.RunJobs();
            Read<DateViewerRange?>(viewer, "Value").ShouldBe(new DateViewerRange(new DateTime(2026, 7, 20), null));
            cells = viewer.GetVisualDescendants().OfType<DateViewerCell>().ToList();
            cells.First(c => c.Model?.Value == new DateTime(2026, 7, 25)).Activate();
            Dispatcher.UIThread.RunJobs();
            Read<DateViewerRange?>(viewer, "Value").ShouldBe(new DateViewerRange(new DateTime(2026, 7, 20), new DateTime(2026, 7, 25)));
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Range_Dual_Panel_Does_Not_Select_Duplicate_Outside_Endpoint_Cells()
    {
        var start = new DateTime(2026, 9, 28);
        var end = new DateTime(2026, 10, 4);
        var viewer = new RangeDateViewer
        {
            DisplayDate = new DateTime(2026, 9, 1),
            Value = new DateViewerRange(start, end)
        };
        var window = new Avalonia.Controls.Window { Width = 600, Height = 360, Content = viewer };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var cells = viewer.GetVisualDescendants().OfType<DateViewerCell>().ToArray();
            var visibleStart = cells.Single(cell => cell.Model?.Value == start && cell.Model.IsInView);
            var visibleEnd = cells.Single(cell => cell.Model?.Value == end && cell.Model.IsInView);
            var duplicateStart = cells.Single(cell => cell.Model?.Value == start && cell.Model.IsInView == false);
            var duplicateEnd = cells.Single(cell => cell.Model?.Value == end && cell.Model.IsInView == false);

            visibleStart.Model!.IsSelected.ShouldBeTrue();
            visibleStart.Model.IsVisualEndpoint.ShouldBeTrue();
            visibleStart.Model.IsRangeStart.ShouldBeTrue();
            visibleEnd.Model!.IsSelected.ShouldBeTrue();
            visibleEnd.Model.IsVisualEndpoint.ShouldBeTrue();
            visibleEnd.Model.IsRangeEnd.ShouldBeTrue();

            duplicateStart.Model!.IsSelected.ShouldBeFalse();
            duplicateStart.Model.IsVisualEndpoint.ShouldBeFalse();
            duplicateStart.Model.IsRangeStart.ShouldBeFalse();
            duplicateStart.Model.IsRangeMiddle.ShouldBeFalse();
            duplicateStart.Model.IsRangeEnd.ShouldBeFalse();
            duplicateEnd.Model!.IsSelected.ShouldBeFalse();
            duplicateEnd.Model.IsVisualEndpoint.ShouldBeFalse();
            duplicateEnd.Model.IsRangeStart.ShouldBeFalse();
            duplicateEnd.Model.IsRangeMiddle.ShouldBeFalse();
            duplicateEnd.Model.IsRangeEnd.ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Header_Commands_Browse_And_Same_Value_Selection_Notifies_Once_Per_Activation()
    {
        var viewer = (DateViewer)Create("DateViewer");
        viewer.DisplayDate = new DateTime(2026, 7, 15);
        var changed = 0; var selected = 0;
        viewer.ValueChanged += (_, _) => changed++;
        viewer.Selected += (_, _) => selected++;
        var window = new Avalonia.Controls.Window { Width = 360, Height = 360, Content = viewer };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs();
            var header = typeof(DateViewer).GetProperty("HeaderContext", BindingFlags.Instance | BindingFlags.NonPublic);
            header.ShouldNotBeNull("default header must supply its strong command context");
            var context = (DateViewerHeaderContext)header!.GetValue(viewer)!;
            context.NavigateCommand.Execute(1);
            Dispatcher.UIThread.RunJobs();
            viewer.DisplayDate.Month.ShouldBe(8);
            viewer.Value.ShouldBeNull();
            var cell = viewer.GetVisualDescendants().OfType<DateViewerCell>().Single(c => c.Model?.Value == new DateTime(2026, 8, 20));
            cell.Activate(); cell.Activate(); Dispatcher.UIThread.RunJobs();
            changed.ShouldBe(1); selected.ShouldBe(2);
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Full_Cell_Template_Preserves_Disabled_Selection_And_Receives_Context()
    {
        var viewer = (DateViewer)Create("DateViewer");
        viewer.DisplayDate = new DateTime(2026, 7, 15);
        viewer.DisabledDate = date => date.Day == 20;
        viewer.FullCellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<DateViewerCellContext>((context, _) =>
            new TextBlock { Text = context!.DisplayValue });
        var window = new Avalonia.Controls.Window { Width = 360, Height = 360, Content = viewer };
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs();
            var cell = viewer.GetVisualDescendants().OfType<DateViewerCell>().Single(c => c.Model?.Value == new DateTime(2026, 7, 20));
            cell.Context.ShouldNotBeNull();
            cell.Activate(); Dispatcher.UIThread.RunJobs();
            viewer.Value.ShouldBeNull();
            var content = cell.GetVisualDescendants().OfType<ContentControl>().Single(c => c.Name == "PART_CellContent");
            content.IsVisible.ShouldBeTrue();
            content.ContentTemplate.ShouldBeSameAs(viewer.FullCellTemplate);
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [Fact]
    public void Secondary_Header_Uses_Edge_Navigation_And_Preserves_Its_Panel_When_Drilling_Up()
    {
        var viewer = new RangeDateViewer { DisplayDate = new DateTime(2026, 7, 15) };
        var primary = viewer.HeaderContext.ShouldNotBeNull();
        var secondary = viewer.SecondaryHeaderContext.ShouldNotBeNull();
        primary.ShowPreviousNavigation.ShouldBeTrue();
        primary.ShowNextNavigation.ShouldBeFalse();
        secondary.ShowPreviousNavigation.ShouldBeFalse();
        secondary.ShowNextNavigation.ShouldBeTrue();

        secondary.ChangePanelCommand.Execute(DateViewerPanelKind.Month);

        viewer.PanelKind.ShouldBe(DateViewerPanelKind.Month);
        viewer.PanelSession.Input.DisplayDate.ShouldBe(new DateTime(2025, 1, 1));
        viewer.PanelSession.Models[0].Anchor.ShouldBe(new DateTime(2025, 1, 1));
        viewer.PanelSession.Models[1].Anchor.ShouldBe(new DateTime(2026, 1, 1));
    }

    [Fact]
    public void Range_Header_Navigation_Refreshes_When_The_Host_Changes_Panel_Count()
    {
        var host = new Host { Input = new DatePanelInput { DisplayDate = new DateTime(2026, 7, 15), PanelCount = 2, IsRangeSelection = true } };
        var viewer = new RangeDateViewer();
        viewer.SetHost(host);
        viewer.HeaderContext!.ShowPreviousNavigation.ShouldBeTrue();
        viewer.HeaderContext.ShowNextNavigation.ShouldBeFalse();

        host.Input = host.Input with { PanelCount = 1 };
        viewer.RefreshHost();
        viewer.HeaderContext!.ShowPreviousNavigation.ShouldBeTrue();
        viewer.HeaderContext.ShowNextNavigation.ShouldBeTrue();

        host.Input = host.Input with { PanelCount = 2 };
        viewer.RefreshHost();
        viewer.HeaderContext!.ShowPreviousNavigation.ShouldBeTrue();
        viewer.HeaderContext.ShowNextNavigation.ShouldBeFalse();
    }

    private static TemplatedControl Create(string name)
    {
        var type = typeof(DatePicker).Assembly.GetType("AtomUI.Desktop.Controls." + name);
        type.ShouldNotBeNull($"public date viewer {name} must exist");
        return (TemplatedControl)Activator.CreateInstance(type!)!;
    }
    private static T Read<T>(object target, string name) => (T)target.GetType().GetProperty(name)!.GetValue(target)!;
    private static void Set(object target, string name, object? value) => target.GetType().GetProperty(name)!.SetValue(target, value);

    private sealed class Host : IDatePanelHost
    {
        internal DatePanelInput Input { get; set; } = new();
        public DatePanelInput ReadInput() => Input;
        public void NavigateTo(DateTime displayDate, DateViewerPanelKind panelKind) => Input = Input with { DisplayDate = displayDate, PanelKind = panelKind };
        public void Activate(DateCellSelection selection) { }
        public void Preview(DateTime? hoveredValue) { }
    }
}
