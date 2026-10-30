namespace AtomUI.Controls;

internal sealed class ImageLoaderPipeline : IDisposable
{
    private readonly object _epochGate = new();
    private readonly SemaphoreSlim _persistentGate = new(1, 1);
    private readonly ImageEncodedCache _encodedCache;
    private readonly ImageDecodedCache _decodedCache;
    private readonly ImageSourceSnapshotIndex _sourceSnapshots = new();
    private readonly ImageFileCache? _fileCache;
    private readonly ImageRequestScheduler _scheduler;
    private readonly ImageRequestCoordinator _coordinator;
    private readonly ImageSourceReaderRegistry _readers;
    private readonly ImageContentValidator _validator;
    private readonly ImageCodecRegistry _codecs;
    private readonly DeferredHttpImageTransport _transport;
    private readonly Dictionary<string, long> _partitionEpochs = [];
    private long _globalEpoch;
    private int _disposeRequested;
    private bool _disposed;

    internal ImageLoaderPipeline(
        ImageLoadingOptions options,
        IEnumerable<ImageCodec> codecs,
        HttpMessageHandler? httpMessageHandler = null,
        Func<HttpImageTransport>? httpTransportFactory = null)
    {
        _encodedCache = new ImageEncodedCache(options.EncodedMemoryCacheBytes, options.EncodedMemoryCacheEntries);
        _decodedCache = new ImageDecodedCache(options.DecodedMemoryCacheBytes, options.DecodedMemoryCacheEntries);
        _fileCache = options.IsPersistentCacheEnabled
            ? new ImageFileCache(
                options.PersistentCacheDirectory!,
                options.PersistentCacheBytes,
                options.PersistentCacheEntries)
            : null;
        _scheduler = new ImageRequestScheduler(
            options.MaxConcurrentDownloads,
            options.MaxConcurrentLocalReads,
            options.MaxConcurrentDecodes);
        _coordinator = new ImageRequestCoordinator();
        _validator = new ImageContentValidator(options);
        _codecs = new ImageCodecRegistry(codecs);
        _transport = new DeferredHttpImageTransport(
            httpTransportFactory ?? (() => new HttpImageTransport(options, httpMessageHandler)),
            httpTransportFactory is null ? httpMessageHandler : null);
        _readers = new ImageSourceReaderRegistry(
        [
            new HttpImageSourceReader(_transport.Get),
            new FileImageSourceReader(options),
            new AssetImageSourceReader(options),
            new StorageFileImageSourceReader(options),
            new BytesImageSourceReader(options),
            new StreamImageSourceReader(options),
            new BorrowedImageSourceReader()
        ]);
    }

    internal int ActiveReads => _scheduler.ActiveReads;
    internal int QueuedReads => _scheduler.QueuedReads;
    internal int ActiveDecodes => _scheduler.ActiveDecodes;
    internal int QueuedDecodes => _scheduler.QueuedDecodes;
    internal bool HasInFlightWork =>
        _coordinator.HasInFlightOperations ||
        ActiveReads != 0 ||
        QueuedReads != 0 ||
        ActiveDecodes != 0 ||
        QueuedDecodes != 0;
    internal int EncodedCacheEntries => _encodedCache.Count;
    internal long EncodedCacheBytes => _encodedCache.Bytes;
    internal int DecodedCacheEntries => _decodedCache.Count;
    internal long DecodedCacheBytes => _decodedCache.Bytes;

