using AtomUI.Animations;
using Avalonia;

namespace AtomUI.Desktop.Controls;

internal sealed class DialogZoomInMotion : ZoomInMotion
{
    internal DialogZoomInMotion(RelativePoint origin, TimeSpan duration) : base(duration)
    {
        Origin = origin;
    }
}

internal sealed class DialogZoomOutMotion : ZoomOutMotion
{
    internal DialogZoomOutMotion(RelativePoint origin, TimeSpan duration) : base(duration)
    {
        Origin = origin;
    }
}
