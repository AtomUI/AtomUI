using System.Buffers.Binary;
using System.Text;

namespace AtomUI.Controls;

internal enum ImageContentFormat
{
    Png,
    Jpeg,
    WebP,
    Bmp,
    Gif,
    Svg
}

/// <summary>
/// Describes encoded image content before it is decoded; it is passed to a codec such as <c>SvgImageCodec</c> in
/// AtomUI.Controls.
/// </summary>
/// <remarks>
/// It names the content format and its media type, the pixel size of the content and whether it is animated.
/// <c>SvgMetadata</c> is set only for SVG content.
/// </remarks>
internal readonly record struct ImageProbeResult(
    ImageContentFormat Format,
    string MediaType,
    int PixelWidth,
    int PixelHeight,
    bool IsAnimated,
    SvgContentMetadata? SvgMetadata = null);

internal sealed class ImageContentValidator
{
    private readonly ImageLoadingOptions _options;
    private readonly SvgContentValidator _svgValidator;

    internal ImageContentValidator(ImageLoadingOptions options)
    {
        _options = options;
        _svgValidator = new SvgContentValidator(options, ValidateEmbeddedRaster);
    }

    internal ImageProbeResult Validate(ImageEncodedContent content, ImageSource source) =>
        Validate(content, source, CancellationToken.None);

    internal ImageProbeResult Validate(
        ImageEncodedContent content,
        ImageSource source,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bytes = content.Bytes.AsSpan();
        if (bytes.IsEmpty)
        {
            throw Failure(ImageLoadErrorCode.InvalidImageData, "Image content is empty.", source);
        }

        ImageProbeResult probe;
        if (IsPng(bytes))
        {
            probe = ProbePng(bytes, source);
        }
        else if (IsJpeg(bytes))
        {
            probe = ProbeJpeg(bytes, source);
        }
        else if (IsGif(bytes))
        {
            probe = ProbeGif(bytes, source);
        }
        else if (IsBmp(bytes))
        {
            probe = ProbeBmp(bytes, source);
        }
        else if (IsWebP(bytes))
        {
            probe = ProbeWebP(bytes, source);
        }
        else if (LooksLikeMarkup(bytes))
        {
            if (IsUnsafeMarkupMediaType(content.MediaType))
            {
                throw Failure(
                    ImageLoadErrorCode.UnsafeVectorContent,
                    "Unsafe markup image content type was rejected.",
                    source);
            }
            var metadata = _svgValidator.Validate(content.Bytes, source, cancellationToken);
            probe = new ImageProbeResult(
                ImageContentFormat.Svg,
                "image/svg+xml",
                ToPixelDimension(metadata.IntrinsicWidth ?? metadata.ViewBoxWidth),
                ToPixelDimension(metadata.IntrinsicHeight ?? metadata.ViewBoxHeight),
                false,
                metadata);
        }
        else
        {
            throw Failure(ImageLoadErrorCode.UnsupportedFormat, "Image format is not supported.", source);
        }

        ValidateMediaType(content.MediaType, probe, source);
        ValidateDimensions(probe, source);
        return probe;
    }

    private ImageProbeResult ValidateEmbeddedRaster(
        byte[] bytes,
        string mediaType,
        ImageSource source)
    {
        var span = bytes.AsSpan();
        ImageProbeResult probe;
        if (IsPng(span))
        {
            probe = ProbePng(span, source);
        }
        else if (IsJpeg(span))
        {
            probe = ProbeJpeg(span, source);
        }
        else if (IsWebP(span))
        {
            probe = ProbeWebP(span, source);
        }
        else
        {
            throw Failure(ImageLoadErrorCode.InvalidImageData, "SVG embedded image format is invalid.", source);
        }
        ValidateMediaType(mediaType, probe, source);
        ValidateDimensions(probe, source);
        return probe;
    }