    internal async Task<ImageLoadResult> LoadAsync(
        NormalizedImageRequest request,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed || Volatile.Read(ref _disposeRequested) != 0, this);
        ImageProgressDispatcher.Report(request.Progress, ImageLoadProgress.Create(ImageLoadStage.Resolving));
        if (request.Source.Kind == ImageSourceKind.Borrowed)
        {
            return await LoadBorrowedImageAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var epoch = GetEpoch(request.PartitionHash);
        var validated = await _coordinator.GetSourceAsync(
            request.SourceOperationKey,
            request.PriorityState,
            (context, operationCancellation) => ResolveSourceCoreAsync(
                request,
                epoch,
                context,
                operationCancellation),
            request.Progress,
            cancellationToken).ConfigureAwait(false);
        var content = validated.Content;
        var contentId = content.ContentId ?? throw new InvalidOperationException("Validated content has no content identity.");
        var codec = _codecs.Select(validated.Probe, request.Source);
        var decodeKey = codec.CreateDecodeKey(request, contentId);

        if (!content.NoStore && request.CanReadSharedCache &&
            _decodedCache.TryAcquireResult(
                decodeKey,
                ImageLoadOrigin.DecodedMemory,
                validated.SourceValidation,
                request.Timing.Snapshot(),
                out var cachedResult))
        {
            ImageProgressDispatcher.Report(request.Progress, ImageLoadProgress.Create(ImageLoadStage.CacheLookup));
            return cachedResult!;
        }

        var operationKey = content.NoStore
            ? new ImageDecodedOperationKey(
                decodeKey,
                request.CacheStorage,
                Guid.NewGuid().ToString("N"))
            : ImageCacheKey.CreateDecodedOperationKey(request, decodeKey);
        return await _coordinator.GetDecodedAsync(
            operationKey,
            request.PriorityState,
            (context, operationCancellation) => DecodeCoreAsync(
                request,
                validated,
                codec,
                decodeKey,
                epoch,
                context,
                operationCancellation),
            entry => entry.AcquireResult(
                entry.Origin,
                validated.SourceValidation,
                contentId.Value,
                request.Timing.Snapshot()),
            request.Progress,
            cancellationToken).ConfigureAwait(false);
    }

