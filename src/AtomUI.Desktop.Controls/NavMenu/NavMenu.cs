using System.Collections.Specialized;
using System.Diagnostics;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Animation;
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
using Avalonia.Logging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A navigation menu that shows a tree of <see cref="INavMenuNode" /> entries, with optional groups and
/// dividers. <see cref="Mode" /> selects the layout and how submenus open: in place below the parent in
/// the inline layout, as popups in the horizontal and vertical layouts. The menu is a single keyboard
/// stop and draws its own keyboard focus outline.
/// </summary>
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
    /// <summary>
    /// Identifies the <see cref="SelectedItem" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenu, INavMenuNode?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, INavMenuNode?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);
    
    /// <summary>
    /// Identifies the <see cref="DefaultSelectedPath" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenu, TreeNodePath?> DefaultSelectedPathProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, TreeNodePath?>(
            nameof(DefaultSelectedPath),
            o => o.DefaultSelectedPath,
            (o, v) => o.DefaultSelectedPath = v);
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NavMenu>();
    
    /// <summary>
    /// Identifies the <see cref="IsAccordionMode" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAccordionModeProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsAccordionMode), false);
    
    /// <summary>
    /// Identifies the <see cref="Mode" /> property.
    /// </summary>
    public static readonly StyledProperty<NavMenuMode> ModeProperty =
        AvaloniaProperty.Register<NavMenu, NavMenuMode>(nameof(Mode), NavMenuMode.Inline);

    /// <summary>
    /// Identifies the <see cref="IsInlineCollapsed" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsInlineCollapsedProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsInlineCollapsed));

    /// <summary>
    /// Identifies the <see cref="InlineCollapsedWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> InlineCollapsedWidthProperty =
        AvaloniaProperty.Register<NavMenu, double>(nameof(InlineCollapsedWidth));

    /// <summary>
    /// Identifies the <see cref="IsCollapsedTooltipEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCollapsedTooltipEnabledProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsCollapsedTooltipEnabled), true);

    /// <summary>
    /// Identifies the <see cref="CollapsedTooltipPlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<PlacementMode> CollapsedTooltipPlacementProperty =
        AvaloniaProperty.Register<NavMenu, PlacementMode>(
            nameof(CollapsedTooltipPlacement),
            PlacementMode.Right);

    /// <summary>
    /// Identifies the <see cref="CollapsedTooltipShowDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> CollapsedTooltipShowDelayProperty =
        AvaloniaProperty.Register<NavMenu, int>(nameof(CollapsedTooltipShowDelay), 400);

    /// <summary>
    /// Identifies the <see cref="CollapsedTooltipBetweenShowDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> CollapsedTooltipBetweenShowDelayProperty =
        AvaloniaProperty.Register<NavMenu, int>(nameof(CollapsedTooltipBetweenShowDelay), 100);
    
    /// <summary>
    /// Identifies the <see cref="IsDarkStyle" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDarkStyleProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsDarkStyle), false);
    
    /// <summary>
    /// Identifies the <see cref="IsItemBackgroundEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsItemBackgroundEnabledProperty =
        AvaloniaProperty.Register<NavMenu, bool>(nameof(IsItemBackgroundEnabled), true);

    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<NavMenu, object?>(nameof(Header));

    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<NavMenu, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// Identifies the <see cref="Footer" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<NavMenu, object?>(nameof(Footer));

    /// <summary>
    /// Identifies the <see cref="FooterTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<NavMenu, IDataTemplate?>(nameof(FooterTemplate));

    /// <summary>
    /// Identifies the <see cref="ItemSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<NavMenu, double>(nameof(ItemSpacing), inherits: true);

    /// <summary>
    /// Identifies the <see cref="DefaultOpenPaths" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenu, IList<TreeNodePath>?> DefaultOpenPathsProperty =
        AvaloniaProperty.RegisterDirect<NavMenu, IList<TreeNodePath>?>(
            nameof(DefaultOpenPaths),
            o => o.DefaultOpenPaths,
            (o, v) => o.DefaultOpenPaths = v);
    
    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty = 
        AvaloniaProperty.Register<NavMenu, bool>(nameof (ShouldUseOverlayPopup));
    
    private INavMenuNode? _selectedItem;
    private int _selectedItemRevision;

    /// <summary>
    /// Gets or sets the selected node, or <see langword="null" /> to clear the selection. Setting a node
    /// selects its item and marks the items on its path once they are built.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The node is set while <c>Items</c> is not empty and the root of its <c>ParentNode</c> chain is not a
    /// direct entry of <c>Items</c>.
    /// </exception>
    public INavMenuNode? SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }
    
    private IList<TreeNodePath>? _defaultOpenPaths;
    
    /// <summary>
    /// Gets or sets the paths, matched against the <c>ItemKey</c> of the items, whose submenus are open after
    /// the menu loads. They are applied once, on the first load; a path whose items are not built yet is
    /// retried on the following layout passes. An inline menu that is collapsed at that time keeps the paths
    /// and applies them when it expands.
    /// </summary>
    public IList<TreeNodePath>? DefaultOpenPaths
    {
        get => _defaultOpenPaths;
        set => SetAndRaise(DefaultOpenPathsProperty, ref _defaultOpenPaths, value);
    }
    
    private TreeNodePath? _defaultSelectedPath;
    
    /// <summary>
    /// Gets or sets the path of the node that is selected when the menu loads while
    /// <see cref="SelectedItem" /> is <see langword="null" />. The segments of the path are matched against
    /// the <c>ItemKey</c> of the items. The path is applied again on every load until a node is selected.
    /// </summary>
    public TreeNodePath? DefaultSelectedPath
    {
        get => _defaultSelectedPath;
        set => SetAndRaise(DefaultSelectedPathProperty, ref _defaultSelectedPath, value);
    }
    
    /// <summary>
    /// Gets or sets whether the menu and its items play their motion, such as the expand and collapse of
    /// submenus and the width change when an inline menu collapses or expands. The theme supplies the
    /// <c>EnableMotion</c> token when no value is set. Setting it to <see langword="false" /> ends a running
    /// width motion.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether at most one top-level submenu is open: opening one closes the others. It applies
    /// in the horizontal and vertical layouts, also in a collapsed inline menu, and turning it on closes the
    /// submenus that are open.
    /// </summary>
    public bool IsAccordionMode
    {
        get => GetValue(IsAccordionModeProperty);
        set => SetValue(IsAccordionModeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the layout of the menu and the way its submenus open. The default is
    /// <see cref="NavMenuMode.Inline" />. Changing it closes the open submenus and keeps the selection.
    /// </summary>
    public NavMenuMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether an inline menu shows only its top-level items. It takes effect only while
    /// <see cref="Mode" /> is <see cref="NavMenuMode.Inline" />: the menu then behaves as a vertical menu
    /// whose submenus open as popups, takes <see cref="InlineCollapsedWidth" /> in place of its own width and
    /// hides the footer. Collapsing closes the open submenus and expanding reopens them; the selection is kept.
    /// </summary>
    public bool IsInlineCollapsed
    {
        get => GetValue(IsInlineCollapsedProperty);
        set => SetValue(IsInlineCollapsedProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the menu while <see cref="IsInlineCollapsed" /> is in effect. It then
    /// replaces the <c>Width</c> of the menu and caps <c>MinWidth</c>; a negative or non-finite value counts
    /// as 0. The theme supplies the <c>InlineCollapsedWidth</c> token when no value is set.
    /// </summary>
    public double InlineCollapsedWidth
    {
        get => GetValue(InlineCollapsedWidthProperty);
        set => SetValue(InlineCollapsedWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a top-level item without a submenu shows a tooltip while an inline menu is
    /// collapsed. The tooltip shows the tooltip of the item or else its header; an item that disables its
    /// own tooltip shows none.
    /// </summary>
    public bool IsCollapsedTooltipEnabled
    {
        get => GetValue(IsCollapsedTooltipEnabledProperty);
        set => SetValue(IsCollapsedTooltipEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets where the tooltip of an item is placed relative to the item while an inline menu is
    /// collapsed.
    /// </summary>
    public PlacementMode CollapsedTooltipPlacement
    {
        get => GetValue(CollapsedTooltipPlacementProperty);
        set => SetValue(CollapsedTooltipPlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets the time, in milliseconds, that the pointer rests on an item of a collapsed inline menu
    /// before its tooltip appears.
    /// </summary>
    public int CollapsedTooltipShowDelay
    {
        get => GetValue(CollapsedTooltipShowDelayProperty);
        set => SetValue(CollapsedTooltipShowDelayProperty, value);
    }

    /// <summary>
    /// Gets or sets the time, in milliseconds, between two tooltips being shown in which the second tooltip
    /// appears without waiting for <see cref="CollapsedTooltipShowDelay" />. It applies to the tooltips of a
    /// collapsed inline menu.
    /// </summary>
    public int CollapsedTooltipBetweenShowDelay
    {
        get => GetValue(CollapsedTooltipBetweenShowDelayProperty);
        set => SetValue(CollapsedTooltipBetweenShowDelayProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the menu, its items and its submenu popups are painted with the dark
    /// <c>NavMenuToken</c> colors, such as <c>DarkMenuBg</c>, <c>DarkItemColor</c> and
    /// <c>DarkMenuPopupBg</c>, instead of the light ones.
    /// </summary>
    public bool IsDarkStyle
    {
        get => GetValue(IsDarkStyleProperty);
        set => SetValue(IsDarkStyleProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the child items of an item that expands in place in an inline menu are drawn on
    /// their own background, <c>SubMenuItemBg</c>. When it is <see langword="false" /> that area is
    /// transparent.
    /// </summary>
    public bool IsItemBackgroundEnabled
    {
        get => GetValue(IsItemBackgroundEnabledProperty);
        set => SetValue(IsItemBackgroundEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the content shown above the items, or to their left in the horizontal layout. It is
    /// hidden while the value is <see langword="null" />.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display <see cref="Header" />.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the content shown below the items, or to their right in the horizontal layout. It is
    /// hidden while the value is <see langword="null" />, and in an inline menu while the menu is collapsed.
    /// </summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display <see cref="Footer" />.
    /// </summary>
    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the gap between the top-level items and between the items inside each submenu and group.
    /// The inline and vertical layouts take the value from the <c>VerticalItemsPanelSpacing</c> token when no
    /// value is set; the horizontal layout has no gap unless a value is set.
    /// </summary>
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the submenus open in the overlay layer of the window instead of as native popup
    /// windows. When it is <see langword="true" />, a submenu can open only where an overlay layer is
    /// available. The theme sets it to <see langword="true" />; the default is <see langword="false" />.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }
    #endregion
    
    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="NavMenuItemClick" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<NavMenuItemClickEventArgs> NavMenuItemClickEvent =
        RoutedEvent.Register<NavMenu, NavMenuItemClickEventArgs>(nameof(NavMenuItemClick), RoutingStrategies.Bubble);
    
    /// <summary>
    /// Identifies the <see cref="NavMenuNodeSelected" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<NavMenuNodeSelectedEventArgs> NavMenuNodeSelectedEvent =
        RoutedEvent.Register<NavMenu, NavMenuNodeSelectedEventArgs>(nameof(NavMenuNodeSelected), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised after an activated item was selected, which follows the item's own <c>Click</c> event and
    /// <see cref="NavMenuNodeSelected" />. Activating an item so that its submenu opens does not raise it.
    /// </summary>
    public event EventHandler<NavMenuItemClickEventArgs>? NavMenuItemClick
    {
        add => AddHandler(NavMenuItemClickEvent, value);
        remove => RemoveHandler(NavMenuItemClickEvent, value);
    }
    
    /// <summary>
    /// Raised after the selection is committed: <see cref="SelectedItem" /> holds the node and the items on
    /// its path are marked. It is raised for a node that the user activated, that was assigned to
    /// <see cref="SelectedItem" /> or that was reached through <see cref="DefaultSelectedPath" />, and not
    /// when <see cref="SelectedItem" /> does not hold the node afterwards.
    /// </summary>
    public event EventHandler<NavMenuNodeSelectedEventArgs>? NavMenuNodeSelected
    {
        add => AddHandler(NavMenuNodeSelectedEvent, value);
        remove => RemoveHandler(NavMenuNodeSelectedEvent, value);
    }
    
    #endregion
    
    #region 内部属性定义

    IEnumerable<INavMenuItem> INavMenuElement.SubItems => EnumerateSubItems();

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

    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<NavMenu>();

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

    private IEnumerable<INavMenuItem> EnumerateSubItems()
    {
        foreach (var child in NavMenuSemanticNavigator.EnumerateDirectItems(this))
        {
            yield return child;
        }
    }
    
    private readonly ManagedFocusOutline _rootFocusOutline = new();
    private bool _isRootFocusOutlineRequested;

    internal INavMenuInteractionHandler? InteractionHandler { get; private set; }
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel { Orientation = Orientation.Vertical });

    private bool _defaultOpenPathsApplied;
    // IsAttachedToVisualTree() still reports true inside OnDetachedFromVisualTree, so track attachment explicitly.
    private bool _isAttachedToVisualTree;
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

        // Keep elements inside a submenu popup from bubbling BringIntoView to an outer ScrollViewer, which
        // would scroll the page again when the selected item opens. In Inline mode the submenus are in the
        // visual tree, so the ScrollViewer of this NavMenu still answers the request.
        AddHandler(RequestBringIntoViewEvent, (_, e) =>
        {
            if (EffectiveMode == NavMenuMode.Inline || e.TargetObject == this)
            {
                return;
            }

            e.Handled = true;
        }, handledEventsToo: true);
    }
    
    /// <summary>
    /// Called when the <c>Items</c> collection of the menu changes; the constructor subscribes it once. It passes the
    /// change to the entry ownership coordinator, which validates the affected entries, and, while
    /// <see cref="IsPopupPinnedOpen" /> is set, queues a reconciliation of the pinned popup, because the change can
    /// alter the pinnable targets or invalidate the pinned node. Call the base implementation from an override,
    /// otherwise neither step runs.
    /// </summary>
    /// <param name="sender">
    /// The <c>Items</c> collection that changed.
    /// </param>
    /// <param name="e">
    /// Describes the change.
    /// </param>
    private protected virtual void HandleItemsViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _entryOwnershipCoordinator.Synchronize(e);
        if (IsPopupPinnedOpen)
        {
            // A change of the entry graph can change the pinnable targets, for example when a declarative pin is
            // written before the entries are filled in, or can invalidate the pinned node. The container-to-node
            // mapping has not converged yet, so the coordinator schedules the evaluation for after the
            // ItemsPresenter has settled.
            _pinnedOpenCoordinator.QueueReconcile(this);
        }
    }
    
    /// <summary>
    /// Calls the base method, then keeps the derived state in step with the changed property: it mirrors
    /// <see cref="ItemSpacing" /> to the items, refreshes the pseudo-classes after <see cref="Mode" />,
    /// <see cref="IsInlineCollapsed" /> or <see cref="IsDarkStyle" /> changes, selects the item of a set
    /// <see cref="SelectedItem" /> and clears the selection for <see langword="null" />, closes every
    /// top-level submenu when <see cref="IsAccordionMode" /> turns on, applies or releases the pinning
    /// after <see cref="IsPopupPinnedOpen" /> changes, and ends the width motion of a collapse or expand
    /// when <see cref="IsMotionEnabled" /> turns off. A derived override must call this method.
    /// </summary>
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
                // Unpinning only lifts the close interception: submenus that are open stay open. The requests
                // are added back at container prepare or attach only once this property is true again.
                _pinnedOpenCoordinator.Release();
            }
        }
    }
    
    private void HandleModeChanged()
    {
        CancelInlineCollapsedWidthMotion();
        ClearInlineCollapsedLayoutWidth();
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

        // Collapsing switches EffectiveMode between Inline and Vertical, so the pin requests must follow
        // the effective mode: they are applied again when collapsed and released on expanding to Inline.
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

    /// <summary>
    /// Calls the base method, then marks the menu as attached, applies the width constraints of a collapsed
    /// inline menu, mounts the interaction handler of the effective mode and reconciles the pinned-open
    /// submenus.
    /// A derived override must call this method.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;
        CoerceInlineCollapsedLayoutConstraints();
        ConfigureInteractionHandler(true);
        _pinnedOpenCoordinator.Reconcile(this);
    }

    /// <summary>
    /// Releases a pinned-open popup, calls the base method, then drops the state that needs the menu to be in
    /// a visual tree: the interaction handler, the root focus outline and the width motion of a collapsing or
    /// expanding inline menu.
    /// A derived override must call this method.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (IsPopupPinnedOpen)
        {
            _pinnedOpenCoordinator.Release();
            ClosePinnedForLifecycle();
        }

        base.OnDetachedFromVisualTree(e);
        _isAttachedToVisualTree = false;
        _rootFocusOutline.Hide();
        CancelInlineCollapsedWidthMotion();
        ClearInlineCollapsedLayoutWidth();
        InteractionHandler?.Detach(this);
        InteractionHandler = null;
    }

    /// <summary>
    /// Calls the base method, then keeps the menu the single keyboard stop: when Tab or directional
    /// navigation gives focus to one of its items, focus is taken back to the menu root. For Tab and
    /// directional navigation the keyboard focus becomes visible. The root focus outline is refreshed.
    /// A derived override must call this method.
    /// </summary>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.NavigationMethod is NavigationMethod.Tab or NavigationMethod.Directional)
        {
            // Selection and pointer presses focus items, which makes the item the Tab-once active element,
            // so Tab re-entry lands on it. The menu is the single keyboard stop: take focus back so the
            // outline is drawn once, on the keyboard active item or on the root.
            if (!ReferenceEquals(e.Source, this) && e.Source is NavMenuItem item && this.IsVisualAncestorOf(item))
            {
                e.Handled = true;
                Focus(e.NavigationMethod, e.KeyModifiers);
                return;
            }

            InteractionHandler?.SetKeyboardFocusVisible(true);
        }

        UpdateRootFocusOutline();
    }

    /// <summary>
    /// Calls the base method, then hides the keyboard focus indication when focus moves to an element outside
    /// the menu and its submenu popups, and refreshes the root focus outline.
    /// A derived override must call this method.
    /// </summary>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        // Focus events of items inside submenu popups bubble here through the logical tree, so the
        // check is logical as well.
        if (!IsMenuElement(e.NewFocusedElement))
        {
            InteractionHandler?.SetKeyboardFocusVisible(false);
        }

        UpdateRootFocusOutline();
    }

    /// <summary>
    /// Requested by the interaction handler while the menu shows keyboard focus and no item is active;
    /// drawn only while the menu itself owns focus.
    /// </summary>
    internal void SetRootFocusOutlineVisible(bool isVisible)
    {
        _isRootFocusOutlineRequested = isVisible;
        UpdateRootFocusOutline();
    }

    private void UpdateRootFocusOutline()
    {
        if (_isRootFocusOutlineRequested && IsFocused && _isAttachedToVisualTree)
        {
            _rootFocusOutline.Show(this);
        }
        else
        {
            _rootFocusOutline.Hide();
        }
    }

    private bool IsMenuElement(IInputElement? element)
    {
        return element is ILogical logical && (ReferenceEquals(logical, this) || this.IsLogicalAncestorOf(logical));
    }

    /// <summary>
    /// Returns <see langword="true" /> for every entry and sets <paramref name="recycleKey" /> to the key of
    /// its kind, so that containers are recycled only within their kind. An entry of another type throws
    /// <see cref="InvalidOperationException" />.
    /// </summary>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NavMenuEntryContainerCoordinator.NeedsContainer(this, item, index, out recycleKey);
    }
    
    /// <summary>
    /// Creates the container for an entry from its recycle key: a <see cref="NavMenuItem" /> for a node, a
    /// <see cref="NavMenuGroupItem" /> for a group and a <see cref="NavMenuDividerItem" /> for a divider.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return NavMenuEntryContainerCoordinator.CreateContainer(item, index, recycleKey);
    }
    
    /// <summary>
    /// Calls the base method, then gives a generated container its entry context and binds it to its entry,
    /// such as the node, the header and the header template of an item.
    /// </summary>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        NavMenuEntryContainerCoordinator.PrepareContainer(this, container, item, index);
    }

    /// <summary>
    /// Removes the state that a generated container borrowed from its entry before the base method recycles
    /// it: the node bindings and the entry context, and for an item also its selection and pinning
    /// bookkeeping and its keyboard and pointer states.
    /// </summary>
    protected override void ClearContainerForItemOverride(Control container)
    {
        NavMenuEntryContainerCoordinator.ClearContainer(this, container);
        base.ClearContainerForItemOverride(container);
    }
    
    internal virtual void PrepareNavMenuItem(NavMenuItem menuItem, object? item, int index)
    {
    }

    internal void ApplySelectionStateToPreparedContainer(NavMenuItem menuItem)
    {
        _selectionCoordinator.PrepareContainer(this, menuItem);
    }

    /// <summary>
    /// Applies the pinning invariant of the container prepare stage. Pin requests are stored by node in
    /// <see cref="NavMenuPinnedOpenCoordinator" />, so a pin that AXAML sets before any container exists
    /// (<c>IsPopupPinnedOpen="True"</c>) and the container rebuild after a <see cref="Mode" /> change are
    /// both completed from those requests when the container is realized, under the same container timing
    /// contract as the selection state.
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
            // Removing a container can invalidate the whole pinned path. Evaluate again once the entries converge.
            _pinnedOpenCoordinator.QueueReconcile(this);
        }
    }

    private void ClosePinnedForLifecycle()
    {
        foreach (var menuItem in NavMenuSemanticNavigator.EnumerateDirectItems(this).OfType<NavMenuItem>())
        {
            menuItem.CloseForLifecycle();
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
    
    private void ConfigureInteractionHandler(bool needMount = false)
    {
        if (needMount)
        {
            InteractionHandler?.Detach(this);
        }

        // DefaultNavMenuInteractionHandler subscribes to the app-wide IInputManager.Process on attach;
        // mounting one while detached would root this menu, so defer until OnAttachedToVisualTree.
        if (!_isAttachedToVisualTree)
        {
            InteractionHandler = null;
            return;
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
        // Disposing a Forward animation before its first tick can fill its default 0.
        // While cancelling, keep coercion at the last presented width until the caller
        // installs the next start or clears the override. The CTS is the execution owner.
        layoutWidth = _inlineCollapsedWidthMotionCancellationTokenSource is { IsCancellationRequested: true }
            ? Bounds.Width
            : InlineCollapsedLayoutWidth;
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
    
    /// <summary>
    /// Called when a <see cref="NavMenuItem" /> in this menu reports that its submenu opened. It extends a
    /// pinned-open popup to the opened item and, outside the inline layout, closes the other top-level
    /// submenus while <see cref="IsAccordionMode" /> is set.
    /// A derived override must call this method to keep both behaviors.
    /// </summary>
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
    
    /// <summary>
    /// Calls the base method, then applies <see cref="DefaultOpenPaths" /> the first time the menu loads.
    /// On every load it selects the node of <see cref="SelectedItem" />, or of
    /// <see cref="DefaultSelectedPath" /> while no node is selected.
    /// A derived override must call this method.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureDefaultOpenedPaths();
        ConfigureDefaultSelectedPath();
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
        startWidth  = NormalizeInlineCollapsedWidth(Bounds.Width);
        CancelInlineCollapsedWidthMotion();
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
        _ = RunInlineCollapsedWidthMotionAsync(startWidth, targetWidth, duration, cancellationTokenSource);
    }

    private async Task RunInlineCollapsedWidthMotionAsync(double startWidth,
                                                          double targetWidth,
                                                          TimeSpan duration,
                                                          CancellationTokenSource cancellationTokenSource)
    {
        var cancellationToken = cancellationTokenSource.Token;
        try
        {
            // The endpoints depend on current rendered bounds and metadata coercion, so a
            // ControlTheme transition cannot express this request-owned layout override.
            if (MotionFrameTiming.Clock is { RefreshInterval: > 0 })
            {
                await RunInlineCollapsedWidthOnPresentationClockAsync(startWidth, targetWidth, duration, cancellationToken);
                return;
            }

            var animation = new Animation
            {
                Duration = duration,
                Easing = new CubicEaseOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0),
                        Setters = { new Setter(InlineCollapsedLayoutWidthProperty, startWidth) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters = { new Setter(InlineCollapsedLayoutWidthProperty, targetWidth) }
                    }
                }
            };
            await animation.RunAsync(this, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // A newer inline collapsed transition owns the layout width now.
        }
        catch (Exception exception)
        {
            Logger.TryGet(LogEventLevel.Error, LogArea.Control)?.Log(
                this, "Inline collapsed width motion failed: {Exception}", exception);
        }
        finally
        {
            // RunAsync can complete before Avalonia disposes its animation binding/final fill.
            // Queue only teardown, after that stack unwinds; no frame values are posted.
            if (!cancellationToken.IsCancellationRequested)
            {
                Dispatcher.UIThread.Post(() => CompleteInlineCollapsedWidthMotion(targetWidth, cancellationTokenSource));
            }
        }
    }

    private async Task RunInlineCollapsedWidthOnPresentationClockAsync(double startWidth, double targetWidth,
        TimeSpan duration, CancellationToken cancellationToken)
    {
        var easing = new CubicEaseOut();
        var driver = new MotionFrameDriver(this, duration, elapsed =>
            InlineCollapsedLayoutWidth = startWidth + (targetWidth - startWidth) * easing.Ease(elapsed / duration));
        cancellationToken.ThrowIfCancellationRequested();
        driver.Start();
        using var cancellation = cancellationToken.Register(static state => ((MotionFrameDriver)state!).Stop(), driver);
        await driver.Completion;
    }

    private void CompleteInlineCollapsedWidthMotion(double targetWidth,
                                                    CancellationTokenSource cancellationTokenSource)
    {
        if (!ReferenceEquals(_inlineCollapsedWidthMotionCancellationTokenSource, cancellationTokenSource) ||
            cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        _inlineCollapsedWidthMotionCancellationTokenSource = null;
        try
        {
            cancellationTokenSource.Cancel();
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }
        InlineCollapsedLayoutWidth = targetWidth;
        ClearInlineCollapsedLayoutWidth();
    }

    // The effective value of this NavMenu (local ThemeConfigProvider scopes and Controls[NavMenu] included).
    internal TimeSpan ResolveInlineCollapsedWidthMotionDuration() =>
        ControlTokenAccessor.Capture<NavMenu>(this).GetEffectiveGlobal<TimeSpan>(SharedTokenKind.MotionDurationMid);

    private void CancelInlineCollapsedWidthMotion()
    {
        var cancellationTokenSource = _inlineCollapsedWidthMotionCancellationTokenSource;
        if (cancellationTokenSource is null)
        {
            return;
        }

        try
        {
            cancellationTokenSource.Cancel();
        }
        finally
        {
            if (ReferenceEquals(_inlineCollapsedWidthMotionCancellationTokenSource, cancellationTokenSource))
            {
                _inlineCollapsedWidthMotionCancellationTokenSource = null;
            }
            cancellationTokenSource.Dispose();
        }
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
        // A SelectedItem that was set directly takes priority over DefaultSelectedPath.
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

    /// <summary>
    /// Closes every submenu of the menu and clears the selection. The call does nothing while
    /// <see cref="IsPopupPinnedOpen" /> is <see langword="true" />.
    /// </summary>
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
    
    void IMenuChildSelectable.SelectChildItem(NavMenuItem child, bool isSelected)
    {
        if (child.IsTopLevel)
        {
            child.SetCurrentValue(NavMenuItem.IsSelectedProperty, isSelected);
        }
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
            // Reject wild data: a node whose root is not among Items, or that is not the last node of its path.
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
