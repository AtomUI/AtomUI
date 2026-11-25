using Avalonia.Interactivity;

// Retain the Desktop CLR names for existing consumers.
namespace AtomUI.Desktop.Controls;

public enum SearchTriggerSource
{
    Button,
    EnterKey
}

public sealed class SearchRequestedEventArgs : RoutedEventArgs
{
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

    public string Query { get; }

    public SearchTriggerSource Trigger { get; }
}
