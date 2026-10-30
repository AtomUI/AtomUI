using System.Collections.ObjectModel;
using System.Diagnostics;
using AtomUI.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Upload;

public class UploadInputPipelineTests
{
    public UploadInputPipelineTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public async Task Programmatic_Batch_Preserves_Order_And_Raises_One_Completed_Event()
    {
        var upload = CreateUpload();
        var events = new List<UploadInputBatchCompletedEventArgs>();
        upload.InputBatchCompleted += (_, args) => events.Add(args);

        await upload.EnqueueFilesAsync(
            [CreateFile("first.txt"), CreateFile("second.txt")],
            TestContext.Current.CancellationToken);

        upload.Files!.Select(item => item.Name).ShouldBe(["first.txt", "second.txt"]);
        events.Count.ShouldBe(1);
        events[0].Source.ShouldBe(UploadInputSource.Programmatic);
        events[0].AcceptedFiles.Select(file => file.Name).ShouldBe(["first.txt", "second.txt"]);
        events[0].RejectedItems.ShouldBeEmpty();
        events[0].Status.ShouldBe(UploadInputBatchStatus.Completed);
        events[0].FailureReason.ShouldBeNull();
    }

    [Fact]
    public async Task Admission_Runs_Before_Count_And_Rejects_Without_Creating_File_Items()
    {
        var policy = new RecordingAdmissionPolicy(file => file.Name != "blocked.txt");
        var upload = CreateUpload();
        upload.MaxCount = 1;
        upload.AdmissionPolicy = policy;
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;

        await upload.EnqueueFilesAsync(
            [CreateFile("blocked.txt"), CreateFile("accepted.txt")],
            TestContext.Current.CancellationToken);

        policy.Names.ShouldBe(["blocked.txt", "accepted.txt"]);
        upload.Files!.Select(item => item.Name).ShouldBe(["accepted.txt"]);
        completed.ShouldNotBeNull();
        completed.RejectedItems.Count.ShouldBe(1);
        completed.RejectedItems[0].Reason.ShouldBe(UploadRejectionReason.AdmissionRejected);
    }

    [Fact]
    public async Task RejectExcess_Accepts_Remaining_Capacity_And_Rejects_The_Rest()
    {
        var upload = CreateUpload();
        upload.MaxCount = 2;
        upload.CountOverflowBehavior = UploadCountOverflowBehavior.RejectExcess;
        await upload.EnqueueFilesAsync([CreateFile("existing.txt")], TestContext.Current.CancellationToken);
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;

        await upload.EnqueueFilesAsync(
            [CreateFile("accepted.txt"), CreateFile("excess.txt")],
            TestContext.Current.CancellationToken);

        upload.Files!.Select(item => item.Name).ShouldBe(["existing.txt", "accepted.txt"]);
        completed.ShouldNotBeNull();
        completed.AcceptedFiles.Select(file => file.Name).ShouldBe(["accepted.txt"]);
        completed.RejectedItems.Single().Reason.ShouldBe(UploadRejectionReason.CountLimitExceeded);
    }

    [Fact]
    public async Task RejectBatch_Is_Atomic_After_All_Files_Pass_Admission()
    {
        var upload = CreateUpload();
        upload.MaxCount = 2;
        upload.CountOverflowBehavior = UploadCountOverflowBehavior.RejectBatch;
        await upload.EnqueueFilesAsync([CreateFile("existing.txt")], TestContext.Current.CancellationToken);
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;

        await upload.EnqueueFilesAsync(
            [CreateFile("first.txt"), CreateFile("second.txt")],
            TestContext.Current.CancellationToken);

        upload.Files!.Select(item => item.Name).ShouldBe(["existing.txt"]);
        completed.ShouldNotBeNull();
        completed.AcceptedFiles.ShouldBeEmpty();
        completed.RejectedItems.Count.ShouldBe(2);
        completed.RejectedItems.ShouldAllBe(item => item.Reason == UploadRejectionReason.CountLimitExceeded);
    }

    [Fact]
    public void ReplaceExisting_Commits_New_Files_Before_Old_Upload_Cancellation_Completes()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var transport = new DelayedCancellationTransport();
            var upload = new Desktop.Controls.Upload
            {
                AutoUpload = true,
                UploadTransport = transport,
                CountOverflowBehavior = UploadCountOverflowBehavior.ReplaceExisting,
                MaxCount = 1,
                Files = new ObservableCollection<UploadFileItem>()
            };
            Dispatcher.UIThread.RunJobs();
            var oldSource = new TestStorageFile("old.txt", "file:///old.txt");

            WaitWithDispatcherPump(AddStorageFileAsync(upload, oldSource));
            WaitWithDispatcherPump(transport.Started.Task);

            var replacement = upload.EnqueueFilesAsync(
                [CreateFile("new.txt")],
                TestContext.Current.CancellationToken);
            try
            {
                WaitWithDispatcherPump(replacement);
                upload.Files!.Select(item => item.Name).ShouldBe(["new.txt"]);
                oldSource.DisposeCount.ShouldBe(0);
            }
            finally
            {
                transport.Release();
                WaitWithDispatcherPump(replacement);
                WaitWithDispatcherPump(transport.Finished.Task);
                WaitUntilWithDispatcherPump(() => oldSource.DisposeCount == 1);
            }
        });
    }

    [Fact]
    public async Task Partial_Commit_Reports_Only_Files_With_Transferred_Source_Ownership()
    {
        var committedSource = new TestStorageFile("committed.txt", "file:///committed.txt");
        var uncommittedSource = new TestStorageFile("not-committed.txt", "file:///not-committed.txt");
        var upload = CreateUpload();
        upload.IsMultipleEnabled = true;
        upload.Files = new ThrowAfterFirstAddCollection();
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;

        await Should.ThrowAsync<InvalidOperationException>(() => upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [committedSource, uncommittedSource],
            UploadDirectoryDropMode.Reject,
            0,
            10,
            TestContext.Current.CancellationToken));

        completed.ShouldNotBeNull();
        completed.Status.ShouldBe(UploadInputBatchStatus.Failed);
        completed.FailureReason.ShouldBe(UploadInputFailureReason.ProcessingFailed);
        completed.AcceptedFiles.Select(file => file.Name).ShouldBe(["committed.txt"]);
        upload.Files!.Select(item => item.Name).ShouldBe(["committed.txt"]);
        committedSource.DisposeCount.ShouldBe(0);
        uncommittedSource.DisposeCount.ShouldBe(1);

        await upload.RemoveFileAsync(
            upload.Files.Single().Id,
            TestContext.Current.CancellationToken);
        committedSource.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Commit_Revalidates_Current_Capacity_On_The_UI_Thread()
    {
        var releasePolicy = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var policy = new RecordingAdmissionPolicy(async (_, cancellationToken) =>
        {
            await releasePolicy.Task.WaitAsync(cancellationToken);
            return true;
        });
        var upload = CreateUpload();
        upload.MaxCount = 1;
        upload.CountOverflowBehavior = UploadCountOverflowBehavior.RejectExcess;
        upload.AdmissionPolicy = policy;
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;

        var input = upload.EnqueueFilesAsync(
            [CreateFile("candidate.txt")],
            TestContext.Current.CancellationToken);
        await policy.FirstEvaluationStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        Dispatcher.UIThread.Invoke(() => upload.Files!.Add(new UploadFileItem { Name = "external.txt" }));
        releasePolicy.SetResult();
        WaitWithDispatcherPump(input);

        upload.Files!.Select(item => item.Name).ShouldBe(["external.txt"]);
        completed.ShouldNotBeNull();
        completed.AcceptedFiles.ShouldBeEmpty();
        completed.RejectedItems.Single().Reason.ShouldBe(UploadRejectionReason.CountLimitExceeded);
    }

    [Fact]
    public async Task Commit_Validates_Lease_Ownership_Before_Mutating_The_Collection()
    {
        var source = new UnownedUploadFileSourceLease();
        var upload = CreateUpload();
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;

        await Should.ThrowAsync<InvalidOperationException>(() => upload.EnqueueFilesAsync(
            [new UploadFileInfo("unowned.txt", source)],
            TestContext.Current.CancellationToken));

        upload.Files.ShouldBeEmpty();
        source.DisposeCount.ShouldBe(0);
        completed.ShouldNotBeNull();
        completed.Status.ShouldBe(UploadInputBatchStatus.Failed);
        completed.AcceptedFiles.ShouldBeEmpty();
    }

    [Fact]
    public async Task Completed_Event_Is_Raised_On_The_UI_Thread()
    {
        var upload = CreateUpload();
        var wasOnUiThread = false;
        upload.InputBatchCompleted += (_, _) => wasOnUiThread = Dispatcher.UIThread.CheckAccess();

        var task = Task.Run(() => upload.EnqueueFilesAsync(
                [CreateFile("file.txt")],
                TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken);
        WaitWithDispatcherPump(task);

        wasOnUiThread.ShouldBeTrue();
    }

    [Fact]
    public void Admission_Policy_Runs_Outside_The_UI_Thread()
    {
        var upload = CreateUpload();
        bool? policyHadUiAccess = null;
        upload.AdmissionPolicy = new RecordingAdmissionPolicy((_, _) =>
        {
            policyHadUiAccess = Dispatcher.UIThread.CheckAccess();
            return Task.FromResult(true);
        });
        Task input = Task.CompletedTask;

        Dispatcher.UIThread.Invoke(() =>
            input = upload.EnqueueFilesAsync(
                [CreateFile("file.txt")],
                TestContext.Current.CancellationToken));
        WaitWithDispatcherPump(input);

        policyHadUiAccess.ShouldBe(false);
    }

    [Fact]
    public async Task Input_Options_Are_An_Immutable_Per_Batch_Snapshot()
    {
        var policyEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releasePolicy = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var patterns = new List<string> { "*.txt" };
        var upload = CreateUpload();
        upload.AllowedFileTypes = [new FilePickerFileType("text") { Patterns = patterns }];
        var evaluationCount = 0;
        upload.AdmissionPolicy = new RecordingAdmissionPolicy(async (_, token) =>
        {
            if (Interlocked.Increment(ref evaluationCount) == 1)
            {
                policyEntered.SetResult();
                await releasePolicy.Task.WaitAsync(token);
            }
            return true;
        });

        var input = upload.EnqueueFilesAsync(
            [CreateFile("first.txt"), CreateFile("second.txt")],
            TestContext.Current.CancellationToken);
        await policyEntered.Task.WaitAsync(TestContext.Current.CancellationToken);
        patterns[0] = "*.png";
        releasePolicy.SetResult();
        WaitWithDispatcherPump(input);

        upload.Files!.Select(item => item.Name).ShouldBe(["first.txt", "second.txt"]);
    }

    [Fact]
    public void Concurrent_Batches_Never_Run_Admission_At_The_Same_Time()
    {
        var active = 0;
        var maximumActive = 0;
        var policy = new RecordingAdmissionPolicy(async (_, cancellationToken) =>
        {
            var current = Interlocked.Increment(ref active);
            SetMaximum(ref maximumActive, current);
            try
            {
                await Task.Delay(20, cancellationToken);
                return true;
            }
            finally
            {
                Interlocked.Decrement(ref active);
            }
        });
        var upload = CreateUpload();
        upload.AdmissionPolicy = policy;

        var first = upload.EnqueueFilesAsync([CreateFile("first.txt")], TestContext.Current.CancellationToken);
        var second = upload.EnqueueFilesAsync([CreateFile("second.txt")], TestContext.Current.CancellationToken);
        WaitWithDispatcherPump(Task.WhenAll(first, second));

        maximumActive.ShouldBe(1);
        upload.Files!.Select(item => item.Name).OrderBy(name => name)
            .ShouldBe(["first.txt", "second.txt"]);
    }

    [Fact]
    public async Task Rejected_Storage_File_Is_Disposed_Exactly_Once()
    {
        var storageFile = new TestStorageFile("image.png", "file:///image.png");
        var upload = CreateUpload();
        upload.AllowedFileTypes =
        [
            new FilePickerFileType("Text")
            {
                Patterns = ["*.txt"]
            }
        ];

        await upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [storageFile],
            UploadDirectoryDropMode.Reject,
            0,
            10,
            TestContext.Current.CancellationToken);

        upload.Files.ShouldBeEmpty();
        storageFile.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Single_Input_Mode_Accepts_The_First_Dropped_Item_And_Rejects_The_Rest()
    {
        var first = new TestStorageFile("first.txt", "file:///first.txt");
        var second = new TestStorageFile("second.txt", "file:///second.txt");
        var upload = CreateUpload();
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;

        await upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [first, second],
            UploadDirectoryDropMode.Reject,
            0,
            10,
            TestContext.Current.CancellationToken);

        upload.Files!.Select(item => item.Name).ShouldBe(["first.txt"]);
        completed.ShouldNotBeNull();
        completed.AcceptedFiles.Select(file => file.Name).ShouldBe(["first.txt"]);
        var rejection = completed.RejectedItems.Single();
        rejection.Name.ShouldBe("second.txt");
        rejection.Reason.ShouldBe(UploadRejectionReason.MultipleSelectionNotAllowed);
        second.DisposeCount.ShouldBe(1);

        await upload.RemoveFileAsync(upload.Files!.Single().Id, TestContext.Current.CancellationToken);
        first.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Multiple_Input_Mode_Accepts_All_Dropped_Items()
    {
        var upload = CreateUpload();
        upload.IsMultipleEnabled = true;

        await upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [
                new TestStorageFile("first.txt", "file:///first.txt"),
                new TestStorageFile("second.txt", "file:///second.txt")
            ],
            UploadDirectoryDropMode.Reject,
            0,
            10,
            TestContext.Current.CancellationToken);

        upload.Files!.Select(item => item.Name).ShouldBe(["first.txt", "second.txt"]);
    }

    [Fact]
    public async Task Duplicate_Top_Level_Storage_References_Are_Accepted_And_Disposed_Once()
    {
        var upload = CreateUpload();
        var storageFile = new TestStorageFile("duplicate.txt", "file:///duplicate.txt");
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;

        await upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [storageFile, storageFile],
            UploadDirectoryDropMode.Reject,
            32,
            10_000,
            TestContext.Current.CancellationToken);

        var item = upload.Files!.Single();
        item.Name.ShouldBe("duplicate.txt");
        completed.ShouldNotBeNull();
        completed.AcceptedFiles.Select(file => file.Name).ShouldBe(["duplicate.txt"]);

        await upload.RemoveFileAsync(item.Id, TestContext.Current.CancellationToken);
        storageFile.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Accepted_Storage_File_Lease_Is_Released_On_Remove_Reset_And_Files_Replacement()
    {
        var upload = CreateUpload();
        var removedFile = new TestStorageFile("remove.txt", "file:///remove.txt");
        await AddStorageFileAsync(upload, removedFile);
        await upload.RemoveFileAsync(upload.Files!.Single().Id, TestContext.Current.CancellationToken);
        removedFile.DisposeCount.ShouldBe(1);

        var resetFile = new TestStorageFile("reset.txt", "file:///reset.txt");
        await AddStorageFileAsync(upload, resetFile);
        await upload.ResetAsync(TestContext.Current.CancellationToken);
        resetFile.DisposeCount.ShouldBe(1);

        var replacedFile = new TestStorageFile("replace.txt", "file:///replace.txt");
        await AddStorageFileAsync(upload, replacedFile);
        upload.Files = new ObservableCollection<UploadFileItem>();
        replacedFile.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Metadata_Failure_Is_Reported_And_Does_Not_Stop_Later_Candidates()
    {
        var failed = new TestStorageFile("failed.txt", "file:///failed.txt")
        {
            MetadataException = new UnauthorizedAccessException("denied")
        };
        var accepted = new TestStorageFile("accepted.txt", "file:///accepted.txt");
        var upload = CreateUpload();
        upload.IsMultipleEnabled = true;
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;

        await upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [failed, accepted],
            UploadDirectoryDropMode.Reject,
            0,
            10,
            TestContext.Current.CancellationToken);

        upload.Files!.Select(item => item.Name).ShouldBe(["accepted.txt"]);
        failed.DisposeCount.ShouldBe(1);
        completed.ShouldNotBeNull();
        completed.RejectedItems.Single().Reason.ShouldBe(UploadRejectionReason.StorageReadFailed);
    }

    [Fact]
    public async Task Oversized_Metadata_Is_Rejected_Without_Stopping_Later_Candidates()
    {
        var oversized = new TestStorageFile(
            "oversized.txt",
            "file:///oversized.txt",
            size: ulong.MaxValue);
        var accepted = new TestStorageFile("accepted.txt", "file:///accepted.txt");
        var upload = CreateUpload();
        upload.IsMultipleEnabled = true;
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;

        await upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [oversized, accepted],
            UploadDirectoryDropMode.Reject,
            0,
            10,
            TestContext.Current.CancellationToken);

        upload.Files!.Select(item => item.Name).ShouldBe(["accepted.txt"]);
        oversized.DisposeCount.ShouldBe(1);
        completed.ShouldNotBeNull();
        completed.Status.ShouldBe(UploadInputBatchStatus.Completed);
        completed.RejectedItems.Single().Reason.ShouldBe(UploadRejectionReason.StorageReadFailed);
    }

    [Fact]
    public async Task Committed_Storage_Lease_Remains_Owned_When_A_Task_Event_Throws()
    {
        var upload = CreateUpload();
        var storageFile = new TestStorageFile("committed.txt", "file:///committed.txt");
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.UploadTaskCreated += (_, _) => throw new InvalidOperationException("task event failed");
        upload.InputBatchCompleted += (_, args) => completed = args;

        await Should.ThrowAsync<InvalidOperationException>(() => AddStorageFileAsync(upload, storageFile));

        upload.Files!.Single().Name.ShouldBe("committed.txt");
        storageFile.DisposeCount.ShouldBe(0);
        completed.ShouldNotBeNull();
        completed.AcceptedFiles.Single().Name.ShouldBe("committed.txt");

        await upload.RemoveFileAsync(
            upload.Files!.Single().Id,
            TestContext.Current.CancellationToken);
        storageFile.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Inserted_Storage_Lease_Remains_Owned_When_Collection_Notification_Throws()
    {
        var upload = CreateUpload();
        var storageFile = new TestStorageFile("collection-event.txt", "file:///collection-event.txt");
        EventHandler throwingHandler = (_, _) =>
            throw new InvalidOperationException("form value event failed");
        ((IFormItemAware)upload).ValueChanged += throwingHandler;

        await Should.ThrowAsync<InvalidOperationException>(() => AddStorageFileAsync(upload, storageFile));

        upload.Files!.Single().Name.ShouldBe("collection-event.txt");
        storageFile.DisposeCount.ShouldBe(0);

        ((IFormItemAware)upload).ValueChanged -= throwingHandler;
        await upload.RemoveFileAsync(
            upload.Files!.Single().Id,
            TestContext.Current.CancellationToken);
        storageFile.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public void External_Collection_Remove_Releases_Storage_Lease_When_Form_Notification_Throws()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var upload = CreateUpload();
            var storageFile = new TestStorageFile("external-remove.txt", "file:///external-remove.txt");
            WaitWithDispatcherPump(AddStorageFileAsync(upload, storageFile));
            EventHandler throwingHandler = (_, _) =>
                throw new InvalidOperationException("form value event failed");
            ((IFormItemAware)upload).ValueChanged += throwingHandler;

            try
            {
                Should.Throw<InvalidOperationException>(() => upload.Files!.RemoveAt(0));
                storageFile.DisposeCount.ShouldBe(1);
            }
            finally
            {
                ((IFormItemAware)upload).ValueChanged -= throwingHandler;
            }
        });
    }

    [Fact]
    public void RemoveFileAsync_Releases_Storage_Lease_When_Form_Notification_Throws()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var upload = CreateUpload();
            var storageFile = new TestStorageFile("remove-api.txt", "file:///remove-api.txt");
            WaitWithDispatcherPump(AddStorageFileAsync(upload, storageFile));
            EventHandler throwingHandler = (_, _) =>
                throw new InvalidOperationException("form value event failed");
            ((IFormItemAware)upload).ValueChanged += throwingHandler;

            try
            {
                var removeTask = upload.RemoveFileAsync(
                    upload.Files!.Single().Id,
                    TestContext.Current.CancellationToken);
                Should.Throw<InvalidOperationException>(() => WaitWithDispatcherPump(removeTask));
                storageFile.DisposeCount.ShouldBe(1);
            }
            finally
            {
                ((IFormItemAware)upload).ValueChanged -= throwingHandler;
            }
        });
    }

    [Fact]
    public void Files_Replacement_Releases_Storage_Lease_When_Form_Notification_Throws()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var upload = CreateUpload();
            var storageFile = new TestStorageFile("replace-files.txt", "file:///replace-files.txt");
            WaitWithDispatcherPump(AddStorageFileAsync(upload, storageFile));
            EventHandler throwingHandler = (_, _) =>
                throw new InvalidOperationException("form value event failed");
            ((IFormItemAware)upload).ValueChanged += throwingHandler;

            try
            {
                Should.Throw<InvalidOperationException>(() =>
                    upload.Files = new ObservableCollection<UploadFileItem>());
                storageFile.DisposeCount.ShouldBe(1);
            }
            finally
            {
                ((IFormItemAware)upload).ValueChanged -= throwingHandler;
            }
        });
    }

    [Fact]
    public void ResetAsync_Releases_Storage_Lease_When_Form_Notification_Throws()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var upload = CreateUpload();
            var storageFile = new TestStorageFile("reset.txt", "file:///reset.txt");
            WaitWithDispatcherPump(AddStorageFileAsync(upload, storageFile));
            EventHandler throwingHandler = (_, _) =>
                throw new InvalidOperationException("form value event failed");
            ((IFormItemAware)upload).ValueChanged += throwingHandler;

            try
            {
                var resetTask = upload.ResetAsync(TestContext.Current.CancellationToken);
                Should.Throw<InvalidOperationException>(() => WaitWithDispatcherPump(resetTask));
                storageFile.DisposeCount.ShouldBe(1);
            }
            finally
            {
                ((IFormItemAware)upload).ValueChanged -= throwingHandler;
            }
        });
    }

    [Fact]
    public void Detach_Releases_Accepted_Lease_When_An_Active_Batch_Completion_Handler_Throws()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var upload = CreateUpload();
            var storageFile = new TestStorageFile("detach-existing.txt", "file:///detach-existing.txt");
            var window = new Avalonia.Controls.Window { Content = upload };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            WaitWithDispatcherPump(AddStorageFileAsync(upload, storageFile));

            var policy = new CancellationBlockingPolicy();
            upload.AdmissionPolicy = policy;
            upload.InputBatchCompleted += (_, _) =>
                throw new InvalidOperationException("batch completion failed");
            var activeBatch = upload.EnqueueFilesAsync(
                [CreateFile("detach-pending.txt")],
                TestContext.Current.CancellationToken);
            WaitWithDispatcherPump(policy.Started.Task);

            window.Close();
            var error = Should.Throw<AggregateException>(() => WaitWithDispatcherPump(activeBatch));
            var innerExceptions = error.Flatten().InnerExceptions;
            innerExceptions.Any(exception =>
                exception is InvalidOperationException && exception.Message == "batch completion failed").ShouldBeTrue();
            innerExceptions.Any(exception => exception is OperationCanceledException).ShouldBeTrue();
            WaitUntilWithDispatcherPump(() => storageFile.DisposeCount == 1);
        });
    }

    [Fact]
    public async Task Cancellation_While_Waiting_For_The_Gate_Disposes_The_Unstarted_Storage_Snapshot()
    {
        var policyGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var policy = new RecordingAdmissionPolicy(async (_, cancellationToken) =>
        {
            await policyGate.Task.WaitAsync(cancellationToken);
            return true;
        });
        var upload = CreateUpload();
        upload.AdmissionPolicy = policy;
        var first = upload.EnqueueFilesAsync([CreateFile("blocking.txt")], TestContext.Current.CancellationToken);
        await policy.FirstEvaluationStarted.Task.WaitAsync(TestContext.Current.CancellationToken);

        var waitingFile = new TestStorageFile("waiting.txt", "file:///waiting.txt");
        using var cancellation = new CancellationTokenSource();
        var waiting = upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [waitingFile],
            UploadDirectoryDropMode.Reject,
            0,
            10,
            cancellation.Token);
        cancellation.Cancel();
        policyGate.SetResult();

        WaitWithDispatcherPump(first);
        Should.Throw<OperationCanceledException>(() => WaitWithDispatcherPump(waiting));

        waitingFile.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Cancellation_During_Directory_Enumeration_Releases_The_Entire_Batch()
    {
        var acquiredChild = new TestStorageFile("acquired.txt", "file:///folder/acquired.txt");
        var unobservedChild = new TestStorageFile("unobserved.txt", "file:///folder/unobserved.txt");
        var enumerationGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var folder = new TestStorageFolder("folder", "file:///folder/", acquiredChild, unobservedChild)
        {
            PauseAfterItemCount = 1,
            EnumerationPauseGate = enumerationGate
        };
        var upload = CreateUpload();
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;
        using var cancellation = new CancellationTokenSource();

        var task = upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [folder],
            UploadDirectoryDropMode.RecursiveFiles,
            5,
            10,
            cancellation.Token);
        await folder.EnumerationPaused.Task.WaitAsync(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        Should.Throw<OperationCanceledException>(() => WaitWithDispatcherPump(task));

        folder.DisposeCount.ShouldBe(1);
        acquiredChild.DisposeCount.ShouldBe(1);
        unobservedChild.DisposeCount.ShouldBe(0);
        completed.ShouldNotBeNull();
        completed.Status.ShouldBe(UploadInputBatchStatus.Cancelled);
        completed.FailureReason.ShouldBeNull();
    }

    [Fact]
    public async Task Cancellation_During_Admission_Raises_Exactly_One_Cancelled_Result()
    {
        var policyGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var policy = new RecordingAdmissionPolicy(async (_, cancellationToken) =>
        {
            await policyGate.Task.WaitAsync(cancellationToken);
            return true;
        });
        var upload = CreateUpload();
        upload.AdmissionPolicy = policy;
        var events = new List<UploadInputBatchCompletedEventArgs>();
        upload.InputBatchCompleted += (_, args) => events.Add(args);
        using var cancellation = new CancellationTokenSource();

        var task = upload.EnqueueFilesAsync([CreateFile("file.txt")], cancellation.Token);
        await policy.FirstEvaluationStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        Should.Throw<OperationCanceledException>(() => WaitWithDispatcherPump(task));

        upload.Files.ShouldBeEmpty();
        events.Count.ShouldBe(1);
        events[0].Status.ShouldBe(UploadInputBatchStatus.Cancelled);
        events[0].FailureReason.ShouldBeNull();
    }

    [Fact]
    public async Task Cancellation_Cleans_Resources_Then_Raises_Event_Then_Throws_To_Caller()
    {
        var policy = new CancellationBlockingPolicy();
        var upload = CreateUpload();
        upload.AdmissionPolicy = policy;
        var storageFile = new TestStorageFile("file.txt", "file:///file.txt");
        var disposeCountAtEvent = -1;
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) =>
        {
            disposeCountAtEvent = storageFile.DisposeCount;
            completed = args;
        };
        using var cancellation = new CancellationTokenSource();

        var input = upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [storageFile],
            UploadDirectoryDropMode.Reject,
            32,
            10_000,
            cancellation.Token);
        await policy.Started.Task.WaitAsync(TestContext.Current.CancellationToken);
        cancellation.Cancel();

        Should.Throw<OperationCanceledException>(() => WaitWithDispatcherPump(input));
        disposeCountAtEvent.ShouldBe(1);
        completed.ShouldNotBeNull();
        completed.Status.ShouldBe(UploadInputBatchStatus.Cancelled);
        completed.FailureReason.ShouldBeNull();
    }

    [Fact]
    public async Task Cleanup_Failure_Overrides_Cancellation_After_All_Resources_Are_Attempted()
    {
        var policy = new CancellationBlockingPolicy();
        var upload = CreateUpload();
        upload.IsMultipleEnabled = true;
        upload.AdmissionPolicy = policy;
        var first = new ThrowingStorageFile("first.txt", "file:///first.txt");
        var second = new ThrowingStorageFile("second.txt", "file:///second.txt");
        using var cancellation = new CancellationTokenSource();
        UploadInputBatchCompletedEventArgs? completed = null;
        upload.InputBatchCompleted += (_, args) => completed = args;

        var input = upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [first, second],
            UploadDirectoryDropMode.Reject,
            32,
            10_000,
            cancellation.Token);
        await policy.Started.Task.WaitAsync(TestContext.Current.CancellationToken);
        cancellation.Cancel();

        var error = Should.Throw<AggregateException>(() => WaitWithDispatcherPump(input));

        var flattened = error.Flatten();
        flattened.InnerExceptions.ShouldContain(exception => exception is OperationCanceledException);
        flattened.InnerExceptions.Count(exception => exception is InvalidOperationException).ShouldBe(2);
        first.DisposeCount.ShouldBe(1);
        second.DisposeCount.ShouldBe(1);
        completed.ShouldNotBeNull();
        completed.Status.ShouldBe(UploadInputBatchStatus.Failed);
        completed.FailureReason.ShouldBe(UploadInputFailureReason.ProcessingFailed);
    }

    [Fact]
    public void Remove_Waits_For_Running_Upload_To_End_Before_Releasing_The_Storage_Lease()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var transport = new DelayedCancellationTransport();
            var upload = new Desktop.Controls.Upload
            {
                UploadTransport = transport,
                AutoUpload = true,
                Files = new ObservableCollection<UploadFileItem>()
            };
            Dispatcher.UIThread.RunJobs();
            var storageFile = new TestStorageFile("running.txt", "file:///running.txt");

            WaitWithDispatcherPump(AddStorageFileAsync(upload, storageFile));
            WaitWithDispatcherPump(transport.Started.Task);
            var removeTask = upload.RemoveFileAsync(
                upload.Files!.Single().Id,
                TestContext.Current.CancellationToken);
            WaitWithDispatcherPump(transport.CancellationObserved.Task);

            removeTask.IsCompleted.ShouldBeFalse();
            storageFile.DisposeCount.ShouldBe(0);

            transport.AllowExit.SetResult();
            WaitWithDispatcherPump(removeTask);
            storageFile.DisposeCount.ShouldBe(1);
        });
    }

    [Fact]
    public void External_Collection_Remove_Waits_For_Running_Upload_Before_Releasing_The_Storage_Lease()
    {
        AssertMutationDefersLeaseRelease(upload => upload.Files!.RemoveAt(0));
    }

    [Fact]
    public void External_Collection_Remove_Raises_Cancellation_Before_Releasing_The_Storage_Lease()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var transport = new DelayedCancellationTransport();
            var upload = new Desktop.Controls.Upload
            {
                UploadTransport = transport,
                AutoUpload = true,
                Files = new ObservableCollection<UploadFileItem>()
            };
            Dispatcher.UIThread.RunJobs();
            var storageFile = new TestStorageFile("cancel-event.txt", "file:///cancel-event.txt");
            var cancellationRaised = false;
            upload.UploadTaskCancelled += (_, _) =>
            {
                storageFile.DisposeCount.ShouldBe(0);
                cancellationRaised = true;
            };

            WaitWithDispatcherPump(AddStorageFileAsync(upload, storageFile));
            WaitWithDispatcherPump(transport.Started.Task);
            upload.Files!.RemoveAt(0);
            WaitWithDispatcherPump(transport.CancellationObserved.Task);

            transport.AllowExit.SetResult();
            Thread.Sleep(100);
            storageFile.DisposeCount.ShouldBe(0);

            WaitUntilWithDispatcherPump(() => cancellationRaised);
            WaitUntilWithDispatcherPump(() => storageFile.DisposeCount == 1);
        });
    }

    [Fact]
    public void External_Collection_Reset_Waits_For_Running_Upload_Before_Releasing_The_Storage_Lease()
    {
        AssertMutationDefersLeaseRelease(upload => upload.Files!.Clear());
    }

    [Fact]
    public void Files_Replacement_Waits_For_Running_Upload_Before_Releasing_The_Storage_Lease()
    {
        AssertMutationDefersLeaseRelease(upload =>
            upload.Files = new ObservableCollection<UploadFileItem>());
    }

    [Fact]
    public void SetFormValue_Waits_For_Running_Upload_Before_Releasing_The_Storage_Lease()
    {
        AssertMutationDefersLeaseRelease(upload =>
            ((IFormItemAware)upload).SetFormValue(Array.Empty<UploadFileItem>()));
    }

    [Fact]
    public void ClearFormValue_Waits_For_Running_Upload_Before_Releasing_The_Storage_Lease()
    {
        AssertMutationDefersLeaseRelease(upload =>
            ((IFormItemAware)upload).ClearFormValue());
    }

    [Fact]
    public void Detach_Cancels_An_Active_Input_Batch_And_Releases_Its_Lease()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var policy = new CancellationBlockingPolicy();
            var upload = CreateUpload();
            upload.AdmissionPolicy = policy;
            var storageFile = new TestStorageFile("pending.txt", "file:///pending.txt");
            var window = new Avalonia.Controls.Window { Content = upload };

            window.Show();
            Dispatcher.UIThread.RunJobs();
            var inputTask = AddStorageFileAsync(upload, storageFile);
            WaitWithDispatcherPump(policy.Started.Task);

            window.Close();
            Should.Throw<OperationCanceledException>(() => WaitWithDispatcherPump(inputTask));

            storageFile.DisposeCount.ShouldBe(1);
        });
    }

    private static Desktop.Controls.Upload CreateUpload()
    {
        return new Desktop.Controls.Upload
        {
            AutoUpload = false,
            Files = new ObservableCollection<UploadFileItem>()
        };
    }

    private static UploadFileInfo CreateFile(string name)
    {
        return new UploadFileInfo(name, new UploadTestFileSource(), new Uri($"file:///{name}"), 3);
    }

    private static Task AddStorageFileAsync(Desktop.Controls.Upload upload, TestStorageFile storageFile)
    {
        return upload.ProcessStorageItemsAsync(
            UploadInputSource.DragDrop,
            [storageFile],
            UploadDirectoryDropMode.Reject,
            0,
            10,
            TestContext.Current.CancellationToken);
    }

    private static void AssertMutationDefersLeaseRelease(Action<Desktop.Controls.Upload> mutation)
    {
        var transport = new DelayedCancellationTransport();
        var upload = new Desktop.Controls.Upload
        {
            UploadTransport = transport,
            AutoUpload = true,
            Files = new ObservableCollection<UploadFileItem>()
        };
        Dispatcher.UIThread.RunJobs();
        var storageFile = new TestStorageFile("running.txt", "file:///running.txt");

        WaitWithDispatcherPump(AddStorageFileAsync(upload, storageFile));
        WaitWithDispatcherPump(transport.Started.Task);

        try
        {
            mutation(upload);
            WaitWithDispatcherPump(transport.CancellationObserved.Task);
            storageFile.DisposeCount.ShouldBe(0);
        }
        finally
        {
            transport.AllowExit.TrySetResult();
            WaitUntilWithDispatcherPump(() => storageFile.DisposeCount == 1);
        }
    }

    private static void WaitWithDispatcherPump(Task task)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Invoke(() => WaitWithDispatcherPump(task));
            return;
        }

        var timeout = Stopwatch.StartNew();
        while (!task.IsCompleted && timeout.Elapsed < TimeSpan.FromSeconds(5))
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        task.IsCompleted.ShouldBeTrue("The asynchronous Upload operation should complete within the test timeout.");
        task.GetAwaiter().GetResult();
    }

    private static void WaitUntilWithDispatcherPump(Func<bool> condition)
    {
        var timeout = Stopwatch.StartNew();
        while (!condition() && timeout.Elapsed < TimeSpan.FromSeconds(5))
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        condition().ShouldBeTrue("The Upload lifecycle operation should complete within the test timeout.");
    }

    private static void SetMaximum(ref int location, int value)
    {
        int current;
        do
        {
            current = Volatile.Read(ref location);
            if (current >= value)
            {
                return;
            }
        }
        while (Interlocked.CompareExchange(ref location, value, current) != current);
    }

    private sealed class RecordingAdmissionPolicy : IUploadAdmissionPolicy
    {
        private readonly Func<UploadFileInfo, CancellationToken, ValueTask<bool>> _evaluate;

        internal List<string> Names { get; } = [];
        internal TaskCompletionSource FirstEvaluationStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal RecordingAdmissionPolicy(Func<UploadFileInfo, bool> evaluate)
            : this((file, _) => ValueTask.FromResult(evaluate(file)))
        {
        }

        internal RecordingAdmissionPolicy(Func<UploadFileInfo, CancellationToken, Task<bool>> evaluate)
            : this((file, cancellationToken) => new ValueTask<bool>(evaluate(file, cancellationToken)))
        {
        }

        private RecordingAdmissionPolicy(Func<UploadFileInfo, CancellationToken, ValueTask<bool>> evaluate)
        {
            _evaluate = evaluate;
        }

        public async ValueTask<UploadAdmissionDecision> EvaluateAsync(
            UploadAdmissionContext context,
            CancellationToken cancellationToken = default)
        {
            Names.Add(context.File.Name);
            FirstEvaluationStarted.TrySetResult();
            var accepted = await _evaluate(context.File, cancellationToken);
            return accepted
                ? UploadAdmissionDecision.Accept()
                : UploadAdmissionDecision.Reject();
        }
    }

    private sealed class CancellationBlockingPolicy : IUploadAdmissionPolicy
    {
        internal TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<UploadAdmissionDecision> EvaluateAsync(
            UploadAdmissionContext context,
            CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return UploadAdmissionDecision.Accept();
        }
    }

    private sealed class DelayedCancellationTransport : IFileUploadTransport
    {
        internal TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource CancellationObserved { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource AllowExit { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Finished { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal void Release()
        {
            AllowExit.TrySetResult();
        }

        public async Task<FileUploadResult> UploadAsync(
            UploadFileInfo fileInfo,
            object? context = null,
            IProgress<FileUploadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                CancellationObserved.TrySetResult();
                await AllowExit.Task;
                Finished.TrySetResult();
                throw;
            }

            throw new InvalidOperationException("The blocking transport should only finish by cancellation.");
        }
    }

    private sealed class ThrowAfterFirstAddCollection : Collection<UploadFileItem>
    {
        protected override void InsertItem(int index, UploadFileItem item)
        {
            if (Count == 1)
            {
                throw new InvalidOperationException("The second collection insert failed.");
            }
            base.InsertItem(index, item);
        }
    }

    private sealed class UnownedUploadFileSourceLease : IUploadFileSource, IUploadFileSourceLease
    {
        internal int DisposeCount { get; private set; }

        public ValueTask<Stream> OpenReadAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<Stream>(new MemoryStream([1, 2, 3], writable: false));
        }

        public void Dispose()
        {
            DisposeCount++;
        }
    }
}
