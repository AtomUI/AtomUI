using Avalonia;

namespace AtomUI.Desktop.Controls;

public interface IListBoxItemData
{
    bool IsEnabled { get; set; }
    bool IsSelected { get; set; }
    object? Content { get; set; }
}

public class ListBoxItemData : AvaloniaObject, IListBoxItemData
{
    public static readonly DirectProperty<ListBoxItemData, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItemData, bool>(nameof(IsEnabled),
            o => o.IsEnabled,
            (o, v) => o.IsEnabled = v);
    
    public static readonly DirectProperty<ListBoxItemData, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItemData, bool>(nameof(IsSelected),
            o => o.IsSelected,
            (o, v) => o.IsSelected = v);
    
    public static readonly DirectProperty<ListBoxItemData, object?> ContentProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItemData, object?>(nameof(Content),
            o => o.Content,
            (o, v) => o.Content = v);
    
    private bool _isEnabled = true;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetAndRaise(IsEnabledProperty, ref _isEnabled, value);
    }
    
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
    }
    
    private object? _content;

    public object? Content
    {
        get => _content;
        set => SetAndRaise(ContentProperty, ref _content, value);
    }

}
