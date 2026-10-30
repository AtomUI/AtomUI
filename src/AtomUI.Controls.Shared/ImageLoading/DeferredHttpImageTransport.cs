namespace AtomUI.Controls;

internal sealed class DeferredHttpImageTransport : IDisposable
{
    private readonly object _gate = new();
    private Func<HttpImageTransport>? _factory;
    private HttpImageTransport? _transport;
    private IDisposable? _pendingResource;
    private bool _isValueCreated;
    private bool _acquisitionClosed;
    private bool _disposed;

    internal DeferredHttpImageTransport(
        Func<HttpImageTransport> factory,
        IDisposable? pendingResource = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
        _pendingResource = pendingResource;
    }

    internal bool IsValueCreated
    {
        get
        {
            lock (_gate)
            {
                return _isValueCreated;
            }
        }
    }

    internal HttpImageTransport Get()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_acquisitionClosed || _disposed, this);
            if (_transport is not null)
            {
                return _transport;
            }

            var transport = _factory!();
            ArgumentNullException.ThrowIfNull(transport);
            _transport = transport;
            _factory = null;
            _pendingResource = null;
            _isValueCreated = true;
            return transport;
        }
    }

    internal void CloseAcquisition()
    {
        IDisposable? pendingResource = null;
        lock (_gate)
        {
            if (_acquisitionClosed)
            {
                return;
            }

            _acquisitionClosed = true;
            if (!_isValueCreated)
            {
                _factory = null;
                pendingResource = _pendingResource;
                _pendingResource = null;
            }
        }

        pendingResource?.Dispose();
    }

    public void Dispose()
    {
        CloseAcquisition();
        HttpImageTransport? transport;
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _factory = null;
            _pendingResource = null;
            transport = _transport;
            _transport = null;
        }

        transport?.Dispose();
    }
}
