using AtomUI.Localization;
using AtomUI.Theme;
using Avalonia;
using System.Runtime.ExceptionServices;

namespace AtomUI;

public static class ApplicationExtensions
{
    public static Application UseAtomUI(
        this Application application,
        Action<IAtomUIBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(application);
        if (ApplicationScopeRegistry.Get(application) is not null)
        {
            throw new InvalidOperationException(
                $"AtomUI is already initialized for Application '{application.GetType().FullName}'.");
        }

        var builder = new AtomUIBuilder(application);
        if (application is IGeneratedApplicationLanguageBootstrap bootstrap)
        {
            bootstrap.RegisterApplicationLanguages(builder.Localization);
        }
        configure?.Invoke(builder);

        LocalizationHost? localizationHost = null;
        ApplicationScope? applicationScope = null;
        List<IAtomUIOwnedService>? ownedServices = null;
        try
        {
            // staged 判定须先于本地化构建：Stage 1 是中间态，应用级语言包可能先于其目标包的
            // catalog 到达（catalog 随包注册被后置），此时允许未解析引用，由 Stage 2 演化闭合。
            var staged = builder.DeferredRegistrations.Count > 0;
            localizationHost = builder.LocalizationBuilder.Build(allowUnresolvedBundles: staged);
            var themeManager = builder.ThemeManagerBuilder.Build(staged);
            ownedServices = BuildOwnedServices(application, builder.OwnedServiceRegistrations);
            applicationScope = new ApplicationScope(
                application,
                themeManager,
                localizationHost,
                ownedServices);
            ownedServices = null;
            AvaloniaLocator.CurrentMutable.BindToSelf(themeManager);
            applicationScope.InitializeApplication();
            foreach (var initializer in builder.ThemeManagerBuilder.Initializers)
            {
                initializer(themeManager);
            }

            // 分级启动接线：Deferred 注册块等待宿主就绪后执行。
            // Stage 2 等待宿主指定 surface 的绘制批次与激活；该证据不代表显示器 presentation。
            if (staged)
            {
                // 演化接线：窗口 → 注册块（Deferred）→（成功）合并 + registry v2 + 重应用 → 关窗。
                var themeBuilder = builder.ThemeManagerBuilder;
                var localizationBuilder = builder.LocalizationBuilder;
                applicationScope.BeforeStageTwoRegistrations = () =>
                {
                    themeBuilder.OpenStageTwoWindow();
                    localizationBuilder.OpenStageTwoWindow();
                };
                applicationScope.StageTwoRegistrationCompleted = async cancellation =>
                {
                    // 成功封存与 finally 关闭分开：被捕获的包注册异常仍会在 Freeze 拒绝。
                    themeBuilder.RunDeferredCollects();
                    themeBuilder.ControlRegistrationState.Freeze();
                    localizationBuilder.CloseStageTwoWindow();
                    // 先准备本地化终态；任何 catalog/bundle/语言配置错误都必须在主题注册并入和
                    // 主题发布前失败，确保失败仍保持 Stage 1 可观察状态。
                    var languageInput = localizationBuilder.CaptureStageTwoEvolution(
                        localizationHost.Registry, localizationHost.Snapshots);
                    // 先让输入/渲染处理已排队工作；声明已冻结，后台不访问 Builder 或资源宿主。
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(static () => { },
                        Avalonia.Threading.DispatcherPriority.Background);
                    var (registry, snapshots, supportedLanguages) = await Task.Run(languageInput.Build, cancellation);
                    cancellation.ThrowIfCancellationRequested();
                    themeBuilder.MergeStageTwoInto(themeManager);
                    // Deferred 块内新增的 Theme Definition Resolver 随演化一起提交，目录以当前集合为来源。
                    await themeManager.ExecuteRegistrationEvolutionAsync(
                        themeBuilder.CreateThemeDefinitionRegistrations(),
                        themeEvolution =>
                        {
                            cancellation.ThrowIfCancellationRequested();
                            var languageEvolution = localizationHost.LanguageManager.PrepareEvolution(
                                registry, snapshots, supportedLanguages);
                            new StageTwoEvolutionPlan(themeManager, themeEvolution,
                                localizationHost.LanguageManager, languageEvolution).CommitAndPublish();
                        }, cancellation);
                };
                applicationScope.AfterStageTwoRegistrations = () =>
                {
                    themeBuilder.CloseStageTwoWindow();
                    localizationBuilder.CloseStageTwoWindow();
                };
                applicationScope.ConfigureStartup(builder.DeferredRegistrations);
                // 宿主解析同时处理延迟 MainWindow、待挂载 MainView 与显式无窗策略。
                Avalonia.Threading.Dispatcher.UIThread.Post(
                    applicationScope.ScheduleStageTwoForApplication,
                    Avalonia.Threading.DispatcherPriority.Background);
            }
            else
            {
                applicationScope.CompleteStartup();
            }

            return application;
        }
        catch (Exception initializationException)
        {
            try
            {
                if (applicationScope is not null)
                {
                    applicationScope.CompleteStartup(initializationException);
                    applicationScope.Dispose();
                }
                else
                {
                    DisposeOwnedServices(ownedServices);
                    localizationHost?.Dispose();
                }
            }
            catch (Exception cleanupException)
            {
                throw new AggregateException(
                    "AtomUI application initialization and cleanup failed.",
                    initializationException,
                    cleanupException);
            }
            ExceptionDispatchInfo.Capture(initializationException).Throw();
            throw;
        }
    }

    public static ILanguageManager? GetLanguageManager(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.LanguageManager;
    }

    public static ILocalizer? GetLocalizer(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.Localizer;
    }

    public static IThemeManager? GetThemeManager(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.ThemeManager;
    }

    /// <summary>获取应用唯一的启动状态与框架就绪结果。须在 UseAtomUI 注册后调用。</summary>
    public static IAtomUIStartup GetStartup(this Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        return ApplicationScopeRegistry.Get(application)?.Startup ??
            throw new InvalidOperationException("AtomUI startup is unavailable. Call UseAtomUI before GetStartup.");
    }

    private static List<IAtomUIOwnedService> BuildOwnedServices(
        Application application,
        IReadOnlyList<AtomUIOwnedServiceRegistration> registrations)
    {
        var services = new List<IAtomUIOwnedService>(registrations.Count);
        try
        {
            foreach (var registration in registrations)
            {
                services.Add(registration.Factory(application));
            }
            return services;
        }
        catch (Exception factoryException)
        {
            try
            {
                DisposeOwnedServices(services);
            }
            catch (Exception cleanupException)
            {
                throw new AggregateException(
                    "AtomUI owned-service creation and cleanup failed.",
                    factoryException,
                    cleanupException);
            }
            ExceptionDispatchInfo.Capture(factoryException).Throw();
            throw;
        }
    }

    private static void DisposeOwnedServices(IReadOnlyList<IAtomUIOwnedService>? services)
    {
        if (services is null)
        {
            return;
        }

        List<Exception>? failures = null;
        for (var index = services.Count - 1; index >= 0; index--)
        {
            try
            {
                services[index].Dispose();
            }
            catch (Exception exception)
            {
                failures ??= [];
                failures.Add(exception);
            }
        }
        if (failures is { Count: 1 })
        {
            ExceptionDispatchInfo.Capture(failures[0]).Throw();
        }
        if (failures is { Count: > 1 })
        {
            throw new AggregateException("AtomUI owned-service disposal failed.", failures);
        }
    }
}
