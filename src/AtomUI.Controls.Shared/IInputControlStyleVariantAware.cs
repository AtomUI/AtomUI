using Avalonia;

namespace AtomUI.Controls;

public interface IInputControlStyleVariantAware
{
    InputControlStyleVariant StyleVariant { get; set; }
}

public abstract class InputControlStyleVariantProperty
{
    public const string StyleVariantPropertyName = "StyleVariant";
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        AvaloniaProperty.Register<StyledElement, InputControlStyleVariant>(StyleVariantPropertyName, InputControlStyleVariant.Outline);
}
