using System.Diagnostics;
using System.Reactive;
using AtomUI.Localization;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.Navigation;
using Avalonia;
using ReactiveUI;

namespace AtomUI.Toolkits.GalleryBase.Shell;

public class GalleryWorkspaceViewModel : ReactiveObject, IScreen, IDisposable
{
    private readonly IThemeManager? _themeManager;
    private readonly ILanguageManager? _languageManager;
    private readonly IGallerySystemAppearanceSource _systemAppearanceSource;
    private readonly EventHandler<ThemeChangedEventArgs>? _themeChangedHandler;
    private readonly EventHandler<ThemeCatalogChangedEventArgs>? _themeCatalogChangedHandler;
    private readonly EventHandler<LanguageChangedEventArgs>? _languageChangedHandler;
    private readonly Lazy<ReactiveCommand<bool, Unit>> _toggleDarkModeCommand;
    private readonly Lazy<ReactiveCommand<ThemePreference, Unit>> _setAppearanceModeCommand;
    private readonly Lazy<ReactiveCommand<bool, Unit>> _toggleCompactModeCommand;
    private readonly Lazy<ReactiveCommand<bool, Unit>> _toggleMotionCommand;
    private readonly Lazy<ReactiveCommand<bool, Unit>> _toggleWaveSpiritCommand;
    private readonly Lazy<ReactiveCommand<string, Unit>> _switchThemeCommand;
    private readonly Lazy<ReactiveCommand<Unit, Unit>> _switchToZhCNCommand;
    private readonly Lazy<ReactiveCommand<Unit, Unit>> _switchToZhTWCommand;
    private readonly Lazy<ReactiveCommand<Unit, Unit>> _switchToEnUSCommand;
    private readonly Lazy<ReactiveCommand<Unit, Unit>> _switchToPtBRCommand;
    private IDisposable? _systemAppearanceSubscription;
    private bool _isDisposed;
    private bool _hasExplicitAppearanceMode;
    private bool _isDark;
    private bool _isCompact;
    private bool _isMotionEnabled = true;
    private bool _isWaveSpiritEnabled = true;
    private ThemeAlgorithm[] _baseAlgorithms = [ThemeAlgorithm.Default];
    private IReadOnlyList<ThemeInfo> _availableThemes = Array.Empty<ThemeInfo>();
    private string _currentThemeId = IThemeManager.DEFAULT_THEME_ID;
    private ThemePreference _appearanceMode = ThemePreference.Light;

    private bool _isZhCN;
    private bool _isZhTW;
    private bool _isEnUS;
    private bool _isPtBR;

    public RoutingState Router { get; } = new();

    public GalleryNavigationViewModel Navigation { get; }

    public ReactiveCommand<bool, Unit> ToggleDarkModeCommand => _toggleDarkModeCommand.Value;

    public ReactiveCommand<ThemePreference, Unit> SetAppearanceModeCommand => _setAppearanceModeCommand.Value;

    public ReactiveCommand<bool, Unit> ToggleCompactModeCommand => _toggleCompactModeCommand.Value;

    public ReactiveCommand<bool, Unit> ToggleMotionCommand => _toggleMotionCommand.Value;

    public ReactiveCommand<bool, Unit> ToggleWaveSpiritCommand => _toggleWaveSpiritCommand.Value;

    public ReactiveCommand<string, Unit> SwitchThemeCommand => _switchThemeCommand.Value;

    public ReactiveCommand<Unit, Unit> SwitchToZhCNCommand => _switchToZhCNCommand.Value;

    public ReactiveCommand<Unit, Unit> SwitchToZhTWCommand => _switchToZhTWCommand.Value;

    public ReactiveCommand<Unit, Unit> SwitchToEnUSCommand => _switchToEnUSCommand.Value;

    public ReactiveCommand<Unit, Unit> SwitchToPtBRCommand => _switchToPtBRCommand.Value;

    public IReadOnlyList<ThemeInfo> AvailableThemes
    {
        get => _availableThemes;
        private set => this.RaiseAndSetIfChanged(ref _availableThemes, value);
    }

    public string CurrentThemeId
    {
        get => _currentThemeId;
        private set => this.RaiseAndSetIfChanged(ref _currentThemeId, value);
    }

