using System.ComponentModel;
using System.Windows.Input;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Toolkits.GalleryBase.Shell;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using MenuItem = AtomUI.Desktop.Controls.MenuItem;

namespace AtomUIGallery.Workspace.Views;

/// <summary>
/// Fills the Theme menu of the Gallery workspace window: design languages, brand color swatches and the themes
/// that declare no design language. The menu follows the view model and is rebuilt when it changes.
/// </summary>
internal sealed class GalleryThemeMenu : IDisposable
{
    private const string DesignLanguageGroupName = "ThemeDesignLanguage";
    private const string BrandColorGroupName = "ThemeBrandColor";
    private const string NeutralColorGroupName = "ThemeNeutralColor";
    private const string OtherThemeGroupName = "ThemeColor";

    // Design language identifiers are matched exactly; an unknown language falls back to the base theme's name.
    private static readonly Dictionary<string, WorkspaceWindowLangResourceKind> DesignLanguageNames = new(StringComparer.Ordinal)
    {
        ["AntDesign"]      = WorkspaceWindowLangResourceKind.DesignLanguageAntDesign,
        ["PrimeNG.Aura"]   = WorkspaceWindowLangResourceKind.DesignLanguagePrimeNGAura,
        ["PrimeNG.Lara"]   = WorkspaceWindowLangResourceKind.DesignLanguagePrimeNGLara,
        ["PrimeNG.Nora"]   = WorkspaceWindowLangResourceKind.DesignLanguagePrimeNGNora,
        ["Shadcn.Vega"]    = WorkspaceWindowLangResourceKind.DesignLanguageShadcnVega,
        ["Shadcn.Nova"]    = WorkspaceWindowLangResourceKind.DesignLanguageShadcnNova,
        ["Shadcn.Maia"]    = WorkspaceWindowLangResourceKind.DesignLanguageShadcnMaia,
        ["Shadcn.Lyra"]    = WorkspaceWindowLangResourceKind.DesignLanguageShadcnLyra,
        ["Shadcn.Mira"]    = WorkspaceWindowLangResourceKind.DesignLanguageShadcnMira,
        ["Shadcn.Luma"]    = WorkspaceWindowLangResourceKind.DesignLanguageShadcnLuma,
        ["Shadcn.Sera"]    = WorkspaceWindowLangResourceKind.DesignLanguageShadcnSera,
        ["Shadcn.Rhea"]    = WorkspaceWindowLangResourceKind.DesignLanguageShadcnRhea,
    };

    private readonly GalleryWorkspaceViewModel _viewModel;
    private readonly MenuItem _designLanguageItem;
    private readonly MenuItem _brandColorItem;
    private readonly MenuItem _neutralColorItem;
    private readonly MenuItem _otherThemesItem;
    private readonly StableCommand _switchThemeCommand;
    private readonly StableCommand _switchDesignLanguageCommand;
    private readonly StableCommand _setBrandColorCommand;
    private readonly StableCommand _setNeutralColorCommand;
    private readonly PropertyChangedEventHandler _propertyChangedHandler;
    private bool _disposed;
    private bool _rebuildPending;

    /// <summary>
    /// Gets or sets the number of completed rebuilds; used by tests to observe coalescing.
    /// </summary>
    internal int RebuildCount { get; set; }

    public GalleryThemeMenu(GalleryWorkspaceViewModel viewModel,
                            MenuItem designLanguageItem,
                            MenuItem brandColorItem,
                            MenuItem neutralColorItem,
                            MenuItem otherThemesItem)
    {
        _viewModel                   = viewModel;
        _designLanguageItem          = designLanguageItem;
        _brandColorItem              = brandColorItem;
        _neutralColorItem            = neutralColorItem;
        _otherThemesItem             = otherThemesItem;
        _switchThemeCommand          = new StableCommand(viewModel.SwitchThemeCommand);
        _switchDesignLanguageCommand = new StableCommand(viewModel.SwitchDesignLanguageCommand);
        _setBrandColorCommand        = new StableCommand(viewModel.SetBrandColorCommand);
        _setNeutralColorCommand      = new StableCommand(viewModel.SetNeutralColorCommand);
        Rebuild();
        _propertyChangedHandler = HandleViewModelPropertyChanged;
        viewModel.PropertyChanged += _propertyChangedHandler;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _viewModel.PropertyChanged -= _propertyChangedHandler;
    }

