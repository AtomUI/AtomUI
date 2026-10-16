using System.Runtime.CompilerServices;
using AtomUI.Registration;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI;

internal sealed record EarlyStartup(AtomUIBuilder Builder, Task<PreparedThemeStartup>? Prepare);

/// <summary>
/// UseAtomUI 登记的配置。配置回调对每个 Application 实例执行一次：第一个实例在 UseAtomUI 中提前执行
/// 并捕获 Prepare 输入；平台服务就绪后启动后台 Prepare。同一 AppBuilder 上之后的实例
/// （headless 按测试隔离）在 Attach 中同步重跑。
/// </summary>
internal sealed class StartupConfiguration
{
    private static readonly ConditionalWeakTable<AppBuilder, StartupConfiguration> s_configured = new();
    private readonly Action<IAtomUIBuilder> _configure;
    private EarlyStartup? _early;
    private Func<PreparedThemeStartup>? _prepareFactory;

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
        configuration.BeginEarly();
        return configuration;
    }

    private void BeginEarly()
    {
        if (Design.IsDesignMode || OperatingSystem.IsBrowser() || OperatingSystem.IsWasi())
        {
            return;
        }
        StartupEventSource.Log.PhaseStart(StartupPhase.Configure);
        var builder = CreateBuilder(ApplicationType);
        StartupEventSource.Log.PhaseStop(StartupPhase.Configure);
        if (builder.ThemeManagerBuilder.CanPrepareInBackground)
        {
            var inputs = builder.ThemeManagerBuilder.CaptureStartupInputs();
            // Snapshot definitions only: the worker must not retain or read the mutable builder.
            // Trimmed JIT builds create the external TypeMap lazily; NativeAOT and untrimmed
            // builds have no such startup work to move off the commit thread.
            var definitions = ControlRegistrationRuntime.IsTrimmed && RuntimeFeature.IsDynamicCodeCompiled
                ? builder.PackageDeclarations.Select(package => package.Definition).ToArray()
                : Array.Empty<ControlPackageDefinition>();
            _prepareFactory = () =>
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
            };
        }
        _early = new EarlyStartup(builder, null);
    }

    internal void StartPrepareAfterPlatformSetup(AppBuilder appBuilder)
    {
        if (_early is not { } early || _prepareFactory is not { } prepareFactory)
        {
            return;
        }
        // Avalonia fixes the dispatcher's clock origin when it is first created. Creating
        // it before the native backend gives DispatcherTimer deadlines a different origin.
        // The platform has now installed the UI dispatcher, so worker misuse cannot claim
        // its thread identity. Preparation still overlaps Application construction.
        Dispatcher.UIThread.VerifyAccess();
        _prepareFactory = null;
        _early = early with { Prepare = Task.Run(prepareFactory) };
    }

    internal EarlyStartup? TakeEarly() => Interlocked.Exchange(ref _early, null);

    internal AtomUIBuilder CreateBuilder(Type? applicationType)
    {
        var builder = new AtomUIBuilder(applicationType);
        _configure(builder);
        return builder;
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
