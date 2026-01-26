using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class CompleteValueChangedEventArgs : RoutedEventArgs
{
    public string? Value { get; }

    public CompleteValueChangedEventArgs(string? value, RoutedEvent? routedEvent)
        : base(routedEvent)
    {
        Value = value;
    }
}