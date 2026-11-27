using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// A decoded image with its size information, held by the decoded cache and shared between image loads; codecs
/// return it from <c>ImageCodec.DecodeAsync</c>, as <c>SvgImageCodec</c> in AtomUI.Controls does.
/// </summary>
/// <remarks>
/// The image stays alive while the cache holds it (<c>AddCacheMembership</c>), while an operation retains it
/// (<c>RetainOperation</c>) or while a result obtained from <c>AcquireResult</c> is not released. It is disposed
/// when the last of them goes away, but only when the entry was created with <c>ownsImage</c> set to
/// <see langword="true" />.
/// </remarks>
internal sealed class ImageDecodedCacheEntry
{
    private readonly object _gate = new();
    private IImage? _image;
    private int _leaseCount;
    private int _operationReferences;
    private bool _hasCacheMembership;
    private readonly bool _ownsImage;

    internal ImageDecodedCacheEntry(
        IImage image,
        bool ownsImage,
        int originalPixelWidth,
        int originalPixelHeight,
        int decodedPixelWidth,
        int decodedPixelHeight,
        long decodedBytes,
        string? mediaType,
        ImageLoadOrigin origin = ImageLoadOrigin.Local)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
        _ownsImage = ownsImage;
        OriginalPixelWidth = originalPixelWidth;
        OriginalPixelHeight = originalPixelHeight;
        DecodedPixelWidth = decodedPixelWidth;
        DecodedPixelHeight = decodedPixelHeight;
        DecodedBytes = decodedBytes;
        MediaType = mediaType;
        Origin = origin;
    }

    internal int OriginalPixelWidth { get; }

    internal int OriginalPixelHeight { get; }

    internal int DecodedPixelWidth { get; }

    internal int DecodedPixelHeight { get; }

    internal long DecodedBytes { get; }

    internal string? MediaType { get; }

    internal ImageLoadOrigin Origin { get; }

    internal void AddCacheMembership()
    {
        lock (_gate)
        {
            ThrowIfReleased();
            if (_hasCacheMembership)
            {
                throw new InvalidOperationException("Decoded cache entry already has cache membership.");
            }
            _hasCacheMembership = true;
        }
    }

    internal void RetainOperation()
    {
        lock (_gate)
        {
            ThrowIfReleased();
            _operationReferences++;
        }
    }

    internal ImageLoadResult AcquireResult(
        ImageLoadOrigin origin,
        ImageSourceValidation sourceValidation,
        string? contentId,
        IReadOnlyDictionary<ImageLoadStage, TimeSpan>? stageDurations = null)
    {
        IImage image;
        lock (_gate)
        {
            ThrowIfReleased();
            _leaseCount++;
            image = _image!;
        }
        return new ImageLoadResult(
            image,
            new ImageResultLease(ReleaseLease),
            OriginalPixelWidth,
            OriginalPixelHeight,
            DecodedPixelWidth,
            DecodedPixelHeight,
            MediaType,
            origin,
            sourceValidation,
            contentId,
            stageDurations);
    }

    internal void ReleaseOperation()
    {
        IImage? dispose = null;
        lock (_gate)
        {
            if (_operationReferences == 0)
            {
                return;
            }
            _operationReferences--;
            dispose = TakeDisposableImageCore();
        }
        DisposeImage(dispose);
    }

    internal void RemoveCacheMembership()
    {
        IImage? dispose = null;
        lock (_gate)
        {
            if (!_hasCacheMembership)
            {
                return;
            }
            _hasCacheMembership = false;
            dispose = TakeDisposableImageCore();
        }
        DisposeImage(dispose);
    }

    internal void Discard()
    {
        IImage? dispose;
        lock (_gate)
        {
            if (_image is null)
            {
                return;
            }
            if (_hasCacheMembership || _operationReferences != 0 || _leaseCount != 0)
            {
                throw new InvalidOperationException("A retained decoded cache entry cannot be discarded.");
            }
            dispose = _image;
            _image = null;
        }
        DisposeImage(_ownsImage ? dispose : null);
    }

    private void ReleaseLease()
    {
        IImage? dispose = null;
        lock (_gate)
        {
            if (_leaseCount == 0)
            {
                return;
            }
            _leaseCount--;
            dispose = TakeDisposableImageCore();
        }
        DisposeImage(dispose);
    }

    private IImage? TakeDisposableImageCore()
    {
        if (_image is null || _hasCacheMembership || _operationReferences != 0 || _leaseCount != 0)
        {
            return null;
        }
        var image = _image;
        _image = null;
        return _ownsImage ? image : null;
    }

    private void ThrowIfReleased()
    {
        if (_image is null)
        {
            throw new ObjectDisposedException(nameof(ImageDecodedCacheEntry));
        }
    }

    private static void DisposeImage(IImage? image)
    {
        (image as IDisposable)?.Dispose();
    }
}
