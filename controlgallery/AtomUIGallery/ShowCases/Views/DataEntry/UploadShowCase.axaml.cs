using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class UploadShowCase : ReactiveUserControl<UploadViewModel>
{
    private WindowMessageManager? _messageManager;
    
    public UploadShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is UploadViewModel vm)
            {
                vm.DefaultTaskList = [
                    new UploadTaskInfo()
                    {
                        TaskId      = Guid.NewGuid(),
                        FileName    = "xxx.png",
                        IsImageFile = true,
                        Status      = FileUploadStatus.Uploading,
                        Progress    = 33
                    },
                    new UploadTaskInfo()
                    {
                        TaskId      = Guid.NewGuid(),
                        FileName    = "yyy.png",
                        IsImageFile = true,
                        Status      = FileUploadStatus.Success,
                        Progress    = 100
                    },
                    new UploadTaskInfo()
                    {
                        TaskId       = Guid.NewGuid(),
                        FileName     = "zzz.png",
                        IsImageFile  = true,
                        Status       = FileUploadStatus.Failed,
                        ErrorMessage = "Server Error 500"
                    },
                ];
                InitPictureWallTaskList(vm);
                InitPictureListTaskList(vm);
            }
        });
        InitializeComponent();
        BasicUpload.UploadTransport     =  new UploadMockTransport();
        BasicUpload.UploadTaskFailed    += HandleUploadFailed;
        BasicUpload.UploadTaskCompleted += HandleUploadCompleted;
        
        AvatarDemoPictureCardUpload.UploadTransport     =  new UploadMockTransport();
        AvatarDemoPictureCardUpload.UploadTaskFailed    += HandleUploadFailed;
        AvatarDemoPictureCardUpload.UploadTaskCompleted += HandleUploadCompleted;
        
        AvatarDemoPictureCircleUpload.UploadTransport     =  new UploadMockTransport();
        AvatarDemoPictureCircleUpload.UploadTaskFailed    += HandleUploadFailed;
        AvatarDemoPictureCircleUpload.UploadTaskCompleted += HandleUploadCompleted;
        
        DefaultFileList.UploadTransport               =  new UploadMockTransport();
        DefaultFileList.UploadTaskFailed              += HandleUploadFailed;
        DefaultFileList.UploadTaskCompleted           += HandleUploadCompleted;
        
        AvatarDemoPictureCardUpload.UploadTaskAboutToScheduling   += HandleAboutToScheduling;
        AvatarDemoPictureCircleUpload.UploadTaskAboutToScheduling += HandleAboutToScheduling;
        AvatarDemoPictureCardUpload.UploadTaskFailed              += HandleUploadFailed;
        AvatarDemoPictureCircleUpload.UploadTaskFailed            += HandleUploadFailed;
        AvatarDemoPictureCircleUpload.UploadTaskCompleted         += HandleUploadCompleted;
        AvatarDemoPictureCardUpload.UploadTaskCompleted           += HandleUploadCompleted;
        PicturesWallUpload.UploadTransport                        =  new UploadMockTransport();
        PicturesWallUpload.UploadTaskFailed                       += HandleUploadFailed;
        PicturesWallUpload.UploadTaskCompleted                    += HandleUploadCompleted;
        
        PicturesCircleWallUpload.UploadTransport                  =  new UploadMockTransport();
        PicturesCircleWallUpload.UploadTaskFailed                 += HandleUploadFailed;
        PicturesCircleWallUpload.UploadTaskCompleted              += HandleUploadCompleted;
        
        DragAndDropUpload.UploadTransport                         =  new UploadMockTransport();
        DragAndDropUpload.UploadTaskFailed                        += HandleUploadFailed;
        DragAndDropUpload.UploadTaskCompleted                     += HandleUploadCompleted;
        
        PictureListUpload.UploadTransport                         =  new UploadMockTransport();
        PictureListUpload.UploadTaskFailed                        += HandleUploadFailed;
        PictureListUpload.UploadTaskCompleted                     += HandleUploadCompleted;
        
        MaxCount1Upload.UploadTransport                           =  new UploadMockTransport();
        MaxCount1Upload.UploadTaskFailed                          += HandleUploadFailed;
        MaxCount1Upload.UploadTaskCompleted                       += HandleUploadCompleted;
        
        MaxCount3Upload.UploadTransport                           =  new UploadMockTransport();
        MaxCount3Upload.UploadTaskFailed                          += HandleUploadFailed;
        MaxCount3Upload.UploadTaskCompleted                       += HandleUploadCompleted;
        
        DirectoryUpload.UploadTransport                           =  new UploadMockTransport();
        DirectoryUpload.UploadTaskFailed                          += HandleUploadFailed;
        DirectoryUpload.UploadTaskCompleted                       += HandleUploadCompleted;
        PngOnlyUpload.UploadTransport                             =  new UploadMockTransport();
        PngOnlyUpload.UploadTaskAboutToScheduling                 += HandlePngUploadAboutToScheduling;
        PngOnlyUpload.UploadTaskFailed                            += HandleUploadFailed;
        PngOnlyUpload.UploadTaskCompleted                         += HandleUploadCompleted;
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_messageManager == null)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            _messageManager = new WindowMessageManager(topLevel)
            {
                MaxItems = 10
            };
        }
    }

    private void HandleAboutToScheduling(object? sender, UploadTaskAboutToSchedulingEventArgs e)
    {
        var fileInfo          = e.UploadFileInfo;
        var ext               = Path.GetExtension(fileInfo.FilePath.LocalPath);
        var isAllowedFileType = false;
        if (ext == ".jpeg" || ext == ".jpg" || ext == ".png")
        {
            isAllowedFileType = true;
        }

        if (!isAllowedFileType)
        {
            e.Result       = UploadPredicateResult.CancelWithInTaskList;
            e.CancelReason = "You can only upload JPG/PNG file!";
            return;
        }
        var isLt2M = (double)fileInfo.Size / 1024 / 1024 < 2;
        if (!isLt2M) {
            e.Result       = UploadPredicateResult.CancelWithInTaskList;
            e.CancelReason = "Image must smaller than 2MB!";
        }
    }
    
    private void HandlePngUploadAboutToScheduling(object? sender, UploadTaskAboutToSchedulingEventArgs e)
    {
        var fileInfo = e.UploadFileInfo;
        var ext      = Path.GetExtension(fileInfo.FilePath.LocalPath);
        if (ext != ".png")
        {
            e.Result       = UploadPredicateResult.Cancel;
            e.CancelReason = "You can only upload PNG file!";
        }
    }
    
    private void HandleUploadFailed(object? sender, UploadTaskFailedEventArgs e)
    {
        var errorMsg = e.Result.UserFriendlyMessage;
        _messageManager?.Show(new Message(
            type: MessageType.Error,
            content:$"{errorMsg}"
        ));
    }
    
    private void HandleUploadCompleted(object? sender, UploadTaskCompletedEventArgs e)
    {
        _messageManager?.Show(new Message(
            type: MessageType.Success,
            content:$"{e.UploadFileInfo.Name} upload successfully!"
        ));
    }

    private void InitPictureWallTaskList(UploadViewModel uploadViewModel)
    {
        uploadViewModel.PicturesWallDefaultTaskList = [
            new UploadTaskInfo()
            {
                TaskId      = Guid.NewGuid(),
                FileName    = "image.png",
                IsImageFile = true,
                Status      = FileUploadStatus.Success,
                FilePath    = new Uri("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png")
            },
            new UploadTaskInfo()
            {
                TaskId      = Guid.NewGuid(),
                FileName    = "image.png",
                IsImageFile = true,
                Status      = FileUploadStatus.Success,
                FilePath    = new Uri("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png")
            },
            new UploadTaskInfo()
            {
                TaskId      = Guid.NewGuid(),
                FileName    = "image.png",
                IsImageFile = true,
                Status      = FileUploadStatus.Success,
                FilePath    = new Uri("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png")
            },
            new UploadTaskInfo()
            {
                TaskId      = Guid.NewGuid(),
                FileName    = "image.png",
                IsImageFile = true,
                Status      = FileUploadStatus.Success,
                FilePath    = new Uri("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png")
            },
            new UploadTaskInfo()
            {
                TaskId      = Guid.NewGuid(),
                FileName    = "image.png",
                IsImageFile = true,
                Status      = FileUploadStatus.Uploading,
                Progress    = 50
            },
            new UploadTaskInfo()
            {
                TaskId       = Guid.NewGuid(),
                FileName     = "image.png",
                IsImageFile  = true,
                Status       = FileUploadStatus.Failed,
                ErrorMessage = "Upload error!"
            },
        ];
    }

    private void InitPictureListTaskList(UploadViewModel uploadViewModel)
    {
        uploadViewModel.PictureListStyleDefaultTaskList =
        [
            new UploadTaskInfo()
            {
                TaskId      = Guid.NewGuid(),
                FileName    = "xxx.png",
                IsImageFile = true,
                Status      = FileUploadStatus.Uploading,
                Progress    = 33
            },
            new UploadTaskInfo()
            {
                TaskId      = Guid.NewGuid(),
                FileName    = "yyy.png",
                IsImageFile = true,
                Status      = FileUploadStatus.Success,
                FilePath    = new Uri("avares://AtomUIGallery/Assets/ImagePreviewerShowCase/1.png")
            },
            new UploadTaskInfo()
            {
                TaskId       = Guid.NewGuid(),
                FileName     = "zzz.png",
                IsImageFile  = true,
                Status       = FileUploadStatus.Failed,
                ErrorMessage = "Upload error!"
            },
        ];
    }
}

