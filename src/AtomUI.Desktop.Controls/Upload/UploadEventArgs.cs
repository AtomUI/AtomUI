using System.ComponentModel;
using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public class UploadTaskCreatedEventArgs : EventArgs
{
    public Guid TaskId { get; }
    public UploadFileInfo UploadFileInfo { get; }

    public UploadTaskCreatedEventArgs(Guid taskId, UploadFileInfo uploadFileInfo)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
    }
}

public class UploadTaskAboutToSchedulingEventArgs : CancelEventArgs
{
    public Guid TaskId { get; }
    public UploadFileInfo UploadFileInfo { get; }
    
    public string? CancelReason { get; set; }

    public UploadTaskAboutToSchedulingEventArgs(Guid taskId, UploadFileInfo uploadFileInfo)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
    }
}

public class UploadTaskProgressEventArgs : EventArgs
{
    public Guid TaskId { get; }
    public UploadFileInfo UploadFileInfo { get; }
    public double Progress { get; }

    public UploadTaskProgressEventArgs(Guid taskId, UploadFileInfo uploadFileInfo, double progress)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
        Progress = progress;
    }
}

public class UploadTaskCompletedEventArgs : EventArgs
{
    public Guid TaskId { get; }
    public UploadFileInfo UploadFileInfo { get; }
    public FileUploadResult Result { get; }

    public UploadTaskCompletedEventArgs(Guid taskId, UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
        Result = result;
    }
}

public class UploadTaskFailedEventArgs : EventArgs
{
    public Guid TaskId { get; }
    public UploadFileInfo UploadFileInfo { get; }
    public FileUploadResult Result { get; }

    public UploadTaskFailedEventArgs(Guid taskId, UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
        Result         = result;
    }
}

public class UploadTaskCancelledEventArgs : EventArgs
{
    public Guid TaskId { get; }
    public UploadFileInfo UploadFileInfo { get; }
    public FileUploadResult Result { get; }

    public UploadTaskCancelledEventArgs(Guid taskId, UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
        Result         = result;
    }
}

public class UploadTaskRemovedEventArgs : EventArgs
{
    public Guid TaskId { get; }
    public UploadFileInfo UploadFileInfo { get; }

    public UploadTaskRemovedEventArgs(Guid taskId, UploadFileInfo uploadFileInfo)
    {
        TaskId         = taskId;
        UploadFileInfo = uploadFileInfo;
    }
}