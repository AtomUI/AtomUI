using System.Collections.Concurrent;
using System.Diagnostics;
using Avalonia.Threading;

namespace AtomUI.Controls;

internal class FileUploadScheduler : IFileUploadScheduler
{
    private SemaphoreSlim _concurrentSemaphore;
    private IFileUploadTransport? _transport;
    private readonly ConcurrentQueue<FileUploadTask> _pendingQueue = new();
    private readonly ConcurrentBag<FileUploadTask> _runningTasks = new();
    private int _isScheduleEnabledFlag = 1;

    public IFileUploadTransport? Transport => _transport;
    
    public FileUploadScheduler(IFileUploadTransport? transport = null, int maxConcurrentTasks = 3)
    {
        _transport           = transport;
        _concurrentSemaphore = new SemaphoreSlim(maxConcurrentTasks);
    }
    
    public void EnqueueTask(FileUploadTask task)
    {
        Debug.Assert(_transport != null);
        _pendingQueue.Enqueue(task);
        _ = TryStartNextUploadAsync();
    }
    
    private async Task TryStartNextUploadAsync()
    {
        if (_transport == null || !IsScheduleEnabled())
        {
            return;
        }

        while (_concurrentSemaphore.CurrentCount > 0 && _pendingQueue.TryDequeue(out var task))
        {
            if (task.Status != FileUploadStatus.Pending)
            {
                continue;
            }
            
            await _concurrentSemaphore.WaitAsync();
            
            if (task.Status != FileUploadStatus.Pending)
            {
                _concurrentSemaphore.Release();
                continue; 
            }
            
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken       = cancellationTokenSource.Token;
            task.CancellationTokenSource = cancellationTokenSource;
   
            Debug.Assert(task.UploadFileInfo != null);
            
            var progress = new Progress<FileUploadProgress>(report =>
            {
                task.Progress = report.Percentage;
                task.UploadProgressHandler?.Invoke(task.Id, task.UploadFileInfo, task.Progress);
            });
            
            _ = Task.Run(async () =>
            {
                FileUploadResult? result = null;
                try
                {
                    task.Status = FileUploadStatus.Uploading;
                    result = await _transport.UploadAsync(
                        task.UploadFileInfo,
                        task.Context,
                        progress,
                        cancellationToken
                    );

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        task.Result = result;
                        task.Status = result.IsSuccess ? FileUploadStatus.Success : FileUploadStatus.Failed;
                        if (!result.IsSuccess)
                        {
                            Debug.WriteLine(
                                $"Upload failed: {task.UploadFileInfo.FilePath}, Reason: {result.UserFriendlyMessage}");
                            task.UploadFailedHandler?.Invoke(task.Id, task.UploadFileInfo, result);
                        }
                        else
                        {
                            task.UploadCompletedHandler?.Invoke(task.Id, task.UploadFileInfo, result);
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        task.Status = FileUploadStatus.Cancelled;
                        task.UploadCancelledHandler?.Invoke(task.Id, task.UploadFileInfo, FileUploadResult.CancelledResult("upload cancelled"));
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Upload error: {task.UploadFileInfo.FilePath}, Error: {ex.Message}");
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        task.Status = FileUploadStatus.Failed;
                        task.UploadFailedHandler?.Invoke(task.Id, task.UploadFileInfo, FileUploadResult.FailureResult(FileUploadErrorCode.Unknown, ex.Message));
                    });
                }
                finally
                {
                    _concurrentSemaphore.Release();
                    Dispatcher.UIThread.Post(() =>
                    {
                        task.CancellationTokenSource?.Dispose();
                        task.CancellationTokenSource = null;
                    });
                    _ = TryStartNextUploadAsync();
                }
            }, cancellationToken);
        }
    }
    
    public async Task CancelUploadAsync(FileUploadTask task)
    {
        if (task.Status == FileUploadStatus.Uploading)
        {
            if (task.CancellationTokenSource != null)
            {
                await task.CancellationTokenSource.CancelAsync();
            }
            task.CancellationTokenSource = null;
        }
        else if (task.Status == FileUploadStatus.Pending)
        {
            task.Status = FileUploadStatus.Cancelled;
        }
    }
    
    public async Task CancelAllAsync()
    {
        DisableSchedule();
        while (_runningTasks.Count > 0) 
        {
            if (_runningTasks.TryTake(out var uploadInfo))
            {
                await CancelUploadAsync(uploadInfo);
            }
        }
    }

    public async Task SetMaxConcurrentTasksAsync(int taskCount)
    {
        await CancelAllAsync();
        _concurrentSemaphore.Dispose();
        _concurrentSemaphore = new SemaphoreSlim(taskCount);
        EnableSchedule();
    }

    public async Task SetTransportAsync(IFileUploadTransport transport)
    {
        await CancelAllAsync();
        _transport = transport;
        EnableSchedule();
    }

    public void DisableSchedule()
    {
        Interlocked.Exchange(ref _isScheduleEnabledFlag, 0);
    }

    public void EnableSchedule()
    {
        Interlocked.Exchange(ref _isScheduleEnabledFlag, 1);
    }

    public bool IsScheduleEnabled()
    {
        return Interlocked.CompareExchange(ref _isScheduleEnabledFlag, 1, 1) == 1;
    }
}