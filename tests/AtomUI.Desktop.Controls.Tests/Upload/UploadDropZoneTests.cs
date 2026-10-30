using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Upload;

public class UploadDropZoneTests
{
    public UploadDropZoneTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Public_Contract_Has_The_Designed_Defaults_And_Validation()
    {
        var zone = new UploadDropZone();

        zone.IsOpenFileDialogOnClick.ShouldBeTrue();
        zone.SourceKind.ShouldBe(UploadSourceKind.Files);
        zone.IsFileDropEnabled.ShouldBeTrue();
        zone.DirectoryDropMode.ShouldBe(UploadDirectoryDropMode.Reject);
        zone.MaxDirectoryDepth.ShouldBe(32);
        zone.MaxEnumeratedItems.ShouldBe(10_000);
        zone.DragState.ShouldBe(UploadDragState.None);
        zone.IsDropProcessing.ShouldBeFalse();
        typeof(UploadDropZone).GetProperty(nameof(UploadDropZone.DragState))!.SetMethod.ShouldBeNull();
        typeof(UploadDropZone).GetProperty(nameof(UploadDropZone.IsDropProcessing))!.SetMethod.ShouldBeNull();

        Should.Throw<ArgumentException>(() => zone.MaxDirectoryDepth = -1);
        Should.Throw<ArgumentException>(() => zone.MaxEnumeratedItems = 0);
    }

    [Fact]
    public void Enter_And_Over_Negotiate_Copy_Without_Materializing_Files()
    {
        var transfer = CountingDataTransfer.WithFiles(
            new TestStorageFile("file.txt", "file:///file.txt"));
        var upload = CreateUpload(new UploadDropZone
        {
            Width = 100,
            Height = 100
        }, out var zone);

        ShowInWindow(upload, () =>
        {
            var enter = RaiseDrag(zone, DragDrop.DragEnterEvent, transfer, DragDropEffects.Copy | DragDropEffects.Move);
            enter.Handled.ShouldBeTrue();
            enter.DragEffects.ShouldBe(DragDropEffects.Copy);
            zone.DragState.ShouldBe(UploadDragState.Accepting);
            transfer.ItemsAccessCount.ShouldBe(0);
            transfer.TryGetRawCount.ShouldBe(0);

            var over = RaiseDrag(zone, DragDrop.DragOverEvent, transfer, DragDropEffects.Copy | DragDropEffects.Link);
            over.Handled.ShouldBeTrue();
            over.DragEffects.ShouldBe(DragDropEffects.Copy);
            zone.DragState.ShouldBe(UploadDragState.Accepting);
            transfer.ItemsAccessCount.ShouldBe(0);
            transfer.TryGetRawCount.ShouldBe(0);
        });
    }

    [Theory]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void Enter_Rejects_When_Zone_Cannot_Accept_The_Session(
        bool isEnabled,
        bool isFileDropEnabled,
        bool hasCopyEffect)
    {
        var zone = new UploadDropZone
        {
            IsEnabled = isEnabled,
            IsFileDropEnabled = isFileDropEnabled
        };
        var upload = CreateUpload(zone, out _);
        var transfer = CountingDataTransfer.WithFiles(
            new TestStorageFile("file.txt", "file:///file.txt"));

        ShowInWindow(upload, () =>
        {
            var args = RaiseDrag(
                zone,
                DragDrop.DragEnterEvent,
                transfer,
                hasCopyEffect ? DragDropEffects.Copy : DragDropEffects.Move);

            args.Handled.ShouldBeTrue();
            args.DragEffects.ShouldBe(DragDropEffects.None);
            zone.DragState.ShouldBe(isEnabled && isFileDropEnabled
                ? UploadDragState.Rejecting
                : UploadDragState.None);
            transfer.ItemsAccessCount.ShouldBe(0);
        });
    }

