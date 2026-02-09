namespace AtomUI.Desktop.Controls;

public record AutoCompleteOption : IAutoCompleteOption
{
    public object? Header { get; set; }

    public bool IsEnabled { get; set; } = true;
    
    public object? Value { get; set; }

    public bool IsSelected { get; set; }
    
    public string? Key { get; init; }
}
