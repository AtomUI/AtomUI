using System.Net;
using System.Net.Http.Headers;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Shared.Tests.ImageLoading;

public class HttpImageTransportTests
{
    [Fact]
    public async Task NotModified_Reuses_Stale_Body_And_Merges_Freshness_Metadata()
    {
        var handler = new SequenceHttpMessageHandler(request =>
        {
            request.Headers.IfNoneMatch.Single().Tag.ShouldBe("\"v1\"");
            var response = new HttpResponseMessage(HttpStatusCode.NotModified);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromMinutes(10),
                MustRevalidate = true
            };
            response.Headers.Date = DateTimeOffset.UtcNow;
            response.Headers.ETag = new EntityTagHeaderValue("\"v2\"");
            return response;
        });
        using var transport = new HttpImageTransport(ImageLoadingTestSupport.CreateOptions(), handler);
        var request = Normalize("https://example.com/image.png");
        var stale = ImageLoadingTestSupport.CreateContent(ImageLoadingTestSupport.CreatePngHeader()) with
        {
            ETag = "\"v1\"",
            NoCache = true,
            IsRemote = true
        };

        var result = await transport.FetchAsync(
            request,
            stale,
            null,
            TestContext.Current.CancellationToken);

        result.Bytes.ShouldBeSameAs(stale.Bytes);
        result.SourceValidation.ShouldBe(ImageSourceValidation.Revalidated);
        result.ETag.ShouldBe("\"v2\"");
        result.MustRevalidate.ShouldBeTrue();
        result.FreshUntil.ShouldNotBeNull();
        result.FreshUntil.Value.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Unexpected_NotModified_Without_Body_Retries_Once_As_Full_Request()
    {
        var calls = 0;
        var handler = new SequenceHttpMessageHandler(
            _ =>
            {
                Interlocked.Increment(ref calls);
                return new HttpResponseMessage(HttpStatusCode.NotModified);
            },
            request =>
            {
                Interlocked.Increment(ref calls);
                request.Headers.IfNoneMatch.ShouldBeEmpty();
                return OkResponse();
            });
        using var transport = new HttpImageTransport(ImageLoadingTestSupport.CreateOptions(), handler);

        var result = await transport.FetchAsync(
            Normalize("https://example.com/image.png"),
            null,
            null,
            TestContext.Current.CancellationToken);

        calls.ShouldBe(2);
        result.Origin.ShouldBe(ImageLoadOrigin.Network);
    }

    [Fact]
    public async Task Cross_Origin_Redirect_Strips_Credentials_By_Default()
    {
        var handler = new SequenceHttpMessageHandler(
            request =>
            {
                request.Headers.Contains("Authorization").ShouldBeTrue();
                return RedirectResponse("https://cdn.example.com/image.png");
            },
            request =>
            {
                request.Headers.Contains("Authorization").ShouldBeFalse();
                return OkResponse();
            });
        using var transport = new HttpImageTransport(ImageLoadingTestSupport.CreateOptions(), handler);

        var result = await transport.FetchAsync(
            Normalize(
                "https://origin.example.com/image.png",
                new ImageRequestOptions
                {
                    CachePartition = "account",
                    Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer secret" }
                }),
            null,
            null,
            TestContext.Current.CancellationToken);

        result.Origin.ShouldBe(ImageLoadOrigin.Network);
        handler.CallCount.ShouldBe(2);
    }

    [Fact]
    public async Task Credential_Forwarding_Allowlist_Preserves_Credentials_On_Approved_Redirect()
    {
        var options = ImageLoadingTestSupport.CreateOptions(
            builder => builder.AllowCredentialForwardingOrigin("https://cdn.example.com"));
        var handler = new SequenceHttpMessageHandler(
            _ => RedirectResponse("https://cdn.example.com/image.png"),
            request =>
            {
                request.Headers.GetValues("Authorization").Single().ShouldBe("Bearer secret");
                return OkResponse();
            });
        using var transport = new HttpImageTransport(options, handler);

        await transport.FetchAsync(
            Normalize(
                "https://origin.example.com/image.png",
                new ImageRequestOptions
                {
                    CachePartition = "account",
                    Headers = new Dictionary<string, string> { ["Authorization"] = "Bearer secret" }
                },
                options),
            null,
            null,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Https_To_Http_Redirect_Is_Rejected_Before_Second_Request()
    {
        var handler = new SequenceHttpMessageHandler(
            _ => RedirectResponse("http://example.com/image.png"));
        using var transport = new HttpImageTransport(ImageLoadingTestSupport.CreateOptions(), handler);

        var exception = await Should.ThrowAsync<ImageLoadFailureException>(async () =>
            await transport.FetchAsync(
                Normalize("https://example.com/image.png"),
                null,
                null,
                TestContext.Current.CancellationToken));

        exception.Error.Code.ShouldBe(ImageLoadErrorCode.RedirectBlocked);
        handler.CallCount.ShouldBe(1);
    }

    [Fact]
    public async Task Origin_Allowlist_Blocks_Unapproved_Origin_Without_Sending_Request()
    {
        var options = ImageLoadingTestSupport.CreateOptions(
            builder => builder.AllowHttpOrigin("https://allowed.example.com"));
        var handler = new SequenceHttpMessageHandler(_ => OkResponse());
        using var transport = new HttpImageTransport(options, handler);

        var exception = await Should.ThrowAsync<ImageLoadFailureException>(async () =>
            await transport.FetchAsync(
                Normalize("https://blocked.example.com/image.png", options: options),
                null,
                null,
                TestContext.Current.CancellationToken));

        exception.Error.Code.ShouldBe(ImageLoadErrorCode.OriginNotAllowed);
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task Cache_Control_And_Vary_Metadata_Are_Normalized_Without_Storing_Header_Values()
    {
        const string secretLanguage = "en-US,private-token";
        var handler = new SequenceHttpMessageHandler(_ =>
        {
            var response = OkResponse();
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromMinutes(5),
                Private = true,
                NoCache = true
            };
            response.Headers.Vary.Add("Accept-Language");
            response.Headers.Date = DateTimeOffset.UtcNow;
            response.Headers.Age = TimeSpan.FromSeconds(10);
            return response;
        });
        using var transport = new HttpImageTransport(ImageLoadingTestSupport.CreateOptions(), handler);

        var result = await transport.FetchAsync(
            Normalize(
                "https://example.com/image.png",
                new ImageRequestOptions
                {
                    Headers = new Dictionary<string, string> { ["Accept-Language"] = secretLanguage }
                }),
            null,
            null,
            TestContext.Current.CancellationToken);

        result.NoCache.ShouldBeTrue();
        result.IsPrivate.ShouldBeTrue();
        result.MaxAge.ShouldBe(TimeSpan.FromMinutes(5));
        result.ResponseAge.ShouldBe(TimeSpan.FromSeconds(10));
        result.VaryHeaders.ShouldBe(["Accept-Language"]);
        result.VaryDigest.ShouldNotBeNullOrWhiteSpace();
        result.VaryDigest.ShouldNotContain(secretLanguage);
    }

    [Fact]
    public async Task NoStore_And_VaryStar_Disable_Caching()
    {
        var handler = new SequenceHttpMessageHandler(_ =>
        {
            var response = OkResponse();
            response.Headers.CacheControl = new CacheControlHeaderValue { NoStore = true };
            response.Headers.Vary.Add("*");
            return response;
        });
        using var transport = new HttpImageTransport(ImageLoadingTestSupport.CreateOptions(), handler);

        var result = await transport.FetchAsync(
            Normalize("https://example.com/image.png"),
            null,
            null,
            TestContext.Current.CancellationToken);

        result.NoStore.ShouldBeTrue();
        result.VaryDigest.ShouldBeNull();
    }

    [Fact]
    public async Task Content_Length_Above_Limit_Is_Rejected_Before_Body_Read()
    {
        var options = ImageLoadingTestSupport.CreateOptions(builder => builder.MaxResponseBytes = 4);
        var handler = new SequenceHttpMessageHandler(_ =>
        {
            var response = OkResponse();
            response.Content.Headers.ContentLength = 5;
            return response;
        });
        using var transport = new HttpImageTransport(options, handler);

        var exception = await Should.ThrowAsync<ImageLoadFailureException>(async () =>
            await transport.FetchAsync(
                Normalize("https://example.com/image.png", options: options),
                null,
                null,
                TestContext.Current.CancellationToken));

        exception.Error.Code.ShouldBe(ImageLoadErrorCode.ResponseTooLarge);
    }

    private static NormalizedImageRequest Normalize(
        string source,
        ImageRequestOptions? requestOptions = null,
        ImageLoadingOptions? options = null)
    {
        options ??= ImageLoadingTestSupport.CreateOptions();
        return ImageCacheKey.Normalize(
            new ImageLoadRequest(ImageSource.Parse(source)) { Options = requestOptions },
            options,
            forceReload: false);
    }

    private static HttpResponseMessage OkResponse()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(ImageLoadingTestSupport.CreatePngHeader())
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        return response;
    }

    private static HttpResponseMessage RedirectResponse(string location)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Redirect);
        response.Headers.Location = new Uri(location);
        return response;
    }

    private sealed class SequenceHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses;
        private int _callCount;

        internal SequenceHttpMessageHandler(
            params Func<HttpRequestMessage, HttpResponseMessage>[] responses)
        {
            _responses = new Queue<Func<HttpRequestMessage, HttpResponseMessage>>(responses);
        }

        internal int CallCount => Volatile.Read(ref _callCount);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _callCount);
            var response = _responses.Dequeue()(request);
            response.RequestMessage ??= request;
            return Task.FromResult(response);
        }
    }
}
