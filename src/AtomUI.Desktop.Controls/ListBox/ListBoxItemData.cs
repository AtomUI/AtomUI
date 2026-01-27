using Avalonia;

namespace AtomUI.Desktop.Controls;

public interface IListBoxItemData
{
    bool IsEnabled { get; set; }
    bool IsSelected { get; set; }
    object? Value { get; set; }
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
    
    public static readonly DirectProperty<ListBoxItemData, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<ListBoxItemData, object?>(nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v);
    
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
    
    private object? _value;

    public object? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

}