    [Fact]
    public void Enter_Rejects_Non_File_Data_And_A_Zone_Without_An_Upload_Owner()
    {
        var textTransfer = CountingDataTransfer.WithText("text");
        var ownedZone = new UploadDropZone();
        var upload = CreateUpload(ownedZone, out _);

        ShowInWindow(upload, () =>
        {
            var args = RaiseDrag(ownedZone, DragDrop.DragEnterEvent, textTransfer, DragDropEffects.Copy);
            args.DragEffects.ShouldBe(DragDropEffects.None);
            ownedZone.DragState.ShouldBe(UploadDragState.Rejecting);
        });

        var ownerlessZone = new UploadDropZone();
        ownerlessZone.Arrange(new Rect(0, 0, 100, 100));
        var fileTransfer = CountingDataTransfer.WithFiles(
            new TestStorageFile("file.txt", "file:///file.txt"));
        var ownerlessArgs = RaiseDrag(
            ownerlessZone,
            DragDrop.DragEnterEvent,
            fileTransfer,
            DragDropEffects.Copy);
        ownerlessArgs.DragEffects.ShouldBe(DragDropEffects.None);
        ownerlessZone.DragState.ShouldBe(UploadDragState.None);
    }

    [Fact]
    public void Leave_Only_Clears_State_After_The_Pointer_Leaves_The_Zone_Bounds()
    {
        var transfer = CountingDataTransfer.WithFiles(
            new TestStorageFile("file.txt", "file:///file.txt"));
        var upload = CreateUpload(new UploadDropZone
        {
            Width = 100,
            Height = 100
        }, out var zone);

        ShowInWindow(upload, () =>
        {
            RaiseDrag(zone, DragDrop.DragEnterEvent, transfer, DragDropEffects.Copy);
            var inside = RaiseDrag(zone, DragDrop.DragLeaveEvent, transfer, DragDropEffects.Copy, new Point(10, 10));
            inside.Handled.ShouldBeTrue();
            zone.DragState.ShouldBe(UploadDragState.Accepting);

            var outside = RaiseDrag(
                zone,
                DragDrop.DragLeaveEvent,
                transfer,
                DragDropEffects.Copy,
                new Point(zone.Bounds.Width + 1, zone.Bounds.Height + 1));
            outside.Handled.ShouldBeTrue();
            zone.DragState.ShouldBe(UploadDragState.None);
        });
    }

    [Fact]
    public void Nearest_Nested_DropZone_Owns_The_Routed_Drag_Session()
    {
        var inner = new UploadDropZone();
        var outer = new UploadDropZone { Content = inner };
        var upload = CreateUpload(outer, out _);
        var transfer = CountingDataTransfer.WithFiles(
            new TestStorageFile("file.txt", "file:///file.txt"));

        ShowInWindow(upload, () =>
        {
            var args = RaiseDrag(inner, DragDrop.DragEnterEvent, transfer, DragDropEffects.Copy);

            args.Handled.ShouldBeTrue();
            inner.DragState.ShouldBe(UploadDragState.Accepting);
            outer.DragState.ShouldBe(UploadDragState.None);
        });
    }

    [Fact]
    public void Nearest_Enabled_Ancestor_Owns_When_The_Inner_DropZone_Is_Unavailable()
    {
        var inner = new UploadDropZone { IsFileDropEnabled = false };
        var outer = new UploadDropZone { Content = inner };
        var upload = CreateUpload(outer, out _);
        var transfer = CountingDataTransfer.WithFiles(
            new TestStorageFile("file.txt", "file:///file.txt"));

        ShowInWindow(upload, () =>
        {
            var args = RaiseDrag(inner, DragDrop.DragEnterEvent, transfer, DragDropEffects.Copy);

            args.Handled.ShouldBeTrue();
            args.DragEffects.ShouldBe(DragDropEffects.Copy);
            inner.DragState.ShouldBe(UploadDragState.None);
            outer.DragState.ShouldBe(UploadDragState.Accepting);
        });
    }

    [Fact]
    public void Drop_Materializes_Files_Once_And_Tracks_Processing_Until_The_Batch_Completes()
    {
        var policyGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var upload = CreateUpload(new UploadDropZone(), out var zone);
        upload.AdmissionPolicy = new BlockingPolicy(policyGate.Task);
        var storageFile = new TestStorageFile("file.txt", "file:///file.txt");
        var transfer = CountingDataTransfer.WithFiles(storageFile);

        ShowInWindow(upload, () =>
        {
            var args = RaiseDrag(zone, DragDrop.DropEvent, transfer, DragDropEffects.Copy);

            args.Handled.ShouldBeTrue();
            args.DragEffects.ShouldBe(DragDropEffects.Copy);
            zone.DragState.ShouldBe(UploadDragState.None);
            zone.IsDropProcessing.ShouldBeTrue();
            transfer.ItemsAccessCount.ShouldBe(1);
            transfer.TryGetRawCount.ShouldBe(1);

            policyGate.SetResult();
            RunDispatcherJobsUntil(() => !zone.IsDropProcessing);
            upload.Files!.Select(item => item.Name).ShouldBe(["file.txt"]);
        });
    }

