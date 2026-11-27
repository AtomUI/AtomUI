using Avalonia.Input;
using Avalonia.VisualTree;

namespace AtomUI.Input;

/// <summary>
/// Helper methods for XY focus navigation.
/// Extracted from Avalonia 12 internal XYFocusHelpers class.
/// </summary>
internal static class XYFocusHelpers
{
    /// <summary>
    /// Determines whether XY navigation is allowed for the specified input element and key device type.
    /// </summary>
    /// <param name="visual">
    /// The input element to check.
    /// </param>
    /// <param name="keyDeviceType">
    /// The type of key device (Keyboard, Gamepad, Remote, or null for programmatic).
    /// </param>
    /// <returns>
    /// True if XY navigation is allowed; otherwise, false.
    /// </returns>
    internal static bool IsAllowedXYNavigationMode(this InputElement visual, KeyDeviceType? keyDeviceType)
    {
        return IsAllowedXYNavigationMode(XYFocus.GetNavigationModes(visual), keyDeviceType);
    }

    /// <summary>
    /// Determines whether XY navigation is allowed for the specified navigation modes and key device type.
    /// </summary>
    /// <param name="modes">
    /// The XY focus navigation modes.
    /// </param>
    /// <param name="keyDeviceType">
    /// The type of key device (Keyboard, Gamepad, Remote, or null for programmatic).
    /// </param>
    /// <returns>
    /// True if XY navigation is allowed; otherwise, false.
    /// </returns>
    private static bool IsAllowedXYNavigationMode(XYFocusNavigationModes modes, KeyDeviceType? keyDeviceType)
    {
        return keyDeviceType switch
        {
            null => modes != XYFocusNavigationModes.Disabled, // programmatic input, allow any subtree except Disabled.
            KeyDeviceType.Keyboard => (modes & XYFocusNavigationModes.Keyboard) != 0,
            KeyDeviceType.Gamepad => (modes & XYFocusNavigationModes.Gamepad) != 0,
            KeyDeviceType.Remote => (modes & XYFocusNavigationModes.Remote) != 0,
            _ => throw new ArgumentOutOfRangeException(nameof(keyDeviceType), keyDeviceType, null)
        };
    }

    /// <summary>
    /// Finds the root element for XY focus search starting from the specified visual.
    /// </summary>
    /// <param name="visual">
    /// The starting input element.
    /// </param>
    /// <param name="keyDeviceType">
    /// The type of key device, or <see langword="null" /> for programmatic input.
    /// </param>
    /// <returns>
    /// The last ancestor reached by walking up from <paramref name="visual" /> through ancestors that allow XY
    /// navigation for <paramref name="keyDeviceType" />, or <paramref name="visual" /> itself when the nearest
    /// ancestor does not allow it. The result is never <see langword="null" />.
    /// </returns>
    internal static InputElement? FindXYSearchRoot(this InputElement visual, KeyDeviceType? keyDeviceType)
    {
        InputElement candidate = visual;
        InputElement? candidateParent = visual.FindAncestorOfType<InputElement>();

        while (candidateParent is not null && candidateParent.IsAllowedXYNavigationMode(keyDeviceType))
        {
            candidate = candidateParent;
            candidateParent = candidate.FindAncestorOfType<InputElement>();
        }

        return candidate;
    }
}
