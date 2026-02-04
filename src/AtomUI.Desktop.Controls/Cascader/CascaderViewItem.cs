using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(CascaderViewPseudoClass.NodeToggleTypeCheckBox, StdPseudoClass.Pressed, StdPseudoClass.Expanded,
    StdPseudoClass.Selected)]
public class CascaderViewItem : TemplatedControl, ISelectable, IListItemVirtualizingContextAware
{
    #region 公共属性定义
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<CascaderViewItem, object?>(nameof(Header));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<CascaderViewItem, IDataTemplate?>(nameof(HeaderTemplate));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<CascaderViewItem>();

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool>(
            nameof(IsExpanded),
            defaultBindingMode: BindingMode.TwoWay);

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<CascaderViewItem, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<bool?> IsCheckedProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool?>(nameof(IsChecked), false);

    public static readonly DirectProperty<CascaderViewItem, bool> IsLeafProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(nameof(IsLeaf),
            o => o.IsLeaf);

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool>(nameof(IsLoading), false);

    public static readonly DirectProperty<CascaderViewItem, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, object?>(nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v);

    public static readonly StyledProperty<bool> IsCheckBoxEnabledProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool>(nameof(IsCheckBoxEnabled), true);

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    private bool _isLeaf;

    public bool IsLeaf
    {
        get => _isLeaf;
        internal set => SetAndRaise(IsLeafProperty, ref _isLeaf, value);
    }

    private object? _value;

    public object? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsCheckBoxEnabled
    {
        get => GetValue(IsCheckBoxEnabledProperty);
        set => SetValue(IsCheckBoxEnabledProperty, value);
    }

    public TreeNodeKey? ItemKey { get; set; }

    #endregion

    #region 内部事件定义
    internal static readonly RoutedEvent<RoutedEventArgs> ExpandedEvent =
        RoutedEvent.Register<CascaderViewItem, RoutedEventArgs>(nameof(Expanded), 
            RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    
    internal static readonly RoutedEvent<RoutedEventArgs> CollapsedEvent =
        RoutedEvent.Register<CascaderViewItem, RoutedEventArgs>(nameof(Collapsed), 
            RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    
    internal static readonly RoutedEvent<RoutedEventArgs> CheckedEvent =
        RoutedEvent.Register<CascaderViewItem, RoutedEventArgs>(nameof(Checked), 
            RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    
    internal static readonly RoutedEvent<RoutedEventArgs> SelectedEvent =
        RoutedEvent.Register<CascaderViewItem, RoutedEventArgs>(nameof(Selected), 
            RoutingStrategies.Bubble | RoutingStrategies.Tunnel);

    internal static readonly RoutedEvent<RoutedEventArgs> ClickedEvent =
        RoutedEvent.Register<CascaderViewItem, RoutedEventArgs>(
            nameof(Clicked),
            RoutingStrategies.Bubble);
    
    internal event EventHandler<RoutedEventArgs>? Clicked
    {
        add => AddHandler(ClickedEvent, value);
        remove => RemoveHandler(ClickedEvent, value);
    }
    
    internal event EventHandler<RoutedEventArgs>? Expanded
    {
        add => AddHandler(ExpandedEvent, value);
        remove => RemoveHandler(ExpandedEvent, value);
    }
    
    internal event EventHandler<RoutedEventArgs>? Collapsed
    {
        add => AddHandler(CollapsedEvent, value);
        remove => RemoveHandler(CollapsedEvent, value);
    }
    
    internal event EventHandler<RoutedEventArgs>? Checked
    {
        add => AddHandler(CheckedEvent, value);
        remove => RemoveHandler(CheckedEvent, value);
    }
    
    internal event EventHandler<RoutedEventArgs>? Selected
    {
        add => AddHandler(SelectedEvent, value);
        remove => RemoveHandler(SelectedEvent, value);
    }
    #endregion

    #region 内部属性定义
    internal static readonly StyledProperty<IconTemplate?> ExpandIconProperty =
        AvaloniaProperty.Register<CascaderViewItem, IconTemplate?>(nameof(ExpandIcon));

    internal static readonly StyledProperty<IconTemplate?> LoadingIconProperty =
        AvaloniaProperty.Register<CascaderViewItem, IconTemplate?>(nameof(LoadingIcon));

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CascaderViewItem>();
    
    internal static readonly StyledProperty<ItemToggleType> ToggleTypeProperty =
        TreeView.ToggleTypeProperty.AddOwner<CascaderViewItem>();
    
    internal static readonly DirectProperty<CascaderViewItem, bool> HasItemAsyncDataLoaderProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(nameof(HasItemAsyncDataLoader),
            o => o.HasItemAsyncDataLoader,
            (o, v) => o.HasItemAsyncDataLoader = v);
    
    internal static readonly DirectProperty<CascaderViewItem, bool> IsMaxSelectReachedProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(nameof(IsMaxSelectReached),
            o => o.IsMaxSelectReached,
            (o, v) => o.IsMaxSelectReached = v);
    
    internal IconTemplate? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }

    internal IconTemplate? LoadingIcon
    {
        get => GetValue(LoadingIconProperty);
        set => SetValue(LoadingIconProperty, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public ItemToggleType ToggleType
    {
        get => GetValue(ToggleTypeProperty);
        set => SetValue(ToggleTypeProperty, value);
    }
    
    private bool _hasItemAsyncDataLoader;

    internal bool HasItemAsyncDataLoader
    {
        get => _hasItemAsyncDataLoader;
        set => SetAndRaise(HasItemAsyncDataLoaderProperty, ref _hasItemAsyncDataLoader, value);
    }
    
    private bool _isMaxSelectReached;

    internal bool IsMaxSelectReached
    {
        get => _isMaxSelectReached;
        set => SetAndRaise(IsMaxSelectReachedProperty, ref _isMaxSelectReached, value);
    }

    public int Level => GetLevel();
    internal ICascaderViewOption? AttachedOption => DataContext as ICascaderViewOption;
    
    int IListItemVirtualizingContextAware.VirtualIndex { get; set; } = -1;
    bool IListItemVirtualizingContextAware.VirtualContextOperating { get; set; }
    #endregion
    
    internal bool AsyncLoaded;
    private static readonly Point s_invalidPoint = new (double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;

    static CascaderViewItem()
    {
        SelectableMixin.Attach<CascaderViewItem>(IsSelectedProperty);
        PressedMixin.Attach<CascaderViewItem>();
        FocusableProperty.OverrideDefaultValue<CascaderViewItem>(true);
        IsExpandedProperty.Changed.AddClassHandler<CascaderViewItem, bool>((item, e) => item.HandleIsExpandedChanged(e));
        IsSelectedProperty.Changed.AddClassHandler<CascaderViewItem, bool>((item, e) => item.HandleIsSelectedChanged(e));
        IsCheckedProperty.Changed.AddClassHandler<CascaderViewItem, bool?>((item, e) => item.HandleIsCheckedChanged(e));
        AffectsRender<CascaderViewItem>(BorderBrushProperty,
            BorderThicknessProperty,
            BackgroundProperty);
    }
    
    private void HandleIsExpandedChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (this is IListItemVirtualizingContextAware virtualListItem && 
            virtualListItem.VirtualContextOperating)
        {
            return;
        }
        var routedEvent = args.NewValue.Value ? ExpandedEvent : CollapsedEvent;
        var eventArgs   = new RoutedEventArgs { RoutedEvent = routedEvent, Source = this };
        RaiseEvent(eventArgs);
    }
    
    private void HandleIsSelectedChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        RaiseEvent(new RoutedEventArgs(SelectedEvent, this));
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == HasItemAsyncDataLoaderProperty)
        {
            ConfigureIsLeaf();
        }
        else if (change.Property == ToggleTypeProperty)
        {
            HandleToggleTypeChanged(change);
        }

        if (change.Property == IsCheckedProperty ||
            change.Property == ToggleTypeProperty ||
            change.Property == IsSelectedProperty ||
            change.Property == IsExpandedProperty)
        {
            UpdatePseudoClasses();
        }
    }
    
    private void HandleToggleTypeChanged(AvaloniaPropertyChangedEventArgs change)
    {
    }
    
    private void HandleIsCheckedChanged(AvaloniaPropertyChangedEventArgs<bool?> change)
    {
        if (this is IListItemVirtualizingContextAware virtualListItem && 
            virtualListItem.VirtualContextOperating)
        {
            return;
        }
        RaiseEvent(new RoutedEventArgs(CheckedEvent, this));
    }
    
    internal bool IsEffectiveCheckable()
    {
        if (!IsEnabled || !IsCheckBoxEnabled || ToggleType == ItemToggleType.None)
        {
            return false;
        }

        return true;
    }

    internal void RaiseClick()
    {
        RaiseEvent(new RoutedEventArgs(ClickedEvent));
    }
 
    private void ConfigureIsLeaf()
    {
        if (HasItemAsyncDataLoader)
        {
            if (AttachedOption?.Children.Count > 0)
            {
                IsLeaf = false;
            }
            else if (AttachedOption?.IsLeaf == true || AsyncLoaded)
            {
                IsLeaf = true;
            }
        }
        else
        {
            IsLeaf = AttachedOption?.Children.Count == 0;
        }
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty),
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
        ConfigureIsLeaf();
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
    
    private int GetLevel()
    {
        var level = 0;
        // 通过数据
        if (DataContext is ICascaderViewOption option)
        {
            var current = option;
            while (current != null)
            {
                ++level;
                current = current.ParentNode as ICascaderViewOption;
            }
        }
        return level;
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(CascaderViewPseudoClass.NodeToggleTypeCheckBox, ToggleType == ItemToggleType.CheckBox);
        PseudoClasses.Set(StdPseudoClass.Expanded, IsExpanded);
        PseudoClasses.Set(StdPseudoClass.Checked, IsChecked == true);
    }
}