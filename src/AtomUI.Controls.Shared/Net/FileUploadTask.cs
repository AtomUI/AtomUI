namespace AtomUI.Controls;

public class FileUploadTask
{
    public Guid Id { get; set; }
    public FileUploadStatus Status { get; set; } = FileUploadStatus.Pending;
    public double Progress { get; set; }
    public UploadFileInfo? UploadFileInfo {  get; set; }
    public object? Context { get; set; }
    public FileUploadResult? Result { get; set; }
    public CancellationTokenSource? CancellationTokenSource { get; set; }
    
    public Action<Guid, UploadFileInfo, double>? UploadProgressHandler { get; set; }
    public Action<Guid, UploadFileInfo, FileUploadResult>? UploadCompletedHandler { get; set; }
    public Action<Guid, UploadFileInfo, FileUploadResult>? UploadFailedHandler { get; set; }
    public Action<Guid, UploadFileInfo, FileUploadResult>? UploadCancelledHandler { get; set; }

    public FileUploadTask()
    {
        Id = Guid.NewGuid();
    }
}