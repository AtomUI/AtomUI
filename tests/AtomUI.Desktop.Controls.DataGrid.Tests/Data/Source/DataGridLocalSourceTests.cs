using System.Collections.Immutable;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;

public class DataGridLocalSourceTests
{
    [Fact]
    public void LocalSource_Uses_Source_Ordinal_As_Final_TieBreak()
    {
        var rows = new[] { new Row(1, 20, "b"), new Row(2, 20, "a"), new Row(3, 10, "a") };
        using var source = LocalFixtures.Create(rows);
        var query = DataGridQuery.Empty.WithSorts(
            [new(new DataGridFieldId("age"), DataGridSortDirection.Ascending)]);

        var result = Fetch(source,
            LocalFixtures.Request(query, 0, 3), TestContext.Current.CancellationToken);

        result.Entries.Select(entry => entry.RowKey).ShouldBe([
            DataGridRowKey.FromInt64(3),
            DataGridRowKey.FromInt64(1),
            DataGridRowKey.FromInt64(2)]);
    }

    [Fact]
    public void LocalSource_Combines_Filters_With_And()
    {
        var rows = new[]
        {
            new Row(1, 10, "a"),
            new Row(2, 20, "a"),
            new Row(3, 30, "b"),
            new Row(4, 40, "a")
        };
        using var source = LocalFixtures.Create(rows);
        var query = DataGridQuery.Empty.WithFilters(
            [
                new(
                    new DataGridFieldId("age"),
                    new DataGridOperatorId("gte"),
                    [DataGridScalar.FromInt64(20)]),
                new(
                    new DataGridFieldId("region"),
                    new DataGridOperatorId("equals"),
                    [DataGridScalar.FromString("a")])
            ]);

        var result = Fetch(source,
            LocalFixtures.Request(query, 0, 32), TestContext.Current.CancellationToken);

        result.TotalDataCount.ShouldBe(2);
        result.Entries.Select(entry => entry.RowKey).ShouldBe([
            DataGridRowKey.FromInt64(2),
            DataGridRowKey.FromInt64(4)]);
    }

    [Fact]
    public void LocalSource_Applies_Groups_Before_Sorts()
    {
        var rows = new[]
        {
            new Row(1, 10, "b"),
            new Row(2, 20, "a"),
            new Row(3, 30, "b"),
            new Row(4, 40, "a")
        };
        using var source = LocalFixtures.Create(rows);
        var query = new DataGridQuery(
            [new(new DataGridFieldId("age"), DataGridSortDirection.Descending)],
            [],
            [new(new DataGridFieldId("region"), DataGridSortDirection.Ascending)]);

        var result = Fetch(source,
            LocalFixtures.Request(query, 0, 32), TestContext.Current.CancellationToken);

        result.Entries.Select(entry => entry.Kind).ShouldBe([
            DataGridSourceEntryKind.GroupHeader,
            DataGridSourceEntryKind.Data,
            DataGridSourceEntryKind.Data,
            DataGridSourceEntryKind.GroupHeader,
            DataGridSourceEntryKind.Data,
            DataGridSourceEntryKind.Data]);
        result.Entries.Where(entry => entry.Kind == DataGridSourceEntryKind.Data)
              .Select(entry => entry.RowKey)
              .ShouldBe([
                  DataGridRowKey.FromInt64(4),
                  DataGridRowKey.FromInt64(2),
                  DataGridRowKey.FromInt64(3),
                  DataGridRowKey.FromInt64(1)]);
    }

    [Fact]
    public void LocalSource_Pages_Data_Before_Group_Insertion_And_Uses_Global_Indices()
    {
        var rows = new[]
        {
            new Row(1, 10, "a"),
            new Row(2, 20, "a"),
            new Row(3, 30, "b"),
            new Row(4, 40, "b")
        };
        using var source = LocalFixtures.Create(rows);
        var query = DataGridQuery.Empty.WithGroups(
            [new(new DataGridFieldId("region"), DataGridSortDirection.Ascending)]);
        var request = LocalFixtures.Request(
            query,
            0,
            32,
            new DataGridPageRequest(1, 2));

        var result = Fetch(source, request, TestContext.Current.CancellationToken);

        result.WindowDataCount.ShouldBe(2);
        result.TotalDataCount.ShouldBe(4);
        result.TotalEntryCount.ShouldBe(4);
        var data = result.Entries.Where(entry => entry.Kind == DataGridSourceEntryKind.Data).ToArray();
        data.Select(entry => entry.WindowDataIndex).ShouldBe([0, 1]);
        data.Select(entry => entry.DataIndex).ShouldBe([1L, 2L]);
    }

    [Fact]
    public void LocalSource_Collapse_Omits_Descendants_But_Preserves_LeafCount()
    {
        var rows = new[]
        {
            new Row(1, 10, "a"),
            new Row(2, 20, "a"),
            new Row(3, 30, "b")
        };
        using var source = LocalFixtures.Create(rows);
        var query = DataGridQuery.Empty.WithGroups(
            [new(new DataGridFieldId("region"), DataGridSortDirection.Ascending)]);
        var expanded = Fetch(source,
            LocalFixtures.Request(query, 0, 32), TestContext.Current.CancellationToken);
        var firstGroup = expanded.Entries[0].Group!;

        var collapsed = Fetch(source,
            LocalFixtures.Request(
                query,
                0,
                32,
                expansion: DataGridGroupExpansion.AllExpanded.Collapse(firstGroup.Key),
                expectedSnapshot: expanded.Snapshot),
            TestContext.Current.CancellationToken);

        collapsed.TotalDataCount.ShouldBe(3);
        collapsed.TotalEntryCount.ShouldBe(3);
        collapsed.Entries[0].Group!.LeafCount.ShouldBe(2);
        collapsed.Entries.Count(entry => entry.Kind == DataGridSourceEntryKind.Data).ShouldBe(1);
        collapsed.Entries.Single(entry => entry.Kind == DataGridSourceEntryKind.Data)
                 .RowKey.ShouldBe(DataGridRowKey.FromInt64(3));
    }

    [Fact]
    public void LocalSource_Returns_Exact_Short_Tail_And_Stable_Snapshot_Totals()
    {
        var rows = Enumerable.Range(0, 35).Select(index => new Row(index, index, "a")).ToArray();
        using var source = LocalFixtures.Create(rows);
        var first = Fetch(source,
            LocalFixtures.Request(DataGridQuery.Empty, 0, 32), TestContext.Current.CancellationToken);
        var tail = Fetch(source,
            LocalFixtures.Request(
                DataGridQuery.Empty,
                32,
                32,
                expectedSnapshot: first.Snapshot),
            TestContext.Current.CancellationToken);

        first.Entries.Length.ShouldBe(32);
        tail.Entries.Length.ShouldBe(3);
        tail.Snapshot.ShouldBe(first.Snapshot);
        tail.TotalDataCount.ShouldBe(first.TotalDataCount);
        tail.WindowDataCount.ShouldBe(first.WindowDataCount);
        tail.TotalEntryCount.ShouldBe(first.TotalEntryCount);
    }

    [Fact]
    public void LocalSource_Completes_Cached_Range_Without_Task_Scheduling()
    {
        using var source = LocalFixtures.Create(
            Enumerable.Range(0, 64).Select(index => new Row(index, index, "a")).ToArray());
        var request = LocalFixtures.Request(DataGridQuery.Empty, 0, 32);
        Fetch(source, request, TestContext.Current.CancellationToken);

        var cached = source.FetchAsync(request, TestContext.Current.CancellationToken);

        cached.IsCompletedSuccessfully.ShouldBeTrue();
        GetCompleted(cached).Entries.Length.ShouldBe(32);
    }

    [Fact]
    public void LocalSource_Empty_Query_Reads_Only_Requested_Range_And_Completes_Synchronously()
    {
        var rows = new TrackingReadOnlyRows(1_000_000);
        using var source = LocalFixtures.Create(rows, preferredRangeSize: 64, maximumRangeSize: 64);
        var pending = source.FetchAsync(
            LocalFixtures.Request(DataGridQuery.Empty, 999_936, 64),
            TestContext.Current.CancellationToken);
        var completedSynchronously = pending.IsCompletedSuccessfully;
        var result = GetCompleted(pending);

        completedSynchronously.ShouldBeTrue();
        rows.IndexReadCount.ShouldBe(64);
        source.CachedProjectionCount.ShouldBe(0);
        result.TotalDataCount.ShouldBe(1_000_000);
        result.Entries.Length.ShouldBe(64);
        result.Entries[0].RowKey.ShouldBe(DataGridRowKey.FromInt64(999_936));
    }

    [Fact]
    public void LocalSource_Observes_Cancellation_And_Old_Snapshot_Expiry()
    {
        var rows = new TrackingCollection<Row>(
            Enumerable.Range(0, 128).Select(index => new Row(index, index, "a")));
        using var source = LocalFixtures.Create(rows);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Should.Throw<OperationCanceledException>(() => Fetch(
            source,
            LocalFixtures.Request(DataGridQuery.Empty, 0, 32),
            cancellation.Token));

        var first = Fetch(source,
            LocalFixtures.Request(DataGridQuery.Empty, 0, 32), TestContext.Current.CancellationToken);
        rows.Add(new Row(200, 200, "a"));
        Should.Throw<DataGridSnapshotExpiredException>(() => Fetch(
                source,
                LocalFixtures.Request(
                    DataGridQuery.Empty,
                    0,
                    32,
                    expectedSnapshot: first.Snapshot),
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public void LocalSource_Preserves_OperationCanceledException_From_Sort_Comparer()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);
        var comparer = new CancelingComparer(cancellation);
        var descriptor = DataGridLocalSourceDescriptor.For<Row>(
                static row => DataGridRowKey.FromInt64(row.Id))
            .Field(
                new DataGridFieldId("age"),
                static row => row.Age,
                comparer,
                static value => DataGridScalar.FromInt64(value));
        var rows = Enumerable.Range(0, 5_000)
                             .Select(index => new Row(index, 5_000 - index, "a"))
                             .ToArray();
        using var source = DataGridLocalSource.Create(rows, descriptor);
        var query = DataGridQuery.Empty.WithSorts(
            [new(new DataGridFieldId("age"), DataGridSortDirection.Ascending)]);

        Should.Throw<OperationCanceledException>(() => Fetch(
            source,
            LocalFixtures.Request(query, 0, 32),
            cancellation.Token));
    }

    [Fact]
    public void LocalSource_Invalidation_Clears_Cache_And_Raises_One_Event()
    {
        var rows = new TrackingCollection<Row>([new Row(1, 10, "a")]);
        using var source = LocalFixtures.Create(rows);
        var invalidationCount = 0;
        source.Invalidated += (_, _) => invalidationCount++;
        var query = DataGridQuery.Empty.WithSorts(
            [new(new DataGridFieldId("age"), DataGridSortDirection.Ascending)]);
        var first = Fetch(source,
            LocalFixtures.Request(query, 0, 32),
            TestContext.Current.CancellationToken);
        source.CachedProjectionCount.ShouldBeGreaterThan(0);

        rows.Add(new Row(2, 20, "b"));

        invalidationCount.ShouldBe(1);
        source.CachedProjectionCount.ShouldBe(0);
        var second = Fetch(source,
            LocalFixtures.Request(query, 0, 32),
            TestContext.Current.CancellationToken);
        second.Snapshot.ShouldNotBe(first.Snapshot);
        second.TotalDataCount.ShouldBe(2);
    }

    [Fact]
    public void LocalSource_Keeps_Projection_Cache_At_Exact_Configured_Capacity()
    {
        var rows = Enumerable.Range(0, 100).Select(index => new Row(index, index, "a")).ToArray();
        using var source = LocalFixtures.Create(rows, maximumProjectionCount: 2);

        for (var value = 0; value < 8; value++)
        {
            var query = DataGridQuery.Empty.WithFilters(
                [new(
                    new DataGridFieldId("age"),
                    new DataGridOperatorId("gte"),
                    [DataGridScalar.FromInt64(value)])]);
            Fetch(source,
                LocalFixtures.Request(query, 0, 32), TestContext.Current.CancellationToken);
            source.CachedProjectionCount.ShouldBeLessThanOrEqualTo(2);
        }
    }

    [Fact]
    public void LocalSource_Handles_One_Million_Rows_Without_Total_Sized_Result()
    {
        var rows = Enumerable.Range(0, 1_000_000)
                             .Select(index => new Row(index, index, "a"))
                             .ToArray();
        using var source = LocalFixtures.Create(rows, preferredRangeSize: 64, maximumRangeSize: 64);

        var result = Fetch(source,
            LocalFixtures.Request(DataGridQuery.Empty, 999_936, 64),
            TestContext.Current.CancellationToken);

        result.TotalDataCount.ShouldBe(1_000_000);
        result.TotalEntryCount.ShouldBe(1_000_000);
        result.Entries.Length.ShouldBe(64);
        result.Entries[0].RowKey.ShouldBe(DataGridRowKey.FromInt64(999_936));
    }

    internal sealed record Row(long Id, int Age, string Region);

    private sealed class CancelingComparer : IComparer<int>
    {
        private readonly CancellationTokenSource _cancellation;
        private int _comparisonCount;

        public CancelingComparer(CancellationTokenSource cancellation)
        {
            _cancellation = cancellation;
        }

        public int Compare(int left, int right)
        {
            if (Interlocked.Increment(ref _comparisonCount) == 32)
            {
                _cancellation.Cancel();
            }
            return left.CompareTo(right);
        }
    }

    private sealed class TrackingReadOnlyRows : IReadOnlyList<Row>
    {
        private int _indexReadCount;

        public TrackingReadOnlyRows(int count)
        {
            Count = count;
        }

        public int Count { get; }

        public int IndexReadCount => Volatile.Read(ref _indexReadCount);

        public Row this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                Interlocked.Increment(ref _indexReadCount);
                return new Row(index, index, "a");
            }
        }

        public IEnumerator<Row> GetEnumerator()
        {
            for (var index = 0; index < Count; index++)
            {
                yield return this[index];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal static DataGridRangeResult Fetch(
        DataGridLocalSource<Row> source,
        DataGridFetchRequest request,
        CancellationToken cancellationToken) =>
        source.FetchAsync(request, cancellationToken).GetAwaiter().GetResult();

    private static DataGridRangeResult GetCompleted(ValueTask<DataGridRangeResult> valueTask) =>
        valueTask.GetAwaiter().GetResult();

    internal static class LocalFixtures
    {
        public static DataGridLocalSource<Row> Create(
            IReadOnlyList<Row> rows,
            int preferredRangeSize = 32,
            int maximumRangeSize = 128,
            int maximumProjectionCount = 4)
        {
            var ageFilters = ImmutableArray.Create(
                new DataGridLocalFilter<int>(
                    new DataGridOperatorId("gte"),
                    1,
                    1,
                    DataGridScalarKinds.SignedInteger,
                    static (value, values) => DataGridScalar.FromInt64(value) >= values[0]));
            var regionFilters = ImmutableArray.Create(
                new DataGridLocalFilter<string>(
                    new DataGridOperatorId("equals"),
                    1,
                    1,
                    DataGridScalarKinds.String,
                    static (value, values) => DataGridScalar.FromString(value) == values[0]));
            var descriptor = DataGridLocalSourceDescriptor.For<Row>(
                    static row => DataGridRowKey.FromInt64(row.Id))
                .Field(
                    new DataGridFieldId("age"),
                    static row => row.Age,
                    Comparer<int>.Default,
                    static value => DataGridScalar.FromInt64(value),
                    ageFilters)
                .Field(
                    new DataGridFieldId("region"),
                    static row => row.Region,
                    StringComparer.Ordinal,
                    DataGridScalar.FromString,
                    regionFilters,
                    canGroup: true);
            return DataGridLocalSource.Create(
                rows,
                descriptor,
                new DataGridLocalSourceOptions
                {
                    PreferredRangeSize = preferredRangeSize,
                    MaximumRangeSize = maximumRangeSize,
                    MaximumProjectionCount = maximumProjectionCount
                });
        }

        public static DataGridFetchRequest Request(
            DataGridQuery query,
            int start,
            int count,
            DataGridPageRequest? pageRequest = null,
            DataGridGroupExpansion? expansion = null,
            DataGridSnapshotId? expectedSnapshot = null) =>
            new(
                query,
                pageRequest,
                expansion ?? DataGridGroupExpansion.AllExpanded,
                new DataGridRange(start, count),
                expectedSnapshot,
                0,
                0);
    }

    internal sealed class TrackingCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>
    {
        public TrackingCollection(IEnumerable<T> items)
            : base(items)
        {
        }

        public int CollectionChangedHandlerCount { get; private set; }

        public override event System.Collections.Specialized.NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add
            {
                CollectionChangedHandlerCount++;
                base.CollectionChanged += value;
            }
            remove
            {
                CollectionChangedHandlerCount--;
                base.CollectionChanged -= value;
            }
        }
    }
}
