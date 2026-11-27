using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// One image of the collection that an image previewer shows, with its optional thumbnail, fallback image,
/// load options and title.
/// </summary>
public sealed record ImagePreviewItem
{
    /// <summary>
    /// Creates an item that shows the given image.
    /// </summary>
    /// <param name="source">
    /// The full image of the item. It cannot be <see langword="null" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source" /> is <see langword="null" />.
    /// </exception>
    public ImagePreviewItem(ImageSource source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>
    /// Gets the full image that the preview shows for this item. The thumbnail load also tries it, after
    /// <see cref="ThumbnailSource" />.
    /// </summary>
    public ImageSource Source { get; init; }

    /// <summary>
    /// Gets the image shown as the thumbnail of the item, or <see langword="null" /> to load the thumbnail from
    /// <see cref="Source" />. The thumbnail load tries this image first, then <see cref="Source" />, then
    /// <see cref="FallbackSource" />.
    /// </summary>
    public ImageSource? ThumbnailSource { get; init; }

    /// <summary>
    /// Gets the image that is tried when a load of this item fails, or <see langword="null" /> for none. The full
    /// load tries it after <see cref="Source" />; the thumbnail load after <see cref="ThumbnailSource" /> and
    /// <see cref="Source" />.
    /// </summary>
    public ImageSource? FallbackSource { get; init; }

    /// <summary>
    /// Gets the options for the loads of this item, such as the cache policies, or <see langword="null" /> to load
    /// with the defaults of <see cref="ImageRequestOptions" />.
    /// </summary>
    public ImageRequestOptions? RequestOptions { get; init; }

    /// <summary>
    /// Gets the title of the item, or <see langword="null" /> when it has none. The preview dialog uses it, when
    /// it is not null or white-space, to decide the title of the preview.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets arbitrary data carried with the item, or <see langword="null" />. The image previewer does not read it.
    /// </summary>
    public object? Tag { get; init; }
}
