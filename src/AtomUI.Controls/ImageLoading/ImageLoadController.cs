using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Controls;

internal interface IImageLoadControllerHost
{
    Visual Visual { get; }

    ImageSource? Source { get; }

    ImageSource? FallbackSource { get; }

    ImageRequestOptions? RequestOptions { get; }

    ImageRequestPriority Priority { get; }

    bool IsImageLoadAttached { get; }

    (int Width, int Height)? GetDecodePixelSize();

    ImageLoadError? GetConfigurationError();

    void SetLoadedImage(IImage? image);

    void SetLoadState(ImageLoadState state, ImageLoadError? error, ImageLoadProgress? progress, bool isFallback);

    void RaiseImageOpened(ImageOpenedEventArgs eventArgs);

    void RaiseImageFailed(ImageFailedEventArgs eventArgs);
}

internal sealed class ImageLoadController : IDisposable
{
    private readonly IImageLoadControllerHost _host;
    private ImageCancellationState? _requestCancellation;
    private IDisposable? _scalingSubscription;
    private ImageLoadResult? _currentResult;
    private string? _currentSourceIdentity;
    private (int Width, int Height)? _lastDecodeSize;
    private long _generation;
    private bool _disposed;

    internal ImageLoadController(IImageLoadControllerHost host)
    {
        _host = host;
    }

