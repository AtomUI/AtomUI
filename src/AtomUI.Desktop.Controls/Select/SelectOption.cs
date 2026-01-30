using Avalonia;

namespace AtomUI.Desktop.Controls;

public interface ISelectOption : IListBoxItemData
{
    object? Header { get; }
    string? Group { get; }
    bool IsDynamicAdded { get; }
}

public class SelectOption : ListBoxItemData, ISelectOption, IGroupListItemData
{
    public static readonly DirectProperty<SelectOption, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<SelectOption, object?>(nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);
    
    private object? _header;

    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }
    
    public bool IsDynamicAdded { get; init; } = false;
    
    public string? Group { get; init; }

    bool IGroupListItemData.IsGroupItem { get; set; } = false;
}
