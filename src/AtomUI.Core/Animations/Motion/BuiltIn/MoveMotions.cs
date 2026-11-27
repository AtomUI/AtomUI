using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

/// <summary>
/// Moves the target down into place from the travel distance above its final position
/// while fading it in from opacity 0 to 1.
/// </summary>
public class MoveUpInMotion : AbstractMotion
{
    /// <summary>
    /// The distance the target travels, in device-independent pixels, or <see langword="null" /> to use the height
    /// of the target. A negative or non-finite value throws an <see cref="ArgumentOutOfRangeException" />
    /// when the motion is played.
    /// </summary>
    public double? Offset { get; set; }

    public MoveUpInMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given travel distance, duration, easing and fill mode.
    /// </summary>
    /// <param name="offset">
    /// The travel distance; see <see cref="Offset" />.
    /// </param>
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
    public MoveUpInMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    /// <summary>
    /// The <c>MotionCurves.OutCirc</c> easing (ease-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.TranslateY" /> channel from <c>-offset</c> to 0
    /// and an opacity channel from 0 to 1.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        var offset = Offset ?? context.TargetSize.Height;
        if (!double.IsFinite(offset) || offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Offset));
        }
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.TranslateY.From(-offset).To(0);
        builder.Opacity.From(0).To(1);
    }
}

/// <summary>
/// Moves the target up by the travel distance while fading it out from opacity 1 to 0.
/// </summary>
public class MoveUpOutMotion : AbstractMotion
{
    /// <summary>
    /// The distance the target travels, in device-independent pixels, or <see langword="null" /> to use the height
    /// of the target. A negative or non-finite value throws an <see cref="ArgumentOutOfRangeException" />
    /// when the motion is played.
    /// </summary>
    public double? Offset { get; set; }

    public MoveUpOutMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given travel distance, duration, easing and fill mode.
    /// </summary>
    /// <param name="offset">
    /// The travel distance; see <see cref="Offset" />.
    /// </param>
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
    public MoveUpOutMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    /// <summary>
    /// The <c>MotionCurves.InOutCirc</c> easing (ease-in-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.TranslateY" /> channel from 0 to <c>-offset</c>
    /// and an opacity channel from 1 to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        var offset = Offset ?? context.TargetSize.Height;
        if (!double.IsFinite(offset) || offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Offset));
        }
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.TranslateY.From(0).To(-offset);
        builder.Opacity.From(1).To(0);
    }
}

/// <summary>
/// Moves the target up into place from the travel distance below its final position
/// while fading it in from opacity 0 to 1.
/// </summary>
public class MoveDownInMotion : AbstractMotion
{
    /// <summary>
    /// The distance the target travels, in device-independent pixels, or <see langword="null" /> to use the height
    /// of the target. A negative or non-finite value throws an <see cref="ArgumentOutOfRangeException" />
    /// when the motion is played.
    /// </summary>
    public double? Offset { get; set; }

    public MoveDownInMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given travel distance, duration, easing and fill mode.
    /// </summary>
    /// <param name="offset">
    /// The travel distance; see <see cref="Offset" />.
    /// </param>
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
    public MoveDownInMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    /// <summary>
    /// The <c>MotionCurves.OutCirc</c> easing (ease-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.TranslateY" /> channel from <c>offset</c> to 0
    /// and an opacity channel from 0 to 1.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        var offset = Offset ?? context.TargetSize.Height;
        if (!double.IsFinite(offset) || offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Offset));
        }
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.TranslateY.From(offset).To(0);
        builder.Opacity.From(0).To(1);
    }
}

/// <summary>
/// Moves the target down by the travel distance while fading it out from opacity 1 to 0.
/// </summary>
public class MoveDownOutMotion : AbstractMotion
{
    /// <summary>
    /// The distance the target travels, in device-independent pixels, or <see langword="null" /> to use the height
    /// of the target. A negative or non-finite value throws an <see cref="ArgumentOutOfRangeException" />
    /// when the motion is played.
    /// </summary>
    public double? Offset { get; set; }

    public MoveDownOutMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given travel distance, duration, easing and fill mode.
    /// </summary>
    /// <param name="offset">
    /// The travel distance; see <see cref="Offset" />.
    /// </param>
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
    public MoveDownOutMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    /// <summary>
    /// The <c>MotionCurves.InOutCirc</c> easing (ease-in-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.TranslateY" /> channel from 0 to <c>offset</c>
    /// and an opacity channel from 1 to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        var offset = Offset ?? context.TargetSize.Height;
        if (!double.IsFinite(offset) || offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Offset));
        }
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.TranslateY.From(0).To(offset);
        builder.Opacity.From(1).To(0);
    }
}

/// <summary>
/// Moves the target right into place from the travel distance to the left of its final
/// position while fading it in from opacity 0 to 1.
/// </summary>
public class MoveLeftInMotion : AbstractMotion
{
    /// <summary>
    /// The distance the target travels, in device-independent pixels, or <see langword="null" /> to use the width
    /// of the target. A negative or non-finite value throws an <see cref="ArgumentOutOfRangeException" />
    /// when the motion is played.
    /// </summary>
    public double? Offset { get; set; }

    public MoveLeftInMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given travel distance, duration, easing and fill mode.
    /// </summary>
    /// <param name="offset">
    /// The travel distance; see <see cref="Offset" />.
    /// </param>
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
    public MoveLeftInMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    /// <summary>
    /// The <c>MotionCurves.OutCirc</c> easing (ease-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.TranslateX" /> channel from <c>-offset</c> to 0
    /// and an opacity channel from 0 to 1.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        var offset = Offset ?? context.TargetSize.Width;
        if (!double.IsFinite(offset) || offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Offset));
        }
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.TranslateX.From(-offset).To(0);
        builder.Opacity.From(0).To(1);
    }
}

/// <summary>
/// Moves the target left by the travel distance while fading it out from opacity 1 to 0.
/// </summary>
public class MoveLeftOutMotion : AbstractMotion
{
    /// <summary>
    /// The distance the target travels, in device-independent pixels, or <see langword="null" /> to use the width
    /// of the target. A negative or non-finite value throws an <see cref="ArgumentOutOfRangeException" />
    /// when the motion is played.
    /// </summary>
    public double? Offset { get; set; }

    public MoveLeftOutMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given travel distance, duration, easing and fill mode.
    /// </summary>
    /// <param name="offset">
    /// The travel distance; see <see cref="Offset" />.
    /// </param>
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
    public MoveLeftOutMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    /// <summary>
    /// The <c>MotionCurves.InOutCirc</c> easing (ease-in-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.TranslateX" /> channel from 0 to <c>-offset</c>
    /// and an opacity channel from 1 to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        var offset = Offset ?? context.TargetSize.Width;
        if (!double.IsFinite(offset) || offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Offset));
        }
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.TranslateX.From(0).To(-offset);
        builder.Opacity.From(1).To(0);
    }
}

/// <summary>
/// Moves the target left into place from the travel distance to the right of its final
/// position while fading it in from opacity 0 to 1.
/// </summary>
public class MoveRightInMotion : AbstractMotion
{
    /// <summary>
    /// The distance the target travels, in device-independent pixels, or <see langword="null" /> to use the width
    /// of the target. A negative or non-finite value throws an <see cref="ArgumentOutOfRangeException" />
    /// when the motion is played.
    /// </summary>
    public double? Offset { get; set; }

    public MoveRightInMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given travel distance, duration, easing and fill mode.
    /// </summary>
    /// <param name="offset">
    /// The travel distance; see <see cref="Offset" />.
    /// </param>
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
    public MoveRightInMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    /// <summary>
    /// The <c>MotionCurves.OutCirc</c> easing (ease-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.TranslateX" /> channel from <c>offset</c> to 0
    /// and an opacity channel from 0 to 1.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        var offset = Offset ?? context.TargetSize.Width;
        if (!double.IsFinite(offset) || offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Offset));
        }
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.TranslateX.From(offset).To(0);
        builder.Opacity.From(0).To(1);
    }
}

/// <summary>
/// Moves the target right by the travel distance while fading it out from opacity 1 to 0.
/// </summary>
public class MoveRightOutMotion : AbstractMotion
{
    /// <summary>
    /// The distance the target travels, in device-independent pixels, or <see langword="null" /> to use the width
    /// of the target. A negative or non-finite value throws an <see cref="ArgumentOutOfRangeException" />
    /// when the motion is played.
    /// </summary>
    public double? Offset { get; set; }

    public MoveRightOutMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given travel distance, duration, easing and fill mode.
    /// </summary>
    /// <param name="offset">
    /// The travel distance; see <see cref="Offset" />.
    /// </param>
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
    public MoveRightOutMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    /// <summary>
    /// The <c>MotionCurves.InOutCirc</c> easing (ease-in-out circular), used when
    /// <see cref="AbstractMotion.Easing" /> is null.
    /// </summary>
    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    /// <summary>
    /// Declares a <see cref="MotionBuilder.TranslateX" /> channel from 0 to <c>offset</c>
    /// and an opacity channel from 1 to 0.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        var offset = Offset ?? context.TargetSize.Width;
        if (!double.IsFinite(offset) || offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Offset));
        }
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.TranslateX.From(0).To(offset);
        builder.Opacity.From(1).To(0);
    }
}
