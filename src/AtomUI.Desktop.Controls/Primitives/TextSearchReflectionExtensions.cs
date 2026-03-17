using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Controls.Utils;
using AtomUI.Reflection;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

internal static class TextSearchUtils
{
    #region 反射信息定义
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(TextSearch))]
    internal static readonly Lazy<MethodInfo> GetEffectiveTextMethodInfo = new Lazy<MethodInfo>(() =>
        typeof(TextSearch).GetMethodInfoOrThrow("GetEffectiveText",
            BindingFlags.Static | BindingFlags.NonPublic));
    
    #endregion

    public static string GetEffectiveText(object? item, BindingEvaluator<string?>? textBindingEvaluator)
    {
        var value = GetEffectiveTextMethodInfo.Value.Invoke(null, [item, textBindingEvaluator]) as string;
        Debug.Assert(value != null);
        return value;
    }
}