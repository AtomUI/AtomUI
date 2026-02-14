using AtomUI.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class InlineNavMenuInteractionHandler : INavMenuInteractionHandler
{
     internal INavMenu? Menu { get; private set; }
    
    public void Attach(NavMenu navMenu) => AttachCore(navMenu);
    public void Detach(NavMenu navMenu) => DetachCore(navMenu);

    private bool _currentPressedIsValid = false;
    private NavMenuItem? _latestSelectedItem = null;
    private NavMenuItem? _latestClickedItem = null;

    internal void AttachCore(INavMenu navMenu)
    {
        if (Menu != null)
        {
            throw new NotSupportedException("InlineNavMenuInteractionHandler is already attached.");
        }
        Menu                 =  navMenu;
        Menu.PointerPressed  += PointerPressed;
        Menu.PointerReleased += PointerReleased;
    }

    internal void DetachCore(INavMenu navMenu)
    {
        if (Menu != navMenu)
        {
            throw new NotSupportedException("InlineNavMenuInteractionHandler is not attached to the navMenu.");
        }
        Menu.PointerPressed  -= PointerPressed;
        Menu.PointerReleased -= PointerReleased;
        Menu                 =  null;
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
    
    internal void Click(INavMenuItem item)
    {
        if (item is IClickableControl clickableControl)
        {
            clickableControl.RaiseClick();
        }
        if (Menu is NavMenu navMenu)
        {
            navMenu.RaiseNavMenuItemClick(item);
        }
    }
    
    public void Select(NavMenuItem menuItem)
    {
        if (menuItem.HasSubMenu)
        {
            if (menuItem.IsSubMenuOpen)
            {
                menuItem.Close();
            }
            else
            {
                Open(menuItem);
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

    internal void Open(INavMenuItem menuItem) => menuItem.Open();
    
    internal static NavMenuItem? GetMenuItemCore(StyledElement? item)
    {
        NavMenuItem? target = null;
        var current = item;
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
}