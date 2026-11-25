using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Avalonia;
using Avalonia.Threading;
using Shouldly;
using Svg;
using Xunit;

namespace AtomUI.Controls.Tests.ImageLoading;

public class SvgImageCodecTests
{
    private const string ValidSvg =
        "<svg xmlns='http://www.w3.org/2000/svg' width='48' height='32' viewBox='0 0 48 32'>" +
        "<rect width='48' height='32' fill='#1677ff'/></svg>";

    static SvgImageCodecTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Codec_Uses_The_Approved_Secure_Static_Contract()
    {
        var codec = new SvgImageCodec();
        var parameters = SvgImageCodec.CreateParameters();

        codec.Id.ShouldBe("atomui.svg");
        codec.Version.ShouldBe(2);
        codec.IsDecodeSizeDependent.ShouldBeFalse();
        parameters.Entities.ShouldBeNull();
        parameters.Css.ShouldBeNull();
        parameters.CurrentColor.ShouldBeNull();
        var loadOptions = parameters.LoadOptions.ShouldNotBeNull();
        loadOptions.ProcessingMode.ShouldBe(SvgProcessingMode.SecureStatic);
        loadOptions.ExternalResources.ShouldBe(SvgExternalResourcePolicy.SameDocumentAndDataOnly);
        loadOptions.PreserveUnknownElements.ShouldBeFalse();
        loadOptions.PreferSvg2Href.ShouldBeTrue();
    }

    [Fact]
    public async Task Http_Svg_Loads_Through_The_Unified_Pipeline_And_Reuses_One_Vector_Image()
    {
        var handler = new SvgResponseHandler(ValidSvg);
        var options = new ImageLoadingOptionsBuilder().Build("svg-codec-tests");
        using var loader = new ImageLoader(options, [new SvgImageCodec()], handler);
        var source = ImageSource.Parse("https://example.com/avatar.svg");

        var firstTask = Task.Run(async () => await loader.LoadAsync(
            new ImageLoadRequest(source) { DecodePixelWidth = 32, DecodePixelHeight = 32 },
            TestContext.Current.CancellationToken));
        WaitUntil(() => firstTask.IsCompleted, "first HTTP SVG load");
        using var first = await firstTask;

        var secondTask = Task.Run(async () => await loader.LoadAsync(
            new ImageLoadRequest(source) { DecodePixelWidth = 512, DecodePixelHeight = 512 },
            TestContext.Current.CancellationToken));
        WaitUntil(() => secondTask.IsCompleted, "second HTTP SVG load");
        using var second = await secondTask;

        first.IsSuccess.ShouldBeTrue();
        second.IsSuccess.ShouldBeTrue();
        second.Origin.ShouldBe(ImageLoadOrigin.DecodedMemory);
        second.Image.ShouldBeSameAs(first.Image);
        first.Image.ShouldNotBeNull().Size.ShouldBe(new Size(48, 32));
        var backgroundSize = await Task.Run(
            () => first.Image.ShouldNotBeNull().Size,
            TestContext.Current.CancellationToken);
        backgroundSize.ShouldBe(new Size(48, 32));
        handler.RequestCount.ShouldBe(1);
    }

    [Fact]
    public async Task Background_Last_Lease_Release_Does_Not_Access_SvgImage_Off_Thread()
    {
        var handler = new SvgResponseHandler(ValidSvg);
        var options = new ImageLoadingOptionsBuilder().Build("svg-release-tests");
        using var loader = new ImageLoader(options, [new SvgImageCodec()], handler);
        var loadTask = Task.Run(async () => await loader.LoadAsync(
            new ImageLoadRequest(ImageSource.Parse("https://example.com/release.svg")),
            TestContext.Current.CancellationToken));
        WaitUntil(() => loadTask.IsCompleted, "SVG release test load");
        var result = await loadTask;
        result.IsSuccess.ShouldBeTrue();

        var clearTask = Task.Run(async () =>
        {
            result.Dispose();
            await loader.ClearCacheAsync(
                new ImageCacheClearRequest(),
                TestContext.Current.CancellationToken);
        }, TestContext.Current.CancellationToken);
        WaitUntil(() => clearTask.IsCompleted, "background SVG release");
        await clearTask;

        Should.NotThrow(() => Dispatcher.UIThread.RunJobs());
    }

    [Fact]
    public async Task Invalid_Model_Is_Reported_As_DecodeFailed()
    {
        var options = new ImageLoadingOptionsBuilder().Build("svg-invalid-model-tests");
        var source = new BytesImageSource("<svg"u8.ToArray(), "invalid-svg");
        var request = ImageCacheKey.Normalize(new ImageLoadRequest(source), options, forceReload: false);
        var content = new ImageEncodedContent(
            "<svg"u8.ToArray(),
            "image/svg+xml",
            ImageLoadOrigin.Local,
            DateTimeOffset.UtcNow).MarkValidated();
        var metadata = new SvgContentMetadata(
            1,
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            16,
            16,
            16,
            16,
            256);
        var probe = new ImageProbeResult(
            ImageContentFormat.Svg,
            "image/svg+xml",
            16,
            16,
            false,
            metadata);

        var exception = await Should.ThrowAsync<ImageLoadFailureException>(() =>
            new SvgImageCodec().DecodeAsync(
                content,
                probe,
                request,
                TestContext.Current.CancellationToken));

        exception.Error.Code.ShouldBe(ImageLoadErrorCode.DecodeFailed);
    }

    private static void WaitUntil(Func<bool> predicate, string description)
    {
        var timeout = Stopwatch.StartNew();
        while (!predicate())
        {
            Dispatcher.UIThread.RunJobs();
            if (timeout.Elapsed >= TimeSpan.FromSeconds(5))
            {
                throw new TimeoutException($"Timed out waiting for {description}.");
            }
            Thread.Yield();
        }
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class SvgResponseHandler(string svg) : HttpMessageHandler
    {
        private readonly byte[] _bytes = Encoding.UTF8.GetBytes(svg);
        private int _requestCount;

        internal int RequestCount => Volatile.Read(ref _requestCount);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _requestCount);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new ByteArrayContent(_bytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/svg+xml");
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromMinutes(5)
            };
            return Task.FromResult(response);
        }
    }
}
