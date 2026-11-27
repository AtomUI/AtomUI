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

/// <summary>
/// The screen that hosts the Gallery routes and holds the Gallery-wide settings: appearance mode, density,
/// motion switches, theme, design language, brand and neutral colors, and language. The settings are changed
/// through commands and reported as bindable state. They are applied through the application's theme and
/// language managers, and changes those managers report are mirrored here. <see cref="Dispose" /> releases
/// the subscriptions and disposes <see cref="Navigation" />.
/// </summary>
/// <remarks>
/// Every command that changes the theme awaits the theme manager. It completes when the transition was applied
/// or was superseded by a newer request, and it faults with <see cref="ThemeLoadException" /> when the
/// transition failed.
/// </remarks>
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

    /// <summary>
    /// The router of this screen. Navigating to a Gallery route executes <c>NavigateAndReset</c> on it with the view
    /// model created for that route.
    /// </summary>
    public RoutingState Router { get; } = new();

    /// <summary>
    /// The navigation tree of the Gallery; it navigates <see cref="Router" /> to the routes of the configuration. It
    /// comes from the constructor's <c>navigationFactory</c>, or is created by default, and is disposed together with
    /// this view model.
    /// </summary>
    public GalleryNavigationViewModel Navigation { get; }

    /// <summary>
    /// Gets the command that switches the appearance to dark or light, as the parameter says. It does the same as
    /// <see cref="SetAppearanceModeCommand" /> with <see cref="ThemePreference.Dark" /> or
    /// <see cref="ThemePreference.Light" />.
    /// </summary>
    public ReactiveCommand<bool, Unit> ToggleDarkModeCommand => _toggleDarkModeCommand.Value;

    /// <summary>
    /// Gets the command that applies the appearance preference passed as the parameter. After
    /// <see cref="ThemePreference.System" />, the light or dark appearance follows the system appearance until
    /// another preference is applied. A value other than Light, Dark or System fails the command with
    /// <see cref="ArgumentOutOfRangeException" />.
    /// </summary>
    public ReactiveCommand<ThemePreference, Unit> SetAppearanceModeCommand => _setAppearanceModeCommand.Value;

    /// <summary>
    /// Gets the command that applies the compact or the default density, as the parameter says. The compact density
    /// is applied only when the current design language offers it.
    /// </summary>
    public ReactiveCommand<bool, Unit> ToggleCompactModeCommand => _toggleCompactModeCommand.Value;

    /// <summary>
    /// Gets the command that enables or disables motion animations, as the parameter says. Disabling motion also
    /// disables the wave spirit effect; enabling motion does not enable it again. From then on the switch overrides
    /// the motion declaration of every theme that is applied.
    /// </summary>
    public ReactiveCommand<bool, Unit> ToggleMotionCommand => _toggleMotionCommand.Value;

    /// <summary>
    /// Gets the command that enables or disables the wave spirit effect, as the parameter says. Enabling the effect
    /// also enables motion, which it depends on; disabling it leaves motion unchanged. From then on the switch
    /// overrides the wave spirit declaration of every theme that is applied.
    /// </summary>
    public ReactiveCommand<bool, Unit> ToggleWaveSpiritCommand => _toggleWaveSpiritCommand.Value;

    /// <summary>
    /// Gets the command that switches to the theme with the id passed as the parameter, one of
    /// <see cref="AvailableThemes" />. The current theme and an unknown id are ignored, while a null or blank id
    /// fails the command. A theme that declares its own appearance also sets <see cref="AppearanceMode" /> to it,
    /// unless the system preference already yields it.
    /// </summary>
    public ReactiveCommand<string, Unit> SwitchThemeCommand => _switchThemeCommand.Value;

    /// <summary>
    /// Gets the command that switches to a design language, keeping the appearance, density and the brand and
    /// neutral colors remembered for that language. The parameter is a key of <see cref="DesignLanguages" />; the
    /// current key and a key that matches no language are ignored, while a null or blank key fails the command.
    /// </summary>
    public ReactiveCommand<string, Unit> SwitchDesignLanguageCommand => _switchDesignLanguageCommand.Value;

    /// <summary>
    /// Gets the command that sets the brand color of the current design language. It does nothing when the language
    /// does not accept a brand color; null or a color equal to the theme's own accent color clears the choice.
    /// </summary>
    public ReactiveCommand<Color?, Unit> SetBrandColorCommand => _setBrandColorCommand.Value;

    /// <summary>
    /// Sets the neutral color of the current design language; null keeps the definition's own. It does nothing when
    /// the language does not accept a neutral color; a color equal to the theme's own neutral color clears the
    /// choice.
    /// </summary>
    public ReactiveCommand<Color?, Unit> SetNeutralColorCommand => _setNeutralColorCommand.Value;

    /// <summary>
    /// Gets the command that asks the language manager to switch the Gallery language to Simplified Chinese
    /// (<c>LanguageTags.ZhCN</c>).
    /// </summary>
    public ReactiveCommand<Unit, Unit> SwitchToZhCNCommand => _switchToZhCNCommand.Value;

    /// <summary>
    /// Gets the command that asks the language manager to switch the Gallery language to Traditional Chinese
    /// (<c>LanguageTags.ZhTW</c>).
    /// </summary>
    public ReactiveCommand<Unit, Unit> SwitchToZhTWCommand => _switchToZhTWCommand.Value;

    /// <summary>
    /// Gets the command that asks the language manager to switch the Gallery language to English (United States)
    /// (<c>LanguageTags.EnUS</c>).
    /// </summary>
    public ReactiveCommand<Unit, Unit> SwitchToEnUSCommand => _switchToEnUSCommand.Value;

    /// <summary>
    /// Gets the command that asks the language manager to switch the Gallery language to Portuguese (Brazil)
    /// (<c>LanguageTags.PtBR</c>).
    /// </summary>
    public ReactiveCommand<Unit, Unit> SwitchToPtBRCommand => _switchToPtBRCommand.Value;

    /// <summary>
    /// Gets the themes of the theme manager's catalog in catalog order, as a snapshot; it is empty when no theme
    /// manager is available. A catalog change replaces the list and raises a property change.
    /// </summary>
    public IReadOnlyList<ThemeInfo> AvailableThemes
    {
        get => _availableThemes;
        private set => this.RaiseAndSetIfChanged(ref _availableThemes, value);
    }

    /// <summary>
    /// Gets the id of the theme applied by the theme manager, or an empty string when no theme is applied. The value
    /// is taken from the theme manager at construction and follows its theme and catalog changes.
    /// </summary>
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

    /// <summary>
    /// The design language keys of the enabled languages, in catalog order.
    /// </summary>
    public IReadOnlyList<string> DesignLanguages => Languages.SelectMany(static language =>
        language.Styles.Count == 1
            ? [language.Id]
            : language.Styles.Select(style => language.Id + "." + style.Id)).ToArray();

    /// <summary>
    /// Gets whether the current design language accepts a replaceable brand color.
    /// </summary>
    public bool CanChooseBrandColor => IsAdjustable(nameof(SharedTokenKind.ColorPrimary));

    /// <summary>
    /// Gets the brand color remembered for the current design language, or <see langword="null" /> when the theme's own
    /// color is in use. The memory lives in this view model only.
    /// </summary>
    public Color? BrandColor =>
        CurrentDesignLanguage is { } language && CanChooseBrandColor
            ? _brandByLanguage.GetValueOrDefault(language)
            : null;

    /// <summary>
    /// Gets the light, dark or system preference that decides the appearance. Until an explicit preference is
    /// chosen it is Light or Dark, following the appearance of the applied theme. Switching theme with
    /// <see cref="SwitchThemeCommand" /> to a theme that declares its own appearance sets it to that appearance,
    /// even after an explicit choice, unless the system preference already yields it.
    /// </summary>
    public ThemePreference AppearanceMode
    {
        get => _appearanceMode;
        private set => SetAppearanceModeState(value);
    }

    /// <summary>
    /// The language seed a neutral color family choice writes: the base color of shadcn/ui (ui.shadcn.com/create
    /// Base Color). A language that declares it as an adjustable seed offers the neutral picker.
    /// </summary>
    public const string NeutralSeedName = "BaseColor";

    /// <summary>
    /// Gets whether the current design language accepts a replaceable neutral color family.
    /// </summary>
    public bool CanChooseNeutralColor => IsAdjustable(NeutralSeedName);

    /// <summary>
    /// Gets the neutral color remembered for the current design language, if it accepts one.
    /// </summary>
    public Color? NeutralColor =>
        CurrentDesignLanguage is { } neutralLanguage && CanChooseNeutralColor
            ? _neutralByLanguage.GetValueOrDefault(neutralLanguage)
            : null;

    /// <summary>
    /// Gets whether the light appearance preference is selected. It stays false while the system preference is
    /// selected, whatever the system appearance is.
    /// </summary>
    public bool IsLightAppearanceMode => AppearanceMode == ThemePreference.Light;

    /// <summary>
    /// Gets whether the dark appearance preference is selected. It stays false while the system preference is
    /// selected, whatever the system appearance is.
    /// </summary>
    public bool IsDarkAppearanceMode => AppearanceMode == ThemePreference.Dark;

    /// <summary>
    /// Gets whether the system appearance preference is selected. The light and the dark property are false then.
    /// </summary>
    public bool IsSystemAppearanceMode => AppearanceMode == ThemePreference.System;

    /// <summary>
    /// Gets whether the current language is Simplified Chinese (<c>LanguageTags.ZhCN</c>).
    /// The value is updated when the language manager reports a language change.
    /// </summary>
    public bool IsZhCN
    {
        get => _isZhCN;
        private set => this.RaiseAndSetIfChanged(ref _isZhCN, value);
    }

    /// <summary>
    /// Gets whether the current language is Traditional Chinese (<c>LanguageTags.ZhTW</c>).
    /// The value is updated when the language manager reports a language change.
    /// </summary>
    public bool IsZhTW
    {
        get => _isZhTW;
        private set => this.RaiseAndSetIfChanged(ref _isZhTW, value);
    }

    /// <summary>
    /// Gets whether the current language is English (United States) (<c>LanguageTags.EnUS</c>).
    /// The value is updated when the language manager reports a language change.
    /// </summary>
    public bool IsEnUS
    {
        get => _isEnUS;
        private set => this.RaiseAndSetIfChanged(ref _isEnUS, value);
    }

    /// <summary>
    /// Gets whether the current language is Portuguese (Brazil) (<c>LanguageTags.PtBR</c>).
    /// The value is updated when the language manager reports a language change.
    /// </summary>
    public bool IsPtBR
    {
        get => _isPtBR;
        private set => this.RaiseAndSetIfChanged(ref _isPtBR, value);
    }

    /// <summary>
    /// Creates the workspace view model for a built configuration. It reads the current theme and language from the
    /// application's theme and language managers, when there are any, subscribes to their changes, and owns the
    /// navigation view model that <paramref name="navigationFactory" /> returns.
    /// </summary>
    /// <param name="configuration">
    /// The built Gallery configuration; the navigation resolves its nodes against the routes it holds.
    /// </param>
    /// <param name="navigationFactory">
    /// Creates the navigation view model, receiving this view model as its host screen. When it is
    /// <see langword="null" />, a default <see cref="GalleryNavigationViewModel" /> is created.
    /// </param>
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

    /// <summary>
    /// Releases the subscription to the system appearance and to the theme and language manager events, and disposes
    /// <see cref="Navigation" />. The view that owns this view model calls it when it is disposed; further calls do
    /// nothing.
    /// </summary>
    /// <remarks>
    /// An override must call the base method, or the subscriptions and the navigation view model are not released.
    /// </remarks>
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
