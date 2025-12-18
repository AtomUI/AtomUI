using System.Net;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public class Upload : ContentControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<IReadOnlyList<string>?> AcceptsProperty =
        AvaloniaProperty.Register<Upload, IReadOnlyList<string>?>(nameof(Accepts));
    
    public static readonly StyledProperty<string?> ActionUrlProperty =
        AvaloniaProperty.Register<Upload, string?>(nameof(ActionUrl));
    
    public static readonly StyledProperty<Func<string?, CancellationToken, Task<bool>>?> BeforeUploadProperty =
        AvaloniaProperty.Register<Upload, Func<string?, CancellationToken, Task<bool>>?>(
            nameof(BeforeUpload));
    
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
    
    public static readonly StyledProperty<Func<UploadFileInfo, UploadListType, Control>?> IconRenderProperty =
        AvaloniaProperty.Register<Upload, Func<UploadFileInfo, UploadListType, Control>?>(nameof(IconRender));
    
    public static readonly StyledProperty<bool> IsMultipleEnabledProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsMultipleEnabled));
    
    public static readonly StyledProperty<bool> IsOpenFileDialogOnClickProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsOpenFileDialogOnClick), true);
    
    public static readonly StyledProperty<bool> IsPastableProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsPastable));
    
    public static readonly StyledProperty<IReadOnlyList<UploadFileInfo>?> DefaultFileListProperty =
        AvaloniaProperty.Register<Upload, IReadOnlyList<UploadFileInfo>?>(nameof(DefaultFileList));
    
    public static readonly StyledProperty<int> MaxConcurrentTasksProperty =
        AvaloniaProperty.Register<Upload, int>(nameof(MaxConcurrentTasks), 3);
    
    public static readonly DirectProperty<Upload, IFileUploadTransport?> UploadTransportProperty =
        AvaloniaProperty.RegisterDirect<Upload, IFileUploadTransport?>(
            nameof(UploadTransport),
            o => o.UploadTransport,
            (o, v) => o.UploadTransport = v);
    
    public IReadOnlyList<string>? Accepts
    {
        get => GetValue(AcceptsProperty);
        set => SetValue(AcceptsProperty, value);
    }
    
    public string? ActionUrl
    {
        get => GetValue(ActionUrlProperty);
        set => SetValue(ActionUrlProperty, value);
    }
    
    public Func<string?, CancellationToken, Task<bool>>? BeforeUpload
    {
        get => GetValue(BeforeUploadProperty);
        set => SetValue(BeforeUploadProperty, value);
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
    
    public Func<UploadFileInfo, UploadListType, Control>? IconRender
    {
        get => GetValue(IconRenderProperty);
        set => SetValue(IconRenderProperty, value);
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
    
    public bool IsPastable
    {
        get => GetValue(IsPastableProperty);
        set => SetValue(IsPastableProperty, value);
    }
    
    public IReadOnlyList<UploadFileInfo>? DefaultFileList
    {
        get => GetValue(DefaultFileListProperty);
        set => SetValue(DefaultFileListProperty, value);
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
    
    public AvaloniaList<UploadTaskInfo> TaskInfoList { get; } = new ();
    
    #endregion

    #region 公共回调函数定义

    public Func<UploadFileInfo, bool>? ImageTypePredicate { get; set; }
    public Action<UploadFileInfo>? FilePreviewHandler { get; set; }
    public Action<UploadFileInfo>? FileDropHandler { get; set; }
    public Action<UploadFileInfo, Task<bool>>? FileRemovedHandler { get; set; }
    
    #endregion
    
    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => UploadToken.ID;

    #endregion
    
    private IDisposable? _clickSubscription;
    private Point? _latestClickPosition;
    private ItemsControl? _uploadListControl;
    private Control? _triggerContent;
    private FileUploadScheduler _uploadScheduler;

    public Upload()
    {
        _uploadScheduler = new FileUploadScheduler();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureInputManager();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _clickSubscription?.Dispose();
    }

    private void ConfigureInputManager()
    {
        var inputManager = AvaloniaLocator.Current.GetService<IInputManager>();
        _clickSubscription = inputManager?.Process.Subscribe(ListenForMouseEvent);
    }
    
    private void ListenForMouseEvent(RawInputEventArgs e)
    {
        if (e is RawPointerEventArgs mouseEventArgs)
        {
            var point = mouseEventArgs.Point;
            var localPoint = TopLevel.GetTopLevel(this)?.TranslatePoint(point.Position, this) ?? default;
            var constraintOffset = _triggerContent?.TranslatePoint(new Point(0, 0), this) ?? default;
            var constraintBounds = new Rect(constraintOffset, _triggerContent?.Bounds.Size ?? new Size());
            if (constraintBounds.Contains(localPoint))
            {
                if (mouseEventArgs.Type == RawPointerEventType.LeftButtonDown)
                {
                    _latestClickPosition = localPoint;
                }
                else if (mouseEventArgs.Type == RawPointerEventType.LeftButtonUp)
                {
                    if (IsOpenFileDialogOnClick)
                    {
                        if (_latestClickPosition != null && constraintBounds.Contains(_latestClickPosition.Value))
                        {
                            OpenFileDialog();
                        }
                    }

                    _latestClickPosition = null;
                }
                else if (mouseEventArgs.Type == RawPointerEventType.Move)
                {
                    
                }
            }
        }
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
                    await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                    {
                        AllowMultiple = IsMultipleEnabled
                    });
                }
                else
                {
                    var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                    {
                        AllowMultiple = IsMultipleEnabled
                    });
                    foreach (var file in result)
                    {
                        await EnqueueUploadFile(file);
                    }
                }
            });
        }
    }

    private async Task EnqueueUploadFile(IStorageFile file)
    {
        var properties = await file.GetBasicPropertiesAsync();
        var uploadFile = new UploadFileInfo(
            file.Name,
            file.Path,
            properties.Size ?? 0,
            properties.DateCreated,
            properties.DateModified);
        var task = new FileUploadTask
        {
            UploadFileInfo = uploadFile
        };
        
        task.UploadProgressHandler  = HandleUploadProgressChanged;
        task.UploadCompletedHandler = HandleUploadCompleted;
        task.UploadFailedHandler    = HandleUploadFailed;
        task.UploadCancelledHandler = HandleUploadCancelled;

        var uploadTaskInfo = new UploadTaskInfo();
        uploadTaskInfo.TaskId = task.Id;
        TaskInfoList.Add(uploadTaskInfo);
        _uploadScheduler.EnqueueTask(task);
    }

    private void HandleUploadProgressChanged(UploadFileInfo fileInfo, double progress)
    {
        Console.WriteLine($"Upload Progress: {progress:00.00}%");
    }
    
    private void HandleUploadCompleted(UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        Console.WriteLine($"Upload Completed: {result.RemoteUrl}");
    }
    
    private void HandleUploadFailed(UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        Console.WriteLine($"Upload Failed: {result.UserFriendlyMessage}");
    }
    
    private void HandleUploadCancelled(UploadFileInfo uploadFileInfo, FileUploadResult result)
    {
        Console.WriteLine($"Upload Cancelled: {result.UserFriendlyMessage}");
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _uploadListControl = e.NameScope.Find<ItemsControl>(UploadThemeConstants.UploadListPart);
        _triggerContent = e.NameScope.Find<ContentPresenter>(UploadThemeConstants.TriggerContentPart);
        if (_uploadListControl != null)
        {
            _uploadListControl.ItemsSource = TaskInfoList;
        }
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
    }
}