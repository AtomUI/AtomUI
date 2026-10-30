namespace AtomUI.Controls;

internal sealed class ImageDecodedCache : IDisposable
{
    private readonly object _gate = new();
    private readonly Dictionary<ImageDecodeKey, CacheItem> _items = [];
    private readonly LinkedList<ImageDecodeKey> _lru = [];
    private readonly long _maxBytes;
    private readonly int _maxEntries;
    private long _bytes;
    private bool _disposed;

    internal ImageDecodedCache(long maxBytes, int maxEntries)
    {
        _maxBytes = maxBytes;
        _maxEntries = maxEntries;
    }

    internal int Count
    {
        get { lock (_gate)
            {
                return _items.Count;
            }
        }
    }

    internal long Bytes
    {
        get { lock (_gate)
            {
                return _bytes;
            }
        }
    }

    internal bool TryAcquireResult(
        ImageDecodeKey key,
        ImageLoadOrigin origin,
        ImageSourceValidation sourceValidation,
        IReadOnlyDictionary<ImageLoadStage, TimeSpan>? stageDurations,
        out ImageLoadResult? result)
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            if (!_items.TryGetValue(key, out var item))
            {
                result = null;
                return false;
            }
            _lru.Remove(item.Node);
            _lru.AddFirst(item.Node);
            result = item.Entry.AcquireResult(
                origin,
                sourceValidation,
                key.ContentId.Value,
                stageDurations);
            return true;
        }
    }

    internal bool TryRetain(ImageDecodeKey key, out ImageDecodedCacheEntry? entry)
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            if (!_items.TryGetValue(key, out var item))
            {
                entry = null;
                return false;
            }
            _lru.Remove(item.Node);
            _lru.AddFirst(item.Node);
            item.Entry.RetainOperation();
            entry = item.Entry;
            return true;
        }
    }

    internal bool TryAdd(ImageDecodeKey key, ImageDecodedCacheEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.DecodedBytes > _maxBytes)
        {
            return false;
        }

        List<ImageDecodedCacheEntry>? removed = null;
        lock (_gate)
        {
            ThrowIfDisposed();
            if (_items.TryGetValue(key, out var existing) && ReferenceEquals(existing.Entry, entry))
            {
                return true;
            }
            if (existing is not null)
            {
                removed = [existing.Entry];
                RemoveCore(key);
            }

            entry.AddCacheMembership();
            var node = _lru.AddFirst(key);
            _items.Add(key, new CacheItem(entry, node));
            _bytes += entry.DecodedBytes;
            removed ??= [];
            TrimCore(removed);
        }
        ReleaseMemberships(removed);
        return true;
    }

    internal void Clear(string? partitionHash)
    {
        List<ImageDecodedCacheEntry> removed = [];
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }
            var keys = partitionHash is null
                ? _items.Keys.ToArray()
                : _items.Keys.Where(key => key.PartitionHash == partitionHash).ToArray();
            foreach (var key in keys)
            {
                var entry = _items[key].Entry;
                RemoveCore(key);
                removed.Add(entry);
            }
        }
        ReleaseMemberships(removed);
    }

    internal List<ImageDecodedCacheEntry> ExtractByContentId(
        string partitionHash,
        ImageContentId contentId)
    {
        List<ImageDecodedCacheEntry> removed = [];
        lock (_gate)
        {
            if (_disposed)
            {
                return removed;
            }
            foreach (var key in _items.Keys
                         .Where(key => key.PartitionHash == partitionHash && key.ContentId == contentId)
                         .ToArray())
            {
                var entry = _items[key].Entry;
                RemoveCore(key);
                removed.Add(entry);
            }
        }
        return removed;
    }

    public void Dispose()
    {
        List<ImageDecodedCacheEntry> removed;
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            removed = _items.Values.Select(item => item.Entry).ToList();
            _items.Clear();
            _lru.Clear();
            _bytes = 0;
        }
        ReleaseMemberships(removed);
    }

    private void TrimCore(List<ImageDecodedCacheEntry> removed)
    {
        while ((_bytes > _maxBytes || _items.Count > _maxEntries) && _lru.Last is { } node)
        {
            var entry = _items[node.Value].Entry;
            RemoveCore(node.Value);
            removed.Add(entry);
        }
    }

    private void RemoveCore(ImageDecodeKey key)
    {
        if (!_items.Remove(key, out var item))
        {
            return;
        }
        _lru.Remove(item.Node);
        _bytes -= item.Entry.DecodedBytes;
    }

    internal static void ReleaseMemberships(IEnumerable<ImageDecodedCacheEntry> entries)
    {
        foreach (var entry in entries)
        {
            entry.RemoveCacheMembership();
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private sealed record CacheItem(ImageDecodedCacheEntry Entry, LinkedListNode<ImageDecodeKey> Node);
}
