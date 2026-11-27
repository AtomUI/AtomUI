using Avalonia.Interactivity;

// Retain the Desktop CLR names for existing consumers.
namespace AtomUI.Desktop.Controls;

/// <summary>
/// What started a search in a search input.
/// </summary>
public enum SearchTriggerSource
{
    /// <summary>
    /// The search button was clicked.
    /// </summary>
    Button,
    /// <summary>
    /// The Enter key was pressed in the input.
    /// </summary>
    EnterKey
}

/// <summary>
/// Describes a search requested from a search input.
/// </summary>
public sealed class SearchRequestedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Creates the event arguments.
    /// </summary>
    /// <param name="routedEvent">
    /// The routed event to raise.
    /// </param>
    /// <param name="source">
    /// The control that raised the event.
    /// </param>
    /// <param name="query">
    /// The text to search for.
    /// </param>
    /// <param name="trigger">
    /// What started the search.
    /// </param>
    public SearchRequestedEventArgs(
        RoutedEvent routedEvent,
        object? source,
        string query,
        SearchTriggerSource trigger)
        : base(routedEvent, source)
    {
        Query   = query;
        Trigger = trigger;
    }

    /// <summary>
    /// The text to search for; an empty string when the input is empty.
    /// </summary>
    public string Query { get; }

    /// <summary>
    /// What started the search: the search button or the Enter key.
    /// </summary>
    public SearchTriggerSource Trigger { get; }
}
