using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaTabStrip = Avalonia.Controls.Primitives.TabStrip;

public abstract class BaseTabStrip : AvaloniaTabStrip, 
                                     ISizeTypeAware,
                                     IMotionAwareControl,
                                     IControlSharedTokenResourcesHost
{
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel());

    #region 公共属性定义

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<BaseTabStrip>();

    public static readonly StyledProperty<Dock> TabStripPlacementProperty =
        AvaloniaProperty.Register<BaseTabStrip, Dock>(nameof(TabStripPlacement), Dock.Top);

    public static readonly StyledProperty<bool> TabAlignmentCenterProperty =
        AvaloniaProperty.Register<BaseTabStrip, bool>(nameof(TabAlignmentCenter));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<BaseTabStrip>();
    
    public static readonly StyledProperty<double> HeaderStartEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabStrip, double>(nameof (HeaderStartEdgePadding));
    
    public static readonly StyledProperty<double> HeaderEndEdgePaddingProperty = 
        AvaloniaProperty.Register<BaseTabStrip, double>(nameof (HeaderEndEdgePadding));
    
    public static readonly StyledProperty<object?> HeaderStartExtraContentProperty = 
        AvaloniaProperty.Register<BaseTabStrip, object?>(nameof (HeaderStartExtraContent));
    
    public static readonly StyledProperty<IDataTemplate?> HeaderStartExtraContentTemplateProperty =
        AvaloniaProperty.Register<BaseTabStrip, IDataTemplate?>(nameof(HeaderStartExtraContentTemplate));
    
    public static readonly StyledProperty<object?> HeaderEndExtraContentProperty = 
        AvaloniaProperty.Register<BaseTabStrip, object?>(nameof (HeaderEndExtraContent));
    
    public static readonly StyledProperty<IDataTemplate?> HeaderEndExtraContentTemplateProperty =
        AvaloniaProperty.Register<BaseTabStrip, IDataTemplate?>(nameof(HeaderEndExtraContentTemplate));
    
    public static readonly StyledProperty<bool> IsTabClosableProperty =
        AvaloniaProperty.Register<BaseTabStrip, bool>(nameof(IsTabClosable));
    
    public static readonly StyledProperty<bool> IsTabAutoHideCloseButtonProperty =
        AvaloniaProperty.Register<BaseTabStrip, bool>(nameof(IsTabAutoHideCloseButton));
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public Dock TabStripPlacement
    {
        get => GetValue(TabStripPlacementProperty);
        set => SetValue(TabStripPlacementProperty, value);
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
    
    #region 公共事件定义
    
    public static readonly RoutedEvent<TabStripClosingEventArgs> ClosingEvent =
        RoutedEvent.Register<BaseTabStrip, TabStripClosingEventArgs>(nameof(Closing), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<TabStripClosedEventArgs> ClosedEvent =
        RoutedEvent.Register<BaseTabStrip, TabStripClosedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    public event EventHandler<TabStripClosingEventArgs>? Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    public event EventHandler<TabStripClosedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }
    
    #endregion
    
    #region 内部属性定义
    internal static readonly DirectProperty<BaseTabStrip, Thickness> EffectiveHeaderPaddingProperty =
        AvaloniaProperty.RegisterDirect<BaseTabStrip, Thickness>(nameof(EffectiveHeaderPadding),
            o => o.EffectiveHeaderPadding,
            (o, v) => o.EffectiveHeaderPadding = v);
        
    private Thickness _effectiveHeaderPadding;

    internal Thickness EffectiveHeaderPadding
    {
        get => _effectiveHeaderPadding;
        set => SetAndRaise(EffectiveHeaderPaddingProperty, ref _effectiveHeaderPadding, value);
    }
    
    private protected readonly Dictionary<TabStripItem, CompositeDisposable> ItemsBindingDisposables = new();
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TabControlToken.ID;
    
    #endregion

    static BaseTabStrip()
    {
        ItemsPanelProperty.OverrideDefaultValue<BaseTabStrip>(DefaultPanel);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<BaseTabStrip>(false);
        AffectsRender<BaseTabStrip>(TabStripPlacementProperty, BorderBrushProperty);
        AffectsMeasure<BaseTabStrip>(TabStripPlacementProperty);
    }

    public BaseTabStrip()
    {
        this.RegisterResources();
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
    }
    
    public bool CloseTab(TabStripItem tabStripItem)
    {
        if (!tabStripItem.IsClosable)
        {
            return false;
        }
        var closingArgs = new TabStripClosingEventArgs(ClosingEvent, tabStripItem);
        RaiseEvent(closingArgs);

        if (closingArgs.Cancel)
        { 
            return false;
        }

        if (SelectedItem == tabStripItem)
        {
            var index = Items.IndexOf(tabStripItem);
            if (index > 0)
            {
                SelectedIndex = index - 1;
            }
            else if (Items.Count > 1)
            {
                SelectedIndex = 1;
            }
            else
            {
                SelectedIndex = -1;
            }
        }
        
        Items.Remove(tabStripItem);
        
        var closedArgs = new TabStripClosedEventArgs(ClosedEvent, tabStripItem);
        RaiseEvent(closedArgs);
        
        return true;
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is TabStripItem tabItem)
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

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TabStripItem tabStripItem)
        {
            var disposables = new CompositeDisposable(4);
            tabStripItem.TabStripPlacement = TabStripPlacement;
            
            if (item != null && item is not Visual)
            {
                if (!tabStripItem.IsSet(TabStripItem.ContentProperty))
                {
                    tabStripItem.SetCurrentValue(TabStripItem.ContentProperty, item);
                }
                
                if (item is ITabItemData tabItemData)
                {
                    if (!tabStripItem.IsSet(TabStripItem.IconProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.IconProperty, tabItemData.Icon);
                    }
                    if (!tabStripItem.IsSet(TabStripItem.CloseIconProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.CloseIconProperty, tabItemData.CloseIcon);
                    }
                    if (!tabStripItem.IsSet(TabStripItem.ContentProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.ContentProperty, tabItemData.Header);
                    }
                    if (!tabStripItem.IsSet(TabStripItem.IsEnabledProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.IsEnabledProperty, tabItemData.IsEnabled);
                    }
                    if (!tabStripItem.IsSet(TabStripItem.IsClosableProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.IsClosableProperty, tabItemData.IsClosable);
                    }
                    if (!tabStripItem.IsSet(TabStripItem.IsAutoHideCloseButtonProperty))
                    {
                        tabStripItem.SetCurrentValue(TabStripItem.IsAutoHideCloseButtonProperty, tabItemData.IsAutoHideCloseButton);
                    }
                }
            }

            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, tabStripItem, TabItem.ContentTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, tabStripItem, TabStripItem.SizeTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, tabStripItem, TabStripItem.IsMotionEnabledProperty));
            
            PrepareTabStripItem(tabStripItem, item, index, disposables);
            
            if (ItemsBindingDisposables.TryGetValue(tabStripItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                ItemsBindingDisposables.Remove(tabStripItem);
            }
            ItemsBindingDisposables.Add(tabStripItem, disposables);
            ConfigureTabStripItem(tabStripItem);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type TabStripItem.");
        }
    }

    protected virtual void PrepareTabStripItem(TabStripItem tabStripItem, object? item, int index, CompositeDisposable compositeDisposable)
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TabStripPlacementProperty)
        {
            UpdatePseudoClasses();
            for (var i = 0; i < ItemCount; ++i)
            {
                var itemContainer = ContainerFromIndex(i);
                if (itemContainer is TabStripItem tabStripItem)
                {
                    tabStripItem.TabStripPlacement = TabStripPlacement;
                }
            }

            ConfigureEffectiveHeaderPadding();
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
                    if (item is TabStripItem tabStripItem)
                    {
                        ConfigureTabStripItem(tabStripItem);
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

    public override void Render(DrawingContext context)
    {
        if (Items.Count > 0)
        {
            Point startPoint      = default;
            Point endPoint        = default;
            var   borderThickness = BorderThickness.Left;
            var   offsetDelta     = borderThickness / 2;
            if (TabStripPlacement == Dock.Top)
            {
                startPoint = new Point(0, Bounds.Height - offsetDelta);
                endPoint   = new Point(Bounds.Width, Bounds.Height - offsetDelta);
            }
            else if (TabStripPlacement == Dock.Right)
            {
                startPoint = new Point(offsetDelta, 0);
                endPoint   = new Point(offsetDelta, Bounds.Height);
            }
            else if (TabStripPlacement == Dock.Bottom)
            {
                startPoint = new Point(0, offsetDelta);
                endPoint   = new Point(Bounds.Width, offsetDelta);
            }
            else
            {
                startPoint = new Point(Bounds.Width - offsetDelta, 0);
                endPoint   = new Point(Bounds.Width - offsetDelta, Bounds.Height);
            }

            using var optionState = context.PushRenderOptions(new RenderOptions
            {
                EdgeMode = EdgeMode.Aliased
            });
            context.DrawLine(new Pen(BorderBrush, borderThickness), startPoint, endPoint);
        }
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureEffectiveHeaderPadding();
    }
    
    private void ConfigureTabStripItem(TabStripItem tabItem)
    {
        tabItem.SetValue(TabStripItem.IsClosableProperty, IsTabClosable, BindingPriority.Template);
        tabItem.SetValue(TabStripItem.IsAutoHideCloseButtonProperty, IsTabAutoHideCloseButton, BindingPriority.Template);
    }
}

public class TabStripClosingEventArgs : RoutedEventArgs
{
    public TabStripClosingEventArgs(RoutedEvent routedEvent, TabStripItem tabStripItem)
        : base(routedEvent)
    {
        TabStripItem = tabStripItem;
    }
    
    public TabStripItem TabStripItem { get; }
    public bool Cancel { get; set; }
}

public class TabStripClosedEventArgs : RoutedEventArgs
{
    public TabStripClosedEventArgs(RoutedEvent routedEvent, TabStripItem tabStripItem)
        : base(routedEvent)
    {
        TabStripItem = tabStripItem;
    }
    
    public TabStripItem TabStripItem { get; }
}