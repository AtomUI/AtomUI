using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.MotionScene;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

using AvaloniaTreeItem = Avalonia.Controls.TreeViewItem;

[PseudoClasses(CascaderViewPseudoClass.NodeToggleTypeCheckBox, CascaderViewPseudoClass.NodeToggleTypeRadio)]
public class CascaderViewItem : AvaloniaTreeItem, IRadioButton, ICascaderViewItemData
{
    #region 公共属性定义
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<CascaderViewItem, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<bool?> IsCheckedProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool?>(nameof(IsChecked), false);

    public static readonly DirectProperty<CascaderViewItem, bool> IsLeafProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(nameof(IsLeaf),
            o => o.IsLeaf);

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool>(nameof(IsLoading), false);
    
    public static readonly StyledProperty<string?> GroupNameProperty =
        RadioButton.GroupNameProperty.AddOwner<CascaderViewItem>();
    
    public static readonly DirectProperty<CascaderViewItem, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, object?>(nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v);
    
    public static readonly StyledProperty<bool> IsIndicatorEnabledProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool>(nameof(IsIndicatorEnabled), true);

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

    public string? GroupName
    {
        get => GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }
    
    public bool IsIndicatorEnabled
    {
        get => GetValue(IsIndicatorEnabledProperty);
        set => SetValue(IsIndicatorEnabledProperty, value);
    }
    
    IList<ICascaderViewItemData> ITreeNode<ICascaderViewItemData>.Children => Items.OfType<ICascaderViewItemData>().ToList();
    ITreeNode<ICascaderViewItemData>? ITreeNode<ICascaderViewItemData>.ParentNode => Parent as ITreeNode<ICascaderViewItemData>;
    
    public TreeNodeKey? ItemKey { get; set; }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<AvaloniaTreeItem, RoutedEventArgs>(
            nameof(Click),
            RoutingStrategies.Bubble);
    
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }
    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<PathIcon?> ExpandIconProperty =
        AvaloniaProperty.Register<CascaderViewItem, PathIcon?>(nameof(ExpandIcon));

    internal static readonly StyledProperty<PathIcon?> LoadingIconProperty =
        AvaloniaProperty.Register<CascaderViewItem, PathIcon?>(nameof(LoadingIcon));
    
    internal static readonly DirectProperty<CascaderViewItem, bool> IsShowIconProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(nameof(IsShowIcon),
            o => o.IsShowIcon,
            (o, v) => o.IsShowIcon = v);

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CascaderViewItem>();
    
    internal static readonly StyledProperty<ItemToggleType> ToggleTypeProperty =
        TreeView.ToggleTypeProperty.AddOwner<CascaderViewItem>();
    
    internal static readonly StyledProperty<bool> IsShowLeafIconProperty =
        AvaloniaProperty.Register<CascaderViewItem, bool>(nameof(IsShowLeafIcon));
    
    internal static readonly DirectProperty<CascaderViewItem, bool> HasItemAsyncDataLoaderProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(nameof(HasItemAsyncDataLoader),
            o => o.HasItemAsyncDataLoader,
            (o, v) => o.HasItemAsyncDataLoader = v);
    
    internal static readonly DirectProperty<CascaderViewItem, bool> IsAutoExpandParentProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(nameof(IsAutoExpandParent),
            o => o.IsAutoExpandParent,
            (o, v) => o.IsAutoExpandParent = v);
    
    internal static readonly DirectProperty<CascaderViewItem, bool> IsFilterModeProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(nameof(IsFilterMode),
            o => o.IsFilterMode,
            (o, v) => o.IsFilterMode = v);
    
    internal static readonly DirectProperty<CascaderViewItem, bool> IsFilterMatchProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(nameof(IsFilterMatch),
            o => o.IsFilterMatch,
            (o, v) => o.IsFilterMatch = v);
    
    internal static readonly DirectProperty<CascaderViewItem, string?> FilterHighlightWordsProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, string?>(nameof(FilterHighlightWords),
            o => o.FilterHighlightWords,
            (o, v) => o.FilterHighlightWords = v);

    internal static readonly StyledProperty<IBrush?> FilterHighlightForegroundProperty =
        TreeView.FilterHighlightForegroundProperty.AddOwner<CascaderViewItem>();
    
    internal static readonly DirectProperty<CascaderViewItem, CascaderItemFilterAction> ItemFilterActionProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, CascaderItemFilterAction>(
            nameof(ItemFilterAction),
            o => o.ItemFilterAction,
            (o, v) => o.ItemFilterAction = v);
    
    internal static readonly DirectProperty<CascaderViewItem, bool> IsSelectableProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewItem, bool>(
            nameof(IsSelectable),
            o => o.IsSelectable,
            (o, v) => o.IsSelectable = v);
    
    internal PathIcon? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }

    internal PathIcon? LoadingIcon
    {
        get => GetValue(LoadingIconProperty);
        set => SetValue(LoadingIconProperty, value);
    }
    
    private bool _isShowIcon = true;

    internal bool IsShowIcon
    {
        get => _isShowIcon;
        set => SetAndRaise(IsShowIconProperty, ref _isShowIcon, value);
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

    internal bool IsShowLeafIcon
    {
        get => GetValue(IsShowLeafIconProperty);
        set => SetValue(IsShowLeafIconProperty, value);
    }
    
    private bool _hasItemAsyncDataLoader;

    internal bool HasItemAsyncDataLoader
    {
        get => _hasItemAsyncDataLoader;
        set => SetAndRaise(HasItemAsyncDataLoaderProperty, ref _hasItemAsyncDataLoader, value);
    }
        
    private bool _isAutoExpandParent;

    internal bool IsAutoExpandParent
    {
        get => _isAutoExpandParent;
        set => SetAndRaise(IsAutoExpandParentProperty, ref _isAutoExpandParent, value);
    }
    
    private bool _isFilterMode;

    internal bool IsFilterMode
    {
        get => _isFilterMode;
        set => SetAndRaise(IsFilterModeProperty, ref _isFilterMode, value);
    }
    
    private bool _isFilterMatch;

    internal bool IsFilterMatch
    {
        get => _isFilterMatch;
        set => SetAndRaise(IsFilterMatchProperty, ref _isFilterMatch, value);
    }
    
    private string? _filterHighlightWords;

    internal string? FilterHighlightWords
    {
        get => _filterHighlightWords;
        set => SetAndRaise(FilterHighlightWordsProperty, ref _filterHighlightWords, value);
    }
    
    internal IBrush? FilterHighlightForeground
    {
        get => GetValue(FilterHighlightForegroundProperty);
        set => SetValue(FilterHighlightForegroundProperty, value);
    }
    
    private CascaderItemFilterAction _itemFilterAction = CascaderItemFilterAction.All;
    
    internal CascaderItemFilterAction ItemFilterAction
    {
        get => _itemFilterAction;
        set => SetAndRaise(ItemFilterActionProperty, ref _itemFilterAction, value);
    }

    private bool _isSelectable = true;
    
    internal bool IsSelectable
    {
        get => _isSelectable;
        set => SetAndRaise(IsSelectableProperty, ref _isSelectable, value);
    }
    
    internal CascaderView? OwnerView { get; set; }

    private ICascaderViewInteractionHandler? CascaderViewInteractionHandler => this.FindLogicalAncestorOfType<CascaderView>()?.InteractionHandler;
    
    #endregion
    
    private BaseMotionActor? _itemsPresenterMotionActor;
    private readonly BorderRenderHelper _borderRenderHelper;
    private TreeViewItemHeader? _header;
    private readonly Dictionary<AvaloniaTreeItem, CompositeDisposable> _itemsBindingDisposables = new();
    internal bool AsyncLoaded;
    private FilterContextBackup? _filterContextBackup;

    static CascaderViewItem()
    {
        AffectsRender<AvaloniaTreeItem>(
            IsShowLeafIconProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            BackgroundProperty);
    }

    public CascaderViewItem()
    {
        _borderRenderHelper               =  new BorderRenderHelper();
        LogicalChildren.CollectionChanged += HandleLogicalChildrenChanged;
    }
    
    private void HandleLogicalChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is AvaloniaTreeItem cascaderViewItem)
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
        else if (change.Property == GroupNameProperty)
        {
            HandleGroupNameChanged(change);
        }
        else if (change.Property == IsCheckedProperty)
        {
            HandleIsCheckedChanged(change);
        }
        else if (change.Property == ToggleTypeProperty)
        {
            HandleToggleTypeChanged(change);
        }
        else if (change.Property == IsExpandedProperty)
        {
            HandleExpandedChanged();
        }

        if (change.Property == IsSelectedProperty)
        {
            if (IsSelected && !IsSelectable)
            {
                SetCurrentValue(IsSelectedProperty, false);
                throw new InvalidOperationException("The IsSelectable property value is False, meaning selection is not supported.");
            }
        }

        if (change.Property == IsSelectableProperty)
        {
            if (!IsSelectable)
            {
                SetCurrentValue(IsSelectedProperty, false);
            }
        }
    }

    private void HandleExpandedChanged()
    {
        if (IsExpanded)
        {
            ExpandChildren();
        }
        else
        {
            CollapseChildren();
        }
    }

    private void HandleGroupNameChanged(AvaloniaPropertyChangedEventArgs change)
    {
        (CascaderViewInteractionHandler as DefaultCascaderViewInteractionHandler)?.OnGroupOrTypeChanged(this, change.GetOldValue<string>());
    }
    
    private void HandleToggleTypeChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newValue = change.GetNewValue<ItemToggleType>();
        PseudoClasses.Set(TreeViewPseudoClass.NodeToggleTypeRadio, newValue == ItemToggleType.Radio);
        PseudoClasses.Set(TreeViewPseudoClass.NodeToggleTypeCheckBox, newValue == ItemToggleType.CheckBox);
        (CascaderViewInteractionHandler as DefaultCascaderViewInteractionHandler)?.OnGroupOrTypeChanged(this, GroupName);
    }
    
    private void HandleIsCheckedChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var newValue = change.GetNewValue<bool?>();
        PseudoClasses.Set(StdPseudoClass.Checked, newValue == true);
        (CascaderViewInteractionHandler as DefaultCascaderViewInteractionHandler)?.OnCheckedChanged(this);
    }

    private void ExpandChildren()
    {
        // if (_itemsPresenterMotionActor is null ||
        //     _animating ||
        //     OwnerView is null ||
        //     _isRealExpanded)
        // {
        //     return;
        // }
        //
        // if (!IsMotionEnabled || OwnerView.IsExpandAllProcess)
        // {
        //     _itemsPresenterMotionActor.Opacity   = 1.0;
        //     _itemsPresenterMotionActor.IsVisible = true;
        //     _isRealExpanded                      = true;
        //     return;
        // }
        //
        // _animating = true;
        // _header?.NotifyAnimating(true);
        //
        // var motion = OwnerView.OpenMotion ?? new ExpandMotion(Direction.Top, null, new CubicEaseOut());
        // motion.Duration = OwnerView.MotionDuration;
        //
        // motion.Run(_itemsPresenterMotionActor, () => { _itemsPresenterMotionActor.IsVisible = true; },
        //     () =>
        //     {
        //         _animating = false;
        //         _header?.NotifyAnimating(false);
        //         _isRealExpanded = true;
        //         if (IsAutoExpandParent)
        //         {
        //             if (Parent is AvaloniaTreeItem parentTreeItem)
        //             {
        //                 parentTreeItem.SetCurrentValue(IsExpandedProperty, true);
        //             }
        //         }
        //     });
    }

    private void CollapseChildren()
    {
        // if (_itemsPresenterMotionActor is null ||
        //     _animating ||
        //     OwnerView is null ||
        //     !_isRealExpanded)
        // {
        //     return;
        // }
        //
        // if (!IsMotionEnabled || OwnerView.IsExpandAllProcess)
        // {
        //     _itemsPresenterMotionActor.Opacity   = 0.0;
        //     _itemsPresenterMotionActor.IsVisible = false;
        //     _isRealExpanded                      = false;
        //     return;
        // }
        //
        // _animating = true;
        // _header?.NotifyAnimating(true);
        //
        // var motion = OwnerView.CloseMotion ?? new CollapseMotion(Direction.Top, null, new CubicEaseIn());
        // motion.Duration = OwnerView.MotionDuration;
        //
        // motion.Run(_itemsPresenterMotionActor, null, () =>
        // {
        //     _itemsPresenterMotionActor.IsVisible = false;
        //     _animating                           = false;
        //     _header?.NotifyAnimating(false);
        //     _isRealExpanded = false;
        // });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _header             = e.NameScope.Find<TreeViewItemHeader>(TreeViewItemThemeConstants.HeaderPart);
        _itemsPresenterMotionActor =
            e.NameScope.Find<BaseMotionActor>(TreeViewItemThemeConstants.ItemsPresenterMotionActorPart);
        
        ConfigureIsLeaf();
        
        var originIsMotionEnabled = IsMotionEnabled;
        try
        {
            SetCurrentValue(IsMotionEnabledProperty, false);
            HandleExpandedChanged();
        }
        finally
        {
            SetCurrentValue(IsMotionEnabledProperty, originIsMotionEnabled);
        }
    }
    
    internal bool IsEffectiveCheckable()
    {
        if (!IsEnabled || !IsIndicatorEnabled || ToggleType == ItemToggleType.None)
        {
            return false;
        }

        return true;
    }

    public override void Render(DrawingContext context)
    {
        if (_header != null)
        {
            using var state = context.PushTransform(Matrix.CreateTranslation(_header.Bounds.X, 0));
            _borderRenderHelper.Render(context,
                _header.Bounds.Size,
                new Thickness(),
                CornerRadius,
                BackgroundSizing.InnerBorderEdge,
                Background,
                null);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (PointInHeaderBounds(e))
        {
            NotifyHeaderClick();
        }
    }
    
    internal bool PointInHeaderBounds(PointerReleasedEventArgs e)
    {
        var bounds = new Rect(new Point(0, 0), _header?.Bounds.Size ?? default);
        var point  = e.GetPosition(_header);
        return bounds.Contains(point);
    }
    
    protected virtual void NotifyHeaderClick()
    {
    }
    
    internal void RaiseClick() => RaiseEvent(new RoutedEventArgs(ClickEvent));
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new CascaderViewItem();
    }
    
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
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
            disposables.Add(BindUtils.RelayBind(this, ItemFilterActionProperty, cascaderViewItem, ItemFilterActionProperty));
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, cascaderViewItem, IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowIconProperty, cascaderViewItem, IsShowIconProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowLeafIconProperty, cascaderViewItem,
                IsShowLeafIconProperty));
            disposables.Add(BindUtils.RelayBind(this, ToggleTypeProperty, cascaderViewItem, ToggleTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, IsSelectableProperty, cascaderViewItem, IsSelectableProperty));
            disposables.Add(BindUtils.RelayBind(this, FilterHighlightForegroundProperty, cascaderViewItem, FilterHighlightForegroundProperty));
            disposables.Add(BindUtils.RelayBind(this, HasItemAsyncDataLoaderProperty, cascaderViewItem,
                HasItemAsyncDataLoaderProperty));
            disposables.Add(BindUtils.RelayBind(this, IsAutoExpandParentProperty, cascaderViewItem,
                IsAutoExpandParentProperty));
            
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
            disposables.Add(BindUtils.RelayBind(cascaderViewItemData, nameof(ICascaderViewItemData.IsSelected), cascaderViewItem, IsSelectedProperty, mode:BindingMode.TwoWay));
            disposables.Add(BindUtils.RelayBind(cascaderViewItemData, nameof(ICascaderViewItemData.IsEnabled), cascaderViewItem, IsEnabledProperty, mode:BindingMode.TwoWay));
            disposables.Add(BindUtils.RelayBind(cascaderViewItemData, nameof(ICascaderViewItemData.IsExpanded), cascaderViewItem, IsExpandedProperty, mode:BindingMode.TwoWay));
            disposables.Add(BindUtils.RelayBind(cascaderViewItemData, nameof(ICascaderViewItemData.IsIndicatorEnabled), cascaderViewItem, IsIndicatorEnabledProperty, mode:BindingMode.TwoWay));
            
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
        }
        else
        {
            IsLeaf = ItemCount == 0;
        }
    }

    internal void CreateFilterContextBackup()
    {
        _filterContextBackup = new FilterContextBackup()
        {
            IsVisible = IsVisible,
        };
    }

    internal void ClearFilterMode()
    {
        if (_filterContextBackup != null)
        {
            SetCurrentValue(IsVisibleProperty, _filterContextBackup.IsVisible);
        }
        
        SetCurrentValue(IsExpandedProperty, OwnerView?.SelectedItemsClosure.Contains(this));
        IsFilterMatch        = false;
        IsFilterMode         = false;
        FilterHighlightWords = null;
        _filterContextBackup = null;
    }

    // 用户保存在树处于过滤状态结束后的节点原始上下文属性的恢复
    private record FilterContextBackup
    {
        public bool IsVisible { get; set; }
    }
}