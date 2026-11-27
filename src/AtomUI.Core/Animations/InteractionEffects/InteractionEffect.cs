using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Animations;

/// <summary>
/// Defines transient presentation for a control's accepted input gesture.
/// </summary>
public abstract class InteractionEffect
{
    /// <summary>
    /// Explicitly disables feedback and stops fallback to another definition.
    /// </summary>
    public static InteractionEffect None { get; } = new NoEffect();

    /// <summary>
    /// An application-supplied effect. A null value uses the scoped default.
    /// </summary>
    public static readonly AttachedProperty<InteractionEffect?> EffectProperty =
        AvaloniaProperty.RegisterAttached<InteractionEffect, Control, InteractionEffect?>("Effect", inherits: true);

    /// <summary>
    /// Gets the attached <see cref="InteractionEffect" /> override on <paramref name="target" />.
    /// </summary>
    public static InteractionEffect? GetEffect(Control target) => target.GetValue(EffectProperty);

    /// <summary>
    /// Sets the attached <see cref="InteractionEffect" /> override on <paramref name="target" />.
    /// </summary>
    public static void SetEffect(Control target, InteractionEffect? value) => target.SetValue(EffectProperty, value);

    /// <summary>
    /// Starts press feedback for an input gesture, returning a session to complete or cancel, or
    /// <see langword="null" /> when no press-stage feedback is used.
    /// </summary>
    public virtual IInteractionEffectSession? OnPressStarted(in InteractionEffectStartContext context) => null;

    /// <summary>
    /// Plays activation feedback after a control commits its action.
    /// </summary>
    public virtual void OnActivated(in InteractionEffectActivateContext context) { }

    private sealed class NoEffect : InteractionEffect;
}

/// <summary>
/// A press presentation that can either complete once or be revoked by its host.
/// </summary>
public interface IInteractionEffectSession : IDisposable
{
    /// <summary>
    /// Finishes the active press session, indicating whether the gesture resulted in activation.
    /// </summary>
    void Complete(bool activated);

    /// <summary>
    /// Immediately cancels and tears down the press session.
    /// </summary>
    void Cancel();
}

/// <summary>
/// The host-owned playback services for one input operation.
/// </summary>
public interface IInteractionEffectPlayback
{
    /// <summary>
    /// Plays a transient decoration visual in the interaction adorner layer and returns a lease for exit or cleanup.
    /// </summary>
    IDecorationEffectLease PlayDecoration(Func<Control> createVisual, AbstractMotion motion, InteractionEffectClip clip);

    /// <summary>
    /// Plays a motion directly on the target control surface and returns a lease that restores the surface on disposal.
    /// </summary>
    IDisposable PlayOnSurface(AbstractMotion motion);
}

/// <summary>
/// A decoration that can transfer its original visual into a single exit animation.
/// </summary>
public interface IDecorationEffectLease : IDisposable
{
    /// <summary>
    /// Transitions the still-active decoration visual into a single exit motion, returning <see langword="false" />
    /// if the lease has already exited or been disposed.
    /// </summary>
    bool TryBeginExit(AbstractMotion exit);
}

/// <summary>
/// Clipping boundary applied to a decoration interaction effect visual.
/// </summary>
public enum InteractionEffectClip
{
    /// <summary>
    /// Clips the decoration strictly to the host control's rounded shape.
    /// </summary>
    InsideShape,

    /// <summary>
    /// Allows the decoration to expand beyond the host bounds while remaining clipped to ancestor scroll viewports.
    /// </summary>
    OutsideShape
}

/// <summary>
/// Input modality that triggered an interaction effect.
/// </summary>
public enum InteractionEffectInputSource
{
    /// <summary>
    /// Triggered by a pointer press or click.
    /// </summary>
    Pointer,

    /// <summary>
    /// Triggered by a keyboard activation.
    /// </summary>
    Keyboard,

    /// <summary>
    /// Triggered programmatically or via automation.
    /// </summary>
    Automation
}

/// <summary>
/// The presentation inputs captured at the start of the press.
/// </summary>
public readonly record struct InteractionEffectStartContext(IInteractionEffectPlayback Playback, long OperationId,
    InteractionEffectInputSource InputSource, Point? PointerOrigin, Size Size, CornerRadius CornerRadius,
    double RenderScaling, IBrush? Brush);

/// <summary>
/// The presentation inputs of a committed activation.
/// </summary>
public readonly record struct InteractionEffectActivateContext(IInteractionEffectPlayback Playback, long OperationId,
    InteractionEffectInputSource InputSource, Point? PointerOrigin, Size Size, CornerRadius CornerRadius,
    double RenderScaling, IBrush? Brush, bool HadPress);
