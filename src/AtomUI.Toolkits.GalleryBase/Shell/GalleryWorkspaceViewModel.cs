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
using Avalonia.Media;
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
    private readonly Lazy<ReactiveCommand<string, Unit>> _switchDesignLanguageCommand;
    private readonly Lazy<ReactiveCommand<Color?, Unit>> _setBrandColorCommand;
    private readonly Dictionary<string, Color?> _brandByLanguage = new(StringComparer.Ordinal);
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

    /// <summary>
    /// Gets the command that switches to a design language, keeping the appearance, density and the brand color
    /// remembered for that language.
    /// </summary>
    public ReactiveCommand<string, Unit> SwitchDesignLanguageCommand => _switchDesignLanguageCommand.Value;

    /// <summary>
    /// Gets the command that sets the brand color of the current design language. It does nothing when the language
    /// does not accept a brand color; a color equal to the theme's own accent color clears the choice.
    /// </summary>
    public ReactiveCommand<Color?, Unit> SetBrandColorCommand => _setBrandColorCommand.Value;

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

    /// <summary>
    /// Gets the design language of the current theme, or <see langword="null"/> for a theme that declares none.
    /// </summary>
    public string? CurrentDesignLanguage => FindTheme(CurrentThemeId)?.DesignLanguage;

    /// <summary>
    /// Gets whether the current design language accepts a replaceable brand color.
    /// </summary>
    public bool CanChooseBrandColor =>
        FindTheme(CurrentThemeId) is { DesignLanguage: not null, BrandColor: ThemeBrandColorSupport.Seed };

    /// <summary>
    /// Gets the brand color remembered for the current design language, or <see langword="null"/> when the theme's own
    /// color is in use. The memory lives in this view model only.
    /// </summary>
    public Color? BrandColor =>
        CurrentDesignLanguage is { } language && CanChooseBrandColor
            ? _brandByLanguage.GetValueOrDefault(language)
            : null;

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
        _switchDesignLanguageCommand = new(() => ReactiveCommand.CreateFromTask<string>(SwitchDesignLanguageAsync));
        _setBrandColorCommand = new(() => ReactiveCommand.CreateFromTask<Color?>(SetBrandColorAsync));

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

    private async Task SwitchThemeAsync(string themeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        if (string.Equals(themeId, CurrentThemeId, StringComparison.Ordinal))
        {
            return;
        }
        // A base theme that takes the toggles keeps the gallery's appearance; only a profile applied as defined
        // decides it.
        var takesToggles = ResolveBaseThemeWithToggles(themeId) is not null;
        await ApplyThemeSettingsAsync(themeId);
        if (!takesToggles)
        {
            FollowAppliedProfileAppearance(themeId);
        }
    }

    private async Task SwitchDesignLanguageAsync(string designLanguage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(designLanguage);
        if (string.Equals(designLanguage, CurrentDesignLanguage, StringComparison.Ordinal))
        {
            return;
        }
        // The catalog may have changed since the caller listed the language; an unknown language is ignored instead of
        // faulting the command.
        if (!AvailableThemes.Any(theme => string.Equals(theme.DesignLanguage, designLanguage, StringComparison.Ordinal)))
        {
            return;
        }
        await ApplyThemeSettingsAsync(designLanguage: designLanguage);
        FollowAppliedProfileAppearance(null);
    }

    private async Task SetBrandColorAsync(Color? color)
    {
        if (CurrentDesignLanguage is not { } language || !CanChooseBrandColor)
        {
            return;
        }

        // A color equal to the theme's own accent color means "no custom color".
        if (color is { } chosen && FindTheme(CurrentThemeId)?.AccentColor == chosen)
        {
            color = null;
        }
        if (_brandByLanguage.GetValueOrDefault(language) == color)
        {
            return;
        }

        _brandByLanguage[language] = color;
        this.RaisePropertyChanged(nameof(BrandColor));
        await ApplyThemeSettingsAsync();
    }

    // Picking a language profile picks its appearance: the appearance mode follows the applied profile, unless the
    // system mode already yields that appearance.
    private void FollowAppliedProfileAppearance(string? requestedThemeId)
    {
        if (_themeManager?.CurrentTheme is not { } state ||
            (requestedThemeId is not null && !string.Equals(state.ThemeId, requestedThemeId, StringComparison.Ordinal)) ||
            FindTheme(state.ThemeId)?.DesignLanguage is null)
        {
            return;
        }

        var isDark = state.Appearance == ThemeAppearance.Dark;
        var followsSystem = AppearanceMode == ThemePreference.System &&
                            (_systemAppearanceSource.GetCurrentAppearance() == ThemeAppearance.Dark) == isDark;
        if (!followsSystem)
        {
            ReleaseSystemAppearanceSubscription();
            AppearanceMode = isDark ? ThemePreference.Dark : ThemePreference.Light;
        }
    }

    private async Task ApplyThemeSettingsAsync(string? requestedThemeId = null,
                                               string? designLanguage = null,
                                               ThemeTransitionReason reason = ThemeTransitionReason.UserRequest)
    {
        if (_themeManager is null)
        {
            return;
        }

        var currentThemeId = _themeManager.CurrentTheme?.ThemeId ?? IThemeManager.DEFAULT_THEME_ID;
        var currentLanguage = FindTheme(currentThemeId)?.DesignLanguage;
        var builder        = new ThemeConfigBuilder();
        string themeId;
        Color? brandSeed = null;
        var requestedLanguage = requestedThemeId is not null ? FindTheme(requestedThemeId)?.DesignLanguage : null;

        ThemeSelectionResult? resolved = null;
        if (designLanguage is not null || (requestedThemeId is null && currentLanguage is not null))
        {
            // The resolver picks the profile that matches the appearance and density, or falls back to a definition of
            // the language with the built-in dark and compact algorithms.
            resolved = ThemeSelectionResolver.Resolve(AvailableThemes, CreateSelection(designLanguage ?? currentLanguage!));
        }
        else if (requestedThemeId is not null)
        {
            resolved = ResolveBaseThemeWithToggles(requestedThemeId);
        }

        if (resolved is not null)
        {
            themeId   = resolved.ThemeId;
            brandSeed = resolved.BrandSeed;
            if (resolved.Algorithms is not null)
            {
                builder.WithAlgorithms(resolved.Algorithms.ToArray());
            }
        }
        else if (requestedLanguage is not null)
        {
            // A picked language profile applies as defined, with the brand color remembered for its language.
            themeId = requestedThemeId!;
            if (FindTheme(themeId)?.BrandColor == ThemeBrandColorSupport.Seed)
            {
                brandSeed = _brandByLanguage.GetValueOrDefault(requestedLanguage);
            }
        }
        else
        {
            // Plain themes take the gallery's appearance and density as algorithms. A theme that replaces a language
            // profile starts from Default instead of carrying the profile's own algorithm.
            themeId = requestedThemeId ?? currentThemeId;
            var baseAlgorithms = requestedThemeId is not null && currentLanguage is not null
                ? [ThemeAlgorithm.Default]
                : _baseAlgorithms;
            var algorithms = new List<ThemeAlgorithm>(baseAlgorithms.Length + 2);
            algorithms.AddRange(baseAlgorithms);
            if (_isCompact)
            {
                algorithms.Add(ThemeAlgorithm.Compact);
            }
            if (_isDark)
            {
                algorithms.Add(ThemeAlgorithm.Dark);
            }
            builder.WithAlgorithms(algorithms.ToArray());
        }

        if (brandSeed is { } seed)
        {
            builder.WithToken(nameof(SharedTokenKind.ColorPrimary), FormatColor(seed));
        }

        var config = builder
                     .WithToken(nameof(SharedTokenKind.EnableMotion), _isMotionEnabled ? "true" : "false")
                     .WithToken(nameof(SharedTokenKind.EnableWaveSpirit), _isWaveSpiritEnabled ? "true" : "false")
                     .Build();
        var result = await _themeManager.ApplyThemeAsync(new ThemeRequest(themeId, config, reason));
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

    private ThemeSelection CreateSelection(string language)
    {
        return new ThemeSelection(
            language,
            _isDark ? ThemeAppearance.Dark : ThemeAppearance.Light,
            _isCompact ? ThemeDensity.Compact : ThemeDensity.Normal,
            _brandByLanguage.GetValueOrDefault(language));
    }

    // A requested theme that is its language's light, normal base, while the language has no variant for the current
    // Dark/Compact toggles, takes the toggles the way a theme without a language does (SwitchThemeCommand keeps its
    // contract). Returns null when the theme applies as defined instead.
    private ThemeSelectionResult? ResolveBaseThemeWithToggles(string themeId)
    {
        if (FindTheme(themeId) is not
            { DesignLanguage: { } language, Appearance: ThemeAppearance.Light, Density: ThemeDensity.Normal })
        {
            return null;
        }

        var resolved = ThemeSelectionResolver.Resolve(AvailableThemes, CreateSelection(language));
        return resolved.Algorithms is not null && string.Equals(resolved.ThemeId, themeId, StringComparison.Ordinal)
            ? resolved
            : null;
    }

    private static string FormatColor(Color color)
    {
        return color.A == 255
            ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
            : $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
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
        RaiseDesignLanguageChanged();
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
            RaiseDesignLanguageChanged();
            _isDark = state.Appearance == ThemeAppearance.Dark;
            if (!_hasExplicitAppearanceMode)
            {
                AppearanceMode = _isDark
                    ? ThemePreference.Dark
                    : ThemePreference.Light;
            }
            _isCompact = state.Algorithms.Contains(ThemeAlgorithm.Compact) ||
                         FindTheme(state.ThemeId)?.Density == ThemeDensity.Compact;
            _baseAlgorithms = state.Algorithms
                                   .Where(static algorithm =>
                                       algorithm != ThemeAlgorithm.Compact && algorithm != ThemeAlgorithm.Dark)
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

    private ThemeInfo? FindTheme(string themeId)
    {
        foreach (var theme in AvailableThemes)
        {
            if (string.Equals(theme.Id, themeId, StringComparison.Ordinal))
            {
                return theme;
            }
        }
        return null;
    }

    private void RaiseDesignLanguageChanged()
    {
        this.RaisePropertyChanged(nameof(CurrentDesignLanguage));
        this.RaisePropertyChanged(nameof(CanChooseBrandColor));
        this.RaisePropertyChanged(nameof(BrandColor));
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
