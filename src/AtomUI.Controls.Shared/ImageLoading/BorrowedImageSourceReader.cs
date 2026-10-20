namespace AtomUI.Controls;

internal sealed class BorrowedImageSourceReader : ImageSourceReader
{
    internal override ImageSourceKind Kind => ImageSourceKind.Borrowed;

    internal override Task<ImageSourceReadResult> ReadAsync(
        NormalizedImageRequest request,
        ImageEncodedContent? staleContent,
        IProgress<ImageLoadProgress>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ImageProgressDispatcher.Report(progress, ImageLoadProgress.Create(ImageLoadStage.Reading));
        return Task.FromResult(new ImageSourceReadResult(
            BorrowedImage: ((BorrowedImageSource)request.Source).Image,
            SourceValidation: ImageSourceValidation.NotRequired));
    }
}
