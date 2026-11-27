using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

/// <summary>
/// Enters by growing the target's height from 80% to 100% while its top edge stays in place, and by fading in
/// from opacity 0 to 1. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class SlideUpInMotion : AbstractMotion
{
    public SlideUpInMotion()
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
    /// The easing of the animation, or <see langword="null" /> to use <see cref="DefaultEasing" />.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public SlideUpInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.OutQuint</c> easing (ease-out quintic), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutQuint;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.ScaleY" /> channel from 0.8 to 1, a top-left transform origin
    /// and an opacity channel from 0 to 1.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.ScaleY.From(.8).To(1);
        builder.Opacity.From(0).To(1);
    }
}

/// <summary>
/// Leaves by shrinking the target's height from 100% to 80% while its top edge stays in place, and by fading out
/// from opacity 1 to 0. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class SlideUpOutMotion : AbstractMotion
{
    public SlideUpOutMotion()
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
    /// The easing of the animation, or <see langword="null" /> to use <see cref="DefaultEasing" />.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public SlideUpOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.InQuint</c> easing (ease-in quintic), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InQuint;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.ScaleY" /> channel from 1 to 0.8, a top-left transform origin
    /// and an opacity channel from 1 to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.ScaleY.From(1).To(.8);
        builder.Opacity.From(1).To(0);
    }
}

/// <summary>
/// Enters by growing the target's height from 80% to 100% while its bottom edge stays in place, and by fading in
/// from opacity 0 to 1. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class SlideDownInMotion : AbstractMotion
{
    public SlideDownInMotion()
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
    /// The easing of the animation, or <see langword="null" /> to use <see cref="DefaultEasing" />.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public SlideDownInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.OutQuint</c> easing (ease-out quintic), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutQuint;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.ScaleY" /> channel from 0.8 to 1, a bottom-right transform origin
    /// and an opacity channel from 0 to 1.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 1, RelativeUnit.Relative);
        builder.ScaleY.From(.8).To(1);
        builder.Opacity.From(0).To(1);
    }
}

/// <summary>
/// Leaves by shrinking the target's height from 100% to 80% while its bottom edge stays in place, and by fading out
/// from opacity 1 to 0. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class SlideDownOutMotion : AbstractMotion
{
    public SlideDownOutMotion()
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
    /// The easing of the animation, or <see langword="null" /> to use <see cref="DefaultEasing" />.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public SlideDownOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.InQuint</c> easing (ease-in quintic), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InQuint;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.ScaleY" /> channel from 1 to 0.8, a bottom-right transform origin
    /// and an opacity channel from 1 to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 1, RelativeUnit.Relative);
        builder.ScaleY.From(1).To(.8);
        builder.Opacity.From(1).To(0);
    }
}

/// <summary>
/// Enters by growing the target's width from 80% to 100% while its left edge stays in place, and by fading in
/// from opacity 0 to 1. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class SlideLeftInMotion : AbstractMotion
{
    public SlideLeftInMotion()
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
    /// The easing of the animation, or <see langword="null" /> to use <see cref="DefaultEasing" />.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public SlideLeftInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.OutQuint</c> easing (ease-out quintic), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutQuint;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.ScaleX" /> channel from 0.8 to 1, a top-left transform origin
    /// and an opacity channel from 0 to 1.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.ScaleX.From(.8).To(1);
        builder.Opacity.From(0).To(1);
    }
}

/// <summary>
/// Leaves by shrinking the target's width from 100% to 80% while its left edge stays in place, and by fading out
/// from opacity 1 to 0. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class SlideLeftOutMotion : AbstractMotion
{
    public SlideLeftOutMotion()
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
    /// The easing of the animation, or <see langword="null" /> to use <see cref="DefaultEasing" />.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public SlideLeftOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.InQuint</c> easing (ease-in quintic), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InQuint;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.ScaleX" /> channel from 1 to 0.8, a top-left transform origin
    /// and an opacity channel from 1 to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.ScaleX.From(1).To(.8);
        builder.Opacity.From(1).To(0);
    }
}

/// <summary>
/// Enters by growing the target's width from 80% to 100% while its right edge stays in place, and by fading in
/// from opacity 0 to 1. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class SlideRightInMotion : AbstractMotion
{
    public SlideRightInMotion()
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
    /// The easing of the animation, or <see langword="null" /> to use <see cref="DefaultEasing" />.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public SlideRightInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.OutQuint</c> easing (ease-out quintic), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutQuint;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.ScaleX" /> channel from 0.8 to 1, a top-right transform origin
    /// and an opacity channel from 0 to 1.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 0, RelativeUnit.Relative);
        builder.ScaleX.From(.8).To(1);
        builder.Opacity.From(0).To(1);
    }
}

/// <summary>
/// Leaves by shrinking the target's width from 100% to 80% while its right edge stays in place, and by fading out
/// from opacity 1 to 0. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class SlideRightOutMotion : AbstractMotion
{
    public SlideRightOutMotion()
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
    /// The easing of the animation, or <see langword="null" /> to use <see cref="DefaultEasing" />.
    /// </param>
    /// <param name="fillMode">
    /// The fill mode of the animation. The default applies the start values during the delay
    /// and keeps the end values after the animation.
    /// </param>
    public SlideRightOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.InQuint</c> easing (ease-in quintic), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InQuint;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.ScaleX" /> channel from 1 to 0.8, a top-right transform origin
    /// and an opacity channel from 1 to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 0, RelativeUnit.Relative);
        builder.ScaleX.From(1).To(.8);
        builder.Opacity.From(1).To(0);
    }
}
