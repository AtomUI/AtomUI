using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering;

namespace AtomUI.Utils;

/// <summary>
/// Makes the Windows render loop produce frames in step with the display refresh.
/// </summary>
/// <remarks>
/// In RedirectionSurface composition mode, and in the fallback used when no compositor is available, Avalonia
/// drives the render loop with <c>SleepLoopRenderTimer</c>: the render thread sleeps until the next frame in a
/// kernel wait with a timeout, which is not aligned to the vertical blank. At the default Windows timer
/// resolution of about 15.6 ms the wait is also stretched to the next system clock tick, so the render thread
/// sleeps one frame too long at irregular intervals, and every composited animation stutters with it. The other
/// composition modes are driven by the system compositor at the vertical blank and need no handling.
///     <para>
///     The fix has two levels. The sleep timer is preferably replaced with <see cref="DwmVsyncRenderTimer" />.
///     When a change in the internal layout of Avalonia makes that replacement impossible, the process timer
///     resolution is raised to 1 ms instead, and stays raised until the process exits. That only removes the
///     extra frame of sleep; the cadence is still not locked to the refresh rate.
///     </para>
/// </remarks>
[SupportedOSPlatform("windows")]
internal static class WindowsRenderPacing
{
    private const uint TimerResolutionMilliseconds = 1;
    private static int s_timerResolutionRaised;

    /// <summary>
    /// Call it once the platform services are ready. By then the render loop and its timer have been created
    /// from the final <see cref="Win32PlatformOptions" />.
    /// </summary>
    internal static void Apply(AppBuilder appBuilder)
    {
        if (Design.IsDesignMode)
        {
            return;
        }
        bool sleeps;
        if (AvaloniaLocator.Current.GetService<IRenderLoop>() is { } loop &&
            RenderLoopReflection.TryGetTimer(loop, out var timer))
        {
            if (timer is not SleepLoopRenderTimer)
            {
                return;
            }
            if (RenderLoopReflection.TryReplaceTimer(loop, new DwmVsyncRenderTimer()))
            {
                return;
            }
            sleeps = true;
        }
        else
        {
            // When the timer cannot be read, infer it from the options: RedirectionSurface as the first
            // preferred mode means the sleep timer.
            sleeps = AvaloniaLocator.Current.GetService<Win32PlatformOptions>()?.CompositionMode is { Count: > 0 } modes &&
                     modes[0] == Win32CompositionMode.RedirectionSurface;
        }
        if (!sleeps)
        {
            return;
        }
        StartupProcess.LogWarning(
            "AtomUI could not replace Avalonia's sleep-based render timer with the DWM vsync timer; " +
            "raising the process timer resolution to 1 ms instead. Animations will not be locked to the display refresh.");
        if (Interlocked.Exchange(ref s_timerResolutionRaised, 1) == 0)
        {
            // When the call fails (TIMERR_NOCANDO), the system default resolution stays in effect and rendering
            // behaves as it would without the raise, so nothing more is needed.
            _ = timeBeginPeriod(TimerResolutionMilliseconds);
        }
    }

    [DllImport("winmm.dll")]
    private static extern uint timeBeginPeriod(uint uPeriod);
}
