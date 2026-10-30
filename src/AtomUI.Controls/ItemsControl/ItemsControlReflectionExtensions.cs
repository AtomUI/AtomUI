using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;

namespace AtomUI.Controls;

internal static class ItemsControlReflectionExtensions
{
    #region 反射信息定义

    private static readonly Lazy<PropertyInfo> WrapFocusPropertyInfo = new(() =>
        FixedMemberReflection.RequireProperty(
            typeof(ItemsControl).GetProperty("WrapFocus", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(ItemsControl),
            "WrapFocus"));
    
    #endregion
    
    public static void SetWrapFocus(this ItemsControl itemsControl, bool value)
    {
        WrapFocusPropertyInfo.Value.SetValue(itemsControl, value);
    }
}
