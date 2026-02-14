using AtomUI.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class DefaultNavMenuInteractionHandler : INavMenuInteractionHandler
{
    private IDisposable? _inputManagerSubscription;
    private IRenderRoot? _root;
    private IDisposable? _currentOpenDelayRunDisposable;
    private IDisposable? _currentCloseDelayRunDisposable;
    private bool _currentPressedIsValid = false;
    private NavMenuItem? _latestSelectedItem = null;
    private NavMenuItem? _latestClickedItem = null;

    public DefaultNavMenuInteractionHandler()
        : this(AvaloniaLocator.Current.GetService<IInputManager>(), DefaultDelayRun)
    {
    }

    public DefaultNavMenuInteractionHandler(IInputManager? inputManager, Func<Action, TimeSpan, IDisposable> delayRun)
    {
        delayRun = delayRun ?? throw new ArgumentNullException(nameof(delayRun));

        InputManager = inputManager;
        DelayRun     = delayRun;
    }

    public void Attach(NavMenu navMenu) => AttachCore(navMenu);
    public void Detach(NavMenu navMenu) => DetachCore(navMenu);

    protected Func<Action, TimeSpan, IDisposable> DelayRun { get; }

    protected IInputManager? InputManager { get; }

    internal INavMenu? Menu { get; private set; }

    public static TimeSpan MenuShowDelay { get; set; } = TimeSpan.FromMilliseconds(400);

    protected virtual void GotFocus(object? sender, GotFocusEventArgs e)
    {
    }

    protected virtual void LostFocus(object? sender, RoutedEventArgs e)
    {
    }

    protected virtual void NotifyPointerEntered(object? sender, RoutedEventArgs e)
    {
        var menuItem = GetMenuItemCore(e.Source as Control) as INavMenuItem;
        if (menuItem?.Parent == null)
        {
            return;
        }

        _currentOpenDelayRunDisposable?.Dispose();
        _currentCloseDelayRunDisposable?.Dispose();
        if (menuItem.HasSubMenu)
        {
            OpenWithDelay(menuItem);
        }
        else if (menuItem.Parent != null)
        {
            foreach (var sibling in menuItem.Parent.SubItems)
            {
                if (sibling.IsSubMenuOpen)
                {
                    sibling.Close();
                }
            }
        }
    }

    protected virtual void NotifyPointerExited(object? sender, RoutedEventArgs e)
    {
        var menuItem = GetMenuItemCore(e.Source as Control) as INavMenuItem;

        if (menuItem?.Parent == null)
        {
            return;
        }

        _currentOpenDelayRunDisposable?.Dispose();
        _currentCloseDelayRunDisposable?.Dispose();

        if (!menuItem.IsPointerOverSubMenu)
        {
            _currentCloseDelayRunDisposable = DelayRun(() =>
            {
                if (!menuItem.IsPointerOverSubMenu)
                {
                    menuItem.Close();
                }
            }, MenuShowDelay);
        }
    }

    protected virtual void PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var sourceControl = e.Source as Control;
        var menuItem      = GetMenuItemCore(sourceControl);
        if (menuItem is null || !menuItem.ItemHeader.IsVisualAncestorOf(sourceControl)) 
        {
            return;
        }
        
        _currentPressedIsValid = true;
        _latestClickedItem     = menuItem;
        if (sender is Visual visual &&
            e.GetCurrentPoint(visual).Properties.IsLeftButtonPressed)
        {
            Select(menuItem);
            e.Handled = true;
        }
    }
    
    protected virtual void PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_latestClickedItem is null || !_currentPressedIsValid) 
        {
            return;
        }

        _currentPressedIsValid = false;

        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            Click(_latestClickedItem);
            e.Handled = true;
        }
    }

    public void Select(NavMenuItem menuItem)
    {
        if (menuItem.HasSubMenu)
        {
            if (!menuItem.IsSubMenuOpen)
            {
                menuItem.Open();
            }
        }
        else
        {
            // 判断当前选中的是不是自己
            if (!ReferenceEquals(_latestSelectedItem, menuItem))
            {
                ISet<NavMenuItem> oldSelectedPaths = new HashSet<NavMenuItem>();
                if (_latestSelectedItem != null)
                {
                    var oldItems = NavMenu.CollectSelectPathItems(_latestSelectedItem);
                    foreach (var oldItem in oldItems)
                    {
                        oldSelectedPaths.Add(oldItem);
                    }
                }
                
                var newItems         = NavMenu.CollectSelectPathItems(menuItem);
                var newSelectedPaths = newItems.ToHashSet();
                
                var delta = oldSelectedPaths.Except(newSelectedPaths);

                var navMenu = Menu as NavMenu;
                foreach (var oldInSelectPathItem in delta)
                {
                    oldInSelectPathItem.SetCurrentValue(NavMenuItem.IsInSelectedPathProperty, false);
                }

                if (_latestSelectedItem != null)
                {
                    var oldParentItem = ItemsControl.ItemsControlFromItemContainer(_latestSelectedItem) as IMenuChildSelectable;
                    oldParentItem?.SelectChildItem(_latestSelectedItem, false);
                }

                foreach (var newInSelectPathItem in newSelectedPaths)
                {
                    newInSelectPathItem.SetCurrentValue(NavMenuItem.IsInSelectedPathProperty, true);
                }

                var parentItem = ItemsControl.ItemsControlFromItemContainer(menuItem) as IMenuChildSelectable;
                parentItem?.SelectChildItem(menuItem, true);
                _latestSelectedItem = menuItem;
                navMenu?.RaiseNavMenuItemSelected(menuItem);
            }
        }
    }
    
    protected virtual void RawInput(RawInputEventArgs e)
    {
        var mouse = e as RawPointerEventArgs;

        if (mouse?.Type == RawPointerEventType.NonClientLeftButtonDown)
        {
            if (_latestSelectedItem is INavMenuItem latestSelectedItem)
            {
                var topLevelItem = FindTopLevelMenuItem(latestSelectedItem);
                if (topLevelItem != null && topLevelItem.IsSubMenuOpen)
                {
                    topLevelItem.IsSubMenuOpen = false;
                }
            }
        }
    }

    protected virtual void RootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_latestSelectedItem is INavMenuItem latestSelectedItem)
        {
            var topLevelItem = FindTopLevelMenuItem(latestSelectedItem);
            if (topLevelItem != null && topLevelItem.IsSubMenuOpen)
            {
                if (e.Source is ILogical control && !topLevelItem.IsLogicalAncestorOf(control))
                {
                    topLevelItem.IsSubMenuOpen = false;
                }
            }
        }
        else
        {
            if (e.Source is ILogical control && !Menu.IsLogicalAncestorOf(control))
            {
                CloseAllTopLevelMenuItems();
            }
        }
    }

    protected virtual void WindowDeactivated(object? sender, EventArgs e)
    {
        CloseAllTopLevelMenuItems();
    }

    private void CloseAllTopLevelMenuItems()
    {
        if (Menu != null)
        {
            foreach (var i in Menu.SubItems)
            {
                i.Close();
            }
        }
    }

    internal void AttachCore(INavMenu navMenu)
    {
        if (Menu != null)
        {
            throw new NotSupportedException("DefaultMenuInteractionHandler is already attached.");
        }
        
        Menu                 =  navMenu;
        Menu.GotFocus        += GotFocus;
        Menu.LostFocus       += LostFocus;
        Menu.PointerPressed  += PointerPressed;
        Menu.PointerReleased += PointerReleased;
        
        Menu.AddHandler(NavMenuItem.PointerEnteredItemEvent, NotifyPointerEntered);
        Menu.AddHandler(NavMenuItem.PointerExitedItemEvent, NotifyPointerExited);
        
        _root = Menu.VisualRoot;
        
        if (_root is InputElement inputRoot)
        {
            inputRoot.AddHandler(InputElement.PointerPressedEvent, RootPointerPressed, RoutingStrategies.Tunnel);
        }
        
        if (_root is WindowBase window)
        {
            window.Deactivated += WindowDeactivated;
        }
        
        if (_root is TopLevel tl && tl.PlatformImpl != null)
        {
            tl.PlatformImpl.LostFocus += TopLevelLostPlatformFocus;
        }
        
        _inputManagerSubscription = InputManager?.Process.Subscribe(RawInput);
    }

    internal void DetachCore(INavMenu navMenu)
    {
        if (Menu != navMenu)
        {
            throw new NotSupportedException("DefaultMenuInteractionHandler is not attached to the navMenu.");
        }

        Menu.GotFocus        -= GotFocus;
        Menu.LostFocus       -= LostFocus;
        Menu.PointerPressed  -= PointerPressed;
        Menu.PointerReleased -= PointerReleased;
       
        Menu.RemoveHandler(NavMenuItem.PointerEnteredItemEvent, NotifyPointerEntered);
        Menu.RemoveHandler(NavMenuItem.PointerExitedItemEvent, NotifyPointerExited);

        if (_root is InputElement inputRoot)
        {
            inputRoot.RemoveHandler(InputElement.PointerPressedEvent, RootPointerPressed);
        }

        if (_root is WindowBase root)
        {
            root.Deactivated -= WindowDeactivated;
        }

        if (_root is TopLevel tl && tl.PlatformImpl != null)
        {
            tl.PlatformImpl.LostFocus -= TopLevelLostPlatformFocus;
        }

        _inputManagerSubscription?.Dispose();

        Menu               = null;
        _root              = null;
    }

    internal void Click(INavMenuItem item)
    {
        (item as IClickableControl)?.RaiseClick();
        if (Menu is NavMenu navMenu) 
        {
            navMenu.RaiseNavMenuItemClick(item);
            if (!item.HasSubMenu && !item.StaysOpenOnClick)
            {
                var topLevelItem = FindTopLevelMenuItem(item);
                topLevelItem?.Close();
            }
        }
    }

    private static INavMenuItem? FindTopLevelMenuItem(INavMenuItem item)
    {
        if (item.IsTopLevel)
        {
            return item;
        }

        var current = item;
        while (current != null && !current.IsTopLevel)
        {
            current = current.Parent as INavMenuItem;
        }
        return current;
    }
    
    internal void OpenWithDelay(INavMenuItem item)
    {
        void Execute()
        {
            var parent = item.Parent as NavMenuItem;
            if (!item.IsTopLevel && parent?.Popup?.Host != null)
            {
                item.Open();
            }
        }
        _currentOpenDelayRunDisposable?.Dispose();
        _currentOpenDelayRunDisposable = DelayRun(Execute, MenuShowDelay);
    }

    internal static NavMenuItem? GetMenuItemCore(StyledElement? item)
    {
        NavMenuItem? target  = null;
        var           current = item;
        while (current != null)
        {
            if (current is NavMenuItem menuItem)
            {
                target = menuItem;
                break;
            }
            current = current.Parent;
        }
        return target;
    }

    private void TopLevelLostPlatformFocus()
    {
        Menu?.Close();
    }

    private static IDisposable DefaultDelayRun(Action action, TimeSpan timeSpan)
    {
        return DispatcherTimer.RunOnce(action, timeSpan);
    }
}