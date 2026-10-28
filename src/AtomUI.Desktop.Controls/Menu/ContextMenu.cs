using System.ComponentModel;
using AtomUI.Controls;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaContextMenu = Avalonia.Controls.ContextMenu;

public class ContextMenu : AvaloniaContextMenu,
                           ICustomizableSizeTypeAware,
                           IMotionAwareControl,
                           IScrollAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<BoxShadows> PopupRootShadowProperty =
        Popup.PopupRootShadowProperty.AddOwner<ContextMenu>();
    
    public static readonly StyledProperty<BoxShadows> OverlayHostShadowProperty =
        Popup.OverlayHostShadowProperty.AddOwner<ContextMenu>();
    
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ContextMenu>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ContextMenu>();

    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        ScrollAwareControlProperty.IsScrollEnabledProperty.AddOwner<ContextMenu>();

    public static readonly StyledProperty<int> DisplayPageSizeProperty =
        Menu.DisplayPageSizeProperty.AddOwner<ContextMenu>();

    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        Menu.ShouldUseOverlayPopupProperty.AddOwner<ContextMenu>();
    
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<ContextMenu>();
    
    public static readonly StyledProperty<Motion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<ContextMenu>();
        
    public static readonly StyledProperty<Motion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<ContextMenu>();
    
    public BoxShadows PopupRootShadow
    {
        get => GetValue(PopupRootShadowProperty);
        set => SetValue(PopupRootShadowProperty, value);
    }
    
    public BoxShadows OverlayHostShadow
    {
        get => GetValue(OverlayHostShadowProperty);
        set => SetValue(OverlayHostShadowProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsScrollEnabled
    {
        get => GetValue(IsScrollEnabledProperty);
        set => SetValue(IsScrollEnabledProperty, value);
    }

    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
    }

    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }
    
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }
    
    public Motion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    public Motion? CloseMotion
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

    private Popup? _popup;
    private IDisposable? _deactivationSubscription;
    private bool _ignorePlacementChanged;
    private Control? _pinnedOpenOwner;

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
        _popup.Closed                += this.OnPopupClosed;
        _popup.AddClosingEventHandler(HandlePopupClosing);
        _popup.KeyUp += this.OnPopupClosing;
        
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

    protected override void OnInitialized()
    {
        base.OnInitialized();
        CustomPopupPlacementCallback =  _popup!.HandleCustomPlacement;
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

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (item is MenuSeparatorData)
        {
            return new MenuSeparator();
        }

        return new MenuItem();
    }

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

    protected virtual void PrepareMenuItem(MenuItem menuItem, object? item, int index)
    {
    }

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
        
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is MenuItem menuItem)
            {
                menuItem.Close();
            }
        }

        _popup!.IsOpen = false;

        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;
    }

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureMaxPopupHeight();
        ConfigurePlacement();
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
}