    [Fact]
    public void Drop_Snapshot_Failure_Raises_One_Failed_Batch_And_Is_Observed()
    {
        var upload = CreateUpload(new UploadDropZone(), out var zone);
        UploadInputBatchCompletedEventArgs? completed = null;
        var eventCount = 0;
        upload.InputBatchCompleted += (_, args) =>
        {
            eventCount++;
            completed = args;
        };

        ShowInWindow(upload, () =>
        {
            var args = RaiseDrag(
                zone,
                DragDrop.DropEvent,
                new ThrowingFileDataTransfer(new IOException("snapshot failed")),
                DragDropEffects.Copy);

            args.Handled.ShouldBeTrue();
            args.DragEffects.ShouldBe(DragDropEffects.None);
            zone.DragState.ShouldBe(UploadDragState.None);
            RunDispatcherJobsUntil(() => eventCount == 1 && !zone.IsDropProcessing);

            completed.ShouldNotBeNull();
            completed.Source.ShouldBe(UploadInputSource.DragDrop);
            completed.Status.ShouldBe(UploadInputBatchStatus.Failed);
            completed.FailureReason.ShouldBe(UploadInputFailureReason.DataSnapshotFailed);
            completed.AcceptedFiles.ShouldBeEmpty();
            completed.RejectedItems.ShouldBeEmpty();
        });
    }

    [Theory]
    [InlineData(UploadSourceKind.Files)]
    [InlineData(UploadSourceKind.Directories)]
    public void Primary_Click_Uses_The_DropZone_SourceKind(UploadSourceKind sourceKind)
    {
        var adapter = new RecordingStorageProviderAdapter();
        var zone = new UploadDropZone { SourceKind = sourceKind };
        var upload = CreateUpload(zone, out _);
        upload.StorageProviderAdapter = adapter;

        ShowInWindow(upload, () =>
        {
            var args = RaisePointerReleased(zone, MouseButton.Left);
            RunDispatcherJobsUntil(() => adapter.TotalOpenCount == 1);

            args.Handled.ShouldBeTrue();
            adapter.FileOpenCount.ShouldBe(sourceKind == UploadSourceKind.Files ? 1 : 0);
            adapter.FolderOpenCount.ShouldBe(sourceKind == UploadSourceKind.Directories ? 1 : 0);
        });
    }

    [Fact]
    public void Click_Does_Nothing_When_Disabled_Not_Primary_Or_Opted_Out()
    {
        var adapter = new RecordingStorageProviderAdapter();
        var zone = new UploadDropZone { IsOpenFileDialogOnClick = false };
        var upload = CreateUpload(zone, out _);
        upload.StorageProviderAdapter = adapter;

        ShowInWindow(upload, () =>
        {
            RaisePointerReleased(zone, MouseButton.Left).Handled.ShouldBeFalse();
            zone.IsOpenFileDialogOnClick = true;
            RaisePointerReleased(zone, MouseButton.Right).Handled.ShouldBeFalse();
            zone.IsEnabled = false;
            RaisePointerReleased(zone, MouseButton.Left).Handled.ShouldBeFalse();
            adapter.TotalOpenCount.ShouldBe(0);
        });
    }

    private static Desktop.Controls.Upload CreateUpload(UploadDropZone content, out UploadDropZone zone)
    {
        zone = content;
        return new Desktop.Controls.Upload
        {
            AutoUpload = false,
            Files = new ObservableCollection<UploadFileItem>(),
            TriggerContent = content
        };
    }

    private static DragEventArgs RaiseDrag(
        Control target,
        RoutedEvent<DragEventArgs> routedEvent,
        IDataTransfer dataTransfer,
        DragDropEffects effects,
        Point? position = null)
    {
        var args = new DragEventArgs(
            routedEvent,
            dataTransfer,
            target,
            position ?? new Point(1, 1),
            KeyModifiers.None)
        {
            Source = target,
            DragEffects = effects
        };
        target.RaiseEvent(args);
        return args;
    }

