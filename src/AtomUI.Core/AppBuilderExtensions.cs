using AtomUI.Utils;
using Avalonia;
using Avalonia.Media;

namespace AtomUI;

/// <summary>
/// AtomUI's extensions to <see cref="AppBuilder" />.
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Enables AtomUI for the application that <paramref name="appBuilder" /> builds.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This method was already called on <paramref name="appBuilder" />.
    /// </exception>
    /// <remarks>
    /// This call only registers the configuration. The <paramref name="configure" /> callback runs on the UI thread
    /// once the platform services are ready; the designer, Browser and WASI skip that early run and configure the
    /// builder when the kernel attaches instead. After the early run, the global tokens are prepared on a
    /// background thread in parallel with the creation of the <see cref="Application" />, when the configured
    /// theme sources allow it. When the callback runs, no <see cref="Application" /> exists yet and the AtomUI
    /// kernel is not committed. The kernel is committed by <see cref="ApplicationStartup.Attach" />, which the
    /// source generator calls from the generated <c>RegisterServices</c> override of a partial application.
    /// Startup throws an <see cref="InvalidOperationException" /> when the application never attaches.
    /// </remarks>
    public static AppBuilder UseAtomUI(this AppBuilder appBuilder, Action<IAtomUIBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(appBuilder);
        ArgumentNullException.ThrowIfNull(configure);
        var configuration = StartupConfiguration.Register(appBuilder, configure);
        return appBuilder.With(configuration)
                         .AfterPlatformServicesSetup(configuration.ConfigureAndPrepareAfterPlatformSetup)
                         .AfterApplicationSetup(StartupConfiguration.VerifyAttached);
    }

    /// <summary>
    /// Applies the platform defaults that AtomUI recommends. Its main purpose is to work around platform issues
    /// that Avalonia has not fixed upstream yet and that visibly degrade the AtomUI experience, so that an
    /// application gets the best result without configuring the options itself.
    /// </summary>
    /// <remarks>
    /// It currently sets:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     <b>Windows</b>: <see cref="Win32PlatformOptions.RenderingMode" /> falls back from
    ///     <see cref="Win32RenderingMode.AngleEgl" /> to <see cref="Win32RenderingMode.Software" />. On Windows
    ///     10 and Windows 11 the composition fallback order is RedirectionSurface, WinUIComposition,
    ///     DirectComposition and LowLatencyDxgiSwapChain; CSD windows keep their theme background configuration.
    ///     With RedirectionSurface, Avalonia's render loop is timed by a thread sleep and does not follow the
    ///     monitor refresh rate. When that timer is the one in use, AtomUI replaces it with a timer woken by the
    ///     DWM composition cadence (DwmFlush), which locks animation ticks to the refresh rate. When the timer
    ///     cannot be replaced, it falls back to raising the process timer resolution to 1 ms (timeBeginPeriod).
    ///     Neither is done when the caller overrides the options with another first composition mode.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     <b>macOS</b>: <see cref="AvaloniaNativePlatformOptions.RenderingMode" /> puts
    ///     <see cref="AvaloniaNativeRenderingMode.Metal" /> first. Metal is presented directly by the render
    ///     thread, so compositor animations (such as the startup Splash progress) still reach the screen while
    ///     the UI thread is busy with the startup commit; OpenGL and Software are presented through an
    ///     IOSurface and need the main thread to set the layer content. Avalonia 12.1.0 fixed the earlier
    ///     Metal jitter of the window content during live resize by presenting scaled frames synchronously.
    ///     On macOS 14 and later, a single-screen Metal session gets a render clock that follows the display;
    ///     the Avalonia clock is restored when the screen configuration changes or the application exits.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     <b>Linux</b>: sets <see cref="X11PlatformOptions.EnableDrawnDecorations" /> to <c>true</c>, which
    ///     the window decorations that AtomUI draws itself (CSD) require.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     <b>All platforms</b>: registers Microsoft YaHei as a font fallback, which the font manager tries
    ///     before anything else when it matches a codepoint.
    ///     </description>
    ///   </item>
    /// </list>
    /// Callers can keep chaining <c>.With(...)</c> to override or extend these options.
    /// </remarks>
    /// <example>
    /// <code>
    /// BuildAvaloniaApp()
    ///     .UseAtomUIPlatformDetect()
    ///     .WithAtomUIDefaultOptions()
    ///     .StartWithClassicDesktopLifetime(args);
    /// </code>
    /// </example>
    public static AppBuilder WithAtomUIDefaultOptions(this AppBuilder appBuilder)
    {
        // Platform options are set per target platform. In a RID-specific NativeAOT publish, OperatingSystem.IsXxx()
        // is folded to a constant, so the branches of the other platforms and the platform option types they reference
        // are trimmed away as a whole.
        if (OperatingSystem.IsMacOS())
        {
            appBuilder = appBuilder.With(new AvaloniaNativePlatformOptions
            {
                RenderingMode =
                [
                    AvaloniaNativeRenderingMode.Metal,
                    AvaloniaNativeRenderingMode.OpenGl,
                    AvaloniaNativeRenderingMode.Software
                ]
            });
            if (OperatingSystem.IsMacOSVersionAtLeast(14))
            {
                appBuilder = appBuilder.AfterPlatformServicesSetup(MacOSRenderPacing.Apply);
            }
        }
        if (OperatingSystem.IsWindows())
        {
            appBuilder = appBuilder.With(new Win32PlatformOptions
            {
                RenderingMode =
                [
                    Win32RenderingMode.AngleEgl,
                    Win32RenderingMode.Software
                ],
                // RedirectionSurface first: only the redirection surface is composited by DWM in sync with window
                // geometry changes, so the opposite edge shows no stale frame when the left or top edge is dragged in
                // a live resize (WinUI, DComp and the flip swapchain all lag by at least one frame).
                CompositionMode =
                [
                    Win32CompositionMode.RedirectionSurface,
                    Win32CompositionMode.WinUIComposition,
                    Win32CompositionMode.DirectComposition,
                    Win32CompositionMode.LowLatencyDxgiSwapChain
                ]
            });
            // With RedirectionSurface, Avalonia's render loop is timed by a thread sleep and does not follow the
            // monitor refresh rate; WindowsRenderPacing.Apply swaps in a timer that is woken by the DWM
            // composition cadence. It is deferred until the platform services are up and decides by the timer
            // that was actually created, so nothing is replaced when the caller later selects another
            // composition mode.
            appBuilder = appBuilder.AfterPlatformServicesSetup(WindowsRenderPacing.Apply);
        }
        if (OperatingSystem.IsLinux())
        {
            appBuilder = appBuilder.With(new X11PlatformOptions
            {
                EnableDrawnDecorations = true
            });
        }
        return appBuilder
            .With(new FontManagerOptions
            {
                FontFallbacks = [new FontFallback
                {
                    FontFamily = new FontFamily("Microsoft YaHei")
                }]
            });
    }
}
