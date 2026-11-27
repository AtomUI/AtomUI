using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the gradient that a gradient color picker reports through its ValueSelected event.
/// </summary>
public class GradientColorSelectedEventArgs : EventArgs
{
    /// <summary>
    /// The selected gradient, or <see langword="null" /> when there is none.
    /// </summary>
    public LinearGradientBrush? Value { get; }

    /// <summary>
    /// Creates the arguments that report <paramref name="value" /> as the selected gradient.
    /// </summary>
    public GradientColorSelectedEventArgs(LinearGradientBrush value)
    {
        Value = value;
    }
}