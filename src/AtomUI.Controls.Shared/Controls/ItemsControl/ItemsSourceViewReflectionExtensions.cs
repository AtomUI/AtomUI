using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;

namespace AtomUI.Controls;

internal static class ItemsSourceViewReflectionExtensions
{
    #region 反射信息定义
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(ItemsSourceView))]
    internal static readonly Lazy<MethodInfo> TryGetInitializedSourceMethodInfo = new Lazy<MethodInfo>(() =>
        typeof(ItemsSourceView).GetMethodInfoOrThrow("TryGetInitializedSource",
            BindingFlags.Instance | BindingFlags.NonPublic));
    
    #endregion

    public static IList? TryGetInitializedSource(this ItemsSourceView itemsView)
    {
        return TryGetInitializedSourceMethodInfo.Value.Invoke(itemsView, []) as IList;
    }
}