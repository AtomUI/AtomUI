using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the data of the <c>ImageOpened</c> event: the preview item whose full image finished loading, its
/// position in the item list of the preview and where the image data came from.
/// </summary>
public sealed class ImagePreviewOpenedEventArgs : EventArgs
{
    internal ImagePreviewOpenedEventArgs(ImagePreviewItem item, int index, ImageLoadOrigin origin)
    {
        Item = item;
        Index = index;
        Origin = origin;
    }

    /// <summary>
    /// The preview item whose full image finished loading.
    /// </summary>
    public ImagePreviewItem Item { get; }

    /// <summary>
    /// The zero-based position of <see cref="Item" /> in the item list of the preview, or -1 when the item is not
    /// in the list.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Where the full image data came from, recorded as an <see cref="ImageLoadOrigin" />.
    /// </summary>
    public ImageLoadOrigin Origin { get; }
}

/// <summary>
/// Carries the data of the <c>ImageFailed</c> event: the preview item whose full image failed to load, its
/// position in the item list of the preview and the reported error.
/// </summary>
public sealed class ImagePreviewFailedEventArgs : EventArgs
{
    internal ImagePreviewFailedEventArgs(ImagePreviewItem item, int index, ImageLoadError error)
    {
        Item = item;
        Index = index;
        Error = error;
    }

    /// <summary>
    /// The preview item whose full image failed to load.
    /// </summary>
    public ImagePreviewItem Item { get; }

    /// <summary>
    /// The zero-based position of <see cref="Item" /> in the item list of the preview, or -1 when the item is not
    /// in the list.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// The error that the loader reported for the full image, as an <see cref="ImageLoadError" />.
    /// </summary>
    public ImageLoadError Error { get; }
}