    public ThemePreference AppearanceMode
    {
        get => _appearanceMode;
        private set => SetAppearanceModeState(value);
    }

    public bool IsLightAppearanceMode => AppearanceMode == ThemePreference.Light;

    public bool IsDarkAppearanceMode => AppearanceMode == ThemePreference.Dark;

    public bool IsSystemAppearanceMode => AppearanceMode == ThemePreference.System;

    public bool IsZhCN
    {
        get => _isZhCN;
        private set => this.RaiseAndSetIfChanged(ref _isZhCN, value);
    }

    public bool IsZhTW
    {
        get => _isZhTW;
        private set => this.RaiseAndSetIfChanged(ref _isZhTW, value);
    }

    public bool IsEnUS
    {
        get => _isEnUS;
        private set => this.RaiseAndSetIfChanged(ref _isEnUS, value);
    }

    public bool IsPtBR
    {
        get => _isPtBR;
        private set => this.RaiseAndSetIfChanged(ref _isPtBR, value);
    }

    public GalleryWorkspaceViewModel(GalleryBaseConfiguration configuration,
                                     Func<IScreen, GalleryNavigationViewModel>? navigationFactory = null)
        : this(configuration, navigationFactory, GallerySystemAppearanceSource.Instance)
    {
    }

    internal GalleryWorkspaceViewModel(GalleryBaseConfiguration configuration,
                                       Func<IScreen, GalleryNavigationViewModel>? navigationFactory,
                                       IGallerySystemAppearanceSource systemAppearanceSource,
                                       ILanguageManager? languageManager = null)
    {
        ArgumentNullException.ThrowIfNull(systemAppearanceSource);

        _systemAppearanceSource = systemAppearanceSource;
        Navigation = navigationFactory?.Invoke(this) ?? new GalleryNavigationViewModel(this, configuration);

        _themeManager = Application.Current?.GetThemeManager();
        _languageManager = languageManager ?? (Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)
            : null);
        AvailableThemes = CaptureThemes(_themeManager?.AvailableThemes);
        SyncThemeState(_themeManager?.CurrentTheme, null);
        SyncLanguageState(_languageManager?.Current);

        _toggleDarkModeCommand = new(() => ReactiveCommand.CreateFromTask<bool>(SetDarkModeAsync));
        _setAppearanceModeCommand = new(() => ReactiveCommand.CreateFromTask<ThemePreference>(SetAppearanceModeAsync));
        _toggleCompactModeCommand = new(() => ReactiveCommand.CreateFromTask<bool>(SetCompactModeAsync));
        _toggleMotionCommand = new(() => ReactiveCommand.CreateFromTask<bool>(SetMotionEnabledAsync));
        _toggleWaveSpiritCommand = new(() => ReactiveCommand.CreateFromTask<bool>(SetWaveSpiritEnabledAsync));
        _switchThemeCommand = new(() => ReactiveCommand.CreateFromTask<string>(SwitchThemeAsync));

        _switchToZhCNCommand = new(() => ReactiveCommand.Create(() => SetLanguage(LanguageTags.ZhCN)));
        _switchToZhTWCommand = new(() => ReactiveCommand.Create(() => SetLanguage(LanguageTags.ZhTW)));
        _switchToEnUSCommand = new(() => ReactiveCommand.Create(() => SetLanguage(LanguageTags.EnUS)));
        _switchToPtBRCommand = new(() => ReactiveCommand.Create(() => SetLanguage(LanguageTags.PtBR)));

