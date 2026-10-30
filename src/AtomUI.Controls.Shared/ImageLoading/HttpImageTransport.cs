using System.Net;

namespace AtomUI.Controls;

internal sealed class HttpImageTransport : IDisposable
{
    private static readonly HttpRequestOptionsKey<IDictionary<string, object>> s_browserFetchOptionsKey =
        new("WebAssemblyFetchOptions");

    private static readonly IDictionary<string, object> s_browserFetchOptions =
        new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["credentials"] = "omit"
        };

    private static readonly HashSet<HttpStatusCode> s_redirectStatusCodes =
    [
        HttpStatusCode.Moved,
        HttpStatusCode.Redirect,
        HttpStatusCode.RedirectMethod,
        HttpStatusCode.TemporaryRedirect,
        HttpStatusCode.PermanentRedirect
    ];

    private readonly ImageLoadingOptions _options;
    private readonly HttpClient _client;
    private bool _disposed;

    internal HttpImageTransport(ImageLoadingOptions options, HttpMessageHandler? handler = null)
    {
        _options = options;
        if (handler is null)
        {
            handler = OperatingSystem.IsBrowser()
                ? new HttpClientHandler()
                : new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    UseCookies = false
                };
        }
        _client = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    internal async Task<ImageEncodedContent> FetchAsync(
        NormalizedImageRequest request,
        ImageEncodedContent? staleContent,
        IProgress<ImageLoadProgress>? progress,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var initialUri = ((HttpImageSource)request.Source).Uri;
        ValidateUri(initialUri, request.Source.DisplayName);
        var currentUri = initialUri;
        var initialOrigin = ImageCacheKey.NormalizeOrigin(initialUri);
        var redirectCount = 0;
        var retriedUnexpectedNotModified = false;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateOrigin(currentUri, request.Source.DisplayName);
            using var message = new HttpRequestMessage(HttpMethod.Get, currentUri);
            if (OperatingSystem.IsBrowser())
            {
                message.Options.Set(s_browserFetchOptionsKey, s_browserFetchOptions);
            }
            AddHeaders(message, request, initialOrigin, ImageCacheKey.NormalizeOrigin(currentUri));
            if (staleContent is not null)
            {
                if (!string.IsNullOrWhiteSpace(staleContent.ETag))
                {
                    message.Headers.TryAddWithoutValidation("If-None-Match", staleContent.ETag);
                }
                else if (staleContent.LastModified is not null)
                {
                    message.Headers.IfModifiedSince = staleContent.LastModified;
                }
            }

            HttpResponseMessage response;
            try
            {
                response = await _client.SendAsync(
                    message,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException exception)
            {
                throw ImageSourceReadHelpers.Failure(
                    ImageLoadErrorCode.NetworkFailure,
                    "The image request failed.",
                    request.Source.DisplayName,
                    exception);
            }

            using (response)
            {
                var visibleFinalUri = response.RequestMessage?.RequestUri ?? currentUri;
                ValidateUri(visibleFinalUri, request.Source.DisplayName);
                ValidateOrigin(visibleFinalUri, request.Source.DisplayName);
                if (currentUri.Scheme == Uri.UriSchemeHttps && visibleFinalUri.Scheme == Uri.UriSchemeHttp)
                {
                    throw ImageSourceReadHelpers.Failure(
                        ImageLoadErrorCode.RedirectBlocked,
                        "HTTPS image requests cannot redirect to HTTP.",
                        request.Source.DisplayName);
                }

                if (!OperatingSystem.IsBrowser() && s_redirectStatusCodes.Contains(response.StatusCode))
                {
                    if (redirectCount >= _options.MaxRedirects)
                    {
                        throw ImageSourceReadHelpers.Failure(
                            ImageLoadErrorCode.TooManyRedirects,
                            "The image request exceeded the redirect limit.",
                            request.Source.DisplayName);
                    }
                    if (response.Headers.Location is null)
                    {
                        throw ImageSourceReadHelpers.Failure(
                            ImageLoadErrorCode.InvalidSource,
                            "The image redirect did not include a target URI.",
                            request.Source.DisplayName);
                    }
                    var next = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location
                        : new Uri(currentUri, response.Headers.Location);
                    ValidateUri(next, request.Source.DisplayName);
                    if (currentUri.Scheme == Uri.UriSchemeHttps && next.Scheme == Uri.UriSchemeHttp)
                    {
                        throw ImageSourceReadHelpers.Failure(
                            ImageLoadErrorCode.RedirectBlocked,
                            "HTTPS image requests cannot redirect to HTTP.",
                            request.Source.DisplayName);
                    }
                    redirectCount++;
                    currentUri = next;
                    continue;
                }

                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    if (staleContent is null)
                    {
                        if (!retriedUnexpectedNotModified)
                        {
                            retriedUnexpectedNotModified = true;
                            continue;
                        }
                        throw ImageSourceReadHelpers.Failure(
                            ImageLoadErrorCode.InvalidImageData,
                            "The image server returned an invalid cache response.",
                            request.Source.DisplayName);
                    }
                    return MergeNotModified(staleContent, response, request);
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw ImageSourceReadHelpers.Failure(
                        ImageLoadErrorCode.HttpStatus,
                        "The image server returned an unsuccessful status.",
                        request.Source.DisplayName,
                        httpStatus: (int)response.StatusCode);
                }

                var contentLength = response.Content.Headers.ContentLength;
                if (contentLength > _options.MaxResponseBytes)
                {
                    throw ImageSourceReadHelpers.Failure(
                        ImageLoadErrorCode.ResponseTooLarge,
                        "Image content exceeds the configured size limit.",
                        request.Source.DisplayName);
                }
                ImageProgressDispatcher.Report(progress, ImageLoadProgress.Create(ImageLoadStage.Downloading));
                try
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                    var bytes = await ImageSourceReadHelpers.ReadAllBytesAsync(
                        stream,
                        _options.MaxResponseBytes,
                        ImageLoadStage.Downloading,
                        progress,
                        contentLength,
                        cancellationToken).ConfigureAwait(false);
                    return CreateContent(bytes, response, request);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (HttpRequestException exception)
                {
                    throw ImageSourceReadHelpers.Failure(
                        ImageLoadErrorCode.NetworkFailure,
                        "The image response could not be read.",
                        request.Source.DisplayName,
                        exception);
                }
                catch (IOException exception)
                {
                    throw ImageSourceReadHelpers.Failure(
                        ImageLoadErrorCode.NetworkFailure,
                        "The image response could not be read.",
                        request.Source.DisplayName,
                        exception);
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _client.Dispose();
    }

    private void ValidateUri(Uri uri, string? displayName)
    {
        if (!uri.IsAbsoluteUri ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            string.IsNullOrWhiteSpace(uri.Host) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            uri.AbsoluteUri.Length > 4096)
        {
            throw ImageSourceReadHelpers.Failure(
                ImageLoadErrorCode.InvalidSource,
                "The image URI is not valid for HTTP loading.",
                displayName);
        }
    }

    private void ValidateOrigin(Uri uri, string? displayName)
    {
        if (_options.AllowedHttpOrigins.Count == 0)
        {
            return;
        }
        var origin = ImageCacheKey.NormalizeOrigin(uri);
        if (!_options.AllowedHttpOrigins.Contains(origin))
        {
            throw ImageSourceReadHelpers.Failure(
                ImageLoadErrorCode.OriginNotAllowed,
                "The image origin is not allowed by application policy.",
                displayName);
        }
    }

    private void AddHeaders(
        HttpRequestMessage message,
        NormalizedImageRequest request,
        string initialOrigin,
        string currentOrigin)
    {
        var canForwardCredentials = initialOrigin.Equals(currentOrigin, StringComparison.OrdinalIgnoreCase) ||
            _options.CredentialForwardingOrigins.Contains(currentOrigin);
        foreach (var pair in request.Headers)
        {
            if (!canForwardCredentials && _options.AuthenticationHeaderNames.Contains(pair.Key))
            {
                continue;
            }
            message.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
        }
    }

    private static ImageEncodedContent CreateContent(
        byte[] bytes,
        HttpResponseMessage response,
        NormalizedImageRequest request)
    {
        var receivedAt = DateTimeOffset.UtcNow;
        var cacheControl = response.Headers.CacheControl;
        var age = response.Headers.Age ?? TimeSpan.Zero;
        var responseDate = response.Headers.Date;
        var expires = response.Content.Headers.Expires;
        var maxAge = cacheControl?.MaxAge;
        var freshUntil = CalculateFreshUntil(receivedAt, responseDate, age, maxAge, expires);
        var varyHeaders = response.Headers.Vary
            .Select(header => header.Trim())
            .Where(header => header.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(header => header, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var noStore = cacheControl?.NoStore == true || varyHeaders.Contains("*", StringComparer.Ordinal);
        return new ImageEncodedContent(
            bytes,
            response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant(),
            ImageLoadOrigin.Network,
            receivedAt,
            freshUntil,
            response.Headers.ETag?.ToString(),
            response.Content.Headers.LastModified,
            noStore,
            cacheControl?.NoCache == true,
            cacheControl?.MustRevalidate == true,
            IsRemote: true,
            VaryHeaders: varyHeaders,
            VaryDigest: varyHeaders.Length == 0 || noStore
                ? null
                : ImageCacheKey.HashSelectedHeaders(request.Headers, varyHeaders),
            ResponseDate: responseDate,
            ResponseAge: age,
            Expires: expires,
            MaxAge: maxAge,
            IsPrivate: cacheControl?.Private == true);
    }

    private static ImageEncodedContent MergeNotModified(
        ImageEncodedContent staleContent,
        HttpResponseMessage response,
        NormalizedImageRequest request)
    {
        var metadata = CreateContent(staleContent.Bytes, response, request);
        var hasCacheControl = response.Headers.CacheControl is not null;
        var hasVary = response.Headers.Vary.Any();
        var responseDate = metadata.ResponseDate ?? staleContent.ResponseDate;
        var responseAge = metadata.ResponseAge ?? staleContent.ResponseAge ?? TimeSpan.Zero;
        var expires = metadata.Expires ?? staleContent.Expires;
        var maxAge = metadata.MaxAge ?? staleContent.MaxAge;
        return staleContent with
        {
            SourceValidation = ImageSourceValidation.Revalidated,
            StoredAt = metadata.StoredAt,
            FreshUntil = CalculateFreshUntil(
                metadata.StoredAt,
                responseDate,
                responseAge,
                maxAge,
                expires),
            ETag = metadata.ETag ?? staleContent.ETag,
            LastModified = metadata.LastModified ?? staleContent.LastModified,
            NoStore = hasCacheControl || hasVary ? metadata.NoStore : staleContent.NoStore,
            NoCache = hasCacheControl ? metadata.NoCache : staleContent.NoCache,
            MustRevalidate = hasCacheControl ? metadata.MustRevalidate : staleContent.MustRevalidate,
            MediaType = metadata.MediaType ?? staleContent.MediaType,
            VaryHeaders = hasVary ? metadata.VaryHeaders : staleContent.VaryHeaders,
            VaryDigest = hasVary ? metadata.VaryDigest : staleContent.VaryDigest,
            ResponseDate = responseDate,
            ResponseAge = responseAge,
            Expires = expires,
            MaxAge = maxAge,
            IsPrivate = hasCacheControl ? metadata.IsPrivate : staleContent.IsPrivate
        };
    }

    private static DateTimeOffset? CalculateFreshUntil(
        DateTimeOffset receivedAt,
        DateTimeOffset? responseDate,
        TimeSpan age,
        TimeSpan? maxAge,
        DateTimeOffset? expires)
    {
        if (maxAge is { } lifetime)
        {
            var apparentAge = responseDate is { } date && receivedAt > date
                ? receivedAt - date
                : TimeSpan.Zero;
            var currentAge = age > apparentAge ? age : apparentAge;
            return receivedAt + (lifetime > currentAge ? lifetime - currentAge : TimeSpan.Zero);
        }
        return expires;
    }
}
