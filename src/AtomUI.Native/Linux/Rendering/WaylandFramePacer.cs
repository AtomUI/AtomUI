namespace AtomUI.Native;

internal interface IWaylandPreparedFrame
{
    object Owner { get; }
    bool IsValid { get; }
    void Submit();
    void Invalidate();
}

// Owned by a persistent surface, not by an individual EGL target. Worker-thread only.
internal sealed class WaylandFramePacer(
    Func<bool> hasPendingCallback,
    Action wakeup,
    Action<Exception> reportFailure) : IDisposable
{
    private IWaylandPreparedFrame? _prepared;
    private bool _disposed;

    internal bool IsFailed { get; private set; }
    internal bool HasPreparedFrame => _prepared is not null;
    internal bool CanPrepare => !_disposed && !IsFailed && _prepared is null;

    internal void CompleteFrame(IWaylandPreparedFrame frame)
    {
        if (!CanPrepare)
        {
            throw new InvalidOperationException("The Wayland prepared-frame slot is unavailable.");
        }

        if (hasPendingCallback())
        {
            _prepared = frame;
        }
        else
        {
            Submit(frame);
        }
    }

    internal void OnFrameReady()
    {
        if (_disposed || IsFailed || _prepared is not { } frame)
        {
            return;
        }

        _prepared = null;
        Submit(frame);
    }

    private void Submit(IWaylandPreparedFrame frame)
    {
        try
        {
            if (!frame.IsValid)
            {
                frame.Invalidate();
                wakeup();
                return;
            }

            frame.Submit();
        }
        catch (Exception error) when (error is not OutOfMemoryException)
        {
            // A deferred submission runs inside protocol dispatch, outside the compositor's
            // render exception boundary. Retire the target and report the failure explicitly.
            IsFailed = true;
            frame.Invalidate();
            reportFailure(error);
            wakeup();
        }
    }

    internal void Cancel(object owner)
    {
        if (_prepared is { } frame && ReferenceEquals(frame.Owner, owner))
        {
            _prepared = null;
            frame.Invalidate();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        var prepared = _prepared;
        _prepared = null;
        prepared?.Invalidate();
    }
}
