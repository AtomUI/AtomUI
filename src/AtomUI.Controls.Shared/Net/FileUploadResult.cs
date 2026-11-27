namespace AtomUI.Controls;

/// <summary>
/// Category of the failure or cancellation of an upload.
/// A result carries <see cref="None" /> unless it failed or was canceled.
/// </summary>
public enum FileUploadErrorCode
{
    /// <summary>
    /// No error. The default of a result that neither failed nor was canceled.
    /// </summary>
    None = 0,
    /// <summary>
    /// The connection to the server failed or timed out.
    /// </summary>
    NetworkError,
    /// <summary>
    /// The server answered with a 5xx status.
    /// </summary>
    ServerError,
    /// <summary>
    /// The server rejected the request with a 4xx status, for example because
    /// authentication failed or a parameter was invalid.
    /// </summary>
    ClientError,
    /// <summary>
    /// The local file could not be read, for example because it is missing or access is denied.
    /// </summary>
    FileError,
    /// <summary>
    /// The file failed validation, for example because its type or size is not allowed.
    /// </summary>
    ValidationFailed,
    /// <summary>
    /// The upload was canceled before it finished.
    /// </summary>
    Cancelled,
    /// <summary>
    /// An error that belongs to none of the other categories.
    /// The scheduler reports it when the transport throws an exception.
    /// </summary>
    Unknown
}

/// <summary>
/// Life-cycle state of one upload. Upload list items expose it to their theme to select the appearance.
/// </summary>
public enum FileUploadStatus
{
    /// <summary>
    /// The upload is queued and has not started.
    /// </summary>
    Pending,
    /// <summary>
    /// The transport is sending the file.
    /// </summary>
    Uploading,
    /// <summary>
    /// The transport returned a result whose <see cref="FileUploadResult.IsSuccess" /> is <see langword="true" />.
    /// </summary>
    Success,
    /// <summary>
    /// The transport returned an unsuccessful result or threw an exception that is not a cancellation.
    /// </summary>
    Failed,
    /// <summary>
    /// The upload was canceled, while it was running or while it was still queued.
    /// </summary>
    Cancelled
}

/// <summary>
/// Outcome of one upload. The transport returns it, the scheduler stores it on the task
/// and passes it to the handlers of the finished task.
/// </summary>
public record FileUploadResult
{
    /// <summary>
    /// Final state of the upload. A result created without the factory methods keeps the default
    /// <see cref="FileUploadStatus.Pending" />.
    /// </summary>
    public FileUploadStatus Status { get; init; } = FileUploadStatus.Pending;
    
    /// <summary>
    /// Is <see langword="true" /> when <see cref="Status" /> is <see cref="FileUploadStatus.Success" />.
    /// </summary>
    public bool IsSuccess => Status == FileUploadStatus.Success;
    
    /// <summary>
    /// Category of the failure or cancellation. <see cref="FileUploadErrorCode.None" /> when neither happened.
    /// </summary>
    public FileUploadErrorCode ErrorCode { get; init; } = FileUploadErrorCode.None;
    
    /// <summary>
    /// Message that describes the failure or cancellation and is suitable to show to the user.
    /// <see langword="null" /> when none was supplied.
    /// </summary>
    public string? UserFriendlyMessage { get; init; }
    
    /// <summary>
    /// Detailed error text for debugging and logging.
    /// <see langword="null" /> unless the transport supplied one.
    /// </summary>
    public string? InternalErrorMessage { get; init; }
    
    /// <summary>
    /// The exception that caused the failure, when the transport supplied one; otherwise <see langword="null" />.
    /// </summary>
    public Exception? Exception { get; init; }
    
    /// <summary>
    /// HTTP or HTTPS address where the uploaded file can be accessed on the server.
    /// <see langword="null" /> on failed and canceled results.
    /// </summary>
    public Uri? RemoteUrl { get; init; }
    
