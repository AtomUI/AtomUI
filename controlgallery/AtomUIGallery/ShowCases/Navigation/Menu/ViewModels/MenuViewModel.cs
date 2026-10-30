using System.Reactive;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls;
using Avalonia.Interactivity;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Menu;

public class MenuViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public static EntityKey ID = "Menu";

    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    private List<IMenuItemData>? _menuItems;

    public List<IMenuItemData>? MenuItems
    {
        get => _menuItems;
        set => this.RaiseAndSetIfChanged(ref _menuItems, value);
    }

    private List<IMenuItemData>? _menuFlyoutItems;

    public List<IMenuItemData>? MenuFlyoutItems
    {
        get => _menuFlyoutItems;
        set => this.RaiseAndSetIfChanged(ref _menuFlyoutItems, value);
    }

    private List<IMenuItemData>? _contextMenuItems;

    public List<IMenuItemData>? ContextMenuItems
    {
        get => _contextMenuItems;
        set => this.RaiseAndSetIfChanged(ref _contextMenuItems, value);
    }

    private bool _isDark;

    public bool IsDark
    {
        get => _isDark;
        set => this.RaiseAndSetIfChanged(ref _isDark, value);
    }

    private bool _isPopupScrollEnabled = true;

    public bool IsPopupScrollEnabled
    {
        get => _isPopupScrollEnabled;
        set => this.RaiseAndSetIfChanged(ref _isPopupScrollEnabled, value);
    }

    private NavMenuMode _mode = NavMenuMode.Inline;

    public NavMenuMode Mode
    {
        get => _mode;
        set => this.RaiseAndSetIfChanged(ref _mode, value);
    }

    private IList<TreeNodePath>? _defaultOpenPaths;

    public IList<TreeNodePath>? DefaultOpenPaths
    {
        get => _defaultOpenPaths;
        set => this.RaiseAndSetIfChanged(ref _defaultOpenPaths, value);
    }

    private TreeNodePath? _defaultSelectedPath;

    public TreeNodePath? DefaultSelectedPath
    {
        get => _defaultSelectedPath;
        set => this.RaiseAndSetIfChanged(ref _defaultSelectedPath, value);
    }

    private IList<TreeNodePath>? _semanticPreviewOpenPaths;

    public IList<TreeNodePath>? SemanticPreviewOpenPaths
    {
        get => _semanticPreviewOpenPaths;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewOpenPaths, value);
    }

    private TreeNodePath? _semanticPreviewSelectedPath;

    public TreeNodePath? SemanticPreviewSelectedPath
    {
        get => _semanticPreviewSelectedPath;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewSelectedPath, value);
    }

    private bool _isInlineCollapsed;

    public bool IsInlineCollapsed
    {
        get => _isInlineCollapsed;
        set => this.RaiseAndSetIfChanged(ref _isInlineCollapsed, value);
    }

    private bool _isStructuredNavMenuCollapsed;

    public bool IsStructuredNavMenuCollapsed
    {
        get => _isStructuredNavMenuCollapsed;
        set => this.RaiseAndSetIfChanged(ref _isStructuredNavMenuCollapsed, value);
    }

    private IList<TreeNodePath>? _inlineCollapsedOpenPaths;

    public IList<TreeNodePath>? InlineCollapsedOpenPaths
    {
        get => _inlineCollapsedOpenPaths;
        set => this.RaiseAndSetIfChanged(ref _inlineCollapsedOpenPaths, value);
    }

    private TreeNodePath? _inlineCollapsedSelectedPath;

    public TreeNodePath? InlineCollapsedSelectedPath
    {
        get => _inlineCollapsedSelectedPath;
        set => this.RaiseAndSetIfChanged(ref _inlineCollapsedSelectedPath, value);
    }

    private INavMenuNode? _defaultSelectedNode;

    public INavMenuNode? DefaultSelectedNode
    {
        get => _defaultSelectedNode;
        set => this.RaiseAndSetIfChanged(ref _defaultSelectedNode, value);
    }

    private List<INavMenuNode>? _inlineNavMenuNodes;

    public List<INavMenuNode>? InlineNavMenuNodes
    {
        get => _inlineNavMenuNodes;
        set => this.RaiseAndSetIfChanged(ref _inlineNavMenuNodes, value);
    }

    private List<INavMenuNode>? _itemsSourceDemoNavMenuNodes;

    public List<INavMenuNode>? ItemsSourceDemoNavMenuNodes
    {
        get => _itemsSourceDemoNavMenuNodes;
        set => this.RaiseAndSetIfChanged(ref _itemsSourceDemoNavMenuNodes, value);
    }

    private string _lastCommandKey = "-";

    public string LastCommandKey
    {
        get => _lastCommandKey;
        private set => this.RaiseAndSetIfChanged(ref _lastCommandKey, value);
    }

    public ReactiveCommand<string, Unit> NavigateCommand { get; }

    public MenuViewModel(IScreen screen)
    {
        HostScreen      = screen;
        Activator       = new ViewModelActivator();
        NavigateCommand = ReactiveCommand.Create<string>(OnNavigate);
    }

    private void OnNavigate(string itemKey)
    {
        LastCommandKey = itemKey;
    }

    public void HandleChangeModeCheckChanged(object? sender, RoutedEventArgs? args)
    {
        if (sender is AtomUIToggleSwitch changeModeSwitch)
        {
            if (changeModeSwitch.IsChecked.HasValue)
            {
                Mode = changeModeSwitch.IsChecked.Value ? NavMenuMode.Vertical : NavMenuMode.Inline;
            }
            else
            {
                Mode = NavMenuMode.Inline;
            }
        }
    }

    public void HandleChangeStyleCheckChanged(object? sender, RoutedEventArgs? args)
    {
        if (sender is AtomUIToggleSwitch changeStyleSwitch)
        {
            IsDark = changeStyleSwitch.IsChecked.GetValueOrDefault();
        }
    }

    public void HandleToggleInlineCollapsedClick(object? sender, RoutedEventArgs? args)
    {
        IsInlineCollapsed = !IsInlineCollapsed;
    }

    public void HandleToggleStructuredNavMenuCollapsedClick(object? sender, RoutedEventArgs? args)
    {
        IsStructuredNavMenuCollapsed = !IsStructuredNavMenuCollapsed;
    }

}
