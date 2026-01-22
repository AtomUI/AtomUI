using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(CascaderViewPseudoClass.NodeToggleTypeCheckBox, 
    StdPseudoClass.Pressed, StdPseudoClass.Expanded)]
public class CascaderViewItem : HeaderedItemsControl
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool>(
            nameof(IsExpanded),
            defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly DirectProperty<CascaderViewItem, int> LevelProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, int>(
            nameof(Level), o => o.Level);
    
    public static readonly RoutedEvent<RoutedEventArgs> ExpandedEvent =
        RoutedEvent.Register<CascaderViewItem, RoutedEventArgs>(nameof(Expanded), RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    
    public static readonly RoutedEvent<RoutedEventArgs> CollapsedEvent =
        RoutedEvent.Register<CascaderViewItem, RoutedEventArgs>(nameof(Collapsed), RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    
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
    
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    private int _level;
    public int Level
    {
        get => _level;
        private set => SetAndRaise(LevelProperty, ref _level, value);
    }
    
    public event EventHandler<RoutedEventArgs>? Expanded
    {
        add => AddHandler(ExpandedEvent, value);
        remove => RemoveHandler(ExpandedEvent, value);
    }
    
    public event EventHandler<RoutedEventArgs>? Collapsed
    {
        add => AddHandler(CollapsedEvent, value);
        remove => RemoveHandler(CollapsedEvent, value);
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
    
    private static readonly FuncTemplate<Panel?> DefaultPanel =
        new(() => new StackPanel());
    
    private CascaderView? _cascaderView;
    internal CascaderView? CascaderViewOwner => _cascaderView;
    
    public TreeNodeKey? ItemKey { get; set; }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<CascaderViewItem, RoutedEventArgs>(
            nameof(Click),
            RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
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
    
    internal static readonly DirectProperty<CascaderViewItem, Rect> HeaderBoundsProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, Rect>(
            nameof(HeaderBounds),
            o => o.HeaderBounds,
            (o, v) => o.HeaderBounds = v);
    
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
    
    private Rect _headerBounds;
    
    internal Rect HeaderBounds
    {
        get => _headerBounds;
        set => SetAndRaise(HeaderBoundsProperty, ref _headerBounds, value);
    }
    
    private bool _isMaxSelectReached;

    internal bool IsMaxSelectReached
    {
        get => _isMaxSelectReached;
        set => SetAndRaise(IsMaxSelectReachedProperty, ref _isMaxSelectReached, value);
    }

    internal CascaderView? OwnerView { get; set; }

    private ICascaderViewInteractionHandler? CascaderViewInteractionHandler => this.FindLogicalAncestorOfType<CascaderView>()?.InteractionHandler;
    
    #endregion
    
    private CascaderViewItemHeader? _header;
    private readonly Dictionary<CascaderViewItem, CompositeDisposable> _itemsBindingDisposables = new();
    internal bool AsyncLoaded;
    private bool _templateApplied;
    private bool _deferredBringIntoViewFlag;

    static CascaderViewItem()
    {
        PressedMixin.Attach<CascaderViewItem>();
        AffectsRender<CascaderViewItem>(
            BorderBrushProperty,
            BorderThicknessProperty,
            BackgroundProperty);
        FocusableProperty.OverrideDefaultValue<CascaderViewItem>(true);
        ItemsPanelProperty.OverrideDefaultValue<CascaderViewItem>(DefaultPanel);
        RequestBringIntoViewEvent.AddClassHandler<CascaderViewItem>((x, e) => x.HandleRequestBringIntoView(e));
        IsExpandedProperty.Changed.AddClassHandler<CascaderViewItem, bool>((x, e) => x.HandleIsExpandedChanged(e));
    }

    public CascaderViewItem()
    {
        LogicalChildren.CollectionChanged += HandleLogicalChildrenChanged;
        Items.CollectionChanged           += HandleCollectionChanged;
    }
    
    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_cascaderView is null)
        {
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                foreach (var i in e.OldItems!)
                {
                    _cascaderView.CheckedItems.Remove(i);
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                _cascaderView.CheckedItems.Clear();
                break;
        }
    }
    
    private void HandleIsExpandedChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        var routedEvent = args.NewValue.Value ? ExpandedEvent : CollapsedEvent;
        var eventArgs   = new RoutedEventArgs() { RoutedEvent = routedEvent, Source = this };
        RaiseEvent(eventArgs);
    }
    
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _cascaderView = this.GetLogicalAncestors().OfType<CascaderView>().FirstOrDefault();
        if (DataContext is ICascaderViewItemData cascaderViewItemData)
        {
            var                              level   = 0;
            ITreeNode<ICascaderViewItemData>? current = cascaderViewItemData;
            while (current != null)
            {
                level++;
                current = current.ParentNode;
            }
            Level = level - 1;
        }
        else
        {
            Level = CalculateDistanceFromLogicalParent<CascaderView>(this) - 1;
        }
        if (ItemTemplate == null && _cascaderView?.ItemTemplate != null)
        {
            SetCurrentValue(ItemTemplateProperty, _cascaderView.ItemTemplate);
        }
        if (ItemContainerTheme == null && _cascaderView?.ItemContainerTheme != null)
        {
            SetCurrentValue(ItemContainerThemeProperty, _cascaderView.ItemContainerTheme);
        }
    }
    
    protected virtual void HandleRequestBringIntoView(RequestBringIntoViewEventArgs e)
    {
        if (e.TargetObject == this)
        {
            if (!_templateApplied)
            {
                _deferredBringIntoViewFlag = true;
                return;
            }

            if (_header != null)
            {
                var m = _header.TransformToVisual(this);

                if (m.HasValue)
                {
                    var bounds = new Rect(_header.Bounds.Size);
                    var rect   = bounds.TransformToAABB(m.Value);
                    e.TargetRect = rect;
                }
            }
        }
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Handled)
        {
            Func<CascaderViewItem, bool>? handler =
                e.Key switch
                {
                    Key.Left => ApplyToItemOrRecursivelyIfCtrl(FocusAwareCollapseItem, e.KeyModifiers),
                    Key.Right => ApplyToItemOrRecursivelyIfCtrl(ExpandItem, e.KeyModifiers),
                    Key.Enter => ApplyToItemOrRecursivelyIfCtrl(IsExpanded ? CollapseItem : ExpandItem, e.KeyModifiers),

                    // do not handle CTRL with numpad keys
                    Key.Subtract => FocusAwareCollapseItem,
                    Key.Add => ExpandItem,
                    Key.Divide => ApplyToSubtree(CollapseItem),
                    Key.Multiply => ApplyToSubtree(ExpandItem),
                    _ => null,
                };

            if (handler is not null)
            {
                e.Handled = handler(this);
            }

            // NOTE: these local functions do not use the TreeView.Expand/CollapseSubtree
            // function because we want to know if any items were in fact expanded to set the
            // event handled status. Also the handling here avoids a potential infinite recursion/stack overflow.
            static Func<CascaderViewItem, bool> ApplyToSubtree(Func<CascaderViewItem, bool> f)
            {
                // Calling toList enumerates all items before applying functions. This avoids a
                // potential infinite loop if there is an infinite tree (the control catalog is
                // lazily infinite). But also means a lazily loaded tree will not be expanded completely.
                return t => SubTree(t)
                            .ToList()
                            .Select(treeViewItem => f(treeViewItem))
                            .Aggregate(false, (p, c) => p || c);
            }

            static Func<CascaderViewItem, bool> ApplyToItemOrRecursivelyIfCtrl(Func<CascaderViewItem,bool> f, KeyModifiers keyModifiers)
            {
                if (keyModifiers.HasAllFlags(KeyModifiers.Control))
                {
                    return ApplyToSubtree(f);
                }

                return f;
            }

            static bool ExpandItem(CascaderViewItem treeViewItem)
            {
                if (treeViewItem.ItemCount > 0 && !treeViewItem.IsExpanded)
                {
                    treeViewItem.SetCurrentValue(IsExpandedProperty, true);
                    return true;
                }

                return false;
            }

            static bool CollapseItem(CascaderViewItem treeViewItem)
            {
                if (treeViewItem.ItemCount > 0 && treeViewItem.IsExpanded)
                {
                    treeViewItem.SetCurrentValue(IsExpandedProperty, false);
                    return true;
                }

                return false;
            }

            static bool FocusAwareCollapseItem(CascaderViewItem treeViewItem)
            {
                if (treeViewItem.ItemCount > 0 && treeViewItem.IsExpanded)
                {
                    if (treeViewItem.IsFocused)
                    {
                        treeViewItem.SetCurrentValue(IsExpandedProperty, false);
                    }
                    else
                    {
                        treeViewItem.Focus(NavigationMethod.Directional);
                    }

                    return true;
                }

                return false;
            }

            static IEnumerable<CascaderViewItem> SubTree(CascaderViewItem treeViewItem)
            {
                return new[] { treeViewItem }.Concat(treeViewItem.LogicalChildren.OfType<CascaderViewItem>().SelectMany(child => SubTree(child)));
            }
        }

        // Don't call base.OnKeyDown - let events bubble up to containing TreeView.
    }
    
    private void HandleLogicalChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is CascaderViewItem cascaderViewItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(cascaderViewItem, out var disposable))
                        {
                            disposable.Dispose();
                        }
                        _itemsBindingDisposables.Remove(cascaderViewItem);
                    }
                }
            }
        }
    }
    
    private CascaderView EnsureCascaderView() => _cascaderView ??
                                                 throw new InvalidOperationException("The CascaderViewItem is not part of a CascaderView.");
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OwnerView = this.GetLogicalAncestors().OfType<CascaderView>().FirstOrDefault();
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == ItemCountProperty ||
            change.Property == HasItemAsyncDataLoaderProperty)
        {
            ConfigureIsLeaf();
        }
        else if (change.Property == IsCheckedProperty)
        {
            HandleIsCheckedChanged(change);
        }
        else if (change.Property == ToggleTypeProperty)
        {
            HandleToggleTypeChanged(change);
        }
    }
    
    private void HandleToggleTypeChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newValue = change.GetNewValue<ItemToggleType>();
        PseudoClasses.Set(CascaderViewPseudoClass.NodeToggleTypeCheckBox, newValue == ItemToggleType.CheckBox);
    }
    
    private void HandleIsCheckedChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newValue = change.GetNewValue<bool?>();
        PseudoClasses.Set(StdPseudoClass.Checked, newValue == true);
        (CascaderViewInteractionHandler as DefaultCascaderViewInteractionHandler)?.OnCheckedChanged(this);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _header             = e.NameScope.Find<CascaderViewItemHeader>(CascaderViewItemThemeConstants.HeaderPart);
        _templateApplied = true;
        ConfigureIsLeaf();
        if (_deferredBringIntoViewFlag)
        {
            _deferredBringIntoViewFlag = false;
            Dispatcher.UIThread.Post(this.BringIntoView); // must use the Dispatcher, otherwise the TreeView doesn't scroll
        }
    }
    
    internal bool IsEffectiveCheckable()
    {
        if (!IsEnabled || !IsCheckBoxEnabled || ToggleType == ItemToggleType.None)
        {
            return false;
        }

        return true;
    }
    
    internal bool PointInHeaderBounds(PointerReleasedEventArgs e)
    {
        var bounds = new Rect(new Point(0, 0), _header?.Bounds.Size ?? default);
        var point  = e.GetPosition(_header);
        return bounds.Contains(point);
    }
    
    internal void RaiseClick() => RaiseEvent(new RoutedEventArgs(ClickEvent));
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        EnsureCascaderView();
        return new CascaderViewItem();
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        EnsureCascaderView();
        return NeedsContainer<CascaderViewItem>(item, out recycleKey);
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        if (container is CascaderViewItem cascaderViewItem)
        {
            cascaderViewItem.OwnerView = OwnerView;
            var disposables = new CompositeDisposable(8);
            
            if (item != null && item is not Visual && item is ICascaderViewItemData cascaderViewItemData)
            {
                ApplyNodeData(cascaderViewItem, cascaderViewItemData, disposables);
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, cascaderViewItem, HeaderTemplateProperty));
            }
            disposables.Add(BindUtils.RelayBind(this, ItemsPanelProperty, cascaderViewItem, ItemsPanelProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, cascaderViewItem, IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, ToggleTypeProperty, cascaderViewItem, ToggleTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMaxSelectReachedProperty, cascaderViewItem, IsMaxSelectReachedProperty));
            disposables.Add(BindUtils.RelayBind(this, HasItemAsyncDataLoaderProperty, cascaderViewItem,
                HasItemAsyncDataLoaderProperty));
            
            PrepareCascaderViewItem(cascaderViewItem, item, index, disposables);
            
            if (_itemsBindingDisposables.TryGetValue(cascaderViewItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(cascaderViewItem);
            }
            _itemsBindingDisposables.Add(cascaderViewItem, disposables);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type CascaderViewItem.");
        }
    }
    
    protected virtual void PrepareCascaderViewItem(CascaderViewItem cascaderViewItem, object? item, int index, CompositeDisposable disposables)
    {
    }

    internal static void ApplyNodeData(CascaderViewItem cascaderViewItem, ICascaderViewItemData cascaderViewItemData, CompositeDisposable disposables)
    {
        if (cascaderViewItemData is not Visual)
        {
            disposables.Add(BindUtils.RelayBind(cascaderViewItemData, nameof(ICascaderViewItemData.Icon), cascaderViewItem, IconProperty, mode:BindingMode.TwoWay));
            disposables.Add(BindUtils.RelayBind(cascaderViewItemData, nameof(ICascaderViewItemData.IsChecked), cascaderViewItem, IsCheckedProperty, mode:BindingMode.TwoWay));
            disposables.Add(BindUtils.RelayBind(cascaderViewItemData, nameof(ICascaderViewItemData.IsEnabled), cascaderViewItem, IsEnabledProperty, mode:BindingMode.TwoWay));
            disposables.Add(BindUtils.RelayBind(cascaderViewItemData, nameof(ICascaderViewItemData.IsExpanded), cascaderViewItem, IsExpandedProperty, mode:BindingMode.TwoWay));
            disposables.Add(BindUtils.RelayBind(cascaderViewItemData, nameof(ICascaderViewItemData.IsCheckBoxEnabled), cascaderViewItem, IsCheckBoxEnabledProperty, mode:BindingMode.TwoWay));
            
            if (cascaderViewItem.ItemKey == null)
            {
                cascaderViewItem.ItemKey = cascaderViewItemData.ItemKey;
            }
            
            if (!cascaderViewItem.IsSet(IsLeafProperty))
            {
                cascaderViewItem.IsLeaf = cascaderViewItemData.IsLeaf;
            }
        }
    }

    private void ConfigureIsLeaf()
    {
        if (HasItemAsyncDataLoader)
        {
            if (ItemCount > 0)
            {
                IsLeaf = false;
            }
            else if (AsyncLoaded)
            {
                IsLeaf = true;
            }
            else if (DataContext is ICascaderViewItemData itemData)
            {
                IsLeaf = itemData.IsLeaf;
            }
        }
        else
        {
            IsLeaf = ItemCount == 0;
        }
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
}