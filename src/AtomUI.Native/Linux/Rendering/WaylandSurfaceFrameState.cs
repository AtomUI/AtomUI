namespace AtomUI.Native;

// A WSurface survives graphics-context replacement. Its outstanding callback must continue
// notifying the same pacer even when the framework creates another EGL surface wrapper.
internal sealed class WaylandSurfaceFrameState : IDisposable
{
    private readonly int _threadId = Environment.CurrentManagedThreadId;

    internal WaylandSurfaceBridge Bridge { get; }
    internal WaylandFramePacer Pacer { get; }
    internal bool IsDisposed { get; private set; }

    internal WaylandSurfaceFrameState(WaylandSurfaceBridge bridge)
    {
        Bridge = bridge;
        Pacer = new WaylandFramePacer(() => bridge.GetCallback() is not null, bridge.Wakeup, bridge.ReportFailure);
        bridge.Register(this);
        try
        {
            InstallCallbackRelay();
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    internal void InstallCallbackRelay()
    {
        if (Bridge.GetCallback() is not { } callback)
        {
            return;
        }

        var original = Bridge.GetListener(callback);
        if (original is not WaylandFrameCallbackRelay)
        {
            Bridge.SetListener(callback, new WaylandFrameCallbackRelay(original, Pacer.OnFrameReady));
        }
    }

    internal void VerifyAccess()
    {
        if (_threadId != Environment.CurrentManagedThreadId)
        {
            throw new InvalidOperationException("Wayland frame pacing must run on its owning worker thread.");
        }
    }

    public void Dispose()
    {
        VerifyAccess();
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        Pacer.Dispose();
        Bridge.Unregister(this);
    }
}
