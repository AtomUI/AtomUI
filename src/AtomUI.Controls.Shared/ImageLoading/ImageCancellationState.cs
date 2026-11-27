namespace AtomUI.Controls;

/// <summary>
/// The cancellation source of one image load, shared by the controller that owns the load and the operation that
/// runs it; <c>ImagePreviewEntry</c> in AtomUI.Controls owns one per thumbnail and full image load.
/// </summary>
/// <remarks>
/// <c>Cancel</c> requests cancellation once and ignores the non-fatal exceptions that cancellation callbacks throw,
/// because those callbacks run outside the image state machine. The source is disposed only after both
/// <c>ReleaseController</c> and <c>ReleaseOperation</c> have been called and no cancellation is running, so either
/// side may release first; a later <c>Cancel</c> then does nothing.
/// </remarks>
internal sealed class ImageCancellationState
{
    private readonly CancellationTokenSource _source = new();
    private readonly object _gate = new();
    private bool _controllerReleased;
    private bool _operationReleased;
    private bool _canceling;
    private bool _disposed;

    internal CancellationToken Token => _source.Token;

    internal bool IsCancellationRequested => _source.IsCancellationRequested;

    internal void Cancel()
    {
        lock (_gate)
        {
            if (_disposed || _canceling)
            {
                return;
            }
            _canceling = true;
        }

        try
        {
            _source.Cancel();
        }
        catch (Exception exception) when (ImageLoadEventDispatcher.IsNonFatal(exception))
        {
            // Cancellation callbacks are outside the image state machine.
        }
        finally
        {
            lock (_gate)
            {
                _canceling = false;
                DisposeCore();
            }
        }
    }

    internal void ReleaseController()
    {
        lock (_gate)
        {
            _controllerReleased = true;
            DisposeCore();
        }
    }

    internal void ReleaseOperation()
    {
        lock (_gate)
        {
            _operationReleased = true;
            DisposeCore();
        }
    }

    private void DisposeCore()
    {
        if (!_disposed && !_canceling && _controllerReleased && _operationReleased)
        {
            _disposed = true;
            _source.Dispose();
        }
    }
}
