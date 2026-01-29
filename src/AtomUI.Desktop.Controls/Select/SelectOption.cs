using Avalonia;

namespace AtomUI.Desktop.Controls;

public interface ISelectOption
{
    object? Header { get; }
    bool IsEnabled { get; }
    object? Value { get; }
    string? Group { get; }
    bool IsDynamicAdded { get; }
}

public class SelectOption : AvaloniaObject, ISelectOption
{
    public static readonly DirectProperty<SelectOption, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<SelectOption, object?>(nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);
    
    public static readonly DirectProperty<SelectOption, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<SelectOption, bool>(nameof(IsEnabled),
            o => o.IsEnabled,
            (o, v) => o.IsEnabled = v);
    
    public static readonly DirectProperty<SelectOption, object?> ValueProperty =
        AvaloniaProperty.RegisterDirect<SelectOption, object?>(nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v);
    
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
    
    public string? Group { get; init; }
    public bool IsDynamicAdded { get; init; } = false;
}
