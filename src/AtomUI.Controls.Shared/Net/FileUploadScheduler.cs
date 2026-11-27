using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace AtomUI.Controls;

/// <summary>
/// Queueing <c>IFileUploadScheduler</c>: queues upload tasks and runs at most <c>maxConcurrentTasks</c> of them at
/// the same time through <c>Transport</c>. <c>UploadQueue</c> in AtomUI.Desktop.Controls owns one.
/// </summary>
/// <remarks>
/// <c>EnqueueTask</c> requires a transport and starts pending tasks as long as scheduling is enabled and a slot is
/// free. A started task is marked <c>Uploading</c>, receives its cancellation token source and an
/// <c>ExecutionTask</c> that completes when the upload has ended, and reports progress and outcome only through
/// the handlers of the task.
/// </remarks>
internal class FileUploadScheduler : IFileUploadScheduler
{
    private readonly object _scheduleSyncRoot = new();
    private readonly SemaphoreSlim _maintenanceGate = new(1, 1);
    private SemaphoreSlim _concurrentSemaphore;
    private IFileUploadTransport? _transport;
    private readonly ConcurrentQueue<FileUploadTask> _pendingQueue = new();
    private readonly ConcurrentDictionary<Guid, FileUploadTask> _runningTasks = new();
    private int _isScheduleEnabledFlag = 1;

    public IFileUploadTransport? Transport
    {
        get
        {
            lock (_scheduleSyncRoot)
            {
                return _transport;
            }
        }
    }
    
