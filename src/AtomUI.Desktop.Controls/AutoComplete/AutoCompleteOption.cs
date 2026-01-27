using Avalonia;

namespace AtomUI.Desktop.Controls;

public class AutoCompleteOption : AvaloniaObject, IAutoCompleteOption
{
    public static readonly DirectProperty<AutoCompleteOption, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<AutoCompleteOption, object?>(nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);
    
    public static readonly DirectProperty<AutoCompleteOption, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<AutoCompleteOption, bool>(nameof(IsEnabled),
            o => o.IsEnabled,
            (o, v) => o.IsEnabled = v);
    
    public static readonly DirectProperty<AutoCompleteOption, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<AutoCompleteOption, object?>(nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v);
    
    public static readonly DirectProperty<AutoCompleteOption, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<AutoCompleteOption, bool>(nameof(IsSelected),
            o => o.IsSelected,
            (o, v) => o.IsSelected = v);
    
    private object? _header;

    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }
    
    private bool _isEnabled = true;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetAndRaise(IsEnabledProperty, ref _isEnabled, value);
    }
    
    private object? _value = true;

    public object? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }
    
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
    }
    
    public string? Key { get; init; }
}
