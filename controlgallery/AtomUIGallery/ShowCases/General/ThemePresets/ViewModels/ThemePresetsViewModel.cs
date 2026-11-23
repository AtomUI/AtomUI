using System.Reactive;
using System.Reactive.Subjects;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Resources;
using AtomUI.Toolkits.GalleryBase.Shell;
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
    // Tailwind 500 stops; PrimeNG resolves each to its transcribed Tailwind scale.
    private static readonly (Color Color, ThemePresetsShowCaseLangResourceKind Name, string Fallback)[] s_swatches =
    [
        (Color.FromUInt32(0xFF10B981u), ThemePresetsShowCaseLangResourceKind.Emerald, "Emerald"),
        (Color.FromUInt32(0xFF3B82F6u), ThemePresetsShowCaseLangResourceKind.Blue, "Blue"),
        (Color.FromUInt32(0xFF8B5CF6u), ThemePresetsShowCaseLangResourceKind.Violet, "Violet"),
        (Color.FromUInt32(0xFFF43F5Eu), ThemePresetsShowCaseLangResourceKind.Rose, "Rose"),
        (Color.FromUInt32(0xFFF59E0Bu), ThemePresetsShowCaseLangResourceKind.Amber, "Amber"),
    ];
    // PrimeNG's default primary is slate-950, which the language turns into the noir
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
    // The seven shadcn base colors; a swatch writes the light --muted-foreground (the shadcn language seed BaseColor).
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
    private IReadOnlyList<ThemePresetChoice> _choices = [];
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
    private string? _swatchLanguage;
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
            if (value == _selectedPresetId || !_choices.Any(item => item.SelectionId == value))
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
    /// Whether the language allows changing the primary seed and the previewed definition leaves it unlocked.
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
    /// <summary>Whether the previewed definition allows changing the language's base color.</summary>
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
        foreach (var swatch in _primarySwatchItems.Concat(_neutralSwatchItems))
        {
            swatch.RefreshName();
        }
    }

    private void UpdateSwatches(ThemeInfo previewInfo)
    {
        var defaults = _catalog.LanguageDefaults(previewInfo);
        var presetPrimary = previewInfo.AccentColor ?? defaults.AccentColor ?? s_defaultPrimary;
        var language = _catalog.Language(previewInfo);
        CanChoosePrimary = language?.AdjustableSeeds.Contains(nameof(SharedTokenKind.ColorPrimary)) == true &&
                           !previewInfo.LockedSeeds.Contains(nameof(SharedTokenKind.ColorPrimary));
        var shadcnLanguage = previewInfo.Language == "Shadcn";
        if (_swatchLanguage != previewInfo.Language || presetPrimary != _presetPrimary)
        {
            _presetPrimary = presetPrimary;
            _swatchLanguage = previewInfo.Language;
            var offered = previewInfo.Language == "PrimeNG"
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
        var neutralSeedName = GalleryWorkspaceViewModel.NeutralSeedName;
        if (language?.AdjustableSeeds.Contains(neutralSeedName) == true && !previewInfo.LockedSeeds.Contains(neutralSeedName))
        {
            var hasSeed = previewInfo.SeedValues.TryGetValue(neutralSeedName, out var seed) ||
                          defaults.SeedValues.TryGetValue(neutralSeedName, out seed);
            _presetNeutral = hasSeed && Color.TryParse(seed, out var color) ? color : null;
            if (_selectedNeutral is not null && _selectedNeutral == _presetNeutral)
            {
                _selectedNeutral = null;
                this.RaisePropertyChanged(nameof(SelectedNeutral));
            }
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
        _choices = _catalog.Read();
        _presets = _choices.Where(item => item.ShowInBar)
            .Select(item => new ThemePresetItem(item.SelectionId, item.Name, item.Info.AccentColor ?? Colors.Transparent,
                IconFor(item.PreviewId))).ToArray();
        this.RaisePropertyChanged(nameof(Presets));
        if (!_choices.Any(item => item.SelectionId == _selectedPresetId))
        {
            _selectedPresetId = _presets.FirstOrDefault()?.ThemeId;
            this.RaisePropertyChanged(nameof(SelectedPresetId));
        }
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_selectedPresetId is null)
        {
            return;
        }
        var choice = _choices.Single(item => item.SelectionId == _selectedPresetId);
        UpdateSwatches(choice.Info);
        this.RaiseAndSetIfChanged(ref _previewThemeId, choice.PreviewId, nameof(PreviewThemeId));
        this.RaiseAndSetIfChanged(ref _previewConfig,
            _catalog.CreateConfig(choice, IsDark, IsCompact,
                _selectedPrimary is { } primary ? FormatColor(primary) : null,
                _selectedNeutral is { } neutral ? FormatColor(neutral) : null),
            nameof(PreviewConfig));
        CopySucceeded = false;
        CopyFailed = false;
    }

    public string ExportThemeXml()
    {
        var choice = _choices.Single(item => item.SelectionId == _selectedPresetId);
        return _catalog.Export(choice, _previewConfig!);
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
