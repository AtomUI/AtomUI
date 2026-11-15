using System.ComponentModel;
using System.Windows.Input;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Toolkits.GalleryBase.Shell;
using AtomUIGallery.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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
    private const string OtherThemeGroupName = "ThemeColor";

    // Design language identifiers are matched exactly; an unknown language falls back to the base theme's name.
    private static readonly Dictionary<string, WorkspaceWindowLangResourceKind> DesignLanguageNames = new(StringComparer.Ordinal)
    {
        ["AntDesign"]      = WorkspaceWindowLangResourceKind.DesignLanguageAntDesign,
        ["Bootstrap"]      = WorkspaceWindowLangResourceKind.DesignLanguageBootstrap,
        ["Material"]       = WorkspaceWindowLangResourceKind.DesignLanguageMaterial,
        ["PrimeNG.Aura"]   = WorkspaceWindowLangResourceKind.DesignLanguagePrimeNGAura,
        ["PrimeNG.Lara"]   = WorkspaceWindowLangResourceKind.DesignLanguagePrimeNGLara,
        ["PrimeNG.Nora"]   = WorkspaceWindowLangResourceKind.DesignLanguagePrimeNGNora,
        ["Shadcn.Zinc"]    = WorkspaceWindowLangResourceKind.DesignLanguageShadcnZinc,
        ["Shadcn.Slate"]   = WorkspaceWindowLangResourceKind.DesignLanguageShadcnSlate,
        ["Shadcn.Stone"]   = WorkspaceWindowLangResourceKind.DesignLanguageShadcnStone,
    };

    private readonly GalleryWorkspaceViewModel _viewModel;
    private readonly MenuItem _designLanguageItem;
    private readonly MenuItem _brandColorItem;
    private readonly MenuItem _otherThemesItem;
    private readonly StableCommand _switchThemeCommand;
    private readonly StableCommand _switchDesignLanguageCommand;
    private readonly StableCommand _setBrandColorCommand;
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
                            MenuItem otherThemesItem)
    {
        _viewModel                   = viewModel;
        _designLanguageItem          = designLanguageItem;
        _brandColorItem              = brandColorItem;
        _otherThemesItem             = otherThemesItem;
        _switchThemeCommand          = new StableCommand(viewModel.SwitchThemeCommand);
        _switchDesignLanguageCommand = new StableCommand(viewModel.SwitchDesignLanguageCommand);
        _setBrandColorCommand        = new StableCommand(viewModel.SetBrandColorCommand);
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
        RebuildOtherThemes(themes);
    }

    private void RebuildDesignLanguages(IReadOnlyList<ThemeInfo> themes)
    {
        _designLanguageItem.Items.Clear();
        foreach (var language in ThemeSelectionResolver.DesignLanguages(themes))
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
            SetHeader(item, CreateHeader(baseTheme?.AccentColor, header => BindLanguageName(header, language, baseTheme)));
            _designLanguageItem.Items.Add(item);
        }
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
            SetHeader(item, CreateHeader(swatch.Color, header => header.Text = swatch.Name));
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

    private void RebuildOtherThemes(IReadOnlyList<ThemeInfo> themes)
    {
        _otherThemesItem.Items.Clear();
        foreach (var theme in themes)
        {
            if (theme.DesignLanguage is not null)
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
            SetHeader(item, CreateHeader(theme.AccentColor, header => header.Text = theme.Name));
            _otherThemesItem.Items.Add(item);
        }

        _otherThemesItem.IsVisible = _otherThemesItem.Items.Count > 0;
    }

    // The base definition of a language is its light, normal variant, matching ThemeSelectionResolver.
    private static ThemeInfo? FindBaseTheme(IReadOnlyList<ThemeInfo> themes, string language)
    {
        ThemeInfo? first = null;
        foreach (var theme in themes)
        {
            if (!string.Equals(theme.DesignLanguage, language, StringComparison.Ordinal))
            {
                continue;
            }

            first ??= theme;
            if (theme.Appearance == ThemeAppearance.Light && theme.Density == ThemeDensity.Normal)
            {
                return theme;
            }
        }

        return first;
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

    private static void SetHeader(MenuItem item, Control header)
    {
        item.Header = header;
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
