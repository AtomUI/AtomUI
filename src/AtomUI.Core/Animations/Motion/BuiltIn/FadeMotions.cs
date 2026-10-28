using Avalonia.Animation;
using Avalonia.Animation.Easings;

namespace AtomUI.Animations;

public class FadeInMotion : AbstractMotion
{
    public FadeInMotion()
    {
    }

    public FadeInMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Opacity.From(0).To(1);
    }
}

public class FadeOutMotion : AbstractMotion
{
    public FadeOutMotion()
    {
    }

    public FadeOutMotion(TimeSpan? duration, Easing? easing = null, FillMode fillMode = FillMode.Both)
        : base(duration, easing, fillMode)
    {
    }

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Opacity.From(1).To(0);
    }
}
