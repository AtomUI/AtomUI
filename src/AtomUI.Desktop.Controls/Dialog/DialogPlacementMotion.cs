using AtomUI.Animations;
using Avalonia;
using Avalonia.Animation.Easings;

namespace AtomUI.Desktop.Controls;

internal sealed class DialogPlacementMotion(bool opening) : AbstractMotion, IPlacementAwareMotion
{
    protected override Easing DefaultEasing => opening ? MotionCurves.OutCirc : MotionCurves.InOutCirc;

    public AbstractMotion Resolve(PlacementMotionContext geometry, RelativePoint? retainedOrigin)
    {
        var result = ComponentMotionSelection.CopyOptions(this, new DialogPlacementMotion(opening));
        result.Origin ??= retainedOrigin ?? (geometry.TriggerOrigin is { } point
            ? new RelativePoint(point, RelativeUnit.Absolute) : RelativePoint.Center);
        return result;
    }

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        if (opening)
        {
            builder.Scale.From(0.2).To(1);
            builder.Opacity.From(0).To(1);
        }
        else
        {
            builder.Scale.FromCurrent().To(0.2);
            builder.Opacity.FromCurrent().To(0);
        }
    }
}