    public FileUploadScheduler(IFileUploadTransport? transport = null, int maxConcurrentTasks = 3)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrentTasks, 1);
        _transport           = transport;
        _concurrentSemaphore = new SemaphoreSlim(maxConcurrentTasks);
    }
    
    public void EnqueueTask(FileUploadTask task)
    {
        Debug.Assert(Transport != null);
        _pendingQueue.Enqueue(task);
        TryStartNextUploads();
    }

    private void TryStartNextUploads()
    {
        while (TryPrepareNextUpload(
                   out var task,
                   out var transport,
                   out var cancellationTokenSource,
                   out var executionCompletion))
        {
            Debug.Assert(task.UploadFileInfo != null);

            var progress = new Progress<FileUploadProgress>(report =>
            {
                var taskProgress = report.Percentage;
                if (!TryRecordUploadProgress(task, taskProgress))
                {
                    return;
                }

                task.UploadProgressHandler?.Invoke(task.Id, task.UploadFileInfo, taskProgress);
            });

            var executionTask = Task.Run(() => ExecuteUploadAsync(
                task,
                transport,
                progress,
                cancellationTokenSource.Token));
            _ = ObserveExecutionTaskAsync(task, executionTask, executionCompletion);
        }
    }

    private bool TryPrepareNextUpload(
        out FileUploadTask task,
        out IFileUploadTransport transport,
        out CancellationTokenSource cancellationTokenSource,
        out TaskCompletionSource executionCompletion)
    {
        lock (_scheduleSyncRoot)
        {
            if (_transport is null || !IsScheduleEnabledCore() || !_concurrentSemaphore.Wait(0))
            {
                task = null!;
                transport = null!;
                cancellationTokenSource = null!;
                executionCompletion = null!;
                return false;
            }

            while (_pendingQueue.TryDequeue(out var candidate))
            {
                lock (candidate)
                {
                    if (candidate.Status != FileUploadStatus.Pending)
                    {
                        continue;
                    }

                    cancellationTokenSource = new CancellationTokenSource();
                    executionCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                    candidate.CancellationTokenSource = cancellationTokenSource;
                    candidate.ExecutionTask           = executionCompletion.Task;
                    candidate.Status                  = FileUploadStatus.Uploading;
                    _runningTasks.TryAdd(candidate.Id, candidate);
                    task = candidate;
                    transport = _transport;
                    return true;
                }
            }

            _concurrentSemaphore.Release();
            task = null!;
            transport = null!;
            cancellationTokenSource = null!;
            executionCompletion = null!;
            return false;
        }
    }

    private async Task ExecuteUploadAsync(
        FileUploadTask task,
        IFileUploadTransport transport,
        IProgress<FileUploadProgress> progress,
        CancellationToken cancellationToken)
    {
        FileUploadResult result;
        FileUploadStatus status;
        try
        {
            result = await transport.UploadAsync(
                task.UploadFileInfo!,
                task.Context,
                progress,
                cancellationToken);

            status = result.IsSuccess ? FileUploadStatus.Success : FileUploadStatus.Failed;
            if (!result.IsSuccess)
            {
                Debug.WriteLine(
                    $"Upload failed: {task.UploadFileInfo!.Path}, Reason: {result.UserFriendlyMessage}");
            }
        }
        catch (OperationCanceledException)
        {
            status = FileUploadStatus.Cancelled;
            result = FileUploadResult.CancelledResult("upload cancelled");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Upload error: {task.UploadFileInfo!.Path}, Error: {ex.Message}");
            status = FileUploadStatus.Failed;
            result = FileUploadResult.FailureResult(FileUploadErrorCode.Unknown, ex.Message);
        }
        SetUploadResult(task, status, result);
        try
        {
            switch (status)
            {
                case FileUploadStatus.Success:
                    task.UploadCompletedHandler?.Invoke(task.Id, task.UploadFileInfo!, result);
                    break;
                case FileUploadStatus.Cancelled:
                    task.UploadCancelledHandler?.Invoke(task.Id, task.UploadFileInfo!, result);
                    break;
                default:
                    task.UploadFailedHandler?.Invoke(task.Id, task.UploadFileInfo!, result);
                    break;
            }
        }
        finally
        {
            lock (task)
            {
                task.CancellationTokenSource?.Dispose();
                task.CancellationTokenSource = null;
            }
            _runningTasks.TryRemove(task.Id, out _);
            _concurrentSemaphore.Release();
            TryStartNextUploads();
        }
    }

    private static async Task ObserveExecutionTaskAsync(
        FileUploadTask uploadTask,
        Task executionTask,
        TaskCompletionSource executionCompletion)
    {
        try
        {
            await executionTask.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Upload completion callback failed: {ex.Message}");
        }
        finally
        {
            executionCompletion.TrySetResult();
            lock (uploadTask)
            {
                if (ReferenceEquals(uploadTask.ExecutionTask, executionCompletion.Task))
                {
                    uploadTask.ExecutionTask = null;
                }
            }
        }
    }

    private static bool TryRecordUploadProgress(FileUploadTask task, double progress)
    {
        lock (task)
        {
            if (task.Status != FileUploadStatus.Uploading)
            {
                return false;
            }

            task.Progress = progress;
            return true;
        }
    }

    private static void SetUploadResult(
        FileUploadTask task,
        FileUploadStatus status,
        FileUploadResult result)
    {
        lock (task)
        {
            task.Result = result;
            task.Status = status;
        }
    }
    
    public async Task CancelUploadAsync(FileUploadTask task)
    {
        Task? executionTask;
        var notifyPendingCancellation = false;
        lock (task)
        {
            executionTask = task.ExecutionTask;
            if (task.Status == FileUploadStatus.Uploading)
            {
                task.CancellationTokenSource?.Cancel();
            }
            else if (task.Status == FileUploadStatus.Pending)
            {
                task.Status = FileUploadStatus.Cancelled;
                notifyPendingCancellation = true;
            }
        }

        if (notifyPendingCancellation)
        {
            task.UploadCancelledHandler?.Invoke(task.Id,
                task.UploadFileInfo!,
                FileUploadResult.CancelledResult("upload cancelled"));
        }

        if (executionTask is not null)
        {
            await executionTask.ConfigureAwait(false);
        }
    }
    
    public async Task CancelAllAsync(CancellationToken cancellationToken = default)
    {
        await _maintenanceGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var wasScheduleEnabled = DisableScheduleCore();
            try
            {
                await CancelAllCoreAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (wasScheduleEnabled)
                {
                    EnableSchedule();
                }
            }
        }
        finally
        {
            _maintenanceGate.Release();
        }
    }

    public async Task SetMaxConcurrentTasksAsync(int taskCount, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskCount, 1);
        await _maintenanceGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var wasScheduleEnabled = DisableScheduleCore();
            try
            {
                await CancelAllCoreAsync(cancellationToken).ConfigureAwait(false);
                lock (_scheduleSyncRoot)
                {
                    _concurrentSemaphore.Dispose();
                    _concurrentSemaphore = new SemaphoreSlim(taskCount);
                }
            }
            finally
            {
                if (wasScheduleEnabled)
                {
                    EnableSchedule();
                }
            }
        }
        finally
        {
            _maintenanceGate.Release();
        }
    }

    public async Task SetTransportAsync(IFileUploadTransport? transport, CancellationToken cancellationToken = default)
    {
        await _maintenanceGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var wasScheduleEnabled = DisableScheduleCore();
            try
            {
                await CancelAllCoreAsync(cancellationToken).ConfigureAwait(false);
                lock (_scheduleSyncRoot)
                {
                    _transport = transport;
                }
            }
            finally
            {
                if (wasScheduleEnabled)
                {
                    EnableSchedule();
                }
            }
        }
        finally
        {
            _maintenanceGate.Release();
        }
    }

    public void DisableSchedule()
    {
        DisableScheduleCore();
    }

    public void EnableSchedule()
    {
        lock (_scheduleSyncRoot)
        {
            Volatile.Write(ref _isScheduleEnabledFlag, 1);
        }
        TryStartNextUploads();
    }

    public bool IsScheduleEnabled()
    {
        return Volatile.Read(ref _isScheduleEnabledFlag) == 1;
    }

    private async Task CancelAllCoreAsync(CancellationToken cancellationToken)
    {
        FileUploadTask[] pendingTasks;
        FileUploadTask[] runningTasks;
        lock (_scheduleSyncRoot)
        {
            var pending = new List<FileUploadTask>();
            while (_pendingQueue.TryDequeue(out var pendingTask))
            {
                pending.Add(pendingTask);
            }
            pendingTasks = pending.ToArray();
            runningTasks = _runningTasks.Values.ToArray();
        }

        List<Exception>? callbackExceptions = null;
        foreach (var pendingTask in pendingTasks)
        {
            var notifyCancellation = false;
            lock (pendingTask)
            {
                if (pendingTask.Status == FileUploadStatus.Pending)
                {
                    pendingTask.Status = FileUploadStatus.Cancelled;
                    notifyCancellation = true;
                }
            }

            if (!notifyCancellation)
            {
                continue;
            }

            try
            {
                pendingTask.UploadCancelledHandler?.Invoke(
                    pendingTask.Id,
                    pendingTask.UploadFileInfo!,
                    FileUploadResult.CancelledResult("upload cancelled"));
            }
            catch (Exception ex)
            {
                (callbackExceptions ??= []).Add(ex);
            }
        }

        var runningCancellationTasks = new Task[runningTasks.Length];
        for (var index = 0; index < runningTasks.Length; index++)
        {
            runningCancellationTasks[index] = CancelUploadAsync(runningTasks[index]);
        }

        if (runningCancellationTasks.Length > 0)
        {
            await Task.WhenAll(runningCancellationTasks)
                      .WaitAsync(cancellationToken)
                      .ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (callbackExceptions is { Count: 1 })
        {
            ExceptionDispatchInfo.Capture(callbackExceptions[0]).Throw();
        }
        if (callbackExceptions is { Count: > 1 })
        {
            throw new AggregateException(callbackExceptions);
        }
    }

    private bool DisableScheduleCore()
    {
        lock (_scheduleSyncRoot)
        {
            return Interlocked.Exchange(ref _isScheduleEnabledFlag, 0) == 1;
        }
    }

    private bool IsScheduleEnabledCore()
    {
        return Volatile.Read(ref _isScheduleEnabledFlag) == 1;
    }
}
