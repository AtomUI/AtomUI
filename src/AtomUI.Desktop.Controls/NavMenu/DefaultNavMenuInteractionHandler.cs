using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

internal class DefaultNavMenuInteractionHandler : NavMenuInteractionHandlerBase
{
    private IDisposable? _inputManagerSubscription;
    private TopLevel? _root;
    private IDisposable? _currentOpenDelayRunDisposable;
    private IDisposable? _currentCloseDelayRunDisposable;
    private IDisposable? _deactivationSubscription;
    private INavMenuItem? _pendingOpenItem;
    private INavMenuItem? _pendingCloseItem;

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

    protected Func<Action, TimeSpan, IDisposable> DelayRun { get; }

    protected IInputManager? InputManager { get; }

    public static TimeSpan MenuShowDelay { get; set; } = TimeSpan.FromMilliseconds(400);

    protected virtual void NotifyPointerEntered(object? sender, RoutedEventArgs e)
    {
        if (IsLayoutGeneratedPointerTransition(e))
        {
            return;
        }

        var menuItem = GetMenuItemCore(e.Source as Control) as INavMenuItem;
        if (menuItem?.Parent == null)
        {
            return;
        }

        DisposePendingDelayRuns();
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
        if (IsLayoutGeneratedPointerTransition(e))
        {
            return;
        }

        var menuItem = GetMenuItemCore(e.Source as Control) as INavMenuItem;

        if (menuItem?.Parent == null)
        {
            return;
        }

        DisposePendingDelayRuns();

        if (!menuItem.IsPointerOverSubMenu)
        {
            _pendingCloseItem = menuItem;
            _currentCloseDelayRunDisposable = DelayRun(() =>
            {
                if (!menuItem.IsPointerOverSubMenu)
                {
                    menuItem.Close();
                }

                _currentCloseDelayRunDisposable = null;
                _pendingCloseItem               = null;
            }, MenuShowDelay);
        }
    }

    private static bool IsLayoutGeneratedPointerTransition(RoutedEventArgs e)
    {
        // Avalonia revalidates pointer-over on scene invalidation with timestamp 0.
        // Those transitions are caused by layout/scroll moving visuals under a stationary pointer,
        // so they must not drive hover-open or hover-close behavior.
        return e is NavMenuItemPointerEventArgs { PointerTimestamp: 0 };
    }

    protected override void ActivateSubMenuItem(NavMenuItem menuItem)
    {
        if (!menuItem.IsSubMenuOpen)
        {
            menuItem.Open();
        }
    }

    protected virtual void RawInput(RawInputEventArgs e)
    {
        var mouse = e as RawPointerEventArgs;

        if (mouse?.Type == RawPointerEventType.NonClientLeftButtonDown)
        {
            CloseAllTopLevelMenuItems();
        }
    }

    protected virtual void RootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (HasOpenTopLevelItem() && !IsPointerWithinOpenTopLevelItem(e.Source))
        {
            CloseAllTopLevelMenuItems();
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
            foreach (var menuItem in Menu.SubItems)
            {
                if (menuItem.IsSubMenuOpen)
                {
                    menuItem.Close();
                }
            }
        }
    }

    private bool HasOpenTopLevelItem()
    {
        if (Menu == null)
        {
            return false;
        }

        foreach (var child in Menu.SubItems)
        {
            if (child.IsSubMenuOpen)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPointerWithinOpenTopLevelItem(object? source)
    {
        if (Menu == null)
        {
            return false;
        }

        var sourceLogical = source as ILogical;
        var sourceItem    = GetMenuItemCore(source as StyledElement);
        foreach (var openTopLevelItem in Menu.SubItems)
        {
            if (!openTopLevelItem.IsSubMenuOpen)
            {
                continue;
            }

            if (ReferenceEquals(source, openTopLevelItem) ||
                sourceItem != null && ReferenceEquals(FindTopLevelMenuItem(sourceItem), openTopLevelItem) ||
                openTopLevelItem.IsPointerOverSubMenu)
            {
                return true;
            }

            if (sourceLogical != null && openTopLevelItem.IsLogicalAncestorOf(sourceLogical))
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnAttached(INavMenu navMenu)
    {
        navMenu.AddHandler(NavMenuItem.PointerEnteredItemEvent, NotifyPointerEntered);
        navMenu.AddHandler(NavMenuItem.PointerExitedItemEvent, NotifyPointerExited);

        if (navMenu is Visual visual)
        {
            _root = TopLevel.GetTopLevel(visual);
        }

        if (_root is InputElement inputRoot)
        {
            inputRoot.AddHandler(InputElement.PointerPressedEvent, RootPointerPressed, RoutingStrategies.Tunnel);
        }

        _deactivationSubscription = TopLevelDeactivation.Subscribe(_root, WindowDeactivated);

        if (_root != null && _root.PlatformImpl != null)
        {
            _root.PlatformImpl.LostFocus += TopLevelLostPlatformFocus;
        }

        _inputManagerSubscription = InputManager?.Process.Subscribe(RawInput);
    }

    protected override void OnDetached(INavMenu navMenu)
    {
        navMenu.RemoveHandler(NavMenuItem.PointerEnteredItemEvent, NotifyPointerEntered);
        navMenu.RemoveHandler(NavMenuItem.PointerExitedItemEvent, NotifyPointerExited);

        if (_root is InputElement inputRoot)
        {
            inputRoot.RemoveHandler(InputElement.PointerPressedEvent, RootPointerPressed);
        }

        _deactivationSubscription?.Dispose();

        if (_root is TopLevel tl && tl.PlatformImpl != null)
        {
            tl.PlatformImpl.LostFocus -= TopLevelLostPlatformFocus;
        }

        _inputManagerSubscription?.Dispose();
        _inputManagerSubscription = null;

        DisposePendingDelayRuns();
        _root                      = null;
        _deactivationSubscription = null;
    }

    protected override void OnForgotten(NavMenuItem menuItem)
    {
        if (ReferenceEquals(_pendingOpenItem, menuItem))
        {
            DisposePendingOpenDelayRun();
        }

        if (ReferenceEquals(_pendingCloseItem, menuItem))
        {
            DisposePendingCloseDelayRun();
        }
    }
    
    protected override void RaiseItemInvocation(INavMenuItem item)
    {
        base.RaiseItemInvocation(item);
        if (Menu is NavMenu)
        {
            if (!item.HasSubMenu && !item.StaysOpenOnClick)
            {
                var topLevelItem = FindTopLevelMenuItem(item);
                topLevelItem?.Close();
            }
        }
    }

    internal void OpenWithDelay(INavMenuItem item)
    {
        void Execute()
        {
            _currentOpenDelayRunDisposable = null;
            _pendingOpenItem               = null;
            var parent = item.Parent as NavMenuItem;
            if (item.IsTopLevel || parent?.Popup?.IsOpen == true)
            {
                item.Open();
            }
        }
        DisposePendingOpenDelayRun();
        _pendingOpenItem = item;
        _currentOpenDelayRunDisposable = DelayRun(Execute, MenuShowDelay);
    }

    private void DisposePendingDelayRuns()
    {
        DisposePendingOpenDelayRun();
        DisposePendingCloseDelayRun();
    }

    private void DisposePendingOpenDelayRun()
    {
        _currentOpenDelayRunDisposable?.Dispose();
        _currentOpenDelayRunDisposable = null;
        _pendingOpenItem               = null;
    }

    private void DisposePendingCloseDelayRun()
    {
        _currentCloseDelayRunDisposable?.Dispose();
        _currentCloseDelayRunDisposable = null;
        _pendingCloseItem               = null;
    }

    private void TopLevelLostPlatformFocus()
    {
        CloseAllTopLevelMenuItems();
    }

    private static IDisposable DefaultDelayRun(Action action, TimeSpan timeSpan)
    {
        return DispatcherTimer.RunOnce(action, timeSpan);
    }
}
