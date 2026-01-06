using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class TreeSelectAddOnDecoratedBox : AddOnDecoratedBox
{
    public static readonly StyledProperty<bool> IsMultipleProperty =
        AvaloniaProperty.Register<TreeSelectAddOnDecoratedBox, bool>(
            nameof(IsMultiple));
    
    public static readonly DirectProperty<TreeSelectAddOnDecoratedBox, bool> IsSelectionEmptyProperty =
        AvaloniaProperty.RegisterDirect<TreeSelectAddOnDecoratedBox, bool>(
            nameof(IsSelectionEmpty),
            o => o.IsSelectionEmpty,
            (o, v) => o.IsSelectionEmpty = v);
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<TreeSelectAddOnDecoratedBox, bool>(nameof(IsDropDownOpen));
    
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