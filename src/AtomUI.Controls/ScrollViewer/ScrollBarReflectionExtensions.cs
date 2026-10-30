using System.Diagnostics;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace AtomUI.Controls.Commons;

internal static class ScrollBarReflectionExtensions
{
    #region 反射信息定义
    private static readonly Lazy<FieldInfo> TimerFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            typeof(ScrollBar).GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(ScrollBar),
            "_timer"));
    
    private static readonly Lazy<PropertyInfo> IsExpandedPropertyInfo = new(() =>
        FixedMemberReflection.RequireProperty(
            typeof(ScrollBar).GetProperty("IsExpanded", BindingFlags.Instance | BindingFlags.Public),
            typeof(ScrollBar),
            "IsExpanded"));
    #endregion
    
    public static DispatcherTimer? GetTimer(this ScrollBar scrollBar)
    {
        return TimerFieldInfo.Value.GetValue(scrollBar) as DispatcherTimer;
    }
    
    public static void SetIsExpanded(this ScrollBar scrollBar, bool value)
    {
        var isExpandedSetter = IsExpandedPropertyInfo.Value.GetSetMethod(true);
        Debug.Assert(isExpandedSetter != null);
        isExpandedSetter.Invoke(scrollBar, [value]);
    }
}
