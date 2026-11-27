using Avalonia;

namespace AtomUI.Controls;

/// <summary>
/// An input control that exposes its validation state as a <see cref="Status" /> property.
/// </summary>
public interface IInputControlStatusAware
{
    /// <summary>
    /// The validation state of the input frame. <see cref="InputControlStatus.Warning" /> and
    /// <see cref="InputControlStatus.Error" /> make the theme paint the frame in that state, and
    /// <see cref="InputControlStatus.Default" /> leaves the frame without a validation state.
    /// </summary>
    InputControlStatus Status { get; set; }
}

/// <summary>
/// Registers the <see cref="StatusProperty" /> property that input controls share.
/// </summary>
public abstract class InputControlStatusProperty
{
    /// <summary>
    /// The registered name of <see cref="StatusProperty" />.
    /// </summary>
    public const string StatusPropertyName = "Status";
    /// <summary>
    /// The validation state of an input control, shared by the input controls. An input control adds itself as owner
    /// with <c>AddOwner</c> and exposes the value through a <c>Status</c> property of its own. The value does not
    /// inherit and defaults to <see cref="InputControlStatus.Default" />.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        AvaloniaProperty.Register<StyledElement, InputControlStatus>(StatusPropertyName, InputControlStatus.Default);
}
