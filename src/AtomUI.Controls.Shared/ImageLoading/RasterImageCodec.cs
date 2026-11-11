using Avalonia.Media.Imaging;

namespace AtomUI.Controls;

internal sealed class RasterImageCodec : ImageCodec
{
    internal override string Id => "atomui.raster";

    internal override int Version => 1;

    internal override bool CanDecode(ImageProbeResult probe, ImageSource source)
    {
        return probe.Format != ImageContentFormat.Svg;
    }

    internal override Task<ImageDecodedCacheEntry> DecodeAsync(
        ImageEncodedContent content,
        ImageProbeResult probe,
        NormalizedImageRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using var stream = new MemoryStream(content.Bytes, writable: false);
            var (targetWidth, targetHeight) = CalculateTargetSize(
                probe.PixelWidth,
                probe.PixelHeight,
                request.DecodePixelWidth,
                request.DecodePixelHeight,
                request.DecodeToCover);
            Bitmap bitmap;
            if (targetWidth >= probe.PixelWidth && targetHeight >= probe.PixelHeight)
            {
                bitmap = new Bitmap(stream);
            }
            else if (ShouldDecodeToWidth(
                         probe.PixelWidth,
                         probe.PixelHeight,
                         targetWidth,
                         targetHeight,
                         request.DecodePixelWidth,
                         request.DecodePixelHeight,
                         request.DecodeToCover))
            {
                bitmap = Bitmap.DecodeToWidth(stream, targetWidth, BitmapInterpolationMode.HighQuality);
            }
            else
            {
                bitmap = Bitmap.DecodeToHeight(stream, targetHeight, BitmapInterpolationMode.HighQuality);
            }
            var decodedWidth = bitmap.PixelSize.Width;
            var decodedHeight = bitmap.PixelSize.Height;
            var decodedBytes = checked((long)decodedWidth * decodedHeight * 4);
            return Task.FromResult(new ImageDecodedCacheEntry(
                bitmap,
                ownsImage: true,
                probe.PixelWidth,
                probe.PixelHeight,
                decodedWidth,
                decodedHeight,
                decodedBytes,
                probe.MediaType,
                content.Origin));
        }
        catch (ImageLoadFailureException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw ImageSourceReadHelpers.Failure(
                ImageLoadErrorCode.DecodeFailed,
                "Image decoding failed.",
                request.Source.DisplayName,
                exception);
        }
    }

    // Skia derives the other axis by truncating (int)(ratio * size). For a cover box that can leave the other axis below
    // the box (257x127 covering 16x16 decodes to 32x15 by width), so such a choice switches to the other axis. Contain
    // and single-bound requests keep the other axis inside or unbounded, where a one-pixel truncation does not matter.
    internal static bool ShouldDecodeToWidth(
        int originalWidth,
        int originalHeight,
        int targetWidth,
        int targetHeight,
        int maximumWidth,
        int maximumHeight,
        bool cover)
    {
        var byWidth = (double)targetWidth / originalWidth <= (double)targetHeight / originalHeight;
        if (!cover || maximumWidth <= 0 || maximumHeight <= 0)
        {
            return byWidth;
        }
        if (byWidth && (int)((double)originalHeight / originalWidth * targetWidth) < maximumHeight)
        {
            return false;
        }
        if (!byWidth && (int)((double)originalWidth / originalHeight * targetHeight) < maximumWidth)
        {
            return true;
        }
        return byWidth;
    }

    internal static (int Width, int Height) CalculateTargetSize(
        int originalWidth,
        int originalHeight,
        int maximumWidth,
        int maximumHeight,
        bool cover)
    {
        if (maximumWidth == 0 && maximumHeight == 0)
        {
            return (originalWidth, originalHeight);
        }
        var widthScale = maximumWidth > 0 ? (double)maximumWidth / originalWidth : double.PositiveInfinity;
        var heightScale = maximumHeight > 0 ? (double)maximumHeight / originalHeight : double.PositiveInfinity;
        // Contain fits inside the box; cover (only meaningful with both sizes) fills it, cropping the overflow.
        var boxScale = cover && maximumWidth > 0 && maximumHeight > 0
            ? Math.Max(widthScale, heightScale)
            : Math.Min(widthScale, heightScale);
        var scale = Math.Min(1, boxScale);
        return (
            Math.Max(1, (int)Math.Round(originalWidth * scale)),
            Math.Max(1, (int)Math.Round(originalHeight * scale)));
    }
}
