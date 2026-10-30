using Avalonia;
using System.Net;
using System.Net.Http.Headers;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class ImageLoaderLifecycleTests
{
    [Fact]
    public void Attached_Loader_Dispose_Without_Image_Use_Does_Not_Create_Http_Transport()
    {
        var application = new Application();
        var creations = 0;
        var loader = CreateLoader(() =>
        {
            creations++;
            return CreateHttpTransport(new CountingHandler());
        });
        loader.Attach(application);

        loader.Dispose();

        creations.ShouldBe(0);
        application.TryGetImageLoader().ShouldBeNull();
    }

    [Fact]
    public void Unused_Injected_Http_Handler_Is_Disposed_With_Loader()
    {
        var handler = new CountingHandler();
        var loader = new ImageLoader(
            ImageLoadingTestSupport.CreateOptions(),
            [new PassThroughCodec()],
            handler);

        loader.Dispose();

        handler.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task NonHttp_Use_Does_Not_Delay_Injected_Handler_Disposal()
    {
        var handler = new CountingHandler();
        var loader = new ImageLoader(
            ImageLoadingTestSupport.CreateOptions(),
            [new PassThroughCodec()],
            handler);
        using var result = await loader.LoadAsync(
            new ImageLoadRequest(new BytesImageSource(ImageLoadingTestSupport.CreatePngHeader())),
            TestContext.Current.CancellationToken);

        loader.Dispose();

        result.IsSuccess.ShouldBeTrue();
        handler.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Http_Use_Transfers_Injected_Handler_Ownership_To_Transport()
    {
        var handler = new CountingHandler(CreatePngResponse);
        var loader = new ImageLoader(
            ImageLoadingTestSupport.CreateOptions(),
            [new PassThroughCodec()],
            handler);
        using var result = await loader.LoadAsync(
            new ImageLoadRequest(new HttpImageSource(new Uri("https://example.com/injected.png"))),
            TestContext.Current.CancellationToken);

        loader.Dispose();

        result.IsSuccess.ShouldBeTrue();
        handler.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task First_Http_Use_Creates_Transport_And_Loader_Disposes_Its_Handler()
    {
        var application = new Application();
        var creations = 0;
        var handler = new CountingHandler(CreatePngResponse);
        var loader = CreateLoader(() =>
        {
            creations++;
            return CreateHttpTransport(handler);
        });
        loader.Attach(application);

        using var result = await loader.LoadAsync(
            new ImageLoadRequest(new HttpImageSource(new Uri("https://example.com/application.png"))),
            TestContext.Current.CancellationToken);
        loader.Dispose();

        result.IsSuccess.ShouldBeTrue();
        creations.ShouldBe(1);
        handler.DisposeCount.ShouldBe(1);
        application.TryGetImageLoader().ShouldBeNull();
    }

    [Fact]
    public async Task Independent_Application_Loaders_Own_Separate_Http_Transports()
    {
        var firstApplication = new Application();
        var secondApplication = new Application();
        var firstCreations = 0;
        var secondCreations = 0;
        using var firstLoader = CreateLoader(() =>
        {
            firstCreations++;
            return CreateHttpTransport(new CountingHandler(CreatePngResponse));
        });
        using var secondLoader = CreateLoader(() =>
        {
            secondCreations++;
            return CreateHttpTransport(new CountingHandler(CreatePngResponse));
        });
        firstLoader.Attach(firstApplication);
        secondLoader.Attach(secondApplication);

        using var first = await firstLoader.LoadAsync(
            new ImageLoadRequest(new HttpImageSource(new Uri("https://example.com/first.png"))),
            TestContext.Current.CancellationToken);
        secondCreations.ShouldBe(0);
        using var second = await secondLoader.LoadAsync(
            new ImageLoadRequest(new HttpImageSource(new Uri("https://example.com/second.png"))),
            TestContext.Current.CancellationToken);

        first.IsSuccess.ShouldBeTrue();
        second.IsSuccess.ShouldBeTrue();
        firstCreations.ShouldBe(1);
        secondCreations.ShouldBe(1);
        firstApplication.GetImageLoader().ShouldBeSameAs(firstLoader);
        secondApplication.GetImageLoader().ShouldBeSameAs(secondLoader);
    }

    [Fact]
    public void Attach_Publishes_One_Application_Loader_And_Detach_Removes_It()
    {
        var application = new Application();
        using var loader = CreateLoader();
        using var competingLoader = CreateLoader();

        application.TryGetImageLoader().ShouldBeNull();
        Should.Throw<InvalidOperationException>(() => application.GetImageLoader())
              .Message.ShouldContain("UseImageLoading");

        loader.Attach(application);

        application.GetImageLoader().ShouldBeSameAs(loader);
        application.TryGetImageLoader().ShouldBeSameAs(loader);
        Should.Throw<InvalidOperationException>(() => competingLoader.Attach(application))
              .Message.ShouldContain("already attached");

        loader.Detach(application);

        application.TryGetImageLoader().ShouldBeNull();
        competingLoader.Attach(application);
        application.GetImageLoader().ShouldBeSameAs(competingLoader);
    }

    [Fact]
    public async Task Dispose_Stops_Publishing_The_Attached_Loader()
    {
        var application = new Application();
        var loader = CreateLoader();
        loader.Attach(application);

        loader.Dispose();

        application.TryGetImageLoader().ShouldBeNull();
        await Should.ThrowAsync<ObjectDisposedException>(() =>
            loader.LoadAsync(
                new ImageLoadRequest(new BytesImageSource(new byte[] { 1 })))
                  .AsTask());
    }

    [Fact]
    public async Task Waiter_Timeout_Is_Typed_And_Does_Not_Cancel_A_Shared_Request_With_Other_Waiters()
    {
        var readStarted = NewSignal();
        var releaseRead = NewSignal();
        var underlyingCanceled = 0;
        var source = new StreamImageSource(
            async token =>
            {
                using var registration = token.Register(() => Interlocked.Exchange(ref underlyingCanceled, 1));
                readStarted.TrySetResult();
                await releaseRead.Task.WaitAsync(token);
                return new MemoryStream(ImageLoadingTestSupport.CreatePngHeader());
            },
            "waiter-timeout",
            "v1");
        using var loader = CreateLoader();
        var cancellationToken = TestContext.Current.CancellationToken;

        var survivingWaiter = loader.LoadAsync(
            new ImageLoadRequest(source)
            {
                Options = new ImageRequestOptions { Timeout = TimeSpan.FromSeconds(5) }
            },
            cancellationToken).AsTask();
        await readStarted.Task.WaitAsync(cancellationToken);
        var timedOutWaiter = loader.LoadAsync(
            new ImageLoadRequest(source)
            {
                Options = new ImageRequestOptions { Timeout = TimeSpan.FromMilliseconds(50) }
            },
            cancellationToken).AsTask();

        using var timedOutResult = await timedOutWaiter.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        timedOutResult.IsSuccess.ShouldBeFalse();
        timedOutResult.Error.ShouldNotBeNull().Code.ShouldBe(ImageLoadErrorCode.Timeout);
        Volatile.Read(ref underlyingCanceled).ShouldBe(0);
        survivingWaiter.IsCompleted.ShouldBeFalse();

        releaseRead.TrySetResult();
        using var survivingResult = await survivingWaiter.WaitAsync(cancellationToken);

        survivingResult.IsSuccess.ShouldBeTrue();
        Volatile.Read(ref underlyingCanceled).ShouldBe(0);
    }

    [Fact]
    public async Task Unexpected_Source_Exception_Is_Reported_As_A_Typed_Failure()
    {
        using var loader = CreateLoader();
        var source = new StreamImageSource(
            _ => ValueTask.FromException<Stream>(new InvalidOperationException("source failed")),
            "throwing-source");

        using var result = await loader.LoadAsync(
            new ImageLoadRequest(source),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull().Code.ShouldBe(ImageLoadErrorCode.InvalidSource);
        result.Error.Exception.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task LoadEvent_Observer_Failure_Does_Not_Change_Load_Result_Or_Stop_Other_Observers()
    {
        using var loader = CreateLoader();
        var observed = 0;
        loader.LoadEvent += (_, _) => throw new InvalidOperationException("diagnostics observer failed");
        loader.LoadEvent += (_, _) => Interlocked.Increment(ref observed);

        using var result = await loader.LoadAsync(
            new ImageLoadRequest(new BytesImageSource(ImageLoadingTestSupport.CreatePngHeader())),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        Volatile.Read(ref observed).ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Progress_Observer_Failure_Does_Not_Change_Load_Result()
    {
        using var loader = CreateLoader();
        using var result = await loader.LoadAsync(
            new ImageLoadRequest(new BytesImageSource(ImageLoadingTestSupport.CreatePngHeader()))
            {
                Progress = new ThrowingProgress()
            },
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Internal_Shared_Operation_Cancellation_Is_Not_Reported_As_A_Source_Failure()
    {
        var readStarted = NewSignal();
        var releaseRead = NewSignal();
        var source = new StreamImageSource(
            async token =>
            {
                readStarted.TrySetResult();
                await releaseRead.Task.WaitAsync(token);
                return new MemoryStream(ImageLoadingTestSupport.CreatePngHeader());
            },
            "shared-cancel-misclassify",
            "v1");
        using var loader = CreateLoader();
        var cancellationToken = TestContext.Current.CancellationToken;

        var waiter = loader.LoadAsync(
            new ImageLoadRequest(source)
            {
                Options = new ImageRequestOptions { Timeout = TimeSpan.FromSeconds(30) }
            },
            cancellationToken).AsTask();
        await readStarted.Task.WaitAsync(cancellationToken);

        try
        {
            // ClearCacheAsync(CancelInFlight) 拆除共享操作；调用方自身 token 并未取消。
            // 该内部取消必须按取消（OperationCanceledException）交付，而不是源失败结果。
            await loader.ClearCacheAsync(new ImageCacheClearRequest(), cancellationToken);

            await Should.ThrowAsync<OperationCanceledException>(
                () => waiter.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken));
        }
        finally
        {
            releaseRead.TrySetResult();
        }
    }

    private static ImageLoader CreateLoader(Func<HttpImageTransport>? httpTransportFactory = null)
    {
        return new ImageLoader(
            ImageLoadingTestSupport.CreateOptions(),
            [new PassThroughCodec()],
            httpTransportFactory: httpTransportFactory);
    }

    private static HttpImageTransport CreateHttpTransport(HttpMessageHandler handler) =>
        new(ImageLoadingTestSupport.CreateOptions(), handler);

    private static HttpResponseMessage CreatePngResponse(HttpRequestMessage request)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(ImageLoadingTestSupport.CreatePngHeader())
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        return response;
    }

    private static TaskCompletionSource NewSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private sealed class ThrowingProgress : IProgress<ImageLoadProgress>
    {
        public void Report(ImageLoadProgress value) => throw new InvalidOperationException("progress observer failed");
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _send;

        internal CountingHandler(Func<HttpRequestMessage, HttpResponseMessage>? send = null)
        {
            _send = send ?? (_ => throw new NotSupportedException());
        }

        internal int DisposeCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_send(request));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCount++;
            }
            base.Dispose(disposing);
        }
    }

    private sealed class PassThroughCodec : ImageCodec
    {
        internal override string Id => "lifecycle-test";

        internal override int Version => 1;

        internal override bool CanDecode(ImageProbeResult probe, ImageSource source) => true;

        internal override Task<ImageDecodedCacheEntry> DecodeAsync(
            ImageEncodedContent content,
            ImageProbeResult probe,
            NormalizedImageRequest request,
            CancellationToken cancellationToken)
        {
            var width = request.DecodePixelWidth > 0 ? request.DecodePixelWidth : probe.PixelWidth;
            var height = request.DecodePixelHeight > 0 ? request.DecodePixelHeight : probe.PixelHeight;
            return Task.FromResult(new ImageDecodedCacheEntry(
                new TestImage(width, height),
                ownsImage: true,
                probe.PixelWidth,
                probe.PixelHeight,
                width,
                height,
                checked((long)width * height * 4),
                probe.MediaType,
                content.Origin));
        }
    }
}
