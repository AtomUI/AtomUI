namespace AtomUI.Desktop.Controls;

public interface IAutoCompleteOption : IListBoxItemData
{
    string? Key { get; }
    object? Header { get; }
}