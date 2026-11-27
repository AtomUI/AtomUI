using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the color that a color picker reports through its ValueSelected event.
/// </summary>
public class ColorSelectedEventArgs : EventArgs
{
    /// <summary>
    /// The selected color.
    /// </summary>
    public Color Value { get; }

    /// <summary>
    /// Creates the arguments that report <paramref name="value" /> as the selected color.
    /// </summary>
    public ColorSelectedEventArgs(Color value)
    {
        Value = value;
    }
}