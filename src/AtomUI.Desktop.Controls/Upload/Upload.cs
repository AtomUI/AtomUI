using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Platform.Storage;

namespace AtomUI.Desktop.Controls;

public partial class Upload : ContentControl,
                              IMotionAwareControl,
                              IFormItemAware
{
    #region 公共属性定义

    public static readonly StyledProperty<IList<UploadFileItem>?> FilesProperty =
        AvaloniaProperty.Register<Upload, IList<UploadFileItem>?>(
            nameof(Files),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly StyledProperty<IReadOnlyList<FilePickerFileType>?> AllowedFileTypesProperty =
        AvaloniaProperty.Register<Upload, IReadOnlyList<FilePickerFileType>?>(nameof(AllowedFileTypes));

    public static readonly StyledProperty<UploadCountOverflowBehavior> CountOverflowBehaviorProperty =
        AvaloniaProperty.Register<Upload, UploadCountOverflowBehavior>(
            nameof(CountOverflowBehavior),
            UploadCountOverflowBehavior.RejectExcess);

    public static readonly StyledProperty<IUploadAdmissionPolicy?> AdmissionPolicyProperty =
        AvaloniaProperty.Register<Upload, IUploadAdmissionPolicy?>(nameof(AdmissionPolicy));

    public static readonly StyledProperty<object?> ExtraContextProperty =
        AvaloniaProperty.Register<Upload, object?>(nameof(ExtraContext));

    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<Upload, int>(nameof(MaxCount), int.MaxValue);

    public static readonly StyledProperty<int> MaxConcurrentTasksProperty =
        AvaloniaProperty.Register<Upload, int>(nameof(MaxConcurrentTasks), 3);

    public static readonly StyledProperty<bool> AutoUploadProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(AutoUpload), true);

    public static readonly StyledProperty<UploadListType> ListTypeProperty =
        AvaloniaProperty.Register<Upload, UploadListType>(nameof(ListType));

    public static readonly StyledProperty<bool> IsMultipleEnabledProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsMultipleEnabled));

    public static readonly StyledProperty<bool> IsShowUploadListProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsShowUploadList), true);

    public static readonly StyledProperty<double> ListMaxHeightProperty =
        AvaloniaProperty.Register<Upload, double>(
            nameof(ListMaxHeight),
            double.PositiveInfinity);

    public static readonly StyledProperty<ScrollBarVisibility> ListScrollBarVisibilityProperty =
        AvaloniaProperty.Register<Upload, ScrollBarVisibility>(
            nameof(ListScrollBarVisibility),
            ScrollBarVisibility.Disabled);

    public static readonly StyledProperty<TimeSpan?> SuccessAutoRemoveDelayProperty =
        AvaloniaProperty.Register<Upload, TimeSpan?>(nameof(SuccessAutoRemoveDelay));

    public static readonly StyledProperty<string?> PendingTextProperty =
        AvaloniaProperty.Register<Upload, string?>(nameof(PendingText));

    public static readonly StyledProperty<UploadFileValueMode> FileValueModeProperty =
        AvaloniaProperty.Register<Upload, UploadFileValueMode>(
            nameof(FileValueMode),
            UploadFileValueMode.SuccessfulFiles);

    public static readonly StyledProperty<object?> TriggerContentProperty =
        AvaloniaProperty.Register<Upload, object?>(nameof(TriggerContent));

    public static readonly StyledProperty<IDataTemplate?> TriggerContentTemplateProperty =
        AvaloniaProperty.Register<Upload, IDataTemplate?>(nameof(TriggerContentTemplate));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Upload>();

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

    public IList<UploadFileItem>? Files
    {
        get => GetValue(FilesProperty);
        set => SetValue(FilesProperty, value);
    }

    public IReadOnlyList<FilePickerFileType>? AllowedFileTypes
    {
        get => GetValue(AllowedFileTypesProperty);
        set => SetValue(AllowedFileTypesProperty, value);
    }

    public UploadCountOverflowBehavior CountOverflowBehavior
    {
        get => GetValue(CountOverflowBehaviorProperty);
        set => SetValue(CountOverflowBehaviorProperty, value);
    }

    public IUploadAdmissionPolicy? AdmissionPolicy
    {
        get => GetValue(AdmissionPolicyProperty);
        set => SetValue(AdmissionPolicyProperty, value);
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

    public int MaxConcurrentTasks
    {
        get => GetValue(MaxConcurrentTasksProperty);
        set => SetValue(MaxConcurrentTasksProperty, value);
    }

    public bool AutoUpload
    {
        get => GetValue(AutoUploadProperty);
        set => SetValue(AutoUploadProperty, value);
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

    public bool IsShowUploadList
    {
        get => GetValue(IsShowUploadListProperty);
        set => SetValue(IsShowUploadListProperty, value);
    }

    public double ListMaxHeight
    {
        get => GetValue(ListMaxHeightProperty);
        set => SetValue(ListMaxHeightProperty, value);
    }

    public ScrollBarVisibility ListScrollBarVisibility
    {
        get => GetValue(ListScrollBarVisibilityProperty);
        set => SetValue(ListScrollBarVisibilityProperty, value);
    }

    public TimeSpan? SuccessAutoRemoveDelay
    {
        get => GetValue(SuccessAutoRemoveDelayProperty);
        set => SetValue(SuccessAutoRemoveDelayProperty, value);
    }

    public string? PendingText
    {
        get => GetValue(PendingTextProperty);
        set => SetValue(PendingTextProperty, value);
    }

    public UploadFileValueMode FileValueMode
    {
        get => GetValue(FileValueModeProperty);
        set => SetValue(FileValueModeProperty, value);
    }

    [DependsOn(nameof(TriggerContentTemplate))]
    public object? TriggerContent
    {
        get => GetValue(TriggerContentProperty);
        set => SetValue(TriggerContentProperty, value);
    }

    public IDataTemplate? TriggerContentTemplate
    {
        get => GetValue(TriggerContentTemplateProperty);
        set => SetValue(TriggerContentTemplateProperty, value);
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

    #endregion

    #region 公共事件定义

    public event EventHandler<UploadTaskCreatedEventArgs>? UploadTaskCreated;
    public event EventHandler<UploadTaskAboutToSchedulingEventArgs>? UploadTaskAboutToScheduling;
    public event EventHandler<UploadTaskProgressEventArgs>? UploadTaskProgress;
    public event EventHandler<UploadTaskCompletedEventArgs>? UploadTaskCompleted;
    public event EventHandler<UploadTaskCancelledEventArgs>? UploadTaskCancelled;
    public event EventHandler<UploadTaskFailedEventArgs>? UploadTaskFailed;
    public event EventHandler<UploadTaskRemovedEventArgs>? UploadTaskRemoved;
    public event EventHandler<UploadInputBatchCompletedEventArgs>? InputBatchCompleted;

    #endregion

    #region 公共回调函数定义

    public Func<UploadFileInfo, bool>? IsImageFilePredicate { get; set; }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Upload, IList<UploadFileItem>> EffectiveFilesProperty =
        AvaloniaProperty.RegisterDirect<Upload, IList<UploadFileItem>>(
            nameof(EffectiveFiles),
            o => o.EffectiveFiles);

    internal static readonly DirectProperty<Upload, IList<object?>> EffectivePictureItemsProperty =
        AvaloniaProperty.RegisterDirect<Upload, IList<object?>>(
            nameof(EffectivePictureItems),
            o => o.EffectivePictureItems);

    internal IList<UploadFileItem> EffectiveFiles
    {
        get => _effectiveFiles;
        private set => SetAndRaise(EffectiveFilesProperty, ref _effectiveFiles, value);
    }

    internal IList<object?> EffectivePictureItems => _effectivePictureItems;

    #endregion

    private static readonly Regex ImageExtensionRegex =
        new(@"\.(webp|svg|png|gif|jpg|jpeg|jfif|bmp|dpg|ico|heic|heif)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly AvaloniaList<UploadFileItem> _ownedFiles = new();
    private readonly AvaloniaList<object?> _effectivePictureItems = new();
    private readonly UploadAppendContentItem _appendContentItem = new();
    private readonly HashSet<UploadFileItem> _observedFileItems = new();
    private readonly Dictionary<Guid, CancellationTokenSource> _successAutoRemoveDelays = new();
    private readonly Dictionary<Guid, UploadFileInfo> _acceptedFileInfos = new();
    private readonly UploadQueue _uploadQueue;
    private readonly UploadInputPipeline _inputPipeline;
    private IList<UploadFileItem> _effectiveFiles = null!;
    private INotifyCollectionChanged? _attachedFileCollection;
    private EventHandler? _formValueChanged;

    internal static void RegisterTaskRemoveRequestEvent(RoutedEvent<TaskRemoveRequestEventArgs> requestEvent)
    {
        requestEvent.AddClassHandler<Upload>(
            (upload, args) => upload.HandleTaskRemoveRequest(args));
    }

    public Upload()
    {
        _effectiveFiles = _ownedFiles;
        _uploadQueue = new UploadQueue(this, UploadTransport, MaxConcurrentTasks);
        _inputPipeline = new UploadInputPipeline(this);
        SyncAppendContentItem();
        AttachFileCollection(EffectiveFiles);
        SyncEffectivePictureItems();
    }

    public Task EnqueueFilesAsync(IEnumerable<UploadFileInfo> files, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(files);
        var materializedFiles = files.ToArray();
        return _inputPipeline.ProcessFilesAsync(
            UploadInputSource.Programmatic,
            materializedFiles,
            cancellationToken);
    }

    public async Task RemoveFileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var files = EffectiveFiles;
        var item = FindFile(id);
        if (item is null)
        {
            return;
        }

        await _uploadQueue.CancelAsync(id, cancellationToken);
        CancelSuccessAutoRemove(id);
        RemoveFileCore(item, raiseRemovedEvent: true);
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cleanupExceptions = new List<Exception>();
        try
        {
            await _inputPipeline.CancelAllAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            cleanupExceptions.Add(ex);
        }

        try
        {
            await _uploadQueue.CancelAllAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            cleanupExceptions.Add(ex);
        }

        try
        {
            await InvokeOnUiThreadAsync(ClearEffectiveFilesAfterQueueCancellation).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            cleanupExceptions.Add(ex);
        }

        ThrowCleanupExceptions(cleanupExceptions);
    }

    public void Reset()
    {
        StartObservedLifecycleOperation(ResetAsync());
    }

    private void HandleTaskRemoveRequest(TaskRemoveRequestEventArgs args)
    {
        if (args.Handled)
        {
            return;
        }

        args.Handled = true;
        StartObservedLifecycleOperation(RemoveFileAsync(args.TaskId));
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CancelAllSuccessAutoRemove();
        var task = HandleDetachedAsync();
        if (!task.IsCompletedSuccessfully)
        {
            _ = ObserveUploadLifecycleTaskAsync(task);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FilesProperty)
        {
            var typedChange = (AvaloniaPropertyChangedEventArgs<IList<UploadFileItem>?>)change;
            HandleFilesChanged(typedChange.OldValue.Value, typedChange.NewValue.Value);
        }
        else if (change.Property == TriggerContentProperty ||
                 change.Property == TriggerContentTemplateProperty ||
                 change.Property == HorizontalContentAlignmentProperty ||
                 change.Property == VerticalContentAlignmentProperty)
        {
            SyncAppendContentItem();
        }
        else if (change.Property == UploadTransportProperty)
        {
            StartObservedLifecycleOperation(_uploadQueue.SetTransportAsync(UploadTransport));
        }
        else if (change.Property == MaxConcurrentTasksProperty)
        {
            StartObservedLifecycleOperation(_uploadQueue.SetMaxConcurrentTasksAsync(MaxConcurrentTasks));
        }
    }

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();

    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();

    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(object? value)
    {
        var replacementItems = value is IEnumerable<UploadFileItem> items
            ? items.ToArray()
            : [];
        ReplaceEffectiveFilesContents(replacementItems);
    }

    protected virtual object? NotifyGetFormValue()
    {
        return FileValueMode switch
        {
            UploadFileValueMode.AllFiles => EffectiveFiles.ToArray(),
            UploadFileValueMode.Results => EffectiveFiles
                                           .Where(file => file.Status == FileUploadStatus.Success)
                                           .Select(file => file.Result)
                                           .Where(result => result is not null)
                                           .ToArray(),
            _ => EffectiveFiles
                 .Where(file => file.Status == FileUploadStatus.Success)
                 .ToArray()
        };
    }

    protected virtual void NotifyClearFormValue()
    {
        if (Dispatcher.CheckAccess())
        {
            ReplaceEffectiveFilesContents([]);
        }
        else
        {
            StartObservedLifecycleOperation(
                Dispatcher.InvokeAsync(
                    () => ReplaceEffectiveFilesContents([])).GetTask());
        }
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }

    #endregion

    internal void NotifyUploadProgress(UploadFileItem item, UploadFileInfo fileInfo, double progress)
    {
        RunOnUiThread(() =>
        {
            if (IsTerminalUploadStatus(item.Status))
            {
                return;
            }

            item.Status   = FileUploadStatus.Uploading;
            item.Progress = progress;
            SetCurrentValue(IsTaskRunningProperty, true);
            UploadTaskProgress?.Invoke(this, new UploadTaskProgressEventArgs(item.Id, fileInfo, progress));
        });
    }

    internal void NotifyUploadCompleted(UploadFileItem item, UploadFileInfo fileInfo, FileUploadResult result)
    {
        RunOnUiThread(() =>
        {
            item.Status = FileUploadStatus.Success;
            item.Result = result;
            UploadTaskCompleted?.Invoke(this, new UploadTaskCompletedEventArgs(item.Id, fileInfo, result));
            ScheduleSuccessAutoRemove(item);
            UpdateTaskRunning();
        });
    }

    internal void NotifyUploadFailed(UploadFileItem item, UploadFileInfo fileInfo, FileUploadResult result)
    {
        RunOnUiThread(() =>
        {
            item.Status       = FileUploadStatus.Failed;
            item.ErrorMessage = result.UserFriendlyMessage;
            item.Result       = result;
            UploadTaskFailed?.Invoke(this, new UploadTaskFailedEventArgs(item.Id, fileInfo, result));
            UpdateTaskRunning();
        });
    }

    internal void NotifyUploadCancelled(UploadFileItem item, UploadFileInfo fileInfo, FileUploadResult result)
    {
        RunOnUiThread(() =>
        {
            item.Status       = FileUploadStatus.Cancelled;
            item.ErrorMessage = result.UserFriendlyMessage;
            item.Result       = result;
            UploadTaskCancelled?.Invoke(this, new UploadTaskCancelledEventArgs(item.Id, fileInfo, result));
            UpdateTaskRunning();
        });
    }

    private IReadOnlyList<UploadFileInfo> CommitInputFiles(
        IReadOnlyList<UploadFileInfo> files,
        UploadInputBatchOperation operation,
        UploadInputPipelineOptions options)
    {
        ValidateInputFileOwnership(files, operation);
        var (filesToCommit, rejectedFiles) = ApplyInputCountPolicy(
            files,
            options.CountOverflowBehavior,
            options.MaxCount,
            EffectiveFiles.Count);
        foreach (var file in filesToCommit)
        {
            CommitInputFile(file, operation);
        }
        return rejectedFiles;
    }

    private IReadOnlyList<UploadFileInfo> ReplaceInputFiles(
        IReadOnlyList<UploadFileInfo> files,
        UploadInputBatchOperation operation,
        UploadInputPipelineOptions options)
    {
        ValidateInputFileOwnership(files, operation);
        var (filesToCommit, rejectedFiles) = ApplyInputCountPolicy(
            files,
            UploadCountOverflowBehavior.ReplaceExisting,
            options.MaxCount,
            existingCount: 0);
        if (filesToCommit.Count == 0)
        {
            return rejectedFiles;
        }

        var entries = filesToCommit
                      .Select(file => KeyValuePair.Create(CreateInputFileItem(file), file))
                      .ToArray();
        var previousItems = EffectiveFiles.ToArray();
        KeyValuePair<Guid, UploadFileInfo>[]? removedFileInfos = null;
        try
        {
            removedFileInfos = ReplaceEffectiveFilesCore(entries.Select(entry => entry.Key).ToArray());
            foreach (var entry in entries)
            {
                AcceptInputFile(entry.Key, entry.Value, operation);
            }

            NotifyFormValueChanged(EffectiveFiles);
            foreach (var entry in entries)
            {
                RaiseInputFileCommitted(entry.Key, entry.Value);
            }
        }
        catch
        {
            AcceptRetainedInputFiles(entries, operation);
            throw;
        }
        finally
        {
            removedFileInfos ??= TakeRemovedAcceptedFileInfos(previousItems);
            StartUploadCancellationAndRelease(removedFileInfos);
        }
        return rejectedFiles;
    }

    private void CommitInputFile(UploadFileInfo file, UploadInputBatchOperation operation)
    {
        var files = EffectiveFiles;
        var item = CreateInputFileItem(file);

        AttachFileItem(item);
        try
        {
            files.Add(item);
        }
        catch
        {
            if (files.Contains(item))
            {
                AcceptInputFile(item, file, operation);
            }
            else
            {
                DetachFileItem(item);
            }
            throw;
        }

        AcceptInputFile(item, file, operation);
        if (files is not INotifyCollectionChanged)
        {
            InsertEffectivePictureItem(item, files.IndexOf(item));
        }
        NotifyFormValueChanged(files);
        RaiseInputFileCommitted(item, file);
    }

    private UploadFileItem CreateInputFileItem(UploadFileInfo file)
    {
        return new UploadFileItem
        {
            Name        = file.Name,
            Path        = file.Path,
            Size        = file.Size ?? 0,
            PendingText = PendingText,
            IsImageFile = IsImageFile(file),
            UserData    = ExtraContext
        };
    }

    private void AcceptInputFile(
        UploadFileItem item,
        UploadFileInfo file,
        UploadInputBatchOperation operation)
    {
        _acceptedFileInfos[item.Id] = file;
        if (file.Source is IUploadFileSourceLease)
        {
            operation.TransferFileSourceToUpload(file);
        }
        operation.Accept(file);
    }

    private void AcceptRetainedInputFiles(
        IReadOnlyList<KeyValuePair<UploadFileItem, UploadFileInfo>> entries,
        UploadInputBatchOperation operation)
    {
        foreach (var entry in entries)
        {
            if (EffectiveFiles.Contains(entry.Key) && !_acceptedFileInfos.ContainsKey(entry.Key.Id))
            {
                AcceptInputFile(entry.Key, entry.Value, operation);
            }
        }
    }

    private void RaiseInputFileCommitted(UploadFileItem item, UploadFileInfo file)
    {
        UploadTaskCreated?.Invoke(this, new UploadTaskCreatedEventArgs(item.Id, file));

        var aboutToSchedulingEvent = new UploadTaskAboutToSchedulingEventArgs(item.Id, file);
        UploadTaskAboutToScheduling?.Invoke(this, aboutToSchedulingEvent);
        if (aboutToSchedulingEvent.Result == UploadPredicateResult.Cancel)
        {
            item.Status       = FileUploadStatus.Failed;
            item.ErrorMessage = aboutToSchedulingEvent.CancelReason;
            return;
        }

        if (AutoUpload && aboutToSchedulingEvent.Result == UploadPredicateResult.Schedule)
        {
            _uploadQueue.Enqueue(item, file);
        }
        else if (aboutToSchedulingEvent.Result == UploadPredicateResult.CancelWithInTaskList)
        {
            item.Status       = FileUploadStatus.Failed;
            item.ErrorMessage = aboutToSchedulingEvent.CancelReason;
        }
    }

    private static void ValidateInputFileOwnership(
        IReadOnlyList<UploadFileInfo> files,
        UploadInputBatchOperation operation)
    {
        var sourceLeases = new HashSet<IUploadFileSourceLease>(ReferenceEqualityComparer.Instance);
        foreach (var file in files)
        {
            if (file.Source is not IUploadFileSourceLease lease)
            {
                continue;
            }
            if (!operation.OwnsFileSource(file))
            {
                throw new InvalidOperationException("The input file source is not owned by this input batch.");
            }
            if (!sourceLeases.Add(lease))
            {
                throw new InvalidOperationException("An input file source cannot be committed more than once.");
            }
        }
    }

    private static (IReadOnlyList<UploadFileInfo> Accepted, IReadOnlyList<UploadFileInfo> Rejected)
        ApplyInputCountPolicy(
            IReadOnlyList<UploadFileInfo> files,
            UploadCountOverflowBehavior overflowBehavior,
            int maxCount,
            int existingCount)
    {
        var availableCount = Math.Max(0, Math.Max(0, maxCount) - Math.Max(0, existingCount));
        if (files.Count <= availableCount)
        {
            return (files, []);
        }
        if (overflowBehavior == UploadCountOverflowBehavior.RejectBatch)
        {
            return ([], files);
        }
        return (files.Take(availableCount).ToArray(), files.Skip(availableCount).ToArray());
    }

    private void HandleFilesChanged(IList<UploadFileItem>? oldFiles, IList<UploadFileItem>? newFiles)
    {
        CancelAllSuccessAutoRemove();
        var previousFiles = oldFiles ?? _ownedFiles;
        var nextFiles = newFiles ?? _ownedFiles;
        var previousItems = previousFiles.ToArray();
        try
        {
            DetachFileCollection(previousFiles);
            EffectiveFiles = nextFiles;
            AttachFileCollection(EffectiveFiles);
            SyncEffectivePictureItems();
            NotifyFormValueChanged(EffectiveFiles);
        }
        finally
        {
            var retainedIds = EffectiveFiles.Select(item => item.Id).ToHashSet();
            var removedFileInfos = TakeAcceptedFileInfos(
                previousItems.Where(item => !retainedIds.Contains(item.Id)));
            StartUploadCancellationAndRelease(removedFileInfos);
        }
    }

    private void AttachFileCollection(IList<UploadFileItem> files)
    {
        if (files is INotifyCollectionChanged notifyCollectionChanged)
        {
            _attachedFileCollection = notifyCollectionChanged;
            notifyCollectionChanged.CollectionChanged += HandleFilesCollectionChanged;
        }

        foreach (var item in files)
        {
            AttachFileItem(item);
        }
    }

    private void DetachFileCollection(IList<UploadFileItem> files)
    {
        if (_attachedFileCollection is not null)
        {
            _attachedFileCollection.CollectionChanged -= HandleFilesCollectionChanged;
            _attachedFileCollection = null;
        }

        foreach (var item in files)
        {
            DetachFileItem(item);
        }
    }

    private void HandleFilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var removedFileInfos = new List<KeyValuePair<Guid, UploadFileInfo>>();
        if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Replace &&
            e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<UploadFileItem>())
            {
                DetachFileItem(item);
                CancelSuccessAutoRemove(item.Id);
                AddAcceptedFileInfo(item.Id, removedFileInfos);
            }
        }

        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Replace &&
            e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<UploadFileItem>())
            {
                AttachFileItem(item);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            CancelAllSuccessAutoRemove();
            var currentItems = new HashSet<UploadFileItem>(EffectiveFiles, ReferenceEqualityComparer.Instance);
            foreach (var item in _observedFileItems.Where(item => !currentItems.Contains(item)).ToArray())
            {
                DetachFileItem(item);
                AddAcceptedFileInfo(item.Id, removedFileInfos);
            }
            foreach (var item in EffectiveFiles)
            {
                AttachFileItem(item);
            }
        }

        try
        {
            SyncEffectivePictureItems(e);
            NotifyFormValueChanged(EffectiveFiles);
        }
        finally
        {
            StartUploadCancellationAndRelease(removedFileInfos);
        }
    }

    private void AttachFileItem(UploadFileItem item)
    {
        if (_observedFileItems.Add(item))
        {
            item.PropertyChanged += HandleFileItemPropertyChanged;
        }
    }

    private void DetachFileItem(UploadFileItem item)
    {
        if (_observedFileItems.Remove(item))
        {
            item.PropertyChanged -= HandleFileItemPropertyChanged;
        }
    }

    private void HandleFileItemPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not UploadFileItem item || e.Property != UploadFileItem.StatusProperty)
        {
            return;
        }

        if (item.Status == FileUploadStatus.Success)
        {
            ScheduleSuccessAutoRemove(item);
        }
        else
        {
            CancelSuccessAutoRemove(item.Id);
        }

        UpdateTaskRunning();
    }

    private UploadFileItem? FindFile(Guid id)
    {
        return EffectiveFiles.FirstOrDefault(file => file.Id == id);
    }

    private void ClearEffectiveFilesAfterQueueCancellation()
    {
        var previousItems = EffectiveFiles.ToArray();
        KeyValuePair<Guid, UploadFileInfo>[]? removedFileInfos = null;
        try
        {
            removedFileInfos = ReplaceEffectiveFilesCore([]);
            NotifyFormValueChanged(EffectiveFiles);
        }
        finally
        {
            removedFileInfos ??= TakeRemovedAcceptedFileInfos(previousItems);
            DisposeUploadFileInfos(removedFileInfos.Select(pair => pair.Value));
        }
    }

    private void SyncAppendContentItem()
    {
        _appendContentItem.Content                    = TriggerContent;
        _appendContentItem.ContentTemplate            = TriggerContentTemplate;
        _appendContentItem.HorizontalContentAlignment = HorizontalContentAlignment;
        _appendContentItem.VerticalContentAlignment   = VerticalContentAlignment;
        _appendContentItem.IsVisible                  = TriggerContent is not null;
    }

    private void SyncEffectivePictureItems()
    {
        _effectivePictureItems.Clear();
        foreach (var item in EffectiveFiles)
        {
            _effectivePictureItems.Add(item);
        }
        _effectivePictureItems.Add(_appendContentItem);
    }

    private void SyncEffectivePictureItems(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                InsertEffectivePictureItems(e.NewItems, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                RemoveEffectivePictureItems(e.OldItems);
                break;
            case NotifyCollectionChangedAction.Replace:
                RemoveEffectivePictureItems(e.OldItems);
                InsertEffectivePictureItems(e.NewItems, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Move:
                RemoveEffectivePictureItems(e.OldItems);
                InsertEffectivePictureItems(e.NewItems, e.NewStartingIndex);
                break;
            default:
                SyncEffectivePictureItems();
                break;
        }
    }

    private void InsertEffectivePictureItems(IList? items, int index)
    {
        if (items is null)
        {
            return;
        }

        var insertIndex = index;
        foreach (var item in items.OfType<UploadFileItem>())
        {
            if (insertIndex < 0)
            {
                insertIndex = EffectiveFiles.IndexOf(item);
            }
            InsertEffectivePictureItem(item, insertIndex);
            insertIndex++;
        }
    }

    private void InsertEffectivePictureItem(UploadFileItem item, int index)
    {
        if (_effectivePictureItems.Contains(item))
        {
            return;
        }

        var appendIndex = EnsureAppendContentItem();
        var insertIndex = Math.Clamp(index, 0, appendIndex);
        _effectivePictureItems.Insert(insertIndex, item);
    }

    private void RemoveEffectivePictureItems(IList? items)
    {
        if (items is null)
        {
            return;
        }

        foreach (var item in items.OfType<UploadFileItem>())
        {
            RemoveEffectivePictureItem(item);
        }
    }

    private void RemoveEffectivePictureItem(UploadFileItem item)
    {
        _effectivePictureItems.Remove(item);
        EnsureAppendContentItem();
    }

    private int EnsureAppendContentItem()
    {
        var appendIndex = _effectivePictureItems.IndexOf(_appendContentItem);
        if (appendIndex >= 0)
        {
            return appendIndex;
        }

        _effectivePictureItems.Add(_appendContentItem);
        return _effectivePictureItems.Count - 1;
    }

    private bool IsImageFile(UploadFileInfo file)
    {
        if (IsImageFilePredicate is not null)
        {
            return IsImageFilePredicate.Invoke(file);
        }

        var extension = System.IO.Path.GetExtension(file.Name);
        return !string.IsNullOrEmpty(extension) && ImageExtensionRegex.IsMatch(extension);
    }

    private UploadFileInfo CreateUploadFileInfo(UploadFileItem item)
    {
        if (_acceptedFileInfos.TryGetValue(item.Id, out var fileInfo))
        {
            return fileInfo;
        }

        return new UploadFileInfo(
            string.IsNullOrWhiteSpace(item.Name) ? "unnamed" : item.Name,
            DisplayOnlyUploadFileSource.Instance,
            item.Path,
            item.Size);
    }

    private void RemoveFileCore(UploadFileItem item, bool raiseRemovedEvent)
    {
        var files = EffectiveFiles;
        var fileInfo = CreateUploadFileInfo(item);
        try
        {
            files.Remove(item);
            if (files.Contains(item))
            {
                return;
            }

            if (files is not INotifyCollectionChanged)
            {
                DetachFileItem(item);
                RemoveEffectivePictureItem(item);
            }
            NotifyFormValueChanged(files);
            if (raiseRemovedEvent)
            {
                UploadTaskRemoved?.Invoke(this, new UploadTaskRemovedEventArgs(item.Id, fileInfo));
            }
        }
        finally
        {
            if (!files.Contains(item))
            {
                DetachFileItem(item);
                DisposeUploadFileInfo(TakeAcceptedFileInfo(item.Id));
            }
        }
    }

    private void ReleaseAcceptedFileInfo(Guid id)
    {
        DisposeUploadFileInfo(TakeAcceptedFileInfo(id));
    }

    private UploadFileInfo? TakeAcceptedFileInfo(Guid id)
    {
        return _acceptedFileInfos.Remove(id, out var fileInfo) ? fileInfo : null;
    }

    private KeyValuePair<Guid, UploadFileInfo>[] TakeAcceptedFileInfos(IEnumerable<UploadFileItem> items)
    {
        var fileInfos = new List<KeyValuePair<Guid, UploadFileInfo>>();
        foreach (var item in items)
        {
            AddAcceptedFileInfo(item.Id, fileInfos);
        }
        return fileInfos.ToArray();
    }

    private void AddAcceptedFileInfo(Guid id, ICollection<KeyValuePair<Guid, UploadFileInfo>> fileInfos)
    {
        var fileInfo = TakeAcceptedFileInfo(id);
        if (fileInfo is not null)
        {
            fileInfos.Add(KeyValuePair.Create(id, fileInfo));
        }
    }

    private void ReplaceEffectiveFilesContents(IReadOnlyList<UploadFileItem> replacementItems)
    {
        var previousItems = EffectiveFiles.ToArray();
        KeyValuePair<Guid, UploadFileInfo>[]? removedFileInfos = null;
        try
        {
            removedFileInfos = ReplaceEffectiveFilesCore(replacementItems);
            NotifyFormValueChanged(EffectiveFiles);
        }
        finally
        {
            removedFileInfos ??= TakeRemovedAcceptedFileInfos(previousItems);
            StartUploadCancellationAndRelease(removedFileInfos);
        }
    }

    private KeyValuePair<Guid, UploadFileInfo>[] ReplaceEffectiveFilesCore(
        IReadOnlyList<UploadFileItem> replacementItems)
    {
        CancelAllSuccessAutoRemove();
        var files = EffectiveFiles;
        var previousItems = files.ToArray();
        DetachFileCollection(files);
        try
        {
            files.Clear();
            foreach (var item in replacementItems)
            {
                files.Add(item);
            }
        }
        finally
        {
            AttachFileCollection(files);
            SyncEffectivePictureItems();
        }
        return TakeRemovedAcceptedFileInfos(previousItems);
    }

    private KeyValuePair<Guid, UploadFileInfo>[] TakeRemovedAcceptedFileInfos(
        IReadOnlyList<UploadFileItem> previousItems)
    {
        var retainedIds = EffectiveFiles.Select(item => item.Id).ToHashSet();
        return TakeAcceptedFileInfos(previousItems.Where(item => !retainedIds.Contains(item.Id)));
    }

    private void StartUploadCancellationAndRelease(
        IReadOnlyList<KeyValuePair<Guid, UploadFileInfo>> fileInfos)
    {
        if (fileInfos.Count == 0)
        {
            return;
        }

        StartObservedLifecycleOperation(CancelUploadsAndReleaseAsync(fileInfos));
    }

    private async Task CancelUploadsAndReleaseAsync(
        IReadOnlyList<KeyValuePair<Guid, UploadFileInfo>> fileInfos)
    {
        foreach (var pair in fileInfos)
        {
            try
            {
                await _uploadQueue.CancelAsync(pair.Key).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Upload cancellation failed before source release: {ex.Message}");
            }
            finally
            {
                DisposeUploadFileInfo(pair.Value);
            }
        }
    }

    private static void DisposeUploadFileInfos(IEnumerable<UploadFileInfo> fileInfos)
    {
        foreach (var fileInfo in fileInfos)
        {
            DisposeUploadFileInfo(fileInfo);
        }
    }

    private static void DisposeUploadFileInfo(UploadFileInfo? fileInfo)
    {
        if (fileInfo?.Source is IUploadFileSourceLease lease)
        {
            lease.Dispose();
        }
    }

    private void StartObservedLifecycleOperation(Task task)
    {
        if (!task.IsCompletedSuccessfully)
        {
            _ = ObserveUploadLifecycleTaskAsync(task);
        }
    }

    private async Task HandleDetachedAsync()
    {
        var cleanupExceptions = new List<Exception>();
        try
        {
            await _inputPipeline.CancelAllAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            cleanupExceptions.Add(ex);
        }

        try
        {
            await _uploadQueue.CancelAllAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            cleanupExceptions.Add(ex);
        }

        UploadFileInfo[]? detachedFileInfos = null;
        try
        {
            await InvokeOnUiThreadAsync(() =>
            {
                detachedFileInfos = _acceptedFileInfos.Values.ToArray();
                _acceptedFileInfos.Clear();
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            cleanupExceptions.Add(ex);
        }

        if (detachedFileInfos is not null)
        {
            DisposeUploadFileInfos(detachedFileInfos);
        }

        ThrowCleanupExceptions(cleanupExceptions);
    }

    private static void ThrowCleanupExceptions(IReadOnlyList<Exception> exceptions)
    {
        if (exceptions.Count == 1)
        {
            ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
        }
        if (exceptions.Count > 1)
        {
            throw new AggregateException(exceptions);
        }
    }

    private static async Task ObserveUploadLifecycleTaskAsync(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Upload lifecycle cleanup failed: {ex.Message}");
        }
    }

    private void UpdateTaskRunning()
    {
        var isTaskRunning = EffectiveFiles.Any(file => file.Status == FileUploadStatus.Uploading);
        if (IsTaskRunning != isTaskRunning)
        {
            SetCurrentValue(IsTaskRunningProperty, isTaskRunning);
        }
    }

    private static bool IsTerminalUploadStatus(FileUploadStatus status)
    {
        return status is FileUploadStatus.Success or FileUploadStatus.Failed or FileUploadStatus.Cancelled;
    }

    private void RunOnUiThread(Action action)
    {
        if (Dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.InvokeAsync(action).GetTask().GetAwaiter().GetResult();
        }
    }

    private sealed class DisplayOnlyUploadFileSource : IUploadFileSource
    {
        internal static DisplayOnlyUploadFileSource Instance { get; } = new();

        public ValueTask<Stream> OpenReadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromException<Stream>(
                new NotSupportedException("Display-only upload items do not provide file content."));
        }
    }
}
