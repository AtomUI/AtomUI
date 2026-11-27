using System.Runtime.CompilerServices;
using AtomUI.Registration;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI;

internal sealed record EarlyStartup(AtomUIBuilder Builder, Task<PreparedThemeStartup>? Prepare);

/// <summary>
/// Created by <c>UseAtomUI</c>, which only registers the configuration and runs nothing at that point. The
/// callback runs once the platform services are ready; that is also when the Prepare inputs are captured and
/// the background task starts, so that it runs in parallel with the creation of the Application. Each
/// Application instance uses its own builder; an instance that was not configured in the platform callback is
/// configured synchronously in <c>Attach</c>.
/// </summary>
internal sealed class StartupConfiguration
{
    private static readonly ConditionalWeakTable<AppBuilder, StartupConfiguration> s_configured = new();
    private readonly Action<IAtomUIBuilder> _configure;
    private EarlyStartup? _early;

    private StartupConfiguration(Type? applicationType, Action<IAtomUIBuilder> configure)
    {
        ApplicationType = applicationType;
        _configure = configure;
    }

    internal Type? ApplicationType { get; }

    internal static StartupConfiguration Register(AppBuilder appBuilder, Action<IAtomUIBuilder> configure)
    {
        var configuration = new StartupConfiguration(appBuilder.ApplicationType, configure);
        if (!s_configured.TryAdd(appBuilder, configuration))
        {
            throw new InvalidOperationException("UseAtomUI can only be called once per AppBuilder.");
        }
        return configuration;
    }

    internal void ConfigureAndPrepareAfterPlatformSetup(AppBuilder appBuilder)
    {
        if (Design.IsDesignMode || OperatingSystem.IsBrowser() || OperatingSystem.IsWasi())
        {
            return;
        }
        // Application callbacks may indirectly construct AvaloniaObject and create a Dispatcher.
        // Run them only after the platform has established its dispatcher and clock origin.
        Dispatcher.UIThread.VerifyAccess();
        var builder = CreateBuilder(ApplicationType);
        Task<PreparedThemeStartup>? prepare = null;
        if (builder.ThemeManagerBuilder.CanPrepareInBackground)
        {
            var inputs = builder.ThemeManagerBuilder.CaptureStartupInputs();
            // Snapshot definitions only: the worker must not retain or read the mutable builder.
            // Trimmed JIT builds create the external TypeMap lazily; NativeAOT and untrimmed
            // builds have no such startup work to move off the commit thread.
            var definitions = ControlRegistrationRuntime.IsTrimmed && RuntimeFeature.IsDynamicCodeCompiled
                ? builder.PackageDeclarations.Select(package => package.Definition).ToArray()
                : Array.Empty<ControlPackageDefinition>();
            prepare = Task.Run(() =>
            {
                StartupEventSource.Log.PhaseStart(StartupPhase.Prepare);
                try
                {
                    foreach (var definition in definitions)
                    {
                        definition.WarmSelection();
                    }
                    return ThemeStartupPreparation.Prepare(inputs);
                }
                finally
                {
                    StartupEventSource.Log.PhaseStop(StartupPhase.Prepare);
                }
            });
        }
        _early = new EarlyStartup(builder, prepare);
    }

    internal EarlyStartup? TakeEarly() => Interlocked.Exchange(ref _early, null);

    internal AtomUIBuilder CreateBuilder(Type? applicationType)
    {
        StartupEventSource.Log.PhaseStart(StartupPhase.Configure);
        try
        {
            var builder = new AtomUIBuilder(applicationType);
            _configure(builder);
            return builder;
        }
        finally
        {
            StartupEventSource.Log.PhaseStop(StartupPhase.Configure);
        }
    }

    internal static void VerifyAttached(AppBuilder appBuilder)
    {
        if (appBuilder.Instance is { } application && !ApplicationStartup.IsAttached(application))
        {
            throw new InvalidOperationException(
                $"AtomUI was configured with UseAtomUI, but Application '{application.GetType().FullName}' never called " +
                "ApplicationStartup.Attach. Declare the Application class as partial (the AtomUI generator adds the " +
                "RegisterServices hook), or override RegisterServices and call ApplicationStartup.Attach(this) after " +
                "base.RegisterServices().");
        }
    }
}
