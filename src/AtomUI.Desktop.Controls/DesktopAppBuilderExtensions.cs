using System.Runtime.CompilerServices;
using AtomUI.Animations;
using AtomUI.Native;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Threading;

namespace AtomUI;

/// <summary>
/// Selects the Linux windowing backend used by AtomUI desktop applications.
/// </summary>
public enum AtomUIWindowingPlatform
{
    /// <summary>
    /// Uses native Wayland when <c>WAYLAND_DISPLAY</c> is available; otherwise uses X11.
    /// </summary>
    Auto,

    /// <summary>
    /// Uses Avalonia's native Wayland backend.
    /// </summary>
    Wayland,

    /// <summary>
    /// Uses Avalonia's X11 backend. In a Wayland session this runs through XWayland.
    /// </summary>
    X11
}

/// <summary>
/// AtomUI desktop platform extensions for <see cref="AppBuilder"/>.
/// </summary>
public static class DesktopAppBuilderExtensions
{
    /// <summary>
    /// Environment variable that overrides automatic Linux windowing backend selection.
    /// Accepted values are <c>wayland</c> and <c>x11</c>.
    /// </summary>
    public const string WindowingPlatformEnvironmentVariable = "ATOMUI_WINDOWING_PLATFORM";

    /// <summary>
    /// Configures Avalonia's desktop backend, preferring native Wayland when a Wayland display is available.
    /// </summary>
    /// <remarks>
    /// An explicit <paramref name="platform"/> takes precedence over
    /// <c>ATOMUI_WINDOWING_PLATFORM</c>. Automatic detection uses <c>WAYLAND_DISPLAY</c> and falls back to
    /// X11/XWayland. Headless and framebuffer applications should configure their backend directly.
    /// </remarks>
    public static AppBuilder UseAtomUIPlatformDetect(
        this AppBuilder builder,
        AtomUIWindowingPlatform platform = AtomUIWindowingPlatform.Auto)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (!OperatingSystem.IsLinux())
        {
            if (platform != AtomUIWindowingPlatform.Auto)
            {
                throw new PlatformNotSupportedException(
                    $"The {platform} windowing backend can only be selected on Linux.");
            }

            return builder.UsePlatformDetect();
        }

        var selectedPlatform = ResolveLinuxWindowingPlatform(
            platform,
            Environment.GetEnvironmentVariable(WindowingPlatformEnvironmentVariable),
            Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));

        builder.With(new AtomUIWindowingPlatformOptions(selectedPlatform));
        return selectedPlatform switch
        {
            AtomUIWindowingPlatform.Wayland => UseWaylandBackend(builder),
            AtomUIWindowingPlatform.X11 => UseX11Backend(builder),
            _ => throw new InvalidOperationException($"Unsupported Linux windowing backend: {selectedPlatform}.")
        };
    }

    internal static AtomUIWindowingPlatform ResolveLinuxWindowingPlatform(
        AtomUIWindowingPlatform platform,
        string? environmentOverride,
        string? waylandDisplay)
    {
        if (platform is not (AtomUIWindowingPlatform.Auto or
            AtomUIWindowingPlatform.Wayland or
            AtomUIWindowingPlatform.X11))
        {
            throw new ArgumentOutOfRangeException(nameof(platform), platform, "Unknown windowing backend.");
        }

        if (platform != AtomUIWindowingPlatform.Auto)
        {
            return platform;
        }

        if (!string.IsNullOrWhiteSpace(environmentOverride))
        {
            return environmentOverride.Trim().ToLowerInvariant() switch
            {
                "wayland" => AtomUIWindowingPlatform.Wayland,
                "x11" => AtomUIWindowingPlatform.X11,
                _ => throw new InvalidOperationException(
                    $"{WindowingPlatformEnvironmentVariable} must be 'wayland' or 'x11', " +
                    $"but was '{environmentOverride}'.")
            };
        }

        return string.IsNullOrWhiteSpace(waylandDisplay)
            ? AtomUIWindowingPlatform.X11
            : AtomUIWindowingPlatform.Wayland;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static AppBuilder UseWaylandBackend(AppBuilder builder)
    {
#pragma warning disable AVALONIA_WAYLAND_FORCE_CSD
        builder.With(new WaylandPlatformOptions
        {
            ForceDrawnDecorations = true
        });
#pragma warning restore AVALONIA_WAYLAND_FORCE_CSD

        builder.UseWayland()
               .UseSkia()
               .UseHarfBuzz();

        var initializeWayland = builder.WindowingSubsystemInitializer!;
        return builder.UseWindowingSubsystem(() =>
        {
            // Enable on compatible Wayland backends unless explicitly disabled for diagnosis.
            if (!AppContext.TryGetSwitch("AtomUI.Experimental.WaylandFramePacing", out var enableFramePacing) || enableFramePacing)
            {
                if (WaylandFramePacing.TryInstall(ReportWaylandFrameFailure, BindMotionPresentationClock, out var reason))
                {
                    Logger.TryGet(LogEventLevel.Information, AtomUILogArea.System)?.Log(
                        typeof(DesktopAppBuilderExtensions), "Wayland frame pacing enabled: {Reason}", reason);
                }
                else
                {
                    Logger.TryGet(LogEventLevel.Warning, AtomUILogArea.System)?.Log(
                        typeof(DesktopAppBuilderExtensions), "Wayland frame pacing was not enabled: {Reason}", reason);
                }
            }

            initializeWayland();

            // Wayland creates Dispatcher before its worker finishes, then installs a new backend.
            // Share the original clock before platform-ready callbacks or application startup run.
            if (!Dispatcher.UIThread.TryShareManagedClock())
            {
                Logger.TryGet(LogEventLevel.Warning, AtomUILogArea.System)?.Log(
                    typeof(DesktopAppBuilderExtensions),
                    "Wayland shared clock compatibility fix was not applied: " +
                    "the dispatcher backend or clock structure is not supported.");
            }
        }, builder.WindowingSubsystemName ?? "Wayland");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static AppBuilder UseX11Backend(AppBuilder builder)
    {
        return builder.UseX11()
                      .UseSkia()
                      .UseHarfBuzz();
    }

    private static void ReportWaylandFrameFailure(Exception error)
    {
        Logger.TryGet(LogEventLevel.Error, AtomUILogArea.System)?.Log(
            typeof(DesktopAppBuilderExtensions), "Wayland prepared-frame submission failed; retiring its target: {Error}", error);
    }

    // Native cannot reference Core; expose its presentation timing to Motion through Core's contract.
    private static void BindMotionPresentationClock(WaylandPresentationTiming timing)
    {
        MotionFrameTiming.Clock = new WaylandMotionPresentationClock(timing);
    }

    private sealed class WaylandMotionPresentationClock(WaylandPresentationTiming timing) : IMotionPresentationClock
    {
        public long? PredictPresentation(long now) => timing.PredictPresentation(now);
        public long RefreshInterval => timing.RefreshInterval;
        public long FramesStarted => timing.FramesStarted;
        public int FramesInFlight => timing.FramesInFlight;
    }
}

internal sealed record AtomUIWindowingPlatformOptions(AtomUIWindowingPlatform Platform);
