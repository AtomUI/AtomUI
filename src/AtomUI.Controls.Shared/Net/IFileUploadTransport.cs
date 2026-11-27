namespace AtomUI.Controls;

/// <summary>
/// Progress report of one running upload, passed to the <c>IProgress&lt;FileUploadProgress&gt;</c>
/// that the scheduler hands to the transport.
/// </summary>
public record FileUploadProgress
{
    /// <summary>
    /// Number of bytes the transport has sent so far.
    /// </summary>
    public long BytesSent { get; set; }
    /// <summary>
    /// Total number of bytes to send. A value that is not positive makes <see cref="Percentage" /> return <c>0</c>.
    /// </summary>
    public long TotalBytes { get; set; }
    /// <summary>
    /// Share of <see cref="TotalBytes" /> that was sent, from 0 to 100. <c>0</c> when <see cref="TotalBytes" />
    /// is not positive.
    /// </summary>
    public double Percentage => TotalBytes > 0 ? (BytesSent * 100.0) / TotalBytes : 0;
}

/// <summary>
/// Sends one file to a server. The upload scheduler calls it for each queued file that it starts.
/// </summary>
public interface IFileUploadTransport
{
    /// <summary>
    /// Uploads one file and reports its progress while it runs.
    /// </summary>
    /// <param name="fileInfo">
    /// The file to send. Its <see cref="UploadFileInfo.Source" /> provides the content.
    /// </param>
    /// <param name="context">
    /// The object the caller stored on the upload task. The scheduler passes it through unchanged,
    /// so only the caller and the transport agree on its type. May be <see langword="null" />.
    /// </param>
    /// <param name="progress">
    /// Receives progress reports while the upload runs. May be <see langword="null" />.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the upload. Throwing <c>OperationCanceledException</c> makes the scheduler report the upload as
    /// canceled.
    /// </param>
    /// <returns>
    /// A task that completes when the upload has finished. The scheduler marks the file as successful when
    /// <see cref="FileUploadResult.IsSuccess" /> is <see langword="true" /> and as failed otherwise. Any other
    /// exception is reported as a failure with the error code <see cref="FileUploadErrorCode.Unknown" />.
    /// </returns>
    Task<FileUploadResult> UploadAsync(
        UploadFileInfo fileInfo,
        object? context = null,
        IProgress<FileUploadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}