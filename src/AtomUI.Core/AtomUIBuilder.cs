using AtomUI.Localization;
using AtomUI.Registration;
using AtomUI.Theme;
using Avalonia;

namespace AtomUI;

internal sealed class AtomUIBuilder : IAtomUIBuilder
{
    private readonly Dictionary<string, object> _extensionStates = new(StringComparer.Ordinal);
    private readonly List<AtomUIOwnedServiceRegistration> _ownedServiceRegistrations = [];
    private readonly HashSet<string> _ownedServiceIds = new(StringComparer.Ordinal);

    internal AtomUIBuilder(Type? applicationType) : this(applicationType, null, null)
    {
    }

    internal AtomUIBuilder(Type? applicationType, IEnumerable<string>? existingPackages, IEnumerable<string>? existingServiceIds)
    {
        ThemeManagerBuilder = new ThemeManagerBuilder(applicationType);
        LocalizationBuilder = new LocalizationBuilder(appendOnly: existingPackages is not null);
        if (existingPackages is not null)
        {
            ControlRegistrationState.BeginAppend(existingPackages);
        }
        if (existingServiceIds is not null)
        {
            _ownedServiceIds.UnionWith(existingServiceIds);
        }
    }

    public IThemeManagerBuilder Theme => ThemeManagerBuilder;

    public ILocalizationBuilder Localization => LocalizationBuilder;

    internal ThemeManagerBuilder ThemeManagerBuilder { get; }

    internal LocalizationBuilder LocalizationBuilder { get; }

    internal ControlRegistrationState ControlRegistrationState => ThemeManagerBuilder.ControlRegistrationState;

    internal IReadOnlyList<AtomUIOwnedServiceRegistration> OwnedServiceRegistrations => _ownedServiceRegistrations;

    internal IReadOnlyList<EnabledControlPackage> PackageDeclarations => ThemeManagerBuilder.PackageDeclarations;

    internal void AddPackageDeclaration(EnabledControlPackage declaration) => ThemeManagerBuilder.AddPackageDeclaration(declaration);

    internal Func<AtomUIMainWindowContext, Task<Avalonia.Controls.Window>>? MainWindowFactory { get; private set; }

    internal AtomUISplashRegistration? Splash { get; private set; }

    internal void SetMainWindowFactory(Func<AtomUIMainWindowContext, Task<Avalonia.Controls.Window>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        if (MainWindowFactory is not null)
        {
            throw new InvalidOperationException("UseMainWindow can only be called once.");
        }
        MainWindowFactory = factory;
    }

    internal void SetSplash(AtomUISplashRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        if (Splash is not null)
        {
            throw new InvalidOperationException("UseSplash can only be called once.");
        }
        Splash = registration;
    }

    internal TState GetOrAddExtensionState<TState>(string id, Func<TState> factory)
        where TState : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(factory);
        ControlRegistrationState.ThrowIfUnavailable();
        if (_extensionStates.TryGetValue(id, out var current))
        {
            return (TState)current;
        }

        var created = factory();
        _extensionStates.Add(id, created);
        return created;
    }

    internal void AddOwnedService(
        string id,
        Func<Application, IAtomUIOwnedService> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(factory);
        ControlRegistrationState.ThrowIfUnavailable();
        if (!_ownedServiceIds.Add(id))
        {
            return;
        }

        _ownedServiceRegistrations.Add(new AtomUIOwnedServiceRegistration(id, factory));
    }
}
