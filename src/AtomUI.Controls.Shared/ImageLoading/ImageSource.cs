using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace AtomUI.Controls;

/// <summary>
/// The base class of the sources an image is loaded from: a download, a file, an application asset, a
/// storage file, a byte array, a stream or an image the caller already has. Create one of the sealed
/// subclasses, or convert a string with <see cref="Parse" />.
/// </summary>
/// <remarks>
/// The class is closed: external code cannot derive from it, so these are the only source kinds.
/// </remarks>
[TypeConverter(typeof(ImageSourceConverter))]
public abstract class ImageSource
{
    private static readonly ConditionalWeakTable<object, IdentityHolder> s_objectIdentities = new();
    private static long s_nextObjectIdentity;

    internal ImageSource(string? displayName)
    {
        DisplayName = displayName;
    }

    /// <summary>
    /// A short name of the source for diagnostics, such as a file name, or <see langword="null" /> when none was
    /// derived or passed.
    /// </summary>
    public string? DisplayName { get; }

    internal abstract ImageSourceKind Kind { get; }

    internal abstract string CacheIdentity { get; }

    internal virtual ImageSourceVersion? SourceRevision => null;

    internal virtual bool CanPersistSourceSnapshot => false;

    /// <summary>
    /// Converts the text of a source into an <see cref="ImageSource" />, as XAML does.
    /// </summary>
    /// <param name="source">
    /// A fully qualified path, or a URI with the <c>http</c>, <c>https</c>, <c>avares</c> or <c>file</c> scheme.
    /// </param>
    /// <returns>
    /// The matching <see cref="FileImageSource" />, <see cref="HttpImageSource" /> or
    /// <see cref="AssetImageSource" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="source" /> is <see langword="null" />, empty or white space.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="source" /> is not a supported image source.
    /// </exception>
    public static ImageSource Parse(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        if (!TryParse(source, out var result))
        {
            throw new FormatException($"'{source}' is not a supported image source.");
        }
        return result!;
    }

    /// <summary>
    /// Tries to convert the text of a source into an <see cref="ImageSource" />.
    /// </summary>
    /// <param name="source">
    /// The text to convert; <see cref="Parse" /> lists the accepted forms.
    /// </param>
    /// <param name="result">
    /// The converted source, or <see langword="null" /> when the method returns <see langword="false" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the text was converted; <see langword="false" /> when it is blank, has an
    /// unknown form or scheme, or is a URI that the constructor of the matching source rejects.
    /// </returns>
    public static bool TryParse(string? source, out ImageSource? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }
        if (Path.IsPathFullyQualified(source))
        {
            result = new FileImageSource(source);
            return true;
        }
        if (!Uri.TryCreate(source, UriKind.Absolute, out var uri))
        {
            return false;
        }
        try
        {
            result = uri.Scheme.ToLowerInvariant() switch
            {
                "http" or "https" => new HttpImageSource(uri),
                "avares" => new AssetImageSource(uri),
                "file" => new FileImageSource(uri.LocalPath),
                _ => null
            };
            return result is not null;
        }
        catch (ArgumentException)
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Returns <see cref="DisplayName" />, or the cache identity of the source when it has no display name.
    /// </summary>
    public override string ToString() => DisplayName ?? CacheIdentity;

    internal static long GetObjectIdentity(object value)
    {
        return s_objectIdentities.GetValue(
            value,
            static _ => new IdentityHolder(Interlocked.Increment(ref s_nextObjectIdentity))).Value;
    }

    internal static void ValidateOptionalValue(string? value, string parameterName)
    {
        if (value is not null && string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty or whitespace.", parameterName);
        }
    }

    internal static string? ResolveUriDisplayName(Uri uri)
    {
        var segments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.UnescapeDataString)
            .ToArray();
        if (segments.Length == 0)
        {
            return null;
        }
        return segments.Length >= 3 &&
               segments[^1].Length > 0 && segments[^1].All(char.IsDigit) &&
               segments[^2].Length > 0 && segments[^2].All(char.IsDigit)
            ? segments[^3]
            : segments[^1];
    }

    private sealed record IdentityHolder(long Value);
}

/// <summary>
/// An image that the loader downloads with an HTTP GET request. The response is cached under the caching
/// headers of the server.
/// </summary>
public sealed class HttpImageSource : ImageSource
{
    /// <summary>
    /// Creates a source for <paramref name="uri" />.
    /// </summary>
    /// <param name="uri">
    /// An absolute HTTP or HTTPS URI with a host and without user information.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="uri" /> does not meet these conditions.
    /// </exception>
    public HttpImageSource(Uri uri)
        : this(Normalize(uri), true)
    {
    }

    private HttpImageSource(Uri uri, bool _)
        : base(ResolveUriDisplayName(uri))
    {
        Uri = uri;
    }

    /// <summary>
    /// The URI that is requested. Its scheme and host are in lower case, the fragment is removed and a default
    /// port is omitted.
    /// </summary>
    public Uri Uri { get; }

    internal override ImageSourceKind Kind => ImageSourceKind.Http;

    internal override string CacheIdentity => $"http:{Uri.AbsoluteUri}";

