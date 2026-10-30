using System.Collections;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;

namespace AtomUI.Controls;

internal static class ItemCollectionReflectionExtensions
{
    #region 反射信息定义
    private static readonly Lazy<MethodInfo> SetItemsSourceMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(ItemCollection).GetMethod("SetItemsSource", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(ItemCollection),
            "SetItemsSource"));
    
    #endregion

    public static void SetItemsSource(this ItemCollection items, IEnumerable? value)
    {
        SetItemsSourceMethodInfo.Value.Invoke(items, [value]);
    }
}
