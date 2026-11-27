using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Presents the items of a <see cref="AtomUI.Desktop.Controls.MenuFlyout" /> as the menu of its popup. The
/// theme wraps the items in an arrow decorated box and a scroll host, and limits the height of the popup
/// to <see cref="DisplayPageSize" /> item rows while <see cref="IsScrollEnabled" /> is <see langword="true" />.
/// The flyout that creates the presenter relays its arrow, motion and scrolling settings to it; the
/// presenter relays motion, size type, page size and the overlay popup switch to its menu items.
/// </summary>
public class MenuFlyoutPresenter : MenuBase,
                                   IArrowAwareShadowMaskInfoProvider,
                                   IMotionAwareControl,
                                   ICustomizableSizeTypeAware,
                                   IScrollAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="IsArrowVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<MenuFlyoutPresenter>();

    /// <summary>
    /// Identifies the <see cref="ArrowPosition" /> property.
    /// </summary>
    public static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<MenuFlyoutPresenter>();

    /// <summary>
    /// Identifies the <see cref="DisplayPageSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> DisplayPageSizeProperty =
        Menu.DisplayPageSizeProperty.AddOwner<MenuFlyoutPresenter>();

    /// <summary>
    /// Identifies the <see cref="MenuItemClicked" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<FlyoutMenuItemClickedEventArgs> MenuItemClickedEvent =
        RoutedEvent.Register<MenuFlyoutPresenter, FlyoutMenuItemClickedEventArgs>(
            nameof(MenuItemClicked),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<MenuFlyoutPresenter>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<MenuFlyoutPresenter>();

    /// <summary>
    /// Identifies the <see cref="IsScrollEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        ScrollAwareControlProperty.IsScrollEnabledProperty.AddOwner<MenuFlyoutPresenter>();

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        Menu.ShouldUseOverlayPopupProperty.AddOwner<MenuFlyoutPresenter>();

    /// <summary>
    /// Gets or sets whether the popup shows its indicator arrow. The theme forwards the value to the
    /// <see cref="ArrowDecoratedBox" /> of the template: while it is <see langword="false" />, the arrow is not
    /// laid out and the outline of a bordered box leaves it out. The hosting flyout relays its effective arrow
    /// visibility here. Without a value, the default that the arrow decorated box gives the property applies.
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the edge of the popup box that carries the indicator arrow and, for the edge-aligned
    /// values, the end of the edge it sits toward. The value moves only the arrow; where the popup lies next
    /// to its target is up to the host. The theme forwards the value to the <see cref="ArrowDecoratedBox" /> of
    /// the template, and the hosting flyout relays its own arrow position here. Without a value, the default
    /// that the arrow decorated box gives the property applies.
    /// </summary>
    public ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of item rows the popup shows before it scrolls. While
    /// <see cref="IsScrollEnabled" /> is <see langword="true" />, the height of the popup is limited to this
    /// many rows of <c>ItemHeight</c> plus the vertical padding. The value is relayed to every
    /// <see cref="MenuItem" /> container. Without a value, the default that <c>Menu</c>, the owner of the
    /// property, gives it applies.
    /// </summary>
    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the size type of the menu. The presenter does not read the value itself; it is relayed to
    /// every <see cref="MenuItem" /> container.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether motion is enabled for the popup. The theme forwards the value to the scroll host of
    /// the template, the presenter relays it to every <see cref="MenuItem" /> container, and the hosting flyout
    /// relays its own value here. Without a value, the <c>EnableMotion</c> token of the theme applies.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the popup scrolls. The theme forwards the value to the scroll host of the template,
    /// and the hosting flyout relays its own value here. While it is <see langword="true" />, the height of the
    /// popup is limited to <see cref="DisplayPageSize" /> item rows plus the padding; while it is
    /// <see langword="false" />, the popup has no height limit.
    /// </summary>
    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    /// <summary>
    /// Raised when the click of a <see cref="MenuItem" /> reaches the presenter. The sender is the presenter and
    /// <see cref="FlyoutMenuItemClickedEventArgs.Item" /> is the clicked item. The event bubbles up from the
    /// presenter; the hosting flyout forwards it unchanged.
    /// </summary>
    public event EventHandler<FlyoutMenuItemClickedEventArgs>? MenuItemClicked
    {
        add => AddHandler(MenuItemClickedEvent, value);
        remove => RemoveHandler(MenuItemClickedEvent, value);
    }

    /// <summary>
    /// Gets or sets the overlay popup switch of the menu. The presenter does not read the value itself: it is
    /// relayed to every <see cref="MenuItem" /> container, and the hosting flyout relays its own value here.
    /// Without a value, the default that <c>Menu</c>, the owner of the property, gives it applies.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// The flyout that created this presenter, or <see langword="null" />. A flyout assigns itself here when it
    /// creates the presenter.
    /// </summary>
    public MenuFlyout? MenuFlyout { get; set; }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<MenuFlyoutPresenter, double>(nameof(ItemHeight));

    // The presenter pins only its active branch; leaf and sibling containers do not inherit the pin.
    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<MenuFlyoutPresenter>();

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetValue(IsPopupPinnedOpenProperty, value);
    }

    internal static readonly StyledProperty<double> MaxPopupHeightProperty =
        AvaloniaProperty.Register<MenuFlyoutPresenter, double>(nameof(MaxPopupHeight));

    internal double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    internal double MaxPopupHeight
    {
        get => GetValue(MaxPopupHeightProperty);
        set => SetValue(MaxPopupHeightProperty, value);
    }

    #endregion

    private ArrowDecoratedBox? _arrowDecoratedBox;
    private MenuPinnedOpenScope? _pinnedOpenScope;

    static MenuFlyoutPresenter()
    {
        Avalonia.Controls.MenuItem.ClickEvent.AddClassHandler<MenuFlyoutPresenter>(
            (presenter, args) => presenter.HandleMenuItemClicked(args));
    }

    public MenuFlyoutPresenter()
        : base(new DefaultMenuInteractionHandler(true))
    {
    }

    /// <summary>
    /// Creates the presenter with <paramref name="menuInteractionHandler" /> as the handler of its menu
    /// interaction. Use it to install a handler other than the <c>DefaultMenuInteractionHandler</c> that the
    /// parameterless constructor installs.
    /// </summary>
    public MenuFlyoutPresenter(IMenuInteractionHandler menuInteractionHandler)
        : base(menuInteractionHandler)
    {
    }

    /// <summary>
    /// Closes the popup that hosts the presenter: cancels the hover operations that the default interaction
    /// handler has pending, resets <c>SelectedIndex</c> to -1 and sets <c>IsOpen</c> to <see langword="false" />
    /// on the <see cref="Popup" /> that is a logical ancestor of the presenter. Without such a
    /// <see cref="Popup" />, only the hover operations are canceled. Nothing happens while the popup is pinned
    /// open.
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

        // DefaultMenuInteractionHandler calls this
        var host = this.FindLogicalAncestorOfType<Popup>();
        if (host != null)
        {
            SelectedIndex = -1;
            host.IsOpen   = false;
        }
    }

    /// <summary>
    /// Called when a property of the presenter changes. After the base call it recalculates the maximum popup
    /// height when <see cref="DisplayPageSize" />, <see cref="IsScrollEnabled" />, <c>ItemHeight</c> or
    /// <c>Padding</c> changes, applies the pinned-open state to the item containers or releases it when
    /// <c>IsPopupPinnedOpen</c> or the item count changes, and passes a change of <c>SelectedIndex</c> on to
    /// the pinned-open scope while the popup is pinned. Overrides must call the base member to keep these in
    /// effect.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DisplayPageSizeProperty ||
            change.Property == ItemHeightProperty ||
            change.Property == PaddingProperty ||
            change.Property == IsScrollEnabledProperty)
        {
            ConfigureMaxPopupHeight();
        }
        else if (change.Property == IsPopupPinnedOpenProperty || change.Property == ItemCountProperty)
        {
            ReconcilePinnedOpenChildren();
        }
        else if (change.Property == SelectedIndexProperty)
        {
            _pinnedOpenScope?.SelectCurrent();
        }
    }

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

    /// <summary>
    /// Called after the base member has prepared the container of an item. It re-applies the pinned-open state
    /// of the presenter, so that the new container follows it. Overrides must call the base member to keep
    /// this.
    /// </summary>
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        ReconcilePinnedOpenChildren();
    }

    /// <summary>
    /// Called when a submenu opens inside the presenter. It tells the pinned-open scope which menu item opened
    /// its submenu, so that the pin follows that branch, and then calls the base member. Overrides must call
    /// the base member.
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
    /// Creates the container for an item: a <see cref="MenuSeparator" /> for a <see cref="MenuSeparatorData" />,
    /// a <see cref="MenuItemGroup" /> for a <see cref="MenuItemGroupData" /> and, for any other item, a
    /// <see cref="MenuItem" /> that carries <c>DropdownButtonSemanticParts.ItemClass</c>. Items that are menu
    /// controls of these kinds are their own containers and do not reach this method.
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
    /// Returns <see langword="true" /> with <c>DefaultRecycleKey</c> for an item that needs a container of its
    /// own, and <see langword="false" /> with <see langword="null" /> for an item that is its own container: a
    /// <see cref="MenuItem" />, a <see cref="MenuSeparator" /> or a <see cref="MenuItemGroup" />.
    /// </summary>
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
    /// Fills the container of an item from the item and from the settings of the presenter, calls
    /// <see cref="PrepareMenuItem" /> and then the base member.
    /// </summary>
    /// <param name="container">
    /// The container of the item, or the item itself when it is a menu control.
    /// </param>
    /// <param name="item">
    /// The item data. A value that is not a <c>Visual</c> becomes the header of a <see cref="MenuItem" />
    /// container, and a value that implements <c>IMenuItemData</c> also supplies the icon, key, enabled state,
    /// input gesture and danger flag. Each of these values is set only while the container has none of its own.
    /// </param>
    /// <param name="index">
    /// The index of the item, passed on to <see cref="PrepareMenuItem" />.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="container" /> is neither a <see cref="MenuItem" />, a <see cref="MenuSeparator" /> nor a
    /// <see cref="MenuItemGroup" />.
    /// </exception>
    /// <remarks>
    /// A <see cref="MenuItem" /> container gets <c>MenuSemanticLevel.None</c> and the item marker class of
    /// <c>DropdownButtonSemanticParts</c>, and is bound to <c>ItemTemplate</c>, <see cref="IsMotionEnabled" />,
    /// <see cref="SizeType" />, <see cref="DisplayPageSize" /> and <see cref="ShouldUseOverlayPopup" /> of the
    /// presenter. A <see cref="MenuSeparator" /> is laid out horizontally and a <see cref="MenuItemGroup" />
    /// gets <c>MenuSemanticLevel.None</c>.
    /// </remarks>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (container is MenuItem menuItem)
        {
            menuItem.SemanticLevel = MenuSemanticLevel.None;
            menuItem.Classes.Add(DropdownButtonSemanticParts.ItemClass);

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

            menuItem[!MenuItem.IsMotionEnabledProperty]       = this[!IsMotionEnabledProperty];
            menuItem[!MenuItem.ItemTemplateProperty]          = this[!ItemTemplateProperty];
            menuItem[!MenuItem.SizeTypeProperty]              = this[!SizeTypeProperty];
            menuItem[!MenuItem.DisplayPageSizeProperty]       = this[!DisplayPageSizeProperty];
            menuItem[!MenuItem.ShouldUseOverlayPopupProperty] = this[!ShouldUseOverlayPopupProperty];

            PrepareMenuItem(menuItem, item, index);
        }
        else if (container is MenuSeparator menuSeparator)
        {
            menuSeparator.Orientation = Orientation.Horizontal;
        }
        else if (container is MenuItemGroup group)
        {
            group.SemanticLevel = MenuSemanticLevel.None;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container),
                "The container type is incorrect, it must be type MenuItem or MenuSeparator.");
        }

        base.PrepareContainerForItemOverride(container, item, index);
    }

    /// <summary>
    /// Called for a <see cref="MenuItem" /> container after
    /// <see cref="PrepareContainerForItemOverride" /> has filled it from the item and bound it to the settings
    /// of the presenter, and before the base member of that method runs. The default implementation does
    /// nothing; an override adjusts the container for the item and need not call the base member.
    /// </summary>
    protected virtual void PrepareMenuItem(MenuItem menuItem, object? item, int index)
    {
    }

    private void HandleMenuItemClicked(RoutedEventArgs args)
    {
        if (args.Source is MenuItem menuItem)
        {
            var ev = new FlyoutMenuItemClickedEventArgs(MenuItemClickedEvent, menuItem);
            RaiseEvent(ev);
        }
    }

    /// <summary>
    /// Always throws; the presenter cannot be opened from here.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// Thrown on every call. The flyout creates and opens the presenter; call <c>ShowAt</c> of the flyout
    /// instead.
    /// </exception>
    public override void Open()
    {
        throw new NotSupportedException("Use MenuFlyout.ShowAt(Control) instead");
    }

    /// <summary>
    /// Called when the template of the presenter is applied. After the base call it captures the
    /// <see cref="ArrowDecoratedBox" /> of the template, adds the popup root marker class of
    /// <c>DropdownButtonSemanticParts</c> to it, and recalculates the maximum popup height. Overrides must call
    /// the base member to keep the arrow, the shadow mask information and the height limit working.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _arrowDecoratedBox =
            e.NameScope.Find<ArrowDecoratedBox>(AbstractArrowDecoratedBox.ArrowDecoratorPart);
        // The popup.root semantic part is the root visual of the popup (the ArrowDecoratedBox), not the shared
        // MenuFlyoutPresenter container: the border, background and corner radius are rendered by the
        // PART_ContentDecorator of the ArrowDecoratedBox. The marker class cannot be declared statically in the
        // shared ArrowDecoratedBox theme, because that would also mark other ArrowDecoratedBox popups, so it is
        // injected at runtime when the template is applied, as the DropdownButton semantic part contract defines.
        _arrowDecoratedBox?.Classes.Add(DropdownButtonSemanticParts.PopupRootClass);
        ConfigureMaxPopupHeight();
    }

    /// <summary>
    /// Called when the presenter leaves the visual tree. After the base call it closes the submenu of every
    /// top-level menu item, unless the popup is pinned open: that close is temporary and must keep the
    /// declarative submenu state. Overrides must call the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        // A pinned popup closes because its placement target is gone (its tab was switched away or it scrolled out
        // of the viewport). That is a recoverable temporary close, not the user collapsing the menu: the
        // declarative submenu state is kept, and MenuItem restores the submenu presentation with its deferred
        // synchronization once the menu tree is attached again. The cleanup below is for an ordinary close only,
        // so that it does not wipe the declarative expanded state.
        if (IsPopupPinnedOpen)
        {
            return;
        }

        foreach (var i in LogicalChildren)
        {
            if (i is MenuItem menuItem)
            {
                menuItem.IsSubMenuOpen = false;
            }
        }
    }

    /// <summary>
    /// Returns the corner radius of the arrow decorated box of the template, for the shadow mask. Returns a
    /// corner radius of <c>0</c> before the template is applied or when the template has no arrow decorated box.
    /// </summary>
    public CornerRadius GetMaskCornerRadius()
    {
        if (_arrowDecoratedBox is not null)
        {
            return _arrowDecoratedBox.CornerRadius;
        }

        return new CornerRadius(0);
    }

    /// <summary>
    /// Returns the mask bounds that the arrow decorated box of the template reports, translated into the
    /// coordinate space of the presenter, for the shadow mask. Returns <c>Bounds</c> of the presenter before
    /// the template is applied or when the template has no arrow decorated box.
    /// </summary>
    public Rect GetMaskBounds()
    {
        if (_arrowDecoratedBox is not null)
        {
            var contentRect = _arrowDecoratedBox.GetMaskBounds();
            var adjustedPos = _arrowDecoratedBox.TranslatePoint(contentRect.Position, this) ?? default;
            return new Rect(adjustedPos, contentRect.Size);
        }

        return Bounds;
    }

    /// <summary>
    /// Returns the background of the arrow decorated box of the template, for the shadow mask, or
    /// <see langword="null" /> before the template is applied, when the template has no arrow decorated box, or
    /// when the box has no background.
    /// </summary>
    public IBrush? GetMaskBackground()
    {
        return _arrowDecoratedBox?.Background;
    }

    ArrowPosition IArrowAwareShadowMaskInfoProvider.GetArrowPosition()
    {
        return _arrowDecoratedBox?.ArrowPosition ?? default;
    }

    bool IArrowAwareShadowMaskInfoProvider.IsArrowVisible()
    {
        // Without the arrow part, report no arrow; the popup then falls back to the no-arrow shadow and placement.
        return _arrowDecoratedBox?.IsArrowVisible ?? false;
    }

    void IArrowAwareShadowMaskInfoProvider.SetArrowOpacity(double opacity)
    {
        if (_arrowDecoratedBox is not null)
        {
            _arrowDecoratedBox.ArrowOpacity = opacity;
        }
    }

    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorBounds()
    {
        return _arrowDecoratedBox?.ArrowIndicatorBounds ?? default;
    }

    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorLayoutBounds()
    {
        return _arrowDecoratedBox?.ArrowIndicatorLayoutBounds ?? default;
    }

    AbstractArrowDecoratedBox? IArrowAwareShadowMaskInfoProvider.GetArrowDecoratedBox()
    {
        return _arrowDecoratedBox;
    }

    private void ConfigureMaxPopupHeight()
    {
        var maxPopupHeight = IsScrollEnabled
            ? ItemHeight * DisplayPageSize + Padding.Top + Padding.Bottom
            : double.PositiveInfinity;
        SetCurrentValue(MaxPopupHeightProperty, maxPopupHeight);
    }
}

/// <summary>
/// The arguments of the <see cref="MenuFlyoutPresenter.MenuItemClicked" /> event: they carry the menu item
/// that was clicked.
/// </summary>
public class FlyoutMenuItemClickedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// The menu item whose click raised the event.
    /// </summary>
    public MenuItem Item { get; }

    /// <summary>
    /// Creates the arguments for a click on the given menu item.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event that the arguments are raised with; the presenter passes
    /// <see cref="MenuFlyoutPresenter.MenuItemClickedEvent" />.
    /// </param>
    /// <param name="menuItem">
    /// The clicked menu item, exposed as <see cref="Item" />.
    /// </param>
    public FlyoutMenuItemClickedEventArgs(RoutedEvent routedEvent, MenuItem menuItem)
        : base(routedEvent)
    {
        Item = menuItem;
    }
}
