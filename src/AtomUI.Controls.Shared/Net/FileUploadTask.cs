namespace AtomUI.Controls;

/// <summary>
/// One upload that <c>FileUploadScheduler</c> carries out: the file to upload, its status and progress, and the
/// handlers that receive its progress and outcome. <c>UploadQueue</c> in AtomUI.Desktop.Controls keeps its tasks.
/// </summary>
/// <remarks>
/// Each task receives a new <c>Id</c>. <c>Progress</c> is the uploaded percentage reported by the transport. The
/// scheduler sets <c>Status</c>, <c>Result</c>, <c>CancellationTokenSource</c> and <c>ExecutionTask</c>, and invokes
/// exactly one of the completed, canceled or failed handlers when the upload has ended.
/// </remarks>
internal class FileUploadTask
{
    public Guid Id { get; set; }
    public FileUploadStatus Status { get; set; } = FileUploadStatus.Pending;
    public double Progress { get; set; }
    public UploadFileInfo? UploadFileInfo {  get; set; }
    public object? Context { get; set; }
    public FileUploadResult? Result { get; set; }
    public CancellationTokenSource? CancellationTokenSource { get; set; }
    internal Task? ExecutionTask { get; set; }
    
    public Action<Guid, UploadFileInfo, double>? UploadProgressHandler { get; set; }
    public Action<Guid, UploadFileInfo, FileUploadResult>? UploadCompletedHandler { get; set; }
    public Action<Guid, UploadFileInfo, FileUploadResult>? UploadFailedHandler { get; set; }
    public Action<Guid, UploadFileInfo, FileUploadResult>? UploadCancelledHandler { get; set; }

    public FileUploadTask()
    {
        Id = Guid.NewGuid();
    }
}
