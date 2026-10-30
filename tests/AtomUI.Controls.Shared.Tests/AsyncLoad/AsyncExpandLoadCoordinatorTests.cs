using AtomUI.Controls.AsyncLoad;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.AsyncLoad;

public class AsyncExpandLoadCoordinatorTests
{
    [Fact]
    public async Task Concurrent_requests_for_the_same_context_share_one_loader_operation()
    {
        var coordinator = new AsyncExpandLoadCoordinator<string, string>();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var callBarrier = new Barrier(64);
        var calls = 0;

        async Task<string> LoadAsync(string context, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref calls);
            await release.Task.WaitAsync(cancellationToken);
            return context;
        }

        var allRequestsEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requestCount = 0;
        var requests = Enumerable.Range(0, 64)
            .Select(_ => Task.Run(
                () =>
                {
                    callBarrier.SignalAndWait(TestContext.Current.CancellationToken);
                    var operation = coordinator.LoadOrJoinAsync("same", LoadAsync);
                    if (Interlocked.Increment(ref requestCount) == 64)
                    {
                        allRequestsEntered.TrySetResult();
                    }
                    return operation;
                },
                TestContext.Current.CancellationToken))
            .ToArray();

        await allRequestsEntered.Task.WaitAsync(TestContext.Current.CancellationToken);
        release.TrySetResult();
        var outcomes = await Task.WhenAll(requests);

        calls.ShouldBe(1);
        outcomes.ShouldAllBe(outcome => outcome.IsSuccess);
        outcomes.ShouldAllBe(outcome => outcome.Result == "same");
    }

    [Fact]
    public async Task Cancel_reports_cancellation_instead_of_timeout()
    {
        var coordinator = new AsyncSearchLoadCoordinator<string, string>();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var loadTask = coordinator.LoadAsync(
            "same",
            async (_, cancellationToken) =>
            {
                started.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return "unreachable";
            },
            TestContext.Current.CancellationToken);

        await started.Task.WaitAsync(TestContext.Current.CancellationToken);
        coordinator.Cancel();

        var outcome = await loadTask;

        outcome.IsCancelled.ShouldBeTrue();
        outcome.IsTimedOut.ShouldBeFalse();
    }
}
