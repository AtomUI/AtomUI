using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

internal class CascaderViewLevelList : SelectingItemsControl, IListVirtualizingContextAware
{
    #region 公共属性定义

    public static readonly DirectProperty<CascaderViewLevelList, int> LevelProperty =
        AvaloniaProperty.RegisterDirect<CascaderViewLevelList, int>(nameof(Level), 
            o => o.Level);
    
    public static readonly StyledProperty<CascaderViewExpandTrigger> ExpandTriggerProperty =
        CascaderView.ExpandTriggerProperty.AddOwner<CascaderViewLevelList>();
    
    public static readonly StyledProperty<bool> IsAllowSelectParentProperty =
        CascaderView.IsAllowSelectParentProperty.AddOwner<CascaderViewLevelList>();
    
    private int _level;
    public int Level
    {
        get => _level;
        internal set => SetAndRaise(LevelProperty, ref _level, value);
    }
    
    public CascaderViewExpandTrigger ExpandTrigger
    {
        get => GetValue(ExpandTriggerProperty);
        set => SetValue(ExpandTriggerProperty, value);
    }
    
    public bool IsAllowSelectParent
    {
        get => GetValue(IsAllowSelectParentProperty);
        set => SetValue(IsAllowSelectParentProperty, value);
    }

    #endregion
    
    internal CascaderView? OwnerView { get; set; }
    internal CascaderViewItem? ParentCascaderViewItem { get; set; }
    private readonly Dictionary<CascaderViewItem, CompositeDisposable> _itemsBindingDisposables = new();
    private readonly Dictionary<object, IDictionary<object, object?>> _virtualRestoreContext = new();
    
    static CascaderViewLevelList()
    {
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<CascaderViewLevelList>(false);
        RequestBringIntoViewEvent.AddClassHandler<CascaderViewLevelList>((view, e) => e.Handled = true);
        CascaderViewItem.DoubleTappedEvent.AddClassHandler<CascaderViewLevelList>((view, args) => view.HandleCascaderItemDoubleClicked(args));
    }
    
    public CascaderViewLevelList()
    {
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
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var cascaderViewItem = new CascaderViewItem();
        if (item is ICascaderOption option)
        {
            NotifyRestoreDefaultContext(cascaderViewItem, option);
        }
        return cascaderViewItem;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<CascaderViewItem>(item, out recycleKey);
    }
    
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        Debug.Assert(OwnerView != null);
        if (container is CascaderViewItem cascaderViewItem)
        {
            var disposables = new CompositeDisposable(8);

            if (this is IListVirtualizingContextAware listVirtualizingContextAwareControl && 
                cascaderViewItem is IListItemVirtualizingContextAware virtualListItem)
            {
                if (_virtualRestoreContext.TryGetValue(index, out var context))
                {
                    listVirtualizingContextAwareControl.RestoreVirtualizingContext(cascaderViewItem, context);
                    _virtualRestoreContext.Remove(index);
                }
                else
                {
                    if (item is ICascaderOption option)
                    {
                        NotifyRestoreDefaultContext(cascaderViewItem, option);
                    }
                }
                virtualListItem.VirtualIndex = index;
            }
            else
            {
                if (item is ICascaderOption option)
                {
                    NotifyRestoreDefaultContext(cascaderViewItem, option);
                }
            }
            
            if (ItemTemplate != null)
            {
                disposables.Add(BindUtils.RelayBind(this, ItemTemplateProperty, cascaderViewItem, CascaderViewItem.HeaderTemplateProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.LoadingIconProperty, cascaderViewItem, CascaderViewItem.LoadingIconProperty));
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.ExpandIconProperty, cascaderViewItem, CascaderViewItem.ExpandIconProperty));
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.IsMotionEnabledProperty, cascaderViewItem, CascaderViewItem.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.EffectiveToggleTypeProperty, cascaderViewItem, CascaderViewItem.ToggleTypeProperty));
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.HasItemAsyncDataLoaderProperty, cascaderViewItem,
                CascaderViewItem.HasItemAsyncDataLoaderProperty));
            disposables.Add(BindUtils.RelayBind(OwnerView, CascaderView.IsMaxSelectReachedProperty, cascaderViewItem, CascaderViewItem.IsMaxSelectReachedProperty));
            
            if (_itemsBindingDisposables.TryGetValue(cascaderViewItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(cascaderViewItem);
            }
            _itemsBindingDisposables.Add(cascaderViewItem, disposables);
        }
    }
    
    internal bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        var container = GetContainerFromEventSource(e.Source);
        if (container is CascaderViewItem cascaderViewItem && cascaderViewItem.IsLeaf)
        {
            return UpdateSelectionFromEventSource(
                source,
                true,
                false,
                true,
                e.GetCurrentPoint(source).Properties.IsRightButtonPressed);
        }
        return false;
    }
    
    private IDisposable? _clickDisposable;
    private const int DoubleClickInterval = 180;
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Source is Visual source)
        {
            if (IsAllowSelectParent)
            {
                _clickDisposable ??= DispatcherTimer.RunOnce(() =>
                {
                    _clickDisposable = null;
                    var point = e.GetCurrentPoint(source);
                    if (point.Properties.IsLeftButtonPressed || point.Properties.IsRightButtonPressed)
                    {
                        if (GetContainerFromEventSource(e.Source) is CascaderViewItem cascaderViewItem)
                        {
                            cascaderViewItem.RaiseClick();
                            if (ExpandTrigger == CascaderViewExpandTrigger.Click)
                            {
                                if (cascaderViewItem.IsLeaf)
                                {
                                    if (!cascaderViewItem.IsExpanded)
                                    {
                                        cascaderViewItem.IsExpanded = true;
                                    }
                                }
                                else
                                {
                                    cascaderViewItem.IsExpanded = !cascaderViewItem.IsExpanded;
                                }
                            }
                            if (cascaderViewItem.IsLeaf && !cascaderViewItem.IsLoading)
                            {
                                UpdateSelection(
                                    cascaderViewItem,
                                    cascaderViewItem.IsExpanded,
                                    false,
                                    true,
                                    false);
                            }
                        }
                    
                    }
                }, TimeSpan.FromMilliseconds(DoubleClickInterval));
                
            }
            else
            {
                var point = e.GetCurrentPoint(source);
                if (point.Properties.IsLeftButtonPressed || point.Properties.IsRightButtonPressed)
                {
                    if (GetContainerFromEventSource(e.Source) is CascaderViewItem cascaderViewItem)
                    {
                        cascaderViewItem.RaiseClick();
                        if (ExpandTrigger == CascaderViewExpandTrigger.Click)
                        {
                            if (cascaderViewItem.IsLeaf)
                            {
                                if (!cascaderViewItem.IsExpanded)
                                {
                                    cascaderViewItem.IsExpanded = true;
                                }
                            }
                            else
                            {
                                cascaderViewItem.IsExpanded = !cascaderViewItem.IsExpanded;
                            }
                        }
                        if (cascaderViewItem.IsLeaf && !cascaderViewItem.IsLoading)
                        {
                            UpdateSelection(
                                cascaderViewItem,
                                cascaderViewItem.IsExpanded,
                                false,
                                true,
                                false);
                        }
                    }
                }
            }
        }
    }
    
    private void HandleCascaderItemDoubleClicked(RoutedEventArgs args)
    {
        if (IsAllowSelectParent)
        {
            _clickDisposable?.Dispose();
            _clickDisposable = null;
            if (GetContainerFromEventSource(args.Source) is CascaderViewItem cascaderViewItem)
            {
                if (!cascaderViewItem.IsLeaf)
                {
                    cascaderViewItem.IsExpanded = true;
                    UpdateSelection(
                        cascaderViewItem,
                        true,
                        false,
                        true,
                        false);
                }
            }
        }
    }
    
    #region 虚拟化上下文管理
    protected override void ClearContainerForItemOverride(Control element)
    {
        if (this is IListVirtualizingContextAware list && element is IListItemVirtualizingContextAware listItem)
        {
            var context = new Dictionary<object, object?>();
            list.SaveVirtualizingContext(element, context);
            _virtualRestoreContext.Add(listItem.VirtualIndex, context);
            list.ClearContainerValues(element);
        }
        
        base.ClearContainerForItemOverride(element);
    }
    
    protected virtual void NotifyRestoreDefaultContext(CascaderViewItem item, ICascaderOption option)
    {
        item.SetCurrentValue(CascaderViewItem.HeaderProperty, option);
        item.ItemKey = option.ItemKey;
        item.SetCurrentValue(CascaderViewItem.IconProperty, option.Icon);
        item.SetCurrentValue(CascaderViewItem.IsCheckedProperty, option.IsChecked);
        item.SetCurrentValue(CascaderViewItem.IsEnabledProperty, option.IsEnabled);
        item.SetCurrentValue(CascaderViewItem.IsExpandedProperty, option.IsExpanded);
        item.SetCurrentValue(CascaderViewItem.IsCheckBoxEnabledProperty, option.IsCheckBoxEnabled);
        item.AsyncLoaded = false;
    }
    
    protected void NotifySaveVirtualizingContext(CascaderViewItem item, IDictionary<object, object?> context)
    {
        context.Add(CascaderViewItem.IsEnabledProperty, item.IsEnabled);
        context.Add(CascaderViewItem.IsCheckedProperty, item.IsChecked);
        context.Add(CascaderViewItem.IsExpandedProperty, item.IsExpanded);
        context.Add(CascaderViewItem.IsCheckBoxEnabledProperty, item.IsCheckBoxEnabled);
        context.Add(nameof(CascaderViewItem.AsyncLoaded), item.AsyncLoaded);
    }
    
    protected virtual void NotifyRestoreVirtualizingContext(CascaderViewItem item, IDictionary<object, object?> context)
    {
        {
            if (context.TryGetValue(CascaderViewItem.IsEnabledProperty, out var value))
            {
                if (value is bool isEnabled)
                {
                    item.SetCurrentValue(CascaderViewItem.IsEnabledProperty, isEnabled);
                }
            }
        }
        {
            if (context.TryGetValue(CascaderViewItem.IsCheckedProperty, out var value))
            {
                var isChecked = (bool?)value;
                item.SetCurrentValue(CascaderViewItem.IsCheckedProperty, isChecked);
            }
        }
        {
            if (context.TryGetValue(CascaderViewItem.IsExpandedProperty, out var value))
            {
                if (value is bool isExpanded)
                {
                    item.SetCurrentValue(CascaderViewItem.IsExpandedProperty, isExpanded);
                }
            }
        }
        {
            if (context.TryGetValue(CascaderViewItem.IsCheckBoxEnabledProperty, out var value))
            {
                if (value is bool isCheckBoxEnabled)
                {
                    item.SetCurrentValue(CascaderViewItem.IsCheckBoxEnabledProperty, isCheckBoxEnabled);
                }
            }
        }
        {
            if (context.TryGetValue(nameof(CascaderViewItem.AsyncLoaded), out var value))
            {
                if (value is bool asyncLoaded)
                {
                    item.AsyncLoaded = asyncLoaded;
                }
            }
        }
    }
    
    protected virtual void NotifyClearContainerForVirtualizingContext(CascaderViewItem item)
    {
        item.ClearValue(CascaderViewItem.IsEnabledProperty);
        item.ClearValue(CascaderViewItem.IsCheckedProperty);
        item.ClearValue(CascaderViewItem.IsExpandedProperty);
        item.ClearValue(CascaderViewItem.IsCheckBoxEnabledProperty);
        item.AsyncLoaded = false;
    }

    void IListVirtualizingContextAware.SaveVirtualizingContext(Control item, IDictionary<object, object?> context)
    {
        if (item is CascaderViewItem cascaderViewItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(cascaderViewItem, listItem => NotifySaveVirtualizingContext(listItem, context));
        }
    }

    void IListVirtualizingContextAware.RestoreVirtualizingContext(Control item, IDictionary<object, object?> context)
    {
        if (item is CascaderViewItem cascaderViewItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(cascaderViewItem, listItem => NotifyRestoreVirtualizingContext(listItem, context));
        }
    }

    void IListVirtualizingContextAware.RestoreDefaultContext(Control item, object defaultContext)
    {
        if (item is CascaderViewItem cascaderViewItem && defaultContext is ICascaderOption option)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(cascaderViewItem, listItem => NotifyRestoreDefaultContext(listItem, option));
        }
    }

    void IListVirtualizingContextAware.ClearContainerValues(Control item)
    {
        if (item is CascaderViewItem cascaderViewItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(cascaderViewItem, NotifyClearContainerForVirtualizingContext);
        }
    }
    #endregion
}