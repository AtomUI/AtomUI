using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

/// <summary>
/// Enters by scaling the target uniformly from 20% to 100% about its center while fading in from
/// opacity 0 to 1. An explicit <see cref="AbstractMotion.Origin" /> replaces the center.
/// </summary>
public class ZoomInMotion : AbstractMotion
{
    public ZoomInMotion()
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
    public ZoomInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.OutCirc</c> easing (ease-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 0.2 to 1, a center transform origin
    /// and an opacity channel from 0 to 1.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        builder.Scale.From(0.2).To(1);
        builder.Opacity.From(0).To(1);
    }
}

/// <summary>
/// Leaves by scaling the target uniformly from 100% to 20% about its center while fading out from
/// the current opacity to 0. An explicit <see cref="AbstractMotion.Origin" /> replaces the center.
/// </summary>
public class ZoomOutMotion : AbstractMotion
{
    public ZoomOutMotion()
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
    public ZoomOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.InOutCirc</c> easing (ease-in-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 1 to 0.2, a center transform origin
    /// and an opacity channel from the current opacity to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.2);
        builder.Opacity.FromCurrent().To(0);
    }
}

/// <summary>
/// Enters by scaling the target uniformly from 80% to 100% about its center while fading in from
/// opacity 0 to 1. An explicit <see cref="AbstractMotion.Origin" /> replaces the center.
/// </summary>
public class ZoomBigInMotion : AbstractMotion
{
    public ZoomBigInMotion()
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
    public ZoomBigInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.OutCirc</c> easing (ease-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 0.8 to 1, a center transform origin
    /// and an opacity channel from 0 to 1.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        builder.Scale.From(0.8).To(1);
        builder.Opacity.From(0).To(1);
    }
}

/// <summary>
/// Leaves by scaling the target uniformly from 100% to 80% about its center while fading out from
/// the current opacity to 0. An explicit <see cref="AbstractMotion.Origin" /> replaces the center.
/// </summary>
public class ZoomBigOutMotion : AbstractMotion
{
    public ZoomBigOutMotion()
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
    public ZoomBigOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.InOutCirc</c> easing (ease-in-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 1 to 0.8, a center transform origin
    /// and an opacity channel from the current opacity to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.8);
        builder.Opacity.FromCurrent().To(0);
    }
}

/// <summary>
/// Enters by scaling the target uniformly from 80% to 100% about the middle of its top edge while fading in from
/// opacity 0 to the target's base opacity. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class ZoomUpInMotion : AbstractMotion
{
    public ZoomUpInMotion()
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
    public ZoomUpInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.OutCirc</c> easing (ease-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 0.8 to 1, a top-center transform origin
    /// and an opacity channel from 0 to the base opacity.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0, RelativeUnit.Relative);
        builder.Scale.From(0.8).To(1);
        builder.Opacity.From(0).To(context.BaseOpacity);
    }
}

/// <summary>
/// Leaves by scaling the target uniformly from 100% to 80% about the middle of its top edge while fading out from
/// the current opacity to 0. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class ZoomUpOutMotion : AbstractMotion
{
    public ZoomUpOutMotion()
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
    public ZoomUpOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.InOutCirc</c> easing (ease-in-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 1 to 0.8, a top-center transform origin
    /// and an opacity channel from the current opacity to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.8);
        builder.Opacity.FromCurrent().To(0);
    }
}

/// <summary>
/// Enters by scaling the target uniformly from 80% to 100% about the middle of its bottom edge while fading in from
/// opacity 0 to the target's base opacity. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class ZoomDownInMotion : AbstractMotion
{
    public ZoomDownInMotion()
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
    public ZoomDownInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.OutCirc</c> easing (ease-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 0.8 to 1, a bottom-center transform origin
    /// and an opacity channel from 0 to the base opacity.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 1, RelativeUnit.Relative);
        builder.Scale.From(0.8).To(1);
        builder.Opacity.From(0).To(context.BaseOpacity);
    }
}

/// <summary>
/// Leaves by scaling the target uniformly from 100% to 80% about the middle of its bottom edge while fading out from
/// the current opacity to 0. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class ZoomDownOutMotion : AbstractMotion
{
    public ZoomDownOutMotion()
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
    public ZoomDownOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.InOutCirc</c> easing (ease-in-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 1 to 0.8, a bottom-center transform origin
    /// and an opacity channel from the current opacity to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 1, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.8);
        builder.Opacity.FromCurrent().To(0);
    }
}

/// <summary>
/// Enters by scaling the target uniformly from 80% to 100% about the middle of its left edge while fading in from
/// opacity 0 to the target's base opacity. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class ZoomLeftInMotion : AbstractMotion
{
    public ZoomLeftInMotion()
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
    public ZoomLeftInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.OutCirc</c> easing (ease-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 0.8 to 1, a left-center transform origin
    /// and an opacity channel from 0 to the base opacity.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0.5, RelativeUnit.Relative);
        builder.Scale.From(0.8).To(1);
        builder.Opacity.From(0).To(context.BaseOpacity);
    }
}

/// <summary>
/// Leaves by scaling the target uniformly from 100% to 80% about the middle of its left edge while fading out from
/// the current opacity to 0. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class ZoomLeftOutMotion : AbstractMotion
{
    public ZoomLeftOutMotion()
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
    public ZoomLeftOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.InOutCirc</c> easing (ease-in-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 1 to 0.8, a left-center transform origin
    /// and an opacity channel from the current opacity to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0.5, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.8);
        builder.Opacity.FromCurrent().To(0);
    }
}

/// <summary>
/// Enters by scaling the target uniformly from 80% to 100% about the middle of its right edge while fading in from
/// opacity 0 to the target's base opacity. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class ZoomRightInMotion : AbstractMotion
{
    public ZoomRightInMotion()
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
    public ZoomRightInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.OutCirc</c> easing (ease-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 0.8 to 1, a right-center transform origin
    /// and an opacity channel from 0 to the base opacity.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 0.5, RelativeUnit.Relative);
        builder.Scale.From(0.8).To(1);
        builder.Opacity.From(0).To(context.BaseOpacity);
    }
}

/// <summary>
/// Leaves by scaling the target uniformly from 100% to 80% about the middle of its right edge while fading out from
/// the current opacity to 0. An explicit <see cref="AbstractMotion.Origin" /> replaces that anchor.
/// </summary>
public class ZoomRightOutMotion : AbstractMotion
{
    public ZoomRightOutMotion()
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
    public ZoomRightOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    /// <summary>
    /// The <c>MotionCurves.InOutCirc</c> easing (ease-in-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.Scale" /> channel from 1 to 0.8, a right-center transform origin
    /// and an opacity channel from the current opacity to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 0.5, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.8);
        builder.Opacity.FromCurrent().To(0);
    }
}
