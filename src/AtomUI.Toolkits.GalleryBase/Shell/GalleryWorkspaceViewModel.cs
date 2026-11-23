using System.Diagnostics;
using System.Reactive;
using AtomUI.Localization;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Languages;
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
    private readonly Lazy<ReactiveCommand<Color?, Unit>> _setNeutralColorCommand;
    private readonly Dictionary<string, Color?> _neutralByLanguage = new(StringComparer.Ordinal);
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
    // A theme may declare EnableMotion/EnableWaveSpirit explicitly (the Shadcn styles declare no wave spirit).
    // Until the user touches the toggles, the declaration wins; an explicit user choice then overrides it.
    private bool _motionTouched;
    private bool _waveSpiritTouched;
    private IReadOnlyList<ThemeInfo> _availableThemes = Array.Empty<ThemeInfo>();
    private string _currentThemeId = string.Empty;
    // The language and style of the applied theme, the gallery's "design language" key being "Language[.Style]".
    private string? _currentLanguage;
    private DesignStyle? _currentStyle;
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

    /// <summary>Sets the neutral color of the current design language; null keeps the definition's own.</summary>
    public ReactiveCommand<Color?, Unit> SetNeutralColorCommand => _setNeutralColorCommand.Value;

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
    /// Gets the design language key of the current theme: the language identity, followed by <c>.Style</c> for a
    /// language that offers several styles (<c>AntDesign</c>, <c>Shadcn.Nova</c>).
    /// </summary>
    public string? CurrentDesignLanguage => _currentLanguage is { } language ? Key(language, _currentStyle) : null;

    /// <summary>The design language keys of the enabled languages, in catalog order.</summary>
    public IReadOnlyList<string> DesignLanguages => Languages.SelectMany(static language =>
        language.Styles.Count == 1
            ? [language.Id]
            : language.Styles.Select(style => language.Id + "." + style.Id)).ToArray();

    /// <summary>Gets whether the current design language accepts a replaceable brand color.</summary>
    public bool CanChooseBrandColor => IsAdjustable(nameof(SharedTokenKind.ColorPrimary));

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

    /// <summary>
    /// The language seed a neutral colour family choice writes: the base colour of shadcn/ui (ui.shadcn.com/create
    /// Base Color). A language that declares it as an adjustable seed offers the neutral picker.
    /// </summary>
    public const string NeutralSeedName = "BaseColor";

    /// <summary>Gets whether the current design language accepts a replaceable neutral color family.</summary>
    public bool CanChooseNeutralColor => IsAdjustable(NeutralSeedName);

    /// <summary>Gets the neutral color remembered for the current design language, if it accepts one.</summary>
    public Color? NeutralColor =>
        CurrentDesignLanguage is { } neutralLanguage && CanChooseNeutralColor
            ? _neutralByLanguage.GetValueOrDefault(neutralLanguage)
            : null;

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
        AvailableThemes = CaptureThemes(_themeManager?.Themes);
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
        _setNeutralColorCommand = new(() => ReactiveCommand.CreateFromTask<Color?>(SetNeutralColorAsync));

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
            await ApplyThemeSettingsAsync(ThemeTransitionReason.FollowSystem);
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
        _motionTouched = true;
        if (!enabled)
        {
            _isWaveSpiritEnabled = false;
            _waveSpiritTouched = true;
        }
        await ApplyThemeSettingsAsync();
    }

    private async Task SetWaveSpiritEnabledAsync(bool enabled)
    {
        if (enabled)
        {
            _isMotionEnabled = true;
            _motionTouched = true;
        }
        _isWaveSpiritEnabled = enabled;
        _waveSpiritTouched = true;
        await ApplyThemeSettingsAsync();
    }

    private async Task SwitchThemeAsync(string themeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        if (string.Equals(themeId, CurrentThemeId, StringComparison.Ordinal) || FindTheme(themeId) is not { } theme)
        {
            return;
        }

        // A theme that fixes the appearance decides it; an open theme takes the gallery's toggles.
        var style = theme.Style ?? (string.Equals(theme.Language, _currentLanguage, StringComparison.Ordinal) ? _currentStyle : null);
        await ApplyThemeSettingsAsync(theme.Language, style, themeId);
        if (theme.Appearance is not null)
        {
            FollowAppliedProfileAppearance();
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
        if (!TryParseKey(designLanguage, out var language, out var style))
        {
            return;
        }
        await ApplyThemeSettingsAsync(language, style, null);
    }

    private async Task SetNeutralColorAsync(Color? color)
    {
        if (CurrentDesignLanguage is not { } language || !CanChooseNeutralColor)
        {
            return;
        }

        // A color equal to the theme's own neutral seed means "no custom neutral".
        if (color is { } chosenNeutral && FindTheme(CurrentThemeId)?.SeedValues.TryGetValue(NeutralSeedName, out var own) == true &&
            Color.TryParse(own, out var ownNeutral) && ownNeutral == chosenNeutral)
        {
            color = null;
        }
        if (_neutralByLanguage.GetValueOrDefault(language) == color)
        {
            return;
        }

        _neutralByLanguage[language] = color;
        this.RaisePropertyChanged(nameof(NeutralColor));
        await ApplyThemeSettingsAsync();
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

    // A theme that fixes the appearance decides it: the appearance mode follows, unless the system mode yields it.
    private void FollowAppliedProfileAppearance()
    {
        if (_themeManager?.CurrentTheme is not { } state)
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
        _isDark = isDark;
    }

    private Task ApplyThemeSettingsAsync(ThemeTransitionReason reason = ThemeTransitionReason.UserRequest) =>
        _currentLanguage is { } language
            ? ApplyThemeSettingsAsync(language, _currentStyle, IsLanguageDefault(CurrentThemeId) ? null : CurrentThemeId)
            : Task.CompletedTask;

    // The gallery's selection: the language and style, the named theme (if any), its appearance and density toggles,
    // the brand and neutral colors remembered for the language, and the motion switches once the user touched them.
    private async Task ApplyThemeSettingsAsync(string language, DesignStyle? style, string? themeId)
    {
        if (_themeManager is null)
        {
            return;
        }

        var key = Key(language, style);
        var selection = ThemeSelection.Of(language, style)
                                      .WithAppearance(_isDark ? ThemeAppearance.Dark : ThemeAppearance.Light);
        if (themeId is not null)
        {
            selection = selection.WithTheme(themeId);
        }
        var info = Languages.FirstOrDefault(candidate => candidate.Id == language);
        if (_isCompact && info?.Densities.Contains(DesignDensity.Compact) == true)
        {
            selection = selection.WithDensity(DesignDensity.Compact);
        }
        if (info?.AdjustableSeeds.Contains(nameof(SharedTokenKind.ColorPrimary)) == true &&
            _brandByLanguage.GetValueOrDefault(key) is { } brand)
        {
            selection = selection.WithSeed(nameof(SharedTokenKind.ColorPrimary), FormatColor(brand));
        }
        if (info?.AdjustableSeeds.Contains(NeutralSeedName) == true &&
            _neutralByLanguage.GetValueOrDefault(key) is { } neutral)
        {
            selection = selection.WithLanguageSeed(NeutralSeedName, FormatColor(neutral));
        }

        // Only an explicit user choice overrides the theme's own declarations.
        if (_motionTouched)
        {
            selection = selection.WithSeed(nameof(SharedTokenKind.EnableMotion), _isMotionEnabled ? "true" : "false");
        }
        if (_waveSpiritTouched)
        {
            selection = selection.WithSeed(nameof(SharedTokenKind.EnableWaveSpirit), _isWaveSpiritEnabled ? "true" : "false");
        }

        var result = await _themeManager.ApplyAsync(selection);
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

    private IReadOnlyList<DesignLanguageInfo> Languages => _themeManager?.Languages ?? Array.Empty<DesignLanguageInfo>();

    private static string Key(string language, DesignStyle? style) =>
        style is { } value && value.Id != "Default" ? language + "." + value.Id : language;

    private bool TryParseKey(string key, out string language, out DesignStyle? style)
    {
        foreach (var info in Languages)
        {
            if (string.Equals(info.Id, key, StringComparison.Ordinal))
            {
                language = info.Id;
                style = null;
                return true;
            }
            foreach (var candidate in info.Styles)
            {
                if (string.Equals(info.Id + "." + candidate.Id, key, StringComparison.Ordinal))
                {
                    language = info.Id;
                    style = candidate;
                    return true;
                }
            }
        }

        language = "";
        style = null;
        return false;
    }

    private bool IsAdjustable(string seed) =>
        _currentLanguage is { } language &&
        Languages.FirstOrDefault(info => info.Id == language)?.AdjustableSeeds.Contains(seed) == true &&
        FindTheme(CurrentThemeId)?.LockedSeeds.Contains(seed) != true;

    private bool IsLanguageDefault(string themeId) => FindTheme(themeId)?.IsLanguageDefault == true;

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
            await ApplyThemeSettingsAsync(ThemeTransitionReason.FollowSystem).ConfigureAwait(true);
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

        AvailableThemes = CaptureThemes(args.Themes);
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
            _isCompact = state.Variant.Density == DesignDensity.Compact;
            _currentLanguage = state.Language;
            _currentStyle = state.Variant.Style;
            RaiseDesignLanguageChanged();
        }

        // Read the applied snapshot, not the request config: a declaration that was not overridden (Shadcn's
        // EnableWaveSpirit=false) lives in the definition, so only the snapshot tells what actually rendered.
        ThemeSnapshot? snapshot = null;
        if (_themeManager is ThemeManager concreteManager)
        {
            snapshot = concreteManager.CurrentSnapshot;
        }
        if (snapshot is not null &&
            snapshot.Registry.TryGetGlobalToken(nameof(SharedTokenKind.EnableMotion), out var motionToken))
        {
            _isMotionEnabled = snapshot.GlobalTokenValues.Get<bool>(motionToken.Slot);
        }
        else
        {
            _isMotionEnabled = ReadBooleanToken(config, nameof(SharedTokenKind.EnableMotion), true);
        }
        if (snapshot is not null &&
            snapshot.Registry.TryGetGlobalToken(nameof(SharedTokenKind.EnableWaveSpirit), out var waveToken))
        {
            _isWaveSpiritEnabled = _isMotionEnabled && snapshot.GlobalTokenValues.Get<bool>(waveToken.Slot);
        }
        else
        {
            _isWaveSpiritEnabled = _isMotionEnabled &&
                                   ReadBooleanToken(config, nameof(SharedTokenKind.EnableWaveSpirit), true);
        }
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
        this.RaisePropertyChanged(nameof(CanChooseNeutralColor));
        this.RaisePropertyChanged(nameof(NeutralColor));
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
