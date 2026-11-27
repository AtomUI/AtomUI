using System.Reflection;
using AtomUI.Reflection;
using Avalonia;

namespace AtomUI.Utils;

// AvaloniaProperty.Notifying is not public, so it is looked up through reflection and validated by
// FixedMemberReflection.RequireProperty.
internal static class AvaloniaPropertyReflectionExtensions
{
    private static readonly Lazy<PropertyInfo> NotifyingPropertyInfo = new(() =>
        FixedMemberReflection.RequireProperty(
            typeof(AvaloniaProperty).GetProperty(
                "Notifying",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy),
            typeof(AvaloniaProperty),
            "Notifying"));

    public static void InvokeNotifying(this AvaloniaProperty property, AvaloniaObject target, bool status)
    {
        var notifyingDelegate = NotifyingPropertyInfo.Value.GetValue(property)
            as Action<AvaloniaObject, bool>;
        if (notifyingDelegate == null)
        {
            return;
        }
        notifyingDelegate.Invoke(target, status);
    }
}
