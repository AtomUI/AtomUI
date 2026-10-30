using AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Query;

public class DataGridQueryControllerTests
{
    private static readonly DataGridFieldId Age = new("age");

    static DataGridQueryControllerTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Structurally_Equal_Query_Is_A_True_NoOp_And_Event_Precedes_Request()
    {
        var source = Source();
        var (window, grid) = Show(source);
        try
        {
            Complete(source, grid, requestIndex: 0, "initial");
            var query = SortQuery(DataGridSortDirection.Ascending);
            var events = new List<DataGridQueryChangedEventArgs>();
            grid.QueryChanged += (_, args) =>
            {
                grid.Query.ShouldBe(args.NewQuery);
                grid.QueryRevision.ShouldBe(args.Revision);
                source.Requests.Count.ShouldBe(1);
                events.Add(args);
            };

            grid.Query = query;
            source.Requests.Count.ShouldBe(2);
            grid.Query = new DataGridQuery(query.Sorts, query.Filters, query.Groups);

            events.Count.ShouldBe(1);
            events[0].Reason.ShouldBe(DataGridQueryChangeReason.External);
            source.Requests.Count.ShouldBe(2);
            Complete(source, grid, requestIndex: 1, "sorted");
            grid.AppliedQuery.ShouldBe(query);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Reentrant_Query_Change_Invalidates_Outer_Request()
    {
        var source = Source();
        var (window, grid) = Show(source);
        try
        {
            Complete(source, grid, 0, "initial");
            var ascending = SortQuery(DataGridSortDirection.Ascending);
            var descending = SortQuery(DataGridSortDirection.Descending);
            var eventCount = 0;
            grid.QueryChanged += (_, args) =>
            {
                eventCount++;
                if (args.NewQuery == ascending)
                {
                    grid.Query = descending;
                }
            };

            grid.Query = ascending;

            grid.Query.ShouldBe(descending);
            eventCount.ShouldBe(2);
            source.Requests.Count.ShouldBe(2);
            source.RequestAt(1).Request.Query.ShouldBe(descending);
            Complete(source, grid, 1, "descending");
            grid.AppliedQuery.ShouldBe(descending);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Reload_Changes_Generation_Without_QueryChanged()
    {
        var source = Source();
        var (window, grid) = Show(source);
        try
        {
            Complete(source, grid, 0, "initial");
            var queryEventCount = 0;
            grid.QueryChanged += (_, _) => queryEventCount++;

            grid.Reload();

            source.Requests.Count.ShouldBe(2);
            source.RequestAt(1).Request.Query.ShouldBe(grid.Query);
            source.RequestAt(1).Request.DataGeneration
                  .ShouldBeGreaterThan(source.RequestAt(0).Request.DataGeneration);
            queryEventCount.ShouldBe(0);
            Complete(source, grid, 1, "reload");
            grid.LoadState.ShouldBe(DataGridLoadState.Ready);
        }
        finally
        {
            Close(window);
        }
    }

    [Fact]
    public void Failed_Query_Rolls_Back_Once_To_The_Applied_Query()
    {
        var source = Source();
        var (window, grid) = Show(source);
        try
        {
            Complete(source, grid, 0, "initial");
            var reasons = new List<DataGridQueryChangeReason>();
            grid.QueryChanged += (_, args) => reasons.Add(args.Reason);

            grid.Query = SortQuery(DataGridSortDirection.Ascending);
            source.Fail(1, new InvalidOperationException("query failed"));
            PumpUntil(() => grid.LoadState == DataGridLoadState.Error);

            grid.Query.ShouldBe(DataGridQuery.Empty);
            grid.AppliedQuery.ShouldBe(DataGridQuery.Empty);
            reasons.ShouldBe([
                DataGridQueryChangeReason.External,
                DataGridQueryChangeReason.LoadRollback]);
            grid.LoadError.ShouldBeOfType<InvalidOperationException>();
            grid.TotalItemCount.ShouldBe(128);
        }
        finally
        {
            Close(window);
        }
    }

    private static DataGridQuery SortQuery(DataGridSortDirection direction) =>
        DataGridQuery.Empty.WithSorts([new DataGridSort(Age, direction)]);

    private static ControllableDataGridSource Source() => new(new DataGridSourceSchema(
        typeof(Row),
        [new DataGridFieldSchema(
            Age,
            typeof(int),
            DataGridSortDirections.All,
            [],
            canGroup: false)],
        32,
        32))
    {
        HonorCancellation = true
    };

    private static (Window Window, global::AtomUI.Desktop.Controls.DataGrid Grid) Show(
        ControllableDataGridSource source)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = source,
            Width = 320,
            Height = 180
        };
        var window = new Window { Width = 400, Height = 240, Content = grid };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        source.WaitForRequestCount(1);
        return (window, grid);
    }

    private static void Complete(
        ControllableDataGridSource source,
        global::AtomUI.Desktop.Controls.DataGrid grid,
        int requestIndex,
        string snapshot)
    {
        source.Complete(requestIndex, ResultFor(source.RequestAt(requestIndex), snapshot));
        PumpUntil(() => grid.LoadState == DataGridLoadState.Ready);
    }

    private static DataGridRangeResult ResultFor(
        ControllableDataGridSource.PendingRequest pending,
        string snapshot)
    {
        var request = pending.Request;
        var count = Math.Min(request.Range.Count, 128 - request.Range.StartIndex);
        return new DataGridRangeResult(
            request.Range.StartIndex,
            [.. Enumerable.Range(0, count).Select(offset =>
                DataGridSourceEntry.CreateData(
                    DataGridRowKey.FromInt64(request.Range.StartIndex + offset),
                    new Row(request.Range.StartIndex + offset),
                    request.Range.StartIndex + offset,
                    request.Range.StartIndex + offset))],
            128,
            128,
            128,
            new DataGridSnapshotId(snapshot));
    }

    private static void PumpUntil(Func<bool> condition)
    {
        var completed = SpinWait.SpinUntil(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return condition();
        }, TimeSpan.FromSeconds(5));
        completed.ShouldBeTrue();
    }

    private static void Close(Window window)
    {
        window.Close();
        Dispatcher.UIThread.RunJobs();
    }

    private sealed record Row(int Age);
}
