namespace AtomUI.Controls;

internal sealed class HttpImageSourceReader : ImageSourceReader
{
    private readonly Func<HttpImageTransport> _getTransport;

    internal HttpImageSourceReader(Func<HttpImageTransport> getTransport)
    {
        ArgumentNullException.ThrowIfNull(getTransport);
        _getTransport = getTransport;
    }

    internal override ImageSourceKind Kind => ImageSourceKind.Http;

    internal override async Task<ImageSourceReadResult> ReadAsync(
        NormalizedImageRequest request,
        ImageEncodedContent? staleContent,
        IProgress<ImageLoadProgress>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var content = await _getTransport().FetchAsync(
            request,
            staleContent,
            progress,
            cancellationToken).ConfigureAwait(false);
        return new ImageSourceReadResult(content, SourceValidation: content.SourceValidation);
    }
}
