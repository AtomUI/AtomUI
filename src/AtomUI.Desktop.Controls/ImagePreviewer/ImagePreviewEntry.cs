using System.ComponentModel;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

internal sealed class ImagePreviewEntry : INotifyPropertyChanged, IDisposable
{
    private readonly Dispatcher _dispatcher;
    private ImageCancellationState? _fullCancellation;
    private ImageCancellationState? _thumbnailCancellation;
    private ImageRequestPriorityState? _fullPriorityState;
    private ImageRequestPriorityState? _thumbnailPriorityState;
    private ImageLoadResult? _fullResult;
    private ImageLoadResult? _thumbnailResult;
    private PixelSize? _fullRequestSize;
    private PixelSize? _thumbnailRequestSize;
    private PixelSize? _fullResultRequestSize;
    private PixelSize? _thumbnailResultRequestSize;
    private long _fullGeneration;
    private long _thumbnailGeneration;
    private ImageLoadState _fullState;
    private ImageLoadState _thumbnailState;
    private ImageLoadError? _fullError;
    private ImageLoadError? _thumbnailError;
    private ImageLoadProgress? _fullProgress;
    private ImageLoadProgress? _thumbnailProgress;
    private ImageLoadOrigin _fullOrigin;
    private bool _disposed;

    internal ImagePreviewEntry(ImagePreviewItem item)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        // Entries are created with their UI owner. Keep late callbacks on that
        // dispatcher rather than resolving another application's global UI thread.
        _dispatcher = Dispatcher.CurrentDispatcher;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    internal ImagePreviewItem Item { get; }

    internal IImage? FullImage => _fullResult?.Image;

    internal IImage? ThumbnailImage => _thumbnailResult?.Image ?? FullImage;

    internal Size FullImageSize => _fullResult is { OriginalPixelWidth: > 0, OriginalPixelHeight: > 0 } result
        ? new Size(result.OriginalPixelWidth, result.OriginalPixelHeight)
        : FullImage?.Size ?? default;

    internal ImageLoadState FullState
    {
        get => _fullState;
        private set
        {
            if (_fullState == value)
            {
                return;
            }
            _fullState = value;
            RaisePropertyChanged(nameof(FullState));
            RaisePropertyChanged(nameof(IsFullLoading));
            RaisePropertyChanged(nameof(IsFullLoaded));
            RaisePropertyChanged(nameof(IsFullFailed));
        }
    }

    internal ImageLoadState ThumbnailState
    {
        get => _thumbnailState;
        private set
        {
            if (_thumbnailState == value)
            {
                return;
            }
            _thumbnailState = value;
            RaisePropertyChanged(nameof(ThumbnailState));
            RaisePropertyChanged(nameof(IsThumbnailLoading));
            RaisePropertyChanged(nameof(IsThumbnailLoaded));
            RaisePropertyChanged(nameof(IsThumbnailFailed));
        }
    }

    internal ImageLoadError? FullError
    {
        get => _fullError;
        private set
        {
            if (_fullError == value)
            {
                return;
            }
            _fullError = value;
            RaisePropertyChanged(nameof(FullError));
        }
    }

    internal ImageLoadError? ThumbnailError
    {
        get => _thumbnailError;
        private set
        {
            if (_thumbnailError == value)
            {
                return;
            }
            _thumbnailError = value;
            RaisePropertyChanged(nameof(ThumbnailError));
        }
    }

    internal ImageLoadProgress? FullProgress
    {
        get => _fullProgress;
        private set
        {
            if (_fullProgress == value)
            {
                return;
            }
            _fullProgress = value;
            RaisePropertyChanged(nameof(FullProgress));
        }
    }

    internal ImageLoadProgress? ThumbnailProgress
    {
        get => _thumbnailProgress;
        private set
        {
            if (_thumbnailProgress == value)
            {
                return;
            }
            _thumbnailProgress = value;
            RaisePropertyChanged(nameof(ThumbnailProgress));
        }
    }

    internal bool IsFullLoading => FullState == ImageLoadState.Loading;

    internal bool IsFullLoaded => FullState == ImageLoadState.Loaded;

    internal bool IsFullFailed => FullState == ImageLoadState.Failed;

