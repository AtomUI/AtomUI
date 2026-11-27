using Avalonia;

namespace AtomUI.Controls;

/// <summary>
/// Exposes the motion switch of a control. A host that finds this interface on a child relays its own
/// <see cref="IsMotionEnabled" /> to the child. A list control switches motion off on a container while it
/// clears or restores the state of that container.
/// </summary>
public interface IMotionAwareControl
{
    /// <summary>
    /// Gets whether the control animates its state changes. While it is <see langword="false" />, a state change
    /// takes effect at once, without a transition.
    /// </summary>
    bool IsMotionEnabled { get; }
}

/// <summary>
/// Holds the styled properties that motion-aware controls share. A control takes part with <c>AddOwner</c> on
/// <see cref="IsMotionEnabledProperty" /> and, when a motion of the control takes time, on
/// <see cref="MotionDurationProperty" />.
/// </summary>
public abstract class MotionAwareControlProperty
{
    /// <summary>
    /// The name under which <see cref="IsMotionEnabledProperty" /> is registered.
    /// </summary>
    public const string IsMotionEnabledPropertyName = "IsMotionEnabled";
    /// <summary>
    /// The name under which <see cref="MotionDurationProperty" /> is registered.
    /// </summary>
    public const string MotionDurationPropertyName = "MotionDuration";

    /// <summary>
    /// Identifies the <c>IsMotionEnabled</c> property that controls implementing
    /// <see cref="IMotionAwareControl" /> share. A value of <see langword="true" /> lets the control animate
    /// its state changes. The value is set on the control itself and is not inherited: control themes take it
    /// from the <c>EnableMotion</c> token, and a host that wants to pass its value down relays it to the child.
    /// Without a value from a style or the control, it is <see langword="false" />.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        AvaloniaProperty.Register<StyledElement, bool>(IsMotionEnabledPropertyName);
    
    /// <summary>
    /// Identifies the <c>MotionDuration</c> property, the time one motion of the owning control takes; each
    /// owner uses it for its own motion, such as opening and closing or fading in and out. The value is set on
    /// the control itself and is not inherited; control themes take it from a duration token of the control.
    /// Without a value from a style or the control, it is 200 milliseconds.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty = 
        AvaloniaProperty.Register<StyledElement, TimeSpan>(MotionDurationPropertyName, TimeSpan.FromMilliseconds(200));
}