using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Input;

namespace AtomUI.Controls;

internal static class FocusManagerReflectionExtensions
{
    #region 反射信息定义

    private static readonly Lazy<MethodInfo> GetFocusManagerMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(FocusManager).GetMethod("GetFocusManager", BindingFlags.Static | BindingFlags.NonPublic),
            typeof(FocusManager),
            "GetFocusManager"));

    #endregion

    /// <summary>
    /// Calls internal FocusManager.GetFocusManager(IInputElement? element)
    /// </summary>
    public static FocusManager? GetFocusManager(IInputElement? element)
    {
        return GetFocusManagerMethodInfo.Value.Invoke(null, [element]) as FocusManager;
    }
}
