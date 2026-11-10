using System.Reactive;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia;
using Avalonia.Media;
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

public sealed class ThemePresetsViewModel : ReactiveObject, IRoutableViewModel, IDisposable
{
    public static EntityKey ID = "ThemePresets";
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
    private DataGridLocalSource<PreviewRow>? _previewSource;

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
    public ReactiveCommand<Unit, Unit> CopyThemeXml { get; }
    internal Func<string, Task<bool>>? CopyText { get; set; }

    public ThemePresetsViewModel(IScreen screen) : this(screen, Application.Current!.GetThemeManager()!) { }

    internal ThemePresetsViewModel(IScreen screen, IThemeManager manager)
    {
        HostScreen = screen;
        _manager = manager;
        _catalog = new ThemePresetCatalog((ThemeManager)manager);
        CopyThemeXml = ReactiveCommand.CreateFromTask(async () =>
        {
            CopySucceeded = CopyText is { } copy && await copy(ExportThemeXml());
        }, outputScheduler: AvaloniaScheduler.Instance);
        Refresh();
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
        Refresh();
    }

    internal void Deactivate()
    {
        if (!_active) { return; }
        _active = false;
        _manager.ThemeCatalogChanged -= HandleCatalogChanged;
        _previewSource?.Dispose();
        _previewSource = null;
        this.RaisePropertyChanged(nameof(PreviewSource));
        CopyText = null;
    }

    public void Dispose()
    {
        Deactivate();
        CopyThemeXml.Dispose();
    }

    private void HandleCatalogChanged(object? sender, ThemeCatalogChangedEventArgs e) => Refresh();

    private void Refresh()
    {
        _themes = _catalog.Read();
        _presets = _themes.Where(item => !IsProfile(item.Info.Id, item.Package))
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

    private bool IsProfile(string id, string package)
    {
        foreach (var suffix in new[] { ".DarkCompact", ".Compact", ".Dark" })
        {
            if (id.EndsWith(suffix, StringComparison.Ordinal) &&
                _themes.Any(item => item.Package == package && item.Info.Id == id[..^suffix.Length]))
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
        var suffix = IsDark && IsCompact ? ".DarkCompact" : IsDark ? ".Dark" : IsCompact ? ".Compact" : "";
        var profile = _themes.FirstOrDefault(item => item.Package == family.Package && item.Info.Id == _selectedPresetId + suffix);
        var id = profile.Info?.Id ?? _selectedPresetId;
        var algorithms = new List<ThemeAlgorithm> { IsDark ? ThemeAlgorithm.Dark : ThemeAlgorithm.Default };
        if (IsCompact) { algorithms.Add(ThemeAlgorithm.Compact); }
        this.RaiseAndSetIfChanged(ref _previewThemeId, id, nameof(PreviewThemeId));
        this.RaiseAndSetIfChanged(ref _previewConfig, _catalog.CreateConfig(id, algorithms), nameof(PreviewConfig));
        CopySucceeded = false;
    }

    public string ExportThemeXml() => ThemePresetCatalog.Export(_previewThemeId!,
        _themes.Single(item => item.Info.Id == _previewThemeId).Info.Name, _previewConfig!);

    private static AntDesignIconKind? IconFor(string id) => id switch
    {
        _ when id.StartsWith("PrimeNG", StringComparison.Ordinal) => AntDesignIconKind.ThunderboltOutlined,
        _ when id.StartsWith("Shadcn", StringComparison.Ordinal) => AntDesignIconKind.AppstoreOutlined,
        _ when id.StartsWith("Bootstrap", StringComparison.Ordinal) => AntDesignIconKind.LayoutOutlined,
        _ when id.StartsWith("Material", StringComparison.Ordinal) => AntDesignIconKind.BulbOutlined,
        _ => null
    };
}

public sealed record PreviewRow(string Name, string Role);
