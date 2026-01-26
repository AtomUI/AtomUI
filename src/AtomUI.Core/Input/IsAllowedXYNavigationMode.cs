using Avalonia.Input;

namespace AtomUI.Input;

internal static class XYFocusHelpers
{
    internal static bool IsAllowedXYNavigationMode(this InputElement visual,
                                                   KeyDeviceType? keyDeviceType)
    {
        return IsAllowedXYNavigationMode(XYFocus.GetNavigationModes(visual), keyDeviceType);
    }

    private static bool IsAllowedXYNavigationMode(XYFocusNavigationModes modes,
                                                  KeyDeviceType? keyDeviceType)
    {
        if (!keyDeviceType.HasValue)
        {
            return true;
        }

        switch (keyDeviceType.GetValueOrDefault())
        {
            case KeyDeviceType.Keyboard:
                return modes.HasFlag(XYFocusNavigationModes.Keyboard);
            case KeyDeviceType.Gamepad:
                return modes.HasFlag(XYFocusNavigationModes.Gamepad);
            case KeyDeviceType.Remote:
                return modes.HasFlag(XYFocusNavigationModes.Remote);
            default:
                throw new ArgumentOutOfRangeException(nameof(keyDeviceType), keyDeviceType, null);
        }
    }
}