    internal override bool CanPersistSourceSnapshot => true;

    private static Uri Normalize(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!uri.IsAbsoluteUri ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new ArgumentException(
                "HTTP image source must be an absolute HTTP or HTTPS URI with a host and no credentials.",
                nameof(uri));
        }
        var builder = new UriBuilder(uri)
        {
            Scheme = uri.Scheme.ToLowerInvariant(),
            Host = new IdnMapping().GetAscii(uri.Host).ToLowerInvariant(),
            Fragment = string.Empty
        };
        if ((builder.Scheme == Uri.UriSchemeHttp && builder.Port == 80) ||
            (builder.Scheme == Uri.UriSchemeHttps && builder.Port == 443))
        {
            builder.Port = -1;
        }
        return builder.Uri;
    }
}

/// <summary>
/// An image that is read from a file on disk. Direct file access is not available in Browser applications,
/// where a load fails with <see cref="ImageLoadErrorCode.AccessDenied" />.
/// </summary>
public sealed class FileImageSource : ImageSource
{
    /// <summary>
    /// Creates a source for <paramref name="path" />.
    /// </summary>
    /// <param name="path">
    /// The path of the file, relative to the current working directory or absolute.
    /// </param>
    /// <param name="validation">
    /// How the loader detects that the file changed.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="path" /> is <see langword="null" />, empty or white space.
    /// </exception>
    public FileImageSource(
        string path,
        ImageFileValidationMode validation = ImageFileValidationMode.Metadata)
        : base(GetDisplayName(path))
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Path = System.IO.Path.GetFullPath(path);
        Validation = validation;
    }

    /// <summary>
    /// The absolute path of the file.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// How the loader checks that a cached copy of the file is still current when the file is loaded again.
    /// </summary>
    public ImageFileValidationMode Validation { get; }

    internal override ImageSourceKind Kind => ImageSourceKind.File;

    internal override string CacheIdentity => $"file:{NormalizePath(Path)}:{Validation}";

    internal override bool CanPersistSourceSnapshot => true;

    private static string GetDisplayName(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return System.IO.Path.GetFileName(System.IO.Path.GetFullPath(path));
    }

    private static string NormalizePath(string path) =>
        OperatingSystem.IsWindows() ? path.ToUpperInvariant() : path;
}

/// <summary>
/// An image that comes from an application asset. Assets are treated as unchanging, so after the first load
/// the cached image is reused without reading the asset again, unless a load forces a refresh.
/// </summary>
public sealed class AssetImageSource : ImageSource
{
    /// <summary>
    /// Creates a source for <paramref name="uri" />.
    /// </summary>
    /// <param name="uri">
    /// An absolute <c>avares</c> URI with a host, without user information, query or fragment.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="uri" /> does not meet these conditions.
    /// </exception>
    public AssetImageSource(Uri uri)
        : this(Normalize(uri), true)
    {
    }

    private AssetImageSource(Uri uri, bool _)
        : base(ResolveUriDisplayName(uri))
    {
        Uri = uri;
    }

    /// <summary>
    /// The URI of the asset.
    /// </summary>
    public Uri Uri { get; }

    internal override ImageSourceKind Kind => ImageSourceKind.Asset;

    internal override string CacheIdentity => $"asset:{Uri.AbsoluteUri}";

    internal override ImageSourceVersion? SourceRevision => new("application-resource");

    internal override bool CanPersistSourceSnapshot => true;

    private static Uri Normalize(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!uri.IsAbsoluteUri ||
            !uri.Scheme.Equals("avares", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(uri.Host) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ArgumentException(
                "Asset source must be an absolute avares URI with an assembly host and no credentials, query, or fragment.",
                nameof(uri));
        }
        return new Uri(uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped));
    }
}

/// <summary>
/// An image that is read through a file of a storage provider.
/// </summary>
public sealed class StorageFileImageSource : ImageSource
{
    /// <summary>
    /// Creates a source that reads <paramref name="file" />.
    /// </summary>
    /// <param name="file">
    /// The storage file to read.
    /// </param>
    /// <param name="revision">
    /// A token that changes whenever the content of the file changes, or <see langword="null" /> when the caller
    /// cannot supply one.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="file" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="revision" /> is empty or white space.
    /// </exception>
    public StorageFileImageSource(IStorageFile file, string? revision = null)
        : base((file ?? throw new ArgumentNullException(nameof(file))).Name)
    {
        ValidateOptionalValue(revision, nameof(revision));
        File = file;
        Revision = revision;
    }

    /// <summary>
    /// The storage file that is read.
    /// </summary>
    public IStorageFile File { get; }

    /// <summary>
    /// A token that identifies the content of the file. While it stays the same, a cached copy is reused instead
    /// of reading the file and the content may enter the disk cache. Without it the file is read on every load
    /// and stays out of the disk cache.
    /// </summary>
    public string? Revision { get; }

    internal override ImageSourceKind Kind => ImageSourceKind.StorageFile;

    internal override string CacheIdentity => Revision is not null
        ? $"storage:uri:{File.Path.AbsoluteUri}"
        : $"storage:object:{GetObjectIdentity(File)}";

