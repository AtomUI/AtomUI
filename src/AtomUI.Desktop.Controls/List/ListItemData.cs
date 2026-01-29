using Avalonia;

namespace AtomUI.Desktop.Controls;

public interface IListItemData : IListBoxItemData
{
    string? Group { get; }
    object? Content { get; }
}

internal interface IGroupListItemData
{
    bool IsGroupItem { get; set; }
}

public class ListItemData : ListBoxItemData, IListItemData, IGroupListItemData
{
    public static readonly DirectProperty<ListItemData, object?> ContentProperty =
        AvaloniaProperty.RegisterDirect<ListItemData, object?>(nameof(Content),
            o => o.Content,
            (o, v) => o.Content = v);
    
    private object? _content;

    public object? Content
    {
        get => _content;
        set => SetAndRaise(ContentProperty, ref _content, value);
    }
    
    public string? Group { get; init; }

    bool IGroupListItemData.IsGroupItem { get; set; } = false;
}
