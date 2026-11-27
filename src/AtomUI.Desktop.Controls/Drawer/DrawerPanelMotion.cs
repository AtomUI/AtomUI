using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal sealed class DrawerPanelMotion(DrawerPlacement placement, bool opening) : AbstractMotion, IPlacementAwareMotion
{
    protected override Avalonia.Animation.Easings.Easing DefaultEasing => MotionCurves.Ease;

    public AbstractMotion Resolve(PlacementMotionContext geometry, RelativePoint? retainedOrigin)
    {
        var side = geometry.Placement switch
        {
            PlacementMode.Left => DrawerPlacement.Left,
            PlacementMode.Top => DrawerPlacement.Top,
            PlacementMode.Bottom => DrawerPlacement.Bottom,
            _ => DrawerPlacement.Right
        };
        return ComponentMotionSelection.CopyOptions(this, new DrawerPanelMotion(side, opening));
    }

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        var horizontal = placement is DrawerPlacement.Left or DrawerPlacement.Right;
        var extent = horizontal ? context.TargetSize.Width : context.TargetSize.Height;
        var offset = placement is DrawerPlacement.Left or DrawerPlacement.Top ? -extent : extent;
        var translation = horizontal ? builder.TranslateX : builder.TranslateY;
        if (opening)
        {
            translation.From(offset).To(0);
            builder.Opacity.From(0.7).To(1);
        }
        else
        {
            translation.FromCurrent().To(offset);
            builder.Opacity.FromCurrent().To(0.7);
        }
    }
}
