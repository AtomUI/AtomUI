using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization;

public class DataGridIssue521Tests
{
    private const int RowCount = 200;

    static DataGridIssue521Tests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Taller_Viewport_After_Scrolling_To_End_Clamps_Vertical_Offset_To_New_Maximum()
    {
        using var source = Source();
        var grid = Grid(source);
        var window = Show(grid, 640, 700);

        try
        {
            ScrollToEnd(grid);

            window.Width = 1700;
            window.Height = 1000;
            PumpUntil(() => !grid.HasPendingRangeViewport && grid.LoadState == DataGridLoadState.Ready);
            RunIdleJobs();

            var scrollBar = grid.VerticalScrollBar.ShouldNotBeNull();
            scrollBar.Value.ShouldBe(scrollBar.Maximum, tolerance: 0.5);
            grid.VerticalOffset.ShouldBe(scrollBar.Maximum, tolerance: 0.5);
            grid.DisplayData.LastScrollingSlot.ShouldBe(RowCount - 1);
            grid.DisplayData.FirstScrollingSlot.ShouldBeGreaterThanOrEqualTo(0);
            grid.VerticalOffset.ShouldBe(
                grid.GetRangeOffset(grid.DisplayData.FirstScrollingSlot) + grid.NegVerticalOffset,
                tolerance: 0.5);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Wider_Viewport_With_Right_Frozen_Column_Resets_Horizontal_Offset_When_Scroll_Bar_Hides()
    {
        using var source = Source();
        var grid = Grid(source);
        var window = Show(grid, 640, 700);

        try
        {
            var hScrollBar = grid.HorizontalScrollBar.ShouldNotBeNull();
            hScrollBar.IsVisible.ShouldBeTrue();
            hScrollBar.Value = hScrollBar.Maximum;
            grid.ProcessHorizontalScroll(ScrollEventType.ThumbTrack);
            RunIdleJobs();
            grid.HorizontalOffset.ShouldBeGreaterThan(0);

            window.Width = 1700;
            RunIdleJobs();

            hScrollBar.IsVisible.ShouldBeFalse();
            grid.HorizontalOffset.ShouldBe(0, tolerance: 0.01);
        }
        finally
        {
            Close(window);
        }
    }

    private static void ScrollToEnd(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var scrollBar = grid.VerticalScrollBar.ShouldNotBeNull();
        scrollBar.Value = scrollBar.Maximum;
        grid.ProcessVerticalScroll(ScrollEventType.ThumbTrack);
        PumpUntil(() =>
            grid.LoadState == DataGridLoadState.Ready &&
            !grid.HasPendingRangeViewport &&
            grid.DisplayData.LastScrollingSlot == RowCount - 1);
        RunIdleJobs();
    }

    private static DataGridLocalSource<Row> Source() =>
        DataGridLocalSource.Create(
            Enumerable.Range(0, RowCount).Select(static id => new Row(id, Guid.NewGuid().ToString())).ToArray(),
            DataGridLocalSourceDescriptor.For<Row>(static row => DataGridRowKey.FromString(row.Key)));

    private static global::AtomUI.Desktop.Controls.DataGrid Grid(IDataGridSource source)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            IsMotionEnabled = false,
            IsReadOnly = true,
            LeftFrozenColumnCount = 1,
            RightFrozenColumnCount = 1,
            ItemsSource = source
        };
        for (var i = 0; i < 6; i++)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = $"C{i}",
                Binding = new Binding(nameof(Row.Id)),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                MinWidth = 120
            });
        }
        return grid;
    }

    private static AvaloniaWindow Show(global::AtomUI.Desktop.Controls.DataGrid grid, double width, double height)
    {
        var window = new AvaloniaWindow
        {
            Width = width,
            Height = height,
            Content = grid
        };
        window.Show();
        PumpUntil(() =>
            grid.LoadState == DataGridLoadState.Ready &&
            grid.SlotCount == RowCount &&
            grid.DisplayData.NumDisplayedScrollingElements > 0);
        return window;
    }

    private static void RunIdleJobs()
    {
        for (var i = 0; i < 10; i++)
        {
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void PumpUntil(Func<bool> condition)
    {
        SpinWait.SpinUntil(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return condition();
        }, TimeSpan.FromSeconds(5)).ShouldBeTrue();
    }

    private static void Close(AvaloniaWindow window)
    {
        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        window.Close();
    }

    private sealed record Row(int Id, string Key);
}
