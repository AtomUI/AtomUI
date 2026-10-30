namespace AtomUI.Controls;

internal sealed class ImageEncodedCache : IDisposable
{
    private readonly object _gate = new();
    private readonly Dictionary<ImageEncodedContentKey, CacheItem> _items = [];
    private readonly LinkedList<ImageEncodedContentKey> _lru = [];
    private readonly long _maxBytes;
    private readonly int _maxEntries;
    private long _bytes;
    private bool _disposed;

    internal ImageEncodedCache(long maxBytes, int maxEntries)
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

    internal bool TryGet(ImageEncodedContentKey key, out ImageEncodedContent? content)
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            if (!_items.TryGetValue(key, out var item))
            {
                content = null;
                return false;
            }
            _lru.Remove(item.Node);
            _lru.AddFirst(item.Node);
            content = item.Content.WithOrigin(ImageLoadOrigin.EncodedMemory);
            return true;
        }
    }

    internal bool Set(ImageEncodedContentKey key, ImageEncodedContent content)
    {
        if (content.NoStore ||
            content.SecurityPolicyVersion != ImageSecurityPolicy.Version ||
            content.ContentId != key.ContentId ||
            content.Size > _maxBytes)
        {
            return false;
        }

        lock (_gate)
        {
            ThrowIfDisposed();
            RemoveCore(key);
            var node = _lru.AddFirst(key);
            var stored = content.ForContentStore();
            _items.Add(key, new CacheItem(stored, node));
            _bytes += stored.Size;
            TrimCore();
            return _items.ContainsKey(key);
        }
    }

    internal void Remove(ImageEncodedContentKey key)
    {
        lock (_gate)
        {
            if (!_disposed)
            {
                RemoveCore(key);
            }
        }
    }

    internal void Clear(string? partitionHash)
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }
            if (partitionHash is null)
            {
                _items.Clear();
                _lru.Clear();
                _bytes = 0;
                return;
            }
            foreach (var key in _items.Keys.Where(key => key.PartitionHash == partitionHash).ToArray())
            {
                RemoveCore(key);
            }
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _items.Clear();
            _lru.Clear();
            _bytes = 0;
        }
    }

    private void TrimCore()
    {
        while ((_bytes > _maxBytes || _items.Count > _maxEntries) && _lru.Last is { } node)
        {
            RemoveCore(node.Value);
        }
    }

    private void RemoveCore(ImageEncodedContentKey key)
    {
        if (!_items.Remove(key, out var existing))
        {
            return;
        }
        _lru.Remove(existing.Node);
        _bytes -= existing.Content.Size;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private sealed record CacheItem(ImageEncodedContent Content, LinkedListNode<ImageEncodedContentKey> Node);
}
