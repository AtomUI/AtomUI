using System.Reactive.Disposables;
using System.Windows.Input;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Exceptions;
using AtomUI.Input;
using AtomUI.Animations;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal sealed class NavMenuItemPointerEventArgs : RoutedEventArgs
{
    public NavMenuItemPointerEventArgs(RoutedEvent? routedEvent, PointerEventArgs pointerEventArgs)
        : base(routedEvent)
    {
        PointerTimestamp = pointerEventArgs.Timestamp;
    }

    public ulong PointerTimestamp { get; }
}

[PseudoClasses(
    NavMenuItemPseudoClass.Separator, 
    NavMenuItemPseudoClass.Icon, 
    StdPseudoClass.Open,
    StdPseudoClass.Pressed, 
    StdPseudoClass.Selected, 
    NavMenuItemPseudoClass.TopLevel)]
internal class NavMenuItem : HeaderedSelectingItemsControl,
                             INavMenuItem,
                             ISelectable,
                             ICommandSource,
                             IClickableControl,
                             ICustomHitTest,
                             IMenuChildSelectable
{
    #region 公共属性定义
    
    public static readonly StyledProperty<ICommand?> CommandProperty =
        Button.CommandProperty.AddOwner<NavMenuItem>(new(enableDataValidation: true));
    
    public static readonly StyledProperty<object?> CommandParameterProperty =
        Button.CommandParameterProperty.AddOwner<NavMenuItem>();
    
    public static readonly StyledProperty<KeyGesture?> HotKeyProperty =
        HotKeyManager.HotKeyProperty.AddOwner<NavMenuItem>();
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<NavMenuItem, PathIcon?>(nameof(Icon));
    
    public static readonly StyledProperty<KeyGesture?> InputGestureProperty =
        AvaloniaProperty.Register<NavMenuItem, KeyGesture?>(nameof(InputGesture));
    
    public static readonly StyledProperty<bool> IsSubMenuOpenProperty =
        AvaloniaProperty.Register<NavMenuItem, bool>(nameof(IsSubMenuOpen));
    
    public static readonly StyledProperty<bool> StaysOpenOnClickProperty =
        AvaloniaProperty.Register<NavMenuItem, bool>(nameof(StaysOpenOnClick));
    
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<NavMenuItem, bool>(nameof(IsChecked));
    
    public static readonly DirectProperty<NavMenuItem, bool> HasSubMenuProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, bool>(nameof(HasSubMenu),
            o => o.HasSubMenu,
            (o, v) => o.HasSubMenu = v);
    
    public static readonly DirectProperty<NavMenuItem, int> LevelProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, int>(
            nameof(Level), o => o.Level);
    
    public static readonly DirectProperty<NavMenuItem, bool> IsTopLevelProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, bool>(
            nameof(IsTopLevel), o => o.IsTopLevel);
    
    public static readonly StyledProperty<EntityKey?> ItemKeyProperty =
        AvaloniaProperty.Register<NavMenuItem, EntityKey?>(nameof(ItemKey));
    
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    
    public KeyGesture? HotKey
    {
        get => GetValue(HotKeyProperty);
        set => SetValue(HotKeyProperty, value);
    }
    
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public KeyGesture? InputGesture
    {
        get => GetValue(InputGestureProperty);
        set => SetValue(InputGestureProperty, value);
    }
    
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
    
    public bool IsSubMenuOpen
    {
        get => GetValue(IsSubMenuOpenProperty);
        set => SetValue(IsSubMenuOpenProperty, value);
    }
    
    public bool StaysOpenOnClick
    {
        get => GetValue(StaysOpenOnClickProperty);
        set => SetValue(StaysOpenOnClickProperty, value);
    }
    
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
    
    private bool _hasSubMenu;
    
    public bool HasSubMenu
    {
        get => _hasSubMenu;
        set => SetAndRaise(HasSubMenuProperty, ref _hasSubMenu, value);
    }
    
    private int _level;
    
    public int Level
    {
        get => _level;
        private set => SetAndRaise(LevelProperty, ref _level, value);
    }
    
    private bool _isTopLevel;
    public bool IsTopLevel
    {
        get => _isTopLevel;
        private set => SetAndRaise(IsTopLevelProperty, ref _isTopLevel, value);
    }
    
    public EntityKey? ItemKey
    {
        get => GetValue(ItemKeyProperty);
        set => SetValue(ItemKeyProperty, value);
    }
    
    bool INavMenuItem.IsPointerOverSubMenu => _popup?.IsPointerOverPopup ?? false;
    INavMenuNode? INavMenuItem.Node => DataContext as INavMenuNode;
    
    INavMenuElement? INavMenuItem.Parent => SemanticParentItem is not null
        ? SemanticParentItem
        : OwnerMenu;
    IEnumerable<INavMenuItem> INavMenuElement.SubItems => EnumerateSubItems();
    #endregion

    private IEnumerable<INavMenuItem> EnumerateSubItems()
    {
        foreach (var child in NavMenuSemanticNavigator.EnumerateDirectItems(this))
        {
            yield return child;
        }
    }
    
    #region 公共事件定义
    
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<NavMenuItem, RoutedEventArgs>(
            nameof(Click),
            RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> PointerEnteredItemEvent =
        RoutedEvent.Register<NavMenuItem, RoutedEventArgs>(
            nameof(PointerEnteredItem),
            RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> PointerExitedItemEvent =
        RoutedEvent.Register<NavMenuItem, RoutedEventArgs>(
            nameof(PointerExitedItem),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> SubmenuOpenedEvent =
        RoutedEvent.Register<NavMenuItem, RoutedEventArgs>(
            nameof(SubmenuOpened),
            RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> SubmenuClosedEvent =
        RoutedEvent.Register<NavMenuItem, RoutedEventArgs>(
            nameof(SubmenuClosed),
            RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }
    
    public event EventHandler<RoutedEventArgs>? PointerEnteredItem
    {
        add => AddHandler(PointerEnteredItemEvent, value);
        remove => RemoveHandler(PointerEnteredItemEvent, value);
    }
    
    public event EventHandler<RoutedEventArgs>? PointerExitedItem
    {
        add => AddHandler(PointerExitedItemEvent, value);
        remove => RemoveHandler(PointerExitedItemEvent, value);
    }
    
    public event EventHandler<RoutedEventArgs>? SubmenuOpened
    {
        add => AddHandler(SubmenuOpenedEvent, value);
        remove => RemoveHandler(SubmenuOpenedEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? SubmenuClosed
    {
        add => AddHandler(SubmenuClosedEvent, value);
        remove => RemoveHandler(SubmenuClosedEvent, value);
    }

    #endregion
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<NavMenuItem, double> EffectivePopupMinWidthProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, double>(nameof(EffectivePopupMinWidth),
            o => o.EffectivePopupMinWidth,
            (o, v) => o.EffectivePopupMinWidth = v);

    internal static readonly StyledProperty<double> PopupMinWidthProperty =
        AvaloniaProperty.Register<NavMenuItem, double>(nameof(PopupMinWidth));

    internal static readonly DirectProperty<NavMenuItem, NavMenuMode> ModeProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, NavMenuMode>(nameof(Mode),
            o => o.Mode,
            (o, v) => o.Mode = v);
    
    internal static readonly StyledProperty<TimeSpan> OpenCloseMotionDurationProperty =
        AvaloniaProperty.Register<NavMenuItem, TimeSpan>(nameof(OpenCloseMotionDuration));

    internal static readonly DirectProperty<NavMenuItem, bool> IsDarkStyleProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, bool>(nameof(IsDarkStyle),
            o => o.IsDarkStyle,
            (o, v) => o.IsDarkStyle = v);

    internal static readonly StyledProperty<bool> IsItemBackgroundEnabledProperty =
        AvaloniaProperty.Register<NavMenuItem, bool>(nameof(IsItemBackgroundEnabled), true);

    internal static readonly StyledProperty<double> EntryItemSpacingProperty =
        NavMenu.EntryItemSpacingProperty.AddOwner<NavMenuItem>();

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NavMenuItem>();
    
    internal static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty = 
        AvaloniaProperty.Register<NavMenuItem, bool>(nameof (ShouldUseOverlayPopup));
    
    internal static readonly StyledProperty<bool> IsInSelectedPathProperty = 
        AvaloniaProperty.Register<NavMenuItem, bool>(nameof (IsInSelectedPath));

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<NavMenuItem>();

    internal static readonly DirectProperty<NavMenuItem, bool> IsKeyboardActiveProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, bool>(
            nameof(IsKeyboardActive),
            o => o.IsKeyboardActive,
            (o, v) => o.IsKeyboardActive = v);

    internal static readonly DirectProperty<NavMenuItem, bool> IsPointerHoldProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, bool>(
            nameof(IsPointerHold),
            o => o.IsPointerHold,
            (o, v) => o.IsPointerHold = v);

    internal static readonly DirectProperty<NavMenuItem, bool> IsInlineCollapsedProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, bool>(
            nameof(IsInlineCollapsed),
            o => o.IsInlineCollapsed,
            (o, v) => o.IsInlineCollapsed = v);

    internal static readonly DirectProperty<NavMenuItem, object?> NodeHeaderProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, object?>(
            nameof(NodeHeader),
            o => o.NodeHeader,
            (o, v) => o.NodeHeader = v);

    internal static readonly DirectProperty<NavMenuItem, object?> TooltipProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, object?>(
            nameof(Tooltip),
            o => o.Tooltip,
            (o, v) => o.Tooltip = v);

    internal static readonly DirectProperty<NavMenuItem, bool> IsTooltipEnabledProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, bool>(
            nameof(IsTooltipEnabled),
            o => o.IsTooltipEnabled,
            (o, v) => o.IsTooltipEnabled = v,
            unsetValue: true);

    internal static readonly StyledProperty<bool> IsCollapsedTooltipEnabledProperty =
        NavMenu.IsCollapsedTooltipEnabledProperty.AddOwner<NavMenuItem>();

    internal static readonly StyledProperty<PlacementMode> CollapsedTooltipPlacementProperty =
        NavMenu.CollapsedTooltipPlacementProperty.AddOwner<NavMenuItem>();

    internal static readonly StyledProperty<int> CollapsedTooltipShowDelayProperty =
        NavMenu.CollapsedTooltipShowDelayProperty.AddOwner<NavMenuItem>();

    internal static readonly StyledProperty<int> CollapsedTooltipBetweenShowDelayProperty =
        NavMenu.CollapsedTooltipBetweenShowDelayProperty.AddOwner<NavMenuItem>();

    internal static readonly DirectProperty<NavMenuItem, object?> EffectiveCollapsedTooltipProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, object?>(
            nameof(EffectiveCollapsedTooltip),
            o => o.EffectiveCollapsedTooltip);

    private double _effectivePopupMinWidth;

    internal double EffectivePopupMinWidth
    {
        get => _effectivePopupMinWidth;
        set => SetAndRaise(EffectivePopupMinWidthProperty, ref _effectivePopupMinWidth, value);
    }

    internal double PopupMinWidth
    {
        get => GetValue(PopupMinWidthProperty);
        set => SetValue(PopupMinWidthProperty, value);
    }

    internal TimeSpan OpenCloseMotionDuration
    {
        get => GetValue(OpenCloseMotionDurationProperty);
        set => SetValue(OpenCloseMotionDurationProperty, value);
    }

    private NavMenuMode _mode;

    internal NavMenuMode Mode
    {
        get => _mode;
        set => SetAndRaise(ModeProperty, ref _mode, value);
    }

    private bool _isDarkStyle;

    internal bool IsDarkStyle
    {
        get => _isDarkStyle;
        set => SetAndRaise(IsDarkStyleProperty, ref _isDarkStyle, value);
    }

    internal bool IsItemBackgroundEnabled
    {
        get => GetValue(IsItemBackgroundEnabledProperty);
        set => SetValue(IsItemBackgroundEnabledProperty, value);
    }

    internal double EntryItemSpacing
    {
        get => GetValue(EntryItemSpacingProperty);
        set => SetValue(EntryItemSpacingProperty, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }
    
    // 是否在选择路径中
    internal bool IsInSelectedPath
    {
        get => GetValue(IsInSelectedPathProperty);
        set => SetValue(IsInSelectedPathProperty, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    private bool _isKeyboardActive;

    internal bool IsKeyboardActive
    {
        get => _isKeyboardActive;
        set => SetAndRaise(IsKeyboardActiveProperty, ref _isKeyboardActive, value);
    }

    private bool _isPointerHold;

    internal bool IsPointerHold
    {
        get => _isPointerHold;
        set => SetAndRaise(IsPointerHoldProperty, ref _isPointerHold, value);
    }

    private bool _isInlineCollapsed;

    internal bool IsInlineCollapsed
    {
        get => _isInlineCollapsed;
        set => SetAndRaise(IsInlineCollapsedProperty, ref _isInlineCollapsed, value);
    }

    private object? _nodeHeader;

    internal object? NodeHeader
    {
        get => _nodeHeader;
        set => SetAndRaise(NodeHeaderProperty, ref _nodeHeader, value);
    }

    private object? _tooltip;

    internal object? Tooltip
    {
        get => _tooltip;
        set => SetAndRaise(TooltipProperty, ref _tooltip, value);
    }

    private bool _isTooltipEnabled = true;

    internal bool IsTooltipEnabled
    {
        get => _isTooltipEnabled;
        set => SetAndRaise(IsTooltipEnabledProperty, ref _isTooltipEnabled, value);
    }

    internal bool IsCollapsedTooltipEnabled
    {
        get => GetValue(IsCollapsedTooltipEnabledProperty);
        set => SetValue(IsCollapsedTooltipEnabledProperty, value);
    }

    internal PlacementMode CollapsedTooltipPlacement
    {
        get => GetValue(CollapsedTooltipPlacementProperty);
        set => SetValue(CollapsedTooltipPlacementProperty, value);
    }

    internal int CollapsedTooltipShowDelay
    {
        get => GetValue(CollapsedTooltipShowDelayProperty);
        set => SetValue(CollapsedTooltipShowDelayProperty, value);
    }

    internal int CollapsedTooltipBetweenShowDelay
    {
        get => GetValue(CollapsedTooltipBetweenShowDelayProperty);
        set => SetValue(CollapsedTooltipBetweenShowDelayProperty, value);
    }

    private object? _effectiveCollapsedTooltip;

    internal object? EffectiveCollapsedTooltip
    {
        get => _effectiveCollapsedTooltip;
        private set => SetAndRaise(
            EffectiveCollapsedTooltipProperty,
            ref _effectiveCollapsedTooltip,
            value);
    }
    
    internal Control? ItemHeader => _itemHeader;
    
    #endregion
    
    #region 私有事件定义

    private EventHandler? _canExecuteChangeHandler = null;
    
    private EventHandler CanExecuteChangedHandler => _canExecuteChangeHandler ??= new(CanExecuteChanged);

    #endregion
    
    internal static PlatformKeyGestureConverter KeyGestureConverter = new();
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel());

    private bool _commandCanExecute = true;
    private bool _commandBindingError;
    private Popup? _popup;
    private KeyGesture? _hotkey;
    private bool _isEmbeddedInMenu;
    private ContentViewport? _childItemsViewport;
    private DispatcherOperation? _pendingCanExecuteUpdate;

    private Control? _itemHeader;
    private long _inlineRequestVersion;
    private CompositeDisposable? _nodeBindingDisposables;
    private IDisposable? _popupPinnedOpenBinding;
    private bool _isClosingForLifecycle;
    
    internal Popup? Popup => _popup;
    internal NavMenu? OwnerMenu { get; private set; }
    internal NavMenuItem? SemanticParentItem { get; private set; }
    internal ItemsControl? EntryOwner { get; private set; }

    internal CompositeDisposable ResetNodeBindingDisposables()
    {
        _nodeBindingDisposables?.Dispose();
        _nodeBindingDisposables = new CompositeDisposable();
        return _nodeBindingDisposables;
    }

    internal void ClearNodeBindingDisposables()
    {
        _nodeBindingDisposables?.Dispose();
        _nodeBindingDisposables = null;
    }

    internal void UpdateEntryContext(
        NavMenu? ownerMenu,
        NavMenuItem? semanticParentItem,
        int level,
        bool isTopLevel,
        ItemsControl entryOwner)
    {
        OwnerMenu          = ownerMenu;
        SemanticParentItem = semanticParentItem;
        EntryOwner         = entryOwner;
        Level              = level;
        IsTopLevel         = isTopLevel;
        UpdatePseudoClasses();
        ConfigureEffectivePopupMinWidth();
    }

    internal void ClearEntryContext()
    {
        ApplyInlineChildItemsStateImmediately();
        OwnerMenu          = null;
        SemanticParentItem = null;
        EntryOwner         = null;
        Level              = 0;
        IsTopLevel         = false;
        UpdatePseudoClasses();
    }
    
    static NavMenuItem()
    {
        SelectableMixin.Attach<NavMenuItem>(IsSelectedProperty);
        PressedMixin.Attach<NavMenuItem>();
        FocusableProperty.OverrideDefaultValue<NavMenuItem>(true);
        ItemsPanelProperty.OverrideDefaultValue<NavMenuItem>(DefaultPanel);
        ClickEvent.AddClassHandler<NavMenuItem>((x, e) => x.NotifyClicked(e));
        SubmenuOpenedEvent.AddClassHandler<NavMenuItem>((x, e) => x.NotifySubmenuOpened(e));
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<NavMenuItem>(false);
    }
    
    protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;
    
    public void Open()
    {
        IsSubMenuOpen = true;
    }
    
    public void Close()
    {
        if (IsPopupPinnedOpen)
        {
            return;
        }

        Dispatcher.InvokeAsync(async () => await CloseItemAsync(this));
    }
    
    public async Task CloseItemAsync(INavMenuItem menuItem)
    {
        foreach (var child in menuItem.SubItems)
        {
            await CloseItemAsync(child);
        }

        if (menuItem is NavMenuItem navMenuItem)
        {
            if (navMenuItem._popup != null && navMenuItem._popup.IsOpen)
            {
                navMenuItem._popup.IsOpen = false;
            }

            navMenuItem.IsSubMenuOpen = false;
        }
    }

    protected virtual void NotifySubmenuOpened(RoutedEventArgs e)
    {
        if (e.Source is NavMenuItem menuItem)
        {
            if (ReferenceEquals(menuItem.SemanticParentItem, this))
            {
                // TODO 我们在这里对模式做一个区分, Inline 暂时不互斥关闭，后面有时间看是否加一个互斥的标记
                if (Mode != NavMenuMode.Inline)
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
        }
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!_isEmbeddedInMenu &&
            e.InitialPressMouseButton == MouseButton.Left &&
            this.GetVisualsAt(e.GetPosition(this)).Any(visual =>
                ReferenceEquals(visual, this) || this.IsVisualAncestorOf(visual)))
        {
            //Normally the Menu's IMenuInteractionHandler is sending the click events for us
            //However when the item is not embedded into a menu we need to send them ourselves.
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }
    }
    
    public void CanExecuteChanged(object? sender, EventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Post(QueueCanExecuteUpdate, DispatcherPriority.Input);
            return;
        }

        QueueCanExecuteUpdate();
    }

    private void QueueCanExecuteUpdate()
    {
        if (!((ILogical)this).IsAttachedToLogicalTree || _pendingCanExecuteUpdate is not null)
        {
            return;
        }

        _pendingCanExecuteUpdate = Dispatcher.InvokeAsync(() =>
        {
            _pendingCanExecuteUpdate = null;
            TryUpdateCanExecute();
        }, DispatcherPriority.Input);
    }

    private void CancelPendingCanExecuteUpdate()
    {
        _pendingCanExecuteUpdate?.Abort();
        _pendingCanExecuteUpdate = null;
    }
    
    private void TryUpdateCanExecute()
    {
        TryUpdateCanExecute(Command, CommandParameter);
    }
    
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void TryUpdateCanExecute(ICommand? command, object? parameter)
    {
        if (command == null)
        {
            _commandCanExecute = !_commandBindingError;
            UpdateIsEffectivelyEnabled();
            return;
        }

        //Perf optimization - only raise CanExecute event if the menu is open
        if (!((ILogical)this).IsAttachedToLogicalTree || SemanticParentItem is { IsSubMenuOpen: false })
        {
            return;
        }

        var canExecute = command.CanExecute(parameter);
        if (canExecute != _commandCanExecute)
        {
            _commandCanExecute = canExecute;
            UpdateIsEffectivelyEnabled();
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == NodeHeaderProperty ||
            change.Property == TooltipProperty ||
            change.Property == IsTooltipEnabledProperty ||
            change.Property == IsCollapsedTooltipEnabledProperty ||
            change.Property == IsInlineCollapsedProperty ||
            change.Property == IsTopLevelProperty ||
            change.Property == HasSubMenuProperty)
        {
            UpdateEffectiveCollapsedTooltip();
        }

        if (change.Property == IsSelectedProperty)
        {
            IsSelectedChanged(change);
        }
        else if (change.Property == IsSubMenuOpenProperty)
        {
            if (!change.GetNewValue<bool>() && IsPopupPinnedOpen && !_isClosingForLifecycle)
            {
                SetCurrentValue(IsSubMenuOpenProperty, true);
            }
            else
            {
                HandleSubMenuOpenChanged(change);
            }
        }
        else if (change.Property == IsMotionEnabledProperty && !IsMotionEnabled &&
                 _childItemsViewport is not null)
        {
            ApplyInlineChildItemsStateImmediately();
        }
        else if (change.Property == CommandProperty)
        {
            HandleCommandChanged(change);
        }
        else if (change.Property == CommandParameterProperty)
        {
            HandleCommandParameterChanged(change);
        }
        else if (change.Property == ItemCountProperty)
        {
            HasSubMenu = ItemCount > 0;
        }
        else if (change.Property == BoundsProperty ||
                 change.Property == PopupMinWidthProperty)
        {
            ConfigureEffectivePopupMinWidth();
        }
        else if (change.Property == IconProperty)
        {
            if (change.OldValue is PathIcon)
            {
                PseudoClasses.Remove(NavMenuItemPseudoClass.Icon);
            }
        
            if (change.NewValue is PathIcon)
            {
                PseudoClasses.Add(NavMenuItemPseudoClass.Icon);
            }
        }
        else if (change.Property == SelectionModeProperty)
        {
            ValidateSelectionMode();
        }
        else if (change.Property == IsPopupPinnedOpenProperty && change.GetNewValue<bool>())
        {
            TryOpenPinned();
        }
    }

    private void TryOpenPinned()
    {
        if (!IsPopupPinnedOpen ||
            !HasSubMenu ||
            Mode == NavMenuMode.Inline ||
            !this.IsAttachedToVisualTree())
        {
            return;
        }

        SetCurrentValue(IsSubMenuOpenProperty, true);
    }

    internal void CloseForLifecycle()
    {
        _isClosingForLifecycle = true;
        try
        {
            foreach (var child in NavMenuSemanticNavigator.EnumerateDirectItems(this).OfType<NavMenuItem>())
            {
                child.CloseForLifecycle();
            }

            _popup?.CloseForLifecycle();
            SetCurrentValue(IsSubMenuOpenProperty, false);
        }
        finally
        {
            _isClosingForLifecycle = false;
        }
    }

    private void UpdateEffectiveCollapsedTooltip()
    {
        EffectiveCollapsedTooltip = IsCollapsedTooltipEnabled &&
                                    IsTooltipEnabled &&
                                    IsInlineCollapsed &&
                                    IsTopLevel &&
                                    !HasSubMenu
            ? Tooltip ?? NodeHeader
            : null;
    }
    
    private static void HandleCommandChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newCommand = change.NewValue as ICommand;
        if (change.Sender is NavMenuItem menuItem)
        {
            menuItem.CancelPendingCanExecuteUpdate();
            if (((ILogical)menuItem).IsAttachedToLogicalTree)
            {
                if (change.OldValue is ICommand oldCommand)
                {
                    oldCommand.CanExecuteChanged -= menuItem.CanExecuteChangedHandler;
                }

                if (newCommand is not null)
                {
                    newCommand.CanExecuteChanged += menuItem.CanExecuteChangedHandler;
                }
            }

            menuItem.TryUpdateCanExecute(newCommand, menuItem.CommandParameter);
        }
    }
    
    private static void HandleCommandParameterChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Sender is NavMenuItem menuItem)
        {
            menuItem.CancelPendingCanExecuteUpdate();
            (var command, var parameter) = (menuItem.Command, change.NewValue);
            menuItem.TryUpdateCanExecute(command, parameter);
        }
    }
    
    private void ValidateSelectionMode()
    {
        if ((SelectionMode & SelectionMode.Multiple) == SelectionMode.Multiple)
        {
            throw new InvalidPropertyValueException(SelectionModeProperty.Name, SelectionMode.Multiple,
                $"The value '{SelectionMode.Multiple}' is invalid for the '{SelectionModeProperty.Name}' property in NavMenu.");
        }
    }
    
    private void ConfigureEffectivePopupMinWidth()
    {
        if (IsTopLevel)
        {
            if (OwnerMenu is { } navMenu)
            {
                if (navMenu.EffectiveMode == NavMenuMode.Horizontal)
                {
                    EffectivePopupMinWidth = Math.Max(_itemHeader?.Bounds.Width ?? Bounds.Width, PopupMinWidth);
                }
                else
                {
                    EffectivePopupMinWidth = PopupMinWidth;
                }
            }
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(NavMenuItemPseudoClass.TopLevel, IsTopLevel);
        PseudoClasses.Set(StdPseudoClass.Open, IsSubMenuOpen);
    }
    
    private void IsSelectedChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var isSelected = change.GetNewValue<bool>();
        if (isSelected)
        {
            Focus();
        }
    }
    
    private void HandleSubMenuOpenChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var value = (bool)change.NewValue!;
        var requestVersion = ++_inlineRequestVersion;
        var viewport = _childItemsViewport;
        
        if (Mode == NavMenuMode.Inline)
        {
            if (value)
            {
                OwnerMenu?.CloseOtherTopLevelSubmenus(this);
            }

            Dispatcher.InvokeAsync(async () =>
            {
                if (!IsCurrentInlineRequest(requestVersion, viewport, value) ||
                    !await WaitForInlineChildItemsAsync() ||
                    !IsCurrentInlineRequest(requestVersion, viewport, value))
                {
                    return;
                }

                RaiseEvent(new RoutedEventArgs(value ? SubmenuOpenedEvent : SubmenuClosedEvent));
              
                foreach (var item in NavMenuSemanticNavigator.EnumerateDirectItems(this))
                {
                    item.TryUpdateCanExecute();
                }
            });
        }
        else
        {
            if (value)
            {
                foreach (var item in NavMenuSemanticNavigator.EnumerateDirectItems(this))
                {
                    item.TryUpdateCanExecute();
                }
                RaiseEvent(new RoutedEventArgs(SubmenuOpenedEvent));
            }
            else
            {
                CloseSubmenus();
            }
        }
    }
    
    private async Task<bool> WaitForInlineChildItemsAsync()
    {
        if (!HasSubMenu || _childItemsViewport is not { } viewport)
        {
            return true;
        }

        return (await Motion.GetVisibilityTask(viewport)).IsCompleted;
    }

    private void ApplyInlineChildItemsStateImmediately()
    {
        ++_inlineRequestVersion;
        if (_childItemsViewport is { } viewport)
        {
            Motion.SnapVisibility(viewport);
        }
    }

    private bool IsCurrentInlineRequest(long version, ContentViewport? viewport, bool isOpen)
    {
        return version == _inlineRequestVersion && ReferenceEquals(viewport, _childItemsViewport) &&
               Mode == NavMenuMode.Inline && isOpen == IsSubMenuOpen && this.IsAttachedToVisualTree();
    }

    private void CloseSubmenus()
    {
        foreach (var child in NavMenuSemanticNavigator.EnumerateDirectItems(this))
        {
            child.IsSubMenuOpen = false;
        }
    }
    
    private void PopupOpened(object? sender, EventArgs e)
    {
        // If we're using overlay popups, there's a chance we need to do a layout pass before
        // the child items are added to the visual tree. If we don't do this here, then
        // selection breaks.
        if (Presenter is { } presenter &&
            presenter.GetVisualRoot() == null)
        {
            UpdateLayout();
        }

        Presenter?.ApplyTemplate();

        var selected = SelectedIndex;

        if (selected != -1)
        {
            var container = ContainerFromIndex(selected);
            container?.Focus();
        }
    }
    
    private void PopupClosed(object? sender, EventArgs e)
    {
    }
    
    void ICommandSource.CanExecuteChanged(object sender, EventArgs e) => CanExecuteChangedHandler(sender, e);
    
    void IClickableControl.RaiseClick()
    {
        if (IsEffectivelyEnabled)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }
    }
    
    public bool HitTest(Point point)
    {
        if (IsTopLevel)
        {
            if (OwnerMenu is { } navMenu)
            {
                if (navMenu.EffectiveMode == NavMenuMode.Horizontal && _itemHeader is not null)
                {
                    var offset     = _itemHeader.TranslatePoint(new Point(0, 0), this) ?? default;
                    var targetRect = new Rect(offset, _itemHeader.Bounds.Size);
                    if (targetRect.Contains(point))
                    {
                        return true;
                    }

                    return false;
                }
            }
        }
        return true;
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return NavMenuEntryContainerCoordinator.CreateContainer(item, index, recycleKey);
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NavMenuEntryContainerCoordinator.NeedsContainer(this, item, index, out recycleKey);
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

    internal void PrepareGeneratedNavMenuItem(NavMenuItem menuItem, object? item, int index)
    {
        PrepareNavMenuItem(menuItem, item, index);
    }

    protected virtual void PrepareNavMenuItem(NavMenuItem menuItem, object? item, int index)
    {
    }

    protected virtual void NotifyClicked(RoutedEventArgs e)
    {
        var (command, parameter) = (Command, CommandParameter);
        if (ReferenceEquals(e.Source, this) && !e.Handled && command is not null && command.CanExecute(parameter))
        {
            command.Execute(parameter);
            e.Handled = true;
        }
    }
    
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (_hotkey != null) // Control attached again, set Hotkey to create a hotkey manager for this control
        {
            SetCurrentValue(HotKeyProperty, _hotkey);
        }

        base.OnAttachedToLogicalTree(e);
        var (command, parameter) = (Command, CommandParameter);
        if (command is not null)
        {
            command.CanExecuteChanged += CanExecuteChangedHandler;
        }
        
        TryUpdateCanExecute(command, parameter);

        var parent = Parent;

        while (parent is NavMenuItem)
        {
            parent = parent.Parent;
        }

        _isEmbeddedInMenu = parent?.FindLogicalAncestorOfType<INavMenu>(true) != null;
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        // This will cause the hotkey manager to dispose the observer and the reference to this control
        if (HotKey != null)
        {
            _hotkey = HotKey;
            SetCurrentValue(HotKeyProperty, null);
        }

        base.OnDetachedFromLogicalTree(e);
        CancelPendingCanExecuteUpdate();

        if (Command != null)
        {
            Command.CanExecuteChanged -= CanExecuteChangedHandler;
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ApplyInlineChildItemsStateImmediately();
        UpdatePseudoClasses();
        TryUpdateCanExecute();
        TryOpenPinned();
        if (_popup != null)
        {
            _popup.Opened -= PopupOpened;
            _popup.Closed -= PopupClosed;
            _popup.Opened += PopupOpened;
            _popup.Closed += PopupClosed;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_childItemsViewport is not null)
        {
            ApplyInlineChildItemsStateImmediately();
        }

        if (IsPopupPinnedOpen)
        {
            CloseForLifecycle();
        }

        base.OnDetachedFromVisualTree(e);
        if (_popup != null)
        {
            if (IsSubMenuOpen)
            {
                SetCurrentValue(IsSubMenuOpenProperty, false);
            }
            _popup.Opened -= PopupOpened;
            _popup.Closed -= PopupClosed;
        }
    }
    
    protected override void UpdateDataValidation(
        AvaloniaProperty property,
        BindingValueType state,
        Exception? error)
    {
        base.UpdateDataValidation(property, state, error);
        if (property == CommandProperty)
        {
            _commandBindingError = state == BindingValueType.BindingError;
            if (_commandBindingError && _commandCanExecute)
            {
                _commandCanExecute = false;
                UpdateIsEffectivelyEnabled();
            }
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_childItemsViewport is not null)
        {
            ApplyInlineChildItemsStateImmediately();
        }
        base.OnApplyTemplate(e);
        if (_popup != null)
        {
            _popup.CloseForLifecycle();
            _popup.Opened             -= PopupOpened;
            _popup.Closed             -= PopupClosed;
            _popup.DependencyResolver =  null;
        }

        _popupPinnedOpenBinding?.Dispose();
        _popupPinnedOpenBinding = null;

        _popup = e.NameScope.Find<Popup>("PART_Popup");

        if (_popup != null)
        {
            _popupPinnedOpenBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                _popup,
                Popup.IsPopupPinnedOpenProperty);
            _popup.Opened += PopupOpened;
            _popup.Closed += PopupClosed;
        }

        _itemHeader = e.NameScope.Find<Control>("PART_Header");
        _childItemsViewport = null;

        if (Mode == NavMenuMode.Inline)
        {
            _childItemsViewport =
                e.NameScope.Find<ContentViewport>("PART_ChildItemsViewport");
            if (_childItemsViewport is not null)
            {
                Motion.SnapVisibility(_childItemsViewport);
            }
        }
    }

    void IMenuChildSelectable.SelectChildItem(NavMenuItem child, bool isSelected)
    {
        child.SetCurrentValue(IsSelectedProperty, isSelected);
    }
    
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        RaiseEvent(new NavMenuItemPointerEventArgs(PointerEnteredItemEvent, e));
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        RaiseEvent(new NavMenuItemPointerEventArgs(PointerExitedItemEvent, e));
    }
}