    internal bool IsThumbnailLoading => ThumbnailState == ImageLoadState.Loading;

    internal bool IsThumbnailLoaded => ThumbnailState == ImageLoadState.Loaded;

    internal bool IsThumbnailFailed => ThumbnailState == ImageLoadState.Failed;

    internal ImageLoadOrigin FullOrigin => _fullOrigin;

    internal long DisplayTargetOwnerId { get; set; }

    internal long DisplayTargetRevision { get; set; }

    internal void LoadFull(
        int decodePixelWidth,
        int decodePixelHeight,
        ImageRequestPriority priority,
        bool reload = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var requestSize = new PixelSize(decodePixelWidth, decodePixelHeight);
        if (!reload && FullState == ImageLoadState.Loading && Covers(_fullRequestSize, requestSize))
        {
            _fullPriorityState?.Promote(priority);
            return;
        }
        if (!RequiresLoad(
                FullState,
                requestSize,
                _fullRequestSize,
                _fullResultRequestSize,
                reload))
        {
            return;
        }
        var generation = ++_fullGeneration;
        Cancel(ref _fullCancellation);
        var cancellation = new ImageCancellationState();
        var priorityState = new ImageRequestPriorityState(priority);
        _fullCancellation = cancellation;
        _fullPriorityState = priorityState;
        _fullRequestSize = requestSize;
        FullError = null;
        FullProgress = null;
        FullState = ImageLoadState.Loading;
        _ = ObserveAsync(LoadFullAsync(
            generation,
            decodePixelWidth,
            decodePixelHeight,
            priorityState,
            reload,
            cancellation));
    }

    internal void LoadThumbnail(
        int decodePixelWidth,
        int decodePixelHeight,
        ImageRequestPriority priority,
        bool reload = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var requestSize = new PixelSize(decodePixelWidth, decodePixelHeight);
        if (!reload && ThumbnailState == ImageLoadState.Loading && Covers(_thumbnailRequestSize, requestSize))
        {
            _thumbnailPriorityState?.Promote(priority);
            return;
        }
        if (!RequiresLoad(
                ThumbnailState,
                requestSize,
                _thumbnailRequestSize,
                _thumbnailResultRequestSize,
                reload))
        {
            return;
        }
        var generation = ++_thumbnailGeneration;
        Cancel(ref _thumbnailCancellation);
        var cancellation = new ImageCancellationState();
        var priorityState = new ImageRequestPriorityState(priority);
        _thumbnailCancellation = cancellation;
        _thumbnailPriorityState = priorityState;
        _thumbnailRequestSize = requestSize;
        ThumbnailError = null;
        ThumbnailProgress = null;
        ThumbnailState = ImageLoadState.Loading;
        _ = ObserveAsync(LoadThumbnailAsync(
            generation,
            decodePixelWidth,
            decodePixelHeight,
            priorityState,
            reload,
            cancellation));
    }

    internal void CancelFullLoad()
    {
        _fullGeneration++;
        Interlocked.Exchange(ref _fullPriorityState, null);
        Cancel(ref _fullCancellation);
        _fullRequestSize = _fullResultRequestSize;
        FullError = null;
        FullProgress = null;
        if (_fullResult is null)
        {
            FullState = ImageLoadState.Idle;
        }
        else
        {
            FullState = ImageLoadState.Loaded;
        }
    }

    internal void CancelThumbnailLoad()
    {
        _thumbnailGeneration++;
        Interlocked.Exchange(ref _thumbnailPriorityState, null);
        Cancel(ref _thumbnailCancellation);
        _thumbnailRequestSize = _thumbnailResultRequestSize;
        ThumbnailError = null;
        ThumbnailProgress = null;
        if (_thumbnailResult is null)
        {
            ThumbnailState = ImageLoadState.Idle;
        }
        else
        {
            ThumbnailState = ImageLoadState.Loaded;
        }
    }

