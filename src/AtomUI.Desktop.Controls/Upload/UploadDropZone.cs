using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A drag-and-drop target for the <see cref="Upload" /> above it in the visual tree: dropped files and folders are
/// handed to that Upload for processing, and a click opens its picker unless
/// <see cref="IsOpenFileDialogOnClick" /> is <c>false</c>. Its constructor makes the zone a drop target, and it
/// reports a drag through <see cref="DragState" /> and the <see cref="UploadDropZonePseudoClass" /> pseudo-classes.
/// </summary>
[PseudoClasses(
    UploadDropZonePseudoClass.DragOver,
    UploadDropZonePseudoClass.DragAccepting,
    UploadDropZonePseudoClass.DragRejecting,
    UploadDropZonePseudoClass.DropProcessing)]
// Token-owner transparent part: the drop zone wraps the Upload's drag content and has no Tokens of its own.
[TokenOwnerTransparent]
public sealed class UploadDropZone : ContentControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsOpenFileDialogOnClick" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenFileDialogOnClickProperty =
        AvaloniaProperty.Register<UploadDropZone, bool>(nameof(IsOpenFileDialogOnClick), true);

    /// <summary>
    /// Identifies the <see cref="SourceKind" /> property.
    /// </summary>
    public static readonly StyledProperty<UploadSourceKind> SourceKindProperty =
        AvaloniaProperty.Register<UploadDropZone, UploadSourceKind>(nameof(SourceKind));

    /// <summary>
    /// Identifies the <see cref="IsFileDropEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFileDropEnabledProperty =
        AvaloniaProperty.Register<UploadDropZone, bool>(nameof(IsFileDropEnabled), true);

    /// <summary>
    /// Identifies the <see cref="DirectoryDropMode" /> property.
    /// </summary>
    public static readonly StyledProperty<UploadDirectoryDropMode> DirectoryDropModeProperty =
        AvaloniaProperty.Register<UploadDropZone, UploadDirectoryDropMode>(
            nameof(DirectoryDropMode),
            UploadDirectoryDropMode.Reject);

    /// <summary>
    /// Identifies the <see cref="MaxDirectoryDepth" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxDirectoryDepthProperty =
        AvaloniaProperty.Register<UploadDropZone, int>(
            nameof(MaxDirectoryDepth),
            32,
            validate: value => value >= 0);

    /// <summary>
    /// Identifies the <see cref="MaxEnumeratedItems" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxEnumeratedItemsProperty =
        AvaloniaProperty.Register<UploadDropZone, int>(
            nameof(MaxEnumeratedItems),
            10_000,
            validate: value => value > 0);

    /// <summary>
    /// Identifies the <see cref="DragState" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadDropZone, UploadDragState> DragStateProperty =
        AvaloniaProperty.RegisterDirect<UploadDropZone, UploadDragState>(
            nameof(DragState),
            zone => zone.DragState);

    /// <summary>
    /// Identifies the <see cref="IsDropProcessing" /> property.
    /// </summary>
    public static readonly DirectProperty<UploadDropZone, bool> IsDropProcessingProperty =
        AvaloniaProperty.RegisterDirect<UploadDropZone, bool>(
            nameof(IsDropProcessing),
            zone => zone.IsDropProcessing);

    /// <summary>
    /// When <c>true</c>, the default, releasing the left button of the primary pointer over the zone opens the picker
    /// of the owning <see cref="Upload" /> and handles the event. When <c>false</c>, the zone ignores clicks.
    /// </summary>
    public bool IsOpenFileDialogOnClick
    {
        get => GetValue(IsOpenFileDialogOnClickProperty);
        set => SetValue(IsOpenFileDialogOnClickProperty, value);
    }

    /// <summary>
    /// Picker that a click opens on the owning <see cref="Upload" />: <see cref="UploadSourceKind.Files" /> (the
    /// default) adds the picked files; <see cref="UploadSourceKind.Directories" /> adds the files directly inside the
    /// picked folders. Dropped items are not affected.
    /// </summary>
    public UploadSourceKind SourceKind
    {
        get => GetValue(SourceKindProperty);
        set => SetValue(SourceKindProperty, value);
    }

    /// <summary>
    /// Enables accepting dragged files and folders; the default is <c>true</c>. While it is <c>false</c> or the zone is
    /// disabled, a drag over the zone is refused and <see cref="DragState" /> stays
    /// <see cref="UploadDragState.None" />. A click still opens the picker.
    /// </summary>
    public bool IsFileDropEnabled
    {
        get => GetValue(IsFileDropEnabledProperty);
        set => SetValue(IsFileDropEnabledProperty, value);
    }

    /// <summary>
    /// What the owning <see cref="Upload" /> takes from a dropped folder: nothing with
    /// <see cref="UploadDirectoryDropMode.Reject" /> (the default), only the files directly inside it with
    /// <see cref="UploadDirectoryDropMode.TopLevelFiles" />, or the files of its whole tree with
    /// <see cref="UploadDirectoryDropMode.RecursiveFiles" />, as far as <see cref="MaxDirectoryDepth" /> allows.
    /// A folder that is not taken is reported as a rejected item. Dropped files are not affected.
    /// </summary>
    public UploadDirectoryDropMode DirectoryDropMode
    {
        get => GetValue(DirectoryDropModeProperty);
        set => SetValue(DirectoryDropModeProperty, value);
    }

    /// <summary>
    /// Number of folder levels expanded below a dropped folder; the default is 32 and values below 0 are rejected by
    /// validation. It applies with <see cref="UploadDirectoryDropMode.RecursiveFiles" /> only. With 0, only the files
    /// directly inside the dropped folder are taken; a folder that lies deeper is reported as a rejected item.
    /// </summary>
    public int MaxDirectoryDepth
    {
        get => GetValue(MaxDirectoryDepthProperty);
        set => SetValue(MaxDirectoryDepthProperty, value);
    }

    /// <summary>
    /// Limit on the items enumerated below dropped folders; the default is 10000 and values below 1 are rejected by
    /// validation. Each item enumerated past the limit is reported as a rejected item instead of being taken.
    /// </summary>
    public int MaxEnumeratedItems
    {
        get => GetValue(MaxEnumeratedItemsProperty);
        set => SetValue(MaxEnumeratedItemsProperty, value);
    }

    private UploadDragState _dragState;

    /// <summary>
    /// State of the drag over the zone: <see cref="UploadDragState.Accepting" /> or
    /// <see cref="UploadDragState.Rejecting" /> while a drag is over the zone and the dragged data can, or cannot, be
    /// taken. It is <see cref="UploadDragState.None" /> when no drag is over it, when a drop was made, and while the
    /// zone is disabled, file drop is off or no <see cref="Upload" /> is above it.
    /// Each value sets the matching pseudo-class of <see cref="UploadDropZonePseudoClass" />, for styles to show the
    /// drag feedback.
    /// </summary>
    public UploadDragState DragState => _dragState;

    private bool _isDropProcessing;

    /// <summary>
    /// Whether an operation started by a drop on this zone is still running. It becomes <c>true</c> as soon as the
    /// drop is passed to the owning <see cref="Upload" />, and <c>false</c> on the UI thread when the last operation
    /// it started ends, whether it succeeded, failed or was canceled.
    /// </summary>
    public bool IsDropProcessing => _isDropProcessing;

    #endregion

    private int _activeDropOperationCount;

    static UploadDropZone()
    {
        DragDrop.DragEnterEvent.AddClassHandler<UploadDropZone>((zone, args) => zone.HandleDragEnterOrOver(args));
        DragDrop.DragOverEvent.AddClassHandler<UploadDropZone>((zone, args) => zone.HandleDragEnterOrOver(args));
        DragDrop.DragLeaveEvent.AddClassHandler<UploadDropZone>((zone, args) => zone.HandleDragLeave(args));
        DragDrop.DropEvent.AddClassHandler<UploadDropZone>((zone, args) => zone.HandleDrop(args));
    }

    public UploadDropZone()
    {
        DragDrop.SetAllowDrop(this, true);
        AddHandler(PointerReleasedEvent, HandlePointerReleased, RoutingStrategies.Bubble);
    }

    /// <summary>
    /// Sets <see cref="DragState" /> to <see cref="UploadDragState.None" /> before the base member runs. An override
    /// must call the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SetDragState(UploadDragState.None);
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Calls the base member, then sets <see cref="DragState" /> to <see cref="UploadDragState.None" /> when the zone
    /// or its file drop has just been disabled.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if ((change.Property == IsEnabledProperty && !IsEnabled) ||
            (change.Property == IsFileDropEnabledProperty && !IsFileDropEnabled))
        {
            SetDragState(UploadDragState.None);
        }
    }

    private void HandleDragEnterOrOver(DragEventArgs e)
    {
        if (!TryGetSessionOwner(out _))
        {
            HandleUnavailableSession(e);
            return;
        }

        var isAccepted = CanAcceptData(e);
        e.DragEffects = isAccepted ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
        SetDragState(isAccepted ? UploadDragState.Accepting : UploadDragState.Rejecting);
    }

    private void HandleDragLeave(DragEventArgs e)
    {
        if (!TryGetSessionOwner(out _))
        {
            HandleUnavailableSession(e);
            return;
        }

        e.Handled = true;
        if (!new Rect(Bounds.Size).Contains(e.GetPosition(this)))
        {
            SetDragState(UploadDragState.None);
        }
    }

    private void HandleDrop(DragEventArgs e)
    {
        if (!TryGetSessionOwner(out var owner))
        {
            HandleUnavailableSession(e);
            return;
        }

        var isAccepted = CanAcceptData(e);
        IStorageItem[]? storageItems = null;
        if (isAccepted)
        {
            try
            {
                storageItems = e.DataTransfer.TryGetFiles();
                isAccepted = storageItems is { Length: > 0 };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Upload Drop data snapshot failed: {ex.Message}");
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                SetDragState(UploadDragState.None);
                StartDropOperation(owner.ProcessInputFailureAsync(
                    UploadInputSource.DragDrop,
                    UploadInputFailureReason.DataSnapshotFailed,
                    ex));
                return;
            }
        }

        e.DragEffects = isAccepted ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
        SetDragState(UploadDragState.None);

        if (!isAccepted || storageItems is null)
        {
            return;
        }

        StartDropOperation(owner.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            storageItems,
            DirectoryDropMode,
            MaxDirectoryDepth,
            MaxEnumeratedItems));
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Handled ||
            !IsEnabled ||
            !IsOpenFileDialogOnClick ||
            !e.Pointer.IsPrimary ||
            e.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }

        var owner = this.FindAncestorOfType<Upload>();
        if (owner is null)
        {
            return;
        }

        e.Handled = true;
        var task = SourceKind == UploadSourceKind.Directories
            ? owner.SelectDirectoriesAsync()
            : owner.SelectFilesAsync();
        _ = ObserveSelectionOperationAsync(task);
    }

    private bool TryGetSessionOwner([NotNullWhen(true)] out Upload? owner)
    {
        owner = this.FindAncestorOfType<Upload>();
        if (!IsEnabled || !IsFileDropEnabled || owner is null)
        {
            return false;
        }

        return true;
    }

    private static bool CanAcceptData(DragEventArgs e)
    {
        return (e.DragEffects & DragDropEffects.Copy) != 0 &&
               e.DataTransfer.Contains(DataFormat.File);
    }

    private void HandleUnavailableSession(DragEventArgs e)
    {
        SetDragState(UploadDragState.None);
        if (this.FindAncestorOfType<UploadDropZone>() is not null)
        {
            return;
        }

        e.DragEffects = DragDropEffects.None;
        e.Handled = true;
    }

    private void StartDropOperation(Task operation)
    {
        _activeDropOperationCount++;
        UpdateDropProcessingState();
        _ = ObserveDropOperationAsync(operation);
    }

    private async Task ObserveDropOperationAsync(Task operation)
    {
        try
        {
            await operation.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Upload Drop processing failed: {ex.Message}");
        }
        finally
        {
            await Dispatcher.InvokeAsync(() =>
            {
                _activeDropOperationCount = Math.Max(0, _activeDropOperationCount - 1);
                UpdateDropProcessingState();
            });
        }
    }

    private static async Task ObserveSelectionOperationAsync(Task operation)
    {
        try
        {
            await operation.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Upload picker processing failed: {ex.Message}");
        }
    }

    private void SetDragState(UploadDragState value)
    {
        if (_dragState == value)
        {
            return;
        }

        SetAndRaise(DragStateProperty, ref _dragState, value);
        PseudoClasses.Set(UploadDropZonePseudoClass.DragOver, value != UploadDragState.None);
        PseudoClasses.Set(UploadDropZonePseudoClass.DragAccepting, value == UploadDragState.Accepting);
        PseudoClasses.Set(UploadDropZonePseudoClass.DragRejecting, value == UploadDragState.Rejecting);
    }

    private void UpdateDropProcessingState()
    {
        var value = _activeDropOperationCount > 0;
        if (_isDropProcessing != value)
        {
            SetAndRaise(IsDropProcessingProperty, ref _isDropProcessing, value);
        }
        PseudoClasses.Set(UploadDropZonePseudoClass.DropProcessing, value);
    }
}
