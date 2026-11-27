using System.Runtime.ExceptionServices;
using AtomUI.Localization;
using AtomUI.Registration;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AtomUI;

internal sealed class ApplicationScope : IAtomUI, IDisposable
{
    private Application? _application;
    private IResourceDictionary? _applicationResources;
    private readonly List<IAtomUIOwnedService> _ownedServices;
    private readonly HashSet<string> _ownedServiceIds;
    private readonly List<PackageOwnership> _additionalOwnership = [];
    private bool _loadingPackages;
    private bool _registrationCompleted;
    private int _attachedOwnedServiceCount;
    private int _initialized;
    private int _disposed;

    internal ApplicationScope(
        Application application,
        ThemeManager themeManager,
        LocalizationHost localizationHost,
        IReadOnlyList<IAtomUIOwnedService> ownedServices,
        StartupProcess startup,
        IEnumerable<string>? ownedServiceIds = null)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
        ThemeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        LocalizationHost = localizationHost ?? throw new ArgumentNullException(nameof(localizationHost));
        ArgumentNullException.ThrowIfNull(ownedServices);
        _ownedServices = ownedServices.ToList();
        _ownedServiceIds = (ownedServiceIds ?? []).ToHashSet(StringComparer.Ordinal);
        Startup = startup ?? throw new ArgumentNullException(nameof(startup));
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

    // The startup state and the awaitable result are owned by the same object; the state is advanced by the
    // startup orchestration.
    internal StartupProcess Startup { get; }

    internal void ReleaseStartupInputs()
    {
        ThemeManager.CompleteStartupRegistration();
        _registrationCompleted = true;
    }

    internal void InitializeApplication(
        ThemeAppearance? initialSystemAppearance = null,
        ThemeSnapshot? startupGlobalParent = null)
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

