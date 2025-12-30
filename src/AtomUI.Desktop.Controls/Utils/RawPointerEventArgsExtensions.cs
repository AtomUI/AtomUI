using Avalonia.Controls;
using Avalonia.Input.Raw;

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
}