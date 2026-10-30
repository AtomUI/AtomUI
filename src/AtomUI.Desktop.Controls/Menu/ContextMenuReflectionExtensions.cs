using System.ComponentModel;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

using AvaloniaContextMenu = Avalonia.Controls.ContextMenu;

internal static class ContextMenuReflectionExtensions
{
    #region 反射信息定义

    private static readonly Lazy<FieldInfo> PopupFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            typeof(AvaloniaContextMenu).GetField("_popup", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaContextMenu),
            "_popup"));

    private static readonly Lazy<MethodInfo> PopupOpenedMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(AvaloniaContextMenu).GetMethod("PopupOpened", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaContextMenu),
            "PopupOpened"));

    private static readonly Lazy<MethodInfo> PopupClosedMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(AvaloniaContextMenu).GetMethod("PopupClosed", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaContextMenu),
            "PopupClosed"));

    private static readonly Lazy<MethodInfo> PopupClosingMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(AvaloniaContextMenu).GetMethod("PopupClosing", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaContextMenu),
            "PopupClosing"));

    private static readonly Lazy<MethodInfo> PopupKeyUpMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(AvaloniaContextMenu).GetMethod("PopupKeyUp", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaContextMenu),
            "PopupKeyUp"));

    #endregion

    public static void SetPopup(this AvaloniaContextMenu contextMenu, Popup popup)
    {
        PopupFieldInfo.Value.SetValue(contextMenu, popup);
    }
    
    public static void OnPopupOpened(this AvaloniaContextMenu contextMenu, object? sender, EventArgs e)
    {
        PopupOpenedMethodInfo.Value.Invoke(contextMenu, [sender, e]);
    }

    public static void OnPopupClosed(this AvaloniaContextMenu contextMenu, object? sender, EventArgs e)
    {
        PopupClosedMethodInfo.Value.Invoke(contextMenu, [sender, e]);
    }

    public static void OnPopupClosing(this AvaloniaContextMenu contextMenu, object? sender, CancelEventArgs e)
    {
        PopupClosingMethodInfo.Value.Invoke(contextMenu, [sender, e]);
    }
    
    public static void OnPopupClosing(this AvaloniaContextMenu contextMenu, object? sender, KeyEventArgs e)
    {
        PopupKeyUpMethodInfo.Value.Invoke(contextMenu, [sender, e]);
    }
}
