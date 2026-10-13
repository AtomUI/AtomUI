using AtomUI.Localization;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using Avalonia.Threading;
using System.Runtime.ExceptionServices;

namespace AtomUI;

internal sealed class ApplicationScope : IDisposable
{
    private Application? _application;
    private readonly IReadOnlyList<IAtomUIOwnedService> _ownedServices;
    private int _attachedOwnedServiceCount;
    private int _initialized;
    private int _disposed;

    internal ApplicationScope(
        Application application,
        ThemeManager themeManager,
        LocalizationHost localizationHost,
        IReadOnlyList<IAtomUIOwnedService> ownedServices)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
        ThemeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        LocalizationHost = localizationHost ?? throw new ArgumentNullException(nameof(localizationHost));
        _ownedServices = ownedServices ?? throw new ArgumentNullException(nameof(ownedServices));
        FlowDirectionStyle = new Style(selector => selector.Is<TopLevel>());
        FlowDirectionStyle.Setters.Add(new Setter(
            Visual.FlowDirectionProperty,
            new DynamicResourceExtension(LanguageResourceKeys.FlowDirection)));
    }

    internal ThemeManager ThemeManager { get; }

    internal LocalizationHost LocalizationHost { get; }

    internal LanguageManager LanguageManager => LocalizationHost.LanguageManager;

    internal Localizer Localizer => LocalizationHost.Localizer;

    internal LanguageResourceProvider ResourceProvider => LocalizationHost.ResourceProvider;

    internal Style FlowDirectionStyle { get; }

    // 启动状态和可等待结果由同一个对象拥有；等待宿主和准备任务只持有取消资源。
    internal AtomUIStartup Startup { get; } = new();

    private IReadOnlyList<Action>? _deferredRegistrations;
    private CancellationTokenSource? _startupCancellation;
    private StartupTopLevelReadiness? _startupReadiness;
    private StartupLifetimeReadiness? _startupLifetime;

    internal void ConfigureStartup(IReadOnlyList<Action> deferredRegistrations)
    {
        ArgumentNullException.ThrowIfNull(deferredRegistrations);
        ThrowIfDisposed();
        _deferredRegistrations = deferredRegistrations;
        _startupCancellation = new CancellationTokenSource();
        Startup.TryTransition(AtomUIStartupState.Configuring, AtomUIStartupState.WaitingForHost);
    }

    internal void CompleteStartup(Exception? failure = null)
    {
        FinishStartup(failure, CancellationToken.None, AfterStageTwoRegistrations);
    }

    internal void RunStageTwo() => _ = RunStageTwoAsync();

    private async Task RunStageTwoAsync()
    {
        if (Volatile.Read(ref _disposed) != 0 || Startup.State != AtomUIStartupState.WaitingForHost)
        {
            return;
        }
        // 进度处理器可能同步关闭应用；先捕获令牌和关闭动作，finally 始终拥有清理。
        var cancellation = _startupCancellation!.Token;
        var closeRegistrations = AfterStageTwoRegistrations;
        Exception? failure = null;
        if (!Startup.TryTransition(AtomUIStartupState.WaitingForHost, AtomUIStartupState.Initializing))
        {
            return;
        }
        try
        {
            cancellation.ThrowIfCancellationRequested();
            BeforeStageTwoRegistrations?.Invoke();
            var registrations = _deferredRegistrations ?? [];
            foreach (var registration in registrations)
            {
                registration();
                cancellation.ThrowIfCancellationRequested();
            }
            Startup.ReportProgress("RegistrationsCompleted");
            cancellation.ThrowIfCancellationRequested();
            if (StageTwoRegistrationCompleted is { } prepare)
            {
                await prepare(cancellation);
            }
            cancellation.ThrowIfCancellationRequested();
            Startup.ReportProgress("FrameworkEvolved");
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            failure = exception;
        }
        finally
        {
            FinishStartup(failure, cancellation, closeRegistrations);
        }
    }

    private void FinishStartup(Exception? failure, CancellationToken cancellation, Action? closeRegistrations)
    {
        CleanupStartupAction(closeRegistrations, ref failure);
        CleanupStartupAction(ThemeManager.CompleteStartupRegistration, ref failure);
        CleanupStartupAction(ReleaseStartupInputs, ref failure);
        CleanupStartupAction(_startupCancellation is { } source ? source.Dispose : null, ref failure);
        _startupCancellation = null;
        Startup.Complete(failure, cancellation);
    }

    private static void CleanupStartupAction(Action? cleanup, ref Exception? failure)
    {
        try
        {
            cleanup?.Invoke();
        }
        catch (Exception exception)
        {
            if (failure is null)
            {
                failure = exception;
            }
            else
            {
                AtomUIStartup.LogFailure(exception);
            }
        }
    }

    internal void ScheduleStageTwoAfterFirstFrame(TopLevel? startupTopLevel)
    {
        if (Volatile.Read(ref _disposed) != 0 || Startup.State != AtomUIStartupState.WaitingForHost)
        {
            return;
        }
        if (startupTopLevel is null)
        {
            Dispatcher.UIThread.Post(RunStageTwo, DispatcherPriority.Background);
            return;
        }

        _startupReadiness?.Dispose();
        var readiness = new StartupTopLevelReadiness(startupTopLevel, RunStageTwo, CancelStartup);
        _startupReadiness = readiness;
        readiness.Start();
    }

    internal void ScheduleStageTwoForApplication()
    {
        if (Volatile.Read(ref _disposed) != 0 || _application is null || Startup.State != AtomUIStartupState.WaitingForHost)
        {
            return;
        }
        _startupLifetime?.Dispose();
        var readiness = new StartupLifetimeReadiness(_application.ApplicationLifetime,
            ScheduleStageTwoAfterFirstFrame, CancelStartup);
        _startupLifetime = readiness;
        readiness.Start();
    }

    // Stage 2 框架侧演化挂接点（UseAtomUI 接线）：
    // Before = 打开注册窗口；Completed = 全部注册块成功后的演化；After = finally 关窗（无论成败）。
    internal Action? BeforeStageTwoRegistrations;
    internal Func<CancellationToken, Task>? StageTwoRegistrationCompleted;
    internal Action? AfterStageTwoRegistrations;

    private void ReleaseStartupInputs()
    {
        var readiness = _startupReadiness;
        var lifetime = _startupLifetime;
        _startupReadiness = null;
        _startupLifetime = null;
        _deferredRegistrations = null;
        BeforeStageTwoRegistrations = null;
        StageTwoRegistrationCompleted = null;
        AfterStageTwoRegistrations = null;
        Exception? failure = null;
        CleanupStartupAction(readiness is not null ? readiness.Dispose : null, ref failure);
        CleanupStartupAction(lifetime is not null ? lifetime.Dispose : null, ref failure);
        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

    private void CancelStartup()
    {
        if (Startup.IsTerminal)
        {
            return;
        }
        var cancellation = _startupCancellation?.Token ?? new CancellationToken(canceled: true);
        Exception? failure = null;
        CleanupStartupAction(_startupCancellation is { } source ? source.Cancel : null, ref failure);
        if (Startup.State != AtomUIStartupState.Initializing)
        {
            FinishStartup(failure, cancellation, AfterStageTwoRegistrations);
        }
        else
        {
            // 运行任务必须先退出 prepare 和 finally，才能公布终态；关闭能力窗口防止晚期写入。
            CleanupStartupAction(AfterStageTwoRegistrations, ref failure);
            CleanupStartupAction(ReleaseStartupInputs, ref failure);
            if (failure is not null)
            {
                AtomUIStartup.LogFailure(failure);
            }
        }
    }

    internal void InitializeApplication()
    {
        ThrowIfDisposed();
        if (Volatile.Read(ref _initialized) != 0)
        {
            throw new InvalidOperationException("AtomUI application scope is already initialized.");
        }

        var application = _application!;
        if (ApplicationScopeRegistry.Get(application) is not null)
        {
            throw new InvalidOperationException(
                $"AtomUI is already initialized for Application '{application.GetType().FullName}'.");
        }

        application.Resources.MergedDictionaries.Add(ResourceProvider);
        application.Styles.Add(FlowDirectionStyle);
        try
        {
            ThemeManager.InitializeApplication(application);
            ApplicationScopeRegistry.Register(application, this);
            for (var index = 0; index < _ownedServices.Count; index++)
            {
                _ownedServices[index].Attach(application);
                _attachedOwnedServiceCount = index + 1;
            }
            Volatile.Write(ref _initialized, 1);
        }
        catch (Exception initializationException)
        {
            var failures = new List<Exception> { initializationException };
            DetachOwnedServices(application, failures);
            TryCleanup(() => ApplicationScopeRegistry.Unregister(application, this), failures);
            TryCleanup(() => application.Styles.Remove(FlowDirectionStyle), failures);
            TryCleanup(() => application.Resources.MergedDictionaries.Remove(ResourceProvider), failures);
            ThrowFailures("AtomUI application initialization and rollback failed.", failures);
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        var failures = new List<Exception>();
        TryCleanup(CancelStartup, failures);
        var application = Interlocked.Exchange(ref _application, null);
        if (application is not null)
        {
            TryCleanup(() => ApplicationScopeRegistry.Unregister(application, this), failures);
            DetachOwnedServices(application, failures);
            TryCleanup(() => application.Styles.Remove(FlowDirectionStyle), failures);
            TryCleanup(() => application.Resources.MergedDictionaries.Remove(ResourceProvider), failures);
        }

        for (var index = _ownedServices.Count - 1; index >= 0; index--)
        {
            var service = _ownedServices[index];
            TryCleanup(service.Dispose, failures);
        }

        TryCleanup(ThemeManager.Dispose, failures);
        TryCleanup(LocalizationHost.Dispose, failures);
        ThrowFailures("AtomUI application disposal failed.", failures);
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            throw new ObjectDisposedException(nameof(ApplicationScope));
        }
    }

    private void DetachOwnedServices(Application application, ICollection<Exception> failures)
    {
        var count = Interlocked.Exchange(ref _attachedOwnedServiceCount, 0);
        for (var index = count - 1; index >= 0; index--)
        {
            var service = _ownedServices[index];
            TryCleanup(() => service.Detach(application), failures);
        }
    }

    private static void TryCleanup(Action cleanup, ICollection<Exception> failures)
    {
        try
        {
            cleanup();
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }
    }

    private static void ThrowFailures(string message, IReadOnlyList<Exception> failures)
    {
        if (failures.Count == 0)
        {
            return;
        }
        if (failures.Count == 1)
        {
            ExceptionDispatchInfo.Capture(failures[0]).Throw();
        }
        throw new AggregateException(message, failures);
    }
}
