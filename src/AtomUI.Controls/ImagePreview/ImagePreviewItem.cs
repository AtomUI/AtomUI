using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public sealed record ImagePreviewItem
{
    public ImagePreviewItem(ImageSource source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public ImageSource Source { get; init; }

    public ImageSource? ThumbnailSource { get; init; }

    public ImageSource? FallbackSource { get; init; }

    public ImageRequestOptions? RequestOptions { get; init; }

    public string? Title { get; init; }

    public object? Tag { get; init; }
}
