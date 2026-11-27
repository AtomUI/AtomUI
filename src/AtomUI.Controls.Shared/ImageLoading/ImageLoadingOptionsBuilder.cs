using System.Collections.Frozen;

namespace AtomUI.Controls;

/// <summary>
/// Configures the image loader of the application inside <c>UseImageLoading()</c>.
/// </summary>
/// <remarks>
/// The property values are read and validated once, when the loader is created at application startup; a value
/// outside its valid range throws <see cref="InvalidOperationException" /> at that point, and enabling the disk
/// cache in a Browser application throws <see cref="PlatformNotSupportedException" />.
/// </remarks>
public sealed class ImageLoadingOptionsBuilder
{
    private readonly HashSet<string> _authenticationHeaderNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization",
            "Proxy-Authorization",
            "Cookie"
        };
    private readonly HashSet<string> _allowedHttpOrigins = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _credentialForwardingOrigins = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The limits and the conformance mode for SVG documents.
    /// </summary>
    public SvgImageLoadingOptionsBuilder Svg { get; } = new();

    /// <summary>
    /// The largest number of HTTP image downloads that run at the same time. Local sources use a separate pool,
    /// so a slow download never blocks them. The valid range is 1 to 32, or 1 to 8 in Browser applications.
    /// </summary>
    public int MaxConcurrentDownloads { get; set; } = OperatingSystem.IsBrowser() ? 4 : 6;

    internal int MaxConcurrentLocalReads { get; set; } = OperatingSystem.IsBrowser() ? 4 : 16;

    /// <summary>
    /// The largest number of images that are decoded at the same time. The valid range is 1 to 8, or 1 to 4 in
    /// Browser applications.
    /// </summary>
    public int MaxConcurrentDecodes { get; set; } = OperatingSystem.IsBrowser()
        ? 2
        : Math.Min(4, Math.Max(1, Environment.ProcessorCount - 1));

    /// <summary>
    /// The memory budget in bytes for the encoded image bytes that are cached. When the budget or the entry
    /// count is exceeded, the least recently used entries are evicted. It must be greater than zero.
    /// </summary>
    public long EncodedMemoryCacheBytes { get; set; } = OperatingSystem.IsBrowser()
        ? 32L * 1024 * 1024
        : 128L * 1024 * 1024;

    /// <summary>
    /// The largest number of entries in the memory cache of encoded bytes. It must be greater than zero.
    /// </summary>
    public int EncodedMemoryCacheEntries { get; set; } = OperatingSystem.IsBrowser() ? 128 : 256;

    /// <summary>
    /// The memory budget in bytes for decoded images that are cached; a raster image counts as
    /// <c>width * height * 4</c> bytes. When the budget or the entry count is exceeded, the least recently used
    /// entries are evicted. It must be greater than zero.
    /// </summary>
    public long DecodedMemoryCacheBytes { get; set; } = OperatingSystem.IsBrowser()
        ? 64L * 1024 * 1024
        : 256L * 1024 * 1024;

    /// <summary>
    /// The largest number of entries in the memory cache of decoded images. It must be greater than zero.
    /// </summary>
    public int DecodedMemoryCacheEntries { get; set; } = OperatingSystem.IsBrowser() ? 128 : 256;

    /// <summary>
    /// The largest size in bytes of one encoded image, whether it is downloaded or read locally. A larger image
    /// fails with <see cref="ImageLoadErrorCode.ResponseTooLarge" />. It must be greater than zero.
    /// </summary>
    public long MaxResponseBytes { get; set; } = OperatingSystem.IsBrowser()
        ? 16L * 1024 * 1024
        : 32L * 1024 * 1024;

    /// <summary>
    /// The largest width in pixels of an image. A wider image fails with
    /// <see cref="ImageLoadErrorCode.DimensionLimitExceeded" />. It must be greater than zero.
    /// </summary>
    public int MaxImageWidth { get; set; } = OperatingSystem.IsBrowser() ? 8_192 : 16_384;

    /// <summary>
    /// The largest height in pixels of an image. A taller image fails with
    /// <see cref="ImageLoadErrorCode.DimensionLimitExceeded" />. It must be greater than zero.
    /// </summary>
    public int MaxImageHeight { get; set; } = OperatingSystem.IsBrowser() ? 8_192 : 16_384;

    /// <summary>
    /// The largest number of pixels in an image. An image with more pixels fails with
    /// <see cref="ImageLoadErrorCode.PixelLimitExceeded" />. It must be greater than zero.
    /// </summary>
    public long MaxImagePixelCount { get; set; } = OperatingSystem.IsBrowser() ? 32_000_000 : 64_000_000;

    /// <summary>
    /// The largest size in bytes of a decoded image, computed as the pixel count times 4. A larger image fails
    /// with <see cref="ImageLoadErrorCode.DecodedByteLimitExceeded" />. It must be greater than zero.
    /// </summary>
    public long MaxDecodedImageBytes { get; set; } = OperatingSystem.IsBrowser()
        ? 128L * 1024 * 1024
        : 256L * 1024 * 1024;

    /// <summary>
    /// The timeout of a load that sets no <see cref="ImageRequestOptions.Timeout" />. It must be a finite
    /// positive duration.
    /// </summary>
    public TimeSpan DefaultRequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The largest number of redirects an HTTP download may follow. A longer chain fails with
    /// <see cref="ImageLoadErrorCode.TooManyRedirects" />, and 0 rejects every redirect. The valid range is 0 to 32.
    /// In Browser applications the platform follows the redirects, so this limit is not applied.
    /// </summary>
    public int MaxRedirects { get; set; } = 8;

    /// <summary>
    /// <see langword="true" /> to store encoded image bytes on disk so that they survive an application restart.
    /// The disk cache is not available in Browser applications, where enabling it fails when the loader is
    /// created.
    /// </summary>
    public bool IsPersistentCacheEnabled { get; set; }

    /// <summary>
    /// The directory of the disk cache, or <see langword="null" /> or white space for a default location: a
    /// subfolder of the <c>image-cache</c> folder of the application, named by the first 16 characters of a hash of
    /// the application name. The application folder lies under the local application data folder, or under the
    /// temporary folder when the local application data folder is unavailable.
    /// </summary>
    public string? PersistentCacheDirectory { get; set; }

    /// <summary>
    /// The budget in bytes of the disk cache. When the budget or the entry count is exceeded, the least recently
    /// used content is deleted. It must be greater than zero.
    /// </summary>
    public long PersistentCacheBytes { get; set; } = 512L * 1024 * 1024;

    /// <summary>
    /// The largest number of entries in the disk cache. It must be greater than zero.
    /// </summary>
    public int PersistentCacheEntries { get; set; } = 2048;

    /// <summary>
    /// <see langword="true" /> to let requests that carry an authentication header read and write the disk
    /// cache. While it is <see langword="false" />, such requests neither read nor write the disk cache. Either
    /// value leaves a request that has no <see cref="ImageRequestOptions.CachePartition" /> isolated as described
    /// on <see cref="ImageRequestOptions" />.
    /// </summary>
    public bool AllowAuthenticatedPersistentCache { get; set; }

    /// <summary>
    /// Adds a header name that the loader treats as authentication context, besides <c>Authorization</c>,
    /// <c>Proxy-Authorization</c> and <c>Cookie</c>. Such a header is not forwarded to another origin on
    /// redirect, keeps its response out of the disk cache unless
    /// <see cref="AllowAuthenticatedPersistentCache" /> is set, and isolates the request as described on
    /// <see cref="ImageRequestOptions" />.
    /// </summary>
    /// <param name="name">
    /// The header name, compared without case.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="name" /> is blank or is not a valid HTTP header name.
    /// </exception>
    public void AddAuthenticationHeaderName(string name)
    {
        ValidateHeaderName(name);
        _authenticationHeaderNames.Add(name);
    }

    /// <summary>
    /// Adds an HTTP or HTTPS origin to the allow list of the application. While the list is empty every origin
    /// is allowed; once it has an entry, only its origins may be requested, and every redirect target must be on
    /// it too. Another origin fails with <see cref="ImageLoadErrorCode.OriginNotAllowed" />.
    /// </summary>
    /// <param name="origin">
    /// A scheme, a host and an optional port, such as <c>https://example.com</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="origin" /> is blank or contains more than a scheme, a host and a port.
    /// </exception>
    public void AllowHttpOrigin(string origin)
    {
        _allowedHttpOrigins.Add(NormalizeOrigin(origin));
    }

    /// <summary>
    /// Adds an origin to which authentication headers are forwarded when a download is redirected away from its
    /// original origin.
    /// </summary>
    /// <param name="origin">
    /// A scheme, a host and an optional port, such as <c>https://example.com</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="origin" /> is blank or contains more than a scheme, a host and a port.
    /// </exception>
    public void AllowCredentialForwardingOrigin(string origin)
    {
        _credentialForwardingOrigins.Add(NormalizeOrigin(origin));
    }

    internal ImageLoadingOptions Build(string? applicationName)
    {
        Validate();
        applicationName = string.IsNullOrWhiteSpace(applicationName) ? "AtomUI" : applicationName;
        var persistentCacheDirectory = PersistentCacheDirectory;
        if (IsPersistentCacheEnabled && string.IsNullOrWhiteSpace(persistentCacheDirectory))
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(root))
            {
                root = Path.GetTempPath();
            }
            persistentCacheDirectory = Path.Combine(
                root,
                applicationName,
                "image-cache",
                ImageCacheKey.Hash(applicationName)[..16]);
        }
        return new ImageLoadingOptions(
            MaxConcurrentDownloads,
            MaxConcurrentLocalReads,
            MaxConcurrentDecodes,
            EncodedMemoryCacheBytes,
            EncodedMemoryCacheEntries,
            DecodedMemoryCacheBytes,
            DecodedMemoryCacheEntries,
            MaxResponseBytes,
            MaxImageWidth,
            MaxImageHeight,
            MaxImagePixelCount,
            MaxDecodedImageBytes,
            DefaultRequestTimeout,
            MaxRedirects,
            IsPersistentCacheEnabled,
            persistentCacheDirectory,
            PersistentCacheBytes,
            PersistentCacheEntries,
            AllowAuthenticatedPersistentCache,
            _authenticationHeaderNames.ToFrozenSet(StringComparer.OrdinalIgnoreCase),
            _allowedHttpOrigins.ToFrozenSet(StringComparer.OrdinalIgnoreCase),
            _credentialForwardingOrigins.ToFrozenSet(StringComparer.OrdinalIgnoreCase),
            Svg.Build());
    }

    private void Validate()
    {
        var downloadCap = OperatingSystem.IsBrowser() ? 8 : 32;
        var decodeCap = OperatingSystem.IsBrowser() ? 4 : 8;
        ValidateRange(MaxConcurrentDownloads, 1, downloadCap, nameof(MaxConcurrentDownloads));
        ValidateRange(MaxConcurrentLocalReads, 1, 32, nameof(MaxConcurrentLocalReads));
        ValidateRange(MaxConcurrentDecodes, 1, decodeCap, nameof(MaxConcurrentDecodes));
        ValidatePositive(EncodedMemoryCacheBytes, nameof(EncodedMemoryCacheBytes));
        ValidatePositive(EncodedMemoryCacheEntries, nameof(EncodedMemoryCacheEntries));
        ValidatePositive(DecodedMemoryCacheBytes, nameof(DecodedMemoryCacheBytes));
        ValidatePositive(DecodedMemoryCacheEntries, nameof(DecodedMemoryCacheEntries));
        ValidatePositive(MaxResponseBytes, nameof(MaxResponseBytes));
        ValidatePositive(MaxImageWidth, nameof(MaxImageWidth));
        ValidatePositive(MaxImageHeight, nameof(MaxImageHeight));
        ValidatePositive(MaxImagePixelCount, nameof(MaxImagePixelCount));
        ValidatePositive(MaxDecodedImageBytes, nameof(MaxDecodedImageBytes));
        ValidatePositive(PersistentCacheBytes, nameof(PersistentCacheBytes));
        ValidatePositive(PersistentCacheEntries, nameof(PersistentCacheEntries));
        ValidateRange(MaxRedirects, 0, 32, nameof(MaxRedirects));
        if (DefaultRequestTimeout <= TimeSpan.Zero || DefaultRequestTimeout == Timeout.InfiniteTimeSpan)
        {
            throw new InvalidOperationException("DefaultRequestTimeout must be a finite positive duration.");
        }
        if (OperatingSystem.IsBrowser() && IsPersistentCacheEnabled)
        {
            throw new PlatformNotSupportedException("Persistent image cache is not available in Browser applications.");
        }
    }

    private static void ValidateHeaderName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        const string separators = "()<>@,;:/[]?={}\t";
        if (name.Any(character =>
                character <= 32 || character >= 127 || character == '\\' || character == '"' ||
                separators.Contains(character)))
        {
            throw new ArgumentException("Invalid HTTP header name.", nameof(name));
        }
    }

    private static string NormalizeOrigin(string origin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(origin);
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            uri.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ArgumentException("Origin must contain only an HTTP scheme, host, and optional port.", nameof(origin));
        }

        return ImageCacheKey.NormalizeOrigin(uri);
    }

    private static void ValidatePositive(long value, string name)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException($"{name} must be greater than zero.");
        }
    }

    private static void ValidateRange(int value, int minimum, int maximum, string name)
    {
        if (value < minimum || value > maximum)
        {
            throw new InvalidOperationException($"{name} must be between {minimum} and {maximum}.");
        }
    }
}
