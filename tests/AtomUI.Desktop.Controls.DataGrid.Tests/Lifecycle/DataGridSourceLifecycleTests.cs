using System.Runtime.CompilerServices;
using AtomUI.Desktop.Controls.Tests.DataGrid.Data.Source;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Lifecycle;

public class DataGridSourceLifecycleTests
{
    static DataGridSourceLifecycleTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Attach_Replace_Detach_And_Reattach_Own_Exactly_One_Subscription()
    {
        var first = Source();
        var second = Source();
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = first
        };
        var firstWindow = Show(grid);
        first.InvalidatedSubscriberCount.ShouldBe(1);

        grid.ItemsSource = second;

        first.InvalidatedSubscriberCount.ShouldBe(0);
        first.InvalidatedRemoveCount.ShouldBe(1);
        second.InvalidatedSubscriberCount.ShouldBe(1);
        second.InvalidatedAddCount.ShouldBe(1);
        firstWindow.Content = null;
        Dispatcher.UIThread.RunJobs();
        firstWindow.Close();
        second.InvalidatedSubscriberCount.ShouldBe(0);
        second.InvalidatedRemoveCount.ShouldBe(1);

        var secondWindow = Show(grid);
        try
        {
            second.InvalidatedSubscriberCount.ShouldBe(1);
            second.InvalidatedAddCount.ShouldBe(2);
            second.Requests.Count.ShouldBe(2);
        }
        finally
        {
            secondWindow.Close();
            Dispatcher.UIThread.RunJobs();
        }
        second.InvalidatedSubscriberCount.ShouldBe(0);
        second.InvalidatedRemoveCount.ShouldBe(2);
    }

    [Fact]
    public void Detach_Cancels_Active_Load_And_Does_Not_Dispose_External_Source()
    {
        var source = Source();
        source.HonorCancellation = true;
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = source
        };
        var window = Show(grid);
        var request = source.RequestAt(0);

        window.Close();
        Dispatcher.UIThread.RunJobs();

        request.IsCancellationRequested.ShouldBeTrue();
        SpinWait.SpinUntil(
            () => source.ActiveRequestCount == 0,
            TimeSpan.FromSeconds(5)).ShouldBeTrue();
        source.InvalidatedSubscriberCount.ShouldBe(0);
        source.RaiseInvalidated();
    }

    [Fact]
    public void Invalid_Candidate_Source_Leaves_Old_Source_And_Subscription_Untouched()
    {
        var age = new DataGridFieldId("age");
        var oldSource = Source();
        var invalidSource = new ControllableDataGridSource(new DataGridSourceSchema(
            typeof(Row),
            [],
            32,
            32));
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            Query = DataGridQuery.Empty.WithSorts([
                new DataGridSort(age, DataGridSortDirection.Ascending)]),
            ItemsSource = oldSource
        };
        var window = Show(grid);
        try
        {
            Should.Throw<DataGridSourceContractException>(() => grid.ItemsSource = invalidSource);

            grid.ItemsSource.ShouldBeSameAs(oldSource);
            oldSource.InvalidatedSubscriberCount.ShouldBe(1);
            invalidSource.InvalidatedSubscriberCount.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Worker_Invalidation_Rechecks_Source_Identity_Before_Reloading()
    {
        var oldSource = Source();
        var newSource = Source();
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = oldSource
        };
        var window = Show(grid);
        try
        {
            var staleHandler = oldSource.CaptureInvalidatedHandler();
            grid.ItemsSource = newSource;
            var worker = new Thread(() => staleHandler!(oldSource, EventArgs.Empty));
            worker.Start();
            worker.Join();
            Dispatcher.UIThread.RunJobs();

            oldSource.Requests.Count.ShouldBe(1);
            newSource.Requests.Count.ShouldBe(1);
            grid.ItemsSource.ShouldBeSameAs(newSource);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Reapplying_Template_Does_Not_Duplicate_Source_Subscription()
    {
        var source = Source();
        var grid = new TemplateTestDataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = source
        };
        var window = Show(grid);
        try
        {
            source.InvalidatedAddCount.ShouldBe(1);
            source.InvalidatedSubscriberCount.ShouldBe(1);

            grid.ReapplyEmptyTemplate();
            grid.ReapplyEmptyTemplate();

            source.InvalidatedAddCount.ShouldBe(1);
            source.InvalidatedRemoveCount.ShouldBe(0);
            source.InvalidatedSubscriberCount.ShouldBe(1);
        }
        finally
        {
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            window.Close();
        }
        source.InvalidatedSubscriberCount.ShouldBe(0);
        source.InvalidatedRemoveCount.ShouldBe(1);
    }

    [Fact]
    public void Three_Attach_Detach_Cycles_Do_Not_Retain_Grid_Or_Source()
    {
        var references = CreateAndDetachThreeTimes();

        ForceGc();

        references.Grid.IsAlive.ShouldBeFalse(
            "a detached DataGrid must not be retained by source subscriptions or range continuations");
        references.Source.IsAlive.ShouldBeFalse(
            "a detached external source must not be retained by the DataGrid range owner");
    }

    private static ControllableDataGridSource Source() => new(new DataGridSourceSchema(
        typeof(Row),
        [new DataGridFieldSchema(
            new DataGridFieldId("age"),
            typeof(int),
            DataGridSortDirections.All,
            [],
            canGroup: false)],
        32,
        32))
    {
        HonorCancellation = true
    };

    private static Window Show(global::AtomUI.Desktop.Controls.DataGrid grid)
    {
        var source = (ControllableDataGridSource)grid.ItemsSource!;
        var expectedRequestCount = source.Requests.Count + 1;
        var window = new Window { Width = 400, Height = 240, Content = grid };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        source.WaitForRequestCount(expectedRequestCount);
        return window;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static LifetimeReferences CreateAndDetachThreeTimes()
    {
        var source = Source();
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = source
        };

        for (var cycle = 0; cycle < 3; cycle++)
        {
            var window = Show(grid);
            source.InvalidatedSubscriberCount.ShouldBe(1);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            window.Close();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Dispatcher.UIThread.RunJobs();

            source.InvalidatedSubscriberCount.ShouldBe(0);
            SpinWait.SpinUntil(
                () => source.ActiveRequestCount == 0,
                TimeSpan.FromSeconds(5)).ShouldBeTrue();
        }

        return new LifetimeReferences(
            new WeakReference(grid),
            new WeakReference(source));
    }

    private static void ForceGc()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Thread.Sleep(TimeSpan.FromMilliseconds(10));
            Dispatcher.UIThread.RunJobs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private sealed class TemplateTestDataGrid : global::AtomUI.Desktop.Controls.DataGrid
    {
        public void ReapplyEmptyTemplate() =>
            OnApplyTemplate(new TemplateAppliedEventArgs(new NameScope()));
    }

    private sealed record LifetimeReferences(WeakReference Grid, WeakReference Source);

    private sealed record Row(int Age);
}
