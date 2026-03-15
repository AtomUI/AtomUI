using AtomUI.Controls.Data;

namespace AtomUI.Desktop.Controls;

public interface IAutoCompleteOption : IListItemData
{
    object? Header { get; }
}