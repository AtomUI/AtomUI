using AtomUI.Animations;

namespace AtomUI.Desktop.Controls;

internal sealed class DrawerPanelMotion(DrawerPlacement placement, bool opening) : Motion
{
    protected override Avalonia.Animation.Easings.Easing DefaultEasing => MotionCurves.Ease;

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