public class UploadMockTransport : IFileUploadTransport
{
    public async Task<FileUploadResult> UploadAsync(
        UploadFileInfo fileInfo,
        object? context = null,
        IProgress<FileUploadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var totalBytes  = fileInfo.Size;
        var bytesSent   = 0L;
        var elapsedTime = TimeSpan.Zero;
        try
        {
            var random        = Random.Shared.Next(1, 10);
            var isServerError = random % 3 == 0;
            var cycle         = 0;
            while (!cancellationToken.IsCancellationRequested && bytesSent < totalBytes)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                var delay = TimeSpan.FromMilliseconds(Random.Shared.Next(300, 1000));
                await Task.Delay(delay, cancellationToken);
                elapsedTime += delay;
                bytesSent += (long)(totalBytes *
                                    ((double)Random.Shared.NextInt64((long)totalBytes / 20, (long)totalBytes / 10) /
                                     totalBytes));
                bytesSent = Math.Min(bytesSent, totalBytes);
                var uploadProgress = new FileUploadProgress()
                {
                    TotalBytes = fileInfo.Size,
                    BytesSent  = bytesSent,
                };
                progress?.Report(uploadProgress);
                if (isServerError && cycle > 2)
                {
                    return FileUploadResult.FailureResult(FileUploadErrorCode.ServerError, "Max number of elements reached for this resource!");
                }

                ++cycle;
            }

            return FileUploadResult.SuccessResult(
                fileInfo.FilePath,
                fileInfo.Size,
                elapsedTime,
                "Success");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return FileUploadResult.FailureResult(FileUploadErrorCode.Unknown, "Upload failed: " + ex.Message);
        }
    }
}