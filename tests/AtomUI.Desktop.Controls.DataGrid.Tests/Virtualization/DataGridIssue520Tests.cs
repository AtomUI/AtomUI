using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization;

public class DataGridIssue520Tests
{
    private const int RowCount = 3000;

    static DataGridIssue520Tests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Clear_Then_AddRange_After_Scrolling_Down_Starts_New_Data_From_Top()
    {
        var rows = new AvaloniaList<Row>(Rows(0));
        using var source = Source(rows);
        var grid = Grid(source);
        var window = Show(grid);

        try
        {
            ScrollToMiddle(grid);

            rows.Clear();
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready && grid.SlotCount == 0);
            var commitBaseline = grid.RangeViewportCommitCount;

            rows.AddRange(Rows(RowCount));
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready && grid.SlotCount == RowCount);
            RunIdleJobs();

            grid.VerticalOffset.ShouldBe(0, tolerance: 0.1);
            grid.VerticalScrollBar.ShouldNotBeNull().Value.ShouldBe(0, tolerance: 0.1);
            grid.DisplayData.FirstScrollingSlot.ShouldBe(0);
            AssertDisplayedRowsMatchOffset(grid);
            (grid.RangeViewportCommitCount - commitBaseline).ShouldBeLessThanOrEqualTo(2);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Reset_To_New_Row_Keys_After_Scrolling_Down_Keeps_Displayed_Rows_At_Offset()
    {
        var rows = new ResettableCollection<Row>(Rows(0));
        using var source = Source(rows);
        var grid = Grid(source);
        var window = Show(grid);

        try
        {
            ScrollToMiddle(grid);
            var commitBaseline = grid.RangeViewportCommitCount;

            rows.Reset(Rows(RowCount));
            PumpUntil(() => grid.LoadState == DataGridLoadState.Ready && !grid.IsDataStale);
            RunIdleJobs();

            AssertDisplayedRowsMatchOffset(grid);
            (grid.RangeViewportCommitCount - commitBaseline).ShouldBeLessThanOrEqualTo(2);
        }
        finally
        {
            Close(window);
        }
    }

    private static void ScrollToMiddle(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var scrollBar = grid.VerticalScrollBar.ShouldNotBeNull();
        var target = scrollBar.Maximum / 2;
        scrollBar.Value = target;
        grid.ProcessVerticalScroll(ScrollEventType.ThumbTrack);
        PumpUntil(() =>
            grid.LoadState == DataGridLoadState.Ready &&
            grid.VerticalOffset > target / 2 &&
            grid.DisplayData.FirstScrollingSlot > 0);
        AssertDisplayedRowsMatchOffset(grid);
    }

    private static void AssertDisplayedRowsMatchOffset(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var firstSlot = grid.DisplayData.FirstScrollingSlot;
        firstSlot.ShouldBeGreaterThanOrEqualTo(0);
        grid.DisplayData.NumDisplayedScrollingElements.ShouldBeGreaterThan(0);
        grid.NegVerticalOffset.ShouldBeLessThan(grid.GetRangeHeight(firstSlot));
        grid.VerticalOffset.ShouldBe(
            grid.GetRangeOffset(firstSlot) + grid.NegVerticalOffset,
            tolerance: 0.5);
    }

    private static DataGridLocalSource<Row> Source(IReadOnlyList<Row> rows) =>
        DataGridLocalSource.Create(
            rows,
            DataGridLocalSourceDescriptor.For<Row>(static row => DataGridRowKey.FromString(row.Key)));

    private static global::AtomUI.Desktop.Controls.DataGrid Grid(IDataGridSource source)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            IsMotionEnabled = false,
            ItemsSource = source,
            RowHeight = 55,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Width = 420,
            Height = 400
        };
        grid.Columns.Add(new DataGridTemplateColumn
        {
            Header = "Value",
            CellTemplate = new FuncDataTemplate<Row>((row, _) => new TextBlock { Text = row?.Id.ToString() })
        });
        return grid;
    }

    private static AvaloniaWindow Show(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var window = new AvaloniaWindow
        {
            Width = 500,
            Height = 480,
            Content = grid
        };
        window.Show();
        PumpUntil(() =>
            grid.LoadState == DataGridLoadState.Ready &&
            grid.SlotCount == RowCount &&
            grid.DisplayData.NumDisplayedScrollingElements > 0);
        return window;
    }

    // Each row gets a fresh key so the reloaded data shares no identity with the old data.
    private static Row[] Rows(int startId) => Enumerable.Range(startId, RowCount)
        .Select(static id => new Row(id, Guid.NewGuid().ToString()))
        .ToArray();

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

    private sealed class ResettableCollection<T>(IEnumerable<T> items) : ObservableCollection<T>(items)
    {
        public void Reset(IEnumerable<T> items)
        {
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
            OnPropertyChanged(new(nameof(Count)));
            OnPropertyChanged(new("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
