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

    internal AtomUIBuilder(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        ThemeManagerBuilder = new ThemeManagerBuilder(application);
        LocalizationBuilder = new LocalizationBuilder();
    }

    public IThemeManagerBuilder Theme => ThemeManagerBuilder;

    public ILocalizationBuilder Localization => LocalizationBuilder;

    private readonly List<Action> _deferredRegistrations = [];

    internal IReadOnlyList<Action> DeferredRegistrations => _deferredRegistrations;

    internal IAtomUIBuilder Deferred(Action registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        _deferredRegistrations.Add(registration);
        return this;
    }

    internal ThemeManagerBuilder ThemeManagerBuilder { get; }

    internal LocalizationBuilder LocalizationBuilder { get; }

    internal ControlRegistrationState ControlRegistrationState => ThemeManagerBuilder.ControlRegistrationState;

    internal IReadOnlyList<AtomUIOwnedServiceRegistration> OwnedServiceRegistrations => _ownedServiceRegistrations;

    internal TState GetOrAddExtensionState<TState>(string id, Func<TState> factory)
        where TState : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(factory);
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
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
        ControlRegistrationState.ThrowIfInitialConfigurationUnavailable();
        if (!_ownedServiceIds.Add(id))
        {
            return;
        }

        _ownedServiceRegistrations.Add(new AtomUIOwnedServiceRegistration(id, factory));
    }
}
