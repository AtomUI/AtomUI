using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(NavMenuPseudoClass.InlineMode,
    NavMenuPseudoClass.HorizontalMode,
    NavMenuPseudoClass.VerticalMode,
    NavMenuPseudoClass.DarkStyle,
    NavMenuPseudoClass.LightStyle)]
public class NavMenu : ItemsControl,
                       IFocusScope,
                       INavMenu,
                       IMotionAwareControl,
                       IControlSharedTokenResourcesHost,
                       IMenuChildSelectable
{
    #region 公共属性定义
    public static readonly DirectProperty<NavMenu, INavMenuNode?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, INavMenuNode?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    
    public static readonly DirectProperty<NavMenu, TreeNodePath?> DefaultSelectedPathProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, TreeNodePath?>(
            nameof(DefaultSelectedPath),
            o => o.DefaultSelectedPath,
            (o, v) => o.DefaultSelectedPath = v);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NavMenu>();
    
    public static readonly StyledProperty<bool> IsAccordionModeProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsAccordionMode), false);
    
    public static readonly StyledProperty<NavMenuMode> ModeProperty =
        AvaloniaProperty.Register<NavMenu, NavMenuMode>(nameof(Mode), NavMenuMode.Inline);
    
    public static readonly StyledProperty<bool> IsDarkStyleProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsDarkStyle), false);
    
    public static readonly DirectProperty<NavMenu, IList<TreeNodePath>?> DefaultOpenPathsProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, IList<TreeNodePath>?>(
            nameof(DefaultOpenPaths),
            o => o.DefaultOpenPaths,
            (o, v) => o.DefaultOpenPaths = v);
    
    public static readonly StyledProperty<bool> ShouldUseOverlayLayerProperty = 
        AvaloniaProperty.Register<NavMenu, bool>(nameof (ShouldUseOverlayLayer));
    
    public INavMenuNode? _selectedItem;

    public INavMenuNode? SelectedItem
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
    
    public bool ShouldUseOverlayLayer
    {
        get => GetValue(ShouldUseOverlayLayerProperty);
        set => SetValue(ShouldUseOverlayLayerProperty, value);
    }
    #endregion
    
    #region 公共事件定义

    public static readonly RoutedEvent<NavMenuItemClickEventArgs> NavMenuItemClickEvent =
        RoutedEvent.Register<NavMenu, NavMenuItemClickEventArgs>(nameof(NavMenuItemClick), RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<NavMenuNodeSelectedEventArgs> NavMenuNodeSelectedEvent =
        RoutedEvent.Register<NavMenu, NavMenuNodeSelectedEventArgs>(nameof(NavMenuNodeSelected), RoutingStrategies.Bubble);

    public event EventHandler<NavMenuItemClickEventArgs>? NavMenuItemClick
    {
        add => AddHandler(NavMenuItemClickEvent, value);
        remove => RemoveHandler(NavMenuItemClickEvent, value);
    }
    
    public event EventHandler<NavMenuNodeSelectedEventArgs>? NavMenuNodeSelected
    {
        add => AddHandler(NavMenuNodeSelectedEvent, value);
        remove => RemoveHandler(NavMenuNodeSelectedEvent, value);
    }
    
    #endregion
    
    #region 内部属性定义
    
    IRenderRoot? INavMenu.VisualRoot => VisualRoot;

    IEnumerable<INavMenuItem> INavMenuElement.SubItems => LogicalChildren.OfType<INavMenuItem>();
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => NavMenuToken.ID;
    
    #endregion
    
    internal INavMenuInteractionHandler? InteractionHandler { get; private set; }
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel { Orientation = Orientation.Vertical });
    
    private readonly Dictionary<NavMenuItem, CompositeDisposable> _itemsBindingDisposables = new();
    private bool _defaultOpenPathsApplied;
    private int _motionContextLevel;
    private bool _originIsMotionEnabled;
    
    static NavMenu()
    {
        ItemsPanelProperty.OverrideDefaultValue(typeof(NavMenu), DefaultPanel);
        KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue(
            typeof(NavMenu),
            KeyboardNavigationMode.Once);
        AutomationProperties.AccessibilityViewProperty.OverrideDefaultValue<NavMenu>(AccessibilityView.Control);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<NavMenu>(AutomationControlType.Menu);
        NavMenuItem.SubmenuOpenedEvent.AddClassHandler<NavMenu>((navMenu, e) => navMenu.NotifySubmenuOpened(e));
        NavMenu.ModeProperty.Changed.AddClassHandler<NavMenu>((navMenu, e) => navMenu.HandleModeChanged());
    }
    
    public NavMenu()
    {
        this.RegisterResources();
        UpdatePseudoClasses();
        LogicalChildren.CollectionChanged += HandleChildrenChanged;
        Items.CollectionChanged           += HandleItemsViewCollectionChanged;
    }
    
    private void HandleChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
    
    private protected virtual void HandleItemsViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!Items.IsReadOnly)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (item is not INavMenuNode)
                            {
                                throw new InvalidOperationException("The item does not implement the INavMenuNode interface.");
                            }
                        }
                    }
                    break;
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
                foreach (var child in LogicalChildren.OfType<NavMenuItem>())
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

        if (change.Property == SelectedItemProperty)
        {
            if (SelectedItem != null)
            {
                SelectTargetMenuNode(SelectedItem);
            }
        }
    }
    
    private void HandleModeChanged()
    {
        Close();
        ConfigureInteractionHandler(true);
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<NavMenuItem>(item, out recycleKey);
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new NavMenuItem();
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is NavMenuItem menuItem)
        {
            menuItem.OwnerMenu = this;
            var disposables = new CompositeDisposable(6);

            {
                if (item is INavMenuNode menuNode)
                {
                    menuItem.SetCurrentValue(NavMenuItem.HeaderProperty, menuNode);
                    disposables.Add(BindUtils.RelayBind(menuNode, nameof(INavMenuNode.Icon), menuItem, NavMenuItem.IconProperty));
                    disposables.Add(BindUtils.RelayBind(menuNode, nameof(INavMenuNode.IsEnabled), menuItem, NavMenuItem.IsEnabledProperty));
                    disposables.Add(BindUtils.RelayBind(menuNode, nameof(INavMenuNode.Command), menuItem, NavMenuItem.CommandProperty));
                    disposables.Add(BindUtils.RelayBind(menuNode, nameof(INavMenuNode.CommandParameter), menuItem, NavMenuItem.CommandParameterProperty));
                    menuItem.ItemKey = menuNode.ItemKey;
                }
            }

            {
                if (item is INavMenuNode menuNode && menuNode.HeaderTemplate != null)
                {
                    disposables.Add(BindUtils.RelayBind(menuNode, nameof(INavMenuNode.HeaderTemplate), menuItem, NavMenuItem.HeaderTemplateProperty));
                }
                else if (ItemTemplate != null)
                {
                    disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, menuItem, NavMenuItem.HeaderTemplateProperty));
                }
            }
            
            disposables.Add(BindUtils.RelayBind(this, ModeProperty, menuItem, NavMenuItem.ModeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsDarkStyleProperty, menuItem, NavMenuItem.IsDarkStyleProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, menuItem, NavMenuItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, ShouldUseOverlayLayerProperty, menuItem, NavMenuItem.ShouldUseOverlayLayerProperty));
            
            PrepareNavMenuItem(menuItem, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(menuItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(menuItem);
            }
            _itemsBindingDisposables.Add(menuItem, disposables);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type NavMenuItem.");
        }
    }
    
    internal virtual void PrepareNavMenuItem(NavMenuItem menuItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }
    
    private void ConfigureInteractionHandler(bool needMount = false)
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
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(NavMenuPseudoClass.HorizontalMode, Mode == NavMenuMode.Horizontal);
        PseudoClasses.Set(NavMenuPseudoClass.VerticalMode, Mode == NavMenuMode.Vertical);
        PseudoClasses.Set(NavMenuPseudoClass.InlineMode, Mode == NavMenuMode.Inline);
        PseudoClasses.Set(NavMenuPseudoClass.DarkStyle, IsDarkStyle);
        PseudoClasses.Set(NavMenuPseudoClass.LightStyle, !IsDarkStyle);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        HandleModeChanged();
    }
    
    protected virtual void NotifySubmenuOpened(RoutedEventArgs e)
    {
        if (IsAccordionMode)
        {
            if (e.Source is INavMenuItem menuItem && menuItem.Parent == this)
            {
                foreach (var child in this.GetLogicalChildren().OfType<INavMenuItem>())
                {
                    if (child != menuItem && child.IsSubMenuOpen)
                    {
                        child.IsSubMenuOpen = false;
                    }
                }
            }
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureDefaultOpenedPaths();
        ConfigureDefaultSelectedPath();
    }
    
    private void ConfigureDefaultOpenedPaths()
    {
        if (DefaultOpenPaths != null && !_defaultOpenPathsApplied)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                foreach (var defaultOpenPath in DefaultOpenPaths)
                {
                    await TraverNavMenuPathAsync(defaultOpenPath);
                }
                _defaultOpenPathsApplied = true;
            });
        }
    }
    
    private void ConfigureDefaultSelectedPath()
    {
        // 直接设置 SelectedItem 优先级高于 DefaultSelectedPath
        if (SelectedItem != null)
        {
            SelectTargetMenuNode(SelectedItem);
        }
        else if (DefaultSelectedPath != null)
        {
            Dispatcher.UIThread.InvokeAsync(async () => await TraverNavMenuPathAsync(DefaultSelectedPath, (menuItem, i) =>
            {
                if (i == DefaultSelectedPath.Length - 1)
                {
                    InteractionHandler?.Select(menuItem);
                }
            }));
        }
    }

    private void SelectTargetMenuNode(INavMenuNode node)
    {
        var selectPathNodes = CollectPathNodes(node);
        if (selectPathNodes.Count > 0)
        {
            Dispatcher.UIThread.InvokeAsync(async () => await TraverNavMenuPathAsync(selectPathNodes, (menuItem, i) =>
            {
                if (i == selectPathNodes.Count - 1)
                {
                    InteractionHandler?.Select(menuItem);
                }
            }));
        }
    }

    public void Close()
    {
        foreach (var i in ((INavMenu)this).SubItems)
        {
            i.Close();
        }
        
        SelectedItem = null;
    }
    
    internal void RaiseNavMenuItemClick(INavMenuItem menuItem)
    {
        RaiseEvent(new NavMenuItemClickEventArgs(NavMenuItemClickEvent, menuItem));
    }
    
    internal void RaiseNavMenuItemSelected(NavMenuItem menuItem)
    {
        var node = (menuItem as INavMenuItem).Node;
        Debug.Assert(node != null);
        RaiseEvent(new NavMenuNodeSelectedEventArgs(NavMenuNodeSelectedEvent, node));
        SetCurrentValue(SelectedItemProperty, node);
    }

    internal static List<NavMenuItem> CollectSelectPathItems(NavMenuItem menuItem)
    {
        var          items   = new List<NavMenuItem>();
        NavMenuItem? current = menuItem;
        while (current != null)
        {
            if (current != menuItem)
            {
                items.Add(current);
            }
            current = current.GetLogicalParent<NavMenuItem>();
        }

        items.Reverse();
        return items;
    }
    
    void IMenuChildSelectable.SelectChildItem(NavMenuItem child, bool isSelected)
    {
        if (child.IsTopLevel)
        {
            child.SetCurrentValue(NavMenuItem.IsSelectedProperty, isSelected);
        }
    }
    
    private async Task<List<NavMenuItem>?> TraverNavMenuPathAsync(TreeNodePath treeNodePath, Action<NavMenuItem, int>? action = null)
    {
        if (treeNodePath.Length == 0)
        {
            return null;
        }
        try
        {
            EnterDisableMotionRegion();
            var          segments     = treeNodePath.Segments;
            IList        items        = Items.ToList();
            var          pathNodes    = new List<NavMenuItem>();
            NavMenuItem? previousItem = null;
            for (int i = 0; i < segments.Count; i++)
            {
                var  segment    = segments[i];
                bool childFound = false;
                for (var j = 0; j < items.Count; j++)
                {
                    if (items[j] is INavMenuNode item)
                    {
                        var navMenuItem = await (previousItem != null 
                            ? GetNavMenuItemContainerAsync(item, previousItem) 
                            : GetNavMenuItemContainerAsync(item, this));
                        if (navMenuItem == null)
                        {
                            return null;
                        }

                        if (navMenuItem.ItemKey != null && navMenuItem.ItemKey.Value == segment)
                        {
                            navMenuItem.SetCurrentValue(NavMenuItem.IsSubMenuOpenProperty, true);
                            items      = navMenuItem.Items;
                            childFound = true;
                            pathNodes.Add(navMenuItem);
                            action?.Invoke(navMenuItem, i);
                            previousItem = navMenuItem;
                            break;
                        }
                    }
                }

                if (!childFound)
                {
                    return null;
                }
            }

            return pathNodes;
        }
        finally
        {
            ExitDisableMotionRegion();
        }
    }
    
    private async Task<List<NavMenuItem>?> TraverNavMenuPathAsync(List<INavMenuNode> pathNodes, Action<NavMenuItem, int>? action = null)
    {
        if (pathNodes.Count == 0)
        {
            return null;
        }
        try
        {
            EnterDisableMotionRegion();
            IList        items        = Items.ToList();
            var          pathItems    = new List<NavMenuItem>();
            NavMenuItem? previousItem = null;
            for (int i = 0; i < pathNodes.Count; i++)
            {
                var  currentNode = pathNodes[i];
                bool childFound  = false;
                for (var j = 0; j < items.Count; j++)
                {
                    if (items[j] is INavMenuNode node)
                    {
                        var navMenuItem = await (previousItem != null 
                            ? GetNavMenuItemContainerAsync(node, previousItem) 
                            : GetNavMenuItemContainerAsync(node, this));
                        if (navMenuItem == null)
                        {
                            return null;
                        }

                        if (node == currentNode)
                        {
                            navMenuItem.SetCurrentValue(NavMenuItem.IsSubMenuOpenProperty, true);
                            items      = navMenuItem.Items;
                            childFound = true;
                            pathItems.Add(navMenuItem);
                            action?.Invoke(navMenuItem, i);
                            previousItem = navMenuItem;
                            break;
                        }
                    }
                }

                if (!childFound)
                {
                    return null;
                }
            }

            return pathItems;
        }
        finally
        {
            ExitDisableMotionRegion();
        }
    }

    private void EnterDisableMotionRegion()
    {
        if (_motionContextLevel == 0)
        {
            _originIsMotionEnabled = IsMotionEnabled;
            SetCurrentValue(IsMotionEnabledProperty, false);
        }

        ++_motionContextLevel;
    }

    private void ExitDisableMotionRegion()
    {
        --_motionContextLevel;
        if (_motionContextLevel == 0)
        {
            SetCurrentValue(IsMotionEnabledProperty, _originIsMotionEnabled);
        }
    }

    private List<INavMenuNode> CollectPathNodes(INavMenuNode node)
    {
        var pathNodes  = new List<INavMenuNode>();

        if (Items.Count > 0)
        {
            var current  = node;
            while (current != null)
            {
                pathNodes.Add(current);
                current = current.ParentNode as INavMenuNode;
            }
            pathNodes.Reverse();
        
            Debug.Assert(pathNodes.Count > 0);
            // 检查是否是野数据
            var rootNode  = pathNodes.First();
            var foundRoot = false;
            foreach (var root in Items)
            {
                if (rootNode == root)
                {
                    foundRoot = true;
                    break;
                }
            }
            if (!foundRoot || node != pathNodes[^1])
            {
                throw new ArgumentOutOfRangeException(nameof(node), "Wild INavMenuNode, Only part of the path was found");
            }
        }
        
        return pathNodes;
    }
    
    private async Task<NavMenuItem?> GetNavMenuItemContainerAsync(INavMenuNode childNode, ItemsControl current)
    {
        var          cycleCount = 10;
        NavMenuItem? target     = null;
        target = current.ContainerFromItem(childNode) as NavMenuItem;
        if (target != null)
        {
            return target;
        }
        if (current.Presenter?.Panel == null &&
            this.GetVisualRoot() is ILayoutRoot visualRoot)
        {
            var layoutManager = visualRoot.GetLayoutManager();
            layoutManager.ExecuteLayoutPass();
        }
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
}