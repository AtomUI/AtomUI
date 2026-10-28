using AtomUI.Animations;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace AtomUI.Controls.Commons;

internal static class BadgeMotionTransforms
{
    internal static readonly ITransform ScaleNearZero = CreateScale();
    internal static readonly RelativePoint Origin = new(0.5, 0.5, RelativeUnit.Relative);

    private static ITransform CreateScale()
    {
        var builder = TransformOperations.CreateBuilder(1);
        builder.AppendScale(0.01, 0.01);
        return builder.Build();
    }
}

internal sealed class BadgeInMotion(TimeSpan duration) : Motion(duration)
{
    private static readonly Easing Curve = new ExponentialEaseOut();
    protected override Easing DefaultEasing => Curve;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = BadgeMotionTransforms.Origin;
        builder.Scale.From(0.01).To(1);
        builder.Opacity.From(0).To(context.BaseOpacity);
    }
}

internal sealed class BadgeOutMotion(TimeSpan duration) : Motion(duration)
{
    private static readonly Easing Curve = new ExponentialEaseIn();
    protected override Easing DefaultEasing => Curve;

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = BadgeMotionTransforms.Origin;
        builder.Scale.FromCurrent().To(0.01);
        builder.Opacity.FromCurrent().To(0);
    }
}
