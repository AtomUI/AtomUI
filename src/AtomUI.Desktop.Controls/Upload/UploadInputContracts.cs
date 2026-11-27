using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// State of a drag over an <see cref="UploadDropZone" />, as exposed by
/// <see cref="UploadDropZone.DragState" />. The value sets the matching pseudo-classes of the zone, which
/// styles use to show the drag feedback.
/// </summary>
public enum UploadDragState
{
    /// <summary>
    /// No drag feedback is shown: no drag is over the zone, a drop was made, or the zone takes no drop because it
    /// is disabled, <see cref="UploadDropZone.IsFileDropEnabled" /> is <see langword="false" /> or no
    /// <see cref="Upload" /> is above it.
    /// </summary>
    None,
    /// <summary>
    /// A drag is over the zone and the dragged data can be taken, so the zone shows its accepting feedback.
    /// </summary>
    Accepting,
    /// <summary>
    /// A drag is over the zone but the dragged data cannot be taken, so the zone shows its refusing feedback.
    /// </summary>
    Rejecting
}

/// <summary>
/// What an <see cref="Upload" /> takes from a folder that arrives as input. For dropped folders it is set by
/// <see cref="UploadDropZone.DirectoryDropMode" />. A folder that is not taken is reported as a rejected item.
/// </summary>
public enum UploadDirectoryDropMode
{
    /// <summary>
    /// The folder is not taken; it is rejected with <see cref="UploadRejectionReason.DirectoryNotAllowed" />.
    /// This is the default of the drop zone.
    /// </summary>
    Reject,
    /// <summary>
    /// Only the files directly inside the folder are taken. A nested folder is rejected with
    /// <see cref="UploadRejectionReason.DirectoryNotAllowed" />.
    /// </summary>
    TopLevelFiles,
    /// <summary>
    /// The files of the whole folder tree are taken, as deep as <see cref="UploadDropZone.MaxDirectoryDepth" />
    /// allows. A nested folder beyond that depth is rejected with
    /// <see cref="UploadRejectionReason.DirectoryDepthExceeded" />, and one that was already visited with
    /// <see cref="UploadRejectionReason.DirectoryCycleDetected" />.
    /// </summary>
    RecursiveFiles
}

/// <summary>
/// What an <see cref="Upload" /> does with a batch of admitted files that has more files than the list has room
/// for under <see cref="Upload.MaxCount" />.
/// </summary>
public enum UploadCountOverflowBehavior
{
    /// <summary>
    /// The files that fit are added and the remaining ones are rejected with
    /// <see cref="UploadRejectionReason.CountLimitExceeded" />. This is the default.
    /// </summary>
    RejectExcess,
    /// <summary>
    /// No file of the batch is added; all of them are rejected with
    /// <see cref="UploadRejectionReason.CountLimitExceeded" />.
    /// </summary>
    RejectBatch,
    /// <summary>
    /// The new files replace the files in the list. A batch longer than <see cref="Upload.MaxCount" /> is cut to
    /// that length, and the cut-off files are rejected with
    /// <see cref="UploadRejectionReason.CountLimitExceeded" />.
    /// </summary>
    ReplaceExisting
}

/// <summary>
/// Where a batch of incoming files came from. It is reported with the batch in
/// <see cref="UploadInputBatchCompletedEventArgs.Source" /> and handed to the admission policy in
/// <see cref="UploadAdmissionContext.Source" />.
/// </summary>
public enum UploadInputSource
{
    /// <summary>
    /// The files were chosen in the file picker that the control opens.
    /// </summary>
    FilePicker,
    /// <summary>
    /// The files were taken from a folder chosen in the folder picker: only the files directly inside it.
    /// </summary>
    DirectoryPicker,
    /// <summary>
    /// The files were dropped on an <see cref="UploadDropZone" />.
    /// </summary>
    DragDrop,
    /// <summary>
    /// The files were passed to the control from code.
    /// </summary>
    Programmatic
}

/// <summary>
/// How the processing of one batch of incoming files ended, as reported with
/// <see cref="Upload.InputBatchCompleted" />. Rejected files alone do not make a batch fail.
/// </summary>
public enum UploadInputBatchStatus
{
    /// <summary>
    /// The batch was processed to its end without an exception. Individual files may still have been rejected.
    /// </summary>
    Completed,
    /// <summary>
    /// The batch was canceled before it ended.
    /// </summary>
    Cancelled,
    /// <summary>
    /// The batch threw an exception while it was processed or while it was cleaned up.
    /// <see cref="UploadInputFailureReason" /> then names the step that failed.
    /// </summary>
    Failed
}

/// <summary>
/// Which step of a batch failed. It is reported with
/// <see cref="UploadInputBatchCompletedEventArgs.FailureReason" /> only when the status is
/// <see cref="UploadInputBatchStatus.Failed" />.
/// </summary>
public enum UploadInputFailureReason
{
    /// <summary>
    /// The data of a drop could not be read, so the drop ends as a failed batch without any file.
    /// </summary>
    DataSnapshotFailed,
    /// <summary>
    /// An exception was thrown while the files of the batch were read, admitted or added.
    /// </summary>
    ProcessingFailed
}