    internal void Attach()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _scalingSubscription?.Dispose();
        var topLevel = TopLevel.GetTopLevel(_host.Visual);
        if (topLevel is not null)
        {
            EventHandler handler = (_, _) => RefreshSize();
            topLevel.ScalingChanged += handler;
            _scalingSubscription = new DelegateDisposable(() => topLevel.ScalingChanged -= handler);
        }
        Restart(reload: false, configurationChanged: true);
    }

    internal void Detach()
    {
        _scalingSubscription?.Dispose();
        _scalingSubscription = null;
        CancelCurrentRequest();
        ReleaseCurrentResult();
        _currentSourceIdentity = null;
        _lastDecodeSize = null;
        _host.SetLoadedImage(null);
        _host.SetLoadState(ImageLoadState.Idle, null, null, false);
    }

    internal void SourceConfigurationChanged()
    {
        Restart(reload: false, configurationChanged: true);
    }

    internal void RefreshSize()
    {
        if (!_host.IsImageLoadAttached)
        {
            return;
        }
        RefreshSize(_host.GetDecodePixelSize());
    }

    internal void RefreshSize((int Width, int Height)? decodeSize)
    {
        if (!_host.IsImageLoadAttached)
        {
            return;
        }
        if (decodeSize == _lastDecodeSize)
        {
            return;
        }
        _lastDecodeSize = decodeSize;
        Restart(reload: false, configurationChanged: false, decodeSize);
    }

    internal void Reload()
    {
        Restart(reload: true, configurationChanged: false);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _scalingSubscription?.Dispose();
        _scalingSubscription = null;
        CancelCurrentRequest();
        ReleaseCurrentResult();
    }

    private void Restart(bool reload, bool configurationChanged)
    {
        if (_disposed || !_host.IsImageLoadAttached)
        {
            return;
        }
        Restart(reload, configurationChanged, _host.GetDecodePixelSize());
    }

    private void Restart(
        bool reload,
        bool configurationChanged,
        (int Width, int Height)? decodeSize)
    {
        if (_disposed || !_host.IsImageLoadAttached)
        {
            return;
        }
        var source = _host.Source;
        if (source is null)
        {
            CancelCurrentRequest();
            ReleaseCurrentResult();
            _currentSourceIdentity = null;
            _lastDecodeSize = null;
            _host.SetLoadedImage(null);
            _host.SetLoadState(ImageLoadState.Idle, null, null, false);
            return;
        }

        var configurationError = _host.GetConfigurationError();
        if (configurationError is not null)
        {
            CancelCurrentRequest();
            ReleaseCurrentResult();
            _currentSourceIdentity = source.CacheIdentity;
            _lastDecodeSize = null;
            _host.SetLoadedImage(null);
            _host.SetLoadState(ImageLoadState.Failed, configurationError, null, false);
            _host.RaiseImageFailed(new ImageFailedEventArgs(source, false, configurationError));
            return;
        }

        var sourceChanged = _currentSourceIdentity is not null && _currentSourceIdentity != source.CacheIdentity;
        if (sourceChanged)
        {
            CancelCurrentRequest();
            ReleaseCurrentResult();
            _host.SetLoadedImage(null);
        }
        _currentSourceIdentity = source.CacheIdentity;

        if (decodeSize is null)
        {
            _lastDecodeSize = null;
            if (_currentResult is null)
            {
                _host.SetLoadState(ImageLoadState.Loading, null, null, false);
            }
            return;
        }
        _lastDecodeSize = decodeSize;
        CancelCurrentRequest();
        var cancellation = new ImageCancellationState();
        _requestCancellation = cancellation;
        var generation = Interlocked.Increment(ref _generation);
        var fallback = _host.FallbackSource;
        var options = _host.RequestOptions;
        var priority = _host.Priority;
        _host.SetLoadState(ImageLoadState.Loading, null, null, false);
        _ = ObserveAsync(LoadGenerationAsync(
            source,
            fallback,
            options,
            priority,
            decodeSize.Value,
            generation,
            reload,
            cancellation));
    }

    private async Task LoadGenerationAsync(
        ImageSource source,
        ImageSource? fallback,
        ImageRequestOptions? requestOptions,
        ImageRequestPriority priority,
        (int Width, int Height) decodeSize,
        long generation,
        bool reload,
        ImageCancellationState cancellation)
    {
        var cancellationToken = cancellation.Token;
        try
        {
            var application = Application.Current ?? throw new InvalidOperationException(
                "Image controls require an active Avalonia Application.");
            var loader = application.GetImageLoader();
            var options = requestOptions;
            if (reload)
            {
                options = (options ?? new ImageRequestOptions()) with
                {
                    CacheRead = ImageCacheReadPolicy.RefreshSource
                };
            }
            var progress = new CallbackProgress<ImageLoadProgress>(value => PublishProgress(generation, value));
            var primaryResult = await loader.LoadAsync(
                new ImageLoadRequest(source)
                {
                    Options = options,
                    DecodePixelWidth = decodeSize.Width,
                    DecodePixelHeight = decodeSize.Height,
                    Priority = priority,
                    Progress = progress
                },
                cancellationToken).ConfigureAwait(false);
            if (primaryResult.IsSuccess)
            {
                await CommitSuccessAsync(generation, source, primaryResult, isFallback: false).ConfigureAwait(false);
                return;
            }

            var primaryError = primaryResult.Error!;
            primaryResult.Dispose();
            if (fallback is not null && fallback.CacheIdentity != source.CacheIdentity)
            {
                var fallbackResult = await loader.LoadAsync(
                    new ImageLoadRequest(fallback)
                    {
                        Options = options,
                        DecodePixelWidth = decodeSize.Width,
                        DecodePixelHeight = decodeSize.Height,
                        Priority = priority,
                        Progress = progress
                    },
                    cancellationToken).ConfigureAwait(false);
                if (fallbackResult.IsSuccess)
                {
                    await CommitSuccessAsync(generation, fallback, fallbackResult, isFallback: true).ConfigureAwait(false);
                    return;
                }
                var fallbackError = fallbackResult.Error!;
                fallbackResult.Dispose();
                await CommitFailureAsync(generation, fallback, fallbackError, isFallback: true).ConfigureAwait(false);
                return;
            }
            await CommitFailureAsync(generation, source, primaryError, isFallback: false).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception) when (ImageLoadEventDispatcher.IsNonFatal(exception))
        {
            var error = new ImageLoadError(
                ImageLoadErrorCode.InvalidSource,
                "The image loader is not available.",
                SourceDisplayName: source.DisplayName,
                Exception: exception);
            await CommitFailureAsync(generation, source, error, isFallback: false).ConfigureAwait(false);
        }
        finally
        {
            CompleteCancellation(ref _requestCancellation, cancellation);
        }
    }

    private void PublishProgress(long generation, ImageLoadProgress progress)
    {
        try
        {
            _host.Visual.Dispatcher.Post(() =>
            {
                if (generation == Volatile.Read(ref _generation) && _host.IsImageLoadAttached)
                {
                    _host.SetLoadState(ImageLoadState.Loading, null, progress, false);
                }
            });
        }
        catch (InvalidOperationException)
        {
            // The dispatcher can reject late progress during application shutdown.
        }
    }

    private async Task CommitSuccessAsync(
        long generation,
        ImageSource source,
        ImageLoadResult result,
        bool isFallback)
    {
        ImageLoadResult? pending = result;
        try
        {
            await _host.Visual.Dispatcher.InvokeAsync(() =>
            {
                var owned = pending!;
                pending = null;
                if (generation != Volatile.Read(ref _generation) || !_host.IsImageLoadAttached)
                {
                    owned.Dispose();
                    return;
                }
                var previous = _currentResult;
                _currentResult = owned;
                _host.SetLoadedImage(owned.Image);
                _host.SetLoadState(ImageLoadState.Loaded, null, null, isFallback);
                previous?.Dispose();
                _host.RaiseImageOpened(new ImageOpenedEventArgs(
                    source,
                    isFallback,
                    owned.Origin,
                    owned.DecodedPixelWidth,
                    owned.DecodedPixelHeight));
            });
        }
        finally
        {
            pending?.Dispose();
        }
    }

    private async Task CommitFailureAsync(
        long generation,
        ImageSource source,
        ImageLoadError error,
        bool isFallback)
    {
        await _host.Visual.Dispatcher.InvokeAsync(() =>
        {
            if (generation != Volatile.Read(ref _generation) || !_host.IsImageLoadAttached)
            {
                return;
            }
            _host.SetLoadedImage(null);
            _host.SetLoadState(ImageLoadState.Failed, error, null, false);
            ReleaseCurrentResult();
            _host.RaiseImageFailed(new ImageFailedEventArgs(source, isFallback, error));
        });
    }

    private void CancelCurrentRequest()
    {
        Interlocked.Increment(ref _generation);
        var cancellation = Interlocked.Exchange(ref _requestCancellation, null);
        if (cancellation is null)
        {
            return;
        }
        cancellation.Cancel();
        cancellation.ReleaseController();
    }

    private static void CompleteCancellation(
        ref ImageCancellationState? field,
        ImageCancellationState cancellation)
    {
        if (ReferenceEquals(Interlocked.CompareExchange(ref field, null, cancellation), cancellation))
        {
            cancellation.ReleaseController();
        }
        cancellation.ReleaseOperation();
    }

    private static async Task ObserveAsync(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (InvalidOperationException)
        {
            // A detached control may outlive the application dispatcher.
        }
        catch (Exception exception) when (ImageLoadEventDispatcher.IsNonFatal(exception))
        {
            // A late UI commit or observer must not become an unobserved task failure.
        }
    }

    private void ReleaseCurrentResult()
    {
        Interlocked.Exchange(ref _currentResult, null)?.Dispose();
    }

    private sealed class DelegateDisposable : IDisposable
    {
        private Action? _dispose;

        internal DelegateDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _dispose, null)?.Invoke();
        }
    }
}
