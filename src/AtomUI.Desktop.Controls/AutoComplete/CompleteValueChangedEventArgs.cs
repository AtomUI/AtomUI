using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data of the <see cref="AbstractAutoComplete.ValueChanged" /> event.
/// </summary>
public class CompleteValueChangedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// The value of the input after it was updated by typing, by selecting an option or by setting the property.
    /// </summary>
    public string? Value { get; }

    /// <summary>
    /// Creates the arguments for the <see cref="AbstractAutoComplete.ValueChanged" /> event.
    /// </summary>
    /// <param name="value">
    /// The new value of the input; stored as <see cref="Value" />.
    /// </param>
    /// <param name="routedEvent">
    /// The identifier of the routed event; passed to <see cref="RoutedEventArgs" />.
    /// </param>
    public CompleteValueChangedEventArgs(string? value, RoutedEvent? routedEvent)
        : base(routedEvent)
    {
        Value = value;
    }
}