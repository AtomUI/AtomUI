namespace AtomUI.Controls;

internal sealed class StorageFileImageSourceReader : ImageSourceReader
{
    private readonly ImageLoadingOptions _options;

    internal StorageFileImageSourceReader(ImageLoadingOptions options)
    {
        _options = options;
    }

    internal override ImageSourceKind Kind => ImageSourceKind.StorageFile;

    internal override async Task<ImageSourceReadResult> ReadAsync(
        NormalizedImageRequest request,
        ImageEncodedContent? staleContent,
        IProgress<ImageLoadProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (request.CacheRead != ImageCacheReadPolicy.RefreshSource &&
            staleContent is not null && request.Source.SourceRevision is not null &&
            staleContent.SourceVersion == request.Source.SourceRevision)
        {
            return new ImageSourceReadResult(staleContent.WithOrigin(ImageLoadOrigin.EncodedMemory));
        }
        try
        {
            await using var stream = await ((StorageFileImageSource)request.Source).File
                .OpenReadAsync().ConfigureAwait(false);
            ImageProgressDispatcher.Report(progress, ImageLoadProgress.Create(ImageLoadStage.Reading));
            var bytes = await ImageSourceReadHelpers.ReadAllBytesAsync(
                stream,
                _options.MaxResponseBytes,
                ImageLoadStage.Reading,
                progress,
                stream.CanSeek ? stream.Length : null,
                cancellationToken).ConfigureAwait(false);
            return new ImageSourceReadResult(new ImageEncodedContent(
                bytes,
                null,
                ImageLoadOrigin.Local,
                DateTimeOffset.UtcNow,
                FreshUntil: request.Source.SourceRevision is null ? null : DateTimeOffset.MaxValue,
                SourceVersion: request.Source.SourceRevision));
        }
        catch (UnauthorizedAccessException exception)
        {
            throw ImageSourceReadHelpers.Failure(
                ImageLoadErrorCode.AccessDenied,
                "Access to the selected image was denied.",
                request.Source.DisplayName,
                exception);
        }
    }
}
