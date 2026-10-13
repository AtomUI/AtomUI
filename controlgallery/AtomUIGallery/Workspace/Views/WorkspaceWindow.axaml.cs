using System.Reactive;
using System.ComponentModel;
using System.Windows.Input;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Toolkits.GalleryBase.Shell;
using AtomUIGallery.Workspace.ViewModels;
using Avalonia;
using AtomUI;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
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
    private const string ThemeColorGroupName = "ThemeColor";
    private GalleryShellView? _shellView;
    private DispatcherOperation? _deferredContentOperation;
    private readonly CancellationTokenSource _startupWaitingCancellation = new();
    private bool _closed;
    private Avalonia.Controls.TextBlock? _loadingPlaceholder;
    private MenuItem? _themeMenuItem;
    private readonly List<MenuItem> _themeMenuItems = new();
    private PropertyChangedEventHandler? _viewModelPropertyChangedHandler;

    public WorkspaceWindow()
    {
        // 分级启动：窗口只搭骨架（含加载占位），真实 shell 等框架就绪后构建。
        ViewModel = new WorkspaceWindowViewModel();
        InitializeComponent();
        AddLoadingPlaceholder();
        _ = WaitForStartupAsync();
    }

    private async Task WaitForStartupAsync()
    {
        var cancellation = _startupWaitingCancellation.Token;
        try
        {
            var application = Application.Current ?? throw new InvalidOperationException("Application is unavailable.");
            await application.GetStartup().Completion.WaitAsync(cancellation);
            cancellation.ThrowIfCancellationRequested();
            // 晚创建窗口读取相同终态；等待就绪后再调度 shell，取消覆盖排队和运行前窗口。
            _deferredContentOperation = Dispatcher.UIThread.InvokeAsync(BuildDeferredContent, DispatcherPriority.Background);
            await _deferredContentOperation;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (!_closed && _loadingPlaceholder is { } placeholder)
            {
                placeholder.Text = "Startup failed: " + exception.Message;
            }
        }
    }

    // 占位符只服务首帧到 shell 就绪之间的空窗；静态无动画，避免与后置构建抢渲染。
    private void AddLoadingPlaceholder()
    {
        _loadingPlaceholder = new Avalonia.Controls.TextBlock
        {
            Text = "...",
            FontSize = 18,
            Opacity = 0.45,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        ShellHost.Children.Add(_loadingPlaceholder);
    }

    private void BuildDeferredContent()
    {
        _deferredContentOperation = null;
        if (_closed || ViewModel is null || _shellView is not null)
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
        if (_loadingPlaceholder is { } placeholder)
        {
            ShellHost.Children.Remove(placeholder);
            _loadingPlaceholder = null;
        }
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
        _startupWaitingCancellation.Cancel();
        _startupWaitingCancellation.Dispose();
        _deferredContentOperation?.Abort();
        _deferredContentOperation = null;
        RemoveHandler(MenuItem.ClickEvent, HandleMenuItemClick);
        if (ViewModel is not null && _viewModelPropertyChangedHandler is not null)
        {
            ViewModel.PropertyChanged -= _viewModelPropertyChangedHandler;
            _viewModelPropertyChangedHandler = null;
        }
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

        _themeMenuItem = menu.Items
                             .OfType<MenuItem>()
                             .SelectMany(static item => item.Items.OfType<MenuItem>())
                             .FirstOrDefault(static item =>
                                 item.Tag is WindowMenuItemKind.ThemeCatalog);
        if (_themeMenuItem is null)
        {
            return;
        }

        RebuildThemeMenuItems();
        _viewModelPropertyChangedHandler = HandleViewModelPropertyChanged;
        ViewModel.PropertyChanged += _viewModelPropertyChangedHandler;
    }

    private void HandleViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(GalleryWorkspaceViewModel.AvailableThemes) or
            nameof(GalleryWorkspaceViewModel.CurrentThemeId))
        {
            RebuildThemeMenuItems();
        }
    }

    private void RebuildThemeMenuItems()
    {
        if (ViewModel is null || _themeMenuItem is null)
        {
            return;
        }

        foreach (var existing in _themeMenuItems)
        {
            _themeMenuItem.Items.Remove(existing);
        }
        _themeMenuItems.Clear();

        var insertIndex = 0;
        var switchThemeCommand = new StableCommand(ViewModel.SwitchThemeCommand);
        foreach (var theme in ViewModel.AvailableThemes)
        {
            var item = new MenuItem
            {
                Header           = CreateThemeMenuHeader(theme),
                ToggleType       = MenuItemToggleType.Radio,
                GroupName        = ThemeColorGroupName,
                IsChecked        = string.Equals(theme.Id, ViewModel.CurrentThemeId, StringComparison.Ordinal),
                Command          = switchThemeCommand,
                CommandParameter = theme.Id
            };
            _themeMenuItem.Items.Insert(insertIndex++, item);
            _themeMenuItems.Add(item);
        }
    }

    private static Control CreateThemeMenuHeader(ThemeInfo theme)
    {
        var header = new Grid
        {
            Width             = 160,
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        header.Children.Add(new Avalonia.Controls.TextBlock
        {
            Text              = theme.Name,
            VerticalAlignment = VerticalAlignment.Center
        });

        if (theme.AccentColor is { } accentColor)
        {
            var swatch = new Border
            {
                Width               = 12,
                Height              = 12,
                CornerRadius        = new CornerRadius(2),
                Background          = new SolidColorBrush(accentColor),
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

    private sealed class StableCommand(ICommand innerCommand) : ICommand
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