    internal void Unload()
    {
        _fullGeneration++;
        _thumbnailGeneration++;
        Interlocked.Exchange(ref _fullPriorityState, null);
        Interlocked.Exchange(ref _thumbnailPriorityState, null);
        Cancel(ref _fullCancellation);
        Cancel(ref _thumbnailCancellation);
        var fullResult = Interlocked.Exchange(ref _fullResult, null);
        var thumbnailResult = Interlocked.Exchange(ref _thumbnailResult, null);
        _fullRequestSize = null;
        _thumbnailRequestSize = null;
        _fullResultRequestSize = null;
        _thumbnailResultRequestSize = null;
        try
        {
            FullError = null;
            ThumbnailError = null;
            FullProgress = null;
            ThumbnailProgress = null;
            FullState = ImageLoadState.Idle;
            ThumbnailState = ImageLoadState.Idle;
            RaisePropertyChanged(nameof(FullImage));
            RaisePropertyChanged(nameof(ThumbnailImage));
        }
        finally
        {
            fullResult?.Dispose();
            thumbnailResult?.Dispose();
        }
    }

    internal void UnloadFull()
    {
        _fullGeneration++;
        Interlocked.Exchange(ref _fullPriorityState, null);
        Cancel(ref _fullCancellation);
        var fullResult = Interlocked.Exchange(ref _fullResult, null);
        _fullRequestSize = null;
        _fullResultRequestSize = null;
        try
        {
            FullError = null;
            FullProgress = null;
            FullState = ImageLoadState.Idle;
            RaisePropertyChanged(nameof(FullImage));
            RaisePropertyChanged(nameof(ThumbnailImage));
        }
        finally
        {
            fullResult?.Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        Interlocked.Exchange(ref _fullPriorityState, null);
        Interlocked.Exchange(ref _thumbnailPriorityState, null);
        Cancel(ref _fullCancellation);
        Cancel(ref _thumbnailCancellation);
        var fullResult = Interlocked.Exchange(ref _fullResult, null);
        var thumbnailResult = Interlocked.Exchange(ref _thumbnailResult, null);
        try
        {
            NotifyDisposedReset();
        }
        finally
        {
            PropertyChanged = null;
            fullResult?.Dispose();
            thumbnailResult?.Dispose();
        }
    }

    // Dispose 会静默释放位图租约；先通知持有者丢弃图像引用，
    // 避免 DisplayTracker 等订阅方继续渲染已释放的位图。
    // 无订阅者时零开销；有订阅者时使用缓存 EventArgs，零分配。
    private static readonly PropertyChangedEventArgs[] DisposedResetArgs =
    [
        new(nameof(FullState)),
        new(nameof(IsFullLoading)),
        new(nameof(IsFullLoaded)),
        new(nameof(IsFullFailed)),
        new(nameof(FullImage)),
        new(nameof(ThumbnailState)),
        new(nameof(IsThumbnailLoading)),
        new(nameof(IsThumbnailLoaded)),
        new(nameof(IsThumbnailFailed)),
        new(nameof(ThumbnailImage)),
    ];

    private void NotifyDisposedReset()
    {
        var handlers = PropertyChanged;
        if (handlers is null)
        {
            return;
        }
        _fullState         = ImageLoadState.Idle;
        _thumbnailState    = ImageLoadState.Idle;
        _fullError         = null;
        _thumbnailError    = null;
        _fullProgress      = null;
        _thumbnailProgress = null;
        foreach (var args in DisposedResetArgs)
        {
            handlers.Invoke(this, args);
        }
    }

    private async Task LoadFullAsync(
        long generation,
        int width,
        int height,
        ImageRequestPriorityState priorityState,
        bool reload,
        ImageCancellationState cancellation)
    {
        try
        {
            var result = await LoadWithFallbackAsync(
                [Item.Source, Item.FallbackSource],
                width,
                height,
                priorityState,
                reload,
                progress => PublishFullProgress(generation, progress),
                cancellation.Token).ConfigureAwait(false);
            ImageLoadResult? pending = result;
            try
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    var owned = pending!;
                    pending = null;
                    CommitFull(generation, new PixelSize(width, height), owned);
                });
            }
            finally
            {
                pending?.Dispose();
            }
        }
        catch (OperationCanceledException)
        {
            // 调用方取消与共享操作内部取消同样按取消处理，不产生失败提交
            HandleFullCanceled(generation);
        }
        finally
        {
            Interlocked.CompareExchange(ref _fullPriorityState, null, priorityState);
            CompleteCancellation(ref _fullCancellation, cancellation);
        }
    }

    private void HandleFullCanceled(long generation)
    {
        if (_disposed || generation != _fullGeneration)
        {
            return; // 已被更新的请求接管
        }
        FullProgress = null;
        FullError    = null;
        if (_fullResult is null)
        {
            FullState = ImageLoadState.Idle;
        }
        else
        {
            // 保留已提交的旧图（与 CancelFullLoad 的状态归位一致）
            FullState = ImageLoadState.Loaded;
        }
    }

    private async Task LoadThumbnailAsync(
        long generation,
        int width,
        int height,
        ImageRequestPriorityState priorityState,
        bool reload,
        ImageCancellationState cancellation)
    {
        try
        {
            var result = await LoadWithFallbackAsync(
                [Item.ThumbnailSource, Item.Source, Item.FallbackSource],
                width,
                height,
                priorityState,
                reload,
                progress => PublishThumbnailProgress(generation, progress),
                cancellation.Token).ConfigureAwait(false);
            ImageLoadResult? pending = result;
            try
            {
                await _dispatcher.InvokeAsync(() =>
                {
                    var owned = pending!;
                    pending = null;
                    CommitThumbnail(generation, new PixelSize(width, height), owned);
                });
            }
            finally
            {
                pending?.Dispose();
            }
        }
        catch (OperationCanceledException)
        {
            // 调用方取消与共享操作内部取消同样按取消处理，不产生失败提交
            HandleThumbnailCanceled(generation);
        }
        finally
        {
            Interlocked.CompareExchange(ref _thumbnailPriorityState, null, priorityState);
            CompleteCancellation(ref _thumbnailCancellation, cancellation);
        }
    }

    private void HandleThumbnailCanceled(long generation)
    {
        if (_disposed || generation != _thumbnailGeneration)
        {
            return; // 已被更新的请求接管
        }
        ThumbnailProgress = null;
        ThumbnailError    = null;
        if (_thumbnailResult is null)
        {
            ThumbnailState = ImageLoadState.Idle;
        }
        else
        {
            ThumbnailState = ImageLoadState.Loaded;
        }
    }

    private async Task<ImageLoadResult> LoadWithFallbackAsync(
        IEnumerable<ImageSource?> sources,
        int width,
        int height,
        ImageRequestPriorityState priorityState,
        bool reload,
        Action<ImageLoadProgress> reportProgress,
        CancellationToken cancellationToken)
    {
        try
        {
            var loader = (Application.Current ?? throw new InvalidOperationException(
                "ImagePreviewer requires an active Avalonia Application.")).GetImageLoader();
            var options = reload
                ? (Item.RequestOptions ?? new ImageRequestOptions()) with
                {
                    CacheRead = ImageCacheReadPolicy.RefreshSource
                }
                : Item.RequestOptions;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            ImageLoadResult? lastFailure = null;
            foreach (var source in sources)
            {
                if (source is null || !seen.Add(source.CacheIdentity))
                {
                    continue;
                }
                lastFailure?.Dispose();
                lastFailure = await loader.LoadAsync(
                    new ImageLoadRequest(source)
                    {
                        Options = options,
                        DecodePixelWidth = width,
                        DecodePixelHeight = height,
                        Priority = priorityState.Priority,
                        PriorityState = priorityState,
                        Progress = new CallbackProgress<ImageLoadProgress>(reportProgress)
                    },
                    cancellationToken).ConfigureAwait(false);
                if (lastFailure.IsSuccess)
                {
                    return lastFailure;
                }
            }
            return lastFailure ?? new ImageLoadResult(new ImageLoadError(
                ImageLoadErrorCode.InvalidSource,
                "No image source is configured."));
        }
        catch (Exception exception) when (exception is not OperationCanceledException &&
                                          ImageLoadEventDispatcher.IsNonFatal(exception))
        {
            return new ImageLoadResult(new ImageLoadError(
                ImageLoadErrorCode.InvalidSource,
                "The image loader is not available.",
                Exception: exception));
        }
    }

    private void CommitFull(long generation, PixelSize requestSize, ImageLoadResult result)
    {
        if (_disposed || generation != _fullGeneration)
        {
            result.Dispose();
            return;
        }
        FullProgress = null;
        if (result.IsSuccess)
        {
            var previous = _fullResult;
            _fullResult = result;
            _fullResultRequestSize = requestSize;
            _fullRequestSize = requestSize;
            _fullOrigin = result.Origin;
            FullError = null;
            FullState = ImageLoadState.Loaded;
            RaisePropertyChanged(nameof(FullImage));
            RaisePropertyChanged(nameof(ThumbnailImage));
            previous?.Dispose();
        }
        else
        {
            var previous = Interlocked.Exchange(ref _fullResult, null);
            _fullResultRequestSize = null;
            try
            {
                FullError = result.Error;
                FullState = ImageLoadState.Failed;
                RaisePropertyChanged(nameof(FullImage));
                RaisePropertyChanged(nameof(ThumbnailImage));
            }
            finally
            {
                previous?.Dispose();
                result.Dispose();
            }
        }
    }

    private void CommitThumbnail(long generation, PixelSize requestSize, ImageLoadResult result)
    {
        if (_disposed || generation != _thumbnailGeneration)
        {
            result.Dispose();
            return;
        }
        ThumbnailProgress = null;
        if (result.IsSuccess)
        {
            var previous = _thumbnailResult;
            _thumbnailResult = result;
            _thumbnailResultRequestSize = requestSize;
            _thumbnailRequestSize = requestSize;
            ThumbnailError = null;
            ThumbnailState = ImageLoadState.Loaded;
            RaisePropertyChanged(nameof(ThumbnailImage));
            previous?.Dispose();
        }
        else
        {
            var previous = Interlocked.Exchange(ref _thumbnailResult, null);
            _thumbnailResultRequestSize = null;
            try
            {
                ThumbnailError = result.Error;
                ThumbnailState = ImageLoadState.Failed;
                RaisePropertyChanged(nameof(ThumbnailImage));
            }
            finally
            {
                previous?.Dispose();
                result.Dispose();
            }
        }
    }

    private void PublishFullProgress(long generation, ImageLoadProgress progress)
    {
        try
        {
            _dispatcher.Post(() =>
            {
                if (!_disposed && generation == _fullGeneration && FullState == ImageLoadState.Loading)
                {
                    FullProgress = progress;
                }
            });
        }
        catch (InvalidOperationException)
        {
        }
        catch (Exception exception) when (ImageLoadEventDispatcher.IsNonFatal(exception))
        {
        }
    }

    private void PublishThumbnailProgress(long generation, ImageLoadProgress progress)
    {
        try
        {
            _dispatcher.Post(() =>
            {
                if (!_disposed && generation == _thumbnailGeneration && ThumbnailState == ImageLoadState.Loading)
                {
                    ThumbnailProgress = progress;
                }
            });
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static void Cancel(ref ImageCancellationState? cancellation)
    {
        var current = Interlocked.Exchange(ref cancellation, null);
        if (current is null)
        {
            return;
        }
        current.Cancel();
        current.ReleaseController();
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
        }
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

    private static bool RequiresLoad(
        ImageLoadState state,
        PixelSize requestSize,
        PixelSize? activeRequestSize,
        PixelSize? resultRequestSize,
        bool reload)
    {
        if (reload)
        {
            return true;
        }
        return state switch
        {
            // 同尺寸桶下的在途请求由 LoadFull/LoadThumbnail 提升优先级后直接采纳；
            // 不重启接近完成的源读取或解码。
            ImageLoadState.Loading => !Covers(activeRequestSize, requestSize),
            ImageLoadState.Loaded => !Covers(resultRequestSize, requestSize),
            _ => true
        };
    }

    private static bool Covers(PixelSize? existingRequestSize, PixelSize requestedSize)
    {
        if (existingRequestSize is not { } existing)
        {
            return false;
        }
        return CoversAxis(existing.Width, requestedSize.Width) &&
               CoversAxis(existing.Height, requestedSize.Height);
    }

    private static bool CoversAxis(int existing, int requested)
    {
        return existing == 0 || requested > 0 && existing >= requested;
    }

    private void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
