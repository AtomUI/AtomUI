using System.Collections.Concurrent;
using System.Reflection;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.Net;

public class UploadSchedulerTests
{
    [Fact]
    public async Task Running_Task_Count_Increments_When_Task_Starts()
    {
        var transport = new BlockingUploadTransport();
        var scheduler = new FileUploadScheduler(transport);
        var task      = CreateUploadTask("running.txt");

        scheduler.EnqueueTask(task);
        await transport.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);

        GetRunningTaskCount(scheduler).ShouldBe(1);
        task.Status.ShouldBe(FileUploadStatus.Uploading);

        await scheduler.CancelAllAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Running_Task_Count_Decrements_When_Upload_Completes()
    {
        var transport = new BlockingUploadTransport();
        var scheduler = new FileUploadScheduler(transport);
        var task      = CreateUploadTask("complete.txt");

        scheduler.EnqueueTask(task);
        await transport.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);
        GetRunningTaskCount(scheduler).ShouldBe(1);

        transport.Complete(FileUploadResult.SuccessResult(new Uri("https://example.com/complete.txt"), 12, TimeSpan.FromMilliseconds(1)));
        await WaitUntilAsync(() => task.Status == FileUploadStatus.Success);

        GetRunningTaskCount(scheduler).ShouldBe(0);
    }

    [Fact]
    public async Task Queued_Progress_After_Terminal_Result_Is_Not_Published()
    {
        var context = new QueuedSynchronizationContext();
        var transport = new ImmediateProgressSuccessTransport();
        var scheduler = new FileUploadScheduler(transport);
        var task = CreateUploadTask("late-progress.txt");
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var progressReports = new List<double>();
        task.UploadProgressHandler = (_, _, progress) => progressReports.Add(progress);
        task.UploadCompletedHandler = (_, _, _) => completed.TrySetResult();

        EnqueueWithSynchronizationContext(scheduler, task, context);

        await completed.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);
        task.Status.ShouldBe(FileUploadStatus.Success);
        context.PendingCount.ShouldBe(1);

        context.RunAll();

        progressReports.ShouldBeEmpty();
        task.Status.ShouldBe(FileUploadStatus.Success);
    }

    [Fact]
    public async Task CancelAllAsync_Cancels_Running_Task()
    {
        var transport = new BlockingUploadTransport();
        var scheduler = new FileUploadScheduler(transport);
        var task      = CreateUploadTask("cancel.txt");

        scheduler.EnqueueTask(task);
        await transport.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);
        task.Status.ShouldBe(FileUploadStatus.Uploading);

        await scheduler.CancelAllAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => task.Status == FileUploadStatus.Cancelled);

        GetRunningTaskCount(scheduler).ShouldBe(0);
    }

    [Fact]
    public async Task CancelUploadAsync_Waits_For_Transport_To_Exit_After_Cancellation_Is_Observed()
    {
        var transport = new DelayedCancellationUploadTransport();
        var scheduler = new FileUploadScheduler(transport);
        var task = CreateUploadTask("delayed-cancel.txt");

        scheduler.EnqueueTask(task);
        await transport.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);

        var cancellationTask = scheduler.CancelUploadAsync(task);
        await transport.CancellationObserved.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);

        cancellationTask.IsCompleted.ShouldBeFalse();
        transport.AllowExit.TrySetResult();
        await cancellationTask.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);
        task.Status.ShouldBe(FileUploadStatus.Cancelled);
        GetRunningTaskCount(scheduler).ShouldBe(0);
    }

    [Fact]
    public async Task CancelUploadAsync_Waits_When_Transport_Blocks_Before_Returning_Its_Task()
    {
        var transport = new SynchronouslyBlockingUploadTransport();
        var scheduler = new FileUploadScheduler(transport);
        var task = CreateUploadTask("blocked-start.txt");

        var enqueueTask = Task.Run(
            () => scheduler.EnqueueTask(task),
            TestContext.Current.CancellationToken);
        await transport.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);

        var cancellationTask = scheduler.CancelUploadAsync(task);
        await WaitUntilAsync(() => task.CancellationTokenSource?.IsCancellationRequested == true);

        cancellationTask.IsCompleted.ShouldBeFalse();

        transport.AllowReturn.TrySetResult();
        await enqueueTask.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);
        await cancellationTask.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);

        task.Status.ShouldBe(FileUploadStatus.Cancelled);
        GetRunningTaskCount(scheduler).ShouldBe(0);
    }

    [Fact]
    public async Task EnqueueTask_Does_Not_Block_When_Transport_Blocks_Before_Returning_Its_Task()
    {
        var transport = new SynchronouslyBlockingSuccessfulUploadTransport();
        var scheduler = new FileUploadScheduler(transport);
        var task = CreateUploadTask("non-blocking-enqueue.txt");

        var enqueueTask = Task.Run(
            () => scheduler.EnqueueTask(task),
            TestContext.Current.CancellationToken);
        await transport.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);

        try
        {
            // The transport is still blocked, so EnqueueTask must return on its own; give the Task.Run continuation
            // time to observe that return instead of sampling it at the instant the transport signalled Started.
            (await Task.WhenAny(enqueueTask, Task.Delay(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken)))
                .ShouldBeSameAs(enqueueTask, "EnqueueTask must not block on a transport that blocks before returning its task");
        }
        finally
        {
            transport.AllowReturn.TrySetResult();
            await enqueueTask.WaitAsync(
                TimeSpan.FromSeconds(3),
                TestContext.Current.CancellationToken);
        }

        await WaitUntilAsync(() => task.Status == FileUploadStatus.Success);
    }

    [Fact]
    public async Task CancelAllAsync_Waits_For_Completion_Handler_To_Return()
    {
        var transport = new BlockingUploadTransport();
        var scheduler = new FileUploadScheduler(transport);
        var task = CreateUploadTask("completion-handler.txt");
        var handlerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var allowHandlerExit = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        task.UploadCompletedHandler = (_, _, _) =>
        {
            handlerStarted.TrySetResult();
            allowHandlerExit.Task.GetAwaiter().GetResult();
        };

        scheduler.EnqueueTask(task);
        await transport.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);
        transport.Complete(FileUploadResult.SuccessResult(
            new Uri("https://example.com/completion-handler.txt"),
            12,
            TimeSpan.FromMilliseconds(1)));
        await handlerStarted.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);

        var cancellationTask = scheduler.CancelAllAsync(TestContext.Current.CancellationToken);
        try
        {
            cancellationTask.IsCompleted.ShouldBeFalse();
        }
        finally
        {
            allowHandlerExit.TrySetResult();
            await cancellationTask.WaitAsync(
                TimeSpan.FromSeconds(3),
                TestContext.Current.CancellationToken);
        }

        GetRunningTaskCount(scheduler).ShouldBe(0);
    }

    [Fact]
    public async Task CancelAllAsync_Restores_Scheduling_When_A_Pending_Handler_Throws()
    {
        var transport = new BlockingUploadTransport();
        var scheduler = new FileUploadScheduler(transport, maxConcurrentTasks: 1);
        var runningTask = CreateUploadTask("running-before-handler-error.txt");
        var pendingTask = CreateUploadTask("pending-handler-error.txt");
        pendingTask.UploadCancelledHandler = (_, _, _) =>
            throw new InvalidOperationException("pending cancellation callback failed");

        scheduler.EnqueueTask(runningTask);
        await transport.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);
        scheduler.EnqueueTask(pendingTask);

        try
        {
            await Should.ThrowAsync<InvalidOperationException>(() =>
                scheduler.CancelAllAsync(TestContext.Current.CancellationToken));
            scheduler.IsScheduleEnabled().ShouldBeTrue();
            runningTask.Status.ShouldBe(FileUploadStatus.Cancelled);
        }
        finally
        {
            transport.Complete(FileUploadResult.SuccessResult(
                new Uri("https://example.com/running-before-handler-error.txt"),
                12,
                TimeSpan.FromMilliseconds(1)));
        }
    }

    [Fact]
    public async Task CancelAllAsync_Cancels_Every_Pending_Task_When_The_Token_Is_Cancelled_By_A_Handler()
    {
        var transport = new BlockingUploadTransport();
        var scheduler = new FileUploadScheduler(transport);
        using var cancellation = new CancellationTokenSource();
        var firstTask = CreateUploadTask("cancel-token-first.txt");
        var secondTask = CreateUploadTask("cancel-token-second.txt");
        firstTask.UploadCancelledHandler = (_, _, _) => cancellation.Cancel();

        scheduler.DisableSchedule();
        scheduler.EnqueueTask(firstTask);
        scheduler.EnqueueTask(secondTask);

        await Should.ThrowAsync<OperationCanceledException>(() =>
            scheduler.CancelAllAsync(cancellation.Token));

        firstTask.Status.ShouldBe(FileUploadStatus.Cancelled);
        secondTask.Status.ShouldBe(FileUploadStatus.Cancelled);
    }

    [Fact]
    public async Task Concurrent_Transport_Changes_Are_Applied_In_Call_Order()
    {
        var originalTransport = new BlockingUploadTransport();
        var intermediateTransport = new BlockingUploadTransport();
        var finalTransport = new BlockingUploadTransport();
        var scheduler = new FileUploadScheduler(originalTransport);
        var cancellationHandlerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var allowCancellationHandlerExit = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pendingTask = CreateUploadTask("serialized-transport-change.txt");
        pendingTask.UploadCancelledHandler = (_, _, _) =>
        {
            cancellationHandlerStarted.TrySetResult();
            allowCancellationHandlerExit.Task.GetAwaiter().GetResult();
        };

        scheduler.DisableSchedule();
        scheduler.EnqueueTask(pendingTask);
        var firstChange = Task.Run(
            () => scheduler.SetTransportAsync(intermediateTransport, TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken);
        await cancellationHandlerStarted.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);

        var secondChange = scheduler.SetTransportAsync(
            finalTransport,
            TestContext.Current.CancellationToken);
        try
        {
            secondChange.IsCompleted.ShouldBeFalse();
        }
        finally
        {
            allowCancellationHandlerExit.TrySetResult();
            await Task.WhenAll(firstChange, secondChange).WaitAsync(
                TimeSpan.FromSeconds(3),
                TestContext.Current.CancellationToken);
        }

        scheduler.Transport.ShouldBeSameAs(finalTransport);
    }

    [Fact]
    public async Task EnableSchedule_Starts_Tasks_Queued_While_Scheduling_Was_Disabled()
    {
        var transport = new BlockingUploadTransport();
        var scheduler = new FileUploadScheduler(transport);
        var task = CreateUploadTask("resume.txt");

        scheduler.DisableSchedule();
        scheduler.EnqueueTask(task);
        transport.Started.Task.IsCompleted.ShouldBeFalse();

        scheduler.EnableSchedule();
        await transport.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);

        await scheduler.CancelAllAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SetTransportAsync_Cancels_Running_Task_Before_Replacing_Transport()
    {
        var oldTransport = new BlockingUploadTransport();
        var newTransport = new BlockingUploadTransport();
        var scheduler    = new FileUploadScheduler(oldTransport);
        var task         = CreateUploadTask("replace.txt");

        scheduler.EnqueueTask(task);
        await oldTransport.Started.Task.WaitAsync(
            TimeSpan.FromSeconds(3),
            TestContext.Current.CancellationToken);
        task.Status.ShouldBe(FileUploadStatus.Uploading);

        await scheduler.SetTransportAsync(newTransport, TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => task.Status == FileUploadStatus.Cancelled);

        scheduler.Transport.ShouldBeSameAs(newTransport);
        GetRunningTaskCount(scheduler).ShouldBe(0);
    }

    private static FileUploadTask CreateUploadTask(string name)
    {
        return new FileUploadTask
        {
            UploadFileInfo = new UploadFileInfo(
                name,
                new MemoryUploadFileSource(),
                new Uri($"file:///tmp/{name}"),
                12)
        };
    }

    private static int GetRunningTaskCount(FileUploadScheduler scheduler)
    {
        var field = typeof(FileUploadScheduler).GetField("_runningTasks", BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        var runningTasks = field.GetValue(scheduler);
        runningTasks.ShouldNotBeNull();
        var countProperty = runningTasks.GetType().GetProperty("Count");
        countProperty.ShouldNotBeNull();
        return countProperty.GetValue(runningTasks).ShouldBeAssignableTo<int>();
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        while (!condition())
        {
            timeout.Token.ThrowIfCancellationRequested();
            await Task.Delay(10, timeout.Token);
        }
    }

    private static void EnqueueWithSynchronizationContext(
        FileUploadScheduler scheduler,
        FileUploadTask task,
        SynchronizationContext context)
    {
        var previousContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(context);
        try
        {
            scheduler.EnqueueTask(task);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }

    private sealed class BlockingUploadTransport : IFileUploadTransport
    {
        public TaskCompletionSource<UploadFileInfo> Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> CancellationObserved { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly TaskCompletionSource<FileUploadResult> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Complete(FileUploadResult result)
        {
            _completion.TrySetResult(result);
        }

        public async Task<FileUploadResult> UploadAsync(
            UploadFileInfo fileInfo,
            object? context = null,
            IProgress<FileUploadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            using var registration = cancellationToken.Register(static state =>
            {
                ((TaskCompletionSource<bool>)state!).TrySetResult(true);
            }, CancellationObserved);
            Started.TrySetResult(fileInfo);

            return await _completion.Task.WaitAsync(cancellationToken);
        }
    }

    private sealed class ImmediateProgressSuccessTransport : IFileUploadTransport
    {
        public Task<FileUploadResult> UploadAsync(
            UploadFileInfo fileInfo,
            object? context = null,
            IProgress<FileUploadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            progress?.Report(new FileUploadProgress
            {
                BytesSent  = fileInfo.Size ?? 0,
                TotalBytes = fileInfo.Size ?? 0
            });
            return Task.FromResult(FileUploadResult.SuccessResult(
                new Uri("https://example.com/late-progress.txt"),
                fileInfo.Size ?? 0,
                TimeSpan.FromMilliseconds(1)));
        }
    }

    private sealed class QueuedSynchronizationContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<(SendOrPostCallback Callback, object? State)> _callbacks = new();

        internal int PendingCount => _callbacks.Count;

        public override void Post(SendOrPostCallback d, object? state)
        {
            _callbacks.Enqueue((d, state));
        }

        internal void RunAll()
        {
            while (_callbacks.TryDequeue(out var callback))
            {
                callback.Callback(callback.State);
            }
        }
    }

    private sealed class MemoryUploadFileSource : IUploadFileSource
    {
        public ValueTask<Stream> OpenReadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<Stream>(new MemoryStream([1], writable: false));
        }
    }

    private sealed class DelayedCancellationUploadTransport : IFileUploadTransport
    {
        internal TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource CancellationObserved { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource AllowExit { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<FileUploadResult> UploadAsync(
            UploadFileInfo fileInfo,
            object? context = null,
            IProgress<FileUploadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                CancellationObserved.TrySetResult();
                await AllowExit.Task;
                throw;
            }

            throw new InvalidOperationException("The delayed transport should only finish by cancellation.");
        }
    }

    private sealed class SynchronouslyBlockingUploadTransport : IFileUploadTransport
    {
        internal TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource AllowReturn { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<FileUploadResult> UploadAsync(
            UploadFileInfo fileInfo,
            object? context = null,
            IProgress<FileUploadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            AllowReturn.Task.GetAwaiter().GetResult();
            return Task.FromCanceled<FileUploadResult>(cancellationToken);
        }
    }

    private sealed class SynchronouslyBlockingSuccessfulUploadTransport : IFileUploadTransport
    {
        internal TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource AllowReturn { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<FileUploadResult> UploadAsync(
            UploadFileInfo fileInfo,
            object? context = null,
            IProgress<FileUploadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            AllowReturn.Task.GetAwaiter().GetResult();
            return Task.FromResult(FileUploadResult.SuccessResult(
                new Uri("https://example.com/non-blocking-enqueue.txt"),
                12,
                TimeSpan.FromMilliseconds(1)));
        }
    }
}
