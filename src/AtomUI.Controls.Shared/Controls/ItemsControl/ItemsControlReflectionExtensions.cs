using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;

namespace AtomUI.Controls;

internal static class ItemsControlReflectionExtensions
{
    #region 反射信息定义

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicProperties, typeof(ItemsControl))]
    private static readonly Lazy<PropertyInfo> WrapFocusPropertyInfo = new Lazy<PropertyInfo>(() => 
        typeof(ItemsControl).GetPropertyInfoOrThrow("WrapFocus",
            BindingFlags.Instance | BindingFlags.NonPublic));
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicFields, typeof(ItemsControl))]
    private static readonly Lazy<FieldInfo> ItemsFieldInfo = new Lazy<FieldInfo>(() => 
        typeof(ItemsControl).GetFieldInfoOrThrow("_items",
            BindingFlags.Instance | BindingFlags.NonPublic));

    #endregion
    
    public static bool GetWrapFocus(this ItemsControl itemsControl)
    {
        return WrapFocusPropertyInfo.Value.GetValue(itemsControl) as bool? ?? false;
    }

    public static void SetWrapFocus(this ItemsControl itemsControl, bool value)
    {
        WrapFocusPropertyInfo.Value.SetValue(itemsControl, value);
    }
    
    public static ItemCollection GetItems(this ItemsControl itemsControl)
    {
        var item = ItemsFieldInfo.Value.GetValue(itemsControl) as ItemCollection;
        Debug.Assert(item != null);
        return item;
    }
}