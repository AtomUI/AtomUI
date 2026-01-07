using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

using AvaloniaContextMenu = Avalonia.Controls.ContextMenu;

public class ContextMenu : AvaloniaContextMenu,
                           ISizeTypeAware,
                           IMotionAwareControl,
                           IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<ContextMenu>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ContextMenu>();
    
    public static readonly StyledProperty<int> DisplayPageSizeProperty = 
        Menu.DisplayPageSizeProperty.AddOwner<ContextMenu>();
    
    public static readonly StyledProperty<bool> ShouldUseOverlayLayerProperty = 
        Menu.ShouldUseOverlayLayerProperty.AddOwner<ContextMenu>();
    
    public static readonly StyledProperty<BoxShadows> MaskShadowsProperty =
        Popup.MaskShadowsProperty.AddOwner<ContextMenu>();

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
    }
    
    public bool ShouldUseOverlayLayer
    {
        get => GetValue(ShouldUseOverlayLayerProperty);
        set => SetValue(ShouldUseOverlayLayerProperty, value);
    }
    
    public BoxShadows MaskShadows
    {
        get => GetValue(MaskShadowsProperty);
        set => SetValue(MaskShadowsProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<ContextMenu, double>(nameof(ItemHeight));
    
    internal static readonly StyledProperty<double> MaxPopupHeightProperty =
        AvaloniaProperty.Register<ContextMenu, double>(nameof(MaxPopupHeight));
    
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

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => MenuToken.ID;

    #endregion
    
    private Popup? _popup;
    private readonly Dictionary<MenuItem, CompositeDisposable> _itemsBindingDisposables = new();

    public ContextMenu()
    {
        this.RegisterResources();
        LogicalChildren.CollectionChanged  += HandleItemsCollectionChanged;
        // 我们在这里有一次初始化的机会
        _popup = new Popup
        {
            WindowManagerAddShadowHint     = false,
            IsLightDismissEnabled          = false,
            OverlayDismissEventPassThrough = true,
            IsDetectMouseClickEnabled      = true,
            IgnoreFirstDetected            = false
        };

        VerticalOffset            =  10;
        HorizontalOffset          =  10;
        _popup.Opened             += this.CreateEventHandler("PopupOpened");
        _popup.Closed             += this.CreateEventHandler<EventArgs>("PopupClosed");
        _popup.ClickHidePredicate =  MenuPopupClosePredicate;
        _popup.CloseAction        =  MenuPopupCloseAction;
        _popup.AddClosingEventHandler(this.CreateEventHandler<CancelEventArgs>("PopupClosing")!);
        _popup.KeyUp += this.CreateEventHandler<KeyEventArgs>("PopupKeyUp");

        BindUtils.RelayBind(this, ShouldUseOverlayLayerProperty, _popup, Popup.ShouldUseOverlayLayerProperty);
        BindUtils.RelayBind(this, MaskShadowsProperty, _popup, Popup.MaskShadowsProperty);
        
        if (_popup is IPopupHostProvider popupHostProvider)
        {
            popupHostProvider.PopupHostChanged += HandlePopupHostChanged;
        }
        Closing += (sender, args) =>
        {
            args.Cancel = true;
        };
        this.SetPopup(_popup);
        Opened += (sender, args) =>
        {
            _popup.SetIgnoreIsOpenChanged(true);
            _popup.IsMotionAwareOpen = true;
        };
    }
    
    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is MenuItem menuItem)
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
    
    private void HandlePopupHostChanged(IPopupHost? host)
    {
        if (host is PopupRoot popupRoot)
        {
            if (popupRoot.ParentTopLevel is WindowBase window)
            {
                window.Deactivated += (sender, args) =>
                {
                    Close();
                };
            }
        }
    }
    
    private bool MenuPopupClosePredicate(IPopupHostProvider hostProvider, RawPointerEventArgs args)
    {
        var popupRoots = new HashSet<PopupRoot>();
        foreach (var child in Items)
        {
            if (child is MenuItem childMenuItem)
            {
                popupRoots.UnionWith(MenuItem.CollectPopupRoots(childMenuItem));
            }
        }

        if (_popup?.Host is PopupRoot popupRoot)
        {
            popupRoots.Add(popupRoot);
        }
        return !popupRoots.Contains(args.Root);
    }

    private void MenuPopupCloseAction(Popup popup)
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
            var disposables = new CompositeDisposable(4);
            
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
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, menuItem, MenuItem.HeaderTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, menuItem, MenuItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, menuItem, MenuItem.ItemTemplateProperty));
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, menuItem, MenuItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, DisplayPageSizeProperty, menuItem, MenuItem.DisplayPageSizeProperty));
            disposables.Add(BindUtils.RelayBind(this, ShouldUseOverlayLayerProperty, menuItem, MenuItem.ShouldUseOverlayLayerProperty));
            
            PrepareMenuItem(menuItem, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(menuItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(menuItem);
            }
            _itemsBindingDisposables.Add(menuItem, disposables);
        }
        else if (container is MenuSeparator menuSeparator)
        {
            menuSeparator.Orientation = Orientation.Vertical;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type MenuItem or MenuSeparator.");
        }
    }
    
    protected virtual void PrepareMenuItem(MenuItem menuItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }
    
    public override void Close()
    {
        _popup?.SetIgnoreIsOpenChanged(true);
        if (!IsOpen || _popup == null || !_popup.IsVisible)
        {
            return;
        }
        
        if (IsMotionEnabled)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                for (var i = 0; i < ItemCount; i++)
                {
                    var container = ContainerFromIndex(i);
                    if (container is MenuItem menuItem)
                    {
                        await menuItem.CloseItemAsync();
                    }
                }
                _popup.IsMotionAwareOpen = false;
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
            _popup.IsMotionAwareOpen = false;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DisplayPageSizeProperty ||
            change.Property == ItemHeightProperty)
        {
            ConfigureMaxPopupHeight();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureMaxPopupHeight();
    }

    private void ConfigureMaxPopupHeight()
    {
        SetCurrentValue(MaxPopupHeightProperty, ItemHeight * DisplayPageSize + Padding.Top + Padding.Bottom);
    }
}