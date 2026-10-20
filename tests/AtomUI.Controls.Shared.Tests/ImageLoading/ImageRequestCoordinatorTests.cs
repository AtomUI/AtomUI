using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class ImageRequestCoordinatorTests
{
    [Fact]
    public async Task Decoded_Waiters_Share_One_Operation_And_Acquire_Leases_Before_Release()
    {
        using var coordinator = new ImageRequestCoordinator();
        var key = CreateDecodedOperationKey("shared");
        var started = NewSignal();
        var release = NewSignal();
        var image = new TestImage();
        var factoryCalls = 0;

        async Task<ImageDecodedCacheEntry> Factory(
            ImageRequestCoordinator.SharedOperationContext context,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref factoryCalls);
            started.TrySetResult();
            await release.Task.WaitAsync(cancellationToken);
            var entry = CreateOperationEntry(image);
            entry.RetainOperation();
            return entry;
        }

        var firstTask = coordinator.GetDecodedAsync(
            key,
            new ImageRequestPriorityState(ImageRequestPriority.Normal),
            Factory,
            entry => entry.AcquireResult(
                ImageLoadOrigin.Local,
                ImageSourceValidation.Current,
                contentId: null),
            null,
            CancellationToken.None);
        await started.Task;
        var secondTask = coordinator.GetDecodedAsync(
            key,
            new ImageRequestPriorityState(ImageRequestPriority.High),
            Factory,
            entry => entry.AcquireResult(
                ImageLoadOrigin.Local,
                ImageSourceValidation.Current,
                contentId: null),
            null,
            CancellationToken.None);
        release.TrySetResult();

        using var first = await firstTask;
        using var second = await secondTask;

        factoryCalls.ShouldBe(1);
        first.Image.ShouldBeSameAs(image);
        second.Image.ShouldBeSameAs(image);
        image.DisposeCount.ShouldBe(0);
        first.Dispose();
        image.DisposeCount.ShouldBe(0);
        second.Dispose();
        image.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Canceling_One_Waiter_Does_Not_Cancel_Shared_Work_For_Remaining_Waiter()
    {
        using var coordinator = new ImageRequestCoordinator();
        using var firstCancellation = new CancellationTokenSource();
        var key = CreateSourceOperationKey("cancel-one");
        var started = NewSignal();
        var release = NewSignal();
        var underlyingCanceled = false;

        async Task<ImageValidatedContent> Factory(
            ImageRequestCoordinator.SharedOperationContext context,
            CancellationToken cancellationToken)
        {
            started.TrySetResult();
            try
            {
                await release.Task.WaitAsync(cancellationToken);
                return CreateValidated([1]);
            }
            catch (OperationCanceledException)
            {
                underlyingCanceled = true;
                throw;
            }
        }

        var first = coordinator.GetSourceAsync(
            key,
            new ImageRequestPriorityState(ImageRequestPriority.Normal),
            Factory,
            null,
            firstCancellation.Token);
        await started.Task;
        var second = coordinator.GetSourceAsync(
            key,
            new ImageRequestPriorityState(ImageRequestPriority.Normal),
            Factory,
            null,
            CancellationToken.None);

        firstCancellation.Cancel();
        await Should.ThrowAsync<OperationCanceledException>(async () => await first);
        underlyingCanceled.ShouldBeFalse();

        release.TrySetResult();
        (await second).Content.Bytes.ShouldBe([1]);
        underlyingCanceled.ShouldBeFalse();
    }

    [Fact]
    public async Task Last_Waiter_Cancellation_Cancels_Underlying_Operation()
    {
        using var coordinator = new ImageRequestCoordinator();
        using var cancellation = new CancellationTokenSource();
        var started = NewSignal();
        var underlyingCanceled = NewSignal();

        var task = coordinator.GetSourceAsync(
            CreateSourceOperationKey("cancel-last"),
            new ImageRequestPriorityState(ImageRequestPriority.Normal),
            async (_, token) =>
            {
                started.TrySetResult();
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                    throw new InvalidOperationException();
                }
                catch (OperationCanceledException)
                {
                    underlyingCanceled.TrySetResult();
                    throw;
                }
            },
            null,
            cancellation.Token);
        await started.Task;

        cancellation.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(async () => await task);
        await underlyingCanceled.Task;
    }

    [Fact]
    public async Task Completed_Operation_Is_Not_Reused_By_A_New_Waiter()
    {
        using var coordinator = new ImageRequestCoordinator();
        var calls = 0;
        var key = CreateSourceOperationKey("completed");

        Task<ImageValidatedContent> Factory(
            ImageRequestCoordinator.SharedOperationContext context,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(CreateValidated([
                checked((byte)Interlocked.Increment(ref calls))
            ]));
        }

        var first = await coordinator.GetSourceAsync(
            key,
            new ImageRequestPriorityState(ImageRequestPriority.Normal),
            Factory,
            null,
            CancellationToken.None);
        var second = await coordinator.GetSourceAsync(
            key,
            new ImageRequestPriorityState(ImageRequestPriority.Normal),
            Factory,
            null,
            CancellationToken.None);

        first.Content.Bytes.ShouldBe([1]);
        second.Content.Bytes.ShouldBe([2]);
        calls.ShouldBe(2);
    }

    [Fact]
    public async Task Shared_Operation_Priority_Tracks_Highest_Active_Waiter()
    {
        using var coordinator = new ImageRequestCoordinator();
        using var criticalCancellation = new CancellationTokenSource();
        var contextSignal = new TaskCompletionSource<ImageRequestCoordinator.SharedOperationContext>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var release = NewSignal();
        var key = CreateSourceOperationKey("priority");

        async Task<ImageValidatedContent> Factory(
            ImageRequestCoordinator.SharedOperationContext context,
            CancellationToken cancellationToken)
        {
            contextSignal.TrySetResult(context);
            await release.Task.WaitAsync(cancellationToken);
            return CreateValidated([1]);
        }

        var preloadPriority = new ImageRequestPriorityState(ImageRequestPriority.Preload);
        var preload = coordinator.GetSourceAsync(
            key,
            preloadPriority,
            Factory,
            null,
            CancellationToken.None);
        var context = await contextSignal.Task;
        context.Priority.ShouldBe(ImageRequestPriority.Preload);
        preloadPriority.Promote(ImageRequestPriority.High);
        context.Priority.ShouldBe(ImageRequestPriority.High);
        var critical = coordinator.GetSourceAsync(
            key,
            new ImageRequestPriorityState(ImageRequestPriority.Critical),
            Factory,
            null,
            criticalCancellation.Token);
        context.Priority.ShouldBe(ImageRequestPriority.Critical);

        criticalCancellation.Cancel();
        await Should.ThrowAsync<OperationCanceledException>(async () => await critical);
        context.Priority.ShouldBe(ImageRequestPriority.High);

        release.TrySetResult();
        await preload;
    }

    [Fact]
    public async Task Dispose_Cancels_InFlight_Work_Without_Waiting_For_The_Factory_To_Return()
    {
        var coordinator = new ImageRequestCoordinator();
        var started = NewSignal();
        var canceled = NewSignal();
        var release = NewSignal();
        var loadTask = coordinator.GetSourceAsync(
            CreateSourceOperationKey("dispose-non-blocking"),
            new ImageRequestPriorityState(ImageRequestPriority.Normal),
            async (_, token) =>
            {
                using var registration = token.Register(() => canceled.TrySetResult());
                started.TrySetResult();
                await release.Task;
                return CreateValidated([1]);
            },
            null,
            CancellationToken.None);
        await started.Task;

        var disposeTask = Task.Run(coordinator.Dispose, TestContext.Current.CancellationToken);
        await canceled.Task;
        var returnedBeforeFactory = false;
        try
        {
            await disposeTask.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
            returnedBeforeFactory = true;
        }
        catch (TimeoutException)
        {
        }
        finally
        {
            release.TrySetResult();
            await disposeTask;
        }

        returnedBeforeFactory.ShouldBeTrue();
        await Should.ThrowAsync<OperationCanceledException>(async () => await loadTask);
    }

    private static ImageDecodedCacheEntry CreateOperationEntry(TestImage image)
    {
        return new ImageDecodedCacheEntry(
            image,
            ownsImage: true,
            10,
            10,
            10,
            10,
            400,
            "image/png");
    }

    private static ImageValidatedContent CreateValidated(byte[] bytes)
    {
        return new ImageValidatedContent(
            ImageLoadingTestSupport.CreateContent(bytes).MarkValidated(),
            new ImageProbeResult(ImageContentFormat.Png, "image/png", 1, 1, false),
            ImageSourceValidation.Current);
    }

    private static ImageSourceOperationKey CreateSourceOperationKey(string value)
    {
        return new ImageSourceOperationKey(
            new ImageSourceKey(value, string.Empty),
            ImageCacheReadPolicy.ValidateSource,
            ImageCacheStoragePolicy.MemoryAndDisk,
            string.Empty);
    }

    private static ImageDecodedOperationKey CreateDecodedOperationKey(string value)
    {
        return new ImageDecodedOperationKey(
            new ImageDecodeKey(
                string.Empty,
                new ImageContentId(ImageCacheKey.Hash(value)),
                new ImageDecodeSpec(10, 10, "codec")),
            ImageCacheStoragePolicy.MemoryAndDisk,
            string.Empty);
    }

    private static TaskCompletionSource NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}