    private void ValidateDimensions(ImageProbeResult probe, ImageSource source)
    {
        if (probe.Format == ImageContentFormat.Svg)
        {
            var metadata = probe.SvgMetadata!;
            ValidateSvgDimension(metadata.IntrinsicWidth ?? metadata.ViewBoxWidth, _options.MaxImageWidth, source);
            ValidateSvgDimension(metadata.IntrinsicHeight ?? metadata.ViewBoxHeight, _options.MaxImageHeight, source);
            return;
        }
        if (probe.PixelWidth <= 0 || probe.PixelHeight <= 0)
        {
            throw Failure(ImageLoadErrorCode.InvalidImageData, "Image dimensions are invalid.", source);
        }
        if (probe.PixelWidth > _options.MaxImageWidth || probe.PixelHeight > _options.MaxImageHeight)
        {
            throw Failure(
                ImageLoadErrorCode.DimensionLimitExceeded,
                "Image dimensions exceed the configured limit.",
                source);
        }
        long pixels;
        long decodedBytes;
        try
        {
            pixels = checked((long)probe.PixelWidth * probe.PixelHeight);
            decodedBytes = checked(pixels * 4);
        }
        catch (OverflowException)
        {
            throw Failure(ImageLoadErrorCode.PixelLimitExceeded, "Image dimensions are too large.", source);
        }
        if (pixels > _options.MaxImagePixelCount)
        {
            throw Failure(ImageLoadErrorCode.PixelLimitExceeded, "Image pixel count exceeds the configured limit.", source);
        }
        if (decodedBytes > _options.MaxDecodedImageBytes)
        {
            throw Failure(
                ImageLoadErrorCode.DecodedByteLimitExceeded,
                "Decoded image size exceeds the configured limit.",
                source);
        }
        if (probe.IsAnimated)
        {
            throw Failure(ImageLoadErrorCode.AnimationNotSupported, "Animated images are not supported.", source);
        }
    }

    private static void ValidateMediaType(
        string? declaredMediaType,
        ImageProbeResult probe,
        ImageSource source)
    {
        var normalizedMediaType = NormalizeMediaType(declaredMediaType);
        if (normalizedMediaType is null || normalizedMediaType == "application/octet-stream")
        {
            return;
        }
        if (probe.Format == ImageContentFormat.Svg)
        {
            if (normalizedMediaType is "image/svg+xml" or "application/xml" or "text/xml")
            {
                return;
            }
        }
        if (!normalizedMediaType.Equals(probe.MediaType, StringComparison.OrdinalIgnoreCase))
        {
            throw Failure(
                ImageLoadErrorCode.ContentTypeMismatch,
                "Image content does not match its declared content type.",
                source);
        }
    }

    private static ImageProbeResult ProbePng(ReadOnlySpan<byte> bytes, ImageSource source)
    {
        if (bytes.Length < 33)
        {
            throw Failure(ImageLoadErrorCode.InvalidImageData, "PNG header is incomplete.", source);
        }
        if (BinaryPrimitives.ReadUInt32BigEndian(bytes[8..12]) != 13 ||
            !bytes[12..16].SequenceEqual("IHDR"u8))
        {
            throw Failure(ImageLoadErrorCode.InvalidImageData, "PNG IHDR chunk is invalid.", source);
        }
        var width = ReadPngDimension(bytes[16..20], source);
        var height = ReadPngDimension(bytes[20..24], source);
        var animated = false;
        var offset = 8;
        while (offset <= bytes.Length - 12)
        {
            var chunkLength = BinaryPrimitives.ReadUInt32BigEndian(bytes[offset..]);
            var dataStart = offset + 8;
            if (chunkLength > (uint)(bytes.Length - dataStart - 4))
            {
                throw Failure(ImageLoadErrorCode.InvalidImageData, "PNG chunk is truncated.", source);
            }
            var dataLength = (int)chunkLength;
            var chunkType = bytes[(offset + 4)..(offset + 8)];
            if (chunkType.SequenceEqual("acTL"u8))
            {
                animated = true;
            }
            offset = dataStart + dataLength + 4;
            if (chunkType.SequenceEqual("IEND"u8))
            {
                break;
            }
        }
        return new ImageProbeResult(ImageContentFormat.Png, "image/png", width, height, animated);
    }

    private static ImageProbeResult ProbeJpeg(ReadOnlySpan<byte> bytes, ImageSource source)
    {
        var offset = 2;
        while (offset + 4 <= bytes.Length)
        {
            if (bytes[offset] != 0xff)
            {
                offset++;
                continue;
            }
            while (offset < bytes.Length && bytes[offset] == 0xff)
            {
                offset++;
            }
            if (offset >= bytes.Length)
            {
                break;
            }
            var marker = bytes[offset++];
            if (marker is 0xd8 or 0xd9)
            {
                continue;
            }
            if (offset + 2 > bytes.Length)
            {
                break;
            }
            var segmentLength = BinaryPrimitives.ReadUInt16BigEndian(bytes[offset..]);
            if (segmentLength < 2 || offset + segmentLength > bytes.Length)
            {
                break;
            }
            if (marker is >= 0xc0 and <= 0xc3 or >= 0xc5 and <= 0xc7 or >= 0xc9 and <= 0xcb or >= 0xcd and <= 0xcf)
            {
                if (segmentLength < 7)
                {
                    break;
                }
                var height = BinaryPrimitives.ReadUInt16BigEndian(bytes[(offset + 3)..]);
                var width = BinaryPrimitives.ReadUInt16BigEndian(bytes[(offset + 5)..]);
                return new ImageProbeResult(ImageContentFormat.Jpeg, "image/jpeg", width, height, false);
            }
            offset += segmentLength;
        }
        throw Failure(ImageLoadErrorCode.InvalidImageData, "JPEG dimensions could not be read.", source);
    }

