using System.Text.RegularExpressions;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public class Upload : ContentControl, IMotionAwareControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<IReadOnlyList<string>?> AcceptsProperty =
        AvaloniaProperty.Register<Upload, IReadOnlyList<string>?>(nameof(Accepts));
    
    public static readonly StyledProperty<object?> ExtraContextProperty =
        AvaloniaProperty.Register<Upload, object?>(
            nameof(ExtraContext));
    
    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<Upload, int>(
            nameof(MaxCount), int.MaxValue);
    
    public static readonly StyledProperty<bool> IsUploadDirectoryEnabledProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsUploadDirectoryEnabled));
    
    public static readonly StyledProperty<UploadListType> ListTypeProperty =
        AvaloniaProperty.Register<Upload, UploadListType>(nameof(ListType));
    
    public static readonly StyledProperty<bool> IsMultipleEnabledProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsMultipleEnabled));
    
    public static readonly StyledProperty<bool> IsOpenFileDialogOnClickProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsOpenFileDialogOnClick), true);
    
    public static readonly StyledProperty<bool> IsShowUploadListProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsShowUploadList), true);
    
    public static readonly StyledProperty<bool> IsShowUploadTriggerProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsShowUploadTrigger), true);
    
    public static readonly StyledProperty<IList<UploadTaskInfo>?> DefaultTaskListProperty =
        AvaloniaProperty.Register<Upload, IList<UploadTaskInfo>?>(nameof(DefaultTaskList));
    
    public static readonly StyledProperty<int> MaxConcurrentTasksProperty =
        AvaloniaProperty.Register<Upload, int>(nameof(MaxConcurrentTasks), 3);
    
    public static readonly DirectProperty<Upload, IFileUploadTransport?> UploadTransportProperty =
        AvaloniaProperty.RegisterDirect<Upload, IFileUploadTransport?>(
            nameof(UploadTransport),
            o => o.UploadTransport,
            (o, v) => o.UploadTransport = v);
    
    public static readonly DirectProperty<Upload, bool> IsTaskRunningProperty =
        AvaloniaProperty.RegisterDirect<Upload, bool>(
            nameof(IsTaskRunning),
            o => o.IsTaskRunning,
            (o, v) => o.IsTaskRunning = v);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Upload>();
    
    public IReadOnlyList<string>? Accepts
    {
        get => GetValue(AcceptsProperty);
        set => SetValue(AcceptsProperty, value);
    }
    
    public object? ExtraContext
    {
        get => GetValue(ExtraContextProperty);
        set => SetValue(ExtraContextProperty, value);
    }
    
    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }
    
    public bool IsUploadDirectoryEnabled
    {
        get => GetValue(IsUploadDirectoryEnabledProperty);
        set => SetValue(IsUploadDirectoryEnabledProperty, value);
    }
    
    public UploadListType ListType
    {
        get => GetValue(ListTypeProperty);
        set => SetValue(ListTypeProperty, value);
    }
    
    public bool IsMultipleEnabled
    {
        get => GetValue(IsMultipleEnabledProperty);
        set => SetValue(IsMultipleEnabledProperty, value);
    }
    
    public bool IsOpenFileDialogOnClick
    {
        get => GetValue(IsOpenFileDialogOnClickProperty);
        set => SetValue(IsOpenFileDialogOnClickProperty, value);
    }
    
    public bool IsShowUploadList
    {
        get => GetValue(IsShowUploadListProperty);
        set => SetValue(IsShowUploadListProperty, value);
    }
    
    public bool IsShowUploadTrigger
    {
        get => GetValue(IsShowUploadTriggerProperty);
        set => SetValue(IsShowUploadTriggerProperty, value);
    }
    
    public IList<UploadTaskInfo>? DefaultTaskList
    {
        get => GetValue(DefaultTaskListProperty);
        set => SetValue(DefaultTaskListProperty, value);
    }
    
    public int MaxConcurrentTasks
    {
        get => GetValue(MaxConcurrentTasksProperty);
        set => SetValue(MaxConcurrentTasksProperty, value);
    }
    
    private IFileUploadTransport? _uploadTransport;

    public IFileUploadTransport? UploadTransport
    {
        get => _uploadTransport;
        set => SetAndRaise(UploadTransportProperty, ref _uploadTransport, value);
    }
    
    private bool _isTaskRunning;

    public bool IsTaskRunning
    {
        get => _isTaskRunning;
        set => SetAndRaise(IsTaskRunningProperty, ref _isTaskRunning, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public AvaloniaList<UploadTaskInfo> TaskInfoList { get; } = new ();
    
    #endregion
    
    private List<UploadTaskInfo> _allTaskList = new ();

    #region 公共事件定义

    public event EventHandler<UploadTaskCreatedEventArgs>? UploadTaskCreated;
    public event EventHandler<UploadTaskAboutToSchedulingEventArgs>? UploadTaskAboutToScheduling;
    public event EventHandler<UploadTaskProgressEventArgs>? UploadTaskProgress;
    public event EventHandler<UploadTaskCompletedEventArgs>? UploadTaskCompleted;
    public event EventHandler<UploadTaskCancelledEventArgs>? UploadTaskCancelled;
    public event EventHandler<UploadTaskFailedEventArgs>? UploadTaskFailed;
    public event EventHandler<UploadTaskRemovedEventArgs>? UploadTaskRemoved;
    
    #endregion

    #region 公共回调函数定义
    public Func<UploadFileInfo, bool>? IsImageFilePredicate { get; set; }
    #endregion
    
    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => UploadToken.ID;

    #endregion
    
    private ItemsControl? _uploadListControl;
    private FileUploadScheduler _uploadScheduler;
    private static readonly Regex ImageExtensionRegex = 
        new (@"\.(webp|svg|png|gif|jpg|jpeg|jfif|bmp|dpg|ico|heic|heif)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private bool _defaultTaskListApplied;

    static Upload()
    {
        AbstractUploadListItem.TaskRemoveRequestEvent.AddClassHandler<Upload>((upload, args) =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await upload.HandleUploadTaskRemoveRequestAsync(args.TaskId);
            });
        });
        UploadTriggerContent.FileSelectRequestEvent.AddClassHandler<Upload>((upload, args) =>
        {
            upload.HandleFileSelectRequest();
        });
        UploadDefaultDropArea.FilesDroppedEvent.AddClassHandler<Upload>((upload, args) =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await upload.EnqueueUploadFiles(args.Files);
            });
        });
    }
    
    public Upload()
    {
        _uploadScheduler = new FileUploadScheduler();
    }

    protected virtual void OpenFileDialog()
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel != null)
        {
            var storageProvider  = topLevel.StorageProvider;
            if (!storageProvider.CanOpen)
            {
                throw new InvalidOperationException("Can't open storage provider");
            }

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (IsUploadDirectoryEnabled)
                {
                    var directories = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                    {
                        AllowMultiple = IsMultipleEnabled
                    });
                    var files = new List<UploadFileInfo>();
                    foreach (var directory in directories)
                    {
                        foreach (var fileInfo in Directory.EnumerateFiles(directory.Path.LocalPath, "*", SearchOption.TopDirectoryOnly).Select(x => new FileInfo(x)))
                        {
                            files.Add(new UploadFileInfo(fileInfo.Name, new Uri(fileInfo.FullName), fileInfo.Length, fileInfo.CreationTime,  fileInfo.LastWriteTime));
                        }
                    }
                    await EnqueueUploadFiles(files);
                }
                else
                {
                    var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                    {
                        AllowMultiple     = IsMultipleEnabled,
                        SuggestedFileType = new FilePickerFileType("filter")
                        {
                            MimeTypes = Accepts,
                            Patterns  = Accepts,
                            AppleUniformTypeIdentifiers = Accepts
                        }
                    });
                    await EnqueueUploadFiles(files);
                }
            });
        }
    }

    private async Task EnqueueUploadFiles(IReadOnlyList<IStorageFile> files)
    {
        foreach (var file in files)
        {
            await EnqueueUploadFile(file);
        }
    }
    
    private async Task EnqueueUploadFiles(IList<UploadFileInfo> files)
    {
        foreach (var file in files)
        {
            await EnqueueUploadFile(file);
        }
    }

    private async Task EnqueueUploadFile(IStorageFile file)
    {
        var properties = await file.GetBasicPropertiesAsync();
        var uploadFile = new UploadFileInfo(
            file.Name,
            file.Path,
            (long)(properties.Size ?? 0),
            properties.DateCreated,
            properties.DateModified);
        await EnqueueUploadFile(uploadFile);
    }

    private async Task EnqueueUploadFile(UploadFileInfo uploadFile)
    {
        var task = new FileUploadTask
        {
            UploadFileInfo = uploadFile
        };
        
        task.UploadProgressHandler  = HandleUploadProgressChanged;
        task.UploadCompletedHandler = HandleUploadCompleted;
        task.UploadFailedHandler    = HandleUploadFailed;
        task.UploadCancelledHandler = HandleUploadCancelled;

        var uploadTaskInfo = new UploadTaskInfo();
        uploadTaskInfo.TaskId      = task.Id;
        uploadTaskInfo.FileName    = uploadFile.Name;
        uploadTaskInfo.Status      = task.Status;
        uploadTaskInfo.Progress    = task.Progress;
        uploadTaskInfo.IsImageFile = IsImageFile(uploadFile);
        uploadTaskInfo.UploadTask  = task;
        uploadTaskInfo.FilePath    = uploadFile.FilePath;
        
        _allTaskList.Add(uploadTaskInfo);

        if (TaskInfoList.Count > 0 && TaskInfoList.Count == MaxCount)
        {
            if (MaxCount > 1)
            {
                return; 
            }

            await CancelAllUploadTaskAsync();
        }
        
        UploadTaskCreated?.Invoke(this, new UploadTaskCreatedEventArgs(task.Id, uploadFile));
        var aboutToSchedulingEvent = new UploadTaskAboutToSchedulingEventArgs(task.Id, uploadFile);
        UploadTaskAboutToScheduling?.Invoke(this, aboutToSchedulingEvent);
        if (aboutToSchedulingEvent.Result == UploadPredicateResult.Schedule)
        {
            AddUploadTaskInfo(uploadTaskInfo);
            _uploadScheduler.EnqueueTask(task);
        }
        else
        {
            if (aboutToSchedulingEvent.Result == UploadPredicateResult.CancelWithInTaskList)
            {
                AddUploadTaskInfo(uploadTaskInfo);
            }
            uploadTaskInfo.Status = FileUploadStatus.Failed;
            UploadTaskFailed?.Invoke(this, new UploadTaskFailedEventArgs(task.Id, 
                uploadFile, 
                FileUploadResult.FailureResult(FileUploadErrorCode.ClientError, aboutToSchedulingEvent.CancelReason ?? "client error")));
        }
    }

    private void AddUploadTaskInfo(UploadTaskInfo uploadTaskInfo)
    {
        if (ListType == UploadListType.PictureCircle || ListType == UploadListType.PictureCard)
        {
            var index = TaskInfoList.Count - 1;
            TaskInfoList.Insert(index, uploadTaskInfo);
        }
        else
        {
            TaskInfoList.Add(uploadTaskInfo);
        }
    }

    private void HandleUploadProgressChanged(Guid taskId, UploadFileInfo fileInfo, double progress)
    {
        var taskInfo = TaskInfoList.FirstOrDefault(x => x.TaskId == taskId);
        if (taskInfo != null)
        {
            SetCurrentValue(IsTaskRunningProperty, true);
            taskInfo.Status   = FileUploadStatus.Uploading;
            taskInfo.Progress = progress;
            UploadTaskProgress?.Invoke(this, new UploadTaskProgressEventArgs(taskId, fileInfo, progress));
        }
    }
    
    private void HandleUploadCompleted(Guid taskId, UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        var taskInfo = TaskInfoList.FirstOrDefault(x => x.TaskId == taskId);
        if (taskInfo != null)
        {
            taskInfo.Status = FileUploadStatus.Success;
            UploadTaskCompleted?.Invoke(this, new UploadTaskCompletedEventArgs(taskId, uploadFileInfo, result));
        }
        CheckTaskRunning();
    }
    
    private void HandleUploadFailed(Guid taskId, UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        var taskInfo = TaskInfoList.FirstOrDefault(x => x.TaskId == taskId);
        if (taskInfo != null)
        {
            taskInfo.Status       = FileUploadStatus.Failed;
            taskInfo.ErrorMessage = result.UserFriendlyMessage;
            UploadTaskFailed?.Invoke(this, new UploadTaskFailedEventArgs(taskId, uploadFileInfo, result));
        }
        CheckTaskRunning();
    }
    
    private void HandleUploadCancelled(Guid taskId, UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        var taskInfo = TaskInfoList.FirstOrDefault(x => x.TaskId == taskId);
        if (taskInfo != null)
        {
            taskInfo.Status       = FileUploadStatus.Cancelled;
            taskInfo.ErrorMessage = result.UserFriendlyMessage;
            UploadTaskCancelled?.Invoke(this, new UploadTaskCancelledEventArgs(taskId, uploadFileInfo, result));
        }
        CheckTaskRunning();
    }

    private void CheckTaskRunning()
    {
        var isTaskRunning = false;
        foreach (var uploadTaskInfo in TaskInfoList)
        {
            if (uploadTaskInfo.Status == FileUploadStatus.Uploading)
            {
                isTaskRunning = true;
            }
        }
        SetCurrentValue(IsTaskRunningProperty, isTaskRunning);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _uploadListControl = e.NameScope.Find<ItemsControl>(UploadThemeConstants.UploadListPart);
        if (_uploadListControl != null)
        {
            _uploadListControl.ItemsSource = TaskInfoList;
        }
        ConfigurePictureTriggerTask();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UploadTransportProperty)
        {
            if (UploadTransport != null)
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                   await _uploadScheduler.SetTransportAsync(UploadTransport);
                });
            }
        }
        else if (change.Property == MaxConcurrentTasksProperty)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await _uploadScheduler.SetMaxConcurrentTasksAsync(MaxConcurrentTasks);
            });
        }
        else if (change.Property == ListTypeProperty)
        {
            ConfigurePictureTriggerTask();
        }
    }

    private bool IsImageFile(UploadFileInfo uploadFileInfo)
    {
        if (IsImageFilePredicate != null)
        {
            return IsImageFilePredicate.Invoke(uploadFileInfo);
        }
        var extension = Path.GetExtension(uploadFileInfo.FilePath.LocalPath).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }
        return ImageExtensionRegex.IsMatch(extension);
    }

    private async Task HandleUploadTaskRemoveRequestAsync(Guid taskId)
    {
        var taskInfo = _allTaskList.FirstOrDefault(x => x.TaskId == taskId);
        if (taskInfo != null)
        {
            if (taskInfo.Status != FileUploadStatus.Uploading || taskInfo.UploadTask == null)
            {
                TaskInfoList.Remove(taskInfo);
                _allTaskList.Remove(taskInfo);
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _uploadScheduler.CancelUploadAsync(taskInfo.UploadTask);
                    TaskInfoList.Remove(taskInfo);
                    _allTaskList.Remove(taskInfo);
                    UploadTaskRemoved?.Invoke(this, new UploadTaskRemovedEventArgs(taskId, taskInfo.UploadTask.UploadFileInfo!));
                });
            }
        }
    }
    
    private async Task CancelAllUploadTaskAsync()
    {
        for (int i = _allTaskList.Count - 1; i >= 0; i--)
        {
            var taskInfo = TaskInfoList[i];
            if (taskInfo.Status != FileUploadStatus.Uploading || taskInfo.UploadTask == null)
            {
                TaskInfoList.Remove(taskInfo);
                _allTaskList.Remove(taskInfo);
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await _uploadScheduler.CancelUploadAsync(taskInfo.UploadTask);
                    _allTaskList.Remove(taskInfo);
                    TaskInfoList.Remove(taskInfo);
                    UploadTaskRemoved?.Invoke(this, new UploadTaskRemovedEventArgs(taskInfo.TaskId, taskInfo.UploadTask.UploadFileInfo!));
                });
            }
        }
    }
    
    private void HandleFileSelectRequest()
    {
        if (IsOpenFileDialogOnClick)
        {
            OpenFileDialog();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (!_defaultTaskListApplied)
        {
            if (DefaultTaskList != null)
            {
                if (ListType == UploadListType.PictureCircle || ListType == UploadListType.PictureCard)
                {
                    // 检查是否有 trigger
                    var trigger = TaskInfoList.FirstOrDefault(x => x.IsPictureTriggerTask);
                    if (trigger != null)
                    {
                        TaskInfoList.InsertRange(0, DefaultTaskList);
                    }
                    else
                    {
                        TaskInfoList.AddRange(DefaultTaskList);
                    }
                }
                else
                {
                    TaskInfoList.AddRange(DefaultTaskList);
                }
                
                _allTaskList.AddRange(DefaultTaskList);
                _defaultTaskListApplied = true;
            }
        }
    }

    private void ConfigurePictureTriggerTask()
    {
        var taskInfo = TaskInfoList.FirstOrDefault(x => x.IsPictureTriggerTask);
        if (ListType == UploadListType.PictureCircle || ListType == UploadListType.PictureCard)
        {
            if (taskInfo == null)
            {
                TaskInfoList.Add(new UploadTaskInfo()
                {
                    IsPictureTriggerTask = true,
                });
            }
        }
        else
        {
            if (taskInfo != null)
            {
                TaskInfoList.Remove(taskInfo);
            }
        }
    }
}