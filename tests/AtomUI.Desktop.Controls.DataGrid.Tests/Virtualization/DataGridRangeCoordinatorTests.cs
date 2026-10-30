using AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Virtualization;

public class DataGridRangeCoordinatorTests
{
    private static readonly DataGridFieldId Age = new("age");

    [Fact]
    public void Slow_Old_Query_Cannot_Overwrite_Newer_Snapshot()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var first = coordinator.BeginGeneration(
            source, Query(DataGridSortDirection.Ascending), null,
            DataGridGroupExpansion.AllExpanded, fallback: null);
        var oldLoad = EnsureViewport(coordinator, first, Viewport(0, 20));
        var second = coordinator.BeginGeneration(
            source, Query(DataGridSortDirection.Descending), null,
            DataGridGroupExpansion.AllExpanded, fallback: null);
        var newLoad = EnsureViewport(coordinator, second, Viewport(0, 20));

        source.Complete(1, CoordinatorFixtures.ResultFor(
            source.RequestAt(1), "s2", firstKey: 100));
        Await(newLoad).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);
        source.CompleteIgnoringCancellation(0, CoordinatorFixtures.ResultFor(
            source.RequestAt(0), "s1", firstKey: 1));
        Await(oldLoad).Kind.ShouldBe(DataGridPresentationTransitionKind.Superseded);

        coordinator.AppliedSnapshot!.Query.ShouldBe(second.Query);
        coordinator.AppliedSnapshot.EntryAt(0).RowKey
                   .ShouldBe(DataGridRowKey.FromInt64(100));
        coordinator.StaleCommitCount.ShouldBe(0);
    }

    [Fact]
    public void Old_Source_Result_Cannot_Commit_After_Source_Replacement()
    {
        var oldSource = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        var newSource = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var oldGeneration = coordinator.BeginGeneration(
            oldSource, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var oldLoad = EnsureViewport(coordinator, oldGeneration, Viewport(0, 20));
        var newGeneration = coordinator.BeginGeneration(
            newSource, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var newLoad = EnsureViewport(coordinator, newGeneration, Viewport(0, 20));

        newSource.Complete(0, CoordinatorFixtures.ResultFor(
            newSource.RequestAt(0), "new", firstKey: 500));
        Await(newLoad);
        oldSource.CompleteIgnoringCancellation(0, CoordinatorFixtures.ResultFor(
            oldSource.RequestAt(0), "old", firstKey: 1));
        Await(oldLoad).Kind.ShouldBe(DataGridPresentationTransitionKind.Superseded);

        coordinator.AppliedSnapshot!.EntryAt(0).RowKey
                   .ShouldBe(DataGridRowKey.FromInt64(500));
    }

    [Fact]
    public void Bootstrap_Issues_One_Request_Then_Bounds_Concurrency_To_Two()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);

        var load = EnsureViewport(coordinator, generation, Viewport(0, 96));
        source.Requests.Count.ShouldBe(1);
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "s1"));
        source.WaitForRequestCount(3);
        source.ActiveRequestCount.ShouldBe(2);
        source.Complete(1, CoordinatorFixtures.ResultFor(source.RequestAt(1), "s1"));
        source.Complete(2, CoordinatorFixtures.ResultFor(source.RequestAt(2), "s1"));

        Await(load).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);
        source.MaximumObservedConcurrency.ShouldBeLessThanOrEqualTo(2);
        coordinator.AppliedSnapshot!.Blocks.Length.ShouldBe(3);
    }

    [Fact]
    public void Prefetch_Warms_One_Viewport_Each_Side_Without_Changing_Presentation()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var visible = new DataGridDesiredViewport(32, 32, 1);
        var load = EnsureViewport(coordinator, generation, visible);
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "s1"));
        var committed = Await(load).Snapshot.ShouldNotBeNull();

        var prefetch = Prefetch(coordinator, generation, visible);
        source.WaitForRequestCount(3);
        source.RequestAt(1).Request.Range.StartIndex.ShouldBe(64);
        source.RequestAt(2).Request.Range.StartIndex.ShouldBe(0);
        source.MaximumObservedConcurrency.ShouldBeLessThanOrEqualTo(2);
        source.Complete(1, CoordinatorFixtures.ResultFor(source.RequestAt(1), "s1"));
        source.Complete(2, CoordinatorFixtures.ResultFor(source.RequestAt(2), "s1"));
        Await(prefetch);

        coordinator.AppliedSnapshot.ShouldBeSameAs(committed);
        coordinator.CacheCount.ShouldBe(3);
        Await(Prefetch(coordinator, generation, visible));
        source.Requests.Count.ShouldBe(3);
    }

    [Fact]
    public void Overlapping_Viewport_Requests_DeDuplicate_The_Same_Block()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);

        var first = EnsureViewport(coordinator, generation, Viewport(0, 20));
        var second = EnsureViewport(coordinator, generation, Viewport(0, 20));
        source.Requests.Count.ShouldBe(1);
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "s1"));

        Await(second).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);
        Await(first).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);
        source.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public void New_Viewport_Retains_Shared_Block_Before_Canceling_Obsolete_Block()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema())
        {
            HonorCancellation = true
        };
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);

        var initial = EnsureViewport(coordinator, generation, Viewport(0, 20));
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "s1", total: 256));
        Await(initial).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);

        var first = EnsureViewport(coordinator, generation, Viewport(32, 64));
        source.WaitForRequestCount(3);
        var latest = EnsureViewport(coordinator, generation, Viewport(64, 32));

        source.RequestAt(1).Request.Range.StartIndex.ShouldBe(32);
        source.RequestAt(1).IsCancellationRequested.ShouldBeTrue();
        source.RequestAt(2).Request.Range.StartIndex.ShouldBe(64);
        source.RequestAt(2).IsCancellationRequested.ShouldBeFalse();
        source.Complete(2, CoordinatorFixtures.ResultFor(source.RequestAt(2), "s1", total: 256));

        Await(latest).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);
        Await(first).Kind.ShouldBe(DataGridPresentationTransitionKind.Superseded);
        source.Requests.Count.ShouldBe(3);
        coordinator.AppliedSnapshot!.CommittedViewport.FirstVisibleIndex.ShouldBe(64);
    }

    [Fact]
    public void Latest_Viewport_Cancels_Obsolete_Source_Work_Before_It_Queues()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema())
        {
            HonorCancellation = true
        };
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);

        var initial = EnsureViewport(coordinator, generation, Viewport(0, 20));
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "s1"));
        Await(initial).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);

        var first = EnsureViewport(coordinator, generation, Viewport(32, 20));
        source.WaitForRequestCount(2);
        var second = EnsureViewport(coordinator, generation, Viewport(64, 20));
        source.WaitForRequestCount(3);
        var latest = EnsureViewport(coordinator, generation, Viewport(96, 20));

        SpinWait.SpinUntil(() => source.Requests.Count >= 4, TimeSpan.FromMilliseconds(250))
            .ShouldBeTrue("the final viewport must not wait behind obsolete visible requests");
        source.RequestAt(1).IsCancellationRequested.ShouldBeTrue();
        source.RequestAt(2).IsCancellationRequested.ShouldBeTrue();
        source.RequestAt(3).Request.Range.StartIndex.ShouldBe(96);

        source.Complete(3, CoordinatorFixtures.ResultFor(source.RequestAt(3), "s1"));

        Await(latest).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);
        Await(second).Kind.ShouldBe(DataGridPresentationTransitionKind.Superseded);
        Await(first).Kind.ShouldBe(DataGridPresentationTransitionKind.Superseded);
        SpinWait.SpinUntil(() => coordinator.ActiveRequestCount == 0, TimeSpan.FromSeconds(1))
            .ShouldBeTrue();
        coordinator.InFlightBlockCount.ShouldBe(0);
    }

    [Fact]
    public void New_Visible_Viewport_Cancels_Old_Prefetch_Before_Requesting_Final_Target()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema())
        {
            HonorCancellation = true
        };
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var visible = Viewport(64, 32);
        var initial = EnsureViewport(coordinator, generation, visible);
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "s1", total: 256));
        Await(initial).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);

        var prefetch = Prefetch(coordinator, generation, visible);
        source.WaitForRequestCount(3);
        var latest = EnsureViewport(coordinator, generation, Viewport(128, 32));

        source.WaitForRequestCount(4);
        source.RequestAt(1).IsCancellationRequested.ShouldBeTrue();
        source.RequestAt(2).IsCancellationRequested.ShouldBeTrue();
        source.RequestAt(3).Request.Range.StartIndex.ShouldBe(128);
        source.Complete(3, CoordinatorFixtures.ResultFor(source.RequestAt(3), "s1", total: 256));

        Await(latest).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);
        Should.Throw<OperationCanceledException>(() => Await(prefetch));
        coordinator.LastPrefetchError.ShouldBeNull();
    }

    [Fact]
    public void Prefetch_Does_Not_Enter_Scheduler_Before_Visible_Viewport_Commits()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema())
        {
            HonorCancellation = true
        };
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var initial = EnsureViewport(coordinator, generation, Viewport(0, 20));
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "s1", total: 256));
        Await(initial).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);

        var visible = Viewport(64, 32);
        var load = EnsureViewport(coordinator, generation, visible);
        source.WaitForRequestCount(2);

        Await(Prefetch(coordinator, generation, visible));
        source.Requests.Count.ShouldBe(2);

        source.Complete(1, CoordinatorFixtures.ResultFor(source.RequestAt(1), "s1", total: 256));
        Await(load).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);
    }

    [Fact]
    public void Obsolete_Result_From_Source_That_Ignores_Cancellation_Cannot_Pollute_Cache()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);

        var initial = EnsureViewport(coordinator, generation, Viewport(0, 20));
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "s1", total: 256));
        Await(initial).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);

        var first = EnsureViewport(coordinator, generation, Viewport(32, 20));
        source.WaitForRequestCount(2);
        var second = EnsureViewport(coordinator, generation, Viewport(64, 20));
        source.WaitForRequestCount(3);
        var latest = EnsureViewport(coordinator, generation, Viewport(96, 20));

        source.RequestAt(1).IsCancellationRequested.ShouldBeTrue();
        source.RequestAt(2).IsCancellationRequested.ShouldBeTrue();
        source.Requests.Count.ShouldBe(3);
        source.CompleteIgnoringCancellation(
            1,
            CoordinatorFixtures.ResultFor(source.RequestAt(1), "s1", total: 256));
        source.WaitForRequestCount(4);
        source.CompleteIgnoringCancellation(
            2,
            CoordinatorFixtures.ResultFor(source.RequestAt(2), "s1", total: 256));
        source.Complete(3, CoordinatorFixtures.ResultFor(source.RequestAt(3), "s1", total: 256));

        Await(latest).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);
        Await(second).Kind.ShouldBe(DataGridPresentationTransitionKind.Superseded);
        Await(first).Kind.ShouldBe(DataGridPresentationTransitionKind.Superseded);
        SpinWait.SpinUntil(() => coordinator.TrackedRequestCount == 0, TimeSpan.FromSeconds(1))
            .ShouldBeTrue();
        coordinator.CacheCount.ShouldBe(2);
        coordinator.LastError.ShouldBeNull();
        coordinator.AppliedSnapshot!.CommittedViewport.FirstVisibleIndex.ShouldBe(96);
    }

    [Fact]
    public void Snapshot_Mismatch_Leaves_No_Partial_Presentation()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var load = EnsureViewport(coordinator, generation, Viewport(0, 64));
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "s1"));
        source.WaitForRequestCount(2);

        source.Complete(1, CoordinatorFixtures.ResultFor(source.RequestAt(1), "s2"));
        var transition = Await(load);

        transition.Kind.ShouldBe(DataGridPresentationTransitionKind.Failed);
        transition.Error.ShouldBeOfType<DataGridSourceContractException>();
        coordinator.AppliedSnapshot.ShouldBeNull();
        coordinator.CachePinCount.ShouldBe(0);
    }

    [Fact]
    public void Snapshot_Expiry_Restarts_Once_With_Null_Expected_Snapshot()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var load = EnsureViewport(coordinator, generation, Viewport(0, 20));

        source.Fail(0, new DataGridSnapshotExpiredException("expired"));
        source.WaitForRequestCount(2);
        source.RequestAt(1).Request.ExpectedSnapshot.ShouldBeNull();
        source.RequestAt(1).Request.DataGeneration.ShouldBe(generation.DataGeneration + 1);
        source.Complete(1, CoordinatorFixtures.ResultFor(source.RequestAt(1), "s2"));

        Await(load).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);
        coordinator.AppliedSnapshot!.Snapshot.ShouldBe(new DataGridSnapshotId("s2"));
        coordinator.SnapshotExpiryRestartCount.ShouldBe(1);
    }

    [Fact]
    public void Second_Snapshot_Expiry_Is_A_Terminal_Error()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var load = EnsureViewport(coordinator, generation, Viewport(0, 20));

        source.Fail(0, new DataGridSnapshotExpiredException("first"));
        source.WaitForRequestCount(2);
        source.Fail(1, new DataGridSnapshotExpiredException("second"));
        var transition = Await(load);

        transition.Kind.ShouldBe(DataGridPresentationTransitionKind.Failed);
        transition.Error.ShouldBeOfType<DataGridSnapshotExpiredException>();
        coordinator.AppliedSnapshot.ShouldBeNull();
        coordinator.SnapshotExpiryRestartCount.ShouldBe(1);
    }

    [Fact]
    public void MultiBlock_Failure_Preserves_Complete_Fallback()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var initial = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var initialLoad = EnsureViewport(coordinator, initial, Viewport(0, 20));
        source.Complete(0, CoordinatorFixtures.ResultFor(
            source.RequestAt(0), "base", firstKey: 1));
        Await(initialLoad);
        var fallback = coordinator.AppliedSnapshot;

        var refresh = coordinator.BeginGeneration(
            source, Query(DataGridSortDirection.Ascending), null,
            DataGridGroupExpansion.AllExpanded, fallback);
        var refreshLoad = EnsureViewport(coordinator, refresh, Viewport(32, 64));
        source.Complete(1, CoordinatorFixtures.ResultFor(
            source.RequestAt(1), "refresh", firstKey: 100));
        source.WaitForRequestCount(3);
        source.Fail(2, new InvalidOperationException("failed block"));

        var transition = Await(refreshLoad);
        transition.Kind.ShouldBe(DataGridPresentationTransitionKind.RolledBack);
        transition.Snapshot.ShouldBeSameAs(fallback);
        coordinator.AppliedSnapshot.ShouldBeSameAs(fallback);
        coordinator.AppliedSnapshot!.EntryAt(0).RowKey
                   .ShouldBe(DataGridRowKey.FromInt64(1));
    }

    [Fact]
    public void Dispose_Cancels_Active_Request_And_Releases_Pins()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema())
        {
            HonorCancellation = true
        };
        var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var load = EnsureViewport(coordinator, generation, Viewport(0, 20));
        source.RequestAt(0).IsCancellationRequested.ShouldBeFalse();

        coordinator.Dispose();

        Await(load).Kind.ShouldBe(DataGridPresentationTransitionKind.Superseded);
        SpinWait.SpinUntil(
                () => source.ActiveRequestCount == 0 &&
                      coordinator.ActiveRequestCount == 0 &&
                      coordinator.InFlightBlockCount == 0 &&
                      coordinator.TrackedRequestCount == 0,
                TimeSpan.FromSeconds(5))
            .ShouldBeTrue("cooperative cancellation must drain Source and coordinator work");
        coordinator.ActiveRequestCount.ShouldBe(0);
        coordinator.InFlightBlockCount.ShouldBe(0);
        coordinator.TrackedRequestCount.ShouldBe(0);
        coordinator.CachePinCount.ShouldBe(0);
    }

    [Fact]
    public void Synchronously_Completed_Source_Does_Not_Leave_Inflight_Entry()
    {
        var source = new ImmediateDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);

        Await(EnsureViewport(coordinator, generation, Viewport(0, 20)))
             .Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);

        coordinator.InFlightBlockCount.ShouldBe(0);
        coordinator.ActiveRequestCount.ShouldBe(0);
    }

    [Fact]
    public void Refresh_Preserves_Fallback_Block_Pin_Through_Failure()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var initial = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var initialLoad = EnsureViewport(coordinator, initial, Viewport(0, 20));
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "base"));
        Await(initialLoad);
        var fallback = coordinator.AppliedSnapshot!;
        coordinator.CachePinCount.ShouldBe(1);

        var refresh = coordinator.BeginGeneration(
            source, Query(DataGridSortDirection.Ascending), null,
            DataGridGroupExpansion.AllExpanded, fallback);
        coordinator.AppliedSnapshot.ShouldBeSameAs(fallback);
        coordinator.CachePinCount.ShouldBe(1);
        var refreshLoad = EnsureViewport(coordinator, refresh, Viewport(0, 20));
        source.Fail(1, new InvalidOperationException("refresh failed"));

        Await(refreshLoad).Kind.ShouldBe(DataGridPresentationTransitionKind.RolledBack);
        coordinator.AppliedSnapshot.ShouldBeSameAs(fallback);
        coordinator.CachePinCount.ShouldBe(1);
    }

    [Fact]
    public void Generation_Without_Fallback_Releases_Previous_Presentation()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var initial = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var initialLoad = EnsureViewport(coordinator, initial, Viewport(0, 20));
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "base"));
        Await(initialLoad);

        coordinator.BeginGeneration(
            source, Query(DataGridSortDirection.Ascending), null,
            DataGridGroupExpansion.AllExpanded, fallback: null);

        coordinator.AppliedSnapshot.ShouldBeNull();
        coordinator.CachePinCount.ShouldBe(0);
    }

    [Fact]
    public void Successful_Refresh_Drops_The_Generation_Fallback_Reference()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var initial = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var initialLoad = EnsureViewport(coordinator, initial, Viewport(0, 20));
        source.Complete(0, CoordinatorFixtures.ResultFor(source.RequestAt(0), "base"));
        Await(initialLoad);
        var fallback = coordinator.AppliedSnapshot!;

        var refresh = coordinator.BeginGeneration(
            source, Query(DataGridSortDirection.Ascending), null,
            DataGridGroupExpansion.AllExpanded, fallback);
        var refreshLoad = EnsureViewport(coordinator, refresh, Viewport(0, 20));
        source.Complete(1, CoordinatorFixtures.ResultFor(source.RequestAt(1), "fresh"));
        Await(refreshLoad).Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);

        refresh.Fallback.ShouldBeNull();
        coordinator.AppliedSnapshot!.Source.ShouldBeSameAs(source);
        coordinator.AppliedSnapshot.DataGeneration.ShouldBe(refresh.DataGeneration);
    }

    [Fact]
    public void Empty_Result_Commits_An_Empty_Viewport()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        var load = EnsureViewport(coordinator, generation, Viewport(0, 20));
        source.Complete(0, CoordinatorFixtures.ResultFor(
            source.RequestAt(0), "empty", total: 0));

        var transition = Await(load);
        transition.Kind.ShouldBe(DataGridPresentationTransitionKind.Committed);
        transition.Snapshot!.TotalEntryCount.ShouldBe(0);
        transition.Snapshot.CommittedViewport.VisibleCount.ShouldBe(0);
        transition.Snapshot.Blocks.ShouldBeEmpty();
    }

    [Fact]
    public void Caller_Cancellation_Does_Not_Enter_Error_State()
    {
        var source = new ControllableDataGridSource(CoordinatorFixtures.Schema());
        using var coordinator = CoordinatorFixtures.Create();
        var generation = coordinator.BeginGeneration(
            source, DataGridQuery.Empty, null,
            DataGridGroupExpansion.AllExpanded, null);
        using var cancellation = new CancellationTokenSource();
        var load = coordinator.EnsureViewportAsync(
            generation, Viewport(0, 20), cancellation.Token);

        cancellation.Cancel();
        Await(load).Kind.ShouldBe(DataGridPresentationTransitionKind.Superseded);
        coordinator.LastError.ShouldBeNull();
        source.CompleteIgnoringCancellation(0, CoordinatorFixtures.ResultFor(
            source.RequestAt(0), "late"));
        SpinWait.SpinUntil(
            () => coordinator.ActiveRequestCount == 0,
            TimeSpan.FromSeconds(5)).ShouldBeTrue();
    }

    private static DataGridQuery Query(DataGridSortDirection direction) =>
        DataGridQuery.Empty.WithSorts([new DataGridSort(Age, direction)]);

    private static DataGridDesiredViewport Viewport(int start, int count) => new(start, count, 0);

    private static ValueTask<DataGridPresentationTransition> EnsureViewport(
        DataGridRangeCoordinator coordinator,
        DataGridGeneration generation,
        DataGridDesiredViewport viewport) =>
        coordinator.EnsureViewportAsync(
            generation,
            viewport,
            TestContext.Current.CancellationToken);

    private static ValueTask Prefetch(
        DataGridRangeCoordinator coordinator,
        DataGridGeneration generation,
        DataGridDesiredViewport viewport) =>
        coordinator.PrefetchAsync(
            generation,
            viewport,
            TestContext.Current.CancellationToken);

    private static DataGridPresentationTransition Await(
        ValueTask<DataGridPresentationTransition> transition) =>
        transition.GetAwaiter().GetResult();

    private static void Await(ValueTask operation) =>
        operation.GetAwaiter().GetResult();

    private static class CoordinatorFixtures
    {
        public static DataGridSourceSchema Schema() => new(
            typeof(Row),
            [new DataGridFieldSchema(
                Age,
                typeof(int),
                DataGridSortDirections.All,
                [],
                canGroup: false)],
            32,
            32);

        public static DataGridRangeCoordinator Create() => new(cacheCapacity: 8);

        public static DataGridRangeResult ResultFor(
            ControllableDataGridSource.PendingRequest pending,
            string snapshot,
            long? firstKey = null,
            int total = 128) => ResultFor(pending.Request, snapshot, firstKey, total);

        public static DataGridRangeResult ResultFor(
            DataGridFetchRequest request,
            string snapshot,
            long? firstKey = null,
            int total = 128)
        {
            var count = Math.Min(
                request.Range.Count,
                Math.Max(0, total - request.Range.StartIndex));
            var entries = new DataGridSourceEntry[count];
            for (var index = 0; index < count; index++)
            {
                var dataIndex = request.Range.StartIndex + index;
                entries[index] = DataGridSourceEntry.CreateData(
                    DataGridRowKey.FromInt64((firstKey ?? request.Range.StartIndex) + index),
                    new Row(dataIndex),
                    dataIndex,
                    dataIndex);
            }
            return new DataGridRangeResult(
                request.Range.StartIndex,
                [.. entries],
                total,
                total,
                total,
                new DataGridSnapshotId(snapshot));
        }
    }

    private sealed record Row(int Age);

    private sealed class ImmediateDataGridSource(DataGridSourceSchema schema) : IDataGridSource
    {
        public DataGridSourceSchema Schema { get; } = schema;

        public event EventHandler? Invalidated
        {
            add { }
            remove { }
        }

        public ValueTask<DataGridRangeResult> FetchAsync(
            DataGridFetchRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ValueTask<DataGridRangeResult>(
                CoordinatorFixtures.ResultFor(request, "sync"));
        }
    }
}
