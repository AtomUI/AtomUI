using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Input;
using Avalonia.Input.Raw;

namespace AtomUI.Controls;

internal static class RawPointerEventTypeReflectionExtensions
{
    #region 反射信息定义
    private static readonly Lazy<PropertyInfo> InputHitTestResultPropertyInfo = new(() =>
        FixedMemberReflection.RequireProperty(
            typeof(RawPointerEventArgs).GetProperty("InputHitTestResult", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(RawPointerEventArgs),
            "InputHitTestResult"));
    #endregion

    public static (IInputElement? element, IInputElement? firstEnabledAncestor) GetInputHitTestResult(this RawPointerEventArgs rawPointerEventArgs)
    {
        return rawPointerEventArgs.GetPropertyOrThrow<(IInputElement?, IInputElement?)>(InputHitTestResultPropertyInfo.Value);
    }
}
