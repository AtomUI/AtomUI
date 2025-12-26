using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaMenuItem = Avalonia.Controls.MenuItem;

[PseudoClasses(MenuItemPseudoClass.TopLevel)]
public class MenuItem : AvaloniaMenuItem, IMenuItemData
{
    #region 公共属性定义
    
    public new static readonly StyledProperty<PathIcon?> IconProperty = AvaloniaProperty.Register<MenuItem, PathIcon?>(nameof (Icon));

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<MenuItem>();
    
    public static readonly StyledProperty<int> DisplayPageSizeProperty = 
        Menu.DisplayPageSizeProperty.AddOwner<MenuItem>();

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public new PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
    }

    #endregion
    
    IList<IMenuItemData> ITreeNode<IMenuItemData>.Children => Items.OfType<IMenuItemData>().ToList();
    public ITreeNode<IMenuItemData>? ParentNode => Parent as ITreeNode<IMenuItemData>;
    public TreeNodeKey? ItemKey { get; set; }

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> IsCheckStateChangedEvent = 
        RoutedEvent.Register<MenuItem, RoutedEventArgs>(nameof(IsCheckStateChanged), RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? IsCheckStateChanged
    {
        add => AddHandler(IsCheckStateChangedEvent, value);
        remove => RemoveHandler(IsCheckStateChangedEvent, value);
    }
    
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<MenuItem>();
    
    internal static readonly StyledProperty<double> MaxPopupHeightProperty =
        AvaloniaProperty.Register<MenuItem, double>(nameof(MaxPopupHeight));
    
    internal static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<MenuItem, double>(nameof(ItemHeight));
    
    internal static readonly StyledProperty<Thickness> PopupPaddingProperty =
        AvaloniaProperty.Register<MenuItem, Thickness>(nameof(PopupPadding));
    
    internal static readonly StyledProperty<bool> IsUseOverlayLayerProperty = 
        AvaloniaProperty.Register<MenuItem, bool>(nameof (IsUseOverlayLayer));
    
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
    
    internal bool IsUseOverlayLayer
    {
        get => GetValue(IsUseOverlayLayerProperty);
        set => SetValue(IsUseOverlayLayerProperty, value);
    }
    
    #endregion

    internal PopupRoot? SubmenuPopupRoot => _popup?.Host as PopupRoot;

    private RadioButton? _radioButton;
    private CheckBox? _checkBox;
    private Popup? _popup;
    private readonly Dictionary<MenuItem, CompositeDisposable> _itemsBindingDisposables = new();
    
    static MenuItem()
    {
        AffectsRender<MenuItem>(BackgroundProperty);
        AffectsMeasure<MenuItem>(IconProperty);
    }

    public MenuItem()
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ParentProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == IconProperty)
        {
            // 不要删掉，因为父类添加了，删除这几行会导致 icon 到 icon presenter 失败
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
                 change.Property == ItemHeightProperty)
        {
            ConfigureMaxPopupHeight();
        }

        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(MenuItemPseudoClass.TopLevel, IsTopLevel);
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
                }
            }
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, menuItem, MenuItem.HeaderTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, menuItem, MenuItem.ItemTemplateProperty));
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, menuItem, MenuItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, menuItem, MenuItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, IsUseOverlayLayerProperty, menuItem, MenuItem.IsUseOverlayLayerProperty));
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
            menuSeparator.Orientation = Orientation.Horizontal;
        }
        else if (container is not MenuSeparator)
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type MenuItem or MenuSeparator.");
        }
    }
    
    protected virtual void PrepareMenuItem(MenuItem menuItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (_radioButton != null)
        {
            _radioButton.IsCheckedChanged -= HandleRadioButtonCheckedChanged;
        }

        if (_checkBox != null)
        {
            _checkBox.IsCheckedChanged -= HandleCheckBoxCheckedChanged;
        }
        
        _radioButton = e.NameScope.Find<RadioButton>(MenuItemThemeConstants.ToggleRadioPart);
        _checkBox    = e.NameScope.Find<CheckBox>(MenuItemThemeConstants.ToggleCheckboxPart);
        _popup       = e.NameScope.Find<Popup>(MenuItemThemeConstants.PopupPart);
        if (_popup != null)
        {
            _popup.ClickHidePredicate = MenuPopupClosePredicate;
            _popup.CloseAction        = MenuCloseAction;
        }
        if (_radioButton != null)
        {
            _radioButton.IsCheckedChanged += HandleRadioButtonCheckedChanged;
        }

        if (_checkBox != null)
        {
            _checkBox.IsCheckedChanged += HandleCheckBoxCheckedChanged;
        }
        
        UpdatePseudoClasses();
        ConfigureMaxPopupHeight();
    }

    private void HandleRadioButtonCheckedChanged(object? sender, RoutedEventArgs args)
    {
        if (_radioButton != null)
        {
            if (_radioButton.IsVisible)
            {
                IsChecked = _radioButton.IsChecked == true;
            }
        }
    }

    private void HandleCheckBoxCheckedChanged(object? sender, RoutedEventArgs args)
    {
        if (_checkBox != null)
        {
            if (_checkBox.IsVisible)
            {
                IsChecked = _checkBox.IsChecked == true;
            }
        }
    }

    private bool MenuPopupClosePredicate(IPopupHostProvider hostProvider, RawPointerEventArgs args)
    {
        var popupRoots = CollectPopupRoots(this);
        
        return !popupRoots.Contains(args.Root);
    }

    private void MenuCloseAction(Popup popup)
    {
        // 找最上层的 Menu 进行关闭
        var menu = this.FindAncestorOfType<Menu>();
        menu?.Close();
    }

    internal static HashSet<PopupRoot> CollectPopupRoots(MenuItem menuItem)
    {
        var popupRoots = new HashSet<PopupRoot>();
        if (menuItem.IsSubMenuOpen)
        {
            if (menuItem.SubmenuPopupRoot != null)
            {
                popupRoots.Add(menuItem.SubmenuPopupRoot);
            }
        }

        foreach (var child in menuItem.Items)
        {
            if (child is MenuItem childMenuItem)
            {
                var childPopupRoots = CollectPopupRoots(childMenuItem);
                popupRoots.UnionWith(childPopupRoots);
            }
        }
        return popupRoots;
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(ContentPresenter.BackgroundProperty),
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(ContentPresenter.ForegroundProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
    
    public async Task CloseItemAsync()
    {
        for (var i = 0; i < ItemCount; i++)
        {
            var container = ContainerFromIndex(i);
            if (container is MenuItem childMenuItem)
            {
                await childMenuItem.CloseItemAsync();
            }
        }

        if (_popup != null && _popup.IsMotionAwareOpen)
        {
            await _popup.MotionAwareCloseAsync();
        }

        IsSubMenuOpen = false;
    }

    private void ConfigureMaxPopupHeight()
    {
        SetCurrentValue(MaxPopupHeightProperty, ItemHeight * DisplayPageSize + PopupPadding.Top + PopupPadding.Bottom);
    }
}