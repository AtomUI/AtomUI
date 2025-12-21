using AtomUI.Controls;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public class UploadTaskInfo : AvaloniaObject
{
    public static readonly DirectProperty<UploadTaskInfo, Guid> TaskIdProperty =
        AvaloniaProperty.RegisterDirect<UploadTaskInfo, Guid>(
            nameof(TaskId),
            o => o.TaskId,
            (o, v) => o.TaskId = v);
    
    public static readonly DirectProperty<UploadTaskInfo, string?> FileNameProperty =
        AvaloniaProperty.RegisterDirect<UploadTaskInfo, string?>(
            nameof(FileName),
            o => o.FileName,
            (o, v) => o.FileName = v);
    
    public static readonly DirectProperty<UploadTaskInfo, double> ProgressProperty =
        AvaloniaProperty.RegisterDirect<UploadTaskInfo, double>(
            nameof(Progress),
            o => o.Progress,
            (o, v) => o.Progress = v);
    
    public static readonly DirectProperty<UploadTaskInfo, bool> IsImageFileProperty =
        AvaloniaProperty.RegisterDirect<UploadTaskInfo, bool>(
            nameof(IsImageFile),
            o => o.IsImageFile,
            (o, v) => o.IsImageFile = v);
    
    public static readonly DirectProperty<UploadTaskInfo, FileUploadStatus> StatusProperty =
        AvaloniaProperty.RegisterDirect<UploadTaskInfo, FileUploadStatus>(
            nameof(Status),
            o => o.Status,
            (o, v) => o.Status = v);
    
    public static readonly DirectProperty<UploadTaskInfo, string?> ErrorMessageProperty =
        AvaloniaProperty.RegisterDirect<UploadTaskInfo, string?>(
            nameof(ErrorMessage),
            o => o.ErrorMessage,
            (o, v) => o.ErrorMessage = v);
    
    public static readonly DirectProperty<UploadTaskInfo, Uri?> FilePathProperty =
        AvaloniaProperty.RegisterDirect<UploadTaskInfo, Uri?>(
            nameof(FilePath),
            o => o.FilePath,
            (o, v) => o.FilePath = v);
    
    private Guid _taskId;

    public Guid TaskId
    {
        get => _taskId;
        set => SetAndRaise(TaskIdProperty, ref _taskId, value);
    }
    
    private double _progress;

    public double Progress
    {
        get => _progress;
        set => SetAndRaise(ProgressProperty, ref _progress, value);
    }
    
    private bool _isImageFile;

    public bool IsImageFile
    {
        get => _isImageFile;
        set => SetAndRaise(IsImageFileProperty, ref _isImageFile, value);
    }
    
    private string? _fileName;

    public string? FileName
    {
        get => _fileName;
        set => SetAndRaise(FileNameProperty, ref _fileName, value);
    }
    
    private FileUploadStatus _status;

    public FileUploadStatus Status
    {
        get => _status;
        set => SetAndRaise(StatusProperty, ref _status, value);
    }
    
    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetAndRaise(ErrorMessageProperty, ref _errorMessage, value);
    }
    
    private Uri? _filePath;

    public Uri? FilePath
    {
        get => _filePath;
        set => SetAndRaise(FilePathProperty, ref _filePath, value);
    }
    
    internal FileUploadTask? UploadTask { get; set; }
    internal bool IsPictureTriggerTask { get; set; }
}