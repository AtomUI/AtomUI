using System.Reactive;
using System.Reactive.Linq;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia.Interactivity;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class MenuViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Menu";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<IMenuItemData> _menuItems = [];
    
    public List<IMenuItemData> MenuItems
    {
        get => _menuItems;
        set => this.RaiseAndSetIfChanged(ref _menuItems, value);
    }
    
    private List<IMenuItemData> _menuFlyoutItems = [];
    
    public List<IMenuItemData> MenuFlyoutItems
    {
        get => _menuFlyoutItems;
        set => this.RaiseAndSetIfChanged(ref _menuFlyoutItems, value);
    }
        
    private List<IMenuItemData> _contextMenuItems = [];
    
    public List<IMenuItemData> ContextMenuItems
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
    
    private INavMenuNode? _defaultSelectedNode;
    
    public INavMenuNode? DefaultSelectedNode
    {
        get => _defaultSelectedNode;
        set => this.RaiseAndSetIfChanged(ref _defaultSelectedNode, value);
    }

    private List<INavMenuNode> _navMenuItems = [];
    
    public List<INavMenuNode> NavMenuItems
    {
        get => _navMenuItems;
        set => this.RaiseAndSetIfChanged(ref _navMenuItems, value);
    }

    public ReactiveCommand<string, Unit> NavigateCommand { get; set; }
    
    public MenuViewModel(IScreen screen)
    {
        HostScreen      = screen;
        NavigateCommand = ReactiveCommand.CreateFromTask<string>(OnNavigate, Observable.Return(true));
    }
    
    private async Task OnNavigate(string? itemKey)
    {
        await Task.Delay(100);
        Console.WriteLine(itemKey);
    }

    public void HandleChangeModeCheckChanged(object? sender, RoutedEventArgs? args)
    {
        if (sender is ToggleSwitch changeModeSwitch)
        {
            if (changeModeSwitch.IsChecked.HasValue)
            {
                if (changeModeSwitch.IsChecked.Value)
                {
                    Mode = NavMenuMode.Vertical;
                }
                else
                {
                    Mode = NavMenuMode.Inline;
                }
            }
            else
            {
                Mode = NavMenuMode.Inline;
            }
        }
    }

    public void HandleChangeStyleCheckChanged(object? sender, RoutedEventArgs? args)
    {
        if (sender is ToggleSwitch changeModeSwitch)
        {
            if (changeModeSwitch.IsChecked.HasValue)
            {
                IsDark = changeModeSwitch.IsChecked.Value;
            }
            else
            {
                IsDark = false;
            }
        }
    }
}