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
using Point = Avalonia.Point;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(NavMenuItemPseudoClass.Separator, 
    NavMenuItemPseudoClass.Icon, 
    StdPseudoClass.Open,
    StdPseudoClass.Pressed, 
    StdPseudoClass.Selected, 
    NavMenuItemPseudoClass.TopLevel)]
public class NavMenuItem : HeaderedSelectingItemsControl,
                           INavMenuItem,
                           ISelectable,
                           ICommandSource,
                           IClickableControl,
                           ICustomHitTest,
                           INavMenuItemData
{
    #region 公共属性定义
    
    public static readonly StyledProperty<ICommand?> CommandProperty =
        Button.CommandProperty.AddOwner<NavMenuItem>(new(enableDataValidation: true));
    
    public static readonly StyledProperty<KeyGesture?> HotKeyProperty =
        HotKeyManager.HotKeyProperty.AddOwner<NavMenuItem>();
    
    public static readonly StyledProperty<object?> CommandParameterProperty =
        Button.CommandParameterProperty.AddOwner<NavMenuItem>();
    
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
    
    public KeyGesture? HotKey
    {
        get => GetValue(HotKeyProperty);
        set => SetValue(HotKeyProperty, value);
    }
    
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the input gesture that will be displayed in the menu item.
    /// </summary>
    /// <remarks>
    /// Setting this property does not cause the input gesture to be handled by the menu item,
    /// it simply displays the gesture text in the menu.
    /// </remarks>
    public KeyGesture? InputGesture
    {
        get => GetValue(InputGestureProperty);
        set => SetValue(InputGestureProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="NavMenuItem"/> is currently selected.
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the submenu of the <see cref="NavMenuItem"/> is
    /// open.
    /// </summary>
    public bool IsSubMenuOpen
    {
        get => GetValue(IsSubMenuOpenProperty);
        set => SetValue(IsSubMenuOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates the submenu that this <see cref="NavMenuItem"/> is
    /// within should not close when this item is clicked.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="NavMenuItem"/> has a submenu.
    /// </summary>
    public bool HasSubMenu
    {
        get => _hasSubMenu;
        set => SetAndRaise(HasSubMenuProperty, ref _hasSubMenu, value);
    }

    private int _level;

    /// <summary>
    /// Gets the level/indentation of the item.
    /// </summary>
    public int Level
    {
        get => _level;
        private set => SetAndRaise(LevelProperty, ref _level, value);
    }

    /// <summary>
    /// Gets a value that indicates whether the <see cref="NavMenuItem"/> is a top-level main menu item.
    /// </summary>
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
    
    INavMenuElement? INavMenuItem.Parent => Parent as INavMenuElement;
    
    IList<INavMenuItemData> ITreeNode<INavMenuItemData>.Children => Items.OfType<INavMenuItemData>().ToList();
    ITreeNode<INavMenuItemData>? ITreeNode<INavMenuItemData>.ParentNode => Parent as ITreeNode<INavMenuItemData>;

    protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;
    
    INavMenuItem? INavMenuItem.SelectedItem
    {
        get
        {
            var index = SelectedIndex;
            return index != -1 ? (INavMenuItem?)ContainerFromIndex(index) : null;
        }

        set => SelectedIndex = value is Control c ? IndexFromContainer(c) : -1;
    }
    
    IEnumerable<INavMenuItem> INavMenuElement.SubItems => LogicalChildren.OfType<INavMenuItem>();

    private INavMenuInteractionHandler? MenuInteractionHandler =>
        this.FindLogicalAncestorOfType<NavMenu>()?.InteractionHandler;

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<NavMenuItem, double> EffectivePopupMinWidthProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, double>(nameof(EffectivePopupMinWidth),
            o => o.EffectivePopupMinWidth,
            (o, v) => o.EffectivePopupMinWidth = v);

    internal static readonly DirectProperty<NavMenuItem, double> PopupMinWidthProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, double>(nameof(PopupMinWidth),
            o => o.PopupMinWidth,
            (o, v) => o.PopupMinWidth = v);

    internal static readonly DirectProperty<NavMenuItem, NavMenuMode> ModeProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, NavMenuMode>(nameof(Mode),
            o => o.Mode,
            (o, v) => o.Mode = v);

    internal static readonly DirectProperty<NavMenuItem, TimeSpan> OpenCloseMotionDurationProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, TimeSpan>(nameof(OpenCloseMotionDuration),
            o => o.OpenCloseMotionDuration,
            (o, v) => o.OpenCloseMotionDuration = v);

    internal static readonly DirectProperty<NavMenuItem, bool> HasSubMenuProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, bool>(nameof(HasSubMenu),
            o => o.HasSubMenu,
            (o, v) => o.HasSubMenu = v);

    internal static readonly DirectProperty<NavMenuItem, bool> IsDarkStyleProperty =
        AvaloniaProperty.RegisterDirect<NavMenuItem, bool>(nameof(IsDarkStyle),
            o => o.IsDarkStyle,
            (o, v) => o.IsDarkStyle = v);

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NavMenuItem>();
    
    internal static readonly StyledProperty<bool> ShouldUseOverlayLayerProperty = 
        AvaloniaProperty.Register<NavMenuItem, bool>(nameof (ShouldUseOverlayLayer));

    private double _effectivePopupMinWidth;

    internal double EffectivePopupMinWidth
    {
        get => _effectivePopupMinWidth;
        set => SetAndRaise(EffectivePopupMinWidthProperty, ref _effectivePopupMinWidth, value);
    }

    private double _popupMinWidth;

    internal double PopupMinWidth
    {
        get => _popupMinWidth;
        set => SetAndRaise(PopupMinWidthProperty, ref _popupMinWidth, value);
    }

    private TimeSpan _openCloseMotionDuration;

    internal TimeSpan OpenCloseMotionDuration
    {
        get => _openCloseMotionDuration;
        set => SetAndRaise(OpenCloseMotionDurationProperty, ref _openCloseMotionDuration, value);
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

    private Control? _header;
    
    private bool _animating;
    
    internal Popup? Popup => _popup;

    private readonly Dictionary<NavMenuItem, CompositeDisposable> _itemsBindingDisposables = new();
    
    static NavMenuItem()
    {
        SelectableMixin.Attach<NavMenuItem>(IsSelectedProperty);
        PressedMixin.Attach<NavMenuItem>();
        FocusableProperty.OverrideDefaultValue<NavMenuItem>(true);
        ItemsPanelProperty.OverrideDefaultValue<NavMenuItem>(DefaultPanel);
        ClickEvent.AddClassHandler<NavMenuItem>((x, e) => x.OnClick(e));
        SubmenuOpenedEvent.AddClassHandler<NavMenuItem>((x, e) => x.OnSubmenuOpened(e));
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<NavMenuItem>(false);
    }

    public NavMenuItem()
    {
        LogicalChildren.CollectionChanged  += HandleItemsCollectionChanged;
    }
    
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

    /// <summary>
    /// Opens the submenu.
    /// </summary>
    /// <remarks>
    /// This has the same effect as setting <see cref="IsSubMenuOpen"/> to true.
    /// </remarks>
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

    /// <summary>
    /// Closes the submenu.
    /// </summary>
    /// <remarks>
    /// This has the same effect as setting <see cref="IsSubMenuOpen"/> to false.
    /// </remarks>
    public void Close()
    {
        if (Mode == NavMenuMode.Inline)
        {
            if (_animating)
            {
                return;
            }                                       
        }
        
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await CloseItemAsync(this);
        });
    }

    public async Task CloseItemAsync(INavMenuItem navMenuItem)
    {

        foreach (var child in navMenuItem.SubItems)
        {
            if (child is NavMenuItem childNavMenuItem)
            {
                await CloseItemAsync(childNavMenuItem);
            }
        }

        if (navMenuItem is NavMenuItem navMenuItem2)
        {
            if (navMenuItem2._popup != null && navMenuItem2._popup.IsMotionAwareOpen)
            {
                await navMenuItem2._popup.MotionAwareCloseAsync();
            }

            navMenuItem2.IsSubMenuOpen = false;
        }
    }

    /// <inheritdoc/>
    void INavMenuItem.RaiseClick() => RaiseEvent(new RoutedEventArgs(ClickEvent));

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new NavMenuItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        if (item is NavMenuItem or Separator)
        {
            recycleKey = null;
            return false;
        }

        recycleKey = DefaultRecycleKey;
        return true;
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

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (_hotkey != null) // Control attached again, set Hotkey to create a hotkey manager for this control
        {
            SetCurrentValue(HotKeyProperty, _hotkey);
        }

        base.OnAttachedToLogicalTree(e);

        Level = CalculateDistanceFromLogicalParent<NavMenu>(this) - 1;

        (var command, var parameter) = (Command, CommandParameter);
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

    /// <summary>
    /// Called when the <see cref="NavMenuItem"/> is clicked.
    /// </summary>
    /// <param name="e">The click event args.</param>
    protected virtual void OnClick(RoutedEventArgs e)
    {
        (var command, var parameter) = (Command, CommandParameter);
        if (!e.Handled && command is not null && command.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // Don't handle here: let event bubble up to menu.
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

    /// <summary>
    /// Called when a submenu is opened on this NavMenuItem or a child NavMenuItem.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected virtual void OnSubmenuOpened(RoutedEventArgs e)
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_popup != null)
        {
            _popup.Opened             -= PopupOpened;
            _popup.Closed             -= PopupClosed;
            _popup.DependencyResolver =  null;
        }

        _popup       = e.NameScope.Find<Popup>(NavMenuItemThemeConstants.PopupPart);
        if (_popup != null)
        {
            _popup.Opened += PopupOpened;
            _popup.Closed += PopupClosed;
        }

        _header = e.NameScope.Find<Control>(NavMenuItemThemeConstants.HeaderPart);
        
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

    /// <summary>
    /// Closes all submenus of the menu item.
    /// </summary>
    private void CloseSubmenus()
    {
        foreach (var child in ((INavMenuItem)this).SubItems)
        {
            child.IsSubMenuOpen = false;
        }
    }

    /// <summary>
    /// Called when the <see cref="Command"/> property changes.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void CommandChanged(AvaloniaPropertyChangedEventArgs change)
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

    /// <summary>
    /// Called when the <see cref="CommandParameter"/> property changes.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void CommandParameterChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Sender is NavMenuItem menuItem)
        {
            (var command, var parameter) = (menuItem.Command, change.NewValue);
            menuItem.TryUpdateCanExecute(command, parameter);
        }
    }

    /// <summary>
    /// Called when the <see cref="ICommand.CanExecuteChanged"/> event fires.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
    public void CanExecuteChanged(object? sender, EventArgs e)
    {
        TryUpdateCanExecute();
    }

    /// <summary>
    /// Tries to evaluate CanExecute value of a Command if menu is opened
    /// </summary>
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

        if (change.Property == HeaderProperty)
        {
            HeaderChanged(change);
        }
        else if (change.Property == ParentProperty)
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
            SubMenuOpenChanged(change);
        }
        else if (change.Property == CommandProperty)
        {
            CommandChanged(change);
        }
        else if (change.Property == CommandParameterProperty)
        {
            CommandParameterChanged(change);
        }
        else if (change.Property == ItemCountProperty)
        {
            HasSubMenu = ItemCount > 0;
        }
        else if (change.Property == BoundsProperty ||
                 change.Property == PopupMinWidthProperty)
        {
            SetupEffectivePopupMinWidth();
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
    
    private void ValidateSelectionMode()
    {
        if (SelectionMode.HasFlag(SelectionMode.Multiple))
        {
            throw new InvalidPropertyValueException(SelectionModeProperty.Name, SelectionMode.Multiple,
                $"The value '{SelectionMode.Multiple}' is invalid for the '{SelectionModeProperty.Name}' property in NavMenu.");
        }
    }

    private void SetupEffectivePopupMinWidth()
    {
        if (IsTopLevel)
        {
            if (Parent is NavMenu navMenu)
            {
                if (navMenu.Mode == NavMenuMode.Horizontal)
                {
                    EffectivePopupMinWidth = Math.Max(_header?.Bounds.Width ?? Bounds.Width, PopupMinWidth);
                }
                else
                {
                    EffectivePopupMinWidth = PopupMinWidth;
                }
            }
        }
    }

    /// <summary>
    /// Called when the <see cref="HeaderedSelectingItemsControl.Header"/> property changes.
    /// </summary>
    /// <param name="e">The property change event.</param>
    private void HeaderChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var (oldValue, newValue) = change.GetOldAndNewValue<object?>();
        if (Equals(newValue, "-"))
        {
            PseudoClasses.Add(NavMenuItemPseudoClass.Separator);
            Focusable = false;
        }
        else if (Equals(oldValue, "-"))
        {
            PseudoClasses.Remove(NavMenuItemPseudoClass.Separator);
            Focusable = true;
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(NavMenuItemPseudoClass.TopLevel, IsTopLevel);
    }

    /// <summary>
    /// Called when the <see cref="IsSelected"/> property changes.
    /// </summary>
    /// <param name="e">The property change event.</param>
    private void IsSelectedChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var parentMenu = Parent as NavMenu;
        var isSelected = change.GetNewValue<bool>();
        if (isSelected && (parentMenu is null || parentMenu.IsOpen))
        {
            Focus();
        }
    }

    /// <summary>
    /// Called when the <see cref="IsSubMenuOpen"/> property changes.
    /// </summary>
    /// <param name="e">The property change event.</param>
    private void SubMenuOpenChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var value = (bool)change.NewValue!;

        if (Mode == NavMenuMode.Inline)
        {
            // 在这里我们有一个动画的效果
            if (value)
            {
                foreach (var item in ItemsView.OfType<NavMenuItem>())
                {
                    item.TryUpdateCanExecute();
                }
                
                OpenInlineItem();
                RaiseEvent(new RoutedEventArgs(SubmenuOpenedEvent));
                PseudoClasses.Add(StdPseudoClass.Open);
            }
            else
            {
                CloseInlineItem();
                PseudoClasses.Remove(StdPseudoClass.Open);
            }
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
                PseudoClasses.Add(StdPseudoClass.Open);
            }
            else
            {
                CloseSubmenus();
                PseudoClasses.Remove(StdPseudoClass.Open);
            }
        }
    }

    private void OpenInlineItem(bool forceDisableMotion = false)
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
                var motion = new SlideUpInMotion(_openCloseMotionDuration, new CubicEaseOut());
                motion.Run(_childItemsLayoutTransform,
                    () => { _childItemsLayoutTransform.IsVisible = true; },
                    () =>
                    {
                        _animating                           = false;
                    });
            }
            else
            {
                _childItemsLayoutTransform.IsVisible = true;
            }
        }
    }

    internal void CloseInlineItem(bool forceDisableMotion = false)
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
                var motion = new SlideUpOutMotion(_openCloseMotionDuration, new CubicEaseIn());
                motion.Run(_childItemsLayoutTransform, null, () =>
                {
                    _childItemsLayoutTransform.IsVisible = false;
                    _animating                           = false;
                });
            }
            else
            {
                _childItemsLayoutTransform.IsVisible = false;
            }
        }
    }

    /// <summary>
    /// Called when the submenu's <see cref="Popup"/> is opened.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
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

    /// <summary>
    /// Called when the submenu's <see cref="Popup"/> is closed.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event args.</param>
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
                if (navMenu.Mode == NavMenuMode.Horizontal && _header is not null)
                {
                    var offset     = _header.TranslatePoint(new Point(0, 0), this) ?? default;
                    var targetRect = new Rect(offset, _header.Bounds.Size);
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

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is NavMenuItem navMenuItem)
        {
            var disposables = new CompositeDisposable(4);
            
            if (item != null && item is not Visual)
            {
                if (!navMenuItem.IsSet(HeaderProperty))
                {
                    navMenuItem.SetCurrentValue(HeaderProperty, item);
                }

                if (item is INavMenuItemData menuItemData)
                {
                    if (!navMenuItem.IsSet(IconProperty))
                    {
                        navMenuItem.SetCurrentValue(IconProperty, menuItemData.Icon);
                    }

                    if (navMenuItem.ItemKey == null)
                    {
                        navMenuItem.ItemKey = menuItemData.ItemKey;
                    }
                    if (!navMenuItem.IsSet(IsEnabledProperty))
                    {
                        navMenuItem.SetCurrentValue(IsEnabledProperty, menuItemData.IsEnabled);
                    }
                }
            }
            
            disposables.Add(BindUtils.RelayBind(this, ModeProperty, navMenuItem, ModeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsDarkStyleProperty, navMenuItem, IsDarkStyleProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, navMenuItem, IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, ItemContainerThemeProperty, navMenuItem, ItemContainerThemeProperty));
            disposables.Add(BindUtils.RelayBind(this, ShouldUseOverlayLayerProperty, navMenuItem, ShouldUseOverlayLayerProperty));
            PrepareNavMenuItem(navMenuItem, item, index, disposables);
            if (_itemsBindingDisposables.TryGetValue(navMenuItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(navMenuItem);
            }
            _itemsBindingDisposables.Add(navMenuItem, disposables);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type NavMenuItem.");
        }
    }

    protected virtual void PrepareNavMenuItem(NavMenuItem navMenuItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    internal void SelectItemRecursively()
    {
        IsSelected = true;
        if (Parent is NavMenuItem parent)
        {
            parent.SelectItemRecursively();
        }
    }

    internal void RegenerateContainers()
    {
        foreach (var item in Items)
        {
            if (item is NavMenuItem childNavMenuItem)
            {
                childNavMenuItem.RegenerateContainers();
            }
        }

        ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel());
        RefreshContainers();
    }

    private static int CalculateDistanceFromLogicalParent<T>(ILogical? logical, int @default = -1) where T : class
    {
        var result = 0;

        while (logical != null && !(logical is T))
        {
            ++result;
            logical = logical.LogicalParent;
        }

        return logical != null ? result : @default;
    }

    internal bool PointInNavMenuItemHeader(Point point)
    {
        if (_header is null)
        {
            return false;
        }
        
        var offset     = _header.TranslatePoint(new Point(0, 0), this) ?? default;
        var targetRect = new Rect(offset, _header.Bounds.Size);
        return targetRect.Contains(point);
    }
    
}