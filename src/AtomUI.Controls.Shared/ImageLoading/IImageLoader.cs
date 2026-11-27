namespace AtomUI.Controls;

/// <summary>
/// Loads the image of an <see cref="ImageSource" /> with application-wide scheduling, caching and content
/// validation.
/// </summary>
/// <remarks>
/// An application has one loader. It is registered with <c>UseImageLoading()</c> and read back with
/// <c>Application.TryGetImageLoader()</c>.
/// </remarks>
public interface IImageLoader
{
    /// <summary>
    /// Loads the image described by <paramref name="request" />. The task completes when the image is ready to
    /// draw or the load has failed.
    /// </summary>
    /// <param name="request">
    /// The source, the cache options and the decode size.
    /// </param>
    /// <param name="cancellationToken">
    /// Ends this call only. Work that other calls share keeps running until its last waiter leaves.
    /// </param>
    /// <returns>
    /// A result that holds the decoded image, or a failed result that holds an <see cref="ImageLoadError" />.
    /// Dispose it to release the image.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// <paramref name="cancellationToken" /> was canceled, or a <see cref="ClearCacheAsync" /> request canceled
    /// the shared work this call was waiting for.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The loader has been disposed.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="request" /> is <see langword="null" />, or its options are invalid: the timeout is not a
    /// finite positive duration, a header name is forbidden, a header value contains a line break, or the
    /// partition or the variant is blank.
    /// </exception>
    /// <remarks>
    /// A load that fails for an ordinary reason, such as an error status, an unsupported format, a failed decode
    /// or an expired timeout, completes with a failed result instead of throwing. <see cref="LoadEvent" /> is
    /// raised when the load starts and when it ends.
    /// </remarks>
    ValueTask<ImageLoadResult> LoadAsync(
        ImageLoadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes cached images from the memory caches and, when the disk cache is enabled, from disk.
    /// </summary>
    /// <param name="request">
    /// The scope to clear and the caches it covers.
    /// </param>
    /// <param name="cancellationToken">
    /// Interrupts the wait for the disk cache when <see cref="ImageCacheClearRequest.ClearPersistent" /> is set.
    /// The memory caches are cleared before the token is observed.
    /// </param>
    /// <returns>
    /// A task that completes when the selected caches are cleared.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// The loader has been disposed.
    /// </exception>
    /// <remarks>
    /// Results returned before the call stay valid. Loads that were in flight when the call started cannot store
    /// their result in the cleared scope afterwards.
    /// </remarks>
    ValueTask ClearCacheAsync(
        ImageCacheClearRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The depths of the scheduler queues, the sizes of the memory caches and the load counters of this loader,
    /// sampled when the property is read.
    /// </summary>
    ImageLoaderSnapshot Snapshot { get; }

    /// <summary>
    /// Raised when a load starts and once more when it ends; <see cref="ImageLoaderEventArgs.Kind" /> tells which.
    /// </summary>
    /// <remarks>
    /// The starting event is raised synchronously on the thread that calls <see cref="LoadAsync" />. The ending
    /// event is raised on the thread that ends the load, after the outcome is known and before
    /// <see cref="LoadAsync" /> returns or throws. A load that is aborted by disposing the loader has no ending
    /// event. An exception thrown by a handler does not affect the load.
    /// </remarks>
    event EventHandler<ImageLoaderEventArgs>? LoadEvent;
}
