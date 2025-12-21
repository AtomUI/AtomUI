using AtomUI.Controls;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

internal enum UploadListActions
{
    Remove,
    Preview
}

internal class TaskRemoveRequestEventArgs : RoutedEventArgs
{
    public Guid TaskId { get; }

    public TaskRemoveRequestEventArgs(Guid taskId)
    {
        TaskId = taskId;
    }
}

internal class AbstractUploadListItem : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<Guid> TaskIdProperty =
        AvaloniaProperty.Register<AbstractUploadListItem, Guid>(nameof(TaskId));
    
    public static readonly StyledProperty<string?> FileNameProperty =
        AvaloniaProperty.Register<AbstractUploadListItem, string?>(nameof(FileName));
    
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<AbstractUploadListItem, double>(nameof(Progress));
    
    public static readonly StyledProperty<bool> IsImageFileProperty =
        AvaloniaProperty.Register<AbstractUploadListItem, bool>(nameof(IsImageFile));
    
    public static readonly StyledProperty<FileUploadStatus> StatusProperty =
        AvaloniaProperty.Register<AbstractUploadListItem, FileUploadStatus>(nameof(Status));
    
    public static readonly StyledProperty<string?> ErrorMessageProperty =
        AvaloniaProperty.Register<AbstractUploadListItem, string?>(nameof(ErrorMessage));
    
    public static readonly StyledProperty<Uri?> FilePathProperty =
        AvaloniaProperty.Register<AbstractUploadListItem, Uri?>(nameof(FilePath));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractUploadListItem>();

    public Guid TaskId
    {
        get => GetValue(TaskIdProperty);
        set => SetValue(TaskIdProperty, value);
    }
    
    public string? FileName
    {
        get => GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }
    
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public bool IsImageFile
    {
        get => GetValue(IsImageFileProperty);
        set => SetValue(IsImageFileProperty, value);
    }
    
    public FileUploadStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    public string? ErrorMessage
    {
        get => GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }
    
    public Uri? FilePath
    {
        get => GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<TaskRemoveRequestEventArgs> TaskRemoveRequestEvent =
        RoutedEvent.Register<AbstractUploadListItem, TaskRemoveRequestEventArgs>(nameof(TaskRemoveRequest), RoutingStrategies.Bubble);

    public event EventHandler<TaskRemoveRequestEventArgs>? TaskRemoveRequest
    {
        add => AddHandler(TaskRemoveRequestEvent, value);
        remove => RemoveHandler(TaskRemoveRequestEvent, value);
    }

    #endregion

    static AbstractUploadListItem()
    {
        IconButton.ClickEvent.AddClassHandler<AbstractUploadListItem>((o, args) => o.HandleActionButtonClicked((args.Source as IconButton)!));
    }
    
    private void HandleActionButtonClicked(IconButton button)
    {
        if (button.Tag is UploadListActions actionType)
        {
            if (actionType == UploadListActions.Remove)
            {
                RaiseTaskRemoveRequestEvent();
            }
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = new Transitions();
            }
        }
        else
        {
            Transitions = null;
        }
    }

    protected virtual void NotifyConfigureTransitions(Transitions transitions)
    {
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }

    protected virtual void RaiseTaskRemoveRequestEvent()
    {
        RaiseEvent(new TaskRemoveRequestEventArgs(TaskId)
        {
            RoutedEvent = TaskRemoveRequestEvent,
            Source = this
        });
    }
}