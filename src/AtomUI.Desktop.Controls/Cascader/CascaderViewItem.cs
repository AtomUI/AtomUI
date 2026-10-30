using System.Collections.Specialized;
using System.Reactive.Disposables;
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

    public EntityKey? ItemKey { get; set; }

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
    
    internal static readonly RoutedEvent<RoutedEventArgs> ClearDescendantExpandedEvent =
        RoutedEvent.Register<CascaderViewItem, RoutedEventArgs>(nameof(ClearDescendantExpanded), 
            RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    
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
    
    internal event EventHandler<RoutedEventArgs>? ClearDescendantExpanded
    {
        add => AddHandler(ClearDescendantExpandedEvent, value);
        remove => RemoveHandler(ClearDescendantExpandedEvent, value);
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

    internal static readonly StyledProperty<bool> IsCandidateSelectedProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool>(nameof(IsCandidateSelected));

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

    internal bool IsCandidateSelected
    {
        get => GetValue(IsCandidateSelectedProperty);
        set => SetValue(IsCandidateSelectedProperty, value);
    }

    public int Level => GetLevel();
    internal ICascaderOption? AttachedOption => DataContext as ICascaderOption;
    
    int IListItemVirtualizingContextAware.VirtualIndex { get; set; } = -1;
    bool IListItemVirtualizingContextAware.VirtualContextOperating { get; set; }
    #endregion
    
    internal bool AsyncLoaded;
    private static readonly Point s_invalidPoint = new (double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;
    private CompositeDisposable? _cascaderOptionBindingDisposables;

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

    internal void PrepareCascaderOptionData(ICascaderOption option, IResourceHost resourceHost)
    {
        ClearCascaderOptionBindingDisposables();

        if (option is BindableCascaderOption bindableOption)
        {
            var disposables = new CompositeDisposable();
            _cascaderOptionBindingDisposables = disposables;
            disposables.Add(bindableOption.AttachResourceHost(resourceHost));

            ApplyOptionData(this, option);
            BindBindableCascaderOption(bindableOption, disposables);
        }
        else
        {
            ApplyOptionData(this, option);
        }
    }

    internal void ClearCascaderOptionBindingDisposables()
    {
        _cascaderOptionBindingDisposables?.Dispose();
        _cascaderOptionBindingDisposables = null;
    }

    internal void ClearPreparedCascaderOptionData()
    {
        ClearCascaderOptionBindingDisposables();
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
        
        if (change.Property == HasItemAsyncDataLoaderProperty ||
            change.Property == DataContextProperty)
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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ClearPreparedCascaderOptionData();
        base.OnDetachedFromVisualTree(e);
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
            if (AttachedOption != null && AttachedOption.HasChildren())
            {
                IsLeaf = false;
            }
            else if (AttachedOption?.IsLeaf == true || AsyncLoaded)
            {
                IsLeaf = true;
            }
            else
            {
                IsLeaf = false;
            }
        }
        else
        {
            IsLeaf = AttachedOption != null && !AttachedOption.HasChildren();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ConfigureIsLeaf();
    }
    
    private int GetLevel()
    {
        var level = 0;
        // 通过数据
        if (DataContext is ICascaderOption option)
        {
            var current = option;
            while (current != null)
            {
                ++level;
                current = current.ParentNode as ICascaderOption;
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

    internal void NotifyClearDescendantExpanded()
    {
        RaiseEvent(new RoutedEventArgs(ClearDescendantExpandedEvent, this));
    }

    internal static void ApplyOptionData(CascaderViewItem item, ICascaderOption option)
    {
        item.SetCurrentValue(HeaderProperty, option);
        item.ItemKey = option.ItemKey;
        item.SetCurrentValue(ValueProperty, option.Value);
        item.SetCurrentValue(IconProperty, option.Icon);
        item.SetCurrentValue(IsCheckedProperty, option.IsChecked);
        item.SetCurrentValue(IsEnabledProperty, option.IsEnabled);
        item.SetCurrentValue(IsExpandedProperty, option.IsExpanded);
        item.SetCurrentValue(IsCheckBoxEnabledProperty, option.IsCheckBoxEnabled);
        item.AsyncLoaded = false;
        item.ConfigureIsLeaf();
    }

    private void BindBindableCascaderOption(BindableCascaderOption option, CompositeDisposable disposables)
    {
        var childrenCollectionSubscription = new SerialDisposable();
        disposables.Add(childrenCollectionSubscription);

        void AttachChildrenCollectionChanged()
        {
            if (option.Children is INotifyCollectionChanged notifyCollectionChanged)
            {
                NotifyCollectionChangedEventHandler handler = (_, _) => ConfigureIsLeaf();
                notifyCollectionChanged.CollectionChanged += handler;
                childrenCollectionSubscription.Disposable = Disposable.Create(() =>
                {
                    notifyCollectionChanged.CollectionChanged -= handler;
                });
            }
            else
            {
                childrenCollectionSubscription.Disposable = Disposable.Empty;
            }
        }

        void OptionPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == BindableCascaderOption.ChildrenProperty)
            {
                AttachChildrenCollectionChanged();
            }

            SyncContainerFromBindableCascaderOption(option, e.Property);
        }

        void ContainerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            SyncBindableCascaderOptionFromContainer(option, e.Property);
        }

        option.PropertyChanged += OptionPropertyChanged;
        PropertyChanged        += ContainerPropertyChanged;
        AttachChildrenCollectionChanged();

        disposables.Add(Disposable.Create(() =>
        {
            option.PropertyChanged -= OptionPropertyChanged;
            PropertyChanged        -= ContainerPropertyChanged;
        }));
    }

    private void SyncContainerFromBindableCascaderOption(BindableCascaderOption option, AvaloniaProperty property)
    {
        if (property == BindableCascaderOption.IconProperty &&
            !Equals(Icon, option.Icon))
        {
            SetCurrentValue(IconProperty, option.Icon);
        }
        else if (property == BindableCascaderOption.IsCheckedProperty &&
                 IsChecked != option.IsChecked)
        {
            SetCurrentValue(IsCheckedProperty, option.IsChecked);
        }
        else if (property == BindableCascaderOption.IsEnabledProperty &&
                 IsEnabled != option.IsEnabled)
        {
            SetCurrentValue(IsEnabledProperty, option.IsEnabled);
        }
        else if (property == BindableCascaderOption.IsExpandedProperty &&
                 IsExpanded != option.IsExpanded)
        {
            SetCurrentValue(IsExpandedProperty, option.IsExpanded);
        }
        else if (property == BindableCascaderOption.IsCheckBoxEnabledProperty &&
                 IsCheckBoxEnabled != option.IsCheckBoxEnabled)
        {
            SetCurrentValue(IsCheckBoxEnabledProperty, option.IsCheckBoxEnabled);
        }
        else if (property == BindableCascaderOption.ValueProperty &&
                 !Equals(Value, option.Value))
        {
            SetCurrentValue(ValueProperty, option.Value);
        }
        else if (property == BindableCascaderOption.ItemKeyProperty &&
                 ItemKey != option.ItemKey)
        {
            ItemKey = option.ItemKey;
        }
        else if (property == BindableCascaderOption.IsLeafProperty ||
                 property == BindableCascaderOption.ChildrenProperty)
        {
            ConfigureIsLeaf();
        }
    }

    private void SyncBindableCascaderOptionFromContainer(BindableCascaderOption option, AvaloniaProperty property)
    {
        if (property == IsCheckedProperty &&
            option.IsChecked != IsChecked)
        {
            option.IsChecked = IsChecked;
        }
        else if (property == IsExpandedProperty &&
                 option.IsExpanded != IsExpanded)
        {
            option.IsExpanded = IsExpanded;
        }
    }
}