    internal async ValueTask ClearCacheAsync(
        ImageCacheClearRequest request,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed || Volatile.Read(ref _disposeRequested) != 0, this);
        ArgumentNullException.ThrowIfNull(request);
        var partitionHash = request.CachePartition is null
            ? null
            : ImageCacheKey.Hash(request.CachePartition);
        IncrementEpoch(partitionHash);
        _sourceSnapshots.Clear(partitionHash);
        if (request.CancelInFlight)
        {
            _coordinator.Cancel(partitionHash);
        }
        if (request.ClearDecodedMemory)
        {
            _decodedCache.Clear(partitionHash);
        }
        if (request.ClearEncodedMemory)
        {
            _encodedCache.Clear(partitionHash);
        }
        if (request.ClearPersistent && _fileCache is not null)
        {
            await _persistentGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await _fileCache.ClearAsync(partitionHash, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _persistentGate.Release();
            }
        }
    }

    public void Dispose()
    {
        BeginDispose();
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _coordinator.Dispose();
        _scheduler.Dispose();
        _decodedCache.Dispose();
        _encodedCache.Dispose();
        _fileCache?.Dispose();
        _transport.Dispose();
    }

    internal void BeginDispose()
    {
        if (Interlocked.Exchange(ref _disposeRequested, 1) == 0)
        {
            _transport.CloseAcquisition();
        }
    }

    private async Task<ImageDecodedCacheEntry> DecodeCoreAsync(
        NormalizedImageRequest request,
        ImageValidatedContent validated,
        ImageCodec codec,
        ImageDecodeKey decodeKey,
        ImageCacheEpoch epoch,
        ImageRequestCoordinator.SharedOperationContext context,
        CancellationToken cancellationToken)
    {
        var content = validated.Content;
        context.Report(ImageLoadProgress.Create(ImageLoadStage.Queued));
        var decoded = await _scheduler.ScheduleDecodeAsync(
            async token =>
            {
                context.Report(ImageLoadProgress.Create(ImageLoadStage.Decoding));
                return await codec.DecodeAsync(content, validated.Probe, request, token).ConfigureAwait(false);
            },
            () => context.Priority,
            cancellationToken,
            static entry => entry.Discard()).ConfigureAwait(false);
        decoded.RetainOperation();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!content.NoStore && request.CanWriteMemory)
            {
                TryAddDecodedIfEpochCurrent(request.PartitionHash, epoch, decodeKey, decoded);
            }
            return decoded;
        }
        catch
        {
            decoded.ReleaseOperation();
            throw;
        }
    }

    private Task<ImageLoadResult> LoadBorrowedImageAsync(
        NormalizedImageRequest request,
        CancellationToken cancellationToken)
    {
        return _coordinator.GetDecodedAsync(
            ImageCacheKey.CreateBorrowedDecodedOperationKey(request),
            request.PriorityState,
            (context, operationCancellation) => LoadBorrowedImageCoreAsync(
                request,
                context,
                operationCancellation),
            entry => entry.AcquireResult(
                ImageLoadOrigin.Borrowed,
                ImageSourceValidation.NotRequired,
                null,
                request.Timing.Snapshot()),
            request.Progress,
            cancellationToken);
    }

    private async Task<ImageDecodedCacheEntry> LoadBorrowedImageCoreAsync(
        NormalizedImageRequest request,
        ImageRequestCoordinator.SharedOperationContext context,
        CancellationToken cancellationToken)
    {
        context.Report(ImageLoadProgress.Create(ImageLoadStage.Reading));
        var readResult = await _readers.Get(request.Source.Kind)
            .ReadAsync(request, null, null, cancellationToken)
            .ConfigureAwait(false);
        var borrowed = readResult.BorrowedImage!;
        var width = Math.Max(1, (int)Math.Ceiling(borrowed.Size.Width));
        var height = Math.Max(1, (int)Math.Ceiling(borrowed.Size.Height));
        var entry = new ImageDecodedCacheEntry(
            borrowed,
            ownsImage: false,
            width,
            height,
            width,
            height,
            checked((long)width * height * 4),
            null,
            ImageLoadOrigin.Borrowed);
        entry.RetainOperation();
        return entry;
    }

    private async Task<ImageValidatedContent> ResolveSourceCoreAsync(
        NormalizedImageRequest request,
        ImageCacheEpoch epoch,
        ImageRequestCoordinator.SharedOperationContext context,
        CancellationToken cancellationToken)
    {
        context.Report(ImageLoadProgress.Create(ImageLoadStage.CacheLookup));
        var (snapshot, cached) = await TryGetSnapshotContentAsync(
            request,
            epoch,
            cancellationToken).ConfigureAwait(false);
        var generation = _sourceSnapshots.BeginResolution(request.SourceKey);
        if (snapshot is not null && cached is not null)
        {
            if (!MatchesVary(request, snapshot))
            {
                _sourceSnapshots.Remove(request.SourceKey);
                if (_fileCache is not null)
                {
                    await _fileCache.RemoveSourceSnapshotAsync(request.SourceKey, cancellationToken).ConfigureAwait(false);
                }
                snapshot = null;
                cached = null;
            }
            else if (request.CacheRead is ImageCacheReadPolicy.PreferCache or ImageCacheReadPolicy.CacheOnly)
            {
                var cachedValidated = ValidateContent(
                    ApplySnapshot(cached, snapshot),
                    request.Source,
                    ImageSourceValidation.Unverified,
                    cancellationToken);
                PromoteValidatedContentIfAllowed(request, epoch, cachedValidated.Content);
                return cachedValidated;
            }
            else if (CanReuseValidatedSnapshot(request, snapshot))
            {
                var validation = request.Source.Kind == ImageSourceKind.Bytes
                    ? ImageSourceValidation.NotRequired
                    : ImageSourceValidation.Current;
                var cachedValidated = ValidateContent(
                    ApplySnapshot(cached, snapshot),
                    request.Source,
                    validation,
                    cancellationToken);
                PromoteValidatedContentIfAllowed(request, epoch, cachedValidated.Content);
                return cachedValidated;
            }
        }

        if (request.CacheRead == ImageCacheReadPolicy.CacheOnly)
        {
            throw ImageSourceReadHelpers.Failure(
                ImageLoadErrorCode.CacheMiss,
                "The requested image is not available in cache.",
                request.Source.DisplayName);
        }

        context.Report(ImageLoadProgress.Create(ImageLoadStage.Queued));
        var result = await _scheduler.ScheduleReadAsync(
            request.Source.Kind,
            token => _readers.Get(request.Source.Kind).ReadAsync(
                request,
                snapshot is null || cached is null ? null : ApplySnapshot(cached, snapshot),
                new CallbackProgress<ImageLoadProgress>(context.Report),
                token),
            () => context.Priority,
            cancellationToken).ConfigureAwait(false);
        var content = result.EncodedContent ??
                      throw new InvalidOperationException("Source reader did not return encoded content.");
        context.Report(ImageLoadProgress.Create(
            ImageLoadStage.Validating,
            content.Bytes.LongLength,
            content.Bytes.LongLength));
        var validated = ValidateContent(content, request.Source, result.SourceValidation, cancellationToken);
        content = validated.Content;
        var contentId = content.ContentId!.Value;

        if (content.NoStore)
        {
            await RemoveStoredSourceVariantAsync(
                request,
                epoch,
                snapshot,
                contentId,
                generation,
                cancellationToken).ConfigureAwait(false);
            return validated;
        }

        if (request.CanWriteMemory)
        {
            var contentKey = new ImageEncodedContentKey(request.PartitionHash, contentId);
            var nextSnapshot = CreateSnapshot(request, content, contentId, generation);
            if (request.CanPersist && _fileCache is not null)
            {
                await _persistentGate.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    if (!IsEpochCurrent(request.PartitionHash, epoch))
                    {
                        return validated;
                    }
                    var persisted = await _fileCache.SetContentAsync(
                        contentKey,
                        content,
                        cancellationToken).ConfigureAwait(false);
                    if (TryCommitContentAndSnapshotIfEpochCurrent(
                            request.PartitionHash,
                            epoch,
                            contentKey,
                            content,
                            nextSnapshot,
                            persisted) && persisted)
                    {
                        await _fileCache.SetSourceSnapshotAsync(nextSnapshot, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    _persistentGate.Release();
                }
            }
            else
            {
                TryCommitContentAndSnapshotIfEpochCurrent(
                    request.PartitionHash,
                    epoch,
                    contentKey,
                    content,
                    nextSnapshot,
                    contentPersisted: false);
            }
        }
        return validated;
    }

    private async Task RemoveStoredSourceVariantAsync(
        NormalizedImageRequest request,
        ImageCacheEpoch epoch,
        ImageSourceSnapshot? previousSnapshot,
        ImageContentId currentContentId,
        long generation,
        CancellationToken cancellationToken)
    {
        ImageContentId[] contentIds = previousSnapshot is null || previousSnapshot.ContentId == currentContentId
            ? [currentContentId]
            : [previousSnapshot.ContentId, currentContentId];
        List<ImageDecodedCacheEntry> removedDecoded = [];
        lock (_epochGate)
        {
            if (!IsEpochCurrentCore(request.PartitionHash, epoch) ||
                !_sourceSnapshots.RemoveIfNotNewer(request.SourceKey, generation))
            {
                return;
            }
            foreach (var contentId in contentIds)
            {
                _encodedCache.Remove(new ImageEncodedContentKey(request.PartitionHash, contentId));
                removedDecoded.AddRange(_decodedCache.ExtractByContentId(request.PartitionHash, contentId));
            }
        }
        ImageDecodedCache.ReleaseMemberships(removedDecoded);

        if (_fileCache is null)
        {
            return;
        }
        await _persistentGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var removed = await _fileCache.RemoveSourceSnapshotAsync(
                request.SourceKey,
                cancellationToken,
                generation).ConfigureAwait(false);
            if (!removed)
            {
                return;
            }
            foreach (var contentId in contentIds)
            {
                await _fileCache.RemoveContentAsync(
                    new ImageEncodedContentKey(request.PartitionHash, contentId),
                    cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _persistentGate.Release();
        }
    }

    private async Task<(ImageSourceSnapshot? Snapshot, ImageEncodedContent? Content)> TryGetSnapshotContentAsync(
        NormalizedImageRequest request,
        ImageCacheEpoch epoch,
        CancellationToken cancellationToken)
    {
        if (!request.CanReadSharedCache)
        {
            return (null, null);
        }

        _sourceSnapshots.TryGet(request.SourceKey, out var snapshot);
        if (snapshot is null && request.CanReadPersistent && _fileCache is not null)
        {
            snapshot = await _fileCache.TryGetSourceSnapshotAsync(
                request.SourceKey,
                cancellationToken).ConfigureAwait(false);
            if (snapshot is not null && request.CanWriteMemory)
            {
                if (!TryCommitSnapshotIfEpochCurrent(request.PartitionHash, epoch, snapshot))
                {
                    _sourceSnapshots.TryGet(request.SourceKey, out snapshot);
                }
            }
        }
        if (snapshot is null || snapshot.SecurityPolicyVersion != ImageSecurityPolicy.Version)
        {
            return (null, null);
        }

        var contentKey = new ImageEncodedContentKey(request.PartitionHash, snapshot.ContentId);
        if (_encodedCache.TryGet(contentKey, out var memoryContent))
        {
            return (snapshot, memoryContent);
        }
        if (!request.CanReadPersistent || _fileCache is null)
        {
            return (snapshot, null);
        }
        var persistent = await _fileCache.TryGetContentAsync(contentKey, cancellationToken).ConfigureAwait(false);
        if (persistent is null)
        {
            return (snapshot, null);
        }
        return (snapshot, persistent);
    }

    private void PromoteValidatedContentIfAllowed(
        NormalizedImageRequest request,
        ImageCacheEpoch epoch,
        ImageEncodedContent content)
    {
        if (!request.CanWriteMemory || content.NoStore || content.ContentId is not { } contentId)
        {
            return;
        }
        TrySetEncodedIfEpochCurrent(
            request.PartitionHash,
            epoch,
            new ImageEncodedContentKey(request.PartitionHash, contentId),
            content);
    }

    private ImageValidatedContent ValidateContent(
        ImageEncodedContent content,
        ImageSource source,
        ImageSourceValidation sourceValidation,
        CancellationToken cancellationToken)
    {
        var probe = content.SecurityPolicyVersion == ImageSecurityPolicy.Version &&
                    content.ContentId is not null &&
                    content.Probe is { } storedProbe
            ? storedProbe
            : _validator.Validate(content, source, cancellationToken);
        return new ImageValidatedContent(content.MarkValidated(probe), probe, sourceValidation);
    }

    private static ImageSourceSnapshot CreateSnapshot(
        NormalizedImageRequest request,
        ImageEncodedContent content,
        ImageContentId contentId,
        long generation)
    {
        var sourceVersion = content.SourceVersion ?? request.Source.SourceRevision ?? new ImageSourceVersion(contentId.Value);
        return new ImageSourceSnapshot(
            request.SourceKey,
            sourceVersion,
            contentId,
            generation,
            content.StoredAt,
            content.FreshUntil,
            content.ETag,
            content.LastModified,
            content.NoCache,
            content.MustRevalidate,
            content.IsRemote,
            content.IsTrustedAsset,
            content.VaryHeaders,
            content.VaryDigest,
            content.ResponseDate,
            content.ResponseAge,
            content.Expires,
            content.MaxAge,
            content.IsPrivate,
            content.SecurityPolicyVersion);
    }

    private static ImageEncodedContent ApplySnapshot(
        ImageEncodedContent content,
        ImageSourceSnapshot snapshot) =>
        content with
        {
            StoredAt = snapshot.StoredAt,
            FreshUntil = snapshot.FreshUntil,
            ETag = snapshot.ETag,
            LastModified = snapshot.LastModified,
            NoCache = snapshot.NoCache,
            MustRevalidate = snapshot.MustRevalidate,
            IsRemote = snapshot.IsRemote,
            IsTrustedAsset = snapshot.IsTrustedAsset,
            SourceVersion = snapshot.SourceVersion,
            VaryHeaders = snapshot.VaryHeaders,
            VaryDigest = snapshot.VaryDigest,
            SecurityPolicyVersion = snapshot.SecurityPolicyVersion,
            ResponseDate = snapshot.ResponseDate,
            ResponseAge = snapshot.ResponseAge,
            Expires = snapshot.Expires,
            MaxAge = snapshot.MaxAge,
            IsPrivate = snapshot.IsPrivate,
            ContentId = snapshot.ContentId
        };

    private static bool CanReuseValidatedSnapshot(
        NormalizedImageRequest request,
        ImageSourceSnapshot snapshot)
    {
        if (request.CacheRead == ImageCacheReadPolicy.RefreshSource)
        {
            return false;
        }
        return request.Source.Kind switch
        {
            ImageSourceKind.Http => snapshot.IsFresh(DateTimeOffset.UtcNow),
            ImageSourceKind.Asset or ImageSourceKind.Bytes => true,
            ImageSourceKind.StorageFile or ImageSourceKind.Stream =>
                request.Source.SourceRevision is not null &&
                request.Source.SourceRevision == snapshot.SourceVersion,
            _ => false
        };
    }

    private static bool MatchesVary(
        NormalizedImageRequest request,
        ImageSourceSnapshot snapshot) =>
        snapshot.VaryHeaders is not { Length: > 0 } varyHeaders ||
        string.Equals(
            snapshot.VaryDigest,
            ImageCacheKey.HashSelectedHeaders(request.Headers, varyHeaders),
            StringComparison.Ordinal);

    private ImageCacheEpoch GetEpoch(string partitionHash)
    {
        lock (_epochGate)
        {
            _partitionEpochs.TryGetValue(partitionHash, out var partitionEpoch);
            return new ImageCacheEpoch(_globalEpoch, partitionEpoch);
        }
    }

    private bool IsEpochCurrent(string partitionHash, ImageCacheEpoch epoch) =>
        GetEpoch(partitionHash) == epoch;

    private bool TrySetEncodedIfEpochCurrent(
        string partitionHash,
        ImageCacheEpoch epoch,
        ImageEncodedContentKey key,
        ImageEncodedContent content)
    {
        lock (_epochGate)
        {
            if (!IsEpochCurrentCore(partitionHash, epoch))
            {
                return false;
            }
            return _encodedCache.Set(key, content);
        }
    }

    private bool TryCommitContentAndSnapshotIfEpochCurrent(
        string partitionHash,
        ImageCacheEpoch epoch,
        ImageEncodedContentKey contentKey,
        ImageEncodedContent content,
        ImageSourceSnapshot snapshot,
        bool contentPersisted)
    {
        lock (_epochGate)
        {
            if (!IsEpochCurrentCore(partitionHash, epoch))
            {
                return false;
            }
            var contentInMemory = _encodedCache.Set(contentKey, content);
            return (contentInMemory || contentPersisted) && _sourceSnapshots.TryCommit(snapshot);
        }
    }

    private bool TryAddDecodedIfEpochCurrent(
        string partitionHash,
        ImageCacheEpoch epoch,
        ImageDecodeKey key,
        ImageDecodedCacheEntry entry)
    {
        lock (_epochGate)
        {
            return IsEpochCurrentCore(partitionHash, epoch) && _decodedCache.TryAdd(key, entry);
        }
    }

    private bool TryCommitSnapshotIfEpochCurrent(
        string partitionHash,
        ImageCacheEpoch epoch,
        ImageSourceSnapshot snapshot)
    {
        lock (_epochGate)
        {
            return IsEpochCurrentCore(partitionHash, epoch) && _sourceSnapshots.TryCommit(snapshot);
        }
    }

    private bool IsEpochCurrentCore(string partitionHash, ImageCacheEpoch epoch)
    {
        _partitionEpochs.TryGetValue(partitionHash, out var partitionEpoch);
        return epoch == new ImageCacheEpoch(_globalEpoch, partitionEpoch);
    }

    private void IncrementEpoch(string? partitionHash)
    {
        lock (_epochGate)
        {
            if (partitionHash is null)
            {
                _globalEpoch++;
                _partitionEpochs.Clear();
            }
            else
            {
                _partitionEpochs.TryGetValue(partitionHash, out var current);
                _partitionEpochs[partitionHash] = current + 1;
            }
        }
    }
}
