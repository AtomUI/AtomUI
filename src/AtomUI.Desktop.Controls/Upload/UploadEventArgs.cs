using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data of the <see cref="Upload.UploadTaskCreated" /> event. The upload raises it for every file
/// committed to its file list, before it decides whether to schedule the file for upload.
/// </summary>
public class UploadTaskCreatedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="UploadFileItem.Id" /> of the item that the file list shows for the file.
    /// </summary>
    public Guid TaskId { get; }
    /// <summary>
    /// The file the event is about.
    /// </summary>
    public UploadFileInfo UploadFileInfo { get; }

    /// <summary>
    /// Creates the arguments of the <see cref="Upload.UploadTaskCreated" /> event for one file.
    /// </summary>
    public UploadTaskCreatedEventArgs(Guid taskId, UploadFileInfo uploadFileInfo)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
    }
}

/// <summary>
/// Carries the data of the <see cref="Upload.UploadTaskAboutToScheduling" /> event. The upload raises it for a
/// committed file after <see cref="Upload.UploadTaskCreated" /> and before it acts on the handler's
/// <see cref="Result" />.
/// </summary>
public class UploadTaskAboutToSchedulingEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="UploadFileItem.Id" /> of the item that the file list shows for the file.
    /// </summary>
    public Guid TaskId { get; }
    /// <summary>
    /// The file the event is about.
    /// </summary>
    public UploadFileInfo UploadFileInfo { get; }
    
    /// <summary>
    /// What the upload does with the file. The handler sets it, and the upload reads it after the event.
    /// The default is <see cref="UploadPredicateResult.Schedule" />. The effect of each value is described at
    /// <see cref="UploadPredicateResult" />.
    /// </summary>
    public UploadPredicateResult Result { get; set; } = UploadPredicateResult.Schedule;
    /// <summary>
    /// The reason for not uploading the file. The upload stores it as the error message of the file's item when
    /// <see cref="Result" /> is <see cref="UploadPredicateResult.Cancel" /> or
    /// <see cref="UploadPredicateResult.CancelWithInTaskList" />, and does not read it otherwise.
    /// </summary>
    public string? CancelReason { get; set; }

    /// <summary>
    /// Creates the arguments of the <see cref="Upload.UploadTaskAboutToScheduling" /> event for one file, with
    /// <see cref="Result" /> set to <see cref="UploadPredicateResult.Schedule" />.
    /// </summary>
    public UploadTaskAboutToSchedulingEventArgs(Guid taskId, UploadFileInfo uploadFileInfo)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
    }
}

/// <summary>
/// Carries the data of the <see cref="Upload.UploadTaskProgress" /> event. The upload raises it on the UI thread
/// after it has stored a new progress value in the file's item. It is not raised for an item that is already in a
/// terminal status.
/// </summary>
public class UploadTaskProgressEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="UploadFileItem.Id" /> of the item that the file list shows for the file.
    /// </summary>
    public Guid TaskId { get; }
    /// <summary>
    /// The file the event is about.
    /// </summary>
    public UploadFileInfo UploadFileInfo { get; }
    /// <summary>
    /// The progress value that the upload last received for the file. Before the event is raised, the upload
    /// stores the same value as the <c>Progress</c> of the file's item.
    /// </summary>
    public double Progress { get; }

    /// <summary>
    /// Creates the arguments of the <see cref="Upload.UploadTaskProgress" /> event for one file.
    /// </summary>
    public UploadTaskProgressEventArgs(Guid taskId, UploadFileInfo uploadFileInfo, double progress)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
        Progress = progress;
    }
}

/// <summary>
/// Carries the data of the <see cref="Upload.UploadTaskCompleted" /> event. The upload raises it on the UI thread
/// after it has marked the file's item as succeeded, and before it schedules the automatic removal of the item.
/// </summary>
public class UploadTaskCompletedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="UploadFileItem.Id" /> of the item that the file list shows for the file.
    /// </summary>
    public Guid TaskId { get; }
    /// <summary>
    /// The file the event is about.
    /// </summary>
    public UploadFileInfo UploadFileInfo { get; }
    /// <summary>
    /// The result of the finished upload. Before the event is raised, the upload stores the same object as the
    /// <c>Result</c> of the file's <see cref="UploadFileItem" />.
    /// </summary>
    public FileUploadResult Result { get; }

    /// <summary>
    /// Creates the arguments of the <see cref="Upload.UploadTaskCompleted" /> event for one file.
    /// </summary>
    public UploadTaskCompletedEventArgs(Guid taskId, UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
        Result = result;
    }
}

/// <summary>
/// Carries the data of the <see cref="Upload.UploadTaskFailed" /> event. The upload raises it on the UI thread
/// after it has marked the file's item as failed and copied the <c>UserFriendlyMessage</c> of
/// <see cref="Result" /> into the item's error message.
/// </summary>
public class UploadTaskFailedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="UploadFileItem.Id" /> of the item that the file list shows for the file.
    /// </summary>
    public Guid TaskId { get; }
    /// <summary>
    /// The file the event is about.
    /// </summary>
    public UploadFileInfo UploadFileInfo { get; }
    /// <summary>
    /// The result that describes the failure. Before the event is raised, the upload stores it as the
    /// <c>Result</c> of the file's <see cref="UploadFileItem" />, and uses the <c>UserFriendlyMessage</c> of
    /// the result as the item's error message.
    /// </summary>
    public FileUploadResult Result { get; }

    /// <summary>
    /// Creates the arguments of the <see cref="Upload.UploadTaskFailed" /> event for one file.
    /// </summary>
    public UploadTaskFailedEventArgs(Guid taskId, UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
        Result         = result;
    }
}

/// <summary>
/// Carries the data of the <see cref="Upload.UploadTaskCancelled" /> event. The upload raises it on the UI thread
/// after it has marked the file's item as canceled and copied the <c>UserFriendlyMessage</c> of
/// <see cref="Result" /> into the item's error message.
/// </summary>
public class UploadTaskCancelledEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="UploadFileItem.Id" /> of the item that the file list shows for the file.
    /// </summary>
    public Guid TaskId { get; }
    /// <summary>
    /// The file the event is about.
    /// </summary>
    public UploadFileInfo UploadFileInfo { get; }
    /// <summary>
    /// The result that describes the cancellation. Before the event is raised, the upload stores it as the
    /// <c>Result</c> of the file's <see cref="UploadFileItem" />, and uses the <c>UserFriendlyMessage</c> of
    /// the result as the item's error message.
    /// </summary>
    public FileUploadResult Result { get; }

    /// <summary>
    /// Creates the arguments of the <see cref="Upload.UploadTaskCancelled" /> event for one file.
    /// </summary>
    public UploadTaskCancelledEventArgs(Guid taskId, UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
        Result         = result;
    }
}

/// <summary>
/// Carries the data of the <see cref="Upload.UploadTaskRemoved" /> event. The upload raises it after the file's
/// item has been removed from its file list.
/// </summary>
public class UploadTaskRemovedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="UploadFileItem.Id" /> of the item that the file list shows for the file.
    /// </summary>
    public Guid TaskId { get; }
    /// <summary>
    /// The file the event is about. When the upload holds no admitted file for the item, this is a description
    /// built from the item's name, path and size that carries a display-only source.
    /// </summary>
    public UploadFileInfo UploadFileInfo { get; }

    /// <summary>
    /// Creates the arguments of the <see cref="Upload.UploadTaskRemoved" /> event for one file.
    /// </summary>
    public UploadTaskRemovedEventArgs(Guid taskId, UploadFileInfo uploadFileInfo)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
    }
}