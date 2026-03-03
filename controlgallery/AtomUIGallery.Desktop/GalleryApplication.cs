using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Language;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;

namespace AtomUIGallery.Desktop;

public class GalleryApplication : BaseGalleryApplication
{
    private static readonly Uri AvaloniaEditBaseUri      = new("avares://AvaloniaEdit/");
    private static readonly Uri AvaloniaEditThemeStyleUri = new("avares://AvaloniaEdit/Themes/Simple/AvaloniaEdit.xaml");

    public override void Initialize()
    {
        base.Initialize();
        this.UseAtomUI(builder =>
        {
            builder.WithDefaultLanguageVariant(LanguageVariant.zh_CN);
            builder.WithDefaultTheme(IThemeManager.DEFAULT_THEME_ID);
            builder.UseAlibabaSansFont();
            builder.UseDesktopControls();
            builder.UseGalleryControls();
            builder.UseDesktopDataGrid();
            builder.UseDesktopColorPicker();
        });

        ConfigureAvaloniaEditResources();
        ConfigureAvaloniaEditStyles();
    }

    private void ConfigureAvaloniaEditResources()
    {
        Resources ??= new ResourceDictionary();
        EnsureResource("FontSizeNormal", 13d);
        EnsureResource("ControlContentThemeFontSize", 13d);
        EnsureResource("ContentControlThemeFontFamily", FontFamily.Default);
        EnsureResource("ThemeBackgroundBrush", Brushes.White);
        EnsureResource("ThemeForegroundColor", Colors.Black);
        EnsureResource("ThemeBorderMidBrush", new SolidColorBrush(Color.Parse("#FFB0B0B0")));
        EnsureResource("ThemeBorderThickness", new Thickness(1));
        EnsureResource("ThemeBackgroundColor", Colors.White);
        EnsureResource("ThemeBorderLowColor", Color.Parse("#FFD0D0D0"));
        EnsureResource("HighlightColor", Color.Parse("#FF1677FF"));
        EnsureResource("ToolTipBackground", new SolidColorBrush(Color.Parse("#FF2B2B2B")));
        EnsureResource("ToolTipForeground", Brushes.White);
        EnsureResource("ToolTipBorderBrush", new SolidColorBrush(Color.Parse("#FF565656")));
        EnsureResource("ToolTipBorderThemeThickness", new Thickness(1));
        EnsureResource("CompletionToolTipBackground", new SolidColorBrush(Color.Parse("#FF2B2B2B")));
        EnsureResource("CompletionToolTipForeground", Brushes.White);
        EnsureResource("CompletionToolTipBorderBrush", new SolidColorBrush(Color.Parse("#FF565656")));
        EnsureResource("CompletionToolTipBorderThickness", new Thickness(1));
        EnsureResource("OverloadViewerBackground", Brushes.White);
        EnsureResource("OverloadViewerForeground", Brushes.Black);
        EnsureResource("OverloadViewerBorderBrush", new SolidColorBrush(Color.Parse("#FFB0B0B0")));
        EnsureResource("SearchPanelBackgroundBrush", Brushes.White);
        EnsureResource("SearchPanelBorderBrush", new SolidColorBrush(Color.Parse("#FFB0B0B0")));
        EnsureResource("SearchPanelFontFamily", FontFamily.Default);
        EnsureResource("SearchPanelFontSize", 13d);
        EnsureResource("SystemAccentColor", Color.Parse("#FF1677FF"));
        EnsureResource("SystemBaseLowColor", Color.Parse("#FFF3F3F3"));
        EnsureResource("SystemChromeMediumColor", Color.Parse("#FFB0B0B0"));
        EnsureResource("TextAreaSelectionBrush", new SolidColorBrush(Color.Parse("#661677FF")));
    }

    private void ConfigureAvaloniaEditStyles()
    {
        foreach (var style in Styles)
        {
            if (style is StyleInclude styleInclude &&
                styleInclude.Source == AvaloniaEditThemeStyleUri)
            {
                return;
            }
        }

        Styles.Add(new StyleInclude(AvaloniaEditBaseUri) { Source = AvaloniaEditThemeStyleUri });
    }

    private void EnsureResource(string key, object value)
    {
        if (Resources is null)
        {
            return;
        }
        
        if (!Resources.ContainsKey(key))
        {
            Resources[key] = value;
        }
    }

    public GalleryApplication()
    {
        Name = "AtomUI Desktop Gallery";
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow       = CreateWorkspaceWindow();
                desktop.MainWindow.Title = Name;
                break;
            // case ISingleViewApplicationLifetime singleView:
            //     singleView.MainView = new MainView();
            //     break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
