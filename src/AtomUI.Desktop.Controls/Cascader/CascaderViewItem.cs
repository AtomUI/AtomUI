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

/// <summary>
/// The container of one option in a level of a <see cref="CascaderView" />. It shows the icon and the header of
/// the option, preceded by a check box when <see cref="ToggleType" /> is <see cref="ItemToggleType.CheckBox" />.
/// </summary>
/// <remarks>
/// When the item is prepared for an <see cref="ICascaderOption" />, it copies the icon, value, key, check
/// state, expansion state and enabled flags of the option into its own properties. For a
/// <see cref="BindableCascaderOption" /> the item also follows later changes of the option until it is
/// detached from the visual tree, and writes <see cref="IsChecked" /> and <see cref="IsExpanded" /> back to
/// the option.
/// </remarks>
[PseudoClasses(CascaderViewPseudoClass.NodeToggleTypeCheckBox, StdPseudoClass.Pressed, StdPseudoClass.Expanded,
    StdPseudoClass.Selected)]
public class CascaderViewItem : TemplatedControl, ISelectable, IListItemVirtualizingContextAware
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<CascaderViewItem, object?>(nameof(Header));

    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<CascaderViewItem, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// Identifies the <see cref="IsSelected" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<CascaderViewItem>();

    /// <summary>
    /// Identifies the <see cref="IsExpanded" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool>(
            nameof(IsExpanded),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Gets or sets the content presented in the header area of the item. When the item is prepared, the value is
    /// the <see cref="ICascaderOption" /> itself, not its text; <see cref="HeaderTemplate" /> builds the visuals.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that builds the visuals of <see cref="Header" />. When the item is prepared and
    /// its level has an item template, that template is bound to this property; the item template of the level
    /// follows <see cref="CascaderView.OptionTemplate" />.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<CascaderViewItem, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="IsChecked" /> property.
    /// </summary>
    public static readonly StyledProperty<bool?> IsCheckedProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool?>(nameof(IsChecked), false);

    /// <summary>
    /// Identifies the <see cref="IsLeaf" /> property.
    /// </summary>
    public static readonly DirectProperty<CascaderViewItem, bool> IsLeafProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(nameof(IsLeaf),
            o => o.IsLeaf);

    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool>(nameof(IsLoading), false);

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly DirectProperty<CascaderViewItem, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, object?>(nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v);

    /// <summary>
    /// Identifies the <see cref="IsCheckBoxEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCheckBoxEnabledProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool>(nameof(IsCheckBoxEnabled), true);

    /// <summary>
    /// Gets or sets whether the option is the selected option of the <see cref="CascaderView" />, apart from its
    /// checked state. The level list changes the value when the user activates the item. Once the value is
    /// <see langword="true" />, the view stores the option as its <c>SelectedOption</c> and raises
    /// <see cref="CascaderView.OptionSelected" />.
    /// </summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the option is expanded, which makes the <see cref="CascaderView" /> show the children
    /// of the option as the next level and paints the item with the selected option colors of the theme. The
    /// property binds two-way by default.
    /// </summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon shown in front of the option's header, or <see langword="null" /> to show none.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the check state of the option: <see langword="true" /> checked, <see langword="false" />
    /// unchecked and <see langword="null" /> partly checked, which the view uses for a parent with some but not
    /// all of its children checked.
    /// </summary>
    /// <remarks>
    /// The check box of the item template binds to this property two-way. The view stores a change on the option
    /// and derives the state of each parent option from its children.
    /// </remarks>
    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    private bool _isLeaf;

    /// <summary>
    /// Gets whether the option has no children. Only a leaf can be picked as the selection, unless the view
    /// allows selecting parents.
    /// </summary>
    /// <remarks>
    /// The item computes the value from the option. An option whose children are loaded asynchronously is a
    /// leaf only after it reports <see cref="ICascaderOption.IsLeaf" /> or its children were loaded and there are
    /// none. The template derives the visibility of the expand icon from this value and from
    /// <see cref="IsLoading" />.
    /// </remarks>
    public bool IsLeaf
    {
        get => _isLeaf;
        internal set => SetAndRaise(IsLeafProperty, ref _isLeaf, value);
    }

    private object? _value;

    /// <summary>
    /// Gets or sets the value of the option. When the view resolves a path of segments, an option matches a
    /// segment by the string form of this value when its <see cref="ItemKey" /> does not match.
    /// </summary>
    public object? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    /// <summary>
    /// Gets or sets whether the children of the option are being loaded asynchronously. While
    /// <see langword="true" />, the item shows its loading icon and the option cannot be picked as the selection.
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the option may be checked. When <see langword="false" />, the check box of the item
    /// is disabled and clicking the item leaves <see cref="IsChecked" /> unchanged.
    /// </summary>
    public bool IsCheckBoxEnabled
    {
        get => GetValue(IsCheckBoxEnabledProperty);
        set => SetValue(IsCheckBoxEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the key that identifies the option when the view resolves a path of segments. A segment
    /// matches this key first and the string form of <see cref="Value" /> second.
    /// </summary>
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
    
    /// <summary>
    /// Gets or sets the toggle marker shown in front of the option's header. With
    /// <see cref="ItemToggleType.CheckBox" /> the item shows a check box bound two-way to
    /// <see cref="IsChecked" />; with <see cref="ItemToggleType.None" /> it shows none. The item template draws
    /// nothing for <see cref="ItemToggleType.Radio" />.
    /// </summary>
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

    /// <summary>
    /// Gets the depth of the option in the option tree: 1 for an option of the root level and one more for each
    /// ancestor. The value is 0 when the data context of the item is not an <see cref="ICascaderOption" />.
    /// </summary>
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
    
    /// <summary>
    /// Calls the base member first, then keeps the item in step with the changed property: recomputes
    /// <see cref="IsLeaf" /> when <see cref="HasItemAsyncDataLoader" /> or the data context changes, and updates
    /// the pseudo-classes when <see cref="ToggleType" />, <see cref="IsChecked" />, <see cref="IsSelected" /> or
    /// <see cref="IsExpanded" /> changes. Call the base member from an override to keep these states current.
    /// </summary>
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

    /// <summary>
    /// Releases the subscriptions that connect the item to its <see cref="BindableCascaderOption" />, then calls
    /// the base member. An override must call the base member to release them.
    /// </summary>
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

    /// <summary>
    /// Calls the base member, then recomputes <see cref="IsLeaf" /> from the option that is the data context of
    /// the item.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ConfigureIsLeaf();
    }
    
    private int GetLevel()
    {
        var level = 0;
        // The level is counted along the parent chain of the option in the data context.
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
