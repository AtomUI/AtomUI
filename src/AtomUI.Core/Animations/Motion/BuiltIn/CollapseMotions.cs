using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

/// <summary>
/// Expands a target that implements <see cref="IMotionLayout" />: its layout size grows from its current size
/// to its full size while its opacity fades from the current value to the base opacity. Any other target
/// throws a <see cref="NotSupportedException" /> when the motion is played.
/// </summary>
public class ExpandMotion : AbstractMotion
{
    /// <summary>
    /// The direction given to the target's <see cref="IMotionLayout" />, or <see langword="null" /> to use the
    /// target's <see cref="Motion.LayoutDirectionProperty" /> value. The built-in <c>ContentViewport</c> animates
    /// its width for <c>Left</c> and <c>Right</c> and its height for <c>Top</c> and <c>Bottom</c>. An undefined
    /// <see cref="AtomUI.Direction" /> value throws an <see cref="ArgumentOutOfRangeException" /> when the motion
    /// is played.
    /// </summary>
    public Direction? Direction { get; set; }

    public ExpandMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given layout direction, duration, easing and fill mode.
    /// </summary>
    /// <param name="direction">
    /// The layout direction; see <see cref="Direction" />.
    /// </param>
    /// <param name="duration">
    /// The duration of the motion, or <see langword="null" /> to use the <c>MotionDurationMid</c> token
    /// (200 ms when the token is absent).
    /// </param>
    /// <param name="easing">
    /// The easing of the animation, or <see langword="null" /> to use <see cref="AbstractMotion.DefaultEasing" />.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public ExpandMotion(Direction direction, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Direction = direction;
    }

    /// <summary>
    /// The <c>MotionCurves.InOut</c> easing (ease-in-out), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOut;

    /// <summary>
    /// Declares an expanding layout segment for the resolved direction and an opacity channel from the
    /// current opacity to the base opacity.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Expand(Direction ?? context.LayoutDirection);
        builder.Opacity.FromCurrent().To(context.BaseOpacity);
    }
}

/// <summary>
/// Collapses a target that implements <see cref="IMotionLayout" />: its layout size shrinks from its current
/// size to zero while its opacity fades from the current value to fully transparent. Any other target throws
/// a <see cref="NotSupportedException" /> when the motion is played.
/// </summary>
public class CollapseMotion : AbstractMotion
{
    /// <summary>
    /// The direction given to the target's <see cref="IMotionLayout" />, or <see langword="null" /> to use the
    /// target's <see cref="Motion.LayoutDirectionProperty" /> value. The built-in <c>ContentViewport</c> animates
    /// its width for <c>Left</c> and <c>Right</c> and its height for <c>Top</c> and <c>Bottom</c>. An undefined
    /// <see cref="AtomUI.Direction" /> value throws an <see cref="ArgumentOutOfRangeException" /> when the motion
    /// is played.
    /// </summary>
    public Direction? Direction { get; set; }

    public CollapseMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given layout direction, duration, easing and fill mode.
    /// </summary>
    /// <param name="direction">
    /// The layout direction; see <see cref="Direction" />.
    /// </param>
    /// <param name="duration">
    /// The duration of the motion, or <see langword="null" /> to use the <c>MotionDurationMid</c> token
    /// (200 ms when the token is absent).
    /// </param>
    /// <param name="easing">
    /// The easing of the animation, or <see langword="null" /> to use <see cref="AbstractMotion.DefaultEasing" />.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public CollapseMotion(Direction direction, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Direction = direction;
    }

    /// <summary>
    /// The <c>MotionCurves.InOut</c> easing (ease-in-out), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOut;

    /// <summary>
    /// Declares a collapsing layout segment for the resolved direction and an opacity channel from the
    /// current opacity to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Collapse(Direction ?? context.LayoutDirection);
        builder.Opacity.FromCurrent().To(0);
    }
}
