using AtomUI.Controls;
using Avalonia;

namespace AtomUI.Desktop.Controls;

internal class SearchButton : Button,
                              IInputControlStatusAware,
                              IInputControlStyleVariantAware
{
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<SearchButton>();
    
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<SearchButton>();
    
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }
    
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
}