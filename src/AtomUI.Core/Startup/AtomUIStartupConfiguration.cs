using System.Runtime.CompilerServices;
using AtomUI.Registration;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI;

internal sealed record AtomUIEarlyStartup(AtomUIBuilder Builder, Task<PreparedThemeStartup>? Prepare);

/// <summary>
/// UseAtomUI 登记的配置。配置回调对每个 Application 实例执行一次：第一个实例在 UseAtomUI 中提前执行
/// 并启动后台 Prepare；同一 AppBuilder 上之后的实例（headless 按测试隔离）在 Attach 中同步重跑。
/// </summary>
internal sealed class AtomUIStartupConfiguration
{
    private static readonly ConditionalWeakTable<AppBuilder, AtomUIStartupConfiguration> s_configured = new();
    private readonly Action<IAtomUIBuilder> _configure;
    private AtomUIEarlyStartup? _early;

    private AtomUIStartupConfiguration(Type? applicationType, Action<IAtomUIBuilder> configure)
    {
        ApplicationType = applicationType;
        _configure = configure;
    }

    internal Type? ApplicationType { get; }

    internal static AtomUIStartupConfiguration Register(AppBuilder appBuilder, Action<IAtomUIBuilder> configure)
    {
        var configuration = new AtomUIStartupConfiguration(appBuilder.ApplicationType, configure);
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
        // 先把 UI 线程固定为调用线程（主线程）：后台 Prepare 若误触 Dispatcher 会立即抛出，
        // 而不是抢走 UI 线程身份导致平台初始化失败。
        _ = Dispatcher.UIThread;
        AtomUIStartupEventSource.Log.PhaseStart("Configure");
        var builder = CreateBuilder(ApplicationType);
        AtomUIStartupEventSource.Log.PhaseStop("Configure");
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
                AtomUIStartupEventSource.Log.PhaseStart("Prepare");
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
                    AtomUIStartupEventSource.Log.PhaseStop("Prepare");
                }
            });
        }
        _early = new AtomUIEarlyStartup(builder, prepare);
    }

    internal AtomUIEarlyStartup? TakeEarly() => Interlocked.Exchange(ref _early, null);

    internal AtomUIBuilder CreateBuilder(Type? applicationType)
    {
        var builder = new AtomUIBuilder(applicationType);
        _configure(builder);
        return builder;
    }

    internal static void VerifyAttached(AppBuilder appBuilder)
    {
        if (appBuilder.Instance is { } application && !AtomUIStartup.IsAttached(application))
        {
            throw new InvalidOperationException(
                $"AtomUI was configured with UseAtomUI, but Application '{application.GetType().FullName}' never called " +
                "AtomUIStartup.Attach. Declare the Application class as partial (the AtomUI generator adds the " +
                "RegisterServices hook), or override RegisterServices and call AtomUIStartup.Attach(this) after " +
                "base.RegisterServices().");
        }
    }
}
