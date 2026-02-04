using Avalonia.Controls;

namespace AtomUI.Controls;

internal interface IListVirtualizingContextAware
{
    void SaveVirtualizingContext(Control item, IDictionary<object, object?> context);
    void RestoreVirtualizingContext(Control item, IDictionary<object, object?> context);
    void ClearContainerValues(Control item);
    void RestoreDefaultContext(Control item, object defaultContext);
}

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