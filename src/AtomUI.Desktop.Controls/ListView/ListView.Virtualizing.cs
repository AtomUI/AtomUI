using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public partial class ListView : IListVirtualizingContextAware
{
    #region 虚拟化上下文管理
    private protected readonly Dictionary<object, IDictionary<object, object?>> _virtualRestoreContext = new();

    /// <summary>
    /// Prepares an item container for reuse after the items control detached it from its item.
    /// </summary>
    /// <param name="element">
    /// The container being released.
    /// </param>
    /// <remarks>
    /// <para>
    /// Motion is switched off on the container and selection changes reported by the container are ignored
    /// while the method runs.
    /// </para>
    /// <para>
    /// For a <see cref="ListViewItem" /> the virtualizing context is saved under the container's virtual index,
    /// so that it is restored when a container is prepared for that position again. The container values, the
    /// attached <see cref="IsSelectedProperty" /> value and the entry id are cleared, then the items control
    /// implementation runs.
    /// </para>
    /// </remarks>
    protected sealed override void ClearContainerForItemOverride(Control element)
    {
        if (element is ITransitionInitializationOwner owner)
        {
            owner.TransitionInitialization.Reset();
        }

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
                var context = new Dictionary<object, object?>(2);
                list.SaveVirtualizingContext(element, context);
                _virtualRestoreContext[listItem.VirtualIndex] = context;
                list.ClearContainerValues(element);
            }
            element.ClearValue(IsSelectedProperty);
            if (element is ListViewItem listViewItem)
            {
                listViewItem.EntryId = null;
            }
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
    
    /// <summary>
    /// Applies the values that come with the item data to a container: the data becomes <c>Content</c>, its
    /// <c>IsEnabled</c> is copied, and group data also marks the container as a group heading.
    /// </summary>
    /// <param name="item">
    /// The container that receives the values.
    /// </param>
    /// <param name="itemData">
    /// The data of the item the container stands for.
    /// </param>
    /// <remarks>
    /// <para>
    /// The list calls it when it creates a container for an item and again whenever it prepares a container
    /// for an item. A saved virtualizing context, when there is one, is restored afterwards and overrides
    /// these values.
    /// </para>
    /// <para>
    /// An override that does not call the base method must set these values itself.
    /// </para>
    /// </remarks>
    protected virtual void NotifyRestoreDefaultContext(ListViewItem item, IListItemData itemData)
    {
        item.SetCurrentValue(ListViewItem.ContentProperty, itemData);
        if (itemData is IGroupListItemData groupListItemData)
        {
            item.SetCurrentValue(ListViewItem.IsGroupItemProperty, groupListItemData.IsGroupItem);
        }
        item.SetCurrentValue(ListViewItem.IsEnabledProperty, itemData.IsEnabled);
    }

    /// <summary>
    /// Clears <c>Content</c>, <c>ContentTemplate</c>, <c>IsEnabled</c> and <c>IsGroupItem</c> from a container
    /// that is being recycled.
    /// </summary>
    /// <param name="item">
    /// The container being recycled.
    /// </param>
    /// <remarks>
    /// <para>
    /// The list calls it through <see cref="IListVirtualizingContextAware.ClearContainerValues" /> after the
    /// virtualizing context of the container was saved. An override that clears more values calls the base
    /// method for these.
    /// </para>
    /// </remarks>
    protected virtual void NotifyClearContainerForVirtualizingContext(ListViewItem item)
    {
        item.ClearValue(ListViewItem.ContentProperty);
        item.ClearValue(ListViewItem.ContentTemplateProperty);
        item.ClearValue(ListViewItem.IsEnabledProperty);
        item.ClearValue(ListViewItem.IsGroupItemProperty);
    }
    
    /// <summary>
    /// Stores the container state that must survive recycling in <paramref name="context" />: <c>IsEnabled</c>
    /// and <c>IsGroupItem</c>, each keyed by its property.
    /// </summary>
    /// <param name="item">
    /// The container being recycled.
    /// </param>
    /// <param name="context">
    /// The dictionary that receives the values to restore later.
    /// </param>
    /// <remarks>
    /// <para>
    /// The list calls it through <see cref="IListVirtualizingContextAware.SaveVirtualizingContext" /> before
    /// the container is cleared, and passes the same dictionary to
    /// <see cref="NotifyRestoreVirtualizingContext" /> when a container is prepared again for that position.
    /// An override that saves more values calls the base method for these, and must not add the same keys
    /// before doing so.
    /// </para>
    /// </remarks>
    protected virtual void NotifySaveVirtualizingContext(ListViewItem item, IDictionary<object, object?> context)
    {
        context.Add(ListViewItem.IsEnabledProperty, item.IsEnabled);
        context.Add(ListViewItem.IsGroupItemProperty, item.IsGroupItem);
    }

    /// <summary>
    /// Applies a context saved by <see cref="NotifySaveVirtualizingContext" /> to a container: <c>IsEnabled</c>
    /// and <c>IsGroupItem</c> are set when the context holds a value for them.
    /// </summary>
    /// <param name="item">
    /// The container being prepared.
    /// </param>
    /// <param name="context">
    /// The dictionary saved earlier for the position of the container.
    /// </param>
    /// <remarks>
    /// <para>
    /// The list calls it through <see cref="IListVirtualizingContextAware.RestoreVirtualizingContext" /> while
    /// it prepares a container for a position that has a saved context. It runs after
    /// <see cref="NotifyRestoreDefaultContext" />, so the restored values win. An override that restores more
    /// values calls the base method for these.
    /// </para>
    /// </remarks>
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
