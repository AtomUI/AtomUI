using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaMenu = Avalonia.Controls.Menu;

/// <summary>
/// A horizontal menu bar whose entries open their submenu in a dropdown.
/// Each item is shown by a first-level <see cref="MenuItem" /> container; a <see cref="MenuItemGroupData" /> item
/// becomes a titled <see cref="MenuItemGroup" /> and a <see cref="MenuSeparatorData" /> item a vertical separator.
/// A menu hosted in a detached title-bar popup lies outside the visual tree of its host window, so it relays
/// item clicks to that window itself.
/// </summary>
public partial class Menu : AvaloniaMenu,
                            ICustomizableSizeTypeAware,
                            IMotionAwareControl,
                            IScrollAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Menu>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Menu>();

    /// <summary>
    /// Identifies the <see cref="IsScrollEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        ScrollAwareControlProperty.IsScrollEnabledProperty.AddOwner<Menu>();

    /// <summary>
    /// Identifies the <see cref="DisplayPageSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> DisplayPageSizeProperty =
        AvaloniaProperty.Register<Menu, int>(nameof(DisplayPageSize), 10);

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        Flyout.ShouldUseOverlayPopupProperty.AddOwner<Menu>();

    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<Menu>();

    /// <summary>
    /// Size preset of the menu bar and of its first-level entries. Set it on the menu: the value does not inherit,
    /// and it is bound through to every first-level <see cref="MenuItem" />, whose theme applies the same preset.
    /// <c>Large</c>, <c>Middle</c> and <c>Small</c> give the bar the minimum height and the corner radius of that
    /// size; <c>Custom</c> uses the values of <c>Middle</c>. The default is <c>Middle</c>.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Enables the motion of the submenu popups. The theme sets it from the <c>EnableMotion</c> token of the menu.
    /// The value is bound through to every first-level <see cref="MenuItem" />, so the entries and their popups
    /// follow it. While it is enabled, <see cref="Close" /> closes the submenus one after another through
    /// <c>CloseItemAsync</c> on the dispatcher and closes the menu after the last one; without it every submenu is
    /// closed synchronously.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Whether the items of a submenu popup are hosted in a scroll viewer. A submenu with more than
    /// <see cref="DisplayPageSize" /> rows is then limited to that many item rows plus the popup padding and
    /// scrolls. The value inherits, so the menu items and popups below the menu follow the value set on it.
    /// The default is <see langword="true" />.
    /// </summary>
    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    /// <summary>
    /// Number of item rows a submenu popup shows at most. It takes effect only while
    /// <see cref="IsScrollEnabled" /> is <see langword="true" />: the height of the popup is then limited to this
    /// many item heights plus the popup padding, and further rows are reached by scrolling. The value is bound
    /// through to every first-level <see cref="MenuItem" />. The default is 10.
    /// </summary>
    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
    }

    /// <summary>
    /// Whether the popup of a submenu is placed in the overlay layer of the window (<see langword="true" />) or in
    /// the visual tree of its entry (<see langword="false" />). The theme sets it to <see langword="true" />.
    /// The value is bound through to every first-level <see cref="MenuItem" />, which hands it to its popup.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Keeps one submenu open without user interaction: the selected entry, or the first entry that has a submenu
    /// when the selected one has none. The entry becomes the selected item and stays open until this property is
    /// cleared or the menu leaves the visual tree; <see cref="Close" /> does nothing meanwhile. The request waits
    /// while no entry has a submenu yet and is applied when one is created. The value is set without replacing a
    /// binding. Gallery semantic previews pin the popup open this way so the popup parts can be inspected and
    /// highlighted.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion

    private bool _isClosing;
    private bool _isClosingForLifecycle;
    private bool _isApplyingPinnedOpen;
    private bool _isSyncingDetachedTitleBarRadioGroup;
    private IDisposable? _detachedTitleBarPopupDismissRoot;
    private MenuItem? _pinnedOpenMenuItem;

    static Menu()
    {
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<Menu>(false);
    }

    public Menu()
        : base(new DefaultMenuInteractionHandler(false))
    {
        AddHandler(MenuItem.ClickEvent, RelayDetachedTitleBarPopupClickToHostWindow);
        AddHandler(MenuItem.IsCheckStateChangedEvent, SyncDetachedTitleBarRadioGroup);
    }

    /// <summary>
    /// Creates the container for an item: <see cref="MenuSeparator" /> for a <see cref="MenuSeparatorData" />,
    /// <see cref="MenuItemGroup" /> for a <see cref="MenuItemGroupData" /> and <see cref="MenuItem" /> for any
    /// other item. <c>recycleKey</c> is ignored; every container is created new.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (item is MenuSeparatorData)
        {
            return new MenuSeparator();
        }

        if (item is MenuItemGroupData)
        {
            return new MenuItemGroup();
        }

        return new MenuItem();
    }

    /// <summary>
    /// Reports whether an item needs a container. <see cref="MenuItem" />, <see cref="MenuSeparator" /> and
    /// <see cref="MenuItemGroup" /> items are used as their own containers.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the item is anything else; <c>recycleKey</c> is then <c>DefaultRecycleKey</c>.
    /// <see langword="false" /> for the three container types; <c>recycleKey</c> is then <see langword="null" />.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (item is MenuItem or MenuSeparator or MenuItemGroup)
        {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    /// <summary>
    /// Fills a first-level container after the base member has prepared it. A <see cref="MenuItem" /> container
    /// gets the top semantic level. An item that is not a visual element becomes its header unless the header is
    /// already set, and an <see cref="IMenuItemData" /> item also supplies its icon, key, enabled state, input
    /// gesture and danger flag the same way. <see cref="DisplayPageSize" />, <c>ItemTemplate</c> (also as the
    /// header template when one is set), <see cref="SizeType" />, <see cref="IsMotionEnabled" /> and
    /// <see cref="ShouldUseOverlayPopup" /> are bound through to it. <see cref="PrepareMenuItem" /> is then
    /// called, and the container is pinned open when a pinned-open request is still unfulfilled.
    /// A <see cref="MenuSeparator" /> is laid out vertically and a <see cref="MenuItemGroup" /> gets the top
    /// semantic level.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The container is of another type than these three.
    /// </exception>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MenuItem menuItem)
        {
            // First level of a plain menu bar: hand down the top semantic level, so that the container carries the
            // mutually exclusive marker class of the first level.
            menuItem.SemanticLevel = MenuSemanticLevel.TopLevel;

            if (item != null && item is not Visual)
            {
                if (!menuItem.IsSet(MenuItem.HeaderProperty))
                {
                    menuItem.SetCurrentValue(MenuItem.HeaderProperty, item);
                }

                if (item is IMenuItemData menuItemData)
                {
                    if (!menuItem.IsSet(MenuItem.IconProperty))
                    {
                        menuItem.SetCurrentValue(MenuItem.IconProperty, menuItemData.Icon);
                    }

                    if (menuItem.ItemKey == null)
                    {
                        menuItem.ItemKey = menuItemData.ItemKey;
                    }

                    if (!menuItem.IsSet(MenuItem.IsEnabledProperty))
                    {
                        menuItem.SetCurrentValue(IsEnabledProperty, menuItemData.IsEnabled);
                    }

                    if (!menuItem.IsSet(MenuItem.InputGestureProperty))
                    {
                        menuItem.SetCurrentValue(MenuItem.InputGestureProperty, menuItemData.InputGesture);
                    }

                    if (!menuItem.IsSet(MenuItem.IsDangerProperty))
                    {
                        menuItem.SetCurrentValue(MenuItem.IsDangerProperty, menuItemData.IsDanger);
                    }
                }
            }

            if (ItemTemplate != null)
            {
                menuItem[!MenuItem.HeaderTemplateProperty] = this[!ItemTemplateProperty];
            }

            menuItem[!MenuItem.DisplayPageSizeProperty]       = this[!DisplayPageSizeProperty];
            menuItem[!MenuItem.ItemTemplateProperty]           = this[!ItemTemplateProperty];
            menuItem[!SizeTypeProperty]                        = this[!SizeTypeProperty];
            menuItem[!IsMotionEnabledProperty]                 = this[!IsMotionEnabledProperty];
            menuItem[!MenuItem.ShouldUseOverlayPopupProperty]  = this[!ShouldUseOverlayPopupProperty];

            PrepareMenuItem(menuItem, item, index);
            EnsurePinnedOpenMenuItemForContainer(menuItem, index);
        }
        else if (container is MenuSeparator menuSeparator)
        {
            menuSeparator.Orientation = Orientation.Vertical;
        }
        else if (container is MenuItemGroup menuItemGroup)
        {
            // Top-level group: the group title and the group list take the first-level semantic level, and the menu
            // items inside the group inherit that level.
            menuItemGroup.SemanticLevel = MenuSemanticLevel.TopLevel;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container),
                "The container type is incorrect, it must be type MenuItem or MenuSeparator.");
        }
    }

    /// <summary>
    /// Resets a container before it is reused or discarded. A <see cref="MenuItem" /> container loses its
    /// pinned-open state and is closed together with its submenus; it and a <see cref="MenuItemGroup" /> container
    /// drop their semantic level. The base member is called last.
    /// </summary>
    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is MenuItem menuItem)
        {
            menuItem.IsPopupPinnedOpen = false;
            menuItem.CloseForLifecycle();
            menuItem.SemanticLevel = MenuSemanticLevel.None;
        }
        else if (container is MenuItemGroup menuItemGroup)
        {
            menuItemGroup.SemanticLevel = MenuSemanticLevel.None;
        }

        base.ClearContainerForItemOverride(container);
    }

    /// <summary>
    /// Keeps the pinned-open submenu in step after the base member has handled the change: turning
    /// <see cref="IsPopupPinnedOpen" /> on pins an entry and turning it off releases the pinned one; while a
    /// submenu is pinned, a change of <c>SelectedIndex</c> makes the pinned submenu follow the selection, unless
    /// the menu is closing because it leaves the visual tree.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsPopupPinnedOpenProperty)
        {
            if (change.GetNewValue<bool>())
            {
                EnsurePinnedOpenMenuItem();
            }
            else
            {
                ClearPinnedOpenMenuItem();
            }
        }
        else if (change.Property == SelectedIndexProperty && IsPopupPinnedOpen && !_isClosingForLifecycle)
        {
            EnsurePinnedOpenMenuItem();
        }
    }

    /// <summary>
    /// Calls the base member, then applies a pending <see cref="IsPopupPinnedOpen" /> request. Pinning needs item
    /// containers that are realized inside the visual tree.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        EnsurePinnedOpenMenuItem();
    }

    /// <summary>
    /// Closes the pinned-open submenu before the base member is called, because the menu leaves the visual tree:
    /// every entry is closed together with its submenus, and <c>Closed</c> is raised when the menu was open.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (IsPopupPinnedOpen)
        {
            ClosePinnedForLifecycle();
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void EnsurePinnedOpenMenuItem()
    {
        if (!IsPopupPinnedOpen || !this.IsAttachedToVisualTree() || _isApplyingPinnedOpen)
        {
            return;
        }

        MenuItem? menuItem = null;
        var index = SelectedIndex;
        if (index >= 0 && ContainerFromIndex(index) is MenuItem selectedItem && selectedItem.HasSubMenu)
        {
            menuItem = selectedItem;
        }
        else
        {
            for (var i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is MenuItem candidate && candidate.HasSubMenu)
                {
                    menuItem = candidate;
                    index = i;
                    break;
                }
            }
        }

        // The request is not dropped while no container exists yet: IsPopupPinnedOpen is a persistent request, and
        // EnsurePinnedOpenMenuItemForContainer fulfils it when a container is prepared.
        if (menuItem is null)
        {
            return;
        }

        ApplyPinnedOpenMenuItem(menuItem, index);
    }

    // Pinning invariant of the container prepare stage. Declarative usage (AXAML sets IsPopupPinnedOpen="True"
    // before the template is applied and before containers are generated) first gets a container that can be
    // pinned here, so the request has to be fulfilled when the container is realized.
    private void EnsurePinnedOpenMenuItemForContainer(MenuItem menuItem, int index)
    {
        if (!IsPopupPinnedOpen ||
            _pinnedOpenMenuItem is not null ||
            !menuItem.HasSubMenu ||
            !ReferenceEquals(menuItem.Parent, this))
        {
            return;
        }

        ApplyPinnedOpenMenuItem(menuItem, index);
    }

    private void ApplyPinnedOpenMenuItem(MenuItem menuItem, int index)
    {
        if (_isApplyingPinnedOpen)
        {
            return;
        }

        // SetCurrentValue(SelectedIndexProperty) triggers the pinning evaluation again. The re-entrancy guard
        // keeps it from being applied twice.
        _isApplyingPinnedOpen = true;
        try
        {
            if (!ReferenceEquals(_pinnedOpenMenuItem, menuItem))
            {
                if (_pinnedOpenMenuItem != null)
                {
                    _pinnedOpenMenuItem.IsPopupPinnedOpen = false;
                    _pinnedOpenMenuItem.CloseForLifecycle();
                }

                _pinnedOpenMenuItem = menuItem;
            }

            SetCurrentValue(SelectedIndexProperty, index);
            menuItem.IsPopupPinnedOpen = true;
        }
        finally
        {
            _isApplyingPinnedOpen = false;
        }
    }

    private void ClearPinnedOpenMenuItem()
    {
        if (_pinnedOpenMenuItem == null)
        {
            return;
        }

        _pinnedOpenMenuItem.IsPopupPinnedOpen = false;
        _pinnedOpenMenuItem = null;
    }

    /// <summary>
    /// Called for each first-level <see cref="MenuItem" /> container after the menu has filled in its header, its
    /// item data and the forwarded properties, and before the pinned-open request is applied to it. Not called for
    /// separators and groups. The default implementation does nothing.
    /// </summary>
    protected virtual void PrepareMenuItem(MenuItem menuItem, object? item, int index)
    {
    }

    /// <summary>
    /// Calls the base member, then applies the <c>TopLevelMenuItemTheme</c> control theme, looked up from the
    /// menu, to the item containers when the menu has no theme of its own, and registers the menu as the dismiss
    /// root of a detached title-bar popup, which dismisses it through <see cref="Close" />.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureItemContainerTheme(false);
        ConfigureDetachedTitleBarPopupDismissRoot();
    }

    /// <summary>
    /// Releases the registration of the menu as the dismiss root of a detached title-bar popup, then calls the
    /// base member.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        DetachedTitleBarPopupSupport.ClearDismissRoot(ref _detachedTitleBarPopupDismissRoot);
        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    /// Calls the base member, then refreshes the registration of the menu as the dismiss root of a detached
    /// title-bar popup, so that it follows the host window the menu has at that time.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureDetachedTitleBarPopupDismissRoot();
    }

    internal const string TopLevelMenuItemThemeKey = "TopLevelMenuItemTheme";

    private void ConfigureItemContainerTheme(bool force)
    {
        if (Theme == null || force)
        {
            // Resolve from the Menu itself so a theme supplied in a local resource scope wins over the application one.
            if (this.TryFindResource(TopLevelMenuItemThemeKey, out var resource) && resource is ControlTheme theme)
            {
                ItemContainerTheme = theme;
            }
        }
    }

    /// <summary>
    /// Closes the open submenus, resets <c>IsOpen</c> and <c>SelectedIndex</c> and raises the <c>Closed</c> event.
    /// Does nothing while <see cref="IsPopupPinnedOpen" /> is <see langword="true" />. Otherwise the pending
    /// hover operations of the interaction handler are canceled first, and when the menu is open and no close
    /// is in progress, every submenu is closed. With <see cref="IsMotionEnabled" /> the submenus are closed one
    /// after another through <c>CloseItemAsync</c> on the dispatcher; without it they are closed synchronously.
    /// </summary>
    public override void Close()
    {
        if (IsPopupPinnedOpen)
        {
            return;
        }

        if (InteractionHandler is DefaultMenuInteractionHandler interactionHandler)
        {
            interactionHandler.CancelPendingHoverOperations();
        }

        if (!IsOpen || _isClosing)
        {
            return;
        }

        _isClosing = true;
        if (IsMotionEnabled)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                for (var i = 0; i < ItemCount; i++)
                {
                    var container = ContainerFromIndex(i);
                    if (container is MenuItem menuItem)
                    {
                        await menuItem.CloseItemAsync();
                    }
                }

                HandleMenuClosed();
                _isClosing = false;
            });
        }
        else
        {
            for (var i = 0; i < ItemCount; i++)
            {
                var container = ContainerFromIndex(i);
                if (container is MenuItem menuItem)
                {
                    menuItem.Close();
                }
            }

            HandleMenuClosed();
            _isClosing = false;
        }
    }

    internal void CloseImmediately()
    {
        if (IsPopupPinnedOpen)
        {
            return;
        }

        if (InteractionHandler is DefaultMenuInteractionHandler interactionHandler)
        {
            interactionHandler.CancelPendingHoverOperations();
        }

        if (!IsOpen && !_isClosing)
        {
            return;
        }

        _isClosing = false;
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is MenuItem menuItem)
            {
                menuItem.Close();
            }
        }

        HandleMenuClosed();
    }

    private void ClosePinnedForLifecycle()
    {
        _isClosingForLifecycle = true;
        try
        {
            if (InteractionHandler is DefaultMenuInteractionHandler interactionHandler)
            {
                interactionHandler.CancelPendingHoverOperations();
            }

            _isClosing = false;
            ClearPinnedOpenMenuItem();

            for (var i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is MenuItem menuItem)
                {
                    menuItem.CloseForLifecycle();
                }
            }

            if (IsOpen)
            {
                HandleMenuClosed();
            }
            else
            {
                SetCurrentValue(SelectedIndexProperty, -1);
            }
        }
        finally
        {
            _isClosingForLifecycle = false;
        }
    }

    private void HandleMenuClosed()
    {
        IsOpen        = false;
        SelectedIndex = -1;
        RaiseEvent(new RoutedEventArgs
        {
            RoutedEvent = ClosedEvent,
            Source      = this
        });
    }

    private void ConfigureDetachedTitleBarPopupDismissRoot()
    {
        _detachedTitleBarPopupDismissRoot =
            DetachedTitleBarPopupSupport.UpdateDismissRoot(
                this,
                _detachedTitleBarPopupDismissRoot,
                () => IsOpen,
                Close,
                IsInteractionInsideMenu);
    }

    // Title-bar overlay menus are outside the host Window's visual tree, so the
    // default menu root cannot relay clicks or manage radio groups for this case.
    private void RelayDetachedTitleBarPopupClickToHostWindow(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not MenuItem ||
            !DetachedTitleBarPopupSupport.TryResolveHostWindow(this, out var hostWindow))
        {
            return;
        }

        var relayedArgs = new RoutedEventArgs(MenuItem.ClickEvent)
        {
            Source = e.Source
        };
        hostWindow.RaiseRoutedEventFromOverlay((MenuItem)e.Source, relayedArgs);
        e.Handled = relayedArgs.Handled;
    }

    private void SyncDetachedTitleBarRadioGroup(object? sender, RoutedEventArgs e)
    {
        if (_isSyncingDetachedTitleBarRadioGroup ||
            e.Source is not MenuItem checkedItem ||
            !checkedItem.IsChecked ||
            checkedItem.ToggleType != MenuItemToggleType.Radio ||
            !DetachedTitleBarPopupSupport.TryResolveHostWindow(this, out _))
        {
            return;
        }

        _isSyncingDetachedTitleBarRadioGroup = true;
        try
        {
            if (string.IsNullOrEmpty(checkedItem.GroupName))
            {
                UncheckSiblingRadioItems(checkedItem);
            }
            else
            {
                UncheckNamedRadioGroup(checkedItem);
            }
        }
        finally
        {
            _isSyncingDetachedTitleBarRadioGroup = false;
        }
    }

    private static void UncheckSiblingRadioItems(MenuItem checkedItem)
    {
        var parent = ((ILogical)checkedItem).LogicalParent;
        if (parent is null)
        {
            return;
        }

        foreach (var sibling in parent.LogicalChildren)
        {
            if (sibling is MenuItem menuItem &&
                !ReferenceEquals(menuItem, checkedItem) &&
                menuItem.ToggleType == MenuItemToggleType.Radio &&
                string.IsNullOrEmpty(menuItem.GroupName) &&
                menuItem.IsChecked)
            {
                menuItem.SetCurrentValue(MenuItem.IsCheckedProperty, false);
            }
        }
    }

    private void UncheckNamedRadioGroup(MenuItem checkedItem)
    {
        foreach (var menuItem in EnumerateMenuItems(this))
        {
            if (!ReferenceEquals(menuItem, checkedItem) &&
                menuItem.ToggleType == MenuItemToggleType.Radio &&
                menuItem.GroupName == checkedItem.GroupName &&
                menuItem.IsChecked)
            {
                menuItem.SetCurrentValue(MenuItem.IsCheckedProperty, false);
            }
        }
    }

    private static IEnumerable<MenuItem> EnumerateMenuItems(ILogical owner)
    {
        foreach (var child in owner.LogicalChildren)
        {
            if (child is MenuItem menuItem)
            {
                yield return menuItem;
                foreach (var descendant in EnumerateMenuItems(menuItem))
                {
                    yield return descendant;
                }
            }
        }
    }

    private bool IsInteractionInsideMenu(ILogical control)
    {
        if (this.IsLogicalAncestorOf(control))
        {
            return true;
        }

        var current = control as StyledElement;
        while (current is not null)
        {
            if (ReferenceEquals(current, this))
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }
}