    internal override ImageSourceVersion? SourceRevision => Revision is null ? null : new(Revision);

    internal override bool CanPersistSourceSnapshot => Revision is not null;
}

/// <summary>
/// An image whose encoded bytes are held in memory. The bytes are identified by a hash of their content, so
/// equal byte arrays share one cached image.
/// </summary>
public sealed class BytesImageSource : ImageSource
{
    private readonly byte[] _bytes;
    private readonly string _digest;

    /// <summary>
    /// Creates a source for <paramref name="bytes" />. The bytes are copied, so later changes to the buffer of
    /// the caller do not affect the source.
    /// </summary>
    /// <param name="bytes">
    /// The encoded image.
    /// </param>
    /// <param name="displayName">
    /// The name of the source for diagnostics.
    /// </param>
    public BytesImageSource(ReadOnlyMemory<byte> bytes, string? displayName = null)
        : base(displayName)
    {
        _bytes = bytes.ToArray();
        _digest = Convert.ToHexString(SHA256.HashData(_bytes)).ToLowerInvariant();
    }

    /// <summary>
    /// A copy of the encoded bytes.
    /// </summary>
    public ReadOnlyMemory<byte> Bytes => _bytes.ToArray();

    internal ReadOnlyMemory<byte> Content => _bytes;

    internal override ImageSourceKind Kind => ImageSourceKind.Bytes;

    internal override string CacheIdentity => $"bytes:{_digest}";

    internal override ImageSourceVersion? SourceRevision => new(_digest);
}

/// <summary>
/// An image that is read from a stream which a factory opens for every read of the source.
/// </summary>
public sealed class StreamImageSource : ImageSource
{
    /// <summary>
    /// Creates a source that reads the stream which <paramref name="openStream" /> opens.
    /// </summary>
    /// <param name="openStream">
    /// Opens a readable stream. The loader calls it for every read of the source, owns the returned stream and
    /// disposes it.
    /// </param>
    /// <param name="identity">
    /// A key that identifies the content across instances of this source.
    /// </param>
    /// <param name="revision">
    /// A token that changes whenever the content changes, or <see langword="null" /> when the caller cannot
    /// supply one.
    /// </param>
    /// <param name="displayName">
    /// The name of the source for diagnostics. <see langword="null" /> falls back to
    /// <paramref name="identity" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="openStream" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="identity" /> or <paramref name="revision" /> is empty or white space.
    /// </exception>
    public StreamImageSource(
        Func<CancellationToken, ValueTask<Stream>> openStream,
        string? identity = null,
        string? revision = null,
        string? displayName = null)
        : base(displayName ?? identity)
    {
        OpenStream = openStream ?? throw new ArgumentNullException(nameof(openStream));
        ValidateOptionalValue(identity, nameof(identity));
        ValidateOptionalValue(revision, nameof(revision));
        Identity = identity;
        Revision = revision;
    }

    /// <summary>
    /// The factory that opens the stream. It must return a new readable stream on every call; a
    /// <see langword="null" /> result fails the load with <see cref="ImageLoadErrorCode.InvalidSource" />.
    /// </summary>
    public Func<CancellationToken, ValueTask<Stream>> OpenStream { get; }

    /// <summary>
    /// A key that lets instances of this source share cached content. Without it the cache identity is tied to
    /// the factory of this instance.
    /// </summary>
    public string? Identity { get; }

    /// <summary>
    /// A token that identifies the content. While it stays the same, a cached copy is reused instead of opening
    /// the stream. Together with <see cref="Identity" /> it also allows the content to enter the disk cache.
    /// </summary>
    public string? Revision { get; }

    internal override ImageSourceKind Kind => ImageSourceKind.Stream;

    internal override string CacheIdentity => Identity is not null
        ? $"stream:key:{Identity}"
        : $"stream:object:{GetObjectIdentity(OpenStream)}";

    internal override ImageSourceVersion? SourceRevision => Revision is null ? null : new(Revision);

    internal override bool CanPersistSourceSnapshot => Identity is not null && Revision is not null;
}

/// <summary>
/// An image that is already decoded and is supplied by the caller. The loader returns it as is: it is not
/// validated, not resampled to a decode size and never disposed by the loader.
/// </summary>
public sealed class BorrowedImageSource : ImageSource
{
    /// <summary>
    /// Creates a source that supplies <paramref name="image" />.
    /// </summary>
    /// <param name="image">
    /// The decoded image.
    /// </param>
    /// <param name="displayName">
    /// The name of the source for diagnostics.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image" /> is <see langword="null" />.
    /// </exception>
    public BorrowedImageSource(IImage image, string? displayName = null)
        : base(displayName)
    {
        Image = image ?? throw new ArgumentNullException(nameof(image));
    }

    /// <summary>
    /// The image of the caller. Its size is reported as both the original and the decoded size.
    /// </summary>
    public IImage Image { get; }

    internal override ImageSourceKind Kind => ImageSourceKind.Borrowed;

    internal override string CacheIdentity => $"borrowed:object:{GetObjectIdentity(Image)}";
}
