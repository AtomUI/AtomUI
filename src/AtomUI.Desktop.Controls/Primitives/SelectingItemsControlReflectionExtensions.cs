using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls.Primitives;

internal static class SelectingItemsControlReflectionExtensions
{
    #region 反射信息定义
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(SelectingItemsControl))]
    private static readonly Lazy<MethodInfo> MarkContainerSelectedMethodInfo = new Lazy<MethodInfo>(() =>
        typeof(SelectingItemsControl).GetMethodInfoOrThrow("MarkContainerSelected",
            BindingFlags.Instance | BindingFlags.NonPublic));
    #endregion

    public static void MarkContainerSelected(this SelectingItemsControl control, Control container, bool selected)
    {
        MarkContainerSelectedMethodInfo.Value.Invoke(control, [container, selected ]);
    }
}