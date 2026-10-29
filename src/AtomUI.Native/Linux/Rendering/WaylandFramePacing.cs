using Avalonia;
using Avalonia.Platform;

namespace AtomUI.Native;

/// <summary>
/// Installs the Wayland EGL prepared-frame adapter around the platform render interface.
/// </summary>
/// <remarks>
/// Call after the Wayland platform registers its render interface and before the windowing subsystem starts.
/// The caller decides whether to enable it and owns logging; Native only reports through the callbacks.
/// </remarks>
internal static class WaylandFramePacing
{
    internal static bool TryInstall(Action<Exception> reportFailure,
        Action<WaylandPresentationTiming> presentationTimingBound, out string reason)
    {
        if (!WaylandFramePacingCompatibility.TryCreate(reportFailure, presentationTimingBound, out var compatibility,
                out reason))
        {
            return false;
        }
        var renderer = AvaloniaLocator.Current.GetRequiredService<IPlatformRenderInterface>();
        AvaloniaLocator.CurrentMutable.Bind<IPlatformRenderInterface>()
                       .ToConstant(new WaylandFramePacingRenderInterface(renderer, compatibility));
        return true;
    }
}
