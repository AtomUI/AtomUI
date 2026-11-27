using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

/// <summary>
/// Provides press compression and spring release when the target's motion surface is available.
/// </summary>
public sealed class PressScaleInteractionEffect : InteractionEffect
{
    /// <summary>
    /// Shared default instance of the press-scale effect.
    /// </summary>
    public static PressScaleInteractionEffect Default { get; } = new();

    /// <inheritdoc />
    public override IInteractionEffectSession OnPressStarted(in InteractionEffectStartContext context) =>
        new Session(context.Playback, context.Playback.PlayOnSurface(ScaleMotion.Press));

    private sealed class Session(IInteractionEffectPlayback playback, IDisposable lease) : IInteractionEffectSession
    {
        public void Complete(bool activated) => playback.PlayOnSurface(ScaleMotion.Release);
        public void Cancel() => lease.Dispose();
        public void Dispose() => lease.Dispose();
    }

    private sealed class ScaleMotion(double scale, TimeSpan duration, Easing easing) : AbstractMotion(duration, easing)
    {
        internal static readonly ScaleMotion Press = new(0.97, TimeSpan.FromMilliseconds(100), new CubicEaseOut());
        internal static readonly ScaleMotion Release = new(1, TimeSpan.FromMilliseconds(220), new SpringEasing(1, 120, 24, 0));

        protected override void Build(MotionBuilder builder, in MotionContext context) => builder.Scale.FromCurrent().To(scale);
    }
}
