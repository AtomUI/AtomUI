using NWayland.Interop;

namespace AtomUI.Native;

internal sealed class WaylandFrameCallbackRelay(
    IWlEventsListener original,
    Action completed) : IWlEventsListener
{
    private bool _completed;

    public void DispatchEvent(WlEventArgs args)
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        // The original listener must clear the old callback before another buffer creates one.
        original.DispatchEvent(args);
        completed();
    }
}
