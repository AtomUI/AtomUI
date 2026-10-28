using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

public class ZoomInMotion : Motion
{
    public ZoomInMotion()
    {
    }

    public ZoomInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        builder.Scale.From(0.2).To(1);
        builder.Opacity.From(0).To(1);
    }
}

public class ZoomOutMotion : Motion
{
    public ZoomOutMotion()
    {
    }

    public ZoomOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.2);
        builder.Opacity.FromCurrent().To(0);
    }
}

public class ZoomBigInMotion : Motion
{
    public ZoomBigInMotion()
    {
    }

    public ZoomBigInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        builder.Scale.From(0.8).To(1);
        builder.Opacity.From(0).To(1);
    }
}

public class ZoomBigOutMotion : Motion
{
    public ZoomBigOutMotion()
    {
    }

    public ZoomBigOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.8);
        builder.Opacity.FromCurrent().To(0);
    }
}

public class ZoomUpInMotion : Motion
{
    public ZoomUpInMotion()
    {
    }

    public ZoomUpInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0, RelativeUnit.Relative);
        builder.Scale.From(0.8).To(1);
        builder.Opacity.From(0).To(context.BaseOpacity);
    }
}

public class ZoomUpOutMotion : Motion
{
    public ZoomUpOutMotion()
    {
    }

    public ZoomUpOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 0, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.8);
        builder.Opacity.FromCurrent().To(0);
    }
}

public class ZoomDownInMotion : Motion
{
    public ZoomDownInMotion()
    {
    }

    public ZoomDownInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 1, RelativeUnit.Relative);
        builder.Scale.From(0.8).To(1);
        builder.Opacity.From(0).To(context.BaseOpacity);
    }
}

public class ZoomDownOutMotion : Motion
{
    public ZoomDownOutMotion()
    {
    }

    public ZoomDownOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0.5, 1, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.8);
        builder.Opacity.FromCurrent().To(0);
    }
}

public class ZoomLeftInMotion : Motion
{
    public ZoomLeftInMotion()
    {
    }

    public ZoomLeftInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0.5, RelativeUnit.Relative);
        builder.Scale.From(0.8).To(1);
        builder.Opacity.From(0).To(context.BaseOpacity);
    }
}

public class ZoomLeftOutMotion : Motion
{
    public ZoomLeftOutMotion()
    {
    }

    public ZoomLeftOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0.5, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.8);
        builder.Opacity.FromCurrent().To(0);
    }
}

public class ZoomRightInMotion : Motion
{
    public ZoomRightInMotion()
    {
    }

    public ZoomRightInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.OutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 0.5, RelativeUnit.Relative);
        builder.Scale.From(0.8).To(1);
        builder.Opacity.From(0).To(context.BaseOpacity);
    }
}

public class ZoomRightOutMotion : Motion
{
    public ZoomRightOutMotion()
    {
    }

    public ZoomRightOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override Easing DefaultEasing => MotionCurves.InOutCirc;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(1, 0.5, RelativeUnit.Relative);
        builder.Scale.From(1).To(0.8);
        builder.Opacity.FromCurrent().To(0);
    }
}