/// <summary>
/// Why an incoming file or folder was not added to the list. It is reported on each
/// <see cref="UploadRejectedItem" /> of a batch.
/// </summary>
public enum UploadRejectionReason
{
    /// <summary>
    /// The storage item is neither a file nor a folder.
    /// </summary>
    UnsupportedStorageItem,
    /// <summary>
    /// A folder was not taken: it was dropped while the mode is <see cref="UploadDirectoryDropMode.Reject" />, or it
    /// is nested in a folder taken with <see cref="UploadDirectoryDropMode.TopLevelFiles" />.
    /// </summary>
    DirectoryNotAllowed,
    /// <summary>
    /// A nested folder lies deeper than <see cref="UploadDropZone.MaxDirectoryDepth" /> allows in
    /// <see cref="UploadDirectoryDropMode.RecursiveFiles" /> mode.
    /// </summary>
    DirectoryDepthExceeded,
    /// <summary>
    /// A nested folder was met a second time while the folder tree of the batch was enumerated.
    /// </summary>
    DirectoryCycleDetected,
    /// <summary>
    /// More storage items were examined for the batch than <see cref="UploadDropZone.MaxEnumeratedItems" /> allows;
    /// the item that crossed the limit is rejected.
    /// </summary>
    EnumerationLimitExceeded,
    /// <summary>
    /// Access to a folder was denied while its content was listed.
    /// </summary>
    AccessDenied,
    /// <summary>
    /// A storage item could not be read: the properties of a file could not be read or the content of a folder
    /// could not be listed.
    /// </summary>
    StorageReadFailed,
    /// <summary>
    /// The file does not match <see cref="Upload.AllowedFileTypes" />.
    /// </summary>
    FileTypeNotAllowed,
    /// <summary>
    /// The admission policy rejected the file. The code and the message it gave are reported on the rejected item.
    /// </summary>
    AdmissionRejected,
    /// <summary>
    /// The admission policy threw an exception. The file is rejected without a code or a message.
    /// </summary>
    AdmissionPolicyFailed,
    /// <summary>
    /// The batch had more admitted files than the list has room for under <see cref="Upload.MaxCount" />, and this
    /// file is one that does not fit.
    /// </summary>
    CountLimitExceeded,
    /// <summary>
    /// The item is a top-level storage item after the first of the batch while
    /// <see cref="Upload.IsMultipleEnabled" /> is <see langword="false" />. Only the first item is taken.
    /// </summary>
    MultipleSelectionNotAllowed
}

