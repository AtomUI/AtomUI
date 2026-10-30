using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

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
    private readonly Dictionary<object, IDictionary<object, object?>> _virtualRestoreContext = new();
    
    static CascaderViewLevelList()
    {
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<CascaderViewLevelList>(false);
        RequestBringIntoViewEvent.AddClassHandler<CascaderViewLevelList>((view, e) => e.Handled = true);
        CascaderViewItem.DoubleTappedEvent.AddClassHandler<CascaderViewLevelList>((view, args) => view.HandleCascaderItemDoubleClicked(args));
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        var cascaderViewItem = new CascaderViewItem();
        cascaderViewItem.Classes.Add(CascaderSemanticParts.PopupListItemClass);
        if (item is ICascaderOption option)
        {
            CascaderViewItem.ApplyOptionData(cascaderViewItem, option);
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
            if (this is IListVirtualizingContextAware listVirtualizingContextAwareControl && 
                cascaderViewItem is IListItemVirtualizingContextAware virtualListItem)
            {
                if (item is ICascaderOption option)
                {
                    NotifyRestoreDefaultContext(cascaderViewItem, option);
                }

                if (_virtualRestoreContext.TryGetValue(index, out var context))
                {
                    listVirtualizingContextAwareControl.RestoreVirtualizingContext(cascaderViewItem, context);
                    _virtualRestoreContext.Remove(index);
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
            
            OwnerView.UpdateKeyboardCandidateContainer(cascaderViewItem);

            // Containers are prepared/recycled before their ancestor theme bindings are attached.
            // These owner bindings must share the preparation lifetime and local-value priority.
            if (ItemTemplate != null)
            {
                cascaderViewItem[!CascaderViewItem.HeaderTemplateProperty] = this[!ItemTemplateProperty];
            }

            cascaderViewItem[!CascaderViewItem.LoadingIconProperty] = OwnerView[!CascaderView.LoadingIconProperty];
            cascaderViewItem[!CascaderViewItem.ExpandIconProperty]  = OwnerView[!CascaderView.ExpandIconProperty];
            cascaderViewItem[!CascaderViewItem.IsMotionEnabledProperty] =
                OwnerView[!CascaderView.IsMotionEnabledProperty];
            cascaderViewItem[!CascaderViewItem.ToggleTypeProperty] =
                OwnerView[!CascaderView.EffectiveToggleTypeProperty];
            cascaderViewItem[!CascaderViewItem.HasItemAsyncDataLoaderProperty] =
                OwnerView[!CascaderView.HasItemAsyncDataLoaderProperty];
            cascaderViewItem[!CascaderViewItem.IsMaxSelectReachedProperty] =
                OwnerView[!CascaderView.IsMaxSelectReachedProperty];
        }
    }
    
    #region 内部协作 API

    internal bool UpdateSelectionFromPointerEvent(Control source, PointerEventArgs e)
    {
        var container = GetContainerFromEventSource(e.Source);
        if (container is CascaderViewItem cascaderViewItem && cascaderViewItem.IsLeaf)
        {
            return UpdateContainerSelection(cascaderViewItem, !cascaderViewItem.IsSelected);
        }
        return false;
    }

    #endregion
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        OwnerView?.Focus(NavigationMethod.Pointer);

        if (e.Source is Visual source)
        {
            var point = e.GetCurrentPoint(source);
            HandlePointerPressed(source, point);
        }
    }

    protected override void ClearContainerForItemOverride(Control element)
    {
        if (this is IListVirtualizingContextAware list && element is IListItemVirtualizingContextAware listItem)
        {
            var context = new Dictionary<object, object?>(5);
            list.SaveVirtualizingContext(element, context);
            _virtualRestoreContext[listItem.VirtualIndex] = context;
            list.ClearContainerValues(element);
        }
        
        base.ClearContainerForItemOverride(element);
    }

    protected virtual void NotifyRestoreDefaultContext(CascaderViewItem item, ICascaderOption option)
    {
        item.PrepareCascaderOptionData(option, GetResourceHost());
    }

    protected void NotifySaveVirtualizingContext(CascaderViewItem item, IDictionary<object, object?> context)
    {
        context.Add(CascaderViewItem.IsEnabledProperty, item.IsEnabled);
        context.Add(CascaderViewItem.IsCheckedProperty, item.IsChecked);
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
        item.ClearPreparedCascaderOptionData();
        item.ClearValue(CascaderViewItem.HeaderProperty);
        item.ClearValue(CascaderViewItem.ValueProperty);
        item.ItemKey = null;
        item.ClearValue(CascaderViewItem.IconProperty);
        item.ClearValue(CascaderViewItem.IsEnabledProperty);
        item.ClearValue(CascaderViewItem.IsCheckedProperty);
        item.ClearValue(CascaderViewItem.IsExpandedProperty);
        item.ClearValue(CascaderViewItem.IsCheckBoxEnabledProperty);
        item.ClearValue(CascaderViewItem.IsCandidateSelectedProperty);
        item.AsyncLoaded = false;
    }

    #region 实现 IListVirtualizingContextAware 接口

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

    private void HandlePointerPressed(Visual item, PointerPoint point)
    {
        if (point.Properties.IsLeftButtonPressed || point.Properties.IsRightButtonPressed)
        {
            if (GetContainerFromEventSource(item) is CascaderViewItem cascaderViewItem)
            {
                cascaderViewItem.RaiseClick();
                if (ExpandTrigger == CascaderViewExpandTrigger.Click && !cascaderViewItem.IsLeaf)
                {
                    if (cascaderViewItem.IsExpanded)
                    {
                        cascaderViewItem.NotifyClearDescendantExpanded();
                    }
                    cascaderViewItem.IsExpanded = true;
                }

                var isUpdateSelection =
                    (cascaderViewItem.IsLeaf || IsAllowSelectParent) && !cascaderViewItem.IsLoading;
                if (isUpdateSelection)
                {
                    UpdateContainerSelection(cascaderViewItem, GetNextSelectionState(cascaderViewItem));
                }
            }
        }
    }

    private void HandleCascaderItemDoubleClicked(TappedEventArgs e)
    {
        if (e.Source is Visual source)
        {
            if (IsAllowSelectParent)
            {
                if (GetContainerFromEventSource(source) is CascaderViewItem cascaderViewItem)
                {
                    if (ExpandTrigger == CascaderViewExpandTrigger.Click)
                    {
                        if (cascaderViewItem.IsExpanded)
                        {
                            cascaderViewItem.NotifyClearDescendantExpanded();
                        }
                        cascaderViewItem.IsExpanded = true;
                    }

                    var isUpdateSelection =
                        (cascaderViewItem.IsLeaf || IsAllowSelectParent) && !cascaderViewItem.IsLoading;
                    if (isUpdateSelection)
                    {
                        UpdateContainerSelection(cascaderViewItem, GetNextSelectionState(cascaderViewItem));
                    }
                }
            }
        }
    }

    private bool GetNextSelectionState(CascaderViewItem item)
    {
        if (IsAllowSelectParent && OwnerView?.IsCheckable != true)
        {
            return true;
        }

        return !item.IsSelected;
    }

    private bool UpdateContainerSelection(CascaderViewItem item, bool isSelected)
    {
        var index = IndexFromContainer(item);
        if (index == -1)
        {
            return false;
        }

        if (isSelected)
        {
            Selection.Select(index);
        }
        else
        {
            Selection.Deselect(index);
        }

        return true;
    }

    #region 内部协作 API

    internal void ResetVirtualizingContext()
    {
        _virtualRestoreContext.Clear();
    }

    #endregion

    private IResourceHost GetResourceHost()
    {
        if (OwnerView is not null)
        {
            return OwnerView;
        }

        return this;
    }
}
