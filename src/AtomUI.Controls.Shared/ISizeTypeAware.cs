using Avalonia;

namespace AtomUI.Controls;

/// <summary>
/// Implemented by controls whose size follows the fixed presets <c>Large</c>, <c>Middle</c> and <c>Small</c> of
/// <c>SizeType</c>. A form item relays its own size type to the controls it contains, with <c>Custom</c> arriving
/// as <c>Middle</c>. Themes select size-dependent values with selectors such as <c>[SizeType=Large]</c>.
/// </summary>
public interface ISizeTypeAware
{
    /// <summary>
    /// The preset size of the control. The theme derives paddings, line heights and minimum heights from it.
    /// </summary>
    public SizeType SizeType { get; set; }
}

/// <summary>
/// Implemented by controls whose size follows the fixed presets <c>Large</c>, <c>Middle</c> and <c>Small</c> of
/// <c>CustomizableSizeType</c>, or that are sized by the values set on them with <c>Custom</c>. A form item
/// relays its own size type to the controls it contains, and themes select size-dependent values with selectors
/// such as <c>[SizeType=Large]</c>.
/// </summary>
public interface ICustomizableSizeTypeAware
{
    /// <summary>
    /// The preset size of the control, or <c>Custom</c> to apply no preset. With <c>Custom</c> the control takes its
    /// size from the values set on it or from a fallback that differs per control.
    /// </summary>
    public CustomizableSizeType SizeType { get; set; }
}

/// <summary>
/// Holds the shared registration of the <see cref="ISizeTypeAware.SizeType" /> property.
/// </summary>
public abstract class SizeTypeControlProperty
{
    /// <summary>
    /// The registered name of <see cref="SizeTypeProperty" />.
    /// </summary>
    public const string SizeTypePropertyName = "SizeType";
    /// <summary>
    /// The preset size shared by the controls that implement <see cref="ISizeTypeAware" />. A control adds itself
    /// as owner with <c>AddOwner</c> and exposes the value through a <c>SizeType</c> property of its own. The value
    /// does not inherit and defaults to <c>Middle</c>. Themes read it through <c>[SizeType=Large]</c> selectors.
    /// </summary>
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        AvaloniaProperty.Register<StyledElement, SizeType>(SizeTypePropertyName, SizeType.Middle);
}

/// <summary>
/// Holds the shared registration of the <see cref="ICustomizableSizeTypeAware.SizeType" /> property.
/// </summary>
public abstract class CustomizableSizeTypeControlProperty
{
    /// <summary>
    /// The registered name of <see cref="SizeTypeProperty" />.
    /// </summary>
    public const string SizeTypePropertyName = "SizeType";
    /// <summary>
    /// The preset size shared by the controls that implement <see cref="ICustomizableSizeTypeAware" />. A control
    /// adds itself as owner with <c>AddOwner</c> and exposes the value through a <c>SizeType</c> property of its own.
    /// The value does not inherit and defaults to <c>Middle</c>. Themes read it through <c>[SizeType=Large]</c>
    /// selectors.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        AvaloniaProperty.Register<StyledElement, CustomizableSizeType>(SizeTypePropertyName, CustomizableSizeType.Middle);
}