/// <summary>
/// Lets the application decide for each incoming file whether it may be added to an <see cref="Upload" />.
/// Assign it to <see cref="Upload.AdmissionPolicy" />; the control then calls it once for every file that
/// matched <see cref="Upload.AllowedFileTypes" />.
/// </summary>
public interface IUploadAdmissionPolicy
{
    /// <summary>
    /// Decides whether one incoming file is admitted. It is awaited on a thread pool thread, not on the UI thread.
    /// An admitted file can still be rejected by the count limit.
    /// </summary>
    /// <param name="context">
    /// The file, with the batch it came with and the source of the batch.
    /// </param>
    /// <param name="cancellationToken">
    /// Signals that the batch was canceled; throwing <see cref="OperationCanceledException" /> then ends the batch
    /// as canceled.
    /// </param>
    /// <returns>
    /// A decision that admits the file or rejects it with an optional code and message.
    /// </returns>
    /// <remarks>
    /// An exception other than <see cref="OperationCanceledException" />
    /// does not fail the batch: the file is rejected with <see cref="UploadRejectionReason.AdmissionPolicyFailed" />.
    /// </remarks>
    ValueTask<UploadAdmissionDecision> EvaluateAsync(
        UploadAdmissionContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// One incoming file as it is handed to an <see cref="IUploadAdmissionPolicy" />, with the batch it came with
/// and the source of the batch. A new one is created for every file.
/// </summary>
/// <param name="BatchId">
/// Identifies the batch. All files of one batch share the value.
/// </param>
/// <param name="Source">
/// Where the batch came from.
/// </param>
/// <param name="File">
/// The file to evaluate.
/// </param>
public sealed record UploadAdmissionContext(
    Guid BatchId,
    UploadInputSource Source,
    UploadFileInfo File);

/// <summary>
/// The answer of an <see cref="IUploadAdmissionPolicy" /> for one file, created with <see cref="Accept" /> or
/// <see cref="Reject" />.
/// </summary>
public sealed class UploadAdmissionDecision
{
    /// <summary>
    /// Whether the file is admitted. It is <see langword="true" /> only for a decision made with
    /// <see cref="Accept" />.
    /// </summary>
    public bool IsAccepted { get; }
    /// <summary>
    /// The code given to <see cref="Reject" /> that identifies the reason of the rejection, or
    /// <see langword="null" /> when none was given. It is passed on to the rejected item.
    /// </summary>
    public string? RejectionCode { get; }
    /// <summary>
    /// The text given to <see cref="Reject" /> that explains the rejection, or <see langword="null" /> when
    /// none was given. It is passed on to the rejected item.
    /// </summary>
    public string? Message { get; }

    private UploadAdmissionDecision(bool isAccepted, string? rejectionCode, string? message)
    {
        IsAccepted    = isAccepted;
        RejectionCode = rejectionCode;
        Message       = message;
    }

    /// <summary>
    /// Creates the decision that admits the file.
    /// </summary>
    /// <returns>
    /// The decision that admits the file.
    /// </returns>
    public static UploadAdmissionDecision Accept() => new(true, null, null);

    /// <summary>
    /// Creates the decision that rejects the file.
    /// </summary>
    /// <param name="rejectionCode">
    /// Code that identifies the reason; it is passed on to the rejected item.
    /// </param>
    /// <param name="message">
    /// Text that explains the rejection; it is passed on to the rejected item.
    /// </param>
    /// <returns>
    /// The decision that rejects the file.
    /// </returns>
    public static UploadAdmissionDecision Reject(
        string? rejectionCode = null,
        string? message = null) =>
        new(false, rejectionCode, message);
}

/// <summary>
/// One incoming file or folder that was not added to the list, with the reason why. The rejected items of a
/// batch are reported with <see cref="Upload.InputBatchCompleted" />.
/// </summary>
public sealed class UploadRejectedItem
{
    /// <summary>
    /// The name of the rejected file or folder.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The path of the rejected file or folder, or <see langword="null" /> when none is available.
    /// </summary>
    public Uri? Path { get; }
    /// <summary>
    /// Why the item was rejected.
    /// </summary>
    public UploadRejectionReason Reason { get; }
    /// <summary>
    /// The code of the admission policy for a rejection with
    /// <see cref="UploadRejectionReason.AdmissionRejected" />, or <see langword="null" /> when the rejection
    /// carries no code.
    /// </summary>
    public string? RejectionCode { get; }
    /// <summary>
    /// Text that explains the rejection, or <see langword="null" /> when the reason carries none.
    /// </summary>
    public string? Message { get; }

    internal UploadRejectedItem(
        string name,
        Uri? path,
        UploadRejectionReason reason,
        string? rejectionCode = null,
        string? message = null)
    {
        Name          = name;
        Path          = path;
        Reason        = reason;
        RejectionCode = rejectionCode;
        Message       = message;
    }
}

/// <summary>
/// Carries the outcome of one processed batch of incoming files to the handlers of
/// <see cref="Upload.InputBatchCompleted" />.
/// </summary>
public sealed class UploadInputBatchCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Identifies the batch. The admission policy received the same value for each file of the batch.
    /// </summary>
    public Guid BatchId { get; }
    /// <summary>
    /// Where the files of the batch came from.
    /// </summary>
    public UploadInputSource Source { get; }
    /// <summary>
    /// How the processing of the batch ended. <see cref="FailureReason" /> is set only when the status is
    /// <see cref="UploadInputBatchStatus.Failed" />.
    /// </summary>
    public UploadInputBatchStatus Status { get; }
    /// <summary>
    /// Which step of the batch failed. It is set if and only if <see cref="Status" /> is
    /// <see cref="UploadInputBatchStatus.Failed" />.
    /// </summary>
    public UploadInputFailureReason? FailureReason { get; }
    /// <summary>
    /// The files of the batch that were added to the list before the batch ended.
    /// </summary>
    public IReadOnlyList<UploadFileInfo> AcceptedFiles { get; }
    /// <summary>
    /// The files and folders of the batch that were not added, each with the reason why.
    /// </summary>
    public IReadOnlyList<UploadRejectedItem> RejectedItems { get; }

    internal UploadInputBatchCompletedEventArgs(
        Guid batchId,
        UploadInputSource source,
        UploadInputBatchStatus status,
        UploadInputFailureReason? failureReason,
        IReadOnlyList<UploadFileInfo> acceptedFiles,
        IReadOnlyList<UploadRejectedItem> rejectedItems)
    {
        ArgumentNullException.ThrowIfNull(acceptedFiles);
        ArgumentNullException.ThrowIfNull(rejectedItems);
        if ((status == UploadInputBatchStatus.Failed) != failureReason.HasValue)
        {
            throw new ArgumentException("FailureReason must be set if and only if Status is Failed.");
        }

        BatchId       = batchId;
        Source        = source;
        Status        = status;
        FailureReason = failureReason;
        AcceptedFiles = acceptedFiles;
        RejectedItems = rejectedItems;
    }
}
