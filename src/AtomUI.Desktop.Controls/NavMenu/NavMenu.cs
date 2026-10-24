using System.Collections.Specialized;
using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(NavMenuPseudoClass.InlineMode,
    NavMenuPseudoClass.InlineCollapsed,
    NavMenuPseudoClass.HorizontalMode,
    NavMenuPseudoClass.VerticalMode,
    NavMenuPseudoClass.DarkStyle,
    NavMenuPseudoClass.LightStyle)]
public partial class NavMenu : ItemsControl,
                       IFocusScope,
                       INavMenu,
                       IMotionAwareControl,
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

    public static readonly StyledProperty<bool> IsInlineCollapsedProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsInlineCollapsed));

    public static readonly StyledProperty<double> InlineCollapsedWidthProperty =
        AvaloniaProperty.Register<NavMenu, double>(nameof(InlineCollapsedWidth));

    public static readonly StyledProperty<bool> IsCollapsedTooltipEnabledProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsCollapsedTooltipEnabled), true);

    public static readonly StyledProperty<PlacementMode> CollapsedTooltipPlacementProperty =
        AvaloniaProperty.Register<NavMenu, PlacementMode>(
            nameof(CollapsedTooltipPlacement),
            PlacementMode.Right);

    public static readonly StyledProperty<int> CollapsedTooltipShowDelayProperty =
        AvaloniaProperty.Register<NavMenu, int>(nameof(CollapsedTooltipShowDelay), 400);

    public static readonly StyledProperty<int> CollapsedTooltipBetweenShowDelayProperty =
        AvaloniaProperty.Register<NavMenu, int>(nameof(CollapsedTooltipBetweenShowDelay), 100);

    public static readonly StyledProperty<bool> IsDarkStyleProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsDarkStyle), false);

    public static readonly StyledProperty<bool> IsItemBackgroundEnabledProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsItemBackgroundEnabled), true);

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<NavMenu, object?>(nameof(Header));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<NavMenu, IDataTemplate?>(nameof(HeaderTemplate));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<NavMenu, object?>(nameof(Footer));

    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<NavMenu, IDataTemplate?>(nameof(FooterTemplate));

    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<NavMenu, double>(nameof(ItemSpacing), inherits: true);

    public static readonly DirectProperty<NavMenu, IList<TreeNodePath>?> DefaultOpenPathsProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, IList<TreeNodePath>?>(
            nameof(DefaultOpenPaths),
            o => o.DefaultOpenPaths,
            (o, v) => o.DefaultOpenPaths = v);

    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof (ShouldUseOverlayPopup));

    /// <summary>
    /// Gets or sets a value indicating whether the submenu popup remains open without requiring
    /// user interaction. Gallery semantic previews pin the popup open this way so the popup
    /// parts can be inspected and highlighted.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<NavMenu>();

    private INavMenuNode? _selectedItem;

    public INavMenuNode? SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
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

    public bool IsInlineCollapsed
    {
        get => GetValue(IsInlineCollapsedProperty);
        set => SetValue(IsInlineCollapsedProperty, value);
    }

    public double InlineCollapsedWidth
    {
        get => GetValue(InlineCollapsedWidthProperty);
        set => SetValue(InlineCollapsedWidthProperty, value);
    }

    public bool IsCollapsedTooltipEnabled
    {
        get => GetValue(IsCollapsedTooltipEnabledProperty);
        set => SetValue(IsCollapsedTooltipEnabledProperty, value);
    }

    public PlacementMode CollapsedTooltipPlacement
    {
        get => GetValue(CollapsedTooltipPlacementProperty);
        set => SetValue(CollapsedTooltipPlacementProperty, value);
    }

    public int CollapsedTooltipShowDelay
    {
        get => GetValue(CollapsedTooltipShowDelayProperty);
        set => SetValue(CollapsedTooltipShowDelayProperty, value);
    }

    public int CollapsedTooltipBetweenShowDelay
    {
        get => GetValue(CollapsedTooltipBetweenShowDelayProperty);
        set => SetValue(CollapsedTooltipBetweenShowDelayProperty, value);
    }

    public bool IsDarkStyle
    {
        get => GetValue(IsDarkStyleProperty);
        set => SetValue(IsDarkStyleProperty, value);
    }

    public bool IsItemBackgroundEnabled
    {
        get => GetValue(IsItemBackgroundEnabledProperty);
        set => SetValue(IsItemBackgroundEnabledProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    private IList<TreeNodePath>? _defaultOpenPaths;

    public IList<TreeNodePath>? DefaultOpenPaths
    {
        get => _defaultOpenPaths;
        set => SetAndRaise(DefaultOpenPathsProperty, ref _defaultOpenPaths, value);
    }

    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the submenu popup remains open without requiring
    /// user interaction. Gallery semantic previews pin the popup open this way so the popup
    /// parts can be inspected and highlighted.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
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

    internal static readonly DirectProperty<NavMenu, NavMenuMode> EffectiveModeProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, NavMenuMode>(
            nameof(EffectiveMode),
            o => o.EffectiveMode);

    internal static readonly DirectProperty<NavMenu, bool> IsEffectiveInlineCollapsedProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, bool>(
            nameof(IsEffectiveInlineCollapsed),
            o => o.IsEffectiveInlineCollapsed);

    internal static readonly StyledProperty<double> InlineCollapsedLayoutWidthProperty =
        AvaloniaProperty.Register<NavMenu, double>(
            nameof(InlineCollapsedLayoutWidth),
            double.NaN);

    internal static readonly StyledProperty<double> EntryItemSpacingProperty =
        AvaloniaProperty.Register<NavMenu, double>(
            nameof(EntryItemSpacing),
            inherits: true);

    private NavMenuMode _effectiveMode = NavMenuMode.Inline;

    internal NavMenuMode EffectiveMode
    {
        get => _effectiveMode;
        private set => SetAndRaise(EffectiveModeProperty, ref _effectiveMode, value);
    }

    private bool _isEffectiveInlineCollapsed;

    internal bool IsEffectiveInlineCollapsed
    {
        get => _isEffectiveInlineCollapsed;
        private set => SetAndRaise(IsEffectiveInlineCollapsedProperty, ref _isEffectiveInlineCollapsed, value);
    }

    internal double InlineCollapsedLayoutWidth
    {
        get => GetValue(InlineCollapsedLayoutWidthProperty);
        private set => SetValue(InlineCollapsedLayoutWidthProperty, value);
    }

    internal double EntryItemSpacing
    {
        get => GetValue(EntryItemSpacingProperty);
        private set => SetValue(EntryItemSpacingProperty, value);
    }

    #endregion

    #region 内部协作 API

    internal INavMenuInteractionHandler? InteractionHandler { get; private set; }

    internal virtual void PrepareNavMenuItem(NavMenuItem menuItem, object? item, int index)
    {
    }

    internal void ApplySelectionStateToPreparedContainer(NavMenuItem menuItem)
    {
        _selectionCoordinator.PrepareContainer(this, menuItem);
    }

    /// <summary>
    /// 容器 prepare 阶段的钉住不变量。钉住请求以节点为键保存在
    /// <see cref="NavMenuPinnedOpenCoordinator"/> 中，因此声明式写法（AXAML 在容器生成前写入
    /// <c>IsPopupPinnedOpen="True"</c>）以及 Mode 切换后的容器重建，都会在容器实现时按请求补齐，
    /// 与选择状态走同一容器时序契约。
    /// </summary>
    internal void ApplyPinnedOpenStateToPreparedContainer(NavMenuItem menuItem)
    {
        _pinnedOpenCoordinator.PrepareContainer(this, menuItem);
    }

    internal void ForgetGeneratedContainer(NavMenuItem menuItem)
    {
        _pinnedOpenCoordinator.ForgetContainer(this, menuItem);
        _selectionCoordinator.Forget(menuItem);
        InteractionHandler?.Forget(menuItem);
        if (IsPopupPinnedOpen)
        {
            // 容器移除后钉住路径可能整体失效，条目图收敛后再求值一次。
            _pinnedOpenCoordinator.QueueReconcile(this);
        }
    }

    internal bool SelectNavMenuItem(NavMenuItem menuItem)
    {
        return _selectionCoordinator.Select(this, menuItem);
    }

    internal void ClearSelectionState()
    {
        _selectionCoordinator.ClearSelection(this);
    }

    internal void CloseOtherTopLevelSubmenus(INavMenuItem menuItem)
    {
        if (IsAccordionMode && menuItem.Parent == this)
        {
            foreach (var child in NavMenuSemanticNavigator.EnumerateDirectItems(this))
            {
                if (child != menuItem && child.IsSubMenuOpen)
                {
                    child.IsSubMenuOpen = false;
                }
            }
        }
    }

    internal NavMenuItem? FindRealizedMenuItem(INavMenuNode node)
    {
        var pathNodes = CollectPathNodes(node);
        if (pathNodes.Count == 0)
        {
            return null;
        }

        ItemsControl current = this;
        for (var i = 0; i < pathNodes.Count; i++)
        {
            var menuItem = NavMenuSemanticNavigator.FindDirectItem(current, pathNodes[i]);
            if (menuItem is null)
            {
                return null;
            }

            if (i == pathNodes.Count - 1)
            {
                return menuItem;
            }

            current = menuItem;
        }

        return null;
    }

    internal void RaiseNavMenuItemClick(INavMenuItem menuItem)
    {
        RaiseEvent(new NavMenuItemClickEventArgs(NavMenuItemClickEvent, menuItem));
    }

    internal bool TryPublishNavMenuItemSelection(NavMenuItem menuItem)
    {
        var node = (menuItem as INavMenuItem).Node;
        Debug.Assert(node != null);
        SetCurrentValue(SelectedItemProperty, node);
        if (!ReferenceEquals(SelectedItem, node))
        {
            return false;
        }

        RaiseEvent(new NavMenuNodeSelectedEventArgs(NavMenuNodeSelectedEvent, node));
        return ReferenceEquals(SelectedItem, node);
    }

    internal static List<NavMenuItem> CollectSelectPathItems(NavMenuItem menuItem)
    {
        var itemCount = CountSelectPathItems(menuItem);
        var items     = new List<NavMenuItem>(itemCount);
        for (var i = 0; i < itemCount; i++)
        {
            items.Add(null!);
        }

        var current = menuItem.SemanticParentItem;
        for (var i = itemCount - 1; current != null; i--)
        {
            items[i] = current;
            current  = current.SemanticParentItem;
        }
        return items;
    }

    internal static HashSet<NavMenuItem> BuildSelectPathSet(IReadOnlyCollection<NavMenuItem> items)
    {
        var itemSet = new HashSet<NavMenuItem>(items.Count);
        foreach (var item in items)
        {
            itemSet.Add(item);
        }

        return itemSet;
    }

    internal void ExecutePendingContainerLayout(ItemsControl current)
    {
        current.ApplyTemplate();

        if (current.Presenter is { Panel: null } presenter)
        {
            presenter.ApplyTemplate();
        }

        if (current is NavMenuItem { Popup.Child: ILogical popupContent })
        {
            foreach (var descendant in popupContent.GetSelfAndLogicalDescendants())
            {
                if (descendant is ItemsPresenter popupPresenter &&
                    ReferenceEquals(popupPresenter.TemplatedParent, current))
                {
                    popupPresenter.ApplyTemplate();
                }
            }
        }

        if (current is NavMenuItem { Popup: { IsOpen: true, Child: { } popupChild } })
        {
            popupChild.UpdateLayout();
        }

        var topLevel = TopLevel.GetTopLevel(current);
        void ExecuteLayoutPass()
        {
            topLevel?.UpdateLayout();
        }

        ExecuteLayoutPass();
        NavMenuSemanticNavigator.EnsureGroupContainers(current, ExecuteLayoutPass);
    }

    #endregion

    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel { Orientation = Orientation.Vertical });

    private static readonly TimeSpan InlineCollapsedWidthMotionFrameInterval = TimeSpan.FromMilliseconds(16);

    private int _selectedItemRevision;

    private bool _defaultOpenPathsApplied;

    private int _motionContextLevel;

    private bool _originIsMotionEnabled;

    private List<IReadOnlyList<INavMenuNode>>? _inlineCollapsedOpenNodePathCache;

    private List<TreeNodePath>? _inlineCollapsedDefaultOpenPathCache;

    private CancellationTokenSource? _inlineCollapsedWidthMotionCancellationTokenSource;

    private readonly NavMenuSelectionCoordinator _selectionCoordinator = new();

    private readonly NavMenuPinnedOpenCoordinator _pinnedOpenCoordinator = new();

    private readonly NavMenuEntryOwnershipCoordinator _entryOwnershipCoordinator;

    private double _lastInlineExpandedWidth = double.NaN;

    static NavMenu()
    {
        WidthProperty.OverrideMetadata<NavMenu>(
            new StyledPropertyMetadata<double>(coerce: CoerceInlineCollapsedWidth));
        MinWidthProperty.OverrideMetadata<NavMenu>(
            new StyledPropertyMetadata<double>(coerce: CoerceInlineCollapsedMinWidth));
        ItemsPanelProperty.OverrideDefaultValue(typeof(NavMenu), DefaultPanel);
        KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue(
            typeof(NavMenu),
            KeyboardNavigationMode.Once);
        FocusableProperty.OverrideDefaultValue<NavMenu>(true);
        AutomationProperties.AccessibilityViewProperty.OverrideDefaultValue<NavMenu>(AccessibilityView.Control);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<NavMenu>(AutomationControlType.Menu);
        NavMenuItem.SubmenuOpenedEvent.AddClassHandler<NavMenu>((navMenu, e) => navMenu.NotifySubmenuOpened(e));
        NavMenu.ModeProperty.Changed.AddClassHandler<NavMenu>((navMenu, e) => navMenu.HandleModeChanged());
        NavMenu.IsInlineCollapsedProperty.Changed.AddClassHandler<NavMenu>((navMenu, e) => navMenu.HandleInlineCollapsedChanged());
        NavMenu.InlineCollapsedWidthProperty.Changed.AddClassHandler<NavMenu>(
            (navMenu, e) => navMenu.HandleInlineCollapsedWidthChanged());
        NavMenu.InlineCollapsedLayoutWidthProperty.Changed.AddClassHandler<NavMenu>(
            (navMenu, e) => navMenu.CoerceInlineCollapsedLayoutConstraints());
    }

    public NavMenu()
    {
        _entryOwnershipCoordinator = new NavMenuEntryOwnershipCoordinator(this, () => Items);
        UpdatePseudoClasses();
        Items.CollectionChanged += HandleItemsViewCollectionChanged;

        // 阻止弹出式子菜单内的元素把 BringIntoView 冒泡到外层 ScrollViewer，避免
        // 再次打开选中菜单项时触发页面滚动。Inline 模式下子菜单在视觉树内，
        // 让 NavMenu 自己的 ScrollViewer 正常响应即可。
        AddHandler(RequestBringIntoViewEvent, (_, e) =>
        {
            if (EffectiveMode == NavMenuMode.Inline || e.TargetObject == this)
            {
                return;
            }

            e.Handled = true;
        }, handledEventsToo: true);
    }

    private protected virtual void HandleItemsViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _entryOwnershipCoordinator.Synchronize();
        if (IsPopupPinnedOpen)
        {
            // 条目图变化会改变可钉住目标（例如声明式钉住先于条目填充写入）或使当前钉住节点失效。
            // 此刻容器与节点的映射尚未收敛，交由协调器合并调度，在 ItemsPresenter 收敛后再求值。
            _pinnedOpenCoordinator.QueueReconcile(this);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsAccordionModeProperty)
        {
            if (change.GetNewValue<bool>())
            {
                foreach (var child in NavMenuSemanticNavigator.EnumerateDirectItems(this))
                {
                    child.IsSubMenuOpen = false;
                }
            }
        }

        if (change.Property == ItemSpacingProperty)
        {
            if (change.Priority == BindingPriority.Unset)
            {
                ClearValue(EntryItemSpacingProperty);
            }
            else
            {
                EntryItemSpacing = change.GetNewValue<double>();
            }
        }
        if (change.Property == IsDarkStyleProperty ||
            change.Property == ModeProperty ||
            change.Property == IsInlineCollapsedProperty)
        {
            UpdatePseudoClasses();
        }

        if (change.Property == SelectedItemProperty)
        {
            _selectedItemRevision++;
            if (SelectedItem != null)
            {
                if (!IsSelectedNodeAlreadyApplied(SelectedItem))
                {
                    SelectTargetMenuNode(SelectedItem, _selectedItemRevision);
                }
            }
            else
            {
                ClearSelectionState();
            }
        }
        else if (change.Property == IsMotionEnabledProperty &&
                 !change.GetNewValue<bool>())
        {
            CancelInlineCollapsedWidthMotion();
            ClearInlineCollapsedLayoutWidth();
        }
        else if (change.Property == IsPopupPinnedOpenProperty)
        {
            if (change.GetNewValue<bool>())
            {
                _pinnedOpenCoordinator.Reconcile(this);
            }
            else
            {
                // 取消钉住只解除关闭拦截：已打开的子菜单保持打开，只有该属性重新为 true 时
                // 才在容器 prepare / attach 时把请求补回来。
                _pinnedOpenCoordinator.Release();
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        CoerceInlineCollapsedLayoutConstraints();
        ConfigureInteractionHandler(true);
        _pinnedOpenCoordinator.Reconcile(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (IsPopupPinnedOpen)
        {
            _pinnedOpenCoordinator.Release();
            ClosePinnedForLifecycle();
        }

        base.OnDetachedFromVisualTree(e);
        CancelInlineCollapsedWidthMotion();
        ClearInlineCollapsedLayoutWidth();
        InteractionHandler?.Detach(this);
        InteractionHandler = null;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NavMenuEntryContainerCoordinator.NeedsContainer(this, item, index, out recycleKey);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return NavMenuEntryContainerCoordinator.CreateContainer(item, index, recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        NavMenuEntryContainerCoordinator.PrepareContainer(this, container, item, index);
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        NavMenuEntryContainerCoordinator.ClearContainer(this, container);
        base.ClearContainerForItemOverride(container);
    }

    protected virtual void NotifySubmenuOpened(RoutedEventArgs e)
    {
        if (IsPopupPinnedOpen &&
            e.Source is NavMenuItem openedItem &&
            ReferenceEquals(openedItem.OwnerMenu, this))
        {
            _pinnedOpenCoordinator.ExtendToOpenedItem(this, openedItem);
        }

        if (EffectiveMode != NavMenuMode.Inline && e.Source is INavMenuItem menuItem)
        {
            CloseOtherTopLevelSubmenus(menuItem);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureDefaultOpenedPaths();
        ConfigureDefaultSelectedPath();
    }

    public void Close()
    {
        if (IsPopupPinnedOpen)
        {
            return;
        }

        foreach (var menuItem in NavMenuSemanticNavigator.EnumerateDirectItems(this))
        {
            menuItem.Close();
        }

        SelectedItem = null;
    }

    #region 实现 INavMenuElement 接口

    IEnumerable<INavMenuItem> INavMenuElement.SubItems => EnumerateSubItems();

    void IMenuChildSelectable.SelectChildItem(NavMenuItem child, bool isSelected)
    {
        if (child.IsTopLevel)
        {
            child.SetCurrentValue(NavMenuItem.IsSelectedProperty, isSelected);
        }
    }

    #endregion

    private IEnumerable<INavMenuItem> EnumerateSubItems()
    {
        foreach (var child in NavMenuSemanticNavigator.EnumerateDirectItems(this))
        {
            yield return child;
        }
    }

    private void HandleModeChanged()
    {
        _pinnedOpenCoordinator.Release();
        CloseOpenSubmenusPreservingSelection(this);
        UpdateEffectiveMode();
        ConfigureInteractionHandler(true);
        _pinnedOpenCoordinator.Reconcile(this);
    }

    private void HandleInlineCollapsedChanged()
    {
        if (Mode != NavMenuMode.Inline)
        {
            UpdateEffectiveMode();
            ConfigureInteractionHandler(true);
            return;
        }

        if (IsInlineCollapsed)
        {
            CollapseInlineMode();
        }
        else
        {
            ExpandInlineMode();
        }

        // inline collapsed 会把 EffectiveMode 在 Inline 与 Vertical 之间切换，钉住请求必须
        // 跟随该有效模式收敛：折叠时补上钉住，展开回 Inline 时释放。
        _pinnedOpenCoordinator.Reconcile(this);
    }

    private void UpdateEffectiveMode()
    {
        var isEffectiveInlineCollapsed = Mode == NavMenuMode.Inline && IsInlineCollapsed;
        IsEffectiveInlineCollapsed = isEffectiveInlineCollapsed;
        EffectiveMode              = isEffectiveInlineCollapsed ? NavMenuMode.Vertical : Mode;
        CoerceInlineCollapsedLayoutConstraints();
        UpdatePseudoClasses();
    }

    private void ClosePinnedForLifecycle()
    {
        foreach (var menuItem in NavMenuSemanticNavigator.EnumerateDirectItems(this).OfType<NavMenuItem>())
        {
            menuItem.CloseForLifecycle();
        }
    }

    private void ConfigureInteractionHandler(bool needMount = false)
    {
        if (needMount)
        {
            InteractionHandler?.Detach(this);
        }

        if (EffectiveMode == NavMenuMode.Inline)
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

    private static double CoerceInlineCollapsedWidth(AvaloniaObject instance, double value)
    {
        if (instance is not NavMenu navMenu)
        {
            return value;
        }

        if (navMenu.TryGetInlineCollapsedLayoutWidth(out var layoutWidth))
        {
            return layoutWidth;
        }

        return navMenu.IsEffectiveInlineCollapsed
            ? NormalizeInlineCollapsedWidth(navMenu.InlineCollapsedWidth)
            : value;
    }

    private static double CoerceInlineCollapsedMinWidth(AvaloniaObject instance, double value)
    {
        if (instance is not NavMenu navMenu)
        {
            return value;
        }

        if (navMenu.TryGetInlineCollapsedLayoutWidth(out var layoutWidth))
        {
            return Math.Min(value, layoutWidth);
        }

        return navMenu.IsEffectiveInlineCollapsed
            ? Math.Min(value, NormalizeInlineCollapsedWidth(navMenu.InlineCollapsedWidth))
            : value;
    }

    private static double NormalizeInlineCollapsedWidth(double value)
    {
        return double.IsFinite(value) ? Math.Max(0, value) : 0;
    }

    private void CoerceInlineCollapsedLayoutConstraints()
    {
        CoerceValue(WidthProperty);
        CoerceValue(MinWidthProperty);
    }

    private bool TryGetInlineCollapsedLayoutWidth(out double layoutWidth)
    {
        layoutWidth = InlineCollapsedLayoutWidth;
        if (!double.IsFinite(layoutWidth))
        {
            return false;
        }

        layoutWidth = NormalizeInlineCollapsedWidth(layoutWidth);
        return true;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(NavMenuPseudoClass.HorizontalMode, Mode == NavMenuMode.Horizontal);
        PseudoClasses.Set(NavMenuPseudoClass.VerticalMode, Mode == NavMenuMode.Vertical);
        PseudoClasses.Set(NavMenuPseudoClass.InlineMode, Mode == NavMenuMode.Inline);
        PseudoClasses.Set(NavMenuPseudoClass.InlineCollapsed, IsEffectiveInlineCollapsed);
        PseudoClasses.Set(NavMenuPseudoClass.DarkStyle, IsDarkStyle);
        PseudoClasses.Set(NavMenuPseudoClass.LightStyle, !IsDarkStyle);
    }

    private void CollapseInlineMode()
    {
        var shouldAnimateWidth = PrepareInlineCollapsedWidthMotion(true, out var motionStartWidth, out var motionTargetWidth);
        _inlineCollapsedOpenNodePathCache = CollectOpenInlineNodePaths();
        CloseOpenSubmenusPreservingSelection(this);
        UpdateEffectiveMode();
        ConfigureInteractionHandler(true);
        StartPreparedInlineCollapsedWidthMotion(shouldAnimateWidth, motionStartWidth, motionTargetWidth);
    }

    private void ExpandInlineMode()
    {
        var shouldAnimateWidth = PrepareInlineCollapsedWidthMotion(false, out var motionStartWidth, out var motionTargetWidth);
        CloseOpenSubmenusPreservingSelection(this);
        UpdateEffectiveMode();
        ConfigureInteractionHandler(true);
        RestoreInlineCollapsedOpenPaths();
        StartPreparedInlineCollapsedWidthMotion(shouldAnimateWidth, motionStartWidth, motionTargetWidth);
    }

    private void HandleInlineCollapsedWidthChanged()
    {
        if (_inlineCollapsedWidthMotionCancellationTokenSource is not null)
        {
            CancelInlineCollapsedWidthMotion();
            ClearInlineCollapsedLayoutWidth();
        }

        CoerceInlineCollapsedLayoutConstraints();
    }

    private bool PrepareInlineCollapsedWidthMotion(bool collapse,
                                                   out double startWidth,
                                                   out double targetWidth)
    {
        CancelInlineCollapsedWidthMotion();

        startWidth  = NormalizeInlineCollapsedWidth(Bounds.Width);
        targetWidth = collapse
            ? NormalizeInlineCollapsedWidth(InlineCollapsedWidth)
            : ResolveExpandedWidthForInlineCollapsedMotion(startWidth);

        if (collapse && double.IsFinite(startWidth))
        {
            _lastInlineExpandedWidth = startWidth;
        }

        if (!CanRunInlineCollapsedWidthMotion(startWidth, targetWidth))
        {
            ClearInlineCollapsedLayoutWidth();
            return false;
        }

        InlineCollapsedLayoutWidth = startWidth;
        CoerceInlineCollapsedLayoutConstraints();
        return true;
    }

    private bool CanRunInlineCollapsedWidthMotion(double startWidth, double targetWidth)
    {
        return IsLoaded &&
               IsMotionEnabled &&
               double.IsFinite(startWidth) &&
               double.IsFinite(targetWidth) &&
               !MathUtils.AreClose(startWidth, targetWidth);
    }

    private double ResolveExpandedWidthForInlineCollapsedMotion(double fallbackWidth)
    {
        var baseWidth = GetBaseValue(WidthProperty);
        if (baseWidth.HasValue &&
            baseWidth.Value is double width &&
            double.IsFinite(width))
        {
            return NormalizeInlineCollapsedWidth(width);
        }

        if (double.IsFinite(_lastInlineExpandedWidth))
        {
            return NormalizeInlineCollapsedWidth(_lastInlineExpandedWidth);
        }

        return fallbackWidth;
    }

    private void StartPreparedInlineCollapsedWidthMotion(bool shouldAnimate,
                                                         double startWidth,
                                                         double targetWidth)
    {
        if (!shouldAnimate)
        {
            return;
        }

        var duration = ResolveInlineCollapsedWidthMotionDuration();
        if (duration <= TimeSpan.Zero)
        {
            InlineCollapsedLayoutWidth = targetWidth;
            ClearInlineCollapsedLayoutWidth();
            return;
        }

        var cancellationTokenSource = new CancellationTokenSource();
        _inlineCollapsedWidthMotionCancellationTokenSource = cancellationTokenSource;
        RunInlineCollapsedWidthMotion(startWidth, targetWidth, duration, cancellationTokenSource);
    }

    private void RunInlineCollapsedWidthMotion(double startWidth,
                                               double targetWidth,
                                               TimeSpan duration,
                                               CancellationTokenSource cancellationTokenSource)
    {
        // ControlTheme cannot calculate the measured start width or interrupted trajectory.
        // Tick on the UI dispatcher so unrelated thread-pool work cannot stall layout frames.
        var easing = new CubicEaseOut();
        var startTimestamp = Stopwatch.GetTimestamp();
        var timer = DispatcherTimer.Run(() =>
        {
            if (!ReferenceEquals(_inlineCollapsedWidthMotionCancellationTokenSource, cancellationTokenSource))
            {
                return false;
            }

            var progress = ResolveInlineCollapsedWidthMotionProgress(startTimestamp, duration);
            if (progress >= 1d)
            {
                CompleteInlineCollapsedWidthMotion(targetWidth, cancellationTokenSource);
                return false;
            }

            ApplyInlineCollapsedWidthMotionFrame(
                InterpolateInlineCollapsedWidth(startWidth, targetWidth, easing.Ease(progress)),
                cancellationTokenSource);
            return true;
        }, InlineCollapsedWidthMotionFrameInterval, DispatcherPriority.Normal);
        cancellationTokenSource.Token.Register(static subscription => ((IDisposable)subscription!).Dispose(), timer);
    }

    private static double ResolveInlineCollapsedWidthMotionProgress(long startTimestamp, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            return 1d;
        }

        var elapsedSeconds = (Stopwatch.GetTimestamp() - startTimestamp) / (double)Stopwatch.Frequency;
        return Math.Clamp(elapsedSeconds / duration.TotalSeconds, 0d, 1d);
    }

    private static double InterpolateInlineCollapsedWidth(double startWidth, double targetWidth, double progress)
    {
        return startWidth + (targetWidth - startWidth) * progress;
    }

    private void ApplyInlineCollapsedWidthMotionFrame(double width,
                                                      CancellationTokenSource cancellationTokenSource)
    {
        if (ReferenceEquals(_inlineCollapsedWidthMotionCancellationTokenSource, cancellationTokenSource))
        {
            InlineCollapsedLayoutWidth = width;
        }
    }

    private void CompleteInlineCollapsedWidthMotion(double targetWidth,
                                                    CancellationTokenSource cancellationTokenSource)
    {
        if (!ReferenceEquals(_inlineCollapsedWidthMotionCancellationTokenSource, cancellationTokenSource))
        {
            return;
        }

        _inlineCollapsedWidthMotionCancellationTokenSource = null;
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        InlineCollapsedLayoutWidth = targetWidth;
        ClearInlineCollapsedLayoutWidth();
    }

    private TimeSpan ResolveInlineCollapsedWidthMotionDuration()
    {
        var application = Application.Current;
        var themeVariant = application?.ActualThemeVariant;
        if (application?.TryGetResource(SharedTokenKind.MotionDurationMid, themeVariant, out var value) == true &&
            value is TimeSpan duration)
        {
            return duration;
        }

        return TimeSpan.FromMilliseconds(200);
    }

    private void CancelInlineCollapsedWidthMotion()
    {
        var cancellationTokenSource = _inlineCollapsedWidthMotionCancellationTokenSource;
        if (cancellationTokenSource is null)
        {
            return;
        }

        _inlineCollapsedWidthMotionCancellationTokenSource = null;
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }

    private void ClearInlineCollapsedLayoutWidth()
    {
        if (double.IsFinite(InlineCollapsedLayoutWidth))
        {
            InlineCollapsedLayoutWidth = double.NaN;
        }
        CoerceInlineCollapsedLayoutConstraints();
    }

    private List<IReadOnlyList<INavMenuNode>>? CollectOpenInlineNodePaths()
    {
        var paths = new List<IReadOnlyList<INavMenuNode>>();
        CollectOpenInlineNodePaths(this, new List<INavMenuNode>(), paths);
        return paths.Count > 0 ? paths : null;
    }

    private static void CollectOpenInlineNodePaths(
        ItemsControl owner,
        List<INavMenuNode> currentPath,
        List<IReadOnlyList<INavMenuNode>> openPaths)
    {
        foreach (var item in NavMenuSemanticNavigator.EnumerateDirectItems(owner))
        {
            if (((INavMenuItem)item).Node is not { } node)
            {
                continue;
            }

            currentPath.Add(node);
            if (item.HasSubMenu && item.IsSubMenuOpen)
            {
                openPaths.Add(currentPath.ToArray());
                CollectOpenInlineNodePaths(item, currentPath, openPaths);
            }

            currentPath.RemoveAt(currentPath.Count - 1);
        }
    }

    private static void CloseOpenSubmenusPreservingSelection(ItemsControl owner)
    {
        foreach (var item in NavMenuSemanticNavigator.EnumerateDirectItems(owner))
        {
            CloseOpenSubmenusPreservingSelection(item);
            item.SetCurrentValue(NavMenuItem.IsSubMenuOpenProperty, false);
        }
    }

    private void RestoreInlineCollapsedOpenPaths()
    {
        var nodePaths = _inlineCollapsedOpenNodePathCache;
        _inlineCollapsedOpenNodePathCache = null;
        if (nodePaths != null)
        {
            foreach (var path in nodePaths)
            {
                RestoreInlineOpenNodePath(path);
            }
        }

        var defaultOpenPaths = _inlineCollapsedDefaultOpenPathCache;
        _inlineCollapsedDefaultOpenPathCache = null;
        if (defaultOpenPaths != null)
        {
            ReplayDefaultOpenPaths(defaultOpenPaths, GetMaxPathReplayPassCount(defaultOpenPaths));
        }
    }

    private void RestoreInlineOpenNodePath(IReadOnlyList<INavMenuNode> path)
    {
        ItemsControl current = this;
        for (var i = 0; i < path.Count; i++)
        {
            ExecutePendingContainerLayout(current);
            if (NavMenuSemanticNavigator.FindDirectItem(current, path[i]) is not { } item)
            {
                return;
            }

            if (item.HasSubMenu)
            {
                item.SetCurrentValue(NavMenuItem.IsSubMenuOpenProperty, true);
                ExecutePendingContainerLayout(item);
            }

            current = item;
        }
    }

    private void ConfigureDefaultOpenedPaths()
    {
        if (DefaultOpenPaths != null && !_defaultOpenPathsApplied)
        {
            if (IsEffectiveInlineCollapsed)
            {
                _inlineCollapsedDefaultOpenPathCache = new List<TreeNodePath>(DefaultOpenPaths);
                _defaultOpenPathsApplied             = true;
                return;
            }

            Dispatcher.InvokeAsync(
                () => ReplayDefaultOpenPaths(DefaultOpenPaths, GetMaxPathReplayPassCount(DefaultOpenPaths)),
                DispatcherPriority.Loaded);
        }
    }

    private void ConfigureDefaultSelectedPath()
    {
        // 直接设置 SelectedItem 优先级高于 DefaultSelectedPath
        if (SelectedItem != null)
        {
            SelectTargetMenuNode(SelectedItem, _selectedItemRevision);
        }
        else if (DefaultSelectedPath is { } path)
        {
            var selectedItemRevision = _selectedItemRevision;
            Dispatcher.InvokeAsync(
                () => ReplayDefaultSelectedPath(path, selectedItemRevision, path.Length + 1),
                DispatcherPriority.Loaded);
        }
    }

    private void SelectTargetMenuNode(INavMenuNode node, int selectedItemRevision)
    {
        var selectPathNodes = CollectPathNodes(node);
        if (selectPathNodes.Count > 0)
        {
            Dispatcher.InvokeAsync(
                () => ReplaySelectedNodePath(node, selectPathNodes, selectedItemRevision, selectPathNodes.Count + 1),
                DispatcherPriority.Loaded);
        }
    }

    private static int GetMaxPathReplayPassCount(IList<TreeNodePath> paths)
    {
        var maxLength = 0;
        foreach (var path in paths)
        {
            maxLength = Math.Max(maxLength, path.Length);
        }

        return maxLength + 1;
    }

    private void ReplayDefaultOpenPaths(IList<TreeNodePath> paths, int remainingPasses)
    {
        var allApplied = true;
        foreach (var path in paths)
        {
            if (TraverseNavMenuPath(path) is null)
            {
                allApplied = false;
            }
        }

        if (allApplied || remainingPasses <= 0)
        {
            _defaultOpenPathsApplied = true;
            return;
        }

        Dispatcher.InvokeAsync(
            () => ReplayDefaultOpenPaths(paths, remainingPasses - 1),
            DispatcherPriority.Loaded);
    }

    private void ReplayDefaultSelectedPath(TreeNodePath path, int selectedItemRevision, int remainingPasses)
    {
        if (selectedItemRevision != _selectedItemRevision || SelectedItem is not null)
        {
            return;
        }

        var pathItems = TraverseNavMenuPath(path, (menuItem, i) =>
        {
            if (i == path.Length - 1 && selectedItemRevision == _selectedItemRevision && SelectedItem is null)
            {
                SelectNavMenuItem(menuItem);
            }
        });

        if (pathItems != null || remainingPasses <= 0)
        {
            return;
        }

        Dispatcher.InvokeAsync(() => ReplayDefaultSelectedPath(path, selectedItemRevision, remainingPasses - 1),
            DispatcherPriority.Loaded);
    }

    private void ReplaySelectedNodePath(
        INavMenuNode node,
        IReadOnlyList<INavMenuNode> pathNodes,
        int selectedItemRevision,
        int remainingPasses)
    {
        if (selectedItemRevision != _selectedItemRevision ||
            !ReferenceEquals(SelectedItem, node))
        {
            return;
        }

        var pathItems = TraverseNavMenuPath(pathNodes, (menuItem, i) =>
        {
            if (i == pathNodes.Count - 1 &&
                selectedItemRevision == _selectedItemRevision &&
                ReferenceEquals(SelectedItem, node))
            {
                SelectNavMenuItem(menuItem);
            }
        });

        if (pathItems != null || remainingPasses <= 0)
        {
            return;
        }

        Dispatcher.InvokeAsync(() => ReplaySelectedNodePath(node, pathNodes, selectedItemRevision,
            remainingPasses - 1), DispatcherPriority.Loaded);
    }

    private bool IsSelectedNodeAlreadyApplied(INavMenuNode node)
    {
        var pathNodes = CollectPathNodes(node);
        if (pathNodes.Count == 0)
        {
            return false;
        }

        ItemsControl current = this;
        for (var i = 0; i < pathNodes.Count; i++)
        {
            var menuItem = NavMenuSemanticNavigator.FindDirectItem(current, pathNodes[i]);
            if (menuItem is null)
            {
                return false;
            }

            var isLeaf = i == pathNodes.Count - 1;
            if (isLeaf)
            {
                return menuItem.IsSelected;
            }

            if (!menuItem.IsInSelectedPath)
            {
                return false;
            }

            current = menuItem;
        }

        return false;
    }

    private static int CountSelectPathItems(NavMenuItem menuItem)
    {
        var          count   = 0;
        NavMenuItem? current = menuItem.SemanticParentItem;
        while (current != null)
        {
            count++;
            current = current.SemanticParentItem;
        }

        return count;
    }

    private List<NavMenuItem>? TraverseNavMenuPath(TreeNodePath treeNodePath, Action<NavMenuItem, int>? action = null)
    {
        if (treeNodePath.Length == 0)
        {
            return null;
        }

        return TraverseNavMenuPath(
            treeNodePath.Segments,
            (_, menuItem, segment) => menuItem.ItemKey != null && menuItem.ItemKey.Value == segment,
            action);
    }

    private List<NavMenuItem>? TraverseNavMenuPath(IReadOnlyList<INavMenuNode> pathNodes, Action<NavMenuItem, int>? action = null)
    {
        if (pathNodes.Count == 0)
        {
            return null;
        }

        return TraverseNavMenuPath(
            pathNodes,
            (node, _, currentNode) => ReferenceEquals(node, currentNode),
            action);
    }

    private List<NavMenuItem>? TraverseNavMenuPath<TSegment>(
        IReadOnlyList<TSegment> segments,
        Func<INavMenuNode, NavMenuItem, TSegment, bool> isTargetSegment,
        Action<NavMenuItem, int>? action = null)
    {
        try
        {
            EnterDisableMotionRegion();
            ItemsControl current      = this;
            var          pathItems    = new List<NavMenuItem>(segments.Count);
            for (int i = 0; i < segments.Count; i++)
            {
                var  segment    = segments[i];
                bool childFound = false;
                ExecutePendingContainerLayout(current);
                foreach (var navMenuItem in NavMenuSemanticNavigator.EnumerateDirectItems(current))
                {
                    if (((INavMenuItem)navMenuItem).Node is { } node &&
                        isTargetSegment(node, navMenuItem, segment))
                    {
                        var requiresChildContainer = i < segments.Count - 1;
                        if (requiresChildContainer || HasNodeChildren(node))
                        {
                            if (!OpenPathSubmenu(navMenuItem, requiresChildContainer))
                            {
                                return null;
                            }
                        }
                        else
                        {
                            navMenuItem.SetCurrentValue(NavMenuItem.IsSubMenuOpenProperty, true);
                        }

                        current    = navMenuItem;
                        childFound = true;
                        pathItems.Add(navMenuItem);
                        action?.Invoke(navMenuItem, i);
                        break;
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
        var pathCount = CountPathNodes(node);
        var pathNodes = new List<INavMenuNode>(pathCount);

        if (Items.Count > 0)
        {
            for (var i = 0; i < pathCount; i++)
            {
                pathNodes.Add(null!);
            }

            var current = node;
            for (var i = pathCount - 1; current != null; i--)
            {
                pathNodes[i] = current;
                current      = current.ParentNode as INavMenuNode;
            }

            Debug.Assert(pathNodes.Count > 0);
            // 检查是否是野数据
            var rootNode  = pathNodes[0];
            var foundRoot = NavMenuEntryGraph.ContainsDirectNode(Items, rootNode);
            if (!foundRoot || node != pathNodes[^1])
            {
                throw new ArgumentOutOfRangeException(nameof(node), "Wild INavMenuNode, Only part of the path was found");
            }
        }

        return pathNodes;
    }

    private static int CountPathNodes(INavMenuNode node)
    {
        var count   = 0;
        var current = node;
        while (current != null)
        {
            count++;
            current = current.ParentNode as INavMenuNode;
        }

        return count;
    }

    private static bool HasNodeChildren(INavMenuNode node)
    {
        foreach (var _ in node.Children)
        {
            return true;
        }

        return false;
    }

    private bool OpenPathSubmenu(NavMenuItem menuItem, bool requiresChildContainer)
    {
        if (menuItem.Mode != NavMenuMode.Inline &&
            !CanOpenPopup(menuItem))
        {
            return false;
        }

        var wasOpen = menuItem.IsSubMenuOpen;
        menuItem.SetCurrentValue(NavMenuItem.IsSubMenuOpenProperty, true);
        ExecutePendingContainerLayout(menuItem);
        if (menuItem.Mode == NavMenuMode.Inline &&
            !wasOpen)
        {
            return false;
        }

        return !requiresChildContainer || menuItem.Presenter?.Panel != null;
    }

    private static bool CanOpenPopup(NavMenuItem menuItem)
    {
        if (!menuItem.ShouldUseOverlayPopup &&
            RuntimePlatform.Features.SupportsNativeWindow)
        {
            return true;
        }

        return menuItem.GetPopupOverlayLayer() is not null;
    }
}
