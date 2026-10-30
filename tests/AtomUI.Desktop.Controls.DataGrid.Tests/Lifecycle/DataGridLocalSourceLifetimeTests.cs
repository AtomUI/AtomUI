using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;
using static AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source.DataGridLocalSourceTests;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Lifecycle;

public class DataGridLocalSourceLifetimeTests
{
    [Fact]
    public void LocalSource_Subscribes_Once_And_Dispose_Unsubscribes_Exactly_Once()
    {
        var rows = new TrackingCollection<Row>([new Row(1, 10, "a")]);
        var source = LocalFixtures.Create(rows);
        rows.CollectionChangedHandlerCount.ShouldBe(1);

        source.Dispose();
        rows.CollectionChangedHandlerCount.ShouldBe(0);
        source.Dispose();
        rows.CollectionChangedHandlerCount.ShouldBe(0);
    }

    [Fact]
    public void LocalSource_Dispose_Clears_Projection_Cache_And_Rejects_Fetch()
    {
        var source = LocalFixtures.Create([new Row(1, 10, "a")]);
        var cachedQuery = DataGridQuery.Empty.WithSorts(
            [new(new DataGridFieldId("age"), DataGridSortDirection.Ascending)]);
        Fetch(source,
            LocalFixtures.Request(cachedQuery, 0, 1),
            TestContext.Current.CancellationToken);
        source.CachedProjectionCount.ShouldBeGreaterThan(0);

        source.Dispose();

        source.CachedProjectionCount.ShouldBe(0);
        Should.Throw<ObjectDisposedException>(() => Fetch(
                source,
                LocalFixtures.Request(DataGridQuery.Empty, 0, 1),
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Disposed_LocalSource_Is_Not_Retained_By_Observable_Collection()
    {
        var rows = new TrackingCollection<Row>([new Row(1, 10, "a")]);
        var weakSource = CreateDisposeAndRelease(rows);

        ForceGc();
        weakSource.IsAlive.ShouldBeFalse();
        rows.CollectionChangedHandlerCount.ShouldBe(0);
        Should.NotThrow(() => rows.Add(new Row(2, 20, "b")));
    }

    [Fact]
    public void Weak_Collection_Forwarder_Does_Not_Retain_Undisposed_LocalSource()
    {
        var rows = new TrackingCollection<Row>([new Row(1, 10, "a")]);
        var weakSource = CreateAndReleaseWithoutDispose(rows);

        ForceGc();
        weakSource.IsAlive.ShouldBeFalse();
        rows.CollectionChangedHandlerCount.ShouldBe(1);

        rows.Add(new Row(2, 20, "b"));
        rows.CollectionChangedHandlerCount.ShouldBe(0);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateDisposeAndRelease(TrackingCollection<Row> rows)
    {
        var source = LocalFixtures.Create(rows);
        var weakSource = new WeakReference(source);
        source.Dispose();
        return weakSource;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateAndReleaseWithoutDispose(TrackingCollection<Row> rows)
    {
        var source = LocalFixtures.Create(rows);
        return new WeakReference(source);
    }

    private static void ForceGc()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