        if (_themeManager is not null)
        {
            _themeChangedHandler = HandleThemeChanged;
            _themeCatalogChangedHandler = HandleThemeCatalogChanged;
            _themeManager.ThemeChanged += _themeChangedHandler;
            _themeManager.ThemeCatalogChanged += _themeCatalogChangedHandler;
        }
        if (_languageManager is not null)
        {
            _languageChangedHandler = HandleLanguageChanged;
            _languageManager.LanguageChanged += _languageChangedHandler;
        }
    }

    public virtual void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        ReleaseSystemAppearanceSubscription();
        if (_languageManager is not null && _languageChangedHandler is not null)
        {
            _languageManager.LanguageChanged -= _languageChangedHandler;
        }
        if (_themeManager is not null && _themeChangedHandler is not null)
        {
            _themeManager.ThemeChanged -= _themeChangedHandler;
        }
        if (_themeManager is not null && _themeCatalogChangedHandler is not null)
        {
            _themeManager.ThemeCatalogChanged -= _themeCatalogChangedHandler;
        }

        Navigation.Dispose();
    }

    private async Task SetDarkModeAsync(bool isDark)
    {
        await SetAppearanceModeAsync(
            isDark
                ? ThemePreference.Dark
                : ThemePreference.Light);
    }

    private async Task SetAppearanceModeAsync(ThemePreference appearanceMode)
    {
        ValidateAppearanceMode(appearanceMode);

        _hasExplicitAppearanceMode = true;
        AppearanceMode             = appearanceMode;
        if (appearanceMode == ThemePreference.System)
        {
            SubscribeSystemAppearance();
            _isDark = _systemAppearanceSource.GetCurrentAppearance() == ThemeAppearance.Dark;
            await ApplyThemeSettingsAsync(reason: ThemeTransitionReason.FollowSystem);
            return;
        }

        ReleaseSystemAppearanceSubscription();
        _isDark = appearanceMode == ThemePreference.Dark;
        await ApplyThemeSettingsAsync();
    }

    private async Task SetCompactModeAsync(bool isCompact)
    {
        _isCompact = isCompact;
        await ApplyThemeSettingsAsync();
    }

    private async Task SetMotionEnabledAsync(bool enabled)
    {
        _isMotionEnabled = enabled;
        if (!enabled)
        {
            _isWaveSpiritEnabled = false;
        }
        await ApplyThemeSettingsAsync();
    }

    private async Task SetWaveSpiritEnabledAsync(bool enabled)
    {
        if (enabled)
        {
            _isMotionEnabled = true;
        }
        _isWaveSpiritEnabled = enabled;
        await ApplyThemeSettingsAsync();
    }

    private Task SwitchThemeAsync(string themeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        if (string.Equals(themeId, CurrentThemeId, StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }
        return ApplyThemeSettingsAsync(themeId);
    }

    private async Task ApplyThemeSettingsAsync(string? requestedThemeId = null,
                                               ThemeTransitionReason reason = ThemeTransitionReason.UserRequest)
    {
        if (_themeManager is null)
        {
            return;
        }

        var algorithms = new List<ThemeAlgorithm>(_baseAlgorithms.Length + 2);
        algorithms.AddRange(_baseAlgorithms);
        if (_isCompact)
        {
            algorithms.Add(ThemeAlgorithm.Compact);
        }
        if (_isDark)
        {
            algorithms.Add(ThemeAlgorithm.Dark);
        }

        var config = new ThemeConfigBuilder()
                     .WithAlgorithms(algorithms.ToArray())
                     .WithToken(nameof(SharedTokenKind.EnableMotion), _isMotionEnabled ? "true" : "false")
                     .WithToken(nameof(SharedTokenKind.EnableWaveSpirit), _isWaveSpiritEnabled ? "true" : "false")
                     .Build();
        var result = await _themeManager.ApplyThemeAsync(
            new ThemeRequest(
                requestedThemeId ??
                _themeManager.CurrentTheme?.ThemeId ??
                IThemeManager.DEFAULT_THEME_ID,
                config,
                reason));
        if (result.Status == ThemeTransitionStatus.Failed)
        {
            this.RaisePropertyChanged(nameof(CurrentThemeId));
            var message = string.Join(" ", result.Diagnostics.Select(static diagnostic => diagnostic.Message));
            throw new ThemeLoadException(message, result.Exception);
        }
        if (result.Status == ThemeTransitionStatus.Superseded)
        {
            this.RaisePropertyChanged(nameof(CurrentThemeId));
        }
    }

    private void SubscribeSystemAppearance()
    {
        ReleaseSystemAppearanceSubscription();
        _systemAppearanceSubscription = _systemAppearanceSource.Subscribe(HandleSystemAppearanceChanged);
    }

    private void ReleaseSystemAppearanceSubscription()
    {
        _systemAppearanceSubscription?.Dispose();
        _systemAppearanceSubscription = null;
    }

    private async void HandleSystemAppearanceChanged(ThemeAppearance appearance)
    {
        if (_isDisposed || AppearanceMode != ThemePreference.System)
        {
            return;
        }

        var isDark = appearance == ThemeAppearance.Dark;
        if (_isDark == isDark)
        {
            return;
        }

        _isDark = isDark;
        try
        {
            await ApplyThemeSettingsAsync(reason: ThemeTransitionReason.FollowSystem).ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private void HandleThemeChanged(object? sender, ThemeChangedEventArgs args)
    {
        if (!_isDisposed)
        {
            SyncThemeState(args.State, args.Request.Config);
        }
    }

    private void HandleThemeCatalogChanged(object? sender, ThemeCatalogChangedEventArgs args)
    {
        if (_isDisposed)
        {
            return;
        }

        AvailableThemes = CaptureThemes(args.AvailableThemes);
        if (args.CurrentTheme is not null)
        {
            CurrentThemeId = args.CurrentTheme.ThemeId;
        }
    }

    private void HandleLanguageChanged(object? sender, LanguageChangedEventArgs args)
    {
        if (_isDisposed)
        {
            return;
        }

        SyncLanguageState(args.Result.NewState);
    }

    private void SyncLanguageState(LanguageState? state)
    {
        IsZhCN = state?.CurrentLanguage == LanguageTags.ZhCN;
        IsZhTW = state?.CurrentLanguage == LanguageTags.ZhTW;
        IsEnUS = state?.CurrentLanguage == LanguageTags.EnUS;
        IsPtBR = state?.CurrentLanguage == LanguageTags.PtBR;
    }

    private void SyncThemeState(ThemeState? state, ThemeConfig? config)
    {
        if (state is not null)
        {
            CurrentThemeId = state.ThemeId;
            _isDark = state.Appearance == ThemeAppearance.Dark;
            if (!_hasExplicitAppearanceMode)
            {
                AppearanceMode = _isDark
                    ? ThemePreference.Dark
                    : ThemePreference.Light;
            }
            _isCompact = state.Algorithms.Contains(ThemeAlgorithm.Compact);
            _baseAlgorithms = state.Algorithms
                                   .Where(static algorithm =>
                                       algorithm is not ThemeAlgorithm.Compact and not ThemeAlgorithm.Dark)
                                   .ToArray();
            if (_baseAlgorithms.Length == 0)
            {
                _baseAlgorithms = [ThemeAlgorithm.Default];
            }
        }

        _isMotionEnabled = ReadBooleanToken(config, nameof(SharedTokenKind.EnableMotion), true);
        _isWaveSpiritEnabled = _isMotionEnabled &&
                               ReadBooleanToken(config, nameof(SharedTokenKind.EnableWaveSpirit), true);
    }

    private void SetAppearanceModeState(ThemePreference appearanceMode)
    {
        if (_appearanceMode == appearanceMode)
        {
            return;
        }

        _appearanceMode = appearanceMode;
        this.RaisePropertyChanged(nameof(AppearanceMode));
        this.RaisePropertyChanged(nameof(IsLightAppearanceMode));
        this.RaisePropertyChanged(nameof(IsDarkAppearanceMode));
        this.RaisePropertyChanged(nameof(IsSystemAppearanceMode));
    }

    private static IReadOnlyList<ThemeInfo> CaptureThemes(IReadOnlyList<ThemeInfo>? themes)
    {
        return themes is null
            ? Array.Empty<ThemeInfo>()
            : Array.AsReadOnly(themes.ToArray());
    }

    private static bool ReadBooleanToken(ThemeConfig? config, string name, bool defaultValue)
    {
        return config is not null &&
               config.Tokens.TryGetValue(name, out var value) &&
               bool.TryParse(value, out var parsed)
            ? parsed
            : defaultValue;
    }

    private static void ValidateAppearanceMode(ThemePreference appearanceMode)
    {
        if (appearanceMode is not ThemePreference.Light and
            not ThemePreference.Dark and
            not ThemePreference.System)
        {
            throw new ArgumentOutOfRangeException(
                nameof(appearanceMode),
                appearanceMode,
                "Unsupported gallery theme appearance mode.");
        }
    }

    private void SetLanguage(LanguageTag language)
    {
        if (_languageManager is not null)
        {
            _languageManager.ChangeLanguage(language);
        }
    }
}
