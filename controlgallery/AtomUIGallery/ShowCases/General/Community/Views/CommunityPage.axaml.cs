using System.Reactive.Disposables;
using AtomUI;
using AtomUI.Theme;
using Avalonia;

namespace AtomUIGallery.ShowCases.Community;

public partial class CommunityPage : GalleryReactiveUserControl<CommunityViewModel>
{
    public const string LanguageId = nameof(CommunityPage);

    public static readonly StyledProperty<bool> IsDarkThemeModeProperty =
        AvaloniaProperty.Register<CommunityPage, bool>(nameof(IsDarkThemeMode));

    public bool IsDarkThemeMode
    {
        get => GetValue(IsDarkThemeModeProperty);
        set => SetValue(IsDarkThemeModeProperty, value);
    }

    public CommunityPage()
    {
        this.WhenActivated(disposables =>
        {
            var themeManager = Application.Current?.GetThemeManager();
            if (themeManager != null)
            {
                SyncThemeMode(themeManager.CurrentTheme);
                EventHandler<ThemeChangedEventArgs> handler = (_, args) => SyncThemeMode(args.State);
                themeManager.ThemeChanged += handler;
                disposables.Add(Disposable.Create(() => themeManager.ThemeChanged -= handler));
            }
        });
        InitializeComponent();
    }

    private void SyncThemeMode(ThemeState? state)
    {
        SetCurrentValue(IsDarkThemeModeProperty, state?.Appearance == ThemeAppearance.Dark);
    }
}
