using System.Collections.Immutable;
using AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization;

public class DataGridRangeVirtualizationTests
{
    private static readonly DataGridFieldId ValueField = new("value");

    static DataGridRangeVirtualizationTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Synchronous_Source_Commits_First_Range_Without_Publishing_Loading()
    {
        var source = new ImmediateRangeSource();
        var grid = Grid(source);
        var loadStates = new List<DataGridLoadState>();
        grid.PropertyChanged += (_, args) =>
        {
            if (args.Property == global::AtomUI.Desktop.Controls.DataGrid.LoadStateProperty)
            {
                loadStates.Add(grid.LoadState);
            }
        };
        var window = new Window { Width = 420, Height = 280, Content = grid };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            grid.LoadState.ShouldBe(DataGridLoadState.Ready);
            grid.EffectiveIsOperating.ShouldBeFalse();
            loadStates.ShouldBe([DataGridLoadState.Ready]);
            source.FetchCount.ShouldBeGreaterThanOrEqualTo(1);
            grid.DisplayData.RecycledRowCount.ShouldBe(0);
            grid.RangeViewportCommitCount.ShouldBe(1);
        }
        finally
        {
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }
    }

    [Fact]
    public void Pending_Viewport_Keeps_Committed_Rows_And_Layout_Performs_No_IO()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid, source);
        try
        {
            Complete(source, grid, requestIndex: 0, "stable");
            var committedRows = DisplayedRows(grid);
            committedRows.ShouldNotBeEmpty();
            committedRows.Select(static row => ((Row)row.DataContext!).Value)
                         .ShouldAllBe(static value => value < 32);

            grid.RequestRangeViewport(firstVisibleSlot: 320, visibleCount: 8, scrollDirection: 1);
            Dispatcher.UIThread.RunJobs();
            source.WaitForRequestCount(2);
            var requestCount = source.Requests.Count;

            MeasureAndArrange(grid);
            MeasureAndArrange(grid);

            source.Requests.Count.ShouldBe(requestCount);
            DisplayedRows(grid).Select(static row => ((Row)row.DataContext!).Value)
                               .ShouldBe(committedRows.Select(static row => ((Row)row.DataContext!).Value));
            DisplayedRows(grid).Select(static row => row.Slot).Distinct().Count()
                               .ShouldBe(DisplayedRows(grid).Count);

            Complete(source, grid, requestIndex: 1, "stable");

            var swappedRows = DisplayedRows(grid);
            swappedRows.ShouldNotBeEmpty();
            swappedRows.Select(static row => row.Slot).Distinct().Count()
                       .ShouldBe(swappedRows.Count);
            swappedRows.Select(static row => ((Row)row.DataContext!).Value)
                       .ShouldAllBe(static value => value >= 320 && value < 352);
            swappedRows.ShouldAllBe(static row =>
                row.RowKey == DataGridRowKey.FromInt64(((Row)row.DataContext!).Value + 1L));
            swappedRows.ShouldAllBe(static row => row.Index == ((Row)row.DataContext!).Value);
            swappedRows.ShouldAllBe(static row => row.DataIndex == ((Row)row.DataContext!).Value);

            committedRows.ShouldAllBe(static row => row.OwningGrid == null);
            committedRows.ShouldAllBe(static row => row.DataContext == null);
            committedRows.ShouldAllBe(static row => row.Slot == -1);
            committedRows.ShouldAllBe(static row => row.Index == -1);
            committedRows.ShouldAllBe(static row => !row.RowKey.IsValid);
            committedRows.ShouldAllBe(static row => row.DataIndex == -1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Thumb_Targets_Across_Dispatcher_Turns_Cancel_Obsolete_Loads_And_Commit_Only_Final_Rows()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid, source);
        try
        {
            Complete(source, grid, requestIndex: 0, "stable");
            var initialScrollMaximum = grid.VerticalScrollBar.ShouldNotBeNull().Maximum;
            var initialRows = DisplayedRows(grid);
            initialRows.ShouldNotBeEmpty();

            grid.RequestRangeViewport(firstVisibleSlot: 320, visibleCount: 8, scrollDirection: 1);
            Dispatcher.UIThread.RunJobs();
            source.WaitForRequestCount(2);
            grid.RequestRangeViewport(firstVisibleSlot: 640, visibleCount: 8, scrollDirection: 1);
            Dispatcher.UIThread.RunJobs();
            source.WaitForRequestCount(3);
            grid.RequestRangeViewport(firstVisibleSlot: 960, visibleCount: 8, scrollDirection: 1);
            Dispatcher.UIThread.RunJobs();
            source.WaitForRequestCount(4);

            source.RequestAt(1).IsCancellationRequested.ShouldBeTrue();
            source.RequestAt(2).IsCancellationRequested.ShouldBeTrue();
            source.RequestAt(3).Request.Range.StartIndex.ShouldBe(960);
            DisplayedRows(grid).Select(static row => ((Row)row.DataContext!).Value)
                               .ShouldBe(initialRows.Select(static row => ((Row)row.DataContext!).Value));

            Complete(source, grid, requestIndex: 3, "stable");

            grid.DisplayData.FirstScrollingSlot.ShouldBe(960);
            DisplayedRows(grid).ShouldAllBe(static row =>
                ((Row)row.DataContext!).Value >= 960 &&
                ((Row)row.DataContext!).Value < 992);
            grid.VerticalScrollBar!.Maximum.ShouldBe(initialScrollMaximum, tolerance: 0.1);
            source.MaximumObservedConcurrency.ShouldBeLessThanOrEqualTo(2);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Wheel_And_Thumb_Queue_Range_Loads_And_Release_The_Outer_Chain_At_Boundaries()
    {
        var source = Source();
        var grid = Grid(source);
        var window = Show(grid, source);
        try
        {
            Complete(source, grid, requestIndex: 0, "stable");

            grid.UpdateScroll(new Vector(0, 48)).ShouldBeFalse();
            var crossBlockJump = grid.GetRangeOffset(32);
            grid.UpdateScroll(new Vector(0, -crossBlockJump)).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();
            PumpUntil(() => source.Requests.Count >= 2);
            DisplayedRows(grid).ShouldAllBe(static row => row.Slot < 32);

            Complete(source, grid, requestIndex: 1, "stable");
            grid.DisplayData.FirstScrollingSlot.ShouldBeGreaterThanOrEqualTo(32);

            var scrollBar = grid.VerticalScrollBar.ShouldNotBeNull();
            scrollBar.Maximum.ShouldBeGreaterThan(0);
            scrollBar.Value = scrollBar.Maximum;
            grid.ProcessVerticalScroll(ScrollEventType.ThumbTrack);
            Dispatcher.UIThread.RunJobs();
            PumpUntil(() => source.Requests.Count >= 3);

            Complete(source, grid, requestIndex: 2, "stable");
            grid.VerticalOffset.ShouldBe(scrollBar.Maximum, tolerance: 0.1);
            grid.UpdateScroll(new Vector(0, -48)).ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Ten_Thousand_Viewport_Swaps_Keep_Containers_Cache_And_Heights_Bounded()
    {
        var source = new ImmediateRangeSource();
        var grid = Grid(source);
        var window = new Window { Width = 420, Height = 280, Content = grid };
        window.Show();
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
        try
        {
            var visibleLimit = grid.DisplayData.NumDisplayedScrollingElements;
            visibleLimit.ShouldBeGreaterThan(0);

            grid.RequestRangeViewport(320, visibleLimit, 1);
            PumpUntil(() => grid.DisplayData.FirstScrollingSlot == 320);
            grid.DisplayData.LastScrollingSlot.ShouldBeLessThan(320 + visibleLimit);
            var poolBaseline = grid.DisplayData.RecycledRowCount;
            var commitBaseline = grid.RangeViewportCommitCount;

            for (var iteration = 0; iteration < 10_000; iteration++)
            {
                var first = (iteration & 1) == 0 ? 0 : 320;
                grid.RequestRangeViewport(first, visibleLimit, first == 0 ? -1 : 1);
            }

            Dispatcher.UIThread.RunJobs();
            grid.LoadState.ShouldBe(DataGridLoadState.Ready);
            grid.HasPendingRangeViewport.ShouldBeFalse();
            grid.DisplayData.FirstScrollingSlot.ShouldBe(320);
            (grid.RangeViewportCommitCount - commitBaseline).ShouldBeLessThanOrEqualTo(1);
            source.FetchCount.ShouldBe(3);
            source.InvalidatedSubscriberCount.ShouldBe(1);
            grid.RowsPresenter!.Children.OfType<DataGridRow>().Count()
                .ShouldBeLessThanOrEqualTo(visibleLimit);
            grid.DisplayData.RecycledRowCount.ShouldBe(poolBaseline);
            grid.RangeCachePinCount.ShouldBeLessThanOrEqualTo(2);
            grid.RangeMeasuredHeightCount.ShouldBeLessThanOrEqualTo(visibleLimit * 2);
        }
        finally
        {
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }

        source.InvalidatedSubscriberCount.ShouldBe(0);
        grid.RangeCachePinCount.ShouldBe(0);
        grid.RangeMeasuredHeightCount.ShouldBe(0);
    }

    private static ControllableDataGridSource Source() => new(new DataGridSourceSchema(
        typeof(Row),
        [new DataGridFieldSchema(
            ValueField,
            typeof(int),
            DataGridSortDirections.All,
            [],
            canGroup: false)],
        preferredRangeSize: 32,
        maximumRangeSize: 32))
    {
        HonorCancellation = true
    };

    private static global::AtomUI.Desktop.Controls.DataGrid Grid(IDataGridSource source)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = source,
            Width = 360,
            Height = 220
        };
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Value",
            Binding = new Binding(nameof(Row.Value))
        });
        return grid;
    }

    private static Window Show(
        global::AtomUI.Desktop.Controls.DataGrid grid,
        ControllableDataGridSource source)
    {
        var window = new Window { Width = 420, Height = 280, Content = grid };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        source.WaitForRequestCount(1);
        return window;
    }

    private static void Complete(
        ControllableDataGridSource source,
        global::AtomUI.Desktop.Controls.DataGrid grid,
        int requestIndex,
        string snapshot)
    {
        source.Complete(requestIndex, ResultFor(source.RequestAt(requestIndex), snapshot));
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
        Dispatcher.UIThread.RunJobs();
    }

    private static DataGridRangeResult ResultFor(
        ControllableDataGridSource.PendingRequest pending,
        string snapshot) => ResultFor(pending.Request, snapshot);

    private static DataGridRangeResult ResultFor(
        DataGridFetchRequest request,
        string snapshot)
    {
        const int total = 1024;
        var count = Math.Min(request.Range.Count, total - request.Range.StartIndex);
        var entries = ImmutableArray.CreateBuilder<DataGridSourceEntry>(count);
        for (var offset = 0; offset < count; offset++)
        {
            var index = request.Range.StartIndex + offset;
            entries.Add(DataGridSourceEntry.CreateData(
                DataGridRowKey.FromInt64(index + 1L),
                new Row(index),
                index,
                index));
        }
        return new DataGridRangeResult(
            request.Range.StartIndex,
            entries.MoveToImmutable(),
            total,
            total,
            total,
            new DataGridSnapshotId(snapshot));
    }

    private static List<DataGridRow> DisplayedRows(
        global::AtomUI.Desktop.Controls.DataGrid grid) =>
        grid.DisplayData.GetScrollingRows()
            .Cast<DataGridRow>()
            .OrderBy(static row => row.Slot)
            .ToList();

    private static void MeasureAndArrange(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        grid.Measure(new Size(360, 220));
        grid.Arrange(new Rect(0, 0, 360, 220));
        Dispatcher.UIThread.RunJobs();
    }

    private static void PumpUntil(Func<bool> condition)
    {
        SpinWait.SpinUntil(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return condition();
        }, TimeSpan.FromSeconds(5)).ShouldBeTrue();
    }

    private sealed class ImmediateRangeSource : IDataGridSource
    {
        private EventHandler? _invalidated;

        public DataGridSourceSchema Schema { get; } = new(
            typeof(Row),
            [new DataGridFieldSchema(
                ValueField,
                typeof(int),
                DataGridSortDirections.All,
                [],
                canGroup: false)],
            preferredRangeSize: 32,
            maximumRangeSize: 32);

        public int FetchCount { get; private set; }

        public int InvalidatedSubscriberCount =>
            _invalidated?.GetInvocationList().Length ?? 0;

        public event EventHandler? Invalidated
        {
            add => _invalidated += value;
            remove => _invalidated -= value;
        }

        public ValueTask<DataGridRangeResult> FetchAsync(
            DataGridFetchRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FetchCount++;
            return new ValueTask<DataGridRangeResult>(ResultFor(request, "stable"));
        }
    }

    private sealed record Row(int Value);
}
