namespace AtomUI.Desktop.Controls;

public interface IAutoCompleteOption
{
    string? Key { get; }
    object? Header { get; }
    bool IsEnabled { get; }
    object? Value { get; }
}