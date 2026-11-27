using System.ComponentModel;
using AtomUI.Controls;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaContextMenu = Avalonia.Controls.ContextMenu;

/// <summary>
/// A context menu: the menu that a control attaches through its <c>ContextMenu</c> property. An item that is a
/// <see cref="MenuSeparatorData" /> is shown as a <see cref="MenuSeparator" />; every other item becomes a
/// <see cref="MenuItem" />. The items are shown in a <see cref="Popup" /> owned by the menu, to which the shadow
/// and motion settings of the menu are handed.
/// </summary>
public class ContextMenu : AvaloniaContextMenu,
                           ICustomizableSizeTypeAware,
                           IMotionAwareControl,
                           IScrollAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="PopupRootShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> PopupRootShadowProperty =
        Popup.PopupRootShadowProperty.AddOwner<ContextMenu>();

    /// <summary>
    /// Identifies the <see cref="OverlayHostShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> OverlayHostShadowProperty =
        Popup.OverlayHostShadowProperty.AddOwner<ContextMenu>();

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ContextMenu>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ContextMenu>();

    /// <summary>
    /// Identifies the <see cref="IsScrollEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        ScrollAwareControlProperty.IsScrollEnabledProperty.AddOwner<ContextMenu>();

    /// <summary>
    /// Identifies the <see cref="DisplayPageSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> DisplayPageSizeProperty =
        Menu.DisplayPageSizeProperty.AddOwner<ContextMenu>();

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        Menu.ShouldUseOverlayPopupProperty.AddOwner<ContextMenu>();

    /// <summary>
    /// Identifies the <see cref="MotionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<ContextMenu>();

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<ContextMenu>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<ContextMenu>();

    /// <summary>
    /// Shadow around the frame of the popup of the menu while the popup is not hosted in the overlay layer, which
    /// is the case while <see cref="ShouldUseOverlayPopup" /> is <see langword="false" />. The value is handed to
    /// the popup through a binding. The theme sets it from the <c>MenuPopupShadows</c> token of the context menu,
    /// the same value as <see cref="OverlayHostShadow" />.
    /// </summary>
    public BoxShadows PopupRootShadow
    {
        get => GetValue(PopupRootShadowProperty);
        set => SetValue(PopupRootShadowProperty, value);
    }

    /// <summary>
    /// Shadow around the frame of the popup of the menu while the popup is hosted in the overlay layer of the
    /// window, which is the case while <see cref="ShouldUseOverlayPopup" /> is <see langword="true" />. The value
    /// is handed to the popup through a binding. The theme sets it from the <c>MenuPopupShadows</c> token of the
    /// context menu, the same value as <see cref="PopupRootShadow" />.
    /// </summary>
    public BoxShadows OverlayHostShadow
    {
        get => GetValue(OverlayHostShadowProperty);
        set => SetValue(OverlayHostShadowProperty, value);
    }

    /// <summary>
    /// Size preset of the menu items, bound through to every <see cref="MenuItem" /> the menu creates, whose theme
    /// applies the preset. Set it on the context menu: the value does not inherit. The theme of the context menu
    /// sets no value, so the default is <c>Middle</c>.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Enables the open and close motion of the popup of the menu. The theme sets it from the <c>EnableMotion</c>
    /// token of the context menu. The value is handed to the popup and bound through to every
    /// <see cref="MenuItem" />, so the entries and their popups follow it.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Whether the items are hosted in a scroll viewer. With it, a menu with more than
    /// <see cref="DisplayPageSize" /> rows is limited to that many item heights plus the padding of the menu and
    /// scrolls. Without it the popup gets no height limit from the menu and the items are shown without a scroll
    /// viewer. The value inherits, so the menu items follow the value set on the context menu. The default is
    /// <see langword="true" />.
    /// </summary>
    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    /// <summary>
    /// Number of item rows the popup shows at most. It takes effect only while <see cref="IsScrollEnabled" /> is
    /// <see langword="true" />: the height of the popup is then limited to this many item heights plus the padding
    /// of the menu, and further rows are reached by scrolling. The value is bound through to every
    /// <see cref="MenuItem" />. The theme sets 8.
    /// </summary>
    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
    }

    /// <summary>
    /// Whether the popup of the menu is hosted in the overlay layer of the window (<see langword="true" />) or
    /// outside it (<see langword="false" />). The theme sets it to <see langword="true" />. The value is handed to
    /// the popup and bound through to every <see cref="MenuItem" />, which hands it to its own popup.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Duration of the open and close motion of the popup of the menu. The value is handed to the popup through a
    /// binding. The theme sets it from the <c>MotionDurationMid</c> token of the context menu.
    /// </summary>
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    /// <summary>
    /// Motion played when the popup of the menu opens, or <see langword="null" /> to open it without motion. The
    /// value is handed to the popup through a binding. The theme sets <c>PopupMotionDefaults.DropdownOpen</c>.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// Motion played when the popup of the menu closes, or <see langword="null" /> to close it without motion. The
    /// value is handed to the popup through a binding. The theme sets <c>PopupMotionDefaults.DropdownClose</c>.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<ContextMenu, double>(nameof(ItemHeight));

    internal static readonly StyledProperty<double> MaxPopupHeightProperty =
        AvaloniaProperty.Register<ContextMenu, double>(nameof(MaxPopupHeight));

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<ContextMenu>();

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

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion

    #region 运行时字段

    private Popup? _popup;

    private IDisposable? _deactivationSubscription;

    private bool _ignorePlacementChanged;

    private Control? _pinnedOpenOwner;

    #endregion

    #region 构造函数

    static ContextMenu()
    {
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<ContextMenu>(false);
        Control.ContextMenuProperty.Changed.AddClassHandler<Control>(HandleOwnerContextMenuChanged);
    }

    public ContextMenu()
        : base(new DefaultMenuInteractionHandler(true))
    {
        CreatePopup();
    }

    #endregion

    #region 控件 API

    /// <summary>
    /// Sets <c>CustomPopupPlacementCallback</c> to the placement handler of the <see cref="Popup" /> owned by
    /// the menu, after the base member has run.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        CustomPopupPlacementCallback =  _popup!.HandleCustomPlacement;
    }

    /// <summary>
    /// Creates the container for an item: <see cref="MenuSeparator" /> for a <see cref="MenuSeparatorData" /> and
    /// <see cref="MenuItem" /> for any other item. <c>recycleKey</c> is ignored; every container is created new.
    /// </summary>
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (item is MenuSeparatorData)
        {
            return new MenuSeparator();
        }

        return new MenuItem();
    }

    /// <summary>
    /// Reports whether an item needs a container. An item that is a <see cref="MenuItem" /> or a
    /// <see cref="MenuSeparator" /> is used as its own container.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the item is anything else; <c>recycleKey</c> is then <c>DefaultRecycleKey</c>.
    /// <see langword="false" /> for the two container types; <c>recycleKey</c> is then <see langword="null" />.
    /// </returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (item is MenuItem or MenuSeparator)
        {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
    }

    /// <summary>
    /// Prepares the container of an item. A <see cref="MenuItem" /> gets the semantic level
    /// <see cref="MenuSemanticLevel.None" />. For an item that is not a <c>Visual</c>, the header is set to the item
    /// while it has no value set, and an item that is an <see cref="IMenuItemData" /> also supplies the icon, the
    /// key, the enabled state, the input gesture and the danger flag, each only while it is not set on the item.
    /// The header template, <see cref="IsMotionEnabled" />, <see cref="SizeType" />,
    /// <see cref="DisplayPageSize" /> and <see cref="ShouldUseOverlayPopup" /> are bound through to it, and
    /// <see cref="PrepareMenuItem" /> is called last. A <see cref="MenuSeparator" /> is laid out vertically.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The container is of another type than these two.
    /// </exception>
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is MenuItem menuItem)
        {
            menuItem.SemanticLevel = MenuSemanticLevel.None;
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

            menuItem[!IsMotionEnabledProperty]                = this[!IsMotionEnabledProperty];
            menuItem[!ItemTemplateProperty]                   = this[!ItemTemplateProperty];
            menuItem[!SizeTypeProperty]                       = this[!SizeTypeProperty];
            menuItem[!MenuItem.DisplayPageSizeProperty]       = this[!DisplayPageSizeProperty];
            menuItem[!MenuItem.ShouldUseOverlayPopupProperty] = this[!ShouldUseOverlayPopupProperty];

            PrepareMenuItem(menuItem, item, index);
        }
        else if (container is MenuSeparator menuSeparator)
        {
            menuSeparator.Orientation = Orientation.Vertical;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container),
                "The container type is incorrect, it must be type MenuItem or MenuSeparator.");
        }
    }

    /// <summary>
    /// Called for each <see cref="MenuItem" /> container after the context menu has filled in its header, its item
    /// data and the forwarded properties. Not called for separators. The default implementation does nothing.
    /// </summary>
    protected virtual void PrepareMenuItem(MenuItem menuItem, object? item, int index)
    {
    }

    /// <summary>
    /// Closes the popup of the context menu. Does nothing while the popup is pinned open (<c>IsPopupPinnedOpen</c>).
    /// Otherwise the pending hover operations of the interaction handler are canceled first; when the menu is not
    /// open, the call ends there, and when it is open, the popup is closed.
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

        if (!IsOpen)
        {
            return;
        }

        _popup!.IsOpen = false;
    }

    /// <summary>
    /// Keeps the popup of the menu in step with the menu after the base member has run. A change of
    /// <see cref="DisplayPageSize" />, <c>ItemHeight</c>, <c>Padding</c> or <see cref="IsScrollEnabled" />
    /// recalculates the maximum height of the popup. A change of <c>Placement</c> hands a placement other than
    /// <c>Custom</c> to the popup and makes <c>Custom</c> the placement of the menu. <see langword="true" /> for
    /// <c>IsPopupPinnedOpen</c> opens the menu on its owner when the menu is closed and the owner is attached
    /// to the visual tree.
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
        else if (change.Property == PlacementProperty)
        {
            ConfigurePlacement();
        }
        else if (change.Property == IsPopupPinnedOpenProperty && change.GetNewValue<bool>())
        {
            TryOpenPinned();
        }
    }

    /// <summary>
    /// Recalculates the maximum height of the popup and hands the placement of the menu to the popup once the
    /// template has been applied, after the base member has run.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureMaxPopupHeight();
        ConfigurePlacement();
    }

    #endregion

    #region 内部实现

    private Popup CreatePopup()
    {
        _popup = new Popup
        {
            IsLightDismissEnabled          = true,
            OverlayDismissEventPassThrough = true,
            TakesFocusFromNativeControl    = Popup.GetTakesFocusFromNativeControl(this),
        };
        CustomPopupPlacementCallback =  _popup.HandleCustomPlacement;
        _popup.Opened                += HandlePopupOpened;
        _popup.Closed                += HandlePopupClosed;
        _popup.CloseRequested += HandlePopupClosing;
        _popup.KeyUp += HandlePopupKeyUp;

        _popup[!Popup.PopupRootShadowProperty]       = this[!PopupRootShadowProperty];
        _popup[!Popup.OverlayHostShadowProperty]     = this[!OverlayHostShadowProperty];
        _popup[!Popup.MotionDurationProperty]        = this[!MotionDurationProperty];
        _popup[!Popup.OpenMotionProperty]            = this[!OpenMotionProperty];
        _popup[!Popup.CloseMotionProperty]           = this[!CloseMotionProperty];
        _popup[!Popup.IsMotionEnabledProperty]       = this[!IsMotionEnabledProperty];
        _popup[!Popup.ShouldUseOverlayLayerProperty] = this[!ShouldUseOverlayPopupProperty];
        _popup[!Popup.IsPopupPinnedOpenProperty]     = this[!IsPopupPinnedOpenProperty];

        this.SetPopup(_popup);
        return _popup;
    }

    private void HandlePopupClosing(object? sender, CancelEventArgs e)
    {
        if (InteractionHandler is DefaultMenuInteractionHandler interactionHandler)
        {
            interactionHandler.CancelPendingHoverOperations();
        }

        if (!e.Cancel)
        {
            this.OnPopupClosing(sender, e);
        }
        if (!e.Cancel)
        {
            for (var i = 0; i < ItemCount; i++)
            {
                if (ContainerFromIndex(i) is MenuItem menuItem)
                {
                    menuItem.Close();
                }
            }
        }
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;
        this.OnPopupClosed(sender, e);
    }

    private void HandlePopupKeyUp(object? sender, KeyEventArgs e)
    {
        var keymap = Application.Current?.PlatformSettings?.HotkeyConfiguration;
        if (IsOpen && keymap?.OpenContextMenu.Any(key => key.Matches(e)) == true)
        {
            Close();
            e.Handled = _popup?.IsPlayingCloseMotion == true || !IsOpen;
        }
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        this.OnPopupOpened(sender, e);
        if (_popup?.PlacementTarget is { } target)
        {
            var topLevel = TopLevel.GetTopLevel(target);
            _deactivationSubscription?.Dispose();
            _deactivationSubscription = TopLevelDeactivation.Subscribe(topLevel, HandleWindowDeactivated);
        }
    }

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        Close();
    }

    private static void HandleOwnerContextMenuChanged(Control owner, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.OldValue is ContextMenu oldContextMenu &&
            ReferenceEquals(oldContextMenu._pinnedOpenOwner, owner))
        {
            oldContextMenu.SetPinnedOpenOwner(null);
        }

        if (change.NewValue is ContextMenu newContextMenu)
        {
            newContextMenu.SetPinnedOpenOwner(owner);
        }
    }

    private void SetPinnedOpenOwner(Control? owner)
    {
        if (ReferenceEquals(_pinnedOpenOwner, owner))
        {
            return;
        }

        if (_pinnedOpenOwner != null)
        {
            _pinnedOpenOwner.AttachedToVisualTree -= HandlePinnedOpenOwnerAttached;
            _pinnedOpenOwner.DetachedFromVisualTree -= HandlePinnedOpenOwnerDetached;
            CloseForLifecycle();
        }

        _pinnedOpenOwner = owner;
        if (_pinnedOpenOwner != null)
        {
            _pinnedOpenOwner.AttachedToVisualTree += HandlePinnedOpenOwnerAttached;
            _pinnedOpenOwner.DetachedFromVisualTree += HandlePinnedOpenOwnerDetached;
            TryOpenPinned();
        }
    }

    private void HandlePinnedOpenOwnerAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        TryOpenPinned();
    }

    private void HandlePinnedOpenOwnerDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        CloseForLifecycle();
    }

    private void TryOpenPinned()
    {
        if (!IsPopupPinnedOpen ||
            IsOpen ||
            _pinnedOpenOwner == null ||
            !_pinnedOpenOwner.IsAttachedToVisualTree())
        {
            return;
        }

        Open(_pinnedOpenOwner);
    }

    private void CloseForLifecycle()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            if (ContainerFromIndex(i) is MenuItem menuItem)
            {
                menuItem.CloseForLifecycle();
            }
        }

        _popup?.CloseForLifecycle();
        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;
    }

    private void ConfigureMaxPopupHeight()
    {
        var maxPopupHeight = IsScrollEnabled
            ? ItemHeight * DisplayPageSize + Padding.Top + Padding.Bottom
            : double.PositiveInfinity;
        SetCurrentValue(MaxPopupHeightProperty, maxPopupHeight);
    }

    private void ConfigurePlacement()
    {
        try
        {
            if (_ignorePlacementChanged)
            {
                return;
            }

            _ignorePlacementChanged = true;
            if (_popup != null)
            {
                if (Placement != PlacementMode.Custom)
                {
                    _popup.RequestedPlacement = Placement;
                    Placement                 = PlacementMode.Custom;
                }
            }
        }
        finally
        {
            _ignorePlacementChanged = false;
        }
    }

    #endregion
}
