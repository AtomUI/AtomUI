using System.Runtime.ExceptionServices;
using AtomUI.Localization;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using Avalonia;

namespace AtomUI;

internal static class StartupCommit
{
    /// <summary>
    /// Commits localization, the theme, the owned services and the initializers in a single pass.
    /// <paramref name="initialSystemAppearance" /> and the startup global layer chosen with it come from the same
    /// reading of the system appearance, so an appearance switch between two readings cannot make them disagree.
    /// <paramref name="resolveStartupGlobalParent" /> is invoked only right before the theme is initialized: the
    /// commit steps before that do not depend on the global layer and can overlap the background Prepare, and the
    /// call itself may block until Prepare has finished. When a step fails, the parts built so far are disposed
    /// and the original exception is rethrown, inside an <see cref="AggregateException" /> when cleanup fails too.
    /// </summary>
    internal static ApplicationScope Commit(Application application, AtomUIBuilder builder,
        StartupProcess process, ThemeAppearance? initialSystemAppearance, Func<ThemeSnapshot?>? resolveStartupGlobalParent)
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
            // Application-level language registration needs the Application instance, so it runs here. The registry
            // sorts the catalog and bundles, so the registration order does not matter.
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
            applicationScope.InitializeApplication(initialSystemAppearance, resolveStartupGlobalParent?.Invoke());
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
