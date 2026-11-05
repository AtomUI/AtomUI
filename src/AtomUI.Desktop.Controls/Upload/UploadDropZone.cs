using AtomUI.Theme.Resources;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(
    UploadDropZonePseudoClass.DragOver,
    UploadDropZonePseudoClass.DragAccepting,
    UploadDropZonePseudoClass.DragRejecting,
    UploadDropZonePseudoClass.DropProcessing)]
public sealed class UploadDropZone : ContentControl,
    IThemeInheritanceSource
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsOpenFileDialogOnClickProperty =
        AvaloniaProperty.Register<UploadDropZone, bool>(nameof(IsOpenFileDialogOnClick), true);

    public static readonly StyledProperty<UploadSourceKind> SourceKindProperty =
        AvaloniaProperty.Register<UploadDropZone, UploadSourceKind>(nameof(SourceKind));

    public static readonly StyledProperty<bool> IsFileDropEnabledProperty =
        AvaloniaProperty.Register<UploadDropZone, bool>(nameof(IsFileDropEnabled), true);

    public static readonly StyledProperty<UploadDirectoryDropMode> DirectoryDropModeProperty =
        AvaloniaProperty.Register<UploadDropZone, UploadDirectoryDropMode>(
            nameof(DirectoryDropMode),
            UploadDirectoryDropMode.Reject);

    public static readonly StyledProperty<int> MaxDirectoryDepthProperty =
        AvaloniaProperty.Register<UploadDropZone, int>(
            nameof(MaxDirectoryDepth),
            32,
            validate: value => value >= 0);

    public static readonly StyledProperty<int> MaxEnumeratedItemsProperty =
        AvaloniaProperty.Register<UploadDropZone, int>(
            nameof(MaxEnumeratedItems),
            10_000,
            validate: value => value > 0);

    public static readonly DirectProperty<UploadDropZone, UploadDragState> DragStateProperty =
        AvaloniaProperty.RegisterDirect<UploadDropZone, UploadDragState>(
            nameof(DragState),
            zone => zone.DragState);

    public static readonly DirectProperty<UploadDropZone, bool> IsDropProcessingProperty =
        AvaloniaProperty.RegisterDirect<UploadDropZone, bool>(
            nameof(IsDropProcessing),
            zone => zone.IsDropProcessing);

    public bool IsOpenFileDialogOnClick
    {
        get => GetValue(IsOpenFileDialogOnClickProperty);
        set => SetValue(IsOpenFileDialogOnClickProperty, value);
    }

    public UploadSourceKind SourceKind
    {
        get => GetValue(SourceKindProperty);
        set => SetValue(SourceKindProperty, value);
    }

    public bool IsFileDropEnabled
    {
        get => GetValue(IsFileDropEnabledProperty);
        set => SetValue(IsFileDropEnabledProperty, value);
    }

    public UploadDirectoryDropMode DirectoryDropMode
    {
        get => GetValue(DirectoryDropModeProperty);
        set => SetValue(DirectoryDropModeProperty, value);
    }

    public int MaxDirectoryDepth
    {
        get => GetValue(MaxDirectoryDepthProperty);
        set => SetValue(MaxDirectoryDepthProperty, value);
    }

    public int MaxEnumeratedItems
    {
        get => GetValue(MaxEnumeratedItemsProperty);
        set => SetValue(MaxEnumeratedItemsProperty, value);
    }

    private UploadDragState _dragState;

    public UploadDragState DragState => _dragState;

    private bool _isDropProcessing;

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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SetDragState(UploadDragState.None);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if ((change.Property == IsEnabledProperty && !IsEnabled) ||
            (change.Property == IsFileDropEnabledProperty && !IsFileDropEnabled))
        {
            SetDragState(UploadDragState.None);
        }
    }

    #region 实现 IThemeInheritanceSource 接口

    // AXAML cannot read AvaloniaObject's protected, potentially non-logical inheritance parent.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

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