    private static ImageProbeResult ProbeGif(ReadOnlySpan<byte> bytes, ImageSource source)
    {
        if (bytes.Length < 13)
        {
            throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF header is incomplete.", source);
        }
        var width = BinaryPrimitives.ReadUInt16LittleEndian(bytes[6..8]);
        var height = BinaryPrimitives.ReadUInt16LittleEndian(bytes[8..10]);
        var frameCount = 0;
        var index = 13;
        if ((bytes[10] & 0x80) != 0)
        {
            index += 3 * (1 << ((bytes[10] & 0x07) + 1));
        }
        if (index > bytes.Length)
        {
            throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF color table is truncated.", source);
        }
        while (index < bytes.Length)
        {
            var introducer = bytes[index++];
            switch (introducer)
            {
                case 0x3b:
                    return new ImageProbeResult(ImageContentFormat.Gif, "image/gif", width, height, frameCount > 1);
                case 0x2c:
                    if (index + 9 > bytes.Length)
                    {
                        throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF image descriptor is truncated.", source);
                    }
                    var packed = bytes[index + 8];
                    index += 9;
                    if ((packed & 0x80) != 0)
                    {
                        index += 3 * (1 << ((packed & 0x07) + 1));
                        if (index > bytes.Length)
                        {
                            throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF color table is truncated.", source);
                        }
                    }
                    if (index >= bytes.Length)
                    {
                        throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF image data is truncated.", source);
                    }
                    index++;
                    SkipGifSubBlocks(bytes, ref index, source);
                    frameCount++;
                    break;
                case 0x21:
                    if (index >= bytes.Length)
                    {
                        throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF extension is truncated.", source);
                    }
                    var label = bytes[index++];
                    var fixedLength = label switch
                    {
                        0xf9 => 4,
                        0x01 => 12,
                        0xff => 11,
                        _ => 0
                    };
                    if (fixedLength > 0)
                    {
                        if (index >= bytes.Length || bytes[index++] != fixedLength)
                        {
                            throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF extension block is invalid.", source);
                        }
                        index += fixedLength;
                        if (index > bytes.Length)
                        {
                            throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF extension block is truncated.", source);
                        }
                    }
                    SkipGifSubBlocks(bytes, ref index, source);
                    break;
                default:
                    throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF data contains an invalid block.", source);
            }
        }
        throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF trailer is missing.", source);
    }

    private static ImageProbeResult ProbeBmp(ReadOnlySpan<byte> bytes, ImageSource source)
    {
        if (bytes.Length < 26)
        {
            throw Failure(ImageLoadErrorCode.InvalidImageData, "BMP header is incomplete.", source);
        }
        var rawWidth = BinaryPrimitives.ReadInt32LittleEndian(bytes[18..22]);
        var rawHeight = BinaryPrimitives.ReadInt32LittleEndian(bytes[22..26]);
        if (rawWidth == int.MinValue || rawHeight == int.MinValue)
        {
            throw Failure(ImageLoadErrorCode.DimensionLimitExceeded, "BMP dimensions exceed the configured limit.", source);
        }
        var width = Math.Abs(rawWidth);
        var height = Math.Abs(rawHeight);
        return new ImageProbeResult(ImageContentFormat.Bmp, "image/bmp", width, height, false);
    }

    private static ImageProbeResult ProbeWebP(ReadOnlySpan<byte> bytes, ImageSource source)
    {
        if (bytes.Length < 30)
        {
            throw Failure(ImageLoadErrorCode.InvalidImageData, "WebP header is incomplete.", source);
        }
        var chunk = Encoding.ASCII.GetString(bytes[12..16]);
        int width;
        int height;
        var animated = false;
        switch (chunk)
        {
            case "VP8X":
                animated = (bytes[20] & 0x02) != 0;
                width = 1 + ReadUInt24LittleEndian(bytes[24..27]);
                height = 1 + ReadUInt24LittleEndian(bytes[27..30]);
                break;
            case "VP8 ":
                if (bytes.Length < 30 || bytes[23] != 0x9d || bytes[24] != 0x01 || bytes[25] != 0x2a)
                {
                    throw Failure(ImageLoadErrorCode.InvalidImageData, "WebP VP8 header is invalid.", source);
                }
                width = BinaryPrimitives.ReadUInt16LittleEndian(bytes[26..28]) & 0x3fff;
                height = BinaryPrimitives.ReadUInt16LittleEndian(bytes[28..30]) & 0x3fff;
                break;
            case "VP8L":
                if (bytes[20] != 0x2f)
                {
                    throw Failure(ImageLoadErrorCode.InvalidImageData, "WebP VP8L header is invalid.", source);
                }
                var bits = BinaryPrimitives.ReadUInt32LittleEndian(bytes[21..25]);
                width = (int)(bits & 0x3fff) + 1;
                height = (int)((bits >> 14) & 0x3fff) + 1;
                break;
            default:
                throw Failure(ImageLoadErrorCode.InvalidImageData, "WebP format is invalid.", source);
        }
        return new ImageProbeResult(ImageContentFormat.WebP, "image/webp", width, height, animated);
    }

