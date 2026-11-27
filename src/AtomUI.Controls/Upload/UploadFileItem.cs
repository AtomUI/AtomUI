using AtomUI.Controls;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// One file in the file list of an <c>Upload</c>. It holds the name and location of the file, the state of its
/// upload and its result.
/// </summary>
/// <remarks>
/// The control creates an item for every file that passes its input pipeline. The app can add items of its own.
/// The list binds an item to its list item container, so a change to a property updates what the list shows.
/// </remarks>
public class UploadFileItem : AvaloniaObject
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Id" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadFileItem, Guid> IdProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, Guid>(
            nameof(Id),
            o => o.Id,
            (o, v) => o.Id = v);

    /// <summary>
    /// Identifies the <see cref="Name" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadFileItem, string?> NameProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, string?>(
            nameof(Name),
            o => o.Name,
            (o, v) => o.Name = v);

    /// <summary>
    /// Identifies the <see cref="Path" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadFileItem, Uri?> PathProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, Uri?>(
            nameof(Path),
            o => o.Path,
            (o, v) => o.Path = v);

    /// <summary>
    /// Identifies the <see cref="Size" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadFileItem, long> SizeProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, long>(
            nameof(Size),
            o => o.Size,
            (o, v) => o.Size = v);

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadFileItem, FileUploadStatus> StatusProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, FileUploadStatus>(
            nameof(Status),
            o => o.Status,
            (o, v) => o.Status = v);

    /// <summary>
    /// Identifies the <see cref="Progress" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadFileItem, double> ProgressProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, double>(
            nameof(Progress),
            o => o.Progress,
            (o, v) => o.Progress = v);

    /// <summary>
    /// Identifies the <see cref="ErrorMessage" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadFileItem, string?> ErrorMessageProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, string?>(
            nameof(ErrorMessage),
            o => o.ErrorMessage,
            (o, v) => o.ErrorMessage = v);

    /// <summary>
    /// Identifies the <see cref="Result" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadFileItem, FileUploadResult?> ResultProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, FileUploadResult?>(
            nameof(Result),
            o => o.Result,
            (o, v) => o.Result = v);

    /// <summary>
    /// Identifies the <see cref="UserData" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadFileItem, object?> UserDataProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, object?>(
            nameof(UserData),
            o => o.UserData,
            (o, v) => o.UserData = v);

    /// <summary>
    /// Identifies the <see cref="PendingText" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadFileItem, string?> PendingTextProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, string?>(
            nameof(PendingText),
            o => o.PendingText,
            (o, v) => o.PendingText = v);

    /// <summary>
    /// Identifies the <see cref="IsImageFile" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadFileItem, bool> IsImageFileProperty =
        AvaloniaProperty.RegisterDirect<UploadFileItem, bool>(
            nameof(IsImageFile),
            o => o.IsImageFile,
            (o, v) => o.IsImageFile = v);

    private Guid _id = Guid.NewGuid();

    /// <summary>
    /// Identifies the file inside its <c>Upload</c>. A new item gets a random unique value.
    /// </summary>
    /// <remarks>
    /// Ids must be unique within one <c>Upload</c>. The task events report the id,
    /// <c>Upload.RemoveFileAsync</c> takes it to remove a file. Assign your own value before the item is added to
    /// <c>Upload.Files</c>. The control keys the accepted file info, the queue task and the auto-removal timer by the
    /// id the item has when each of them is registered.
    /// </remarks>
    public Guid Id
    {
        get => _id;
        set => SetAndRaise(IdProperty, ref _id, value);
    }

    private string? _name;

    /// <summary>
    /// Name of the file as the list item shows it. For a picked file it is the name of the storage file.
    /// </summary>
    public string? Name
    {
        get => _name;
        set => SetAndRaise(NameProperty, ref _name, value);
    }

    private Uri? _path;

    /// <summary>
    /// Location of the file as a URI, or <see langword="null" /> when the file has none.
    /// </summary>
    /// <remarks>
    /// For a picked file it is the path of the storage file. A picture list item loads it as the preview
    /// image, which requires an absolute URI.
    /// </remarks>
    public Uri? Path
    {
        get => _path;
        set => SetAndRaise(PathProperty, ref _path, value);
    }

    private long _size;

    /// <summary>
    /// Size of the file in bytes. It is 0 when the size is unknown.
    /// </summary>
    public long Size
    {
        get => _size;
        set => SetAndRaise(SizeProperty, ref _size, value);
    }

    private FileUploadStatus _status = FileUploadStatus.Pending;

    /// <summary>
    /// State of the file's upload. The list item chooses its content by this value.
    /// </summary>
    /// <remarks>
    /// A new item is <see cref="FileUploadStatus.Pending" />. Once the value is
    /// <see cref="FileUploadStatus.Success" />, <see cref="FileUploadStatus.Failed" /> or
    /// <see cref="FileUploadStatus.Cancelled" />, the control drops the progress reports for the item.
    /// </remarks>
    public FileUploadStatus Status
    {
        get => _status;
        set => SetAndRaise(StatusProperty, ref _status, value);
    }

    private double _progress;

    /// <summary>
    /// Portion of the file that has been uploaded, as a percentage from 0 to 100.
    /// </summary>
    /// <remarks>
    /// While the item is uploading, the control updates it from the transport's progress reports and the list
    /// item shows it on a progress bar. The value is 0 when the total size is unknown.
    /// </remarks>
    public double Progress
    {
        get => _progress;
        set => SetAndRaise(ProgressProperty, ref _progress, value);
    }

    private string? _errorMessage;

    /// <summary>
    /// Message that explains why the upload failed or was canceled, or <see langword="null" /> when there is none.
    /// </summary>
    /// <remarks>
    /// For a failed or canceled task it is the user-friendly message of <see cref="Result" />.
    /// When scheduling is canceled through the about-to-schedule event, it is the cancel reason.
    /// The list item shows it as a tooltip for a failed item, and a picture list item also for a canceled one.
    /// </remarks>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetAndRaise(ErrorMessageProperty, ref _errorMessage, value);
    }

    private FileUploadResult? _result;

    /// <summary>
    /// Outcome of the upload, or <see langword="null" /> until the upload has ended.
    /// </summary>
    /// <remarks>
    /// The control sets it when the upload succeeds, fails or is canceled. It does not set it when scheduling was
    /// canceled, although the status is then failed. When <c>Upload.FileValueMode</c> is
    /// <c>UploadFileValueMode.Results</c>, the form value is an array of the results of the files that succeeded.
    /// </remarks>
    public FileUploadResult? Result
    {
        get => _result;
        set => SetAndRaise(ResultProperty, ref _result, value);
    }

    private object? _userData;

    /// <summary>
    /// Data of the app that is carried along with the file, or <see langword="null" /> for none.
    /// </summary>
    /// <remarks>
    /// For a picked file it is the value of <c>Upload.ExtraContext</c> when the item is created. When the item is
    /// uploaded, it is passed as the <c>context</c> argument of <see cref="IFileUploadTransport.UploadAsync" />.
    /// </remarks>
    public object? UserData
    {
        get => _userData;
        set => SetAndRaise(UserDataProperty, ref _userData, value);
    }

    private string? _pendingText;

    /// <summary>
    /// Text that describes the pending state of the file, or <see langword="null" /> for none.
    /// </summary>
    /// <remarks>
    /// For a picked file it is the value of <c>Upload.PendingText</c> when the item is created. No built-in list
    /// template displays it.
    /// </remarks>
    public string? PendingText
    {
        get => _pendingText;
        set => SetAndRaise(PendingTextProperty, ref _pendingText, value);
    }

    private bool _isImageFile;

    /// <summary>
    /// Whether the file is an image.
    /// </summary>
    /// <remarks>
    /// For a picked file the control decides this with <c>Upload.IsImageFilePredicate</c>, or else by the
    /// extension of the file name. In a picture list, the item shows the preview from <see cref="Path" /> only
    /// when this is <see langword="true" /> and the status is <see cref="FileUploadStatus.Success" />.
    /// </remarks>
    public bool IsImageFile
    {
        get => _isImageFile;
        set => SetAndRaise(IsImageFileProperty, ref _isImageFile, value);
    }

    #endregion
}
