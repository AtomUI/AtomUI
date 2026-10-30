using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class ImageRequestSchedulerTests
{
    [Fact]
    public async Task Same_Priority_Work_Runs_In_Fifo_Order()
    {
        using var scheduler = new ImageRequestScheduler(1, 1, 1);
        var blockerStarted = NewSignal();
        var releaseBlocker = NewSignal();
        var order = new List<int>();
        var blocker = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            async token =>
            {
                blockerStarted.TrySetResult();
                await releaseBlocker.Task.WaitAsync(token);
                return 0;
            },
            () => ImageRequestPriority.Critical,
            CancellationToken.None);
        await blockerStarted.Task;

        var first = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            _ =>
            {
                order.Add(1);
                return Task.FromResult(1);
            },
            () => ImageRequestPriority.Normal,
            CancellationToken.None);
        var second = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            _ =>
            {
                order.Add(2);
                return Task.FromResult(2);
            },
            () => ImageRequestPriority.Normal,
            CancellationToken.None);

        releaseBlocker.TrySetResult();
        await Task.WhenAll(blocker, first, second);

        order.ShouldBe([1, 2]);
    }

    [Fact]
    public async Task Queued_Work_Uses_Latest_Promoted_And_Demoted_Priority()
    {
        using var scheduler = new ImageRequestScheduler(1, 1, 1);
        var blockerStarted = NewSignal();
        var releaseBlocker = NewSignal();
        var order = new List<string>();
        var dynamicPriority = ImageRequestPriority.Low;
        var blocker = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            async token =>
            {
                blockerStarted.TrySetResult();
                await releaseBlocker.Task.WaitAsync(token);
                return 0;
            },
            () => ImageRequestPriority.Critical,
            CancellationToken.None);
        await blockerStarted.Task;

        var dynamic = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            _ =>
            {
                order.Add("dynamic");
                return Task.FromResult(1);
            },
            () => dynamicPriority,
            CancellationToken.None);
        var high = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            _ =>
            {
                order.Add("high");
                return Task.FromResult(2);
            },
            () => ImageRequestPriority.High,
            CancellationToken.None);

        dynamicPriority = ImageRequestPriority.Critical;
        releaseBlocker.TrySetResult();
        await Task.WhenAll(blocker, dynamic, high);
        order.ShouldBe(["dynamic", "high"]);

        order.Clear();
        blockerStarted = NewSignal();
        releaseBlocker = NewSignal();
        dynamicPriority = ImageRequestPriority.Critical;
        blocker = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            async token =>
            {
                blockerStarted.TrySetResult();
                await releaseBlocker.Task.WaitAsync(token);
                return 0;
            },
            () => ImageRequestPriority.Critical,
            CancellationToken.None);
        await blockerStarted.Task;
        dynamic = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            _ =>
            {
                order.Add("dynamic");
                return Task.FromResult(1);
            },
            () => dynamicPriority,
            CancellationToken.None);
        high = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            _ =>
            {
                order.Add("high");
                return Task.FromResult(2);
            },
            () => ImageRequestPriority.High,
            CancellationToken.None);

        dynamicPriority = ImageRequestPriority.Low;
        releaseBlocker.TrySetResult();
        await Task.WhenAll(blocker, dynamic, high);
        order.ShouldBe(["high", "dynamic"]);
    }

    [Fact]
    public async Task Aging_Promotes_Waiting_Low_Work_Without_Real_Time_Delay()
    {
        var now = DateTimeOffset.UtcNow;
        using var scheduler = new ImageRequestScheduler(1, 1, 1, () => now);
        var blockerStarted = NewSignal();
        var releaseBlocker = NewSignal();
        var order = new List<string>();
        var blocker = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            async token =>
            {
                blockerStarted.TrySetResult();
                await releaseBlocker.Task.WaitAsync(token);
                return 0;
            },
            () => ImageRequestPriority.Critical,
            CancellationToken.None);
        await blockerStarted.Task;
        var low = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            _ =>
            {
                order.Add("low");
                return Task.FromResult(1);
            },
            () => ImageRequestPriority.Low,
            CancellationToken.None);
        now = now.AddSeconds(6);
        var normal = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            _ =>
            {
                order.Add("normal");
                return Task.FromResult(2);
            },
            () => ImageRequestPriority.Normal,
            CancellationToken.None);

        releaseBlocker.TrySetResult();
        await Task.WhenAll(blocker, low, normal);

        order.ShouldBe(["low", "normal"]);
    }

    [Fact]
    public async Task Blocked_Http_Download_Does_Not_Block_Local_Read()
    {
        using var scheduler = new ImageRequestScheduler(1, 1, 1);
        var downloadStarted = NewSignal();
        var releaseDownload = NewSignal();
        var download = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            async token =>
            {
                downloadStarted.TrySetResult();
                await releaseDownload.Task.WaitAsync(token);
                return 1;
            },
            () => ImageRequestPriority.Normal,
            CancellationToken.None);
        await downloadStarted.Task;

        var localRead = scheduler.ScheduleReadAsync(
            ImageSourceKind.Asset,
            _ => Task.FromResult(2),
            () => ImageRequestPriority.Normal,
            CancellationToken.None);

        (await localRead.WaitAsync(
            TimeSpan.FromSeconds(1),
            TestContext.Current.CancellationToken)).ShouldBe(2);
        releaseDownload.TrySetResult();
        (await download).ShouldBe(1);
    }

    [Fact]
    public async Task Dispose_Cancels_Active_And_Queued_Work()
    {
        var scheduler = new ImageRequestScheduler(1, 1, 1);
        var activeStarted = NewSignal();
        var active = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            async token =>
            {
                activeStarted.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                return 1;
            },
            () => ImageRequestPriority.Normal,
            CancellationToken.None);
        await activeStarted.Task;
        var queued = scheduler.ScheduleReadAsync(
            ImageSourceKind.Http,
            _ => Task.FromResult(2),
            () => ImageRequestPriority.Normal,
            CancellationToken.None);

        await Task.Run(scheduler.Dispose, TestContext.Current.CancellationToken);

        await Should.ThrowAsync<OperationCanceledException>(async () => await active);
        await Should.ThrowAsync<OperationCanceledException>(async () => await queued);
        await WaitUntilAsync(() => scheduler.ActiveReads == 0);
        scheduler.ActiveReads.ShouldBe(0);
        scheduler.QueuedReads.ShouldBe(0);
    }

    [Fact]
    public async Task Dispose_Does_Not_Wait_For_An_Active_Action_That_Ignores_Cancellation()
    {
        var scheduler = new ImageRequestScheduler(1, 1, 1);
        var started = NewSignal();
        var canceled = NewSignal();
        var release = NewSignal();
        var lateResult = new DisposableResult();
        var work = scheduler.ScheduleDecodeAsync(
            async token =>
            {
                using var registration = token.Register(() => canceled.TrySetResult());
                started.TrySetResult();
                await release.Task;
                return lateResult;
            },
            () => ImageRequestPriority.Normal,
            CancellationToken.None,
            static result => result.Dispose());
        await started.Task;

        var disposeTask = Task.Run(scheduler.Dispose, TestContext.Current.CancellationToken);
        await canceled.Task;
        var returnedBeforeAction = false;
        try
        {
            await disposeTask.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
            returnedBeforeAction = true;
        }
        catch (TimeoutException)
        {
        }
        finally
        {
            release.TrySetResult();
            await disposeTask;
        }

        returnedBeforeAction.ShouldBeTrue();
        await Should.ThrowAsync<OperationCanceledException>(async () => await work);
        await lateResult.Disposed.Task.WaitAsync(TestContext.Current.CancellationToken);
        lateResult.DisposeCount.ShouldBe(1);
    }

    private static TaskCompletionSource NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 100 && !condition(); attempt++)
        {
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }

        condition().ShouldBeTrue();
    }

    private sealed class DisposableResult : IDisposable
    {
        internal int DisposeCount { get; private set; }

        internal TaskCompletionSource Disposed { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Dispose()
        {
            DisposeCount++;
            Disposed.TrySetResult();
        }
    }
}
