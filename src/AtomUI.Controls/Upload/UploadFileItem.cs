using AtomUI.Controls;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public class UploadFileItem : AvaloniaObject
{
    #region 公共属性定义

    public static readonly DirectProperty<UploadFileItem, Guid> IdProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, Guid>(
            nameof(Id),
            o => o.Id,
            (o, v) => o.Id = v);

    public static readonly DirectProperty<UploadFileItem, string?> NameProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, string?>(
            nameof(Name),
            o => o.Name,
            (o, v) => o.Name = v);

    public static readonly DirectProperty<UploadFileItem, Uri?> PathProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, Uri?>(
            nameof(Path),
            o => o.Path,
            (o, v) => o.Path = v);

    public static readonly DirectProperty<UploadFileItem, long> SizeProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, long>(
            nameof(Size),
            o => o.Size,
            (o, v) => o.Size = v);

    public static readonly DirectProperty<UploadFileItem, FileUploadStatus> StatusProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, FileUploadStatus>(
            nameof(Status),
            o => o.Status,
            (o, v) => o.Status = v);

    public static readonly DirectProperty<UploadFileItem, double> ProgressProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, double>(
            nameof(Progress),
            o => o.Progress,
            (o, v) => o.Progress = v);

    public static readonly DirectProperty<UploadFileItem, string?> ErrorMessageProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, string?>(
            nameof(ErrorMessage),
            o => o.ErrorMessage,
            (o, v) => o.ErrorMessage = v);

    public static readonly DirectProperty<UploadFileItem, FileUploadResult?> ResultProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, FileUploadResult?>(
            nameof(Result),
            o => o.Result,
            (o, v) => o.Result = v);

    public static readonly DirectProperty<UploadFileItem, object?> UserDataProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, object?>(
            nameof(UserData),
            o => o.UserData,
            (o, v) => o.UserData = v);

    public static readonly DirectProperty<UploadFileItem, string?> PendingTextProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, string?>(
            nameof(PendingText),
            o => o.PendingText,
            (o, v) => o.PendingText = v);

    public static readonly DirectProperty<UploadFileItem, bool> IsImageFileProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, bool>(
            nameof(IsImageFile),
            o => o.IsImageFile,
            (o, v) => o.IsImageFile = v);

    private Guid _id = Guid.NewGuid();

    public Guid Id
    {
        get => _id;
        set => SetAndRaise(IdProperty, ref _id, value);
    }

    private string? _name;

    public string? Name
    {
        get => _name;
        set => SetAndRaise(NameProperty, ref _name, value);
    }

    private Uri? _path;

    public Uri? Path
    {
        get => _path;
        set => SetAndRaise(PathProperty, ref _path, value);
    }

    private long _size;

    public long Size
    {
        get => _size;
        set => SetAndRaise(SizeProperty, ref _size, value);
    }

    private FileUploadStatus _status = FileUploadStatus.Pending;

    public FileUploadStatus Status
    {
        get => _status;
        set => SetAndRaise(StatusProperty, ref _status, value);
    }

    private double _progress;

    public double Progress
    {
        get => _progress;
        set => SetAndRaise(ProgressProperty, ref _progress, value);
    }

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetAndRaise(ErrorMessageProperty, ref _errorMessage, value);
    }

    private FileUploadResult? _result;

    public FileUploadResult? Result
    {
        get => _result;
        set => SetAndRaise(ResultProperty, ref _result, value);
    }

    private object? _userData;

    public object? UserData
    {
        get => _userData;
        set => SetAndRaise(UserDataProperty, ref _userData, value);
    }

    private string? _pendingText;

    public string? PendingText
    {
        get => _pendingText;
        set => SetAndRaise(PendingTextProperty, ref _pendingText, value);
    }

    private bool _isImageFile;

    public bool IsImageFile
    {
        get => _isImageFile;
        set => SetAndRaise(IsImageFileProperty, ref _isImageFile, value);
    }

    #endregion
}
