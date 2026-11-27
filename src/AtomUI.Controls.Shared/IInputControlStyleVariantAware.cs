using Avalonia;

namespace AtomUI.Controls;

/// <summary>
/// An input control that exposes the style of its frame as a <see cref="StyleVariant" /> property.
/// </summary>
public interface IInputControlStyleVariantAware
{
    /// <summary>
    /// The style of the input frame. The theme of each control decides which of the
    /// <see cref="InputControlStyleVariant" /> values it paints differently. The default is
    /// <see cref="InputControlStyleVariant.Outlined" />.
    /// </summary>
    InputControlStyleVariant StyleVariant { get; set; }
}

/// <summary>
/// Registers the <see cref="StyleVariantProperty" /> property that input controls share.
/// </summary>
public abstract class InputControlStyleVariantProperty
{
    /// <summary>
    /// The registered name of <see cref="StyleVariantProperty" />.
    /// </summary>
    public const string StyleVariantPropertyName = "StyleVariant";
    /// <summary>
    /// The frame style of an input control, shared by the input controls. An input control adds itself as owner with
    /// <c>AddOwner</c> and exposes the value through a <c>StyleVariant</c> property of its own. The value does not
    /// inherit; <c>FormItem</c> and <c>FormItemDecorator</c> relay their own value to a contained input control.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        AvaloniaProperty.Register<StyledElement, InputControlStyleVariant>(StyleVariantPropertyName, InputControlStyleVariant.Outlined);
}
