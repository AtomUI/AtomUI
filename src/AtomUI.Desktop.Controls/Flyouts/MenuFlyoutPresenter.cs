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

public class MenuFlyoutPresenter : MenuBase,
                                   IArrowAwareShadowMaskInfoProvider,
                                   IMotionAwareControl,
                                   ICustomizableSizeTypeAware,
                                   IScrollAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<MenuFlyoutPresenter>();

    public static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<MenuFlyoutPresenter>();

    public static readonly StyledProperty<int> DisplayPageSizeProperty =
        Menu.DisplayPageSizeProperty.AddOwner<MenuFlyoutPresenter>();

    public static readonly RoutedEvent<FlyoutMenuItemClickedEventArgs> MenuItemClickedEvent =
        RoutedEvent.Register<MenuFlyoutPresenter, FlyoutMenuItemClickedEventArgs>(
            nameof(MenuItemClicked),
            RoutingStrategies.Bubble);

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<MenuFlyoutPresenter>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<MenuFlyoutPresenter>();

    public static readonly StyledProperty<bool> IsScrollEnabledProperty =
        ScrollAwareControlProperty.IsScrollEnabledProperty.AddOwner<MenuFlyoutPresenter>();

    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        Menu.ShouldUseOverlayPopupProperty.AddOwner<MenuFlyoutPresenter>();

    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    public ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }

    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
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

    public event EventHandler<FlyoutMenuItemClickedEventArgs>? MenuItemClicked
    {
        add => AddHandler(MenuItemClickedEvent, value);
        remove => RemoveHandler(MenuItemClickedEvent, value);
    }

    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

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

    public MenuFlyoutPresenter(IMenuInteractionHandler menuInteractionHandler)
        : base(menuInteractionHandler)
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

        // DefaultMenuInteractionHandler calls this
        var host = this.FindLogicalAncestorOfType<Popup>();
        if (host != null)
        {
            SelectedIndex = -1;
            host.IsOpen   = false;
        }
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

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        ReconcilePinnedOpenChildren();
    }

    protected override void OnSubmenuOpened(RoutedEventArgs e)
    {
        if (e.Source is MenuItem item)
        {
            _pinnedOpenScope?.SubmenuOpened(item);
        }
        base.OnSubmenuOpened(e);
    }

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

    public override void Open()
    {
        throw new NotSupportedException("Use MenuFlyout.ShowAt(Control) instead");
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _arrowDecoratedBox =
            e.NameScope.Find<ArrowDecoratedBox>(AbstractArrowDecoratedBox.ArrowDecoratorPart);
        // popup.root 语义部件指向弹层根视觉面（ArrowDecoratedBox），而非共享的
        // MenuFlyoutPresenter 容器：边框 / 背景 / 圆角由 ArrowDecoratedBox 的
        // PART_ContentDecorator 渲染。标记类无法在共享 ArrowDecoratedBox 主题上静态声明
        // （会污染其他 ArrowDecoratedBox 弹层），因此按 DropdownButton 语义部件契约在
        // 模板应用时运行时注入。
        _arrowDecoratedBox?.Classes.Add(DropdownButtonSemanticParts.PopupRootClass);
        ConfigureMaxPopupHeight();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        // 钉住弹层的关闭来自放置目标生命周期失效（页签切走 / 滚出视口），属于可恢复的临时
        // 关闭而非用户收起菜单：此时保留声明式子菜单状态，菜单树重新附着后由 MenuItem 的
        // 延迟同步恢复子菜单呈现。清理只针对普通关闭，避免把声明式展开状态一并抹掉。
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

    public CornerRadius GetMaskCornerRadius()
    {
        if (_arrowDecoratedBox is not null)
        {
            return _arrowDecoratedBox.CornerRadius;
        }

        return new CornerRadius(0);
    }

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
        // 箭头部件不可用时按无箭头报告，弹层据此降级阴影与定位
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

public class FlyoutMenuItemClickedEventArgs : RoutedEventArgs
{
    public MenuItem Item { get; }

    public FlyoutMenuItemClickedEventArgs(RoutedEvent routedEvent, MenuItem menuItem)
        : base(routedEvent)
    {
        Item = menuItem;
    }
}