        try
        {
            // Application AXAML may replace Resources. The root Styles resource dictionary
            // survives that assignment; its existing application dictionaries retain priority.
            _applicationResources = application.Styles.Resources;
            _applicationResources.MergedDictionaries.Insert(0, ResourceProvider);
            application.Styles.Add(FlowDirectionStyle);
            ThemeManager.InitializeApplication(application, initialSystemAppearance, startupGlobalParent);
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
            DetachApplicationResources(application, failures);
            ThrowFailures("AtomUI application initialization and rollback failed.", failures);
        }
    }

    public void LoadPackages(Action<IAtomUIBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        Dispatcher.UIThread.VerifyAccess();
        ThrowIfDisposed();
        if (_loadingPackages || !_registrationCompleted || Volatile.Read(ref _initialized) == 0)
        {
            throw new InvalidOperationException("Package loading requires an initialized, idle application scope.");
        }
        ThemeManager.BeginPackageLoading();
        var languageEntered = false;
        _loadingPackages = true;
        AtomUIBuilder? builder = null;
        ThemeManager.PackageUpdate? themes = null;
        LanguageManager.PackageUpdate? languages = null;
        var services = new List<IAtomUIOwnedService>();
        var attached = 0;
        var rolledBack = false;

        void Rollback(ICollection<Exception> failures)
        {
            if (rolledBack)
            {
                return;
            }
            rolledBack = true;
            TryCleanup(() => languages?.Rollback(), failures);
            TryCleanup(() => themes?.Rollback(), failures);
            if (builder is not null)
            {
                foreach (var package in builder.PackageDeclarations.Reverse())
                {
                    TryCleanup(package.Release, failures);
                }
            }
            for (var index = attached - 1; index >= 0; index--)
            {
                var service = services[index];
                TryCleanup(() => service.Detach(_application!), failures);
            }
            for (var index = services.Count - 1; index >= 0; index--)
            {
                TryCleanup(services[index].Dispose, failures);
            }
        }

        try
        {
            LanguageManager.BeginPackageLoading();
            languageEntered = true;
            builder = new AtomUIBuilder(_application!.GetType(), ThemeManager.EnabledPackageIds, _ownedServiceIds);
            configure(builder);
            builder.ControlRegistrationState.ThrowIfUnavailable();
            if (builder.PackageDeclarations.Count == 0 && builder.ThemeManagerBuilder.ControlPackages.Count == 0 &&
                builder.ThemeManagerBuilder.Languages.Count == 0 &&
                !builder.ThemeManagerBuilder.HasControlContributions &&
                builder.OwnedServiceRegistrations.Count == 0 && builder.LocalizationBuilder.ModuleData.Count == 0 &&
                builder.LocalizationBuilder.Catalogs.Count == 0 && builder.LocalizationBuilder.TranslationBundles.Count == 0)
            {
                return;
            }
            languages = LanguageManager.PreparePackages(builder.LocalizationBuilder);
            themes = ThemeManager.PreparePackages(builder.ThemeManagerBuilder);
            foreach (var registration in builder.OwnedServiceRegistrations)
            {
                services.Add(registration.Factory(_application) ??
                    throw new InvalidOperationException($"Owned service '{registration.Id}' returned null."));
            }
            var ownership = new PackageOwnership(builder.PackageDeclarations.ToArray(), services.ToArray());
            _additionalOwnership.EnsureCapacity(_additionalOwnership.Count + 1);
            _ownedServiceIds.EnsureCapacity(_ownedServiceIds.Count + builder.OwnedServiceRegistrations.Count);
            var publication = new PackagePublication();
            themes.RunResourceUpdate(() =>
            {
                try
                {
                    languages.Stage(publication);
                    themes.Stage(publication, languages.RunCommittedRead);
                    foreach (var package in builder.PackageDeclarations)
                    {
                        package.PrepareContext(_application, ThemeManager);
                    }
                    for (var index = 0; index < services.Count; index++)
                    {
                        // Attach may acquire ownership before throwing; the attempted service must detach too.
                        attached = index + 1;
                        services[index].Attach(_application);
                    }
                    foreach (var package in builder.PackageDeclarations)
                    {
                        package.Commit(new ControlPackageContext(_application, ThemeManager));
                    }
                    themes.EnsureCurrent();
                }
                catch (Exception error)
                {
                    var failures = new List<Exception> { error };
                    Rollback(failures);
                    ThrowFailures("Package append and rollback failed.", failures);
                }
            });
            _additionalOwnership.Add(ownership);
            _ownedServiceIds.UnionWith(builder.OwnedServiceRegistrations.Select(registration => registration.Id));
            // Every fallible hook/attach/resource finalization has finished. The staged read
            // views share this fence, so background readers switch generations together.
            publication.Complete();
            themes.Commit();
            languages.Commit();
            themes.CompleteCommit();
            languages.CompleteCommit();
            themes.Publish();
        }
        catch (Exception error)
        {
            var failures = new List<Exception> { error };
            if (themes is not null)
            {
                TryCleanup(() => themes.RunResourceUpdate(() => Rollback(failures)), failures);
            }
            else
            {
                Rollback(failures);
            }
            ThrowFailures("Package append and rollback failed.", failures);
        }
        finally
        {
            builder?.ControlRegistrationState.Close();
            builder?.LocalizationBuilder.Close();
            if (languageEntered)
            {
                LanguageManager.EndPackageLoading();
            }
            ThemeManager.EndPackageLoading();
            _loadingPackages = false;
        }
    }

    public void Dispose()
    {
        if (_loadingPackages)
        {
            throw new InvalidOperationException("The application scope cannot be disposed during package loading.");
        }
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        var failures = new List<Exception>();
        var application = Interlocked.Exchange(ref _application, null);
        if (application is not null)
        {
            TryCleanup(() => ApplicationScopeRegistry.Unregister(application, this), failures);
            ReleaseAdditionalOwnership(application, failures);
            DetachOwnedServices(application, failures);
            TryCleanup(() => application.Styles.Remove(FlowDirectionStyle), failures);
            DetachApplicationResources(application, failures);
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

    private void DetachApplicationResources(Application application, ICollection<Exception> failures)
    {
        var resources = _applicationResources;
        _applicationResources = null;
        if (resources is not null)
        {
            TryCleanup(() => resources.MergedDictionaries.Remove(ResourceProvider), failures);
            // Collection/owner notifications can throw after insertion or removal. Release
            // our provider independently even if the collection callback did not finish.
            TryCleanup(() => ((IResourceProvider)ResourceProvider).RemoveOwner(application), failures);
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

    private void ReleaseAdditionalOwnership(Application application, ICollection<Exception> failures)
    {
        for (var batch = _additionalOwnership.Count - 1; batch >= 0; batch--)
        {
            var ownership = _additionalOwnership[batch];
            for (var index = ownership.Packages.Count - 1; index >= 0; index--)
            {
                TryCleanup(ownership.Packages[index].Release, failures);
            }
            for (var index = ownership.Services.Count - 1; index >= 0; index--)
            {
                var service = ownership.Services[index];
                TryCleanup(() => service.Detach(application), failures);
            }
            for (var index = ownership.Services.Count - 1; index >= 0; index--)
            {
                TryCleanup(ownership.Services[index].Dispose, failures);
            }
        }
        _additionalOwnership.Clear();
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

    private sealed record PackageOwnership(IReadOnlyList<EnabledControlPackage> Packages,
        IReadOnlyList<IAtomUIOwnedService> Services);
}
