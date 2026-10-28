using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

public class ExpandMotion : AbstractMotion
{
    public Direction? Direction { get; set; }

    public ExpandMotion()
    {
    }

    public ExpandMotion(Direction direction, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Direction = direction;
    }

    protected override Easing DefaultEasing => MotionCurves.InOut;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Expand(Direction ?? context.LayoutDirection);
        builder.Opacity.FromCurrent().To(context.BaseOpacity);
    }
}

public class CollapseMotion : AbstractMotion
{
    public Direction? Direction { get; set; }

    public CollapseMotion()
    {
    }

    public CollapseMotion(Direction direction, TimeSpan? duration = null, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
        Direction = direction;
    }

    protected override Easing DefaultEasing => MotionCurves.InOut;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Collapse(Direction ?? context.LayoutDirection);
        builder.Opacity.FromCurrent().To(0);
    }
}
