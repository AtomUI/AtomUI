using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class SelectOptionItem : ListItem
{
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<SelectOptionItem, bool>(nameof(IsActive));
    
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
}
