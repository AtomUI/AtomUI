using System.Diagnostics;
using Avalonia;
using Avalonia.Threading;

namespace AtomUI.Controls;

internal sealed class ImageLoader : IImageLoader, IAtomUIOwnedService
{
    private readonly ImageLoadingOptions _options;
    private readonly ImageLoaderPipeline _pipeline;
    private readonly CancellationTokenSource _rootCancellation = new();
    private Application? _application;
    private long _cacheHits;
    private long _cacheMisses;
    private long _canceledLoads;
    private long _failedLoads;
    private int _disposed;

    internal ImageLoader(
        ImageLoadingOptions options,
        IEnumerable<ImageCodec> codecs,
        HttpMessageHandler? httpMessageHandler = null,
        Func<HttpImageTransport>? httpTransportFactory = null)
    {
        _options = options;
        _pipeline = new ImageLoaderPipeline(options, codecs, httpMessageHandler, httpTransportFactory);
    }

    internal bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    public ImageLoaderSnapshot Snapshot => new(
        _pipeline.ActiveReads,
        _pipeline.QueuedReads,
        _pipeline.ActiveDecodes,
        _pipeline.QueuedDecodes,
        _pipeline.EncodedCacheEntries,
        _pipeline.EncodedCacheBytes,
        _pipeline.DecodedCacheEntries,
        _pipeline.DecodedCacheBytes,
        Interlocked.Read(ref _cacheHits),
        Interlocked.Read(ref _cacheMisses),
        Interlocked.Read(ref _canceledLoads),
        Interlocked.Read(ref _failedLoads));

    public event EventHandler<ImageLoaderEventArgs>? LoadEvent;

    public async ValueTask<ImageLoadResult> LoadAsync(
        ImageLoadRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentNullException.ThrowIfNull(request);
        var normalized = ImageCacheKey.Normalize(request, _options, forceReload: false);
        var stopwatch = Stopwatch.StartNew();
        RaiseEvent(ImageLoaderEventKind.Started, normalized.Source.Kind, null, null, stopwatch.Elapsed);

        using var timeoutCancellation = new CancellationTokenSource(normalized.Timeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCancellation.Token,
            _rootCancellation.Token);
        try
        {
            // Shared work captures the normalized request. Keep this caller's observer with its own waiter so
            // canceling the waiter releases the observer even while the transport continues for another caller.
            var result = await _pipeline.LoadAsync(
                normalized,
                normalized.Timing.Wrap(request.Progress),
                linked.Token).ConfigureAwait(false);
            if (result.Origin is ImageLoadOrigin.DecodedMemory or ImageLoadOrigin.EncodedMemory or
                ImageLoadOrigin.Persistent || result.SourceValidation == ImageSourceValidation.Revalidated)
            {
                Interlocked.Increment(ref _cacheHits);
            }
            else
            {
                Interlocked.Increment(ref _cacheMisses);
            }
            RaiseEvent(
                result.Origin is ImageLoadOrigin.DecodedMemory or ImageLoadOrigin.EncodedMemory or
                    ImageLoadOrigin.Persistent || result.SourceValidation == ImageSourceValidation.Revalidated
                    ? ImageLoaderEventKind.CacheHit
                    : ImageLoaderEventKind.Completed,
                normalized.Source.Kind,
                result.Origin,
                null,
                stopwatch.Elapsed);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Interlocked.Increment(ref _canceledLoads);
            RaiseEvent(ImageLoaderEventKind.Canceled, normalized.Source.Kind, null, null, stopwatch.Elapsed);
            throw;
        }
        catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested)
        {
            Interlocked.Increment(ref _failedLoads);
            var error = new ImageLoadError(
                ImageLoadErrorCode.Timeout,
                "The image request timed out.",
                SourceDisplayName: normalized.Source.DisplayName);
            RaiseEvent(ImageLoaderEventKind.Failed, normalized.Source.Kind, null, error.Code, stopwatch.Elapsed);
            return new ImageLoadResult(error, normalized.Timing.Snapshot());
        }
        catch (OperationCanceledException) when (_rootCancellation.IsCancellationRequested)
        {
            throw new ObjectDisposedException(nameof(ImageLoader));
        }
        catch (OperationCanceledException)
        {
            // The shared operation was canceled from inside, for example by teardown when concurrent waiters race to
            // leave or by ClearCacheAsync with CancelInFlight. That is not a source failure: like caller cancellation,
            // it is delivered as canceled and the caller classifies it as canceled.
            Interlocked.Increment(ref _canceledLoads);
            RaiseEvent(
                ImageLoaderEventKind.Canceled,
                normalized.Source.Kind,
                null,
                null,
                stopwatch.Elapsed);
            throw;
        }
        catch (ObjectDisposedException) when (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(ImageLoader));
        }
        catch (ImageLoadFailureException exception)
        {
            Interlocked.Increment(ref _failedLoads);
            RaiseEvent(
                ImageLoaderEventKind.Failed,
                normalized.Source.Kind,
                null,
                exception.Error.Code,
                stopwatch.Elapsed);
            return new ImageLoadResult(exception.Error, normalized.Timing.Snapshot());
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            Interlocked.Increment(ref _failedLoads);
            var error = new ImageLoadError(
                ImageLoadErrorCode.InvalidSource,
                "The image source could not be loaded.",
                SourceDisplayName: normalized.Source.DisplayName,
                Exception: exception);
            RaiseEvent(
                ImageLoaderEventKind.Failed,
                normalized.Source.Kind,
                null,
                error.Code,
                stopwatch.Elapsed);
            return new ImageLoadResult(error, normalized.Timing.Snapshot());
        }
    }

    public ValueTask ClearCacheAsync(
        ImageCacheClearRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        return _pipeline.ClearCacheAsync(request, cancellationToken);
    }

    public void Attach(Application application)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentNullException.ThrowIfNull(application);
        if (Interlocked.CompareExchange(ref _application, application, null) is not null)
        {
            throw new InvalidOperationException("Image loader is already attached to an Application.");
        }
        try
        {
            ImageLoaderStore.Attach(application, this);
        }
        catch
        {
            Interlocked.Exchange(ref _application, null);
            throw;
        }
    }

    public void Detach(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        if (!ReferenceEquals(Volatile.Read(ref _application), application))
        {
            return;
        }
        ImageLoaderStore.Detach(application, this);
        Interlocked.CompareExchange(ref _application, null, application);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }
        var application = Interlocked.Exchange(ref _application, null);
        if (application is not null)
        {
            ImageLoaderStore.Detach(application, this);
        }
        _pipeline.BeginDispose();
        _rootCancellation.Cancel();
        LoadEvent = null;
        if (Dispatcher.UIThread.CheckAccess() && _pipeline.HasInFlightWork)
        {
            _ = Task.Run(DisposePipeline);
        }
        else
        {
            DisposePipeline();
        }
    }

    private void DisposePipeline()
    {
        try
        {
            _pipeline.Dispose();
        }
        finally
        {
            _rootCancellation.Dispose();
        }
    }

    private void RaiseEvent(
        ImageLoaderEventKind kind,
        ImageSourceKind sourceKind,
        ImageLoadOrigin? origin,
        ImageLoadErrorCode? errorCode,
        TimeSpan elapsed)
    {
        ImageLoadEventDispatcher.Dispatch(
            LoadEvent,
            this,
            new ImageLoaderEventArgs(kind, sourceKind, origin, errorCode, elapsed));
    }

    private static bool IsNonFatal(Exception exception) =>
        exception is not OperationCanceledException && ImageLoadEventDispatcher.IsNonFatal(exception);
}
