using System.Runtime.ExceptionServices;
using AtomUI.Localization;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using Avalonia;

namespace AtomUI;

internal static class StartupCommit
{
    /// <summary>
    /// 单次提交：本地化、主题、自有服务、初始化器。<paramref name="initialSystemAppearance"/> 与选取
    /// <paramref name="startupGlobalParent"/> 用的是同一次系统外观读取，避免两次读取之间外观切换导致不一致。
    /// </summary>
    internal static ApplicationScope Commit(Application application, AtomUIBuilder builder,
        StartupProcess process, ThemeAppearance? initialSystemAppearance, ThemeSnapshot? startupGlobalParent)
    {
        if (ApplicationScopeRegistry.Get(application) is not null)
        {
            throw new InvalidOperationException(
                $"AtomUI is already initialized for Application '{application.GetType().FullName}'.");
        }
        StartupEventSource.Log.PhaseStart(StartupPhase.Commit);
        var image = builder.ThemeManagerBuilder.StartupImage ?? (application as Registration.IGeneratedStartupImageHost)?.GetStartupImage();
        if (image is not null)
        {
            builder.ThemeManagerBuilder.UseStartupImage(image);
        }
        LocalizationHost? localizationHost = null;
        ThemeManager? themeManager = null;
        ApplicationScope? applicationScope = null;
        List<IAtomUIOwnedService>? ownedServices = null;
        try
        {
            // 应用级语言注册需要 Application 实例，在此执行；目录与 bundle 由 registry 排序，与注册顺序无关。
            if (application is IGeneratedApplicationLanguageBootstrap bootstrap)
            {
                bootstrap.RegisterApplicationLanguages(builder.Localization);
            }
            if (image is not null)
            {
                builder.LocalizationBuilder.ApplyBuildProof(image.VerifiedLanguages, image.LanguageModules);
            }
            StartupEventSource.Log.PhaseStart(StartupPhase.CommitLocalization);
            localizationHost = builder.LocalizationBuilder.Build();
            StartupEventSource.Log.PhaseStop(StartupPhase.CommitLocalization);
            StartupEventSource.Log.PhaseStart(StartupPhase.CommitTheme);
            themeManager = builder.ThemeManagerBuilder.Build();
            StartupEventSource.Log.PhaseStart(StartupPhase.CommitOwnedServices);
            ownedServices = BuildOwnedServices(application, builder.OwnedServiceRegistrations);
            StartupEventSource.Log.PhaseStop(StartupPhase.CommitOwnedServices);
            foreach (var package in builder.PackageDeclarations)
            {
                package.PrepareContext(application, themeManager);
            }
            applicationScope = new ApplicationScope(application, themeManager, localizationHost, ownedServices, process,
                builder.OwnedServiceRegistrations.Select(registration => registration.Id));
            ownedServices = null;
            AvaloniaLocator.CurrentMutable.BindToSelf(themeManager);
            applicationScope.InitializeApplication(initialSystemAppearance, startupGlobalParent);
            StartupEventSource.Log.PhaseStop(StartupPhase.CommitTheme);
            StartupEventSource.Log.PhaseStart(StartupPhase.CommitInitializers);
            foreach (var initializer in builder.ThemeManagerBuilder.Initializers)
            {
                initializer(themeManager);
            }
            foreach (var package in builder.PackageDeclarations)
            {
                package.Commit(new Registration.ControlPackageContext(application, themeManager));
            }
            StartupEventSource.Log.PhaseStop(StartupPhase.CommitInitializers);
            applicationScope.ReleaseStartupInputs();
            return applicationScope;
        }
        catch (Exception initializationException)
        {
            builder.ThemeManagerBuilder.DiscardPendingRegistrations();
            var cleanupFailures = new List<Exception>();
            void TryCleanup(Action cleanup)
            {
                try
                {
                    cleanup();
                }
                catch (Exception cleanupException)
                {
                    cleanupFailures.Add(cleanupException);
                }
            }
            if (applicationScope is not null)
            {
                TryCleanup(applicationScope.Dispose);
            }
            else
            {
                TryCleanup(() => DisposeOwnedServices(ownedServices));
                if (themeManager is not null)
                {
                    TryCleanup(themeManager.Dispose);
                }
                if (localizationHost is not null)
                {
                    TryCleanup(localizationHost.Dispose);
                }
            }
            if (cleanupFailures.Count != 0)
            {
                throw new AggregateException(
                    "AtomUI application initialization and cleanup failed.",
                    new[] { initializationException }.Concat(cleanupFailures));
            }
            ExceptionDispatchInfo.Capture(initializationException).Throw();
            throw;
        }
        finally
        {
            StartupEventSource.Log.PhaseStop(StartupPhase.Commit);
        }
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
