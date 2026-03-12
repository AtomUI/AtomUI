using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.Input.Raw;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls.Utils;

internal static class RawPointerEventArgsExtensions
{
    public static Control? GetInputRoot(this RawPointerEventArgs eventArgs)
    {
        var inputRoot = eventArgs.Root;
        if (inputRoot is PopupBuddyLayer popupBuddyLayer)
        {
            return popupBuddyLayer.BuddyPopup.Host as Control;
        }
        return inputRoot as Control;
    }

    public static bool IsPointLogicalIn(this RawPointerEventArgs eventArgs, Control? control)
    {
        if (control == null)
        {
            return false;
        }
        var current = eventArgs.GetInputHitTestResult().element as Control;
        while (current != null)
        {
            if (current == control)
            {
                return true;
            }
            current = current.GetLogicalParent() as Control;
        }

        return false;
    }
}