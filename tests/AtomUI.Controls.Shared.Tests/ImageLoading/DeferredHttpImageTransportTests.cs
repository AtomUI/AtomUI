using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class DeferredHttpImageTransportTests
{
    [Fact]
    public void Construction_Does_Not_Create_Transport()
    {
        var creations = 0;
        using var owner = new DeferredHttpImageTransport(() =>
        {
            creations++;
            return CreateTransport(new CountingHandler());
        });

        owner.IsValueCreated.ShouldBeFalse();
        creations.ShouldBe(0);
    }

    [Fact]
    public async Task Concurrent_First_Get_Creates_One_Transport()
    {
        var creations = 0;
        using var owner = new DeferredHttpImageTransport(() =>
        {
            Interlocked.Increment(ref creations);
            return CreateTransport(new CountingHandler());
        });

        var transports = await Task.WhenAll(
            Enumerable.Range(0, 32).Select(_ => Task.Run(owner.Get)));

        creations.ShouldBe(1);
        owner.IsValueCreated.ShouldBeTrue();
        transports.ShouldAllBe(transport => ReferenceEquals(transport, transports[0]));
    }

    [Fact]
    public void Dispose_Releases_Created_Transport_Exactly_Once()
    {
        var handler = new CountingHandler();
        var owner = new DeferredHttpImageTransport(() => CreateTransport(handler));
        _ = owner.Get();

        owner.Dispose();
        owner.Dispose();

        handler.DisposeCount.ShouldBe(1);
        Should.Throw<ObjectDisposedException>(owner.Get);
    }

    [Fact]
    public void Dispose_Before_First_Use_Does_Not_Create_Transport()
    {
        var creations = 0;
        var owner = new DeferredHttpImageTransport(() =>
        {
            creations++;
            return CreateTransport(new CountingHandler());
        });

        owner.Dispose();

        creations.ShouldBe(0);
        owner.IsValueCreated.ShouldBeFalse();
        Should.Throw<ObjectDisposedException>(owner.Get);
    }

    [Fact]
    public void Dispose_Before_First_Use_Releases_Pending_Owned_Resource()
    {
        var handler = new CountingHandler();
        var owner = new DeferredHttpImageTransport(
            () => CreateTransport(handler),
            handler);

        owner.Dispose();

        handler.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public void Created_Transport_Takes_Pending_Resource_Ownership()
    {
        var handler = new CountingHandler();
        var owner = new DeferredHttpImageTransport(
            () => CreateTransport(handler),
            handler);
        _ = owner.Get();

        owner.Dispose();

        handler.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Dispose_Racing_First_Get_Disposes_Published_Transport_Once()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var entered = new ManualResetEventSlim();
        var release = new ManualResetEventSlim();
        var handler = new CountingHandler();
        var owner = new DeferredHttpImageTransport(() =>
        {
            entered.Set();
            release.Wait(cancellationToken);
            return CreateTransport(handler);
        });

        var getTask = Task.Run(owner.Get, cancellationToken);
        entered.Wait(cancellationToken);
        var disposeTask = Task.Run(owner.Dispose, cancellationToken);
        release.Set();

        _ = await getTask;
        await disposeTask;

        handler.DisposeCount.ShouldBe(1);
        Should.Throw<ObjectDisposedException>(owner.Get);
    }

    [Fact]
    public void Failed_Factory_Is_Not_Published_And_Can_Be_Retried()
    {
        var attempts = 0;
        using var owner = new DeferredHttpImageTransport(() =>
        {
            if (Interlocked.Increment(ref attempts) == 1)
            {
                throw new InvalidOperationException("factory failed");
            }

            return CreateTransport(new CountingHandler());
        });

        Should.Throw<InvalidOperationException>(owner.Get).Message.ShouldBe("factory failed");
        owner.IsValueCreated.ShouldBeFalse();

        owner.Get().ShouldNotBeNull();
        attempts.ShouldBe(2);
        owner.IsValueCreated.ShouldBeTrue();
    }

    private static HttpImageTransport CreateTransport(HttpMessageHandler handler) =>
        new(ImageLoadingTestSupport.CreateOptions(), handler);

    private sealed class CountingHandler : HttpMessageHandler
    {
        internal int DisposeCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCount++;
            }
            base.Dispose(disposing);
        }
    }
}
