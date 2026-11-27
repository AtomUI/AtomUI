using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// An items control that keeps the state of a recycled item container across virtualization. When a container
/// is recycled, the control saves that state into <c>context</c> and clears the values of the container; when
/// the container is prepared again, it applies the default context of the new item and then the context saved
/// for it. Implemented by <c>ListBox</c>, <c>ListView</c> and the Cascader level list in AtomUI.Desktop.Controls.
/// </summary>
internal interface IListVirtualizingContextAware
{
    void SaveVirtualizingContext(Control item, IDictionary<object, object?> context);
    void RestoreVirtualizingContext(Control item, IDictionary<object, object?> context);
    void ClearContainerValues(Control item);
    void RestoreDefaultContext(Control item, object defaultContext);
}

/// <summary>
/// An item container whose position in a virtualized list is tracked by the list. <c>VirtualIndex</c> is the
/// index the list assigns when it prepares the container for a position; the implementers start it at <c>-1</c>,
/// and the list keeps the saved virtualizing contexts under it. <c>VirtualContextOperating</c> is
/// <see langword="true" /> only while <c>ListVirtualizingContextAwareUtils.ExecuteWithinContextClosure</c> runs
/// on the container, with motion disabled for that time. Implemented by the list box item, the list view item
/// and the Cascader view item in AtomUI.Desktop.Controls.
/// </summary>
internal interface IListItemVirtualizingContextAware
{
    int VirtualIndex { get; set; }
    bool VirtualContextOperating { get; set; }
}

internal static class ListVirtualizingContextAwareUtils
{
    public static void ExecuteWithinContextClosure<T>(T item, Action<T> action)
        where T : Control
    {
        if (item is IListItemVirtualizingContextAware virtualAwareItem)
        {
            bool? originIsMotionEnabled = null;
            var   motionAwareControl    = item as IMotionAwareControl;
    
            if (motionAwareControl != null)
            {
                originIsMotionEnabled = motionAwareControl.IsMotionEnabled;
            }
            try
            {
                if (motionAwareControl != null)
                {
                    item.SetCurrentValue(MotionAwareControlProperty.IsMotionEnabledProperty, false);
                }
      
                virtualAwareItem.VirtualContextOperating = true;

                action(item);
            }
            finally
            {
                virtualAwareItem.VirtualContextOperating = false;
                if (motionAwareControl != null)
                {
                    item.SetCurrentValue(MotionAwareControlProperty.IsMotionEnabledProperty, originIsMotionEnabled);
                }
            }
        }
    }
}