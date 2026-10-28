using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

public class SlideUpInMotion : Motion
{
    public SlideUpInMotion()
    {
    }

    public SlideUpInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.OutQuint;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.ScaleY.From(.8).To(1);
        builder.Opacity.From(0).To(1);
    }
}

public class SlideUpOutMotion : Motion
{
    public SlideUpOutMotion()
    {
    }

    public SlideUpOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.InQuint;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.ScaleY.From(1).To(.8);
        builder.Opacity.From(1).To(0);
    }
}

public class SlideDownInMotion : Motion
{
    public SlideDownInMotion()
    {
    }

    public SlideDownInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.OutQuint;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 1, RelativeUnit.Relative);
        builder.ScaleY.From(.8).To(1);
        builder.Opacity.From(0).To(1);
    }
}

public class SlideDownOutMotion : Motion
{
    public SlideDownOutMotion()
    {
    }

    public SlideDownOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.InQuint;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 1, RelativeUnit.Relative);
        builder.ScaleY.From(1).To(.8);
        builder.Opacity.From(1).To(0);
    }
}

public class SlideLeftInMotion : Motion
{
    public SlideLeftInMotion()
    {
    }

    public SlideLeftInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.OutQuint;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.ScaleX.From(.8).To(1);
        builder.Opacity.From(0).To(1);
    }
}

public class SlideLeftOutMotion : Motion
{
    public SlideLeftOutMotion()
    {
    }

    public SlideLeftOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.InQuint;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Relative);
        builder.ScaleX.From(1).To(.8);
        builder.Opacity.From(1).To(0);
    }
}

public class SlideRightInMotion : Motion
{
    public SlideRightInMotion()
    {
    }

    public SlideRightInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.OutQuint;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 0, RelativeUnit.Relative);
        builder.ScaleX.From(.8).To(1);
        builder.Opacity.From(0).To(1);
    }
}

public class SlideRightOutMotion : Motion
{
    public SlideRightOutMotion()
    {
    }

    public SlideRightOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.InQuint;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 0, RelativeUnit.Relative);
        builder.ScaleX.From(1).To(.8);
        builder.Opacity.From(1).To(0);
    }
}
