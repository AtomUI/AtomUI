using AtomUI.Desktop.Controls.Internal.DateViewer;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DatePickers;

public class DateViewerLifecycleTests
{
    static DateViewerLifecycleTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Detach_Unbinds_Realized_Cells_And_Reattach_Rebuilds_The_Public_Viewer()
    {
        var viewer = new DateViewer { DisplayDate = new DateTime(2026, 7, 15) };
        var window = new AvaloniaWindow
        {
            Width = 420,
            Height = 360,
            Content = viewer
        };

        try
        {
            window.Show();
            Drain();

            var cells = viewer.GetVisualDescendants().OfType<DateViewerCell>().ToArray();
            cells.Length.ShouldBe(42);
            cells.ShouldAllBe(cell => cell.Model != null && cell.Session != null);

            window.Content = null;
            Drain();

            cells.ShouldAllBe(cell => cell.Model == null && cell.Session == null);

            window.Content = viewer;
            Drain();

            viewer.GetVisualDescendants().OfType<DateViewerCell>().Count().ShouldBe(42);
            viewer.GetVisualDescendants()
                  .OfType<DateViewerCell>()
                  .ShouldAllBe(cell => cell.Model != null && cell.Session != null);
        }
        finally
        {
            window.Close();
            Drain();
        }
    }

    [Fact]
    public void Pointer_Exit_To_Another_Window_Clears_The_Public_Viewer_Hover()
    {
        var targetDate = new DateTime(2026, 7, 15);
        var viewer = new DateViewer { DisplayDate = targetDate };
        var window = new AvaloniaWindow
        {
            Width = 420,
            Height = 360,
            Content = viewer
        };
        var other = new AvaloniaWindow
        {
            Width = 420,
            Height = 360,
            Content = new Border()
        };

        try
        {
            window.Show();
            other.Show();
            Drain();

            var cell = viewer.GetVisualDescendants()
                             .OfType<DateViewerCell>()
                             .Single(candidate => candidate.Model?.Value == targetDate);
            var point = cell.TranslatePoint(
                                new Point(cell.Bounds.Width / 2, cell.Bounds.Height / 2),
                                window)
                            .ShouldNotBeNull();

            window.MouseMove(point);
            Drain();
            viewer.HoveredValue.ShouldBe(targetDate);

            other.MouseMove(new Point(10, 10));
            Drain();
            viewer.HoveredValue.ShouldBeNull();
        }
        finally
        {
            other.Close();
            window.Close();
            Drain();
        }
    }

    private static void Drain() => Dispatcher.UIThread.RunJobs();
}
