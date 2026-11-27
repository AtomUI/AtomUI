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

/// <summary>
/// A file upload control. It collects files through a trigger, for example a picker or a drop area, shows
/// them in a list with their upload progress and uploads them through the
/// <see cref="UploadTransport" />. Every step is reported by an event, and the control provides its files
/// as the value of a form item.
/// </summary>
public partial class Upload : ContentControl,
                              IMotionAwareControl,
                              IFormItemAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Files" /> property.
    /// </summary>
    public static readonly StyledProperty<IList<UploadFileItem>?> FilesProperty =
        AvaloniaProperty.Register<Upload, IList<UploadFileItem>?>(
            nameof(Files),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the <see cref="AllowedFileTypes" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<FilePickerFileType>?> AllowedFileTypesProperty =
        AvaloniaProperty.Register<Upload, IReadOnlyList<FilePickerFileType>?>(nameof(AllowedFileTypes));

    /// <summary>
    /// Identifies the <see cref="CountOverflowBehavior" /> property.
    /// </summary>
    public static readonly StyledProperty<UploadCountOverflowBehavior> CountOverflowBehaviorProperty =
        AvaloniaProperty.Register<Upload, UploadCountOverflowBehavior>(
            nameof(CountOverflowBehavior),
            UploadCountOverflowBehavior.RejectExcess);

    /// <summary>
    /// Identifies the <see cref="AdmissionPolicy" /> property.
    /// </summary>
    public static readonly StyledProperty<IUploadAdmissionPolicy?> AdmissionPolicyProperty =
        AvaloniaProperty.Register<Upload, IUploadAdmissionPolicy?>(nameof(AdmissionPolicy));

    /// <summary>
    /// Identifies the <see cref="ExtraContext" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ExtraContextProperty =
        AvaloniaProperty.Register<Upload, object?>(nameof(ExtraContext));

    /// <summary>
    /// Identifies the <see cref="MaxCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<Upload, int>(nameof(MaxCount), int.MaxValue);

    /// <summary>
    /// Identifies the <see cref="MaxConcurrentTasks" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxConcurrentTasksProperty =
        AvaloniaProperty.Register<Upload, int>(nameof(MaxConcurrentTasks), 3);

    /// <summary>
    /// Identifies the <see cref="AutoUpload" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> AutoUploadProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(AutoUpload), true);

    /// <summary>
    /// Identifies the <see cref="ListType" /> property.
    /// </summary>
    public static readonly StyledProperty<UploadListType> ListTypeProperty =
        AvaloniaProperty.Register<Upload, UploadListType>(nameof(ListType));

    /// <summary>
    /// Identifies the <see cref="IsMultipleEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMultipleEnabledProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsMultipleEnabled));

    /// <summary>
    /// Identifies the <see cref="IsShowUploadList" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowUploadListProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsShowUploadList), true);

    /// <summary>
    /// Identifies the <see cref="ListMaxHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ListMaxHeightProperty =
        AvaloniaProperty.Register<Upload, double>(
            nameof(ListMaxHeight),
            double.PositiveInfinity);

    /// <summary>
    /// Identifies the <see cref="ListScrollBarVisibility" /> property.
    /// </summary>
    public static readonly StyledProperty<ScrollBarVisibility> ListScrollBarVisibilityProperty =
        AvaloniaProperty.Register<Upload, ScrollBarVisibility>(
            nameof(ListScrollBarVisibility),
            ScrollBarVisibility.Disabled);

    /// <summary>
    /// Identifies the <see cref="SuccessAutoRemoveDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan?> SuccessAutoRemoveDelayProperty =
        AvaloniaProperty.Register<Upload, TimeSpan?>(nameof(SuccessAutoRemoveDelay));

    /// <summary>
    /// Identifies the <see cref="PendingText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> PendingTextProperty =
        AvaloniaProperty.Register<Upload, string?>(nameof(PendingText));

    /// <summary>
    /// Identifies the <see cref="FileValueMode" /> property.
    /// </summary>
    public static readonly StyledProperty<UploadFileValueMode> FileValueModeProperty =
        AvaloniaProperty.Register<Upload, UploadFileValueMode>(
            nameof(FileValueMode),
            UploadFileValueMode.SuccessfulFiles);

    /// <summary>
    /// Identifies the <see cref="TriggerContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> TriggerContentProperty =
        AvaloniaProperty.Register<Upload, object?>(nameof(TriggerContent));

    /// <summary>
    /// Identifies the <see cref="TriggerContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TriggerContentTemplateProperty =
        AvaloniaProperty.Register<Upload, IDataTemplate?>(nameof(TriggerContentTemplate));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Upload>();

    /// <summary>
    /// Identifies the <see cref="UploadTransport" /> property.
    /// </summary>
    public static readonly DirectProperty<Upload, IFileUploadTransport?> UploadTransportProperty =
        AvaloniaProperty.RegisterDirect<Upload, IFileUploadTransport?>(
            nameof(UploadTransport),
            o => o.UploadTransport,
            (o, v) => o.UploadTransport = v);

    /// <summary>
    /// Identifies the <see cref="IsTaskRunning" /> property.
    /// </summary>
    public static readonly DirectProperty<Upload, bool> IsTaskRunningProperty =
        AvaloniaProperty.RegisterDirect<Upload, bool>(
            nameof(IsTaskRunning),
            o => o.IsTaskRunning,
            (o, v) => o.IsTaskRunning = v);

    /// <summary>
    /// Gets or sets the list of the files that the control shows and uploads. The control adds an item for
    /// each accepted file and removes it when the file is removed. A list that implements
    /// <see cref="INotifyCollectionChanged" /> is kept in sync with the changes the app makes to it; a plain
    /// list is read only when it is assigned. Assigning another list cancels the uploads of the files that are
    /// then no longer listed. <see langword="null" /> switches back to the control's own list.
    /// </summary>
    public IList<UploadFileItem>? Files
    {
        get => GetValue(FilesProperty);
        set => SetValue(FilesProperty, value);
    }

    /// <summary>
    /// Gets or sets the file types an incoming file may have. The storage picker offers them as its filter,
    /// and a file from any source that matches none of them is rejected when it is added.
    /// <see langword="null" /> sets no restriction.
    /// </summary>
    public IReadOnlyList<FilePickerFileType>? AllowedFileTypes
    {
        get => GetValue(AllowedFileTypesProperty);
        set => SetValue(AllowedFileTypesProperty, value);
    }

    /// <summary>
    /// Gets or sets how a batch of incoming files is treated when it has more files than the list has room
    /// for under <see cref="MaxCount" />. <see cref="UploadCountOverflowBehavior.RejectExcess" />, the
    /// default, accepts the files that fit and rejects the rest;
    /// <see cref="UploadCountOverflowBehavior.RejectBatch" /> rejects the whole batch;
    /// <see cref="UploadCountOverflowBehavior.ReplaceExisting" /> replaces the files in the list with the new
    /// ones.
    /// </summary>
    public UploadCountOverflowBehavior CountOverflowBehavior
    {
        get => GetValue(CountOverflowBehaviorProperty);
        set => SetValue(CountOverflowBehaviorProperty, value);
    }

    /// <summary>
    /// Gets or sets a policy that evaluates every incoming file before it is added to the list. It is called
    /// once per file with the file and the batch and source the file came with.
    /// <see langword="null" /> runs no custom policy.
    /// </summary>
    public IUploadAdmissionPolicy? AdmissionPolicy
    {
        get => GetValue(AdmissionPolicyProperty);
        set => SetValue(AdmissionPolicyProperty, value);
    }

    /// <summary>
    /// Gets or sets a value the control copies to <see cref="UploadFileItem.UserData" /> of each item it
    /// creates for an accepted file. It is read when the item is created, so a later change does not reach
    /// existing items.
    /// </summary>
    public object? ExtraContext
    {
        get => GetValue(ExtraContextProperty);
        set => SetValue(ExtraContextProperty, value);
    }

    /// <summary>
    /// Gets or sets the largest number of files the list holds. A batch with more files than there is room
    /// for is cut down or rejected as <see cref="CountOverflowBehavior" /> says. The default is
    /// <see cref="int.MaxValue" />, which sets no limit; a value of 0 or less accepts no file.
    /// </summary>
    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }

    /// <summary>
    /// Gets or sets how many uploads run at the same time; the other queued files wait for a free slot. The
    /// default is 3. A change is passed to the upload queue.
    /// </summary>
    public int MaxConcurrentTasks
    {
        get => GetValue(MaxConcurrentTasksProperty);
        set => SetValue(MaxConcurrentTasksProperty, value);
    }

    /// <summary>
    /// Gets or sets whether an accepted file is queued for upload as soon as it was added to the list. While
    /// it is <see langword="false" /> the control does not upload accepted files. The value is read for each
    /// file after <see cref="UploadTaskAboutToScheduling" /> was raised, so files that were added while it was
    /// <see langword="false" /> are not uploaded when it is turned on later.
    /// </summary>
    public bool AutoUpload
    {
        get => GetValue(AutoUploadProperty);
        set => SetValue(AutoUploadProperty, value);
    }

    /// <summary>
    /// Gets or sets how the file list and its items are presented.
    /// <see cref="UploadListType.Text" /> and <see cref="UploadListType.Picture" /> show the trigger content
    /// and a list of rows; <see cref="UploadListType.PictureCard" /> and
    /// <see cref="UploadListType.PictureCircle" /> show picture tiles, with the trigger content as the last
    /// tile.
    /// </summary>
    public UploadListType ListType
    {
        get => GetValue(ListTypeProperty);
        set => SetValue(ListTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether several files can be taken at once. When it is <see langword="true" />, the
    /// storage picker lets the user select several files or folders and a batch of several files is accepted;
    /// otherwise such a batch is rejected.
    /// </summary>
    public bool IsMultipleEnabled
    {
        get => GetValue(IsMultipleEnabledProperty);
        set => SetValue(IsMultipleEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the file list is shown. For the <c>Text</c> and <c>Picture</c> list types the whole
    /// list is hidden; for <c>PictureCard</c> and <c>PictureCircle</c> the value is bound to the visibility of
    /// the list items. The default is <see langword="true" />.
    /// </summary>
    public bool IsShowUploadList
    {
        get => GetValue(IsShowUploadListProperty);
        set => SetValue(IsShowUploadListProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum height of the file list area, in device-independent pixels. A longer list is
    /// cut off at that height unless <see cref="ListScrollBarVisibility" /> lets it scroll. The default is
    /// <see cref="double.PositiveInfinity" />, which leaves the height unbounded.
    /// </summary>
    public double ListMaxHeight
    {
        get => GetValue(ListMaxHeightProperty);
        set => SetValue(ListMaxHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets when the file list shows its vertical scroll bar. The default is
    /// <see cref="ScrollBarVisibility.Disabled" />: the list never scrolls, so a list that is taller than
    /// <see cref="ListMaxHeight" /> is cut off.
    /// </summary>
    public ScrollBarVisibility ListScrollBarVisibility
    {
        get => GetValue(ListScrollBarVisibilityProperty);
        set => SetValue(ListScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets how long a file stays in the list after its upload succeeded; then the control removes it
    /// through <see cref="RemoveFileAsync" />. It applies to a file at the moment its status becomes
    /// <see cref="FileUploadStatus.Success" />. <see langword="null" />, the default, keeps successful files.
    /// </summary>
    public TimeSpan? SuccessAutoRemoveDelay
    {
        get => GetValue(SuccessAutoRemoveDelayProperty);
        set => SetValue(SuccessAutoRemoveDelayProperty, value);
    }

    /// <summary>
    /// Gets or sets the text that an item shows while its file waits for its upload. The value is copied to
    /// <see cref="UploadFileItem.PendingText" /> when the control creates the item for an accepted file.
    /// </summary>
    public string? PendingText
    {
        get => GetValue(PendingTextProperty);
        set => SetValue(PendingTextProperty, value);
    }

    /// <summary>
    /// Gets or sets what a form reads as the value of the control, see <see cref="NotifyGetFormValue" />.
    /// <see cref="UploadFileValueMode.AllFiles" /> returns every item of the list;
    /// <see cref="UploadFileValueMode.SuccessfulFiles" />, the default, returns the items whose upload
    /// succeeded; <see cref="UploadFileValueMode.Results" /> returns the results of those items.
    /// </summary>
    public UploadFileValueMode FileValueMode
    {
        get => GetValue(FileValueModeProperty);
        set => SetValue(FileValueModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the content of the trigger area, where the user starts adding files. Put an
    /// <c>UploadTrigger</c> here to get the built-in picker button. For the
    /// <see cref="UploadListType.PictureCard" /> and <see cref="UploadListType.PictureCircle" /> list types
    /// it is appended as the last item of the tile list. <see langword="null" /> shows no trigger content.
    /// </summary>
    [DependsOn(nameof(TriggerContentTemplate))]
    public object? TriggerContent
    {
        get => GetValue(TriggerContentProperty);
        set => SetValue(TriggerContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="IDataTemplate" /> used to display <see cref="TriggerContent" />.
    /// </summary>
    public IDataTemplate? TriggerContentTemplate
    {
        get => GetValue(TriggerContentTemplateProperty);
        set => SetValue(TriggerContentTemplateProperty, value);
    }

    private IFileUploadTransport? _uploadTransport;

    /// <summary>
    /// Gets or sets the transport that performs the uploads. The control hands it to its upload queue and
    /// passes a change to the queue. The default is <see langword="null" />, in which case the queue does not
    /// start the upload of a file.
    /// </summary>
    public IFileUploadTransport? UploadTransport
    {
        get => _uploadTransport;
        set => SetAndRaise(UploadTransportProperty, ref _uploadTransport, value);
    }

    private bool _isTaskRunning;

    /// <summary>
    /// Gets or sets whether a file of the list is being uploaded. The control keeps the value in step with
    /// the statuses of the items: it changes when the first upload starts and when the last one ends.
    /// </summary>
    public bool IsTaskRunning
    {
        get => _isTaskRunning;
        set => SetAndRaise(IsTaskRunningProperty, ref _isTaskRunning, value);
    }

    /// <summary>
    /// Gets or sets whether the control and the items of its file list play their motion. The theme forwards
    /// the value to the list and to each item.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised on the UI thread after the control added an item for an accepted file to the list, and before
    /// anything is decided about its upload. It is raised for every accepted file, also while
    /// <see cref="AutoUpload" /> is <see langword="false" />.
    /// </summary>
    public event EventHandler<UploadTaskCreatedEventArgs>? UploadTaskCreated;
    /// <summary>
    /// Raised on the UI thread after <see cref="UploadTaskCreated" />, once for each accepted file and before
    /// the control acts on the file. The handler chooses the outcome through
    /// <see cref="UploadTaskAboutToSchedulingEventArgs.Result" />:
    /// <see cref="UploadPredicateResult.Schedule" /> queues the file for upload, but only while
    /// <see cref="AutoUpload" /> is <see langword="true" />; <see cref="UploadPredicateResult.Cancel" /> and
    /// <see cref="UploadPredicateResult.CancelWithInTaskList" /> keep the file out of the queue and give its
    /// item the status <see cref="FileUploadStatus.Failed" /> with the cancellation reason as the error message.
    /// </summary>
    public event EventHandler<UploadTaskAboutToSchedulingEventArgs>? UploadTaskAboutToScheduling;
    /// <summary>
    /// Raised on the UI thread when the transport reports progress for a file. The item already has the status
    /// <see cref="FileUploadStatus.Uploading" /> and the new progress value. Progress that arrives after the
    /// item reached a final status is dropped without raising the event.
    /// </summary>
    public event EventHandler<UploadTaskProgressEventArgs>? UploadTaskProgress;
    /// <summary>
    /// Raised on the UI thread after the upload of a file succeeded. The item already has the status
    /// <see cref="FileUploadStatus.Success" /> and its result; the auto-removal is scheduled afterwards.
    /// </summary>
    public event EventHandler<UploadTaskCompletedEventArgs>? UploadTaskCompleted;
    /// <summary>
    /// Raised on the UI thread after the upload of a file was canceled. The item already has the status
    /// <see cref="FileUploadStatus.Cancelled" />, the result and its user-friendly message as the error text.
    /// </summary>
    public event EventHandler<UploadTaskCancelledEventArgs>? UploadTaskCancelled;
    /// <summary>
    /// Raised on the UI thread after the upload of a file failed. The item already has the status
    /// <see cref="FileUploadStatus.Failed" />, the failed result, and its user-friendly message as the
    /// error text.
    /// </summary>
    public event EventHandler<UploadTaskFailedEventArgs>? UploadTaskFailed;
    /// <summary>
    /// Raised after an item was taken out of the list by <see cref="RemoveFileAsync" />, which also covers the
    /// auto-removal of successful files and the remove request of an item. It is not raised for items that
    /// leave the list in another way, for example when <see cref="Files" /> is replaced or cleared.
    /// </summary>
    public event EventHandler<UploadTaskRemovedEventArgs>? UploadTaskRemoved;
    /// <summary>
    /// Raised on the UI thread after a batch of incoming files was processed, whatever source it came from.
    /// The arguments report the outcome of the batch, the accepted files and the rejected files.
    /// </summary>
    public event EventHandler<UploadInputBatchCompletedEventArgs>? InputBatchCompleted;

    #endregion

    #region 公共回调函数定义

    /// <summary>
    /// Gets or sets a function that decides whether an accepted file is an image. It replaces the built-in
    /// check of the file name's extension. The result is copied to <see cref="UploadFileItem.IsImageFile" />
    /// when the control creates the item.
    /// </summary>
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

    /// <summary>
    /// Runs files through the same admission, file type and count checks as files the user picked. An accepted
    /// file is added to the list and <see cref="UploadTaskCreated" /> and
    /// <see cref="UploadTaskAboutToScheduling" /> are raised for it. The returned task completes when the batch
    /// has been processed; it does not wait for the uploads.
    /// </summary>
    /// <param name="files">
    /// The files to add. The sequence is read once, before any file is processed.
    /// </param>
    /// <param name="cancellationToken">
    /// Stops the batch while its files are still being checked; files that were already added stay in the list.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="files" /> is <see langword="null" />.
    /// </exception>
    public Task EnqueueFilesAsync(IEnumerable<UploadFileInfo> files, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(files);
        var materializedFiles = files.ToArray();
        return _inputPipeline.ProcessFilesAsync(
            UploadInputSource.Programmatic,
            materializedFiles,
            cancellationToken);
    }

    /// <summary>
    /// Cancels the upload of a file, stops its pending auto-removal and takes its item out of the list. The
    /// task completes when the item is removed and <see cref="UploadTaskRemoved" /> has been raised. Nothing
    /// happens when the list has no item with the given id.
    /// </summary>
    /// <param name="id">
    /// The id of the item to remove, see <see cref="UploadFileItem.Id" />.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancels the wait for the upload's cancellation; the item then stays in the list.
    /// </param>
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

    /// <summary>
    /// Cancels every input batch and every upload that is still running, then empties the file list. The task
    /// completes when the cancellations ended and the list was cleared on the UI thread.
    /// </summary>
    /// <param name="cancellationToken">
    /// Ends the reset as canceled while it is still cancelling; the list is then left as it is.
    /// </param>
    /// <exception cref="OperationCanceledException">
    /// The method was canceled before the list was cleared.
    /// </exception>
    /// <exception cref="AggregateException">
    /// Several cleanup steps failed. When one step failed, its own exception is thrown instead.
    /// </exception>
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

    /// <summary>
    /// Empties the file list by running <see cref="ResetAsync" /> without waiting for it. The method returns at
    /// once; an error of the reset is written to the debug output and not thrown.
    /// </summary>
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

    /// <summary>
    /// Stops the work of the control when it is detached from the visual tree. After the base member ran, the
    /// pending auto-removals are canceled, and every input batch and upload is canceled and every accepted
    /// file source is released in the background. An override must call the base member, or uploads keep
    /// running after the control is detached.
    /// </summary>
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

    /// <summary>
    /// Applies property changes to the state of the control. A new <see cref="Files" /> list is attached and
    /// the uploads of the files that left are canceled; a change of the trigger content, of its template or
    /// of the content alignment refreshes the trigger presenter; a new <see cref="UploadTransport" /> or
    /// <see cref="MaxConcurrentTasks" /> is passed to the upload queue. Call the base member first.
    /// </summary>
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

    /// <summary>
    /// Notifies the form item that holds the control that its value changed. The control calls it after the
    /// file list changed. An override must call the base member, or the form is not notified.
    /// </summary>
    /// <param name="value">
    /// The new list. It is not used: the form reads the value through <see cref="NotifyGetFormValue" />.
    /// </param>
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Replaces the content of the file list with the given items when a form sets the value of the control.
    /// The uploads of the replaced files are canceled and their file sources are released.
    /// </summary>
    /// <param name="value">
    /// The items to show, as an <see cref="IEnumerable{UploadFileItem}" />; any other value, including
    /// <see langword="null" />, empties the list. The items are copied into the current list, which itself is
    /// not replaced.
    /// </param>
    protected virtual void NotifySetFormValue(object? value)
    {
        var replacementItems = value is IEnumerable<UploadFileItem> items
            ? items.ToArray()
            : [];
        ReplaceEffectiveFilesContents(replacementItems);
    }

    /// <summary>
    /// Builds the value that a form reads from the control, selected by <see cref="FileValueMode" />.
    /// </summary>
    /// <returns>
    /// A new array with every item of the list, with the items whose upload succeeded, or with the non-null
    /// results of those items.
    /// </returns>
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

    /// <summary>
    /// Empties the file list when a form clears the value of the control. On the UI thread the list is emptied
    /// at once; from another thread the emptying is dispatched to the UI thread, and the method returns before
    /// it ran.
    /// </summary>
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

    /// <summary>
    /// Receives the validation status that a form item reports for the control. The upload does nothing with
    /// it; an override can show the status.
    /// </summary>
    /// <param name="status">
    /// The status the form item computed for the control.
    /// </param>
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
