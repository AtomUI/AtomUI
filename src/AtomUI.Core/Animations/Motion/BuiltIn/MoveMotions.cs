using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

public class MoveUpInMotion : AbstractMotion
{
    public double? Offset { get; set; }

    public MoveUpInMotion()
    {
    }

    public MoveUpInMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    protected override Easing DefaultEasing => MotionCurves.OutCirc;

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

public class MoveUpOutMotion : AbstractMotion
{
    public double? Offset { get; set; }

    public MoveUpOutMotion()
    {
    }

    public MoveUpOutMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

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

public class MoveDownInMotion : AbstractMotion
{
    public double? Offset { get; set; }

    public MoveDownInMotion()
    {
    }

    public MoveDownInMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    protected override Easing DefaultEasing => MotionCurves.OutCirc;

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

public class MoveDownOutMotion : AbstractMotion
{
    public double? Offset { get; set; }

    public MoveDownOutMotion()
    {
    }

    public MoveDownOutMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

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

public class MoveLeftInMotion : AbstractMotion
{
    public double? Offset { get; set; }

    public MoveLeftInMotion()
    {
    }

    public MoveLeftInMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    protected override Easing DefaultEasing => MotionCurves.OutCirc;

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

public class MoveLeftOutMotion : AbstractMotion
{
    public double? Offset { get; set; }

    public MoveLeftOutMotion()
    {
    }

    public MoveLeftOutMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

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

public class MoveRightInMotion : AbstractMotion
{
    public double? Offset { get; set; }

    public MoveRightInMotion()
    {
    }

    public MoveRightInMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    protected override Easing DefaultEasing => MotionCurves.OutCirc;

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

public class MoveRightOutMotion : AbstractMotion
{
    public double? Offset { get; set; }

    public MoveRightOutMotion()
    {
    }

    public MoveRightOutMotion(double offset, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Offset = offset;
    }

    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

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
