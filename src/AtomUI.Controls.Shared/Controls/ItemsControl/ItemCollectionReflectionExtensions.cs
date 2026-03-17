using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;

namespace AtomUI.Controls;

internal static class ItemCollectionReflectionExtensions
{
    #region 反射信息定义
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicEvents, typeof(ItemCollection))]
    private static readonly Lazy<MethodInfo> SourceChangedAddMethodInfo = new Lazy<MethodInfo>(() =>
    {
        var eventInfo = typeof(ItemCollection).GetEventInfoOrThrow("SourceChanged",
            BindingFlags.Instance | BindingFlags.NonPublic);
        // 获取 add 方法（true 表示允许获取非公共方法）
        var addMethod = eventInfo.GetAddMethod(true);
        if (addMethod == null)
        {
            throw new MissingMethodException("No add method found for SourceChanged event.");
        }
        return addMethod;
    });
    #endregion

    public static void AddSourceChangedEvent(this ItemCollection itemsCollection, EventHandler? handler)
    {
        SourceChangedAddMethodInfo.Value.Invoke(itemsCollection, [handler]);
    }
}