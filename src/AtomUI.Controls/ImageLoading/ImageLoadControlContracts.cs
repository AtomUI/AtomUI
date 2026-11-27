namespace AtomUI.Controls;

/// <summary>
/// Exposes the asynchronous image load of a control: the image to load with an optional fallback, the state of
/// the load and the events that end it. It lets a caller drive every image-loading control, such as
/// <see cref="AsyncImage" />, through the same members.
/// </summary>
public interface IImageLoadControl
{
    /// <summary>
    /// Gets or sets the image to load and show. A load starts when a value is set and the control is attached to a
    /// visual tree. <see langword="null" /> clears the shown image and leaves <see cref="LoadState" /> at
    /// <see cref="ImageLoadState.Idle" />; the fallback is not tried.
    /// </summary>
    ImageSource? Source { get; set; }

    /// <summary>
    /// Gets or sets the image to show when loading <see cref="Source" /> fails. It is not tried while
    /// <see cref="Source" /> is <see langword="null" /> or when the load configuration of the control is invalid.
    /// </summary>
    /// <remarks>
    /// A shown fallback leaves <see cref="LoadState" /> at <see cref="ImageLoadState.Loaded" /> and raises
    /// <see cref="ImageOpened" /> with <see cref="ImageOpenedEventArgs.IsFallback" /> set.
    /// </remarks>
    ImageSource? FallbackSource { get; set; }

    /// <summary>
    /// Gets or sets the options for the loads of <see cref="Source" /> and <see cref="FallbackSource" />, such as
    /// the cache policies, or <see langword="null" /> to load with the defaults of
    /// <see cref="ImageRequestOptions" />. Assigning other options restarts the load.
    /// </summary>
    ImageRequestOptions? RequestOptions { get; set; }

    /// <summary>
    /// Gets the state of the current image load. It is <see cref="ImageLoadState.Loaded" /> also while the fallback
    /// image is shown.
    /// </summary>
    ImageLoadState LoadState { get; }

    /// <summary>
    /// Gets why the image load failed. It is <see langword="null" /> unless <see cref="LoadState" /> is
    /// <see cref="ImageLoadState.Failed" />. When the main image and the fallback both failed, it describes the
    /// fallback.
    /// </summary>
    ImageLoadError? LoadError { get; }

    /// <summary>
    /// Gets how far the running image load has advanced. It is <see langword="null" /> unless
    /// <see cref="LoadState" /> is <see cref="ImageLoadState.Loading" />.
    /// </summary>
    ImageLoadProgress? LoadProgress { get; }

    /// <summary>
    /// Gets a value indicating whether an image load is running. This includes a re-decode of the shown image,
    /// which stays visible meanwhile.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Gets a value indicating whether the last image load ended with a shown image, the fallback image included.
    /// It is <see langword="false" /> again while the shown image is decoded anew. It reports the image, not
    /// whether the control is loaded in the visual tree.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Gets a value indicating whether the image load ended with an error and no image is shown. It is
    /// <see langword="false" /> while the fallback image is shown.
    /// </summary>
    bool IsFailed { get; }

    /// <summary>
    /// Raised on the UI thread after a loaded image has become the shown image and <see cref="LoadState" /> has
    /// been set to loaded, so the handler sees the final state. This includes the fallback image;
    /// <see cref="ImageOpenedEventArgs.IsFallback" /> tells which image was shown.
    /// </summary>
    event EventHandler<ImageOpenedEventArgs>? ImageOpened;

    /// <summary>
    /// Raised on the UI thread after a load ended with an error and no image is shown. <see cref="LoadState" />
    /// is <see cref="ImageLoadState.Failed" /> and <see cref="LoadError" /> is set when the handler runs. When the
    /// main image and the fallback both failed, the event reports the fallback.
    /// </summary>
    event EventHandler<ImageFailedEventArgs>? ImageFailed;

    /// <summary>
    /// Loads <see cref="Source" /> again, reading the source instead of taking the cached content, so changes at
    /// the origin become visible. The image that is shown stays until the new load ends. Nothing happens while the
    /// control is detached from the visual tree or while <see cref="Source" /> is <see langword="null" />.
    /// </summary>
    void Reload();
}

/// <summary>
/// Provides data for the <see cref="IImageLoadControl.ImageOpened" /> event.
/// </summary>
public sealed class ImageOpenedEventArgs : EventArgs
{
    internal ImageOpenedEventArgs(
        ImageSource source,
        bool isFallback,
        ImageLoadOrigin origin,
        int pixelWidth,
        int pixelHeight)
    {
        Source = source;
        IsFallback = isFallback;
        Origin = origin;
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
    }

    /// <summary>
    /// Gets the source of the image that was shown: the fallback source when <see cref="IsFallback" /> is
    /// <see langword="true" />, otherwise the main source.
    /// </summary>
    public ImageSource Source { get; }

    /// <summary>
    /// Gets a value indicating whether the shown image was loaded from the fallback source.
    /// </summary>
    public bool IsFallback { get; }

    /// <summary>
    /// Gets the <see cref="ImageLoadOrigin" />, which tells where the shown image was taken from: a cache or an
    /// image that the caller supplied.
    /// </summary>
    public ImageLoadOrigin Origin { get; }

    /// <summary>
    /// Gets the width of the shown image in pixels after decoding.
    /// </summary>
    public int PixelWidth { get; }

    /// <summary>
    /// Gets the height of the shown image in pixels after decoding.
    /// </summary>
    public int PixelHeight { get; }
}

/// <summary>
/// Provides data for the <see cref="IImageLoadControl.ImageFailed" /> event.
/// </summary>
public sealed class ImageFailedEventArgs : EventArgs
{
    internal ImageFailedEventArgs(ImageSource source, bool isFallback, ImageLoadError error)
    {
        Source = source;
        IsFallback = isFallback;
        Error = error;
    }

    /// <summary>
    /// Gets the source whose failure is reported: the fallback source when <see cref="IsFallback" /> is
    /// <see langword="true" />, otherwise the main source.
    /// </summary>
    public ImageSource Source { get; }

    /// <summary>
    /// Gets a value indicating whether the reported error is that of the fallback source, which is the case when
    /// the main image and the fallback both failed.
    /// </summary>
    public bool IsFallback { get; }

    /// <summary>
    /// Gets the error that failed the load.
    /// </summary>
    public ImageLoadError Error { get; }
}