    /// <summary>
    /// Identifier the server assigned to the stored file, for later requests such as deleting or querying it.
    /// <see langword="null" /> when the transport did not supply one.
    /// </summary>
    public string? FileId { get; init; }
    
    /// <summary>
    /// ETag or hash of the file as returned by the server, for integrity checks.
    /// <see langword="null" /> when the transport did not supply one.
    /// </summary>
    public string? FileHash { get; init; }

    // ---------- Performance and metadata ----------
    /// <summary>
    /// Total size of the file in bytes.
    /// </summary>
    public long FileSize { get; init; }
    
    /// <summary>
    /// Time the upload took, as reported by the transport.
    /// </summary>
    public TimeSpan ElapsedTime { get; init; }
    
    /// <summary>
    /// Average upload speed in bytes per second, computed from <see cref="FileSize" />
    /// and <see cref="ElapsedTime" />. <c>0</c> when either of them is not positive.
    /// </summary>
    public double AverageSpeed => FileSize > 0 && ElapsedTime.TotalSeconds > 0 
        ? FileSize / ElapsedTime.TotalSeconds : 0;
    
    /// <summary>
    /// Session identifier for resuming an interrupted upload.
    /// <see cref="SuccessResult" /> does not set it, so it is <see langword="null" /> unless set directly.
    /// </summary>
    public string? ResumableSessionId { get; init; }
    
    /// <summary>
    /// Number of bytes that were uploaded successfully in a chunked upload.
    /// <see cref="SuccessResult" /> does not set it, so it is <c>0</c> unless set directly.
    /// </summary>
    public long BytesUploaded { get; init; }
    
    /// <summary>
    /// Custom metadata returned by the server, such as the width and height of an image or the duration of a video.
    /// <see langword="null" /> when the server returned none.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
    
    /// <summary>
    /// Creates the result of a successful upload.
    /// </summary>
    /// <returns>
    /// A result whose <see cref="Status" /> is <see cref="FileUploadStatus.Success" />. It carries the given
    /// address, size in bytes and duration, plus the server data that was supplied.
    /// </returns>
    public static FileUploadResult SuccessResult(
        Uri remoteUrl, 
        long fileSize, 
        TimeSpan elapsedTime,
        string? fileId = null,
        string? fileHash = null,
        Dictionary<string, object>? metadata = null)
    {
        return new FileUploadResult
        {
            Status = FileUploadStatus.Success,
            RemoteUrl = remoteUrl,
            FileSize = fileSize,
            ElapsedTime = elapsedTime,
            FileId = fileId,
            FileHash = fileHash,
            Metadata = metadata
        };
    }
    
    /// <summary>
    /// Creates the result of a failed upload.
    /// </summary>
    /// <returns>
    /// A result whose <see cref="Status" /> is <see cref="FileUploadStatus.Failed" />, carrying the given
    /// error category, messages and exception.
    /// </returns>
    public static FileUploadResult FailureResult(
        FileUploadErrorCode errorCode,
        string userFriendlyMessage,
        string? internalMessage = null,
        Exception? exception = null)
    {
        return new FileUploadResult
        {
            Status = FileUploadStatus.Failed,
            ErrorCode = errorCode,
            UserFriendlyMessage = userFriendlyMessage,
            InternalErrorMessage = internalMessage,
            Exception = exception
        };
    }
    
    /// <summary>
    /// Creates the result of a canceled upload.
    /// </summary>
    /// <returns>
    /// A result whose <see cref="Status" /> is <see cref="FileUploadStatus.Cancelled" /> and whose
    /// <see cref="ErrorCode" /> is <see cref="FileUploadErrorCode.Cancelled" />.
    /// </returns>
    public static FileUploadResult CancelledResult(string userFriendlyMessage)
    {
        return new FileUploadResult
        {
            Status = FileUploadStatus.Cancelled,
            ErrorCode = FileUploadErrorCode.Cancelled,
            UserFriendlyMessage = userFriendlyMessage
        };
    }
}