    private void HandleViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(GalleryWorkspaceViewModel.AvailableThemes) or
            nameof(GalleryWorkspaceViewModel.CurrentThemeId) or
            nameof(GalleryWorkspaceViewModel.CurrentDesignLanguage) or
            nameof(GalleryWorkspaceViewModel.CanChooseBrandColor) or
            nameof(GalleryWorkspaceViewModel.BrandColor))
        {
            ScheduleRebuild();
        }
    }

    // Several view model properties change together; one rebuild per dispatcher turn avoids repeated work and
    // avoids clearing a menu item while its own click is still being handled.
    private void ScheduleRebuild()
    {
        if (_disposed || _rebuildPending)
        {
            return;
        }

        _rebuildPending = true;
        Dispatcher.UIThread.Post(() =>
        {
            _rebuildPending = false;
            if (!_disposed)
            {
                Rebuild();
            }
        });
    }

    private void Rebuild()
    {
        RebuildCount++;
        var themes = _viewModel.AvailableThemes;
        RebuildDesignLanguages(themes);
        RebuildBrandColors(themes);
        RebuildNeutralColors(themes);
        RebuildOtherThemes(themes);
    }

    private void RebuildDesignLanguages(IReadOnlyList<ThemeInfo> themes)
    {
        _designLanguageItem.Items.Clear();
        var languages = _viewModel.DesignLanguages.ToList();

        // Languages sharing the family prefix (Shadcn.*, PrimeNG.*) fold into one submenu; single-variant
        // languages stay top-level. Grouping is data-driven off the prefix, so a future style package
        // joins its family without further menu changes.
        var families = new List<(string Family, List<string> Languages)>();
        var single   = new List<string>();
        foreach (var language in languages)
        {
            var separator = language.IndexOf('.');
            var family    = separator < 0 ? language : language[..separator];
            var existing  = families.FirstOrDefault(entry => entry.Family == family);
            if (separator < 0)
            {
                single.Add(language);
            }
            else if (existing.Languages is null)
            {
                families.Add((family, [language]));
            }
            else
            {
                existing.Languages.Add(language);
            }
        }

        foreach (var (family, languages2) in families.Where(entry => entry.Languages.Count > 1))
        {
            single.RemoveAll(language => languages2.Contains(language));
        }

        foreach (var language in single.Concat(families.Where(entry => entry.Languages.Count == 1)
                                                      .SelectMany(entry => entry.Languages)))
        {
            _designLanguageItem.Items.Add(CreateLanguageItem(themes, language));
        }

        foreach (var (family, languages2) in families.Where(entry => entry.Languages.Count > 1))
        {
            var parent = new MenuItem { Header = family, Tag = family };
            foreach (var language in languages2)
            {
                parent.Items.Add(CreateLanguageItem(themes, language));
            }
            // Emphasize the family while one of its children is the current language (MenuItem has no
            // highlight member; the radio state itself lives on the child items).
            if (languages2.Contains(_viewModel.CurrentDesignLanguage!))
            {
                parent.FontWeight = Avalonia.Media.FontWeight.Bold;
            }
            _designLanguageItem.Items.Add(parent);
        }
    }

    private MenuItem CreateLanguageItem(IReadOnlyList<ThemeInfo> themes, string language)
    {
        var baseTheme = FindBaseTheme(themes, language);
        var item = new MenuItem
        {
            ToggleType       = MenuItemToggleType.Radio,
            GroupName        = DesignLanguageGroupName,
            IsChecked        = string.Equals(language, _viewModel.CurrentDesignLanguage, StringComparison.Ordinal),
            Command          = _switchDesignLanguageCommand,
            CommandParameter = language,
            Tag              = language
        };
        item.Header = CreateHeader(baseTheme?.AccentColor, header => BindLanguageName(header, language, baseTheme));
        // The parent of a folded family highlights while one of its children is the current language.
        return item;
    }

    private void RebuildBrandColors(IReadOnlyList<ThemeInfo> themes)
    {
        _brandColorItem.Items.Clear();
        var canChoose = _viewModel.CanChooseBrandColor;
        _brandColorItem.IsEnabled = canChoose;
        if (!canChoose || _viewModel.CurrentDesignLanguage is not { } language)
        {
            // A disabled parent cannot open, so the hint is carried by its tooltip. A theme without a design language
            // has no language whose brand color could be fixed, so it gets its own hint.
            AtomUI.Desktop.Controls.ToolTip.SetShowOnDisabled(_brandColorItem, true);
            var tip = new Avalonia.Controls.TextBlock();
            tip.Bind(Avalonia.Controls.TextBlock.TextProperty,
                new DynamicResourceExtension(_viewModel.CurrentDesignLanguage is null
                    ? WorkspaceWindowLangResourceKind.MenuItemBrandColorNoLanguageHint
                    : WorkspaceWindowLangResourceKind.MenuItemBrandColorFixedHint));
            AtomUI.Desktop.Controls.ToolTip.SetTip(_brandColorItem, tip);
            return;
        }

        AtomUI.Desktop.Controls.ToolTip.SetTip(_brandColorItem, null);
        var baseTheme = FindBaseTheme(themes, language);
        var selected  = _viewModel.BrandColor ?? baseTheme?.AccentColor;
        foreach (var swatch in GalleryBrandSwatches.For(language, baseTheme?.Name ?? language, baseTheme?.AccentColor))
        {
            var item = new MenuItem
            {
                ToggleType       = MenuItemToggleType.Radio,
                GroupName        = BrandColorGroupName,
                IsChecked        = selected == swatch.Color,
                Command          = _setBrandColorCommand,
                CommandParameter = swatch.Color,
                Tag              = swatch.Color
            };
            item.Header = CreateHeader(swatch.Color, header => header.Text = swatch.Name);
            _brandColorItem.Items.Add(item);
        }

        _brandColorItem.Items.Add(new MenuSeparator());
        var reset = new MenuItem
        {
            Command          = _setBrandColorCommand,
            CommandParameter = null,
            IsEnabled        = _viewModel.BrandColor is not null
        };
        reset.Bind(MenuItem.HeaderProperty,
            new DynamicResourceExtension(WorkspaceWindowLangResourceKind.MenuItemBrandColorReset));
        _brandColorItem.Items.Add(reset);
    }

    private void RebuildNeutralColors(IReadOnlyList<ThemeInfo> themes)
    {
        _neutralColorItem.Items.Clear();
        var canChoose = _viewModel.CanChooseNeutralColor;
        _neutralColorItem.IsEnabled = canChoose;
        if (!canChoose || _viewModel.CurrentDesignLanguage is not { } language)
        {
            AtomUI.Desktop.Controls.ToolTip.SetShowOnDisabled(_neutralColorItem, true);
            var tip = new Avalonia.Controls.TextBlock();
            tip.Bind(Avalonia.Controls.TextBlock.TextProperty,
                new DynamicResourceExtension(_viewModel.CurrentDesignLanguage is null
                    ? WorkspaceWindowLangResourceKind.MenuItemNeutralColorNoLanguageHint
                    : WorkspaceWindowLangResourceKind.MenuItemNeutralColorFixedHint));
            AtomUI.Desktop.Controls.ToolTip.SetTip(_neutralColorItem, tip);
            return;
        }

        AtomUI.Desktop.Controls.ToolTip.SetTip(_neutralColorItem, null);
        var baseTheme = FindBaseTheme(themes, language);
        var selected  = _viewModel.NeutralColor;
        foreach (var swatch in GalleryNeutralSwatches.For(language, null))
        {
            var item = new MenuItem
            {
                ToggleType       = MenuItemToggleType.Radio,
                GroupName        = NeutralColorGroupName,
                IsChecked        = selected == swatch.Color,
                Command          = _setNeutralColorCommand,
                CommandParameter = swatch.Color,
                Tag              = swatch.Color
            };
            item.Header = CreateHeader(swatch.Color,
                header => header.Text = swatch.Name.Length > 0 ? swatch.Name : baseTheme?.Name ?? language);
            _neutralColorItem.Items.Add(item);
        }

        _neutralColorItem.Items.Add(new MenuSeparator());
        var reset = new MenuItem
        {
            Command          = _setNeutralColorCommand,
            CommandParameter = null,
            IsEnabled        = _viewModel.NeutralColor is not null
        };
        reset.Bind(MenuItem.HeaderProperty,
            new DynamicResourceExtension(WorkspaceWindowLangResourceKind.MenuItemNeutralColorReset));
        _neutralColorItem.Items.Add(reset);
    }

    private void RebuildOtherThemes(IReadOnlyList<ThemeInfo> themes)
    {
        _otherThemesItem.Items.Clear();
        foreach (var theme in themes)
        {
            // The language defaults are offered by the design language items; the named themes are listed here.
            if (theme.IsLanguageDefault)
            {
                continue;
            }

            var item = new MenuItem
            {
                ToggleType       = MenuItemToggleType.Radio,
                GroupName        = OtherThemeGroupName,
                IsChecked        = string.Equals(theme.Id, _viewModel.CurrentThemeId, StringComparison.Ordinal),
                Command          = _switchThemeCommand,
                CommandParameter = theme.Id,
                Tag              = theme.Id
            };
            item.Header = CreateHeader(theme.AccentColor, header => header.Text = theme.Name);
            _otherThemesItem.Items.Add(item);
        }

        _otherThemesItem.IsVisible = _otherThemesItem.Items.Count > 0;
    }

    // A design language key is the language identity, optionally followed by ".Style"; its base theme is the
    // language's implicit default entry.
    private static ThemeInfo? FindBaseTheme(IReadOnlyList<ThemeInfo> themes, string key)
    {
        var separator = key.IndexOf('.');
        var language  = separator < 0 ? key : key[..separator];
        var defaultId = ThemeInfo.LanguageDefaultId(language);
        return themes.FirstOrDefault(theme => theme.Id == defaultId) ??
               themes.FirstOrDefault(theme => string.Equals(theme.Language, language, StringComparison.Ordinal));
    }

    private static void BindLanguageName(Avalonia.Controls.TextBlock text, string language, ThemeInfo? baseTheme)
    {
        if (DesignLanguageNames.TryGetValue(language, out var kind))
        {
            text.Bind(Avalonia.Controls.TextBlock.TextProperty, new DynamicResourceExtension(kind));
        }
        else
        {
            text.Text = baseTheme?.Name ?? language;
        }
    }

    private static Grid CreateHeader(Color? color, Action<Avalonia.Controls.TextBlock> configureText)
    {
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        var text = new Avalonia.Controls.TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center
        };
        configureText(text);
        header.Children.Add(text);

        if (color is { } accent)
        {
            var swatch = new Border
            {
                Width               = 12,
                Height              = 12,
                CornerRadius        = new CornerRadius(2),
                Background          = new SolidColorBrush(accent),
                Margin              = new Thickness(12, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment   = VerticalAlignment.Center,
                IsHitTestVisible    = false
            };
            Grid.SetColumn(swatch, 1);
            header.Children.Add(swatch);
        }

        return header;
    }

    internal sealed class StableCommand(ICommand innerCommand) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter)
        {
            return innerCommand.CanExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            if (innerCommand.CanExecute(parameter))
            {
                innerCommand.Execute(parameter);
            }
        }
    }
}
