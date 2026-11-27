using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the gradient before and after a gradient value of the gradient color picker or of one of its parts
/// changed.
/// </summary>
public class GradientColorChangedEventArgs : EventArgs
{
    /// <summary>
    /// Creates the arguments for a change from <paramref name="oldColor" /> to <paramref name="newColor" />.
    /// </summary>
    public GradientColorChangedEventArgs(LinearGradientBrush? oldColor, LinearGradientBrush? newColor)
    {
        OldColor = oldColor;
        NewColor = newColor;
    }

    /// <summary>
    /// The gradient before the change, or <see langword="null" /> when there was none.
    /// </summary>
    public LinearGradientBrush? OldColor { get; private set; }

    /// <summary>
    /// The gradient after the change, or <see langword="null" /> when the value is empty.
    /// </summary>
    public LinearGradientBrush? NewColor { get; private set; }
}