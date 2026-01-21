using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class CascaderAddOnDecoratedBox : AddOnDecoratedBox
{
    public static readonly StyledProperty<bool> IsMultipleProperty =
        AvaloniaProperty.Register<CascaderAddOnDecoratedBox, bool>(
            nameof(IsMultiple));
    
    public static readonly DirectProperty<CascaderAddOnDecoratedBox, bool> IsSelectionEmptyProperty =
        AvaloniaProperty.RegisterDirect<CascaderAddOnDecoratedBox, bool>(
            nameof(IsSelectionEmpty),
            o => o.IsSelectionEmpty,
            (o, v) => o.IsSelectionEmpty = v);
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<CascaderAddOnDecoratedBox, bool>(nameof(IsDropDownOpen));
    
    public bool IsMultiple
    {
        get => GetValue(IsMultipleProperty);
        set => SetValue(IsMultipleProperty, value);
    }
    
    private bool _isSelectionEmpty = true;

    public bool IsSelectionEmpty
    {
        get => _isSelectionEmpty;
        set => SetAndRaise(IsSelectionEmptyProperty, ref _isSelectionEmpty, value);
    }
    
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
}