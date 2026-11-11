namespace AtomUI.Controls;

internal abstract class ImageCodec
{
    internal abstract string Id { get; }

    internal abstract int Version { get; }

    internal virtual bool IsDecodeSizeDependent => true;

    internal ImageDecodeKey CreateDecodeKey(
        NormalizedImageRequest request,
        ImageContentId contentId)
    {
        return new ImageDecodeKey(
            request.PartitionHash,
            contentId,
            new ImageDecodeSpec(
                IsDecodeSizeDependent ? request.DecodePixelWidth : 0,
                IsDecodeSizeDependent ? request.DecodePixelHeight : 0,
                $"{Id}:{Version}:security:{ImageSecurityPolicy.Version}",
                IsDecodeSizeDependent && request.DecodeToCover));
    }

    internal abstract bool CanDecode(ImageProbeResult probe, ImageSource source);

    internal abstract Task<ImageDecodedCacheEntry> DecodeAsync(
        ImageEncodedContent content,
        ImageProbeResult probe,
        NormalizedImageRequest request,
        CancellationToken cancellationToken);
}

internal sealed class ImageCodecRegistry
{
    private readonly IReadOnlyList<ImageCodec> _codecs;

    internal ImageCodecRegistry(IEnumerable<ImageCodec> codecs)
    {
        var ids = new Dictionary<string, (Type Type, int Version)>(StringComparer.Ordinal);
        var result = new List<ImageCodec>();
        foreach (var codec in codecs)
        {
            if (string.IsNullOrWhiteSpace(codec.Id))
            {
                throw new InvalidOperationException("Image codec ids cannot be empty.");
            }
            if (codec.Version <= 0)
            {
                throw new InvalidOperationException($"Image codec '{codec.Id}' must have a positive version.");
            }
            if (ids.TryGetValue(codec.Id, out var existing))
            {
                if (existing.Type != codec.GetType() || existing.Version != codec.Version)
                {
                    throw new InvalidOperationException($"Image codec id '{codec.Id}' has conflicting registrations.");
                }
                continue;
            }
            ids.Add(codec.Id, (codec.GetType(), codec.Version));
            result.Add(codec);
        }
        _codecs = result;
    }

    internal ImageCodec Select(ImageProbeResult probe, ImageSource source)
    {
        var matches = _codecs.Where(codec => codec.CanDecode(probe, source)).ToArray();
        return matches.Length switch
        {
            1 => matches[0],
            0 => throw ImageSourceReadHelpers.Failure(
                ImageLoadErrorCode.UnsupportedFormat,
                "No registered image codec can decode this content.",
                source.DisplayName),
            _ => throw new InvalidOperationException(
                $"Multiple image codecs match '{probe.Format}' for source kind '{source.Kind}'.")
        };
    }
}
