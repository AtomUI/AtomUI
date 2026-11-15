using System.Reactive;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Toolkits.GalleryBase.Shell;
using AtomUIGallery.Workspace.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using DesktopMenu = AtomUI.Desktop.Controls.Menu;
using MenuItem = AtomUI.Desktop.Controls.MenuItem;

namespace AtomUIGallery.Workspace.Views;

internal enum WindowMenuItemKind
{
    FullScreen,
    Pin,
    Minimize,
    Maximize,
    Move,
    Resize,
    ThemeCatalog,
    BrandColor,
    OtherThemes,
    LightMode,
    DarkMode,
    FollowSystem,
    Compact,
    Motion,
    WaveSpirit,
    LanguageZhCN,
    LanguageZhTW,
    LanguageEnUS,
    LanguagePtBR,
}

public partial class WorkspaceWindow : ReactiveWindow<WorkspaceWindowViewModel>
{
    public const string LanguageId = nameof(WorkspaceWindow);
    private const string TitleBarMenuResourceKey = "WorkspaceTitleBarMenu";
    private GalleryShellView? _shellView;
    private bool _closed;
    private GalleryThemeMenu? _themeMenu;

    public WorkspaceWindow()
    {
        // Splash 在提交前显示，主窗体出现时内核已就绪：窗口直接构建完整 shell，不再有加载占位。
        ViewModel = new WorkspaceWindowViewModel();
        InitializeComponent();
        BuildContent();
    }

    private void BuildContent()
    {
        if (ViewModel is null || _shellView is not null)
        {
            return;
        }
        ConfigureThemeMenu();
        var showCaseNavigation = new CaseNavigation
        {
            Name      = "ShowCaseNavigation",
            ViewModel = ViewModel.CaseNavigation
        };
        _shellView = new GalleryShellView(
            AtomUIGalleryModule.GetConfiguration(),
            showCaseNavigation,
            ViewModel.Router);
        ShellHost.Children.Add(_shellView);
    }

    protected override WindowTitleBar? NotifyCreateTitleBar(WindowTitleBar? oldTitleBar)
    {
        return new GalleryWindowTitleBar
        {
            Name = "PART_TitleBar"
        };
    }

    protected override void NotifyConfigureTitleBar(WindowTitleBar titleBar)
    {
        base.NotifyConfigureTitleBar(titleBar);
        if (titleBar is GalleryWindowTitleBar galleryTitleBar &&
            Resources.TryGetValue(TitleBarMenuResourceKey, out var titleBarMenu))
        {
            galleryTitleBar.Menu = titleBarMenu as Control;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        AddHandler(MenuItem.ClickEvent, HandleMenuItemClick);
    }

    protected override void OnClosed(EventArgs e)
    {
        ReleaseWorkspace();
        base.OnClosed(e);
    }

    private void ReleaseWorkspace()
    {
        if (_closed)
        {
            return;
        }
        _closed = true;
        RemoveHandler(MenuItem.ClickEvent, HandleMenuItemClick);
        _themeMenu?.Dispose();
        _themeMenu = null;
        _shellView?.Dispose();
        _shellView = null;
        ViewModel?.Dispose();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseWorkspace();
        base.OnDetachedFromVisualTree(e);
    }

    private void ConfigureThemeMenu()
    {
        if (ViewModel is null ||
            !Resources.TryGetValue(TitleBarMenuResourceKey, out var menuResource) ||
            menuResource is not DesktopMenu menu)
        {
            return;
        }

        var themeItems = menu.Items
                             .OfType<MenuItem>()
                             .SelectMany(static item => item.Items.OfType<MenuItem>())
                             .Where(static item => item.Tag is WindowMenuItemKind.ThemeCatalog or
                                 WindowMenuItemKind.BrandColor or
                                 WindowMenuItemKind.OtherThemes)
                             .ToDictionary(static item => (WindowMenuItemKind)item.Tag!);
        if (!themeItems.TryGetValue(WindowMenuItemKind.ThemeCatalog, out var designLanguageItem) ||
            !themeItems.TryGetValue(WindowMenuItemKind.BrandColor, out var brandColorItem) ||
            !themeItems.TryGetValue(WindowMenuItemKind.OtherThemes, out var otherThemesItem))
        {
            return;
        }

        _themeMenu = new GalleryThemeMenu(ViewModel, designLanguageItem, brandColorItem, otherThemesItem);
    }

    private void HandleMenuItemClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (e.Source is MenuItem menuItem && menuItem.Tag is WindowMenuItemKind kind)
        {
            if (menuItem.ToggleType == MenuItemToggleType.None)
            {
                return;
            }

            switch (kind)
            {
                case WindowMenuItemKind.FullScreen:
                    IsFullScreenCaptionButtonVisible = menuItem.IsChecked;
                    break;
                case WindowMenuItemKind.Pin:
                    IsPinCaptionButtonVisible = menuItem.IsChecked;
                    break;
                case WindowMenuItemKind.Minimize:
                    IsMinimizeCaptionButtonVisible = menuItem.IsChecked;
                    break;
                case WindowMenuItemKind.Maximize:
                    IsMaximizeCaptionButtonVisible = menuItem.IsChecked;
                    break;
                case WindowMenuItemKind.Move:
                    IsMoveEnabled = menuItem.IsChecked;
                    break;
                case WindowMenuItemKind.Resize:
                    CanResize = menuItem.IsChecked;
                    break;
                case WindowMenuItemKind.LightMode:
                    ViewModel.SetAppearanceModeCommand.Execute(ThemePreference.Light)
                             .Subscribe();
                    break;
                case WindowMenuItemKind.DarkMode:
                    ViewModel.SetAppearanceModeCommand.Execute(ThemePreference.Dark)
                             .Subscribe();
                    break;
                case WindowMenuItemKind.FollowSystem:
                    ViewModel.SetAppearanceModeCommand.Execute(ThemePreference.System)
                             .Subscribe();
                    break;
                case WindowMenuItemKind.Compact:
                    ViewModel.ToggleCompactModeCommand.Execute(menuItem.IsChecked)
                             .Subscribe();
                    break;
                case WindowMenuItemKind.Motion:
                    if (!menuItem.IsChecked &&
                        FindSiblingMenuItem(menuItem, WindowMenuItemKind.WaveSpirit) is { } waveSpiritMenuItem)
                    {
                        waveSpiritMenuItem.IsChecked = false;
                    }
                    ViewModel.ToggleMotionCommand.Execute(menuItem.IsChecked)
                             .Subscribe();
                    break;
                case WindowMenuItemKind.WaveSpirit:
                    if (menuItem.IsChecked &&
                        FindSiblingMenuItem(menuItem, WindowMenuItemKind.Motion) is { } motionMenuItem)
                    {
                        motionMenuItem.IsChecked = true;
                    }
                    ViewModel.ToggleWaveSpiritCommand.Execute(menuItem.IsChecked)
                             .Subscribe();
                    break;
                case WindowMenuItemKind.LanguageZhCN:
                    ViewModel.SwitchToZhCNCommand.Execute(Unit.Default)
                             .Subscribe();
                    break;
                case WindowMenuItemKind.LanguageZhTW:
                    ViewModel.SwitchToZhTWCommand.Execute(Unit.Default)
                             .Subscribe();
                    break;
                case WindowMenuItemKind.LanguageEnUS:
                    ViewModel.SwitchToEnUSCommand.Execute(Unit.Default)
                             .Subscribe();
                    break;
                case WindowMenuItemKind.LanguagePtBR:
                    ViewModel.SwitchToPtBRCommand.Execute(Unit.Default)
                             .Subscribe();
                    break;
            }
        }
    }

    private static MenuItem? FindSiblingMenuItem(MenuItem menuItem, WindowMenuItemKind kind)
    {
        if (menuItem.Parent is not MenuItem parent)
        {
            return null;
        }

        foreach (var item in parent.Items)
        {
            if (item is MenuItem sibling &&
                sibling.Tag is WindowMenuItemKind siblingKind &&
                siblingKind == kind)
            {
                return sibling;
            }
        }

        return null;
    }
}
