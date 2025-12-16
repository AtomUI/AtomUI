using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Theme.Styling;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(
    NavMenuPseudoClass.InlineMode,
    NavMenuPseudoClass.HorizontalMode,
    NavMenuPseudoClass.VerticalMode,
    NavMenuPseudoClass.DarkStyle,
    NavMenuPseudoClass.LightStyle)]
public class NavMenu : ItemsControl,
                       IFocusScope,
                       INavMenu,
                       IMotionAwareControl,
                       IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly DirectProperty<NavMenu, INavMenuItemData?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, INavMenuItemData?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public static readonly DirectProperty<NavMenu, TreeNodePath?> DefaultSelectedPathProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, TreeNodePath?>(
            nameof(DefaultSelectedPath),
            o => o.DefaultSelectedPath,
            (o, v) => o.DefaultSelectedPath = v);

    public static readonly DirectProperty<NavMenu, bool> IsOpenProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, bool>(
            nameof(IsOpen),
            o => o.IsOpen);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NavMenu>();

    public static readonly StyledProperty<bool> IsAccordionModeProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsAccordionMode), false);

    public static readonly StyledProperty<NavMenuMode> ModeProperty =
        AvaloniaProperty.Register<NavMenu, NavMenuMode>(nameof(Mode), NavMenuMode.Horizontal);

    public static readonly StyledProperty<bool> IsDarkStyleProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsDarkStyle), false);

    public static readonly DirectProperty<NavMenu, IList<TreeNodePath>?> DefaultOpenPathsProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, IList<TreeNodePath>?>(
            nameof(DefaultOpenPaths),
            o => o.DefaultOpenPaths,
            (o, v) => o.DefaultOpenPaths = v);
    
    public static readonly StyledProperty<bool> IsUseOverlayLayerProperty = 
        AvaloniaProperty.Register<NavMenu, bool>(nameof (IsUseOverlayLayer));

    public INavMenuItemData? _selectedItem;

    public INavMenuItemData? SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }

    private IList<TreeNodePath>? _defaultOpenPaths;
    
    public IList<TreeNodePath>? DefaultOpenPaths
    {
        get => _defaultOpenPaths;
        set => SetAndRaise(DefaultOpenPathsProperty, ref _defaultOpenPaths, value);
    }
    
    private TreeNodePath? _defaultSelectedPath;
    
    public TreeNodePath? DefaultSelectedPath
    {
        get => _defaultSelectedPath;
        set => SetAndRaise(DefaultSelectedPathProperty, ref _defaultSelectedPath, value);
    }
    
    private bool _isOpen;
    
    public bool IsOpen
    {
        get => _isOpen;
        protected set => SetAndRaise(IsOpenProperty, ref _isOpen, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public bool IsAccordionMode
    {
        get => GetValue(IsAccordionModeProperty);
        set => SetValue(IsAccordionModeProperty, value);
    }
    
    public NavMenuMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public bool IsDarkStyle
    {
        get => GetValue(IsDarkStyleProperty);
        set => SetValue(IsDarkStyleProperty, value);
    }
    
    public bool IsUseOverlayLayer
    {
        get => GetValue(IsUseOverlayLayerProperty);
        set => SetValue(IsUseOverlayLayerProperty, value);
    }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<NavMenuItemClickEventArgs> NavMenuItemClickEvent =
        RoutedEvent.Register<NavMenu, NavMenuItemClickEventArgs>(nameof(NavMenuItemClick), RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<NavMenuItemSelectedEventArgs> NavMenuItemSelectedEvent =
        RoutedEvent.Register<NavMenu, NavMenuItemSelectedEventArgs>(nameof(NavMenuItemSelected), RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<NavMenu, RoutedEventArgs>(nameof(Opened), RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<NavMenu, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    public event EventHandler<NavMenuItemClickEventArgs>? NavMenuItemClick
    {
        add => AddHandler(NavMenuItemClickEvent, value);
        remove => RemoveHandler(NavMenuItemClickEvent, value);
    }
    
    public event EventHandler<NavMenuItemSelectedEventArgs>? NavMenuItemSelected
    {
        add => AddHandler(NavMenuItemSelectedEvent, value);
        remove => RemoveHandler(NavMenuItemSelectedEvent, value);
    }
    
    public event EventHandler<RoutedEventArgs>? Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }
    
    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> HorizontalBorderThicknessProperty =
        AvaloniaProperty.Register<NavMenuItem, double>(nameof(HorizontalBorderThickness));

    internal double HorizontalBorderThickness
    {
        get => GetValue(HorizontalBorderThicknessProperty);
        set => SetValue(HorizontalBorderThicknessProperty, value);
    }
    
    Control IMotionAwareControl.PropertyBindTarget => this;
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => NavMenuToken.ID;
    
    #endregion
    
    INavMenuInteractionHandler? INavMenu.InteractionHandler => InteractionHandler;
    
    IRenderRoot? INavMenu.VisualRoot => VisualRoot;

    IEnumerable<INavMenuItem> INavMenuElement.SubItems => LogicalChildren.OfType<INavMenuItem>();

    /// <summary>
    /// Gets the interaction handler for the menu.
    /// </summary>
    protected internal INavMenuInteractionHandler? InteractionHandler { get; protected set; }

    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel { Orientation = Orientation.Vertical });
    private ItemsPresenter? _menuItemsPresenter;
    private readonly Dictionary<NavMenuItem, CompositeDisposable> _itemsBindingDisposables = new();
    private IDisposable? _borderThicknessDisposable;
    private bool _defaultOpenPathsApplied;

    static NavMenu()
    {
        ItemsPanelProperty.OverrideDefaultValue(typeof(NavMenu), DefaultPanel);
        KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue(
            typeof(NavMenu),
            KeyboardNavigationMode.Once);
        AutomationProperties.AccessibilityViewProperty.OverrideDefaultValue<NavMenu>(AccessibilityView.Control);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<NavMenu>(AutomationControlType.Menu);
        NavMenuItem.SubmenuOpenedEvent.AddClassHandler<NavMenu>((x, e) => x.OnSubmenuOpened(e));
    }
    
    public NavMenu()
    {
        this.RegisterResources();
        UpdatePseudoClasses();
        LogicalChildren.CollectionChanged += HandleItemsCollectionChanged;
    }
    
    public virtual void Close()
    {
        if (!IsOpen)
        {
            return;
        }
        foreach (var i in ((INavMenu)this).SubItems)
        {
            i.Close();
        }

        IsOpen       = false;
        SelectedItem = null;

        RaiseEvent(new RoutedEventArgs
        {
            RoutedEvent = ClosedEvent,
            Source      = this,
        });
    }

    public virtual void Open()
    {
        if (IsOpen)
        {
            return;
        }

        IsOpen = true;

        RaiseEvent(new RoutedEventArgs
        {
            RoutedEvent = OpenedEvent,
            Source      = this,
        });
    }

    /// <summary>
    /// Called when a submenu opens somewhere in the menu.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected virtual void OnSubmenuOpened(RoutedEventArgs e)
    {
        if (IsAccordionMode)
        {
            if (e.Source is NavMenuItem menuItem && menuItem.Parent == this)
            {
                foreach (var child in this.GetLogicalChildren().OfType<NavMenuItem>())
                {
                    if (child != menuItem && child.IsSubMenuOpen)
                    {
                        child.IsSubMenuOpen = false;
                    }
                }
            }
        }
        IsOpen = true;
    }
    
    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is NavMenuItem menuItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(menuItem, out var disposable))
                        {
                            disposable.Dispose();
                        }
                        _itemsBindingDisposables.Remove(menuItem);
                    }
                }
            }
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsAccordionModeProperty)
        {
            if (change.GetNewValue<bool>())
            {
                foreach (var child in this.GetLogicalChildren().OfType<NavMenuItem>())
                {
                    child.IsSubMenuOpen = false;
                }
            }
        }
        if (change.Property == IsDarkStyleProperty ||
            change.Property == ModeProperty)
        {
            UpdatePseudoClasses();
        }

        if (change.Property == ModeProperty)
        {
            ConfigureControlTheme(true);
            ConfigureItemContainerTheme(true);
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == ModeProperty)
            {
                HandleModeChanged();
            }
            else if (change.Property == SelectedItemProperty)
            {
                if (SelectedItem != null)
                {
                    SelectTargetItem(SelectedItem);
                }
            }
        }
    }

    private void HandleModeChanged()
    {
        CloseChildItemsRecursively();
        RegenerateContainersRecursively();
        SetupMenuItemsPresenter();
        SetupInteractionHandler(true);
    }

    private void SetupMenuItemsPresenter()
    {
        if (_menuItemsPresenter?.Panel is StackPanel stackPanel)
        {
            if (Mode == NavMenuMode.Horizontal)
            {
                stackPanel.Orientation = Orientation.Horizontal;
            }
            else
            {
                stackPanel.Orientation = Orientation.Vertical;
            }
        }
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (item is NavMenuItem or Separator)
        {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new NavMenuItem();
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        // Child menu items should not inherit the menu's ItemContainerTheme as that is specific
        // for top-level menu items.
        if ((container as NavMenuItem)?.ItemContainerTheme == ItemContainerTheme)
        {
            container.ClearValue(ItemContainerThemeProperty);
        }

        if (container is NavMenuItem navMenuItem)
        {
            var disposables = new CompositeDisposable(6);
      
            if (item != null && item is not Visual)
            {
                if (!navMenuItem.IsSet(NavMenuItem.HeaderProperty))
                {
                    navMenuItem.SetCurrentValue(NavMenuItem.HeaderProperty, item);
                }

                if (item is INavMenuItemData menuItemData)
                {
                    if (!navMenuItem.IsSet(NavMenuItem.IconProperty))
                    {
                        navMenuItem.SetCurrentValue(NavMenuItem.IconProperty, menuItemData.Icon);
                    }

                    if (navMenuItem.ItemKey == null)
                    {
                        navMenuItem.ItemKey = menuItemData.ItemKey;
                    }
                    if (!navMenuItem.IsSet(IsEnabledProperty))
                    {
                        navMenuItem.SetCurrentValue(IsEnabledProperty, menuItemData.IsEnabled);
                    }
                }
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, navMenuItem, NavMenuItem.HeaderTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, ItemContainerThemeProperty, navMenuItem, ItemContainerThemeProperty));
            disposables.Add(BindUtils.RelayBind(this, ModeProperty, navMenuItem, NavMenuItem.ModeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsDarkStyleProperty, navMenuItem, NavMenuItem.IsDarkStyleProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, navMenuItem, NavMenuItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, IsUseOverlayLayerProperty, navMenuItem, NavMenuItem.IsUseOverlayLayerProperty));
            
            PrepareNavMenuItem(navMenuItem, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(navMenuItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(navMenuItem);
            }
            _itemsBindingDisposables.Add(navMenuItem, disposables);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type NavMenuItem.");
        }
    }
    
    protected virtual void PrepareNavMenuItem(NavMenuItem navMenuItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(NavMenuPseudoClass.HorizontalMode, Mode == NavMenuMode.Horizontal);
        PseudoClasses.Set(NavMenuPseudoClass.VerticalMode, Mode == NavMenuMode.Vertical);
        PseudoClasses.Set(NavMenuPseudoClass.InlineMode, Mode == NavMenuMode.Inline);
        PseudoClasses.Set(NavMenuPseudoClass.DarkStyle, IsDarkStyle);
        PseudoClasses.Set(NavMenuPseudoClass.LightStyle, !IsDarkStyle);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureControlTheme(false);
        ConfigureItemContainerTheme(false);
        SetupInteractionHandler();
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _borderThicknessDisposable = TokenResourceBinder.CreateTokenBinding(this, HorizontalBorderThicknessProperty,
            SharedTokenKey.LineWidth,
            BindingPriority.Template,
            new RenderScaleAwareDoubleConfigure(this));
        InteractionHandler?.Attach(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _borderThicknessDisposable?.Dispose();
        InteractionHandler?.Detach(this);
    }

    private void SetupInteractionHandler(bool needMount = false)
    {
        if (needMount)
        {
            InteractionHandler?.Detach(this);
        }

        if (Mode == NavMenuMode.Inline)
        {
            InteractionHandler = new InlineNavMenuInteractionHandler();
        }
        else
        {
            InteractionHandler = new DefaultNavMenuInteractionHandler();
        }

        if (needMount)
        {
            InteractionHandler?.Attach(this);
        }
    }
    
    private void ConfigureControlTheme(bool force)
    {
        string? resourceKey = null;
        if (Theme == null || force)
        {
            if (Mode == NavMenuMode.Horizontal)
            {
                resourceKey = NavMenuThemeConstants.HorizontalNavMenuThemeId;
            }
            else
            {
                resourceKey = NavMenuThemeConstants.VerticalNavMenuThemeId;
            }
            if (Application.Current != null)
            {
                if (Application.Current.TryFindResource(resourceKey, out var resource))
                {
                    if (resource is ControlTheme theme)
                    {
                        Theme = theme;
                    }
                }
            }
        }
    }
    
    private void ConfigureItemContainerTheme(bool force)
    {
        if (ItemContainerTheme is null || force)
        {
            string resourceKey;
            if (Mode == NavMenuMode.Vertical)
            {
                resourceKey = NavMenuThemeConstants.VerticalNavMenuItemThemeId;
            }
            else if (Mode == NavMenuMode.Inline)
            {
                resourceKey = NavMenuThemeConstants.InlineNavMenuItemThemeId;
            }
            else
            {
                resourceKey = NavMenuThemeConstants.HorizontalNavMenuItemThemeId;
            }

            if (Application.Current != null)
            {
                if (Application.Current.TryFindResource(resourceKey, out var resource))
                {
                    if (resource is ControlTheme theme)
                    {
                        ItemContainerTheme = theme;
                    }
                }
            }
        }
    }

    internal void ClearSelection()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is NavMenuItem navMenuItem)
            {
                ClearSelectionRecursively(navMenuItem);
            }
        }
    }

    internal static void ClearSelectionRecursively(NavMenuItem item, bool skipSelf = false)
    {
        if (!skipSelf)
        {
            item.SetCurrentValue(NavMenuItem.IsSelectedProperty, false);
        }
        
        for (var i = 0; i < item.ItemCount; i++)
        {
            var container = item.ContainerFromIndex(i);
            if (container is NavMenuItem navMenuItem)
            {
                ClearSelectionRecursively(navMenuItem);
            }
        }
        
    }

    private void CloseChildItemsRecursively()
    {
        CloseInlineItems();
        foreach (var i in ((INavMenu)this).SubItems)
        {
            i.Close();
        }
    }

    private void RegenerateContainersRecursively()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is NavMenuItem navMenuItem)
            {
                navMenuItem.RegenerateContainers();
            }
        }
    }

    private void CloseInlineItems()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is NavMenuItem navMenuItem)
            {
                CloseInlineItemRecursively(navMenuItem);
            }
        }
    }

    private void CloseInlineItemRecursively(NavMenuItem navMenuItem)
    {
        for (var i = 0; i < navMenuItem.ItemCount; i++)
        {
            var container = navMenuItem.ContainerFromIndex(i);
            if (container is NavMenuItem childNavMenuItem)
            {
                CloseInlineItemRecursively(childNavMenuItem);
            }
        }

        navMenuItem.CloseInlineItem(true);
        navMenuItem.IsSubMenuOpen = false;
    }

    internal void RaiseNavMenuItemClick(INavMenuItem navMenuItem)
    {
        RaiseEvent(new NavMenuItemClickEventArgs(NavMenuItemClickEvent, navMenuItem));
    }

    internal void RaiseNavMenuItemSelected(INavMenuItem navMenuItem)
    {
        RaiseEvent(new NavMenuItemSelectedEventArgs(NavMenuItemSelectedEvent, navMenuItem));
        SetCurrentValue(SelectedItemProperty, navMenuItem);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _menuItemsPresenter = e.NameScope.Find<ItemsPresenter>(NavMenuThemeConstants.ItemsPresenterPart);
        SetupMenuItemsPresenter();
        HandleModeChanged();
    }

    private void ConfigureDefaultOpenedPaths()
    {
        if (DefaultOpenPaths != null && !_defaultOpenPathsApplied)
        {
            foreach (var defaultOpenPath in DefaultOpenPaths)
            {
                var pathNodes = FindMenuItemByPath(defaultOpenPath);
                OpenMenuItemPaths(pathNodes);
            }

            _defaultOpenPathsApplied = true;
        }
    }

    private void ConfigureDefaultSelectedPath()
    {
        INavMenuItemData? targetItem = null;
        // 直接设置 SelectedItem 优先级高于 DefaultSelectedPath
        if (SelectedItem != null)
        {
            targetItem = SelectedItem;
        }
        else if (DefaultSelectedPath != null)
        { 
            var pathNodes = FindMenuItemByPath(DefaultSelectedPath);
            if (pathNodes.Count > 0)
            {
                targetItem = pathNodes.Last();
            }
        }

        if (targetItem != null)
        {
            SelectTargetItem(targetItem);
        }
    }

    private void SelectTargetItem(INavMenuItemData targetItem)
    {
        IList<INavMenuItemData>? pathNodes = null;
        
        if (targetItem.Children.Count > 0)
        {
            throw new InvalidOperationException("The selected item cannot have a submenu.");
        }
        pathNodes = new List<INavMenuItemData>();
        var current = targetItem;
        while (current != null)
        {
            pathNodes.Add(current);
            current = current.ParentNode as INavMenuItemData;
        }
        pathNodes = pathNodes.Reverse().ToList();
        
        if (pathNodes.Count > 0)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var navMenuItems = await OpenMenuItemPathAsync(pathNodes);
                if (InteractionHandler is not null)
                {
                    List<bool> oldFocusables = [];
                    foreach (var item in navMenuItems)
                    {
                        oldFocusables.Add(item.Focusable);
                        item.Focusable = false;
                    }
                    try
                    {
                        InteractionHandler.Select(navMenuItems.Last());
                    }
                    finally
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            for (var i = 0; i < oldFocusables.Count; i++)
                            {
                                var item         = navMenuItems[i];
                                var oldFocusable = oldFocusables[i];
                                item.Focusable = oldFocusable;
                            }
                        });
                    }
                }
            });
        }
    }

    private IList<INavMenuItemData> FindMenuItemByPath(TreeNodePath treeNodePath)
    {
        if (treeNodePath.Length == 0)
        {
            return [];
        }
        var                     segments  = treeNodePath.Segments;
        IList<INavMenuItemData> items     = Items.OfType<INavMenuItemData>().ToList();
        IList<INavMenuItemData> pathNodes = new List<INavMenuItemData>();
        foreach (var segment in segments)
        {
            bool childFound = false;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.ItemKey != null && item.ItemKey.Value == segment)
                {
                    items      = item.Children;
                    childFound = true;
                    pathNodes.Add(item);
                    break;
                }
            }

            if (!childFound)
            {
                return [];
            }
        }
        return pathNodes;
    }

    private void OpenMenuItemPaths(IList<INavMenuItemData> pathNodes)
    {
        if (pathNodes.Count == 0)
        {
            return;
        }

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await OpenMenuItemPathAsync(pathNodes);
        });
    }

    private async Task<List<NavMenuItem>> OpenMenuItemPathAsync(IList<INavMenuItemData> pathNodes)
    {
        List<NavMenuItem> items = new List<NavMenuItem>();
        try
        {
            ItemsControl current = this;
            foreach (var pathNode in pathNodes)
            {
                var child = await GetNavMenuItemContainerAsync(pathNode, current);
            
                if (child != null)
                {
                    items.Add(child);
                    current               = child;
                    child.IsMotionEnabled = false;
                    child.IsSubMenuOpen   = true;
                }
            }
        }
        finally
        {
            foreach (var item in items)
            {
                item.IsMotionEnabled = true;
            }
        }

        return items;
    }

    private async Task<NavMenuItem?> GetNavMenuItemContainerAsync(INavMenuItemData childNode, ItemsControl current)
    {
        var          cycleCount = 10;
        NavMenuItem? target     = null;
        while (cycleCount > 0)
        {
            target = current.ContainerFromItem(childNode) as NavMenuItem;
            if (target == null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
            else
            {
                break;
            }
            --cycleCount;
        }
        return target;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureDefaultOpenedPaths();
        ConfigureDefaultSelectedPath();
    }
}