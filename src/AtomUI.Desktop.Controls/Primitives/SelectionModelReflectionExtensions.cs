using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls.Selection;

namespace AtomUI.Desktop.Controls.Primitives;

internal static class SelectionModelReflectionExtensions
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _setSourceMethodCache = new();
    private static readonly ConcurrentDictionary<Type, MethodInfo> _setInitSelectedItemsMethodCache = new();

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(SelectionModel<>))]
    public static void SetInitSelectedItems<T>(this SelectionModel<T> model, IList items)
    {
        var targetType = typeof(SelectionModel<T>);
        var method = _setInitSelectedItemsMethodCache.GetOrAdd(targetType, t =>
            t.GetMethodInfoOrThrow("SetInitSelectedItems", BindingFlags.Instance | BindingFlags.NonPublic));
        method.Invoke(model, [items]);
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(SelectionModel<>))]
    public static void SetSource<T>(this SelectionModel<T> model, IEnumerable? value)
    {
        var targetType = typeof(SelectionModel<T>);
        var method = _setSourceMethodCache.GetOrAdd(targetType, t =>
            t.GetMethodInfoOrThrow("SetSource", BindingFlags.Instance | BindingFlags.NonPublic));
        method.Invoke(model, [value]);
    }
}