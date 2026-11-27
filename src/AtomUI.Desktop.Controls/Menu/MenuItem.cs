using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaMenuItem = Avalonia.Controls.MenuItem;

/// <summary>
/// An entry of a <see cref="Menu" />, a <see cref="ContextMenu" /> or a menu flyout. An entry that has
/// items is a submenu entry: the child entries are shown in the popup that the template provides as
/// <c>PART_Popup</c>, which opens and closes with <c>IsSubMenuOpen</c>. A child that is a
/// <see cref="MenuSeparatorData" /> is shown as a <see cref="MenuSeparator" />, a child that is a
/// <see cref="MenuItemGroupData" /> as a <see cref="MenuItemGroup" />, and every other child as a
/// <see cref="MenuItem" />.
/// </summary>
[PseudoClasses(MenuItemPseudoClass.TopLevel)]
public class MenuItem : AvaloniaMenuItem, IMenuItemData, IScrollAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<MenuItem>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<MenuItem>();

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public new static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<MenuItem, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<MenuItem>();

    /// <summary>
    /// Identifies the <see cref="IsScrollEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        ScrollAwareControlProperty.IsScrollEnabledProperty.AddOwner<MenuItem>();

    /// <summary>
    /// Identifies the <see cref="DisplayPageSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> DisplayPageSizeProperty =
        Menu.DisplayPageSizeProperty.AddOwner<MenuItem>();

    /// <summary>
    /// Identifies the <see cref="IsDanger" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDangerProperty =
        AvaloniaProperty.Register<MenuItem, bool>(nameof(IsDanger));

    /// <summary>
    /// Motion played when the popup of the submenu opens, or <see langword="null" /> to open it without motion.
    /// The value is handed to the popup through a template binding. The theme of an entry sets
    /// <c>PopupMotionDefaults.MenuOpen</c>; the theme of a top-level entry sets
    /// <c>PopupMotionDefaults.HorizontalMenuOpen</c>.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// Motion played when the popup of the submenu closes, or <see langword="null" /> to close it without motion.
    /// The value is handed to the popup through a template binding. The theme of an entry sets
    /// <c>PopupMotionDefaults.MenuClose</c>; the theme of a top-level entry sets
    /// <c>PopupMotionDefaults.HorizontalMenuClose</c>.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// The icon shown before the header of the entry, or <see langword="null" /> to show none. It hides the
    /// <c>Icon</c> property of the Avalonia menu item and takes a <see cref="PathIcon" />. Set it on the entry,
    /// or supply it through an <see cref="IMenuItemData" /> item: the host copies it to the entry it creates
    /// while the entry has no value set.
    /// </summary>
    public new PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Size preset of the entry; its theme holds a style rule for each value. The menu, the context menu or the
    /// parent entry that creates the entry binds its value through to it, so set it on that owner.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Whether the items of the submenu are hosted in a scroll viewer. With it, the popup of the submenu is
    /// limited to <see cref="DisplayPageSize" /> item rows plus the padding of the popup and scrolls; without it
    /// the popup gets no height limit from the entry. The value inherits, so the entries follow the value set on
    /// the owning menu or context menu. The default is <see langword="true" />.
    /// </summary>
    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    /// <summary>
    /// Number of item rows the popup of the submenu shows at most. It takes effect only while
    /// <see cref="IsScrollEnabled" /> is <see langword="true" />: the height of the popup is then limited to this
    /// many item heights plus the padding of the popup, and further rows are reached by scrolling. A menu or a
    /// context menu binds its value through to the entries it creates. The registered default is 10.
    /// </summary>
    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
    }

    /// <summary>
    /// Whether the item is a danger (destructive) action, drawn with the menu's danger item colors.
    /// </summary>
    public bool IsDanger
    {
        get => GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    /// <summary>
    /// The logical parent of the entry when it implements <see cref="IMenuItemData" />, or
    /// <see langword="null" /> when the entry has no parent or its parent does not implement it.
    /// </summary>
    public ITreeNode<IMenuItemData>? ParentNode => Parent as ITreeNode<IMenuItemData>;

    /// <summary>
    /// The key of the entry, or <see langword="null" /> when it has none. For an entry that a host creates for
    /// an <see cref="IMenuItemData" /> item, the host copies the key of the item to the entry while the entry
    /// has no key yet.
    /// </summary>
    public EntityKey? ItemKey { get; set; }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="IsCheckStateChanged" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> IsCheckStateChangedEvent =
        RoutedEvent.Register<MenuItem, RoutedEventArgs>(nameof(IsCheckStateChanged), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised after the <c>IsChecked</c> value of the entry has changed, so that handlers read the new value.
    /// The event bubbles, and its arguments carry no check state.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? IsCheckStateChanged
    {
        add => AddHandler(IsCheckStateChangedEvent, value);
        remove => RemoveHandler(IsCheckStateChangedEvent, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> IconMarginInlineEndProperty =
        AvaloniaProperty.Register<MenuItem, double>(nameof(IconMarginInlineEnd));


    internal static readonly DirectProperty<MenuItem, MenuSemanticLevel> SemanticLevelProperty =
        AvaloniaProperty.RegisterDirect<MenuItem, MenuSemanticLevel>(nameof(SemanticLevel),
            item => item.SemanticLevel);

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<MenuItem>();

    internal static readonly StyledProperty<double> MaxPopupHeightProperty =
        AvaloniaProperty.Register<MenuItem, double>(nameof(MaxPopupHeight));

    internal static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<MenuItem, double>(nameof(ItemHeight));

    internal static readonly StyledProperty<Thickness> PopupPaddingProperty =
        AvaloniaProperty.Register<MenuItem, Thickness>(nameof(PopupPadding));

    internal static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<MenuItem, bool>(nameof(ShouldUseOverlayPopup));

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<MenuItem>();

    internal double IconMarginInlineEnd
    {
        get => GetValue(IconMarginInlineEndProperty);
        set => SetValue(IconMarginInlineEndProperty, value);
    }

    // Runtime owner context, not a user style input. Publishing it lets the
    // ControlTheme update its motion preset when a container changes owners.
    private MenuSemanticLevel _semanticLevel;

    internal MenuSemanticLevel SemanticLevel
    {
        get => _semanticLevel;
        set
        {
            MenuSemanticLevelScope.ApplyItemLevel(this, value);
            if (SetAndRaise(SemanticLevelProperty, ref _semanticLevel, value))
            {
                MenuSemanticLevelScope.ApplyChildrenLevel(this, ResolveChildSemanticLevel());
            }
        }
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal double MaxPopupHeight
    {
        get => GetValue(MaxPopupHeightProperty);
        set => SetValue(MaxPopupHeightProperty, value);
    }

    internal double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    internal Thickness PopupPadding
    {
        get => GetValue(PopupPaddingProperty);
        set => SetValue(PopupPaddingProperty, value);
    }

    internal bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    internal bool IsPointerOverSubMenu => _popup?.IsPointerOverPopup ?? false;

    #endregion

    #region 运行时字段

    private Popup? _popup;

    private MenuPinnedOpenScope? _pinnedOpenScope;

    private bool _isSyncingSubMenuPopupState;

    private bool _isUsingDetachedTitleBarPopupPlacement;

    private IDisposable? _detachedTitleBarPopupPlacementTracker;

    #endregion

    #region 构造函数

    static MenuItem()
    {
        IsSubMenuOpenProperty.OverrideMetadata<MenuItem>(
            new StyledPropertyMetadata<bool>(coerce: CoerceIsSubMenuOpen));
        AffectsRender<MenuItem>(BackgroundProperty);
        AffectsMeasure<MenuItem>(IconProperty);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<MenuItem>(false);
        ClickEvent.AddClassHandler<MenuItem>(
            (x, e) => x.CloseOwningMenuBeforeClickHandler(e),
            RoutingStrategies.Bubble,
            handledEventsToo: true);
    }

    #endregion

    #region 控件 API

    /// <summary>
    /// Keeps the state of the entry in step with its properties. Before the base member runs, a change of
    /// <c>IsSubMenuOpen</c> opens or closes the popup of the submenu to match and starts or ends the placement
    /// tracking for a detached title bar. After the base member has run, a new <c>Icon</c> gets this entry as
    /// its templated parent, a change of <c>IsChecked</c> raises <see cref="IsCheckStateChanged" />, a change of
    /// <see cref="DisplayPageSize" />, <c>ItemHeight</c>, <c>PopupPadding</c> or <see cref="IsScrollEnabled" />
    /// recalculates the maximum height of the popup, and a change of <c>IsPopupPinnedOpen</c>,
    /// <c>ItemCount</c> or <c>SelectedIndex</c> updates the pinned-open state of the popup and of the child
    /// entries.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IsSubMenuOpenProperty)
        {
            SyncSubMenuPopupOpenState();
            if (change.GetNewValue<bool>())
            {
                ConfigureDetachedTitleBarPopupPlacement();
            }
            else
            {
                DetachedTitleBarPopupSupport.ClearPopupPlacementTracker(
                    ref _detachedTitleBarPopupPlacementTracker);
            }
        }

        base.OnPropertyChanged(change);
        if (change.Property == ParentProperty)
        {
            UpdatePseudoClasses();
            if (Parent is null)
            {
                SemanticLevel = MenuSemanticLevel.None;
            }
        }
        else if (change.Property == IconProperty)
        {
            if (change.OldValue is Icon oldIcon)
            {
                oldIcon.SetTemplatedParent(null);
            }

            if (change.NewValue is Icon newIcon)
            {
                LogicalChildren.Remove(newIcon);
                newIcon.SetTemplatedParent(this);
            }
        }
        else if (change.Property == IsCheckedProperty)
        {
            RaiseEvent(new RoutedEventArgs(IsCheckStateChangedEvent, this));
        }
        else if (change.Property == DisplayPageSizeProperty ||
                 change.Property == ItemHeightProperty ||
                 change.Property == PopupPaddingProperty ||
                 change.Property == IsScrollEnabledProperty)
        {
            ConfigureMaxPopupHeight();
        }
        else if (change.Property == IsPopupPinnedOpenProperty)
        {
            UpdateSubMenuPopupPinnedOpen();
            if (IsPopupPinnedOpen && ItemCount > 0 && !IsSubMenuOpen)
            {
                SetCurrentValue(IsSubMenuOpenProperty, true);
            }

            ReconcilePinnedOpenChildren();
        }
        else if (change.Property == ItemCountProperty && this.IsAttachedToVisualTree())
        {
            // ItemsControl raises ItemCount before its ItemsPresenter processes the same collection
            // change. Opening the popup synchronously here would make two presenters realize the
            // new container. Cross that collection-notification boundary before reconciling state.
            Dispatcher.Post(ReconcileSubMenuAvailability);
        }
        else if (change.Property == SelectedIndexProperty)
        {
            _pinnedOpenScope?.SelectCurrent();
        }
    }

    /// <summary>
    /// Creates the container for a child item: a <see cref="MenuSeparator" /> for a
    /// <see cref="MenuSeparatorData" />, a <see cref="MenuItemGroup" /> for a <see cref="MenuItemGroupData" />,
    /// and a new <see cref="MenuItem" /> with <c>DropdownButtonSemanticParts.ItemClass</c> in its classes for
    /// any other item. <c>recycleKey</c> is ignored; every container is created new.
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

        var menuItem = new MenuItem();
        menuItem.Classes.Add(DropdownButtonSemanticParts.ItemClass);
        return menuItem;
    }

    /// <summary>
    /// Reports whether a child item needs a container. An item that is a <see cref="MenuItem" />, a
    /// <see cref="MenuSeparator" /> or a <see cref="MenuItemGroup" /> is used as its own container.
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
    /// Prepares the container of a child item. A <see cref="MenuItem" /> or <see cref="MenuItemGroup" />
    /// container gets the semantic level <see cref="MenuSemanticLevel.SubMenu" /> while this entry has a level
    /// of its own, and <see cref="MenuSemanticLevel.None" /> otherwise. For an item that is not a
    /// <c>Visual</c>, the header of a <see cref="MenuItem" /> container is set to the item while it has no
    /// value set, and an item that is an <see cref="IMenuItemData" /> also supplies the icon, the key, the
    /// enabled state, the input gesture and the danger flag, each only while it is not set on the entry.
    /// <c>ItemTemplate</c>, <see cref="SizeType" />, <c>IsMotionEnabled</c> and <c>ShouldUseOverlayPopup</c>
    /// are bound through to it, and <see cref="PrepareMenuItem" /> is called last. A <see cref="MenuSeparator" />
    /// is laid out horizontally.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The container is of another type than these three.
    /// </exception>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MenuItem menuItem)
        {
            menuItem.SemanticLevel = ResolveChildSemanticLevel();

            if (item != null && item is not Visual)
            {
                if (!menuItem.IsSet(HeaderProperty))
                {
                    menuItem.SetCurrentValue(HeaderProperty, item);
                }

                if (item is IMenuItemData menuItemData)
                {
                    if (!menuItem.IsSet(IconProperty))
                    {
                        menuItem.SetCurrentValue(IconProperty, menuItemData.Icon);
                    }

                    if (menuItem.ItemKey == null)
                    {
                        menuItem.ItemKey = menuItemData.ItemKey;
                    }

                    if (!menuItem.IsSet(IsEnabledProperty))
                    {
                        menuItem.SetCurrentValue(IsEnabledProperty, menuItemData.IsEnabled);
                    }

                    if (!menuItem.IsSet(InputGestureProperty))
                    {
                        menuItem.SetCurrentValue(InputGestureProperty, menuItemData.InputGesture);
                    }

                    if (!menuItem.IsSet(IsDangerProperty))
                    {
                        menuItem.SetCurrentValue(IsDangerProperty, menuItemData.IsDanger);
                    }
                }
            }

            if (ItemTemplate != null)
            {
                menuItem[!HeaderTemplateProperty] = this[!ItemTemplateProperty];
            }

            menuItem[!ItemTemplateProperty]          = this[!ItemTemplateProperty];
            menuItem[!SizeTypeProperty]              = this[!SizeTypeProperty];
            menuItem[!IsMotionEnabledProperty]       = this[!IsMotionEnabledProperty];
            menuItem[!ShouldUseOverlayPopupProperty] = this[!ShouldUseOverlayPopupProperty];
            PrepareMenuItem(menuItem, item, index);
        }
        else if (container is MenuSeparator menuSeparator)
        {
            menuSeparator.Orientation = Orientation.Horizontal;
        }
        else if (container is MenuItemGroup menuItemGroup)
        {
            // The styles of the group header and of its child items are handled by the template and the container
            // logic of the MenuItemGroup itself.
            menuItemGroup.SemanticLevel = ResolveChildSemanticLevel();
        }
        else if (container is not MenuSeparator)
        {
            throw new ArgumentOutOfRangeException(nameof(container),
                "The container type is incorrect, it must be type MenuItem or MenuSeparator.");
        }
    }

    /// <summary>
    /// Called for each <see cref="MenuItem" /> container after the entry has filled in the header, the item
    /// data and the properties bound through to it. Not called for separators or groups. The default
    /// implementation does nothing.
    /// </summary>
    protected virtual void PrepareMenuItem(MenuItem menuItem, object? item, int index)
    {
    }

    /// <summary>
    /// Updates the pinned-open state of the child entries once the base member has prepared the container of a
    /// child: an entry that is pinned open reconciles its children through its pinned-open scope, and an entry
    /// that is not releases that scope.
    /// </summary>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        ReconcilePinnedOpenChildren();
    }

    /// <summary>
    /// Resets the semantic level of a <see cref="MenuItem" /> or <see cref="MenuItemGroup" /> container to
    /// <see cref="MenuSemanticLevel.None" /> before the base member clears the container.
    /// </summary>
    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is MenuItem item)
        {
            item.SemanticLevel = MenuSemanticLevel.None;
        }
        else if (container is MenuItemGroup group)
        {
            group.SemanticLevel = MenuSemanticLevel.None;
        }
        base.ClearContainerForItemOverride(container);
    }

    /// <summary>
    /// Tells the pinned-open scope of this entry, when it has one, that the submenu of the entry the event
    /// comes from has opened, before the base member handles the event.
    /// </summary>
    protected override void OnSubmenuOpened(RoutedEventArgs e)
    {
        if (e.Source is MenuItem item)
        {
            _pinnedOpenScope?.SubmenuOpened(item);
        }
        base.OnSubmenuOpened(e);
    }

    /// <summary>
    /// Connects the entry to the popup of its template. The popup named <c>PART_Popup</c> replaces the
    /// previous one, which loses the handlers of its <c>Opened</c> and <c>Closed</c> events. The new popup
    /// gets these handlers and the pinned-open state, and is opened from a posted dispatcher callback while
    /// <c>IsSubMenuOpen</c> is <see langword="true" />. The placement tracking for a detached title bar, the
    /// pseudo-classes and the maximum height of the popup are updated as well.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ClearDetachedTitleBarPopupPlacement();
        if (_popup is not null)
        {
            _popup.Opened -= HandleSubMenuPopupOpened;
            _popup.Closed -= HandleSubMenuPopupClosed;
        }

        base.OnApplyTemplate(e);
        // The level markers of the icon and of the content are declared statically by the template, as in NavMenu;
        // they are not injected here by name.
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        if (_popup != null)
        {
            _popup.Opened += HandleSubMenuPopupOpened;
            _popup.Closed += HandleSubMenuPopupClosed;
            UpdateSubMenuPopupPinnedOpen();
            if (IsSubMenuOpen)
            {
                DeferSubMenuPopupOpen(_popup);
            }
        }
        ConfigureDetachedTitleBarPopupPlacement();
        UpdatePseudoClasses();
        ConfigureMaxPopupHeight();
    }

    /// <summary>
    /// Re-evaluates the state of the popup of the submenu once the entry is attached to the visual tree: it
    /// re-coerces <c>IsSubMenuOpen</c>, so that a leaf entry cannot stay open, and opens the popup from a
    /// posted dispatcher callback while the submenu is open but the popup is closed.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Avalonia's keyboard handler may call Open() for the newly selected root item even when
        // it is a leaf. Re-coerce here as well so a leaf configured before attachment cannot leave
        // its template-owned PART_Popup open as an empty shell.
        CoerceValue(IsSubMenuOpenProperty);
        // Closing and reopening the host of a pinned popup for lifecycle reasons detaches the whole menu tree from
        // the visual tree and attaches it again. The value of IsSubMenuOpen is kept while the popup is pinned, as
        // the IsPopupPinnedOpen request drives it, but the open state of the submenu popup is not restored from
        // the property value on its own, so the deferred sync runs once more when the entry is attached again.
        if (IsSubMenuOpen && _popup is { IsOpen: false })
        {
            DeferSubMenuPopupOpen(_popup);
        }
    }

    /// <summary>
    /// Updates the placement tracking for a detached title bar after the base member has attached the entry
    /// to the logical tree.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureDetachedTitleBarPopupPlacement();
    }

    /// <summary>
    /// Stops the placement tracking for a detached title bar before the base member detaches the entry from
    /// the logical tree.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ClearDetachedTitleBarPopupPlacement();
        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    /// Closes the popup of the submenu after the popups of the child entries: each child is closed through
    /// its own <see cref="CloseItemAsync" /> before <c>IsSubMenuOpen</c> is set to <see langword="false" />
    /// here. The task completes when the close has been requested on every level, not when a close motion
    /// has ended.
    /// </summary>
    /// <param name="cancellationToken">
    /// Passed on to the child entries only; this method does not observe it.
    /// </param>
    public async Task CloseItemAsync(CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is MenuItem childMenuItem)
            {
                await childMenuItem.CloseItemAsync(cancellationToken);
            }
        }

        IsSubMenuOpen = false;
    }

    #endregion

    #region 内部协作 API

    internal void ReconcilePinnedOpenChildren()
    {
        if (IsPopupPinnedOpen)
        {
            (_pinnedOpenScope ??= new MenuPinnedOpenScope(this)).Reconcile();
        }
        else
        {
            _pinnedOpenScope?.Release();
        }
    }

    internal void CloseForLifecycle()
    {
        IsPopupPinnedOpen = false;
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is MenuItem childMenuItem)
            {
                childMenuItem.CloseForLifecycle();
            }
        }

        _popup?.CloseForLifecycle();
        SetCurrentValue(IsSubMenuOpenProperty, false);
    }

    #endregion

    #region 实现 ITreeNode<IMenuItemData> 接口

    IEnumerable<IMenuItemData> ITreeNode<IMenuItemData>.Children => EnumerateChildren();

    #endregion

    #region 内部实现

    private IEnumerable<IMenuItemData> EnumerateChildren()
    {
        foreach (var item in Items)
        {
            if (item is IMenuItemData menuItem)
            {
                yield return menuItem;
            }
        }
    }

    private void ReconcileSubMenuAvailability()
    {
        UpdateSubMenuPopupPinnedOpen();
        CoerceValue(IsSubMenuOpenProperty);
        if (IsPopupPinnedOpen && ItemCount > 0 && !IsSubMenuOpen)
        {
            SetCurrentValue(IsSubMenuOpenProperty, true);
        }

        ReconcilePinnedOpenChildren();
    }

    private static bool CoerceIsSubMenuOpen(AvaloniaObject sender, bool value)
    {
        // ItemCount is the source of truth here. Avalonia publishes ItemCount changes before it
        // updates the :empty pseudo-class that backs HasSubMenu, so coercion during a collection
        // transition must not depend on HasSubMenu's transient value.
        if (sender is MenuItem { ItemCount: 0 } leaf && leaf.IsAttachedToVisualTree())
        {
            return false;
        }

        // Reject a normal collapse before Avalonia closes descendants and publishes
        // SubmenuOpened again. Lifecycle teardown unpins before requesting closure.
        return value || sender is MenuItem { IsPopupPinnedOpen: true, HasSubMenu: true } item &&
            item.IsAttachedToVisualTree() && item._popup?.CanOpenPinnedPopup() == true;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(MenuItemPseudoClass.TopLevel, IsTopLevel);
    }

    // The level of this entry decides the level of its child containers: the children of a top-level item of a
    // plain Menu belong to the submenu level, and the children of a submenu item stay at the submenu level.
    // None means that the entry is outside the semantic scope of a plain Menu; a child container then keeps
    // the marker behavior of its consumer.
    private MenuSemanticLevel ResolveChildSemanticLevel()
    {
        return SemanticLevel == MenuSemanticLevel.None
            ? MenuSemanticLevel.None
            : MenuSemanticLevel.SubMenu;
    }

    // The pinned state is handed down the menu tree (host flyout, presenter, then the entries at each level),
    // but it must act only on the popups that really host a submenu: a pinned popup forces itself open, and
    // the PART_Popup of a leaf item that is made pinned opens as an empty popup without content and stays
    // behind in the overlay layer, with only a rounded white background and a shadow.
    private void UpdateSubMenuPopupPinnedOpen()
    {
        if (_popup is null)
        {
            return;
        }

        _popup.IsPopupPinnedOpen = IsPopupPinnedOpen && ItemCount > 0;
    }

    private void SyncSubMenuPopupOpenState()
    {
        // Before the template is applied (declarative IsSubMenuOpen="True") the sync is left to OnApplyTemplate:
        // the popup of the submenu must not be opened synchronously while the template is applied or the parent
        // popup is forced to lay out.
        if (_popup is null)
        {
            return;
        }

        // The open state and the presentation of the popup propagate in both directions: writing Popup.IsOpen here
        // raises Opened and Closed synchronously, and the handlers write IsSubMenuOpen back and re-enter this
        // method. When the popup cannot be shown (the placement target has left the visible rectangle of the
        // TopLevel, for example a menu below the fold of a long page) the write-back does not converge and
        // recurses without bound until the stack overflows.
        // While this method is already driving the popup it does not drive it again: the popup is already in the
        // target state, so a sync that enters again has nothing to do.
        if (_isSyncingSubMenuPopupState)
        {
            return;
        }

        _isSyncingSubMenuPopupState = true;
        try
        {
            // The open state of the popup of the submenu is not written while the host popup is closed (after a
            // lifecycle close caused by switching to another tab or by scrolling out, the pinned rebound sets
            // IsSubMenuOpen back to true): opening it at that moment would leave the popup hanging as a blank
            // orphan with only the card shadow. The value of IsSubMenuOpen is kept, and the deferred sync of
            // OnAttachedToVisualTree restores the popup once the host has reopened and the menu tree is
            // attached again.
            // The check only intercepts a menu tree that really sits under a popup host (the MenuFlyout case);
            // a plain Menu has no popup host and is not affected.
            if (IsSubMenuOpen &&
                this.FindLogicalAncestorOfType<Popup>() is { IsOpen: false })
            {
                return;
            }

            _popup.IsOpen = IsSubMenuOpen;
        }
        finally
        {
            _isSyncingSubMenuPopupState = false;
        }
    }

    private void DeferSubMenuPopupOpen(Popup popup)
    {
        // A declarative open (IsSubMenuOpen="True", the counterpart of the upstream defaultOpenKeys) is carried
        // out when the template is applied. At that moment the parent popup is usually inside the forced layout
        // or template application of OverlayPopupHost.Show, and a synchronous Open() would modify the collection
        // while PopupOverlayLayer.MeasureOverride enumerates Children ("Collection was modified"), so the open is
        // deferred to the next dispatcher frame.
        Dispatcher.Post(() =>
        {
            if (ReferenceEquals(popup, _popup) && IsSubMenuOpen && !popup.IsOpen)
            {
                popup.IsOpen = true;
            }
        });
    }

    private void HandleSubMenuPopupOpened(object? sender, EventArgs e)
    {
        if (!IsSubMenuOpen)
        {
            SetCurrentValue(IsSubMenuOpenProperty, true);
        }
    }

    private void HandleSubMenuPopupClosed(object? sender, EventArgs e)
    {
        if (IsSubMenuOpen)
        {
            SetCurrentValue(IsSubMenuOpenProperty, false);
        }
    }

    private void CloseOwningMenuBeforeClickHandler(RoutedEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this) || HasSubMenu || StaysOpenOnClick)
        {
            return;
        }

        CloseOwningMenuImmediately();
    }

    private void CloseOwningMenuImmediately()
    {
        StyledElement? current = Parent;
        while (current != null)
        {
            if (current is Menu menu)
            {
                menu.CloseImmediately();
                return;
            }

            if (current is ContextMenu contextMenu)
            {
                contextMenu.Close();
                return;
            }

            if (current is MenuItem menuItem)
            {
                menuItem.Close();
            }

            current = current.Parent;
        }
    }

    private void ConfigureMaxPopupHeight()
    {
        var maxPopupHeight = IsScrollEnabled
            ? ItemHeight * DisplayPageSize + PopupPadding.Top + PopupPadding.Bottom
            : double.PositiveInfinity;
        SetCurrentValue(MaxPopupHeightProperty, maxPopupHeight);
    }

    private void ConfigureDetachedTitleBarPopupPlacement()
    {
        DetachedTitleBarPopupSupport.ConfigurePopupPlacement(
            this,
            _popup,
            ref _isUsingDetachedTitleBarPopupPlacement,
            IsTopLevel);
        _detachedTitleBarPopupPlacementTracker =
            DetachedTitleBarPopupSupport.UpdatePopupPlacementTracker(
                this,
                _popup,
                _detachedTitleBarPopupPlacementTracker,
                () => IsSubMenuOpen,
                IsTopLevel && IsSubMenuOpen);
    }

    private void ClearDetachedTitleBarPopupPlacement()
    {
        DetachedTitleBarPopupSupport.ClearPopupPlacementTracker(
            ref _detachedTitleBarPopupPlacementTracker);
        DetachedTitleBarPopupSupport.ClearPopupPlacement(
            _popup,
            ref _isUsingDetachedTitleBarPopupPlacement);
    }

    #endregion
}
