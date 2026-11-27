using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

/// <summary>
/// Fades the target in from opacity 0 to opacity 1.
/// </summary>
public class FadeInMotion : AbstractMotion
{
    public FadeInMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given duration, easing and fill mode.
    /// </summary>
    /// <param name="duration">
    /// The duration of the motion, or <see langword="null" /> to use the <c>MotionDurationMid</c> token
    /// (200 ms when the token is absent).
    /// </param>
    /// <param name="easing">
    /// The easing of the animation, or <see langword="null" /> to use linear easing.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public FadeInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// Declares an opacity channel from 0 to 1.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Opacity.From(0).To(1);
    }
}

/// <summary>
/// Fades the target out from opacity 1 to opacity 0.
/// </summary>
public class FadeOutMotion : AbstractMotion
{
    public FadeOutMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given duration, easing and fill mode.
    /// </summary>
    /// <param name="duration">
    /// The duration of the motion, or <see langword="null" /> to use the <c>MotionDurationMid</c> token
    /// (200 ms when the token is absent).
    /// </param>
    /// <param name="easing">
    /// The easing of the animation, or <see langword="null" /> to use linear easing.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public FadeOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// Declares an opacity channel from 1 to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Opacity.From(1).To(0);
    }
}
