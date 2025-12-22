using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class BaseTabControl : SelectingItemsControl,
                              IMotionAwareControl,
                              IControlSharedTokenResourcesHost
{
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel());

    #region 公共属性定义
    
    public static readonly StyledProperty<Dock> TabStripPlacementProperty =
        AvaloniaProperty.Register<BaseTabControl, Dock>(nameof(TabStripPlacement), defaultValue: Dock.Top);
    
    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<BaseTabControl>();
    
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<BaseTabControl>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<BaseTabControl>();
    
    public static readonly DirectProperty<BaseTabControl, object?> SelectedContentProperty =
        AvaloniaProperty.RegisterDirect<BaseTabControl, object?>(nameof(SelectedContent), o => o.SelectedContent);

    public static readonly DirectProperty<BaseTabControl, IDataTemplate?> SelectedContentTemplateProperty =
        AvaloniaProperty.RegisterDirect<BaseTabControl, IDataTemplate?>(nameof(SelectedContentTemplate), o => o.SelectedContentTemplate);
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<BaseTabControl>();

    public static readonly StyledProperty<bool> TabAlignmentCenterProperty =
        AvaloniaProperty.Register<BaseTabControl, bool>(nameof(TabAlignmentCenter));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<BaseTabControl>();
    
    public static readonly StyledProperty<double> HeaderStartEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabControl, double>(nameof (HeaderStartEdgePadding));
    
    public static readonly StyledProperty<double> HeaderEndEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabControl, double>(nameof (HeaderEndEdgePadding));
    
    public static readonly StyledProperty<Thickness> ContentPaddingProperty = 
        AvaloniaProperty.Register<BaseTabControl, Thickness>(nameof (ContentPadding));
    
    public static readonly StyledProperty<double> TabAndContentGutterProperty =
        AvaloniaProperty.Register<BaseTabControl, double>(nameof(TabAndContentGutter));
    
    public static readonly StyledProperty<object?> HeaderStartExtraContentProperty = 
        AvaloniaProperty.Register<BaseTabControl, object?>(nameof (HeaderStartExtraContent));
    
    public static readonly StyledProperty<IDataTemplate?> HeaderStartExtraContentTemplateProperty =
        AvaloniaProperty.Register<ContentControl, IDataTemplate?>(nameof(HeaderStartExtraContentTemplate));
    
    public static readonly StyledProperty<object?> HeaderEndExtraContentProperty = 
        AvaloniaProperty.Register<BaseTabControl, object?>(nameof (HeaderEndExtraContent));
    
    public static readonly StyledProperty<IDataTemplate?> HeaderEndExtraContentTemplateProperty =
        AvaloniaProperty.Register<ContentControl, IDataTemplate?>(nameof(HeaderEndExtraContentTemplate));
    
    public static readonly StyledProperty<bool> IsTabClosableProperty =
        AvaloniaProperty.Register<ContentControl, bool>(nameof(IsTabClosable));
    
    public static readonly StyledProperty<bool> IsTabAutoHideCloseButtonProperty =
        AvaloniaProperty.Register<ContentControl, bool>(nameof(IsTabAutoHideCloseButton));

    #region 公共事件定义
    
    public static readonly RoutedEvent<TabClosingEventArgs> ClosingEvent =
        RoutedEvent.Register<BaseTabControl, TabClosingEventArgs>(nameof(Closing), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<TabClosedEventArgs> ClosedEvent =
        RoutedEvent.Register<BaseTabControl, TabClosedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    public event EventHandler<TabClosingEventArgs>? Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    public event EventHandler<TabClosedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }
    
    #endregion

    public Dock TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
    }
    
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }
    
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }
    
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
    
    public object? SelectedContent
    {
        get => _selectedContent;
        internal set => SetAndRaise(SelectedContentProperty, ref _selectedContent, value);
    }
    
    public IDataTemplate? SelectedContentTemplate
    {
        get => _selectedContentTemplate;
        internal set => SetAndRaise(SelectedContentTemplateProperty, ref _selectedContentTemplate, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool TabAlignmentCenter
    {
        get => GetValue(TabAlignmentCenterProperty);
        set => SetValue(TabAlignmentCenterProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public double HeaderStartEdgePadding
    {
        get => GetValue(HeaderStartEdgePaddingProperty);
        set => SetValue(HeaderStartEdgePaddingProperty, value);
    }
    
    public double HeaderEndEdgePadding
    {
        get => GetValue(HeaderEndEdgePaddingProperty);
        set => SetValue(HeaderEndEdgePaddingProperty, value);
    }
    
    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }
    
    public double TabAndContentGutter
    {
        get => GetValue(TabAndContentGutterProperty);
        set => SetValue(TabAndContentGutterProperty, value);
    }
    
    [DependsOn(nameof(HeaderStartExtraContentTemplate))]
    public object? HeaderStartExtraContent
    {
        get => GetValue(HeaderStartExtraContentProperty);
        set => SetValue(HeaderStartExtraContentProperty, value);
    }
    
    public IDataTemplate? HeaderStartExtraContentTemplate
    {
        get => GetValue(HeaderStartExtraContentTemplateProperty);
        set => SetValue(HeaderStartExtraContentTemplateProperty, value);
    }
    
    [DependsOn(nameof(HeaderEndExtraContentTemplate))]
    public object? HeaderEndExtraContent
    {
        get => GetValue(HeaderEndExtraContentProperty);
        set => SetValue(HeaderEndExtraContentProperty, value);
    }
    
    public IDataTemplate? HeaderEndExtraContentTemplate
    {
        get => GetValue(HeaderEndExtraContentTemplateProperty);
        set => SetValue(HeaderEndExtraContentTemplateProperty, value);
    }
    
    public bool IsTabClosable
    {
        get => GetValue(IsTabClosableProperty);
        set => SetValue(IsTabClosableProperty, value);
    }
    
    public bool IsTabAutoHideCloseButton
    {
        get => GetValue(IsTabAutoHideCloseButtonProperty);
        set => SetValue(IsTabAutoHideCloseButtonProperty, value);
    }
    #endregion

    #region 内部属性实现
    
    internal static readonly DirectProperty<BaseTabControl, Thickness> TabStripMarginProperty =
        AvaloniaProperty.RegisterDirect<BaseTabControl, Thickness>(nameof(TabStripMargin),
            o => o.TabStripMargin,
            (o, v) => o.TabStripMargin = v);
    
    internal static readonly DirectProperty<BaseTabControl, Thickness> EffectiveHeaderPaddingProperty =
        AvaloniaProperty.RegisterDirect<BaseTabControl, Thickness>(nameof(EffectiveHeaderPadding),
            o => o.EffectiveHeaderPadding,
            (o, v) => o.EffectiveHeaderPadding = v);

    private Thickness _tabStripMargin;

    internal Thickness TabStripMargin
    {
        get => _tabStripMargin;
        set => SetAndRaise(TabStripMarginProperty, ref _tabStripMargin, value);
    }
    
    private Thickness _effectiveHeaderPadding;

    internal Thickness EffectiveHeaderPadding
    {
        get => _effectiveHeaderPadding;
        set => SetAndRaise(EffectiveHeaderPaddingProperty, ref _effectiveHeaderPadding, value);
    }

    internal ItemsPresenter? ItemsPresenterPart { get; private set; }

    internal ContentPresenter? ContentPart { get; private set; }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TabControlToken.ID;
    
    #endregion
    
    private object? _selectedContent;
    private IDataTemplate? _selectedContentTemplate;
    private CompositeDisposable? _selectedItemSubscriptions;
    private Panel? _alignWrapper;
    private Point _tabStripBorderStartPoint;
    private Point _tabStripBorderEndPoint;
    private protected readonly Dictionary<TabItem, CompositeDisposable> ItemsBindingDisposables = new();
    private const int UnselectedIndex = -1;
    private const int FirstItemIndex  = 0;
    private const int SingleItemCount = 1;

    static BaseTabControl()
    {
        SelectionModeProperty.OverrideDefaultValue<BaseTabControl>(SelectionMode.AlwaysSelected);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<BaseTabControl>(false);
        ItemsPanelProperty.OverrideDefaultValue<BaseTabControl>(DefaultPanel);
        AffectsRender<BaseTabControl>(BorderBrushProperty);
        AffectsMeasure<BaseTabControl>(TabStripMarginProperty, TabAndContentGutterProperty, TabStripPlacementProperty);
        SelectedItemProperty.Changed.AddClassHandler<BaseTabControl>((x, e) => x.UpdateSelectedContent());
    }

    public BaseTabControl()
    {
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
    }
    
    public bool CloseTab(TabItem tabItem)
    {
        if (!tabItem.IsClosable)
        {
            return false;
        }

        var index = GetTabIndex(tabItem);
        if (!CanRemoveItemAt(index))
        {
            return false;
        }

        var closingArgs = new TabClosingEventArgs(ClosingEvent, tabItem);
        RaiseEvent(closingArgs);

        if (closingArgs.Cancel)
        { 
            return false;
        }

        SelectedIndex = GetNextSelectionIndex(index);
        RemoveItemAt(index);
        
        var closedArgs = new TabClosedEventArgs(ClosedEvent, tabItem);
        RaiseEvent(closedArgs);
        
        return true;
    }

    private int GetTabIndex(TabItem tabItem)
    {
        var containerIndex = IndexFromContainer(tabItem);
        return containerIndex >= FirstItemIndex ? containerIndex : Items.IndexOf(tabItem);
    }

    private bool CanRemoveItemAt(int index)
    {
        if (ItemsSource is IList list)
        {
            return IsValidIndex(index, list.Count);
        }

        if (ItemsSource is null)
        {
            return IsValidIndex(index, Items.Count);
        }

        return false;
    }

    private void RemoveItemAt(int index)
    {
        if (ItemsSource is IList list)
        {
            list.RemoveAt(index);
        }
        else
        {
            Items.RemoveAt(index);
        }
    }

    private static bool IsValidIndex(int index, int count)
    {
        return index >= FirstItemIndex && index < count;
    }

    private int GetNextSelectionIndex(int closingIndex)
    {
        if (SelectedIndex != closingIndex)
        {
            return SelectedIndex;
        }

        var isClosingLastItem = Items.Count <= SingleItemCount;
        if (isClosingLastItem)
        {
            return UnselectedIndex;
        }

        var previousIndex = closingIndex - SingleItemCount;
        return previousIndex >= FirstItemIndex ? previousIndex : FirstItemIndex;
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is TabItem tabItem)
                    {
                        if (ItemsBindingDisposables.TryGetValue(tabItem, out var disposable))
                        {
                            disposable.Dispose();
                            ItemsBindingDisposables.Remove(tabItem);
                        }
                    }
                }
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        ItemsPresenterPart = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        ItemsPresenterPart?.ApplyTemplate();

        UpdateTabStripPlacement();

        // Set TabNavigation to Once on the panel if not already set and
        // forward the TabOnceActiveElement to the panel.
        if (ItemsPresenterPart?.Panel is { } panel)
        {
            if (!panel.IsSet(KeyboardNavigation.TabNavigationProperty))
            {
                panel.SetCurrentValue(
                    KeyboardNavigation.TabNavigationProperty,
                    KeyboardNavigationMode.Once);
            }
            KeyboardNavigation.SetTabOnceActiveElement(
                panel,
                KeyboardNavigation.GetTabOnceActiveElement(this));
        }

        _alignWrapper = e.NameScope.Find<Panel>(TabControlThemeConstants.AlignWrapperPart);
        HandlePlacementChanged();
        ConfigureEffectiveHeaderPadding();
    }
    
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);

        if (e.NavigationMethod == NavigationMethod.Directional && e.Source is TabItem)
        {
            e.Handled = UpdateSelectionFromEventSource(e.Source);
        }
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.Pointer.Type == PointerType.Mouse)
        {
            e.Handled = UpdateSelectionFromEventSource(e.Source);
        }
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left && e.Pointer.Type != PointerType.Mouse)
        {
            var container = GetContainerFromEventSource(e.Source);
            if (container != null
                && container.GetVisualsAt(e.GetPosition(container))
                            .Any(c => container == c || container.IsVisualAncestorOf(c)))
            {
                e.Handled = UpdateSelectionFromEventSource(e.Source);
            }
        }
    }
    
    private void UpdateTabStripPlacement()
    {
        var controls = ItemsPresenterPart?.Panel?.Children;
        if (controls is null)
        {
            return;
        }

        foreach (var control in controls)
        {
            if (control is TabItem tabItem)
            {
                tabItem.TabStripPlacement = TabStripPlacement;
            }
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new TabItem();
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<TabItem>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        if (index == SelectedIndex)
        {
            UpdateSelectedContent(container);
        }
        if (container is TabItem tabItem)
        {
            tabItem.TabStripPlacement = TabStripPlacement;
            
            var disposables = new CompositeDisposable(4);
            
            if (item != null && item is not Visual)
            {
                if (!tabItem.IsSet(TabItem.ContentProperty))
                {
                    tabItem.SetCurrentValue(TabItem.ContentProperty, item);
                }

                if (item is ITabItemData tabItemData)
                {
                    if (!tabItem.IsSet(TabItem.IconProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.IconProperty, tabItemData.Icon);
                    }
                    if (!tabItem.IsSet(TabItem.CloseIconProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.CloseIconProperty, tabItemData.CloseIcon);
                    }
                    if (!tabItem.IsSet(TabItem.HeaderProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.HeaderProperty, tabItemData.Header);
                    }
                    if (!tabItem.IsSet(TabItem.IsEnabledProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.IsEnabledProperty, tabItemData.IsEnabled);
                    }
                    if (!tabItem.IsSet(TabItem.IsClosableProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.IsClosableProperty, tabItemData.IsClosable);
                    }
                    if (!tabItem.IsSet(TabItem.IsAutoHideCloseButtonProperty))
                    {
                        tabItem.SetCurrentValue(TabItem.IsAutoHideCloseButtonProperty, tabItemData.IsAutoHideCloseButton);
                    }
                }
            }

            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, tabItem, TabItem.ContentTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, tabItem, TabItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, tabItem, TabItem.IsMotionEnabledProperty));
            
            PrepareTabItem(tabItem, item, index, disposables);
            
            if (ItemsBindingDisposables.TryGetValue(tabItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                ItemsBindingDisposables.Remove(tabItem);
            }
            ItemsBindingDisposables.Add(tabItem, disposables);
            ConfigureTabItem(tabItem);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TabItem.");
        }
    }
    
    protected virtual void PrepareTabItem(TabItem tabItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }
    
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);

        var selectedIndex = SelectedIndex;

        if (selectedIndex == oldIndex || selectedIndex == newIndex)
        {
            UpdateSelectedContent();
        }
    }
    
    protected override void ClearContainerForItemOverride(Control element)
    {
        base.ClearContainerForItemOverride(element);
        UpdateSelectedContent();
    }
    
    private void UpdateSelectedContent(Control? container = null)
    {
        _selectedItemSubscriptions?.Dispose();
        _selectedItemSubscriptions = null;

        if (SelectedIndex == -1)
        {
            SelectedContent = SelectedContentTemplate = null;
        }
        else
        {
            container ??= ContainerFromIndex(SelectedIndex);
            if (container != null)
            {
                if (SelectedContentTemplate != SelectContentTemplate(container.GetValue(ContentTemplateProperty)))
                {
                    // If the value of SelectedContentTemplate is about to change, clear it first. This ensures
                    // that the template is not reused as soon as SelectedContent changes in the statement below
                    // this block, and also that controls generated from it are unloaded before SelectedContent
                    // (which is typically their DataContext) changes.
                    SelectedContentTemplate = null;
                }

                _selectedItemSubscriptions = new CompositeDisposable(
                    container.GetObservable(ContentControl.ContentProperty).Subscribe(v => SelectedContent = v),
                    container.GetObservable(ContentControl.ContentTemplateProperty).Subscribe(v => SelectedContentTemplate = SelectContentTemplate(v)));

                // Note how we fall back to our own ContentTemplate if the container doesn't specify one
                IDataTemplate? SelectContentTemplate(IDataTemplate? containerTemplate) => containerTemplate ?? ContentTemplate;
            }
        }
    }
    
    protected virtual bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (presenter.Name == "PART_SelectedContentHost")
        {
            ContentPart = presenter;
            return true;
        }

        return false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TabStripPlacementProperty)
        {
            RefreshContainers();
        }
        else if (change.Property == ContentTemplateProperty)
        {
            var newTemplate = change.GetNewValue<IDataTemplate?>();
            if (SelectedContentTemplate != newTemplate &&
                ContainerFromIndex(SelectedIndex) is { } container && 
                container.GetValue(ContentControl.ContentTemplateProperty) == null)
            {
                SelectedContentTemplate = newTemplate; // See also UpdateSelectedContent
            }
        }
        else if (change.Property == KeyboardNavigation.TabOnceActiveElementProperty &&
                 ItemsPresenterPart?.Panel is { } panel)
        {
            // Forward TabOnceActiveElement to the panel.
            KeyboardNavigation.SetTabOnceActiveElement(
                panel,
                change.GetNewValue<IInputElement?>());
        }
        if (change.Property == TabStripPlacementProperty || change.Property == TabAndContentGutterProperty)
        {
            UpdatePseudoClasses();
            HandlePlacementChanged();
        }
        else if (change.Property == HeaderStartEdgePaddingProperty || change.Property == HeaderEndEdgePaddingProperty)
        {
            ConfigureEffectiveHeaderPadding();
        }
        if (change.Property == IsTabAutoHideCloseButtonProperty ||
            change.Property == IsTabClosableProperty)
        {
            if (Items.Count > 0)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    var item = Items[i];
                    if (item is TabItem tabItem)
                    {
                        ConfigureTabItem(tabItem);
                    }
                }
            }
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(TabPseudoClass.Top, TabStripPlacement == Dock.Top);
        PseudoClasses.Set(TabPseudoClass.Right, TabStripPlacement == Dock.Right);
        PseudoClasses.Set(TabPseudoClass.Bottom, TabStripPlacement == Dock.Bottom);
        PseudoClasses.Set(TabPseudoClass.Left, TabStripPlacement == Dock.Left);
    }

    private void HandlePlacementChanged()
    {
        if (TabStripPlacement == Dock.Top)
        {
            SetCurrentValue(TabStripMarginProperty, new Thickness(0, 0, 0, TabAndContentGutter));
        }
        else if (TabStripPlacement == Dock.Right)
        {
            SetCurrentValue(TabStripMarginProperty, new Thickness(TabAndContentGutter, 0, 0, 0));
        }
        else if (TabStripPlacement == Dock.Bottom)
        {
            SetCurrentValue(TabStripMarginProperty, new Thickness(0, TabAndContentGutter, 0, 0));
        }
        else
        {
            SetCurrentValue(TabStripMarginProperty, new Thickness(0, 0, TabAndContentGutter, 0));
        }

        ConfigureEffectiveHeaderPadding();
    }

    private void ConfigureEffectiveHeaderPadding()
    {
        if (TabStripPlacement == Dock.Top)
        {
            SetCurrentValue(EffectiveHeaderPaddingProperty, new  Thickness(HeaderStartEdgePadding, 0, HeaderEndEdgePadding, 0));
        }
        else if (TabStripPlacement == Dock.Right)
        {
            SetCurrentValue(EffectiveHeaderPaddingProperty, new Thickness(0, HeaderStartEdgePadding, 0, HeaderEndEdgePadding));
        }
        else if (TabStripPlacement == Dock.Bottom)
        {
            SetCurrentValue(EffectiveHeaderPaddingProperty, new  Thickness(HeaderStartEdgePadding, 0, HeaderEndEdgePadding, 0));
        }
        else
        {
            SetCurrentValue(EffectiveHeaderPaddingProperty, new Thickness(0, HeaderStartEdgePadding, 0, HeaderEndEdgePadding));
        }
    }

    private void SetupTabStripBorderPoints()
    {
        if (_alignWrapper is not null)
        {
            var offset          = _alignWrapper.TranslatePoint(new Point(0, 0), this) ?? default;
            var size            = _alignWrapper.Bounds.Size;
            var borderThickness = BorderThickness.Left;
            var offsetDelta     = borderThickness / 2;
            if (TabStripPlacement == Dock.Top)
            {
                _tabStripBorderStartPoint = new Point(0, size.Height - offsetDelta);
                _tabStripBorderEndPoint   = new Point(size.Width, size.Height - offsetDelta);
            }
            else if (TabStripPlacement == Dock.Right)
            {
                _tabStripBorderStartPoint = new Point(offsetDelta, 0);
                _tabStripBorderEndPoint   = new Point(offsetDelta, size.Height);
            }
            else if (TabStripPlacement == Dock.Bottom)
            {
                _tabStripBorderStartPoint = new Point(0, offsetDelta);
                _tabStripBorderEndPoint   = new Point(size.Width, offsetDelta);
            }
            else
            {
                _tabStripBorderStartPoint = new Point(size.Width - offsetDelta, 0);
                _tabStripBorderEndPoint   = new Point(size.Width - offsetDelta, size.Height);
            }

            _tabStripBorderStartPoint += offset;
            _tabStripBorderEndPoint   += offset;
        }
    }

    public override void Render(DrawingContext context)
    {
        if (Items.Count > 0)
        {
            SetupTabStripBorderPoints();
            var borderThickness = BorderThickness.Left;
            using var optionState = context.PushRenderOptions(new RenderOptions
            {
                EdgeMode = EdgeMode.Aliased
            });
            context.DrawLine(new Pen(BorderBrush, borderThickness), _tabStripBorderStartPoint, _tabStripBorderEndPoint);
        }
    }
    
    private void ConfigureTabItem(TabItem tabItem)
    {
        tabItem.SetValue(TabItem.IsClosableProperty, IsTabClosable, BindingPriority.Template);
        tabItem.SetValue(TabItem.IsAutoHideCloseButtonProperty, IsTabAutoHideCloseButton, BindingPriority.Template);
    }
}


public class TabClosingEventArgs : RoutedEventArgs
{
    public TabClosingEventArgs(RoutedEvent routedEvent, TabItem tabItem)
        : base(routedEvent)
    {
        TabItem = tabItem;
    }
    
    public TabItem TabItem { get; }
    public bool Cancel { get; set; }
}

public class TabClosedEventArgs : RoutedEventArgs
{
    public TabClosedEventArgs(RoutedEvent routedEvent, TabItem tabItem)
        : base(routedEvent)
    {
        TabItem = tabItem;
    }
    
    public TabItem TabItem { get; }
}
