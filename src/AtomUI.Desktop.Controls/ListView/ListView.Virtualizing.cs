using AtomUI.Controls;
using AtomUI.Controls.Data;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public partial class ListView : IListVirtualizingContextAware
{
    #region 虚拟化上下文管理
    private protected readonly Dictionary<object, IDictionary<object, object?>> _virtualRestoreContext = new();

    protected sealed override void ClearContainerForItemOverride(Control element)
    {
        var originMotionEnabled = false; 
        try
        {
            _ignoreContainerSelectionChanged = true;
            if (element is IMotionAwareControl motionAwareControl)
            {
                originMotionEnabled = motionAwareControl.IsMotionEnabled;
                element.SetCurrentValue(IsMotionEnabledProperty, false);
            }
        
            if (this is IListVirtualizingContextAware list && element is IListItemVirtualizingContextAware listItem)
            {
                var context = new Dictionary<object, object?>();
                list.SaveVirtualizingContext(element, context);
                _virtualRestoreContext.Add(listItem.VirtualIndex, context);
                list.ClearContainerValues(element);
            }
            element.ClearValue(IsSelectedProperty);
            base.ClearContainerForItemOverride(element);
        }
        finally
        {
            if (element is IMotionAwareControl)
            {
                element.SetCurrentValue(IsMotionEnabledProperty, originMotionEnabled);
            }
            _ignoreContainerSelectionChanged = false;
        }
    }
    
    protected virtual void NotifyRestoreDefaultContext(ListViewItem item, IListItemData itemData)
    {
        if (!item.IsSet(ListViewItem.ContentProperty))
        {
            item.SetCurrentValue(ListViewItem.ContentProperty, itemData);
        }
        if (itemData is IGroupListItemData groupListItemData)
        {
            item.SetCurrentValue(ListViewItem.IsGroupItemProperty, groupListItemData.IsGroupItem);
        }
        item.SetCurrentValue(ListViewItem.IsEnabledProperty, itemData.IsEnabled);
    }

    protected virtual void NotifyClearContainerForVirtualizingContext(ListViewItem item)
    {
        item.ClearValue(ListViewItem.IsEnabledProperty);
        item.ClearValue(ListViewItem.IsGroupItemProperty);
    }
    
    protected virtual void NotifySaveVirtualizingContext(ListViewItem item, IDictionary<object, object?> context)
    {
        context.Add(ListViewItem.IsEnabledProperty, item.IsEnabled);
        context.Add(ListViewItem.IsGroupItemProperty, item.IsGroupItem);
    }

    protected virtual void NotifyRestoreVirtualizingContext(ListViewItem item, IDictionary<object, object?> context)
    {
        {
            if (context.TryGetValue(ListViewItem.IsEnabledProperty, out var value))
            {
                if (value is bool isEnabled)
                {
                    item.SetCurrentValue(ListViewItem.IsEnabledProperty, isEnabled);
                }
            }
        }
        {
            if (context.TryGetValue(ListViewItem.IsGroupItemProperty, out var value))
            {
                if (value is bool isGroupItem)
                {
                    item.SetCurrentValue(ListViewItem.IsGroupItemProperty, isGroupItem);
                }
            }
        }
    }

    void IListVirtualizingContextAware.SaveVirtualizingContext(Control item, IDictionary<object, object?> context)
    {
        if (item is ListViewItem listViewItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listViewItem, listItem => NotifySaveVirtualizingContext(listItem, context));
        }
    }

    void IListVirtualizingContextAware.RestoreVirtualizingContext(Control item, IDictionary<object, object?> context)
    {
        if (item is ListViewItem listViewItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listViewItem, listItem => NotifyRestoreVirtualizingContext(listItem, context));
        }
    }

    void IListVirtualizingContextAware.RestoreDefaultContext(Control item, object defaultContext)
    {
        if (item is ListViewItem listViewItem && defaultContext is IListItemData listViewItemData)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listViewItem, listItem => NotifyRestoreDefaultContext(listItem, listViewItemData));
        }
    }

    void IListVirtualizingContextAware.ClearContainerValues(Control item)
    {
        if (item is ListViewItem listViewItem)
        {
            ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure(listViewItem, NotifyClearContainerForVirtualizingContext);
        }
    }
    #endregion
}