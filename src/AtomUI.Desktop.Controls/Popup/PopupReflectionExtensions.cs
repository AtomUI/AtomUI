using System.ComponentModel;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;

internal static class PopupReflectionExtensions
{
    #region 反射信息定义

    private static readonly Lazy<EventInfo> ClosingEventInfo = new(() =>
        FixedMemberReflection.RequireEvent(
            typeof(AvaloniaPopup).GetEvent("Closing", BindingFlags.NonPublic | BindingFlags.Instance),
            typeof(AvaloniaPopup),
            "Closing"));

    private static readonly Lazy<MethodInfo> SetPopupParentMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(AvaloniaPopup).GetMethod("SetPopupParent", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaPopup),
            "SetPopupParent"));

    private static readonly Lazy<MethodInfo> HandlePositionChangeMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(AvaloniaPopup).GetMethod("HandlePositionChange", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(AvaloniaPopup),
            "HandlePositionChange"));

    #endregion
    
    public static void AddClosingEventHandler(this AvaloniaPopup popup, EventHandler<CancelEventArgs> handler)
    {
        var closingEventAddMethod = ClosingEventInfo.Value.GetAddMethod(true);
        closingEventAddMethod?.Invoke(popup, [handler]);
    }

    public static void SetPopupParent(this AvaloniaPopup popup, Control? newParent)
    {
        SetPopupParentMethodInfo.Value.Invoke(popup, [newParent]);
    }

    public static void HandlePositionChange(this AvaloniaPopup popup)
    {
        HandlePositionChangeMethodInfo.Value.Invoke(popup, null);
    }
}
