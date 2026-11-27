using AtomUI.Animations;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Controls;

/// <summary>
/// Implemented by controls that can play transient interaction feedback when activated.
/// </summary>
public interface IInteractionEffectAwareControl : IMotionAwareControl
{
    /// <summary>
    /// Gets whether interaction feedback is enabled on this control.
    /// </summary>
    bool IsInteractionEffectEnabled { get; }
}

/// <summary>
/// Defines the shared <see cref="IsInteractionEffectEnabledProperty" /> owned by interaction-effect-aware controls.
/// </summary>
public abstract class InteractionEffectAwareControlProperty : MotionAwareControlProperty
{
    /// <summary>
    /// Name of the <see cref="IsInteractionEffectEnabledProperty" /> styled property.
    /// </summary>
    public const string IsInteractionEffectEnabledPropertyName = "IsInteractionEffectEnabled";

    /// <summary>
    /// Identifies the shared <see cref="IInteractionEffectAwareControl.IsInteractionEffectEnabled" /> styled property.
    /// </summary>
    public static readonly StyledProperty<bool> IsInteractionEffectEnabledProperty =
        AvaloniaProperty.Register<StyledElement, bool>(IsInteractionEffectEnabledPropertyName);

    internal static bool IsEnabled(Control target) => target.IsSet(IsInteractionEffectEnabledProperty)
        ? target.GetValue(IsInteractionEffectEnabledProperty)
        : MotionPreparation.ReadToken(target, SharedTokenKind.EnableInteractionEffect) is true;
}
