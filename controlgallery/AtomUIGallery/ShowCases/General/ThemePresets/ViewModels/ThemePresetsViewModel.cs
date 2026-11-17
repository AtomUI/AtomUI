using System.Reactive;
using System.Reactive.Subjects;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.ThemePresets;

public sealed record ThemePresetItem(string ThemeId, string Name, Color Accent, AntDesignIconKind? Icon)
{
    public bool HasIcon => Icon is not null;
    public IBrush AccentBrush => new SolidColorBrush(Accent);
    public Avalonia.Controls.PathIcon? Symbol => Icon is { } kind
        ? (Avalonia.Controls.PathIcon)new AntDesignIconProvider(kind).ProvideValue(null!) : null;
}

/// <summary>
/// One primary-colour swatch of the theme presets page: the seed colour, its localized name and whether the
/// preview currently uses it.
/// </summary>
public sealed class ThemePrimarySwatch : ReactiveObject
{
    private readonly ThemePresetsShowCaseLangResourceKind _nameKind;
    private readonly string _fallbackName;
    private string _name;
    private bool _isSelected;

    internal ThemePrimarySwatch(Color color, ThemePresetsShowCaseLangResourceKind nameKind, string fallbackName)
    {
        Color = color;
        Brush = new ImmutableSolidColorBrush(color);
        _nameKind = nameKind;
        _fallbackName = fallbackName;
        _name = GalleryLocalization.Get(nameKind, fallbackName);
    }

    public Color Color { get; }
    public IBrush Brush { get; }
    public string Name
    {
        get => _name;
        private set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    public bool IsSelected
    {
        get => _isSelected;
        internal set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    internal void RefreshName() => Name = GalleryLocalization.Get(_nameKind, _fallbackName);
}

public sealed class ThemePresetsViewModel : ReactiveObject, IRoutableViewModel, IDisposable
{
    public static EntityKey ID = "ThemePresets";
    // Tailwind 500 stops; a PrimeNG algorithm resolves each to its transcribed Tailwind scale.
    private static readonly (Color Color, ThemePresetsShowCaseLangResourceKind Name, string Fallback)[] s_swatches =
    [
        (Color.FromUInt32(0xFF10B981u), ThemePresetsShowCaseLangResourceKind.Emerald, "Emerald"),
        (Color.FromUInt32(0xFF3B82F6u), ThemePresetsShowCaseLangResourceKind.Blue, "Blue"),
        (Color.FromUInt32(0xFF8B5CF6u), ThemePresetsShowCaseLangResourceKind.Violet, "Violet"),
        (Color.FromUInt32(0xFFF43F5Eu), ThemePresetsShowCaseLangResourceKind.Rose, "Rose"),
        (Color.FromUInt32(0xFFF59E0Bu), ThemePresetsShowCaseLangResourceKind.Amber, "Amber"),
    ];
    // primeng.dev's default primary (primeng 22.1.5): slate-950, which a PrimeNG algorithm turns into the noir
    // primary (the surface scale). Offered first for the PrimeNG presets.
    private static readonly (Color Color, ThemePresetsShowCaseLangResourceKind Name, string Fallback) s_noirSwatch =
        (Color.FromUInt32(0xFF020617u), ThemePresetsShowCaseLangResourceKind.Noir, "Noir");
    // The seed a definition without its own ColorPrimary starts from.
    private static readonly Color s_defaultPrimary = new DesignToken().ColorPrimary;
    // The seventeen shadcn themes: each swatch writes the theme's light primary (a Tailwind 700 stop; lime and
    // yellow 400), so both appearances resolve to the official values. Neutral is the preset's own primary.
    private static readonly (Color Color, ThemePresetsShowCaseLangResourceKind Name, string Fallback)[] s_shadcnSwatches =
    [
        (Color.FromUInt32(0xFFBB4D00u), ThemePresetsShowCaseLangResourceKind.Amber, "Amber"),
        (Color.FromUInt32(0xFF1447E6u), ThemePresetsShowCaseLangResourceKind.Blue, "Blue"),
        (Color.FromUInt32(0xFF007595u), ThemePresetsShowCaseLangResourceKind.Cyan, "Cyan"),
        (Color.FromUInt32(0xFF007A55u), ThemePresetsShowCaseLangResourceKind.Emerald, "Emerald"),
        (Color.FromUInt32(0xFFA800B7u), ThemePresetsShowCaseLangResourceKind.Fuchsia, "Fuchsia"),
        (Color.FromUInt32(0xFF008236u), ThemePresetsShowCaseLangResourceKind.Green, "Green"),
        (Color.FromUInt32(0xFF432DD7u), ThemePresetsShowCaseLangResourceKind.Indigo, "Indigo"),
        (Color.FromUInt32(0xFF9AE600u), ThemePresetsShowCaseLangResourceKind.Lime, "Lime"),
        (Color.FromUInt32(0xFFCA3500u), ThemePresetsShowCaseLangResourceKind.Orange, "Orange"),
        (Color.FromUInt32(0xFFC6005Cu), ThemePresetsShowCaseLangResourceKind.Pink, "Pink"),
        (Color.FromUInt32(0xFF8200DBu), ThemePresetsShowCaseLangResourceKind.Purple, "Purple"),
        (Color.FromUInt32(0xFFC10007u), ThemePresetsShowCaseLangResourceKind.Red, "Red"),
        (Color.FromUInt32(0xFFC70036u), ThemePresetsShowCaseLangResourceKind.Rose, "Rose"),
        (Color.FromUInt32(0xFF0069A8u), ThemePresetsShowCaseLangResourceKind.Sky, "Sky"),
        (Color.FromUInt32(0xFF00786Fu), ThemePresetsShowCaseLangResourceKind.Teal, "Teal"),
        (Color.FromUInt32(0xFF7008E7u), ThemePresetsShowCaseLangResourceKind.Violet, "Violet"),
        (Color.FromUInt32(0xFFFDC700u), ThemePresetsShowCaseLangResourceKind.Yellow, "Yellow")
    ];
    // The seven shadcn base colors; a swatch writes the light --muted-foreground (the ColorNeutral seed).
    private static readonly (Color Color, ThemePresetsShowCaseLangResourceKind Name, string Fallback)[] s_neutralSwatches =
    [
        (Color.FromUInt32(0xFF737373u), ThemePresetsShowCaseLangResourceKind.Neutral, "Neutral"),
        (Color.FromUInt32(0xFF79716Bu), ThemePresetsShowCaseLangResourceKind.Stone, "Stone"),
        (Color.FromUInt32(0xFF71717Bu), ThemePresetsShowCaseLangResourceKind.Zinc, "Zinc"),
        (Color.FromUInt32(0xFF79697Bu), ThemePresetsShowCaseLangResourceKind.Mauve, "Mauve"),
        (Color.FromUInt32(0xFF7C7C67u), ThemePresetsShowCaseLangResourceKind.Olive, "Olive"),
        (Color.FromUInt32(0xFF67787Cu), ThemePresetsShowCaseLangResourceKind.Mist, "Mist"),
        (Color.FromUInt32(0xFF7C6D67u), ThemePresetsShowCaseLangResourceKind.Taupe, "Taupe")
    ];
    public IScreen HostScreen { get; }
    public string? UrlPathSegment => ID.ToString();
    private readonly IThemeManager _manager;
    private readonly ThemePresetCatalog _catalog;
    private IReadOnlyList<(ThemeInfo Info, string Package)> _themes = [];
    private IReadOnlyList<ThemePresetItem> _presets = [];
    private string? _selectedPresetId;
    private string? _previewThemeId;
    private bool _isDark;
    private bool _isCompact;
    private ThemeConfig? _previewConfig;
    private bool _active;
    private bool _copySucceeded;
    private bool _copyFailed;
    private DataGridLocalSource<PreviewRow>? _previewSource;
    private IReadOnlyList<Color> _primarySwatches = [];
    private IReadOnlyList<ThemePrimarySwatch> _primarySwatchItems = [];
    private IReadOnlyList<ThemePrimarySwatch> _neutralSwatchItems = [];
    private Color? _selectedNeutral;
    private Color? _presetNeutral;
    private Color _presetPrimary;
    private Color? _selectedPrimary;
    private bool _canChoosePrimary = true;
    private readonly BehaviorSubject<bool> _canResetPrimary = new(false);
    private ILanguageManager? _languageManager;

    public IReadOnlyList<AtomUI.Desktop.Controls.SelectOption> People { get; } =
        [new() { ItemKey = "ada", Header = "Ada" }, new() { ItemKey = "grace", Header = "Grace" }];
    public static DataGridFieldId NameField { get; } = new("name");
    public static DataGridFieldId RoleField { get; } = new("role");
    public IDataGridSource? PreviewSource => _previewSource;
    public IReadOnlyList<ThemePresetItem> Presets => _presets;
    public string? SelectedPresetId
    {
        get => _selectedPresetId;
        set
        {
            if (value == _selectedPresetId || !_presets.Any(item => item.ThemeId == value))
            {
                return;
            }
            this.RaiseAndSetIfChanged(ref _selectedPresetId, value);
            UpdatePreview();
        }
    }
    /// <summary>
    /// The primary seeds offered for the previewed definition: the emerald, blue, violet, rose and amber 500 stops,
    /// followed by that definition's own <c>ColorPrimary</c> seed when it is not one of them.
    /// </summary>
    public IReadOnlyList<Color> PrimarySwatches => _primarySwatches;
    /// <summary>
    /// The <see cref="PrimarySwatches"/> as named, selectable items for the swatch bar.
    /// </summary>
    public IReadOnlyList<ThemePrimarySwatch> PrimarySwatchItems => _primarySwatchItems;
    /// <summary>
    /// Whether the previewed definition declares <c>BrandColor="Seed"</c>, so the derived primary states follow
    /// a replaced seed; <c>false</c> for a definition with a fixed brand color.
    /// </summary>
    public bool CanChoosePrimary
    {
        get => _canChoosePrimary;
        private set => this.RaiseAndSetIfChanged(ref _canChoosePrimary, value);
    }
    /// <summary>
    /// The primary seed that replaces the previewed definition's own; <c>null</c> keeps the own seed and its
    /// unchanged preview configuration. Selecting the own seed equals clearing, and nothing can be selected
    /// while <see cref="CanChoosePrimary"/> is <c>false</c>.
    /// </summary>
    public Color? SelectedPrimary
    {
        get => _selectedPrimary;
        set
        {
            if (!_canChoosePrimary || value == _presetPrimary)
            {
                value = null;
            }
            if (value == _selectedPrimary)
            {
                return;
            }
            this.RaiseAndSetIfChanged(ref _selectedPrimary, value);
            _canResetPrimary.OnNext(value is not null);
            UpdateSwatchSelection();
            UpdatePreview();
        }
    }
    public ReactiveCommand<Unit, Unit> ResetPrimary { get; }
    /// <summary>The neutral swatches of the previewed definition; empty unless it accepts a neutral seed.</summary>
    public IReadOnlyList<ThemePrimarySwatch> NeutralSwatchItems => _neutralSwatchItems;
    /// <summary>Whether the previewed definition declares <c>NeutralColor="Seed"</c>.</summary>
    public bool CanChooseNeutral => _neutralSwatchItems.Count > 0;
    /// <summary>
    /// The neutral seed that replaces the previewed definition's own; <c>null</c> keeps the own seed. Selecting
    /// the own seed equals clearing.
    /// </summary>
    public Color? SelectedNeutral
    {
        get => _selectedNeutral;
        set
        {
            if (value == _presetNeutral)
            {
                value = null;
            }
            if (value == _selectedNeutral)
            {
                return;
            }
            this.RaiseAndSetIfChanged(ref _selectedNeutral, value);
            UpdateNeutralSwatchSelection();
            UpdatePreview();
        }
    }
    public bool IsDark
    {
        get => _isDark;
        set { this.RaiseAndSetIfChanged(ref _isDark, value); UpdatePreview(); }
    }
    public bool IsCompact
    {
        get => _isCompact;
        set { this.RaiseAndSetIfChanged(ref _isCompact, value); UpdatePreview(); }
    }
    public string? PreviewThemeId => _previewThemeId;
    public ThemeConfig? PreviewConfig => _previewConfig;
    public bool CopySucceeded
    {
        get => _copySucceeded;
        private set => this.RaiseAndSetIfChanged(ref _copySucceeded, value);
    }
    public bool CopyFailed
    {
        get => _copyFailed;
        private set => this.RaiseAndSetIfChanged(ref _copyFailed, value);
    }
    public ReactiveCommand<Unit, Unit> CopyThemeXml { get; }
    internal Func<string, Task<bool>>? CopyText { get; set; }

    public ThemePresetsViewModel(IScreen screen) : this(screen, Application.Current!.GetThemeManager()!) { }

    internal ThemePresetsViewModel(IScreen screen, IThemeManager manager)
    {
        HostScreen = screen;
        _manager = manager;
        _catalog = new ThemePresetCatalog((ThemeManager)manager);
        CopyThemeXml = ReactiveCommand.CreateFromTask(CopyThemeXmlAsync, outputScheduler: AvaloniaScheduler.Instance);
        ResetPrimary = ReactiveCommand.Create(() => { SelectedPrimary = null; }, _canResetPrimary,
            AvaloniaScheduler.Instance);
        Refresh();
    }

    private async Task CopyThemeXmlAsync()
    {
        var config = _previewConfig;
        CopySucceeded = false;
        CopyFailed = false;
        var xml = ExportThemeXml();
        bool copied;
        try
        {
            copied = CopyText is { } copy && await copy(xml);
        }
        catch (Exception)
        {
            // Clipboard backend writes are a recoverable page operation, not an unhandled command error.
            copied = false;
        }
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (ReferenceEquals(config, _previewConfig))
            {
                CopySucceeded = copied;
                CopyFailed = !copied;
            }
        });
    }

    internal void Activate()
    {
        if (_active) { return; }
        _active = true;
        _previewSource = DataGridLocalSource.Create<PreviewRow>([new("Ada", "UI"), new("Grace", "API")],
            DataGridLocalSourceDescriptor.For<PreviewRow>(row => DataGridRowKey.FromString(row.Name))
                .Field(NameField, row => row.Name).Field(RoleField, row => row.Role));
        this.RaisePropertyChanged(nameof(PreviewSource));
        _manager.ThemeCatalogChanged += HandleCatalogChanged;
        _languageManager = GalleryLocalization.GetLanguageManager();
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged += HandleLanguageChanged;
        }
        Refresh();
        RefreshSwatchNames();
    }

    internal void Deactivate()
    {
        if (!_active) { return; }
        _active = false;
        _manager.ThemeCatalogChanged -= HandleCatalogChanged;
        if (_languageManager is not null)
        {
            _languageManager.LanguageChanged -= HandleLanguageChanged;
            _languageManager = null;
        }
        _previewSource?.Dispose();
        _previewSource = null;
        this.RaisePropertyChanged(nameof(PreviewSource));
        CopyText = null;
    }

    public void Dispose()
    {
        Deactivate();
        CopyThemeXml.Dispose();
        ResetPrimary.Dispose();
        _canResetPrimary.Dispose();
    }

    private void HandleCatalogChanged(object? sender, ThemeCatalogChangedEventArgs e) => Refresh();

    private void HandleLanguageChanged(object? sender, LanguageChangedEventArgs e) => RefreshSwatchNames();

    private void RefreshSwatchNames()
    {
        foreach (var swatch in _primarySwatchItems)
        {
            swatch.RefreshName();
        }
    }

    // The swatches belong to the previewed definition (the mode profile the preview compiles): its own seed and
    // whether it fixes the derived primary states can differ between light and dark profiles.
    private void UpdateSwatches(string previewId)
    {
        var previewInfo = _themes.Single(item => item.Info.Id == previewId).Info;
        var presetPrimary = previewInfo.AccentColor ?? s_defaultPrimary;
        CanChoosePrimary = previewInfo.BrandColor == ThemeBrandColorSupport.Seed;
        var shadcnLanguage = previewInfo.DesignLanguage?.StartsWith("Shadcn.", StringComparison.Ordinal) == true;
        if (_primarySwatchItems.Count == 0 || presetPrimary != _presetPrimary)
        {
            _presetPrimary = presetPrimary;
            var offered = previewInfo.DesignLanguage?.StartsWith("PrimeNG.", StringComparison.Ordinal) == true
                ? s_swatches.Prepend(s_noirSwatch)
                : shadcnLanguage
                    ? s_shadcnSwatches
                    : s_swatches;
            var items = offered.Select(swatch => new ThemePrimarySwatch(swatch.Color, swatch.Name, swatch.Fallback)).ToList();
            if (!items.Any(item => item.Color == presetPrimary))
            {
                items.Add(new ThemePrimarySwatch(presetPrimary, ThemePresetsShowCaseLangResourceKind.PresetColor, "Preset color"));
            }
            _primarySwatchItems = items;
            _primarySwatches = items.Select(item => item.Color).ToArray();
            this.RaisePropertyChanged(nameof(PrimarySwatches));
            this.RaisePropertyChanged(nameof(PrimarySwatchItems));
        }
        // Only one of the five fixed swatches survives a profile change; a previous profile's own seed, the new
        // profile's own seed, or any selection on a profile with fixed primary states falls back to null.
        if (_selectedPrimary is { } selected &&
            (!_canChoosePrimary || selected == presetPrimary ||
             !(shadcnLanguage ? s_shadcnSwatches : s_swatches).Any(swatch => swatch.Color == selected)))
        {
            _selectedPrimary = null;
            _canResetPrimary.OnNext(false);
            this.RaisePropertyChanged(nameof(SelectedPrimary));
        }
        UpdateSwatchSelection();

        // The neutral row follows the previewed definition's declaration; the Shadcn base colors are the choices.
        if (previewInfo.NeutralColor == ThemeNeutralColorSupport.Seed)
        {
            _presetNeutral = previewInfo.NeutralAccentColor;
            var neutralItems = s_neutralSwatches.Select(swatch => new ThemePrimarySwatch(swatch.Color, swatch.Name, swatch.Fallback)).ToList();
            if (_presetNeutral is { } own && !neutralItems.Any(item => item.Color == own))
            {
                neutralItems.Add(new ThemePrimarySwatch(own, ThemePresetsShowCaseLangResourceKind.PresetColor, "Preset color"));
            }
            _neutralSwatchItems = neutralItems;
        }
        else
        {
            _presetNeutral  = null;
            _selectedNeutral = null;
            _neutralSwatchItems = [];
            this.RaisePropertyChanged(nameof(SelectedNeutral));
        }
        this.RaisePropertyChanged(nameof(NeutralSwatchItems));
        this.RaisePropertyChanged(nameof(CanChooseNeutral));
        UpdateNeutralSwatchSelection();
    }

    private void UpdateNeutralSwatchSelection()
    {
        var shown = _selectedNeutral ?? _presetNeutral;
        foreach (var swatch in _neutralSwatchItems)
        {
            swatch.IsSelected = swatch.Color == shown;
        }
    }

    private void UpdateSwatchSelection()
    {
        var shown = _selectedPrimary ?? _presetPrimary;
        foreach (var swatch in _primarySwatchItems)
        {
            swatch.IsSelected = swatch.Color == shown;
        }
    }

    private void Refresh()
    {
        _themes = _catalog.Read();
        _presets = _themes.Where(item => !IsProfile(item.Info, item.Package))
            .Select(item => new ThemePresetItem(item.Info.Id, item.Info.Name, item.Info.AccentColor ?? Colors.Transparent,
                IconFor(item.Info.Id))).ToArray();
        this.RaisePropertyChanged(nameof(Presets));
        if (!_presets.Any(item => item.ThemeId == _selectedPresetId))
        {
            _selectedPresetId = _presets.FirstOrDefault()?.ThemeId;
            this.RaisePropertyChanged(nameof(SelectedPresetId));
        }
        UpdatePreview();
    }

    // A declared design language groups its variants: the light, normal definition is the preset and every other
    // variant of the language is a profile. A definition without a language falls back to the id suffix convention.
    private bool IsProfile(ThemeInfo info, string package)
    {
        if (info.DesignLanguage is { } language)
        {
            return (info.Appearance != ThemeAppearance.Light || info.Density != ThemeDensity.Normal) &&
                   _themes.Any(item => item.Package == package && item.Info.DesignLanguage == language &&
                                       item.Info.Appearance == ThemeAppearance.Light &&
                                       item.Info.Density == ThemeDensity.Normal);
        }
        var id = info.Id;
        foreach (var suffix in new[] { ".DarkCompact", ".Compact", ".Dark" })
        {
            if (id.EndsWith(suffix, StringComparison.Ordinal) &&
                _themes.Any(item => item.Package == package && item.Info.DesignLanguage is null &&
                                    item.Info.Id == id[..^suffix.Length]))
            {
                return true;
            }
        }
        return false;
    }

    private void UpdatePreview()
    {
        if (_selectedPresetId is null) { return; }
        var family = _themes.Single(item => item.Info.Id == _selectedPresetId);
        string id;
        List<ThemeAlgorithm>? algorithms = null;
        Color? seed = null;
        if (family.Info.DesignLanguage is { } language)
        {
            // The resolver picks the exact (appearance, density) variant, or the base definition with the built-in
            // Dark/Compact algorithms appended when the language lacks that variant.
            var result = ThemeSelectionResolver.Resolve(_themes.Select(item => item.Info).ToArray(),
                new ThemeSelection(language, IsDark ? ThemeAppearance.Dark : ThemeAppearance.Light,
                    IsCompact ? ThemeDensity.Compact : ThemeDensity.Normal, _selectedPrimary, _selectedNeutral));
            id = result.ThemeId;
            algorithms = result.Algorithms?.ToList();
            seed = result.BrandSeed;
        }
        else
        {
            // No declared language: look up a mode profile by id suffix, else use the base theme with the
            // built-in Dark/Compact algorithms appended by the ThemeSelectionResolver rule.
            var suffix = IsDark && IsCompact ? ".DarkCompact" : IsDark ? ".Dark" : IsCompact ? ".Compact" : "";
            var profile = _themes.FirstOrDefault(item => item.Package == family.Package &&
                                                         item.Info.DesignLanguage is null &&
                                                         item.Info.Id == _selectedPresetId + suffix);
            id = profile.Info?.Id ?? _selectedPresetId;
            if (profile.Info is null)
            {
                algorithms = FallbackChain(family.Info.Algorithms, IsDark, IsCompact);
            }
            if (_selectedPrimary is { } selected && _themes.Single(item => item.Info.Id == id).Info.BrandColor ==
                ThemeBrandColorSupport.Seed)
            {
                seed = selected;
            }
        }
        UpdateSwatches(id);
        // UpdateSwatches may clear a selection the new profile cannot honor.
        if (seed is not null && _selectedPrimary is null) { seed = null; }
        this.RaiseAndSetIfChanged(ref _previewThemeId, id, nameof(PreviewThemeId));
        this.RaiseAndSetIfChanged(ref _previewConfig,
            _catalog.CreateConfig(id, algorithms, seed is { } primary ? FormatColor(primary) : null,
                _selectedNeutral is { } neutral ? FormatColor(neutral) : null),
            nameof(PreviewConfig));
        CopySucceeded = false;
        CopyFailed = false;
    }

    /// <summary>
    /// The chain of a fallback definition, by the <see cref="ThemeSelectionResolver"/> rule (shared with the Gallery
    /// workspace): the definition's own chain without the built-in <c>Dark</c> and <c>Compact</c> algorithms, then
    /// <c>Compact</c> when compact is asked for, then <c>Dark</c> when dark is asked for.
    /// </summary>
    internal static List<ThemeAlgorithm> FallbackChain(IReadOnlyList<ThemeAlgorithm> baseChain, bool dark, bool compact)
    {
        var chain = new List<ThemeAlgorithm>(baseChain.Count + 2);
        foreach (var algorithm in baseChain)
        {
            if (algorithm != ThemeAlgorithm.Dark && algorithm != ThemeAlgorithm.Compact)
            {
                chain.Add(algorithm);
            }
        }
        if (compact) { chain.Add(ThemeAlgorithm.Compact); }
        if (dark) { chain.Add(ThemeAlgorithm.Dark); }
        return chain;
    }

    public string ExportThemeXml()
    {
        var info = _themes.Single(item => item.Info.Id == _previewThemeId).Info;
        return ThemePresetCatalog.Export(_previewThemeId!, info.Name, _previewConfig!,
            IsDark ? ThemeAppearance.Dark : ThemeAppearance.Light, info.DesignLanguage, info.BrandColor,
            IsCompact ? ThemeDensity.Compact : ThemeDensity.Normal, info.NeutralColor);
    }

    private static string FormatColor(Color color) => color.A == 0xFF
        ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
        : $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static AntDesignIconKind? IconFor(string id) => id switch
    {
        _ when id.StartsWith("PrimeNG", StringComparison.Ordinal) => AntDesignIconKind.ThunderboltOutlined,
        _ when id.StartsWith("Shadcn", StringComparison.Ordinal) => AntDesignIconKind.AppstoreOutlined,
        _ => null
    };
}

public sealed record PreviewRow(string Name, string Role);
