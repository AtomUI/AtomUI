using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Windows.Input;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Exceptions;
using AtomUI.Input;
using AtomUI.MotionScene;
using Avalonia;
using Avalonia.Animation.Easings;
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
    
    public static readonly StyledProperty<TreeNodeKey?> ItemKeyProperty =
        AvaloniaProperty.Register<NavMenuItem, TreeNodeKey?>(nameof(ItemKey));
    
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
    
    public TreeNodeKey? ItemKey
    {
        get => GetValue(ItemKeyProperty);
        set => SetValue(ItemKeyProperty, value);
    }
    
    bool INavMenuItem.IsPointerOverSubMenu => _popup?.IsPointerOverPopup ?? false;
    INavMenuNode? INavMenuItem.Node => DataContext as INavMenuNode;
    
    INavMenuElement? INavMenuItem.Parent => Parent as INavMenuElement;
    IEnumerable<INavMenuItem> INavMenuElement.SubItems => LogicalChildren.OfType<INavMenuItem>();
    #endregion
    
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

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NavMenuItem>();
    
    internal static readonly StyledProperty<bool> ShouldUseOverlayLayerProperty = 
        AvaloniaProperty.Register<NavMenuItem, bool>(nameof (ShouldUseOverlayLayer));
    
    internal static readonly StyledProperty<bool> IsInSelectedPathProperty = 
        AvaloniaProperty.Register<NavMenuItem, bool>(nameof (IsInSelectedPath));

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

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
        
    internal bool ShouldUseOverlayLayer
    {
        get => GetValue(ShouldUseOverlayLayerProperty);
        set => SetValue(ShouldUseOverlayLayerProperty, value);
    }
    
    // 是否在选择路径中
    internal bool IsInSelectedPath
    {
        get => GetValue(IsInSelectedPathProperty);
        set => SetValue(IsInSelectedPathProperty, value);
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
    private BaseMotionActor? _childItemsLayoutTransform;

    private Control? _itemHeader;
    private bool _animating;
    
    internal Popup? Popup => _popup;
    internal NavMenu? OwnerMenu;

    private readonly Dictionary<NavMenuItem, CompositeDisposable> _itemsBindingDisposables = new();
    
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
    
    public NavMenuItem()
    {
        LogicalChildren.CollectionChanged += HandleItemsCollectionChanged;
    }
    
    protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;
    
    private void HandleItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
    
    public void Open()
    {
        if (Mode == NavMenuMode.Inline)
        {
            if (_animating)
            {
                return;
            }
        }

        IsSubMenuOpen = true;
    }
    
    public void Close()
    {
        if (Mode == NavMenuMode.Inline)
        {
            if (_animating)
            {
                return;
            }                                       
        }
        
        Dispatcher.UIThread.InvokeAsync(async () => await CloseItemAsync(this));
    }
    
    public async Task CloseItemAsync(INavMenuItem menuItem)
    {
        {
            foreach (var child in menuItem.SubItems)
            {
                if (child is NavMenuItem childNavMenuItem)
                {
                    await CloseItemAsync(childNavMenuItem);
                }
            }
        }
        if (menuItem is NavMenuItem navMenuItem)
        {
            if (navMenuItem._popup != null && navMenuItem._popup.IsMotionAwareOpen)
            {
                await navMenuItem._popup.MotionAwareCloseAsync();
            }

            navMenuItem.IsSubMenuOpen = false;
        }
    }

    private void ClearStateRecursively(INavMenuItem menuItem)
    {
        foreach (var child in menuItem.SubItems)
        {
            if (child is NavMenuItem childNavMenuItem)
            {
                ClearStateRecursively(childNavMenuItem);
            }
        }
        if (menuItem is NavMenuItem navMenuItem)
        {
            navMenuItem.IsSubMenuOpen    = false;
            navMenuItem.IsInSelectedPath = false;
        }
    }
    
    protected virtual void NotifySubmenuOpened(RoutedEventArgs e)
    {
        if (e.Source is NavMenuItem menuItem)
        {
            if (menuItem.Parent == this)
            {
                // TODO 我们在这里对模式做一个区分, Inline 暂时不互斥关闭，后面有时间看是否加一个互斥的标记
                if (Mode != NavMenuMode.Inline)
                {
                    foreach (var child in ((INavMenuItem)this).SubItems)
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

        if (!_isEmbeddedInMenu)
        {
            //Normally the Menu's IMenuInteractionHandler is sending the click events for us
            //However when the item is not embedded into a menu we need to send them ourselves.
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }
    }
    
    public void CanExecuteChanged(object? sender, EventArgs e) => TryUpdateCanExecute();
    
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
        if (!((ILogical)this).IsAttachedToLogicalTree || Parent is NavMenuItem { IsSubMenuOpen: false })
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
        
        if (change.Property == ParentProperty)
        {
            IsTopLevel = Parent is NavMenu;
            UpdatePseudoClasses();
        }
        else if (change.Property == IsSelectedProperty)
        {
            IsSelectedChanged(change);
        }
        else if (change.Property == IsSubMenuOpenProperty)
        {
            HandleSubMenuOpenChanged(change);
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
    }
    
    private static void HandleCommandChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newCommand = change.NewValue as ICommand;
        if (change.Sender is NavMenuItem menuItem)
        {
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
            (var command, var parameter) = (menuItem.Command, change.NewValue);
            menuItem.TryUpdateCanExecute(command, parameter);
        }
    }
    
    private void ValidateSelectionMode()
    {
        if (SelectionMode.HasFlag(SelectionMode.Multiple))
        {
            throw new InvalidPropertyValueException(SelectionModeProperty.Name, SelectionMode.Multiple,
                $"The value '{SelectionMode.Multiple}' is invalid for the '{SelectionModeProperty.Name}' property in NavMenu.");
        }
    }
    
    private void ConfigureEffectivePopupMinWidth()
    {
        if (IsTopLevel)
        {
            if (Parent is NavMenu navMenu)
            {
                if (navMenu.Mode == NavMenuMode.Horizontal)
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
        
        if (Mode == NavMenuMode.Inline)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                // 在这里我们有一个动画的效果
                if (value)
                {
                    await OpenInlineItemAsync();
                    RaiseEvent(new RoutedEventArgs(SubmenuOpenedEvent));
                }
                else
                {
                    await CloseInlineItemAsync();
                    RaiseEvent(new RoutedEventArgs(SubmenuClosedEvent));
                }
              
                foreach (var item in ItemsView.OfType<NavMenuItem>())
                {
                    item.TryUpdateCanExecute();
                }
            });
        }
        else
        {
            if (value)
            {
                foreach (var item in ItemsView.OfType<NavMenuItem>())
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
    
    private async Task OpenInlineItemAsync(bool forceDisableMotion = false)
    {
        if (HasSubMenu && _childItemsLayoutTransform is not null)
        {
            if (IsMotionEnabled && !forceDisableMotion)
            {
                if (_animating)
                {
                    return;
                }
        
                _animating                           = true;
                _childItemsLayoutTransform.IsVisible = true;
                var motion = new SlideUpInMotion(OpenCloseMotionDuration, new CubicEaseOut());
                await motion.RunAsync(_childItemsLayoutTransform,
                    () => { _childItemsLayoutTransform.IsVisible = true; });
                _animating                           = false;
            }
            else
            {
                _childItemsLayoutTransform.IsVisible = true;
            }
        }
    }

    internal async Task CloseInlineItemAsync(bool forceDisableMotion = false)
    {
        if (HasSubMenu && _childItemsLayoutTransform is not null)
        {
            if (IsMotionEnabled && !forceDisableMotion && _childItemsLayoutTransform.IsVisible)
            {
                if (_animating)
                {
                    return;
                }
        
                _animating                           = true;
                _childItemsLayoutTransform.IsVisible = true;
                var motion = new SlideUpOutMotion(OpenCloseMotionDuration, new CubicEaseIn());
                await motion.RunAsync(_childItemsLayoutTransform);
                _childItemsLayoutTransform.IsVisible = false;
                _animating                           = false;
            }
            else
            {
                _childItemsLayoutTransform.IsVisible = false;
            }
        }
    }
    
    private void CloseSubmenus()
    {
        foreach (var child in ((INavMenuItem)this).SubItems)
        {
            child.IsSubMenuOpen = false;
        }
    }
    
    private void PopupOpened(object? sender, EventArgs e)
    {
        // If we're using overlay popups, there's a chance we need to do a layout pass before
        // the child items are added to the visual tree. If we don't do this here, then
        // selection breaks.
        if (Presenter?.GetVisualRoot() != null)
        {
            UpdateLayout();
        }

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
            if (Parent is NavMenu navMenu)
            {
                if (navMenu.Mode == NavMenuMode.Horizontal && _itemHeader is not null)
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
        return new NavMenuItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<NavMenuItem>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is NavMenuItem menuItem)
        {
            menuItem.OwnerMenu = OwnerMenu;
            var disposables = new CompositeDisposable(4);

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
            
            disposables.Add(BindUtils.RelayBind(this, ModeProperty, menuItem, ModeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsDarkStyleProperty, menuItem, IsDarkStyleProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, menuItem, IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, ItemContainerThemeProperty, menuItem, ItemContainerThemeProperty));
            disposables.Add(BindUtils.RelayBind(this, ShouldUseOverlayLayerProperty, menuItem, ShouldUseOverlayLayerProperty));
            
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

    protected virtual void PrepareNavMenuItem(NavMenuItem menuItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    protected virtual void NotifyClicked(RoutedEventArgs e)
    {
        var (command, parameter) = (Command, CommandParameter);
        if (!e.Handled && command is not null && command.CanExecute(parameter) == true)
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
        Level = CalculateDistanceFromLogicalParent<NavMenu>(this) - 1;

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

        if (Command != null)
        {
            Command.CanExecuteChanged -= CanExecuteChangedHandler;
        }
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdatePseudoClasses();
        TryUpdateCanExecute();
    }
    
    private static int CalculateDistanceFromLogicalParent<T>(ILogical? logical, int defaultDistance = -1) where T : class
    {
        var result = 0;

        while (logical != null && !(logical is T))
        {
            ++result;
            logical = logical.LogicalParent;
        }

        return logical != null ? result : defaultDistance;
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
        ClearStateRecursively(this);
        base.OnApplyTemplate(e);
        if (_popup != null)
        {
            _popup.Opened             -= PopupOpened;
            _popup.Closed             -= PopupClosed;
            _popup.DependencyResolver =  null;
        }

        _popup = e.NameScope.Find<Popup>(NavMenuItemThemeConstants.PopupPart);
        
        if (_popup != null)
        {
            _popup.Opened += PopupOpened;
            _popup.Closed += PopupClosed;
        }

        _itemHeader = e.NameScope.Find<Control>(NavMenuItemThemeConstants.HeaderPart);
        
        if (Mode == NavMenuMode.Inline)
        {
            _childItemsLayoutTransform =
                e.NameScope.Find<BaseMotionActor>(InlineNavMenuItemThemeConstants.ChildItemsLayoutTransformPart);
            if (_childItemsLayoutTransform is not null)
            {
                _childItemsLayoutTransform.SetCurrentValue(IsVisibleProperty, IsSubMenuOpen && HasSubMenu);
            }
        }
    }

    void IMenuChildSelectable.SelectChildItem(NavMenuItem child, bool isSelected)
    {
        UpdateSelection(child, isSelected);
    }
    
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        RaiseEvent(new RoutedEventArgs(PointerEnteredItemEvent));
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        RaiseEvent(new RoutedEventArgs(PointerExitedItemEvent));
    }
    
    internal void RegenerateContainers()
    {
        RefreshContainers();
    }
}