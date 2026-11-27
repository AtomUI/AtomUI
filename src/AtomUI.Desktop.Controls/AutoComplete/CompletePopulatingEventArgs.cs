using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data of the <see cref="AbstractAutoComplete.Populating" /> event. A handler that sets
/// <see cref="CancelEventArgs.Cancel" /> takes the population over: the control then neither matches the
/// options nor raises <see cref="AbstractAutoComplete.Populated" />.
/// </summary>
public class CompletePopulatingEventArgs : CancelEventArgs
{
    /// <summary>
    /// The text that the options are matched against: the value of the input when the population starts.
    /// </summary>
    public string? Predicate { get; }

    /// <summary>
    /// Creates the arguments for the <see cref="AbstractAutoComplete.Populating" /> event.
    /// </summary>
    /// <param name="predicate">
    /// The text the options are matched against; stored as <see cref="Predicate" />.
    /// </param>
    public CompletePopulatingEventArgs(string? predicate)
    {
        Predicate = predicate;
    }
}