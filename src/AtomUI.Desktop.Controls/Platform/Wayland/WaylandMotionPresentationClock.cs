using AtomUI.Animations;
using AtomUI.Native;

namespace AtomUI;

// Native cannot reference Core; Desktop exposes the Wayland presentation timing through Core's contract.
internal sealed class WaylandMotionPresentationClock(WaylandPresentationTiming timing) : IMotionPresentationClock
{
    public long? PredictPresentation(long now) => timing.PredictPresentation(now);
    public long RefreshInterval => timing.RefreshInterval;
    public long FramesStarted => timing.FramesStarted;
    public int FramesInFlight => timing.FramesInFlight;
}