    private static bool LooksLikeMarkup(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf)
        {
            bytes = bytes[3..];
        }
        if (bytes.Length >= 2 &&
            ((bytes[0] == 0xff && bytes[1] == 0xfe) ||
             (bytes[0] == 0xfe && bytes[1] == 0xff)))
        {
            return true;
        }
        foreach (var value in bytes)
        {
            if (value is (byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n')
            {
                continue;
            }
            return value == (byte)'<';
        }
        return false;
    }

    private static bool IsUnsafeMarkupMediaType(string? mediaType)
    {
        var normalized = NormalizeMediaType(mediaType);
        return normalized is "text/html" or "application/xhtml+xml";
    }

    private static string? NormalizeMediaType(string? mediaType)
    {
        if (string.IsNullOrWhiteSpace(mediaType))
        {
            return null;
        }
        var separator = mediaType.IndexOf(';');
        return (separator < 0 ? mediaType : mediaType[..separator]).Trim().ToLowerInvariant();
    }

    private static int ToPixelDimension(double? value)
    {
        if (value is null || !double.IsFinite(value.Value) || value.Value <= 0)
        {
            return 0;
        }
        return value.Value >= int.MaxValue ? int.MaxValue : (int)Math.Ceiling(value.Value);
    }

    private static void ValidateSvgDimension(double? value, int limit, ImageSource source)
    {
        if (value is null)
        {
            return;
        }
        if (!double.IsFinite(value.Value) || value.Value <= 0)
        {
            throw Failure(ImageLoadErrorCode.InvalidImageData, "SVG dimensions are invalid.", source);
        }
        if (value.Value > limit)
        {
            throw Failure(
                ImageLoadErrorCode.DimensionLimitExceeded,
                "SVG dimensions exceed the configured limit.",
                source);
        }
    }

    private static int ReadPngDimension(ReadOnlySpan<byte> value, ImageSource source)
    {
        var dimension = BinaryPrimitives.ReadUInt32BigEndian(value);
        if (dimension > int.MaxValue)
        {
            throw Failure(ImageLoadErrorCode.DimensionLimitExceeded, "PNG dimensions exceed the configured limit.", source);
        }
        return (int)dimension;
    }

    private static void SkipGifSubBlocks(
        ReadOnlySpan<byte> bytes,
        ref int index,
        ImageSource source)
    {
        while (true)
        {
            if (index >= bytes.Length)
            {
                throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF data sub-blocks are truncated.", source);
            }
            var length = bytes[index++];
            if (length == 0)
            {
                return;
            }
            if (length > bytes.Length - index)
            {
                throw Failure(ImageLoadErrorCode.InvalidImageData, "GIF data sub-block is truncated.", source);
            }
            index += length;
        }
    }

    private static int ReadUInt24LittleEndian(ReadOnlySpan<byte> value)
    {
        return value[0] | value[1] << 8 | value[2] << 16;
    }

    private static bool IsPng(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 8 &&
        bytes[0] == 0x89 &&
        bytes[1] == (byte)'P' &&
        bytes[2] == (byte)'N' &&
        bytes[3] == (byte)'G' &&
        bytes[4] == 0x0d &&
        bytes[5] == 0x0a &&
        bytes[6] == 0x1a &&
        bytes[7] == 0x0a;

    private static bool IsJpeg(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff;

    private static bool IsGif(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 6 &&
        (bytes[..6].SequenceEqual("GIF87a"u8) || bytes[..6].SequenceEqual("GIF89a"u8));

    private static bool IsBmp(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 2 && bytes[0] == (byte)'B' && bytes[1] == (byte)'M';

    private static bool IsWebP(ReadOnlySpan<byte> bytes) =>
        bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8);

    private static ImageLoadFailureException Failure(
        ImageLoadErrorCode code,
        string message,
        ImageSource source)
    {
        return ImageSourceReadHelpers.Failure(code, message, source.DisplayName);
    }
}
