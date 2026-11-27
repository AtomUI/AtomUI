using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the color before and after a color value of the color picker or one of its parts changed.
/// </summary>
public class ColorChangedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the arguments for a change from <paramref name="oldColor" /> to <paramref name="newColor" />.
    /// </summary>
    public ColorChangedEventArgs(Color? oldColor, Color? newColor)
    {
        OldColor = oldColor;
        NewColor = newColor;
    }

    /// <summary>
    /// The color before the change, or <see langword="null" /> when there was none.
    /// </summary>
    public Color? OldColor { get; private set; }

    /// <summary>
    /// The color after the change, or <see langword="null" /> when the value is empty.
    /// </summary>
    public Color? NewColor { get; private set; }
}
