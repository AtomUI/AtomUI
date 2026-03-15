using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public record AutoCompleteOption : IAutoCompleteOption
{
    public object? Header { get; set; }
    public string? Group { get; set; }

    public bool IsEnabled { get; set; } = true;
    
    public object? Content { get; set; }

    public bool IsSelected { get; set; }

    public EntityKey? ItemKey { get; init; }
}
