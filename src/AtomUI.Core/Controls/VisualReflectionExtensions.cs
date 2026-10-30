using System.Reflection;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls;

internal static class VisualReflectionExtensions
{
    #region 反射信息定义

    private static readonly Lazy<MethodInfo> SetVisualParentMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(Visual).GetMethod("SetVisualParent", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(Visual),
            "SetVisualParent"));
    
    #endregion
    
    public static void SetVisualParent(this Visual visual, Control? parent)
    {
        SetVisualParentMethodInfo.Value.Invoke(visual, [parent]);
    }
}
