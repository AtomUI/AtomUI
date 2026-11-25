using System.Collections.Frozen;
using System.Security.Cryptography;
using System.Text;

namespace AtomUI.Controls;

internal readonly record struct ImageSourceKey(string Value, string PartitionHash);

internal readonly record struct ImageSourceVersion(string Value)
{
    public static implicit operator ImageSourceVersion(string value) => new(value);

    public override string ToString() => Value;
}

internal readonly record struct ImageContentId(string Value)
{
    internal static ImageContentId Create(ReadOnlySpan<byte> bytes) =>
        new(ImageCacheKey.HashBytes(bytes));

    public override string ToString() => Value;
}

internal readonly record struct ImageEncodedContentKey(string PartitionHash, ImageContentId ContentId);

internal readonly record struct ImageDecodeSpec(int PixelWidth, int PixelHeight, string CodecScope, bool Cover = false);

internal readonly record struct ImageDecodeKey(
    string PartitionHash,
    ImageContentId ContentId,
    ImageDecodeSpec Spec);

internal readonly record struct ImageSourceOperationKey(
    ImageSourceKey SourceKey,
    ImageCacheReadPolicy CacheRead,
    ImageCacheStoragePolicy CacheStorage,
    string ShareScope);

internal readonly record struct ImageDecodedOperationKey(
    ImageDecodeKey DecodeKey,
    ImageCacheStoragePolicy CacheStorage,
    string ShareScope);

internal sealed record NormalizedImageRequest(
    ImageSource Source,
    FrozenDictionary<string, string> Headers,
    ImageCacheReadPolicy CacheRead,
    ImageCacheStoragePolicy CacheStorage,
    string? CachePartition,
    string PartitionHash,
    string? Variant,
    TimeSpan Timeout,
    ImageRequestPriorityState PriorityState,
    int DecodePixelWidth,
    int DecodePixelHeight,
    bool DecodeToCover,
    ImageLoadTimingTracker Timing,
    ImageSourceKey SourceKey,
    ImageSourceOperationKey SourceOperationKey,
    bool HasAuthentication,
    bool CanShare,
    bool CanReadSharedCache,
    bool CanWriteMemory,
    bool CanReadPersistent,
    bool CanPersist)
{
    internal ImageRequestPriority Priority => PriorityState.Priority;
}

internal static class ImageCacheKey
{
    private const string ReaderContract = "reader:1";

    private static readonly FrozenSet<string> s_forbiddenHeaders = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase)
    {
        "Host",
        "Content-Length",
        "Connection",
        "Transfer-Encoding",
        "Upgrade",
        "Range",
        "Proxy-Connection"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    internal static NormalizedImageRequest Normalize(
        ImageLoadRequest request,
        ImageLoadingOptions options,
        bool forceReload)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestOptions = request.Options ?? new ImageRequestOptions();
        var headers = SnapshotHeaders(requestOptions.Headers);
        var hasAuthentication = headers.Keys.Any(options.AuthenticationHeaderNames.Contains);
        var partition = NormalizeOptionalValue(requestOptions.CachePartition, nameof(requestOptions.CachePartition));
        var variant = NormalizeOptionalValue(requestOptions.Variant, nameof(requestOptions.Variant));
        var cacheRead = forceReload ? ImageCacheReadPolicy.RefreshSource : requestOptions.CacheRead;
        var cacheStorage = requestOptions.CacheStorage;
        var privateUnpartitioned = hasAuthentication && partition is null;
        if (privateUnpartitioned)
        {
            cacheStorage = ImageCacheStoragePolicy.None;
        }

        var timeout = requestOptions.Timeout ?? options.DefaultRequestTimeout;
        if (timeout <= TimeSpan.Zero || timeout == Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Image request timeout must be finite and positive.");
        }

        var partitionHash = partition is null ? string.Empty : Hash(partition);
        var timing = new ImageLoadTimingTracker();
        var headerHash = HashHeaders(headers);
        var shareScope = privateUnpartitioned ? Guid.NewGuid().ToString("N") : string.Empty;
        var sourceIdentity = string.Join(
            ':',
            request.Source.CacheIdentity,
            Hash(variant ?? string.Empty),
            partitionHash,
            headerHash,
            ReaderContract);
        var sourceKey = new ImageSourceKey(Hash(sourceIdentity), partitionHash);
        var canUsePersistent = request.Source.CanPersistSourceSnapshot;
        if (hasAuthentication && !options.AllowAuthenticatedPersistentCache)
        {
            canUsePersistent = false;
        }

        return new NormalizedImageRequest(
            request.Source,
            headers,
            cacheRead,
            cacheStorage,
            partition,
            partitionHash,
            variant,
            timeout,
            request.PriorityState ?? new ImageRequestPriorityState(request.Priority),
            request.DecodePixelWidth,
            request.DecodePixelHeight,
            request.DecodeToCover && request.DecodePixelWidth > 0 && request.DecodePixelHeight > 0,
            timing,
            sourceKey,
            new ImageSourceOperationKey(sourceKey, cacheRead, cacheStorage, shareScope),
            hasAuthentication,
            !privateUnpartitioned,
            !privateUnpartitioned,
            !privateUnpartitioned && cacheStorage is not ImageCacheStoragePolicy.None,
            !privateUnpartitioned && canUsePersistent,
            !privateUnpartitioned && canUsePersistent &&
            cacheStorage == ImageCacheStoragePolicy.MemoryAndDisk);
    }

    internal static ImageDecodedOperationKey CreateDecodedOperationKey(
        NormalizedImageRequest request,
        ImageDecodeKey decodeKey) =>
        new(decodeKey, request.CacheStorage, request.SourceOperationKey.ShareScope);

    internal static ImageDecodedOperationKey CreateBorrowedDecodedOperationKey(
        NormalizedImageRequest request)
    {
        var contentId = new ImageContentId(Hash(request.Source.CacheIdentity));
        return CreateDecodedOperationKey(
            request,
            new ImageDecodeKey(
                request.PartitionHash,
                contentId,
                new ImageDecodeSpec(0, 0, "atomui.borrowed-image:1:security:0")));
    }

    internal static string NormalizeOrigin(Uri uri)
    {
        var builder = new UriBuilder(uri)
        {
            Scheme = uri.Scheme.ToLowerInvariant(),
            Host = uri.IdnHost.ToLowerInvariant(),
            Path = string.Empty,
            Query = string.Empty,
            Fragment = string.Empty
        };
        if ((builder.Scheme == "http" && builder.Port == 80) ||
            (builder.Scheme == "https" && builder.Port == 443))
        {
            builder.Port = -1;
        }
        return builder.Uri.GetLeftPart(UriPartial.Authority);
    }

    internal static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    internal static string HashBytes(ReadOnlySpan<byte> value)
    {
        var bytes = SHA256.HashData(value);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    internal static string HashSelectedHeaders(
        IReadOnlyDictionary<string, string> headers,
        IEnumerable<string> names)
    {
        var builder = new StringBuilder();
        foreach (var name in names
                     .Select(name => name.Trim())
                     .Where(name => name.Length > 0)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
        {
            builder.Append(name.ToLowerInvariant()).Append(':');
            builder.Append(headers.TryGetValue(name, out var value) ? Hash(value) : "<absent>");
            builder.Append('\n');
        }
        return Hash(builder.ToString());
    }

    private static FrozenDictionary<string, string> SnapshotHeaders(
        IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null || headers.Count == 0)
        {
            return FrozenDictionary<string, string>.Empty;
        }

        var snapshot = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in headers)
        {
            if (string.IsNullOrWhiteSpace(pair.Key) || s_forbiddenHeaders.Contains(pair.Key))
            {
                throw new ArgumentException($"HTTP header '{pair.Key}' is not allowed.", nameof(headers));
            }
            if (pair.Value.Contains('\r') || pair.Value.Contains('\n'))
            {
                throw new ArgumentException($"HTTP header '{pair.Key}' contains invalid characters.", nameof(headers));
            }
            snapshot[pair.Key] = pair.Value;
        }
        return snapshot.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private static string HashHeaders(FrozenDictionary<string, string> headers)
    {
        if (headers.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var pair in headers.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.Append(pair.Key.ToLowerInvariant())
                .Append(':')
                .Append(Hash(pair.Value))
                .Append('\n');
        }
        return Hash(builder.ToString());
    }

    private static string? NormalizeOptionalValue(string? value, string name)
    {
        if (value is null)
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty or whitespace.", name);
        }
        return value;
    }
}
