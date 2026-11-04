using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AtomUI.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;

internal static class PopupFlyoutBaseReflectionExtensions
{
    private static readonly Lazy<MethodInfo> SetDefaultPlacementTargetMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(PopupFlyoutBase).GetMethod("SetDefaultPlacementTarget", BindingFlags.Instance | BindingFlags.NonPublic,
                null, [typeof(Control)], null),
            typeof(PopupFlyoutBase), "SetDefaultPlacementTarget"));

    [DynamicDependency("SetDefaultPlacementTarget", typeof(PopupFlyoutBase))]
    internal static void ClearDefaultPlacementTarget(this PopupFlyoutBase flyoutBase)
    {
        // The framework keeps this target for IsOpen-based reopening. Retire it
        // only with the owning registration, never during an ordinary close.
        SetDefaultPlacementTargetMethodInfo.Value.Invoke(flyoutBase, [null]);
    }

    private static readonly Lazy<FieldInfo> PopupLazyFieldInfo = new(() =>
        FixedMemberReflection.RequireField(
            typeof(PopupFlyoutBase).GetField("_popupLazy", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(PopupFlyoutBase),
            "_popupLazy"));

    private static readonly Lazy<MethodInfo> OnPopupOpenedMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(PopupFlyoutBase).GetMethod("OnPopupOpened", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(PopupFlyoutBase),
            "OnPopupOpened"));

    private static readonly Lazy<MethodInfo> OnPopupClosedMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(PopupFlyoutBase).GetMethod("OnPopupClosed", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(PopupFlyoutBase),
            "OnPopupClosed"));

    private static readonly Lazy<MethodInfo> OnPopupClosingMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(PopupFlyoutBase).GetMethod("OnPopupClosing", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(PopupFlyoutBase),
            "OnPopupClosing"));

    private static readonly Lazy<MethodInfo> OnPlacementTargetOrPopupKeyUpMethodInfo = new(() =>
        FixedMemberReflection.RequireMethod(
            typeof(PopupFlyoutBase).GetMethod("OnPlacementTargetOrPopupKeyUp", BindingFlags.Instance | BindingFlags.NonPublic),
            typeof(PopupFlyoutBase),
            "OnPlacementTargetOrPopupKeyUp"));

    public static void SetPopupLazy(this PopupFlyoutBase flyoutBase, Lazy<AvaloniaPopup> popupFactory)
    {
        PopupLazyFieldInfo.Value.SetValue(flyoutBase, popupFactory);
    }

    public static void OnPopupOpened(this PopupFlyoutBase flyoutBase, object? sender, EventArgs e)
    {
        OnPopupOpenedMethodInfo.Value.Invoke(flyoutBase, [sender, e]);
    }

    public static void OnPopupClosed(this PopupFlyoutBase flyoutBase, object? sender, EventArgs e)
    {
        OnPopupClosedMethodInfo.Value.Invoke(flyoutBase, [sender, e]);
    }

    public static void OnPopupClosing(this PopupFlyoutBase flyoutBase, object? sender, CancelEventArgs e)
    {
        OnPopupClosingMethodInfo.Value.Invoke(flyoutBase, [sender, e]);
    }

    public static void OnPlacementTargetOrPopupKeyUp(this PopupFlyoutBase flyoutBase, object? sender, KeyEventArgs e)
    {
        OnPlacementTargetOrPopupKeyUpMethodInfo.Value.Invoke(flyoutBase, [sender, e]);
    }
}