    private static PointerReleasedEventArgs RaisePointerReleased(Control source, MouseButton button)
    {
        var args = new PointerReleasedEventArgs(
            source,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            source,
            default,
            0,
            new PointerPointProperties(
                RawInputModifiers.None,
                button == MouseButton.Left ? PointerUpdateKind.LeftButtonReleased : PointerUpdateKind.RightButtonReleased),
            KeyModifiers.None,
            button);
        source.RaiseEvent(args);
        return args;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new Avalonia.Controls.Window
        {
            Width = 420,
            Height = 320,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static void RunDispatcherJobsUntil(Func<bool> condition, int maxPasses = 256)
    {
        for (var i = 0; i < maxPasses; i++)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }
            Thread.Sleep(1);
        }

        condition().ShouldBeTrue("The asynchronous Drop batch should have completed.");
    }

    private sealed class BlockingPolicy(Task gate) : IUploadAdmissionPolicy
    {
        public async ValueTask<UploadAdmissionDecision> EvaluateAsync(
            UploadAdmissionContext context,
            CancellationToken cancellationToken = default)
        {
            await gate.WaitAsync(cancellationToken);
            return UploadAdmissionDecision.Accept();
        }
    }

    private sealed class CountingDataTransfer : IDataTransfer
    {
        private readonly IReadOnlyList<IDataTransferItem> _items;

        public IReadOnlyList<DataFormat> Formats { get; }
        public IReadOnlyList<IDataTransferItem> Items
        {
            get
            {
                ItemsAccessCount++;
                return _items;
            }
        }

        internal int ItemsAccessCount { get; private set; }
        internal int TryGetRawCount => _items.OfType<CountingDataTransferItem>().Sum(item => item.TryGetRawCount);

        private CountingDataTransfer(IReadOnlyList<DataFormat> formats, IReadOnlyList<IDataTransferItem> items)
        {
            Formats = formats;
            _items = items;
        }

        internal static CountingDataTransfer WithFiles(params TestStorageItem[] items)
        {
            return new CountingDataTransfer(
                [DataFormat.File],
                items.Select(item => (IDataTransferItem)new CountingDataTransferItem(DataFormat.File, item)).ToArray());
        }

        internal static CountingDataTransfer WithText(string text)
        {
            return new CountingDataTransfer(
                [DataFormat.Text],
                [new CountingDataTransferItem(DataFormat.Text, text)]);
        }

        public void Dispose()
        {
        }
    }

    private sealed class CountingDataTransferItem(DataFormat format, object value) : IDataTransferItem
    {
        public IReadOnlyList<DataFormat> Formats { get; } = [format];
        internal int TryGetRawCount { get; private set; }

        public object? TryGetRaw(DataFormat requestedFormat)
        {
            TryGetRawCount++;
            return requestedFormat == format ? value : null;
        }
    }

    private sealed class ThrowingFileDataTransfer(Exception exception) : IDataTransfer
    {
        public IReadOnlyList<DataFormat> Formats { get; } = [DataFormat.File];
        public IReadOnlyList<IDataTransferItem> Items { get; } =
            [new ThrowingFileDataTransferItem(exception)];

        public void Dispose()
        {
        }
    }

    private sealed class ThrowingFileDataTransferItem(Exception exception) : IDataTransferItem
    {
        public IReadOnlyList<DataFormat> Formats { get; } = [DataFormat.File];

        public object? TryGetRaw(DataFormat requestedFormat)
        {
            throw exception;
        }
    }

    private sealed class RecordingStorageProviderAdapter : IUploadStorageProviderAdapter
    {
        public bool CanOpenFiles => true;
        public bool CanOpenFolders => true;
        internal int FileOpenCount { get; private set; }
        internal int FolderOpenCount { get; private set; }
        internal int TotalOpenCount => FileOpenCount + FolderOpenCount;

        public Task<IReadOnlyList<IStorageFile>> OpenFilesAsync(
            FilePickerOpenOptions options,
            CancellationToken cancellationToken)
        {
            FileOpenCount++;
            return Task.FromResult<IReadOnlyList<IStorageFile>>([]);
        }

        public Task<IReadOnlyList<IStorageFolder>> OpenFoldersAsync(
            FolderPickerOpenOptions options,
            CancellationToken cancellationToken)
        {
            FolderOpenCount++;
            return Task.FromResult<IReadOnlyList<IStorageFolder>>([]);
        }
    }
}
