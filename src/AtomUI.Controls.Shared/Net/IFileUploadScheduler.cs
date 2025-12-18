namespace AtomUI.Controls;

public interface IFileUploadScheduler
{
    IFileUploadTransport? Transport { get; }
    void EnqueueTask(FileUploadTask task);
    Task CancelUploadAsync(FileUploadTask task);
    Task CancelAllAsync(); 
    void DisableSchedule();
    void EnableSchedule();
    bool IsScheduleEnabled();
    Task SetMaxConcurrentTasksAsync(int taskCount);
    Task SetTransportAsync(IFileUploadTransport transport);
}