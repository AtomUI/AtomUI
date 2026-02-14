using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia.Input;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class 
    MenuShowCase : ReactiveUserControl<MenuViewModel>
{
    private NavMenuNode? _navMenuDefaultSelectedItem;
    
    public MenuShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is MenuViewModel viewModel)
            {
                ChangeModeSwitch.IsCheckedChanged  += viewModel.HandleChangeModeCheckChanged;
                ChangeStyleSwitch.IsCheckedChanged += viewModel.HandleChangeStyleCheckChanged;
                var defaultOpenPaths = new List<TreeNodePath>();
                defaultOpenPaths.Add(new TreeNodePath("/3/SubGroup2"));
                viewModel.DefaultOpenPaths    = defaultOpenPaths;
                viewModel.DefaultSelectedPath = new TreeNodePath("/3/SubGroup1/Option1");
                InitNavMenuTreeNodes(viewModel);
                InitMenuTreeNodes(viewModel);
                InitContextMenuItems(viewModel);
                InitMenuFlyoutMenuItems(viewModel);
            }
        });
        
        InitializeComponent();
    }

    private void InitContextMenuItems(MenuViewModel viewModel)
    {
        var nodes = new List<IMenuItemData>();
        nodes.Add(new MenuItemData()
        {
            Header       = "Cut",
            Icon         = new ScissorOutlined(),
            InputGesture = KeyGesture.Parse("Ctrl+X"),
        });
        nodes.Add(new MenuItemData()
        {
            Header       = "Copy",
            Icon         = new CopyOutlined(),
            InputGesture = KeyGesture.Parse("Ctrl+C"),
        });
        nodes.Add(new MenuItemData()
        {
            Header       = "Delete",
            Icon         = new CopyOutlined(),
            InputGesture = KeyGesture.Parse("Ctrl+D"),
        });
        nodes.Add(new MenuItemData() {
                Header    = "Paste",
                Children = [
                    new MenuItemData()
                    {
                        Header       = "Paste",
                        Icon         = new FileDoneOutlined(),
                        InputGesture = KeyGesture.Parse("Ctrl+P")
                    },
                    new MenuItemData()
                    {
                        Header       = "Paste from History",
                        InputGesture = KeyGesture.Parse("Ctrl+Shift+V")
                    }
                ]
            }
        );
        viewModel.ContextMenuItems = nodes;
    }

    private void InitMenuTreeNodes(MenuViewModel viewModel)
    {
        var nodes = new List<IMenuItemData>();
        nodes.Add(new MenuItemData()
        {
            Header  = "File",
            Children = [new MenuItemData()
            {
                Header       = "New Text File",
                InputGesture = KeyGesture.Parse("Ctrl+N")
            },
            new MenuItemData()
            {
                Header       = "New File",
                InputGesture = KeyGesture.Parse("Ctrl+Alt+N")
            },
            new MenuItemData()
            {
                Header       = "New Window",
                InputGesture = KeyGesture.Parse("Ctrl+Shift+N")
            }]
        });
        nodes.Add(new MenuItemData() {
                Header    = "Edit",
                Children = [
                    new MenuItemData()
                    {
                        Header       = "Undo",
                        InputGesture = KeyGesture.Parse("Ctrl+Shift+Z")
                    },
                    new MenuSeparatorData(),
                    new MenuItemData()
                    {
                        Header       = "Cut",
                        InputGesture = KeyGesture.Parse("Ctrl+X")
                    }
                ]
            }
        );
        nodes.Add(new MenuItemData() {
                Header = "Disabled Item",
                IsEnabled = false
            }
        );
        viewModel.MenuItems = nodes;
    }

    private void InitNavMenuTreeNodes(MenuViewModel viewModel)
    {
        _navMenuDefaultSelectedItem = new NavMenuNode()
        {
            Header  = "Option 4",
            ItemKey = "Option4",
            Icon = new TwitterOutlined()
        };
        var nodes = new List<INavMenuNode>();
        nodes.Add(new NavMenuNode()
        {
            Header  = "Navigation One",
            Icon    = new MailOutlined(),
            ItemKey = "1"
        });
        nodes.Add(new NavMenuNode()
        {
            Header  = "Navigation Two",
            Icon    = new AppstoreOutlined(),
            ItemKey = "2"
        });
        nodes.Add(new NavMenuNode()
        {
            Header  = "Navigation Three - Submenu",
            Icon    = new SettingOutlined(),
            ItemKey = "3",
            Children = [new NavMenuNode()
            {
                Header  = "Item 1",
                ItemKey = "SubGroup1",
                Children = [new NavMenuNode()
                {
                    Header  = "Option 1",
                    ItemKey = "Option1",
                }, new NavMenuNode()
                {
                    Header  = "Option 2",
                    ItemKey = "Option2",
                }]
            },new NavMenuNode()
            {
                Header  = "Item 2",
                ItemKey = "SubGroup2",
                Children = [new NavMenuNode()
                    {
                        Header  = "Option 3",
                        ItemKey = "Option3",
                    }, 
                    _navMenuDefaultSelectedItem
                ]
            }]
        });
        nodes.Add(new NavMenuNode()
        {
            Header  = "Navigation Four",
            ItemKey = "4"
        });
        viewModel.NavMenuItems        = nodes;
        viewModel.DefaultSelectedNode = _navMenuDefaultSelectedItem;
        ItemsSourceDemoNavMenu.SelectedItem = _navMenuDefaultSelectedItem;
    }

    private void InitMenuFlyoutMenuItems(MenuViewModel viewModel)
    {

        var nodes = new List<IMenuItemData>();
        nodes.Add(new MenuItemData()
        {
            Header       = "Cut",
            InputGesture = KeyGesture.Parse("Ctrl+X"),
            Icon = new ScissorOutlined(),
        });
        nodes.Add(new MenuItemData() {
                Header       = "Copy",
                InputGesture = KeyGesture.Parse("Ctrl+C"),
                Icon         = new CopyOutlined(),
            }
        );
        nodes.Add(new MenuItemData() {
                Header       = "Delete",
                InputGesture = KeyGesture.Parse("Ctrl+D"),
                Icon         = new DeleteOutlined(),
            }
        );
        
        nodes.Add(new MenuItemData() {
                Header    = "Paste",
                Children = [
                    new MenuItemData()
                    {
                        Header       = "Paste",
                        InputGesture = KeyGesture.Parse("Ctrl+P"),
                        Icon         = new FileDoneOutlined(),
                    },
                    new MenuSeparatorData(),
                    new MenuItemData()
                    {
                        Header       = "Paste from History",
                        InputGesture = KeyGesture.Parse("Ctrl+Shift+V"),
                    }
                ]
            }
        );
    
        viewModel.MenuFlyoutItems = nodes;
    }
}