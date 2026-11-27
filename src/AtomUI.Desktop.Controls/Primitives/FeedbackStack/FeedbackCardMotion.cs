using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Finite card entry/exit; queue positioning and stack scale stay on the outer card.
/// </summary>
internal sealed class FeedbackCardMotion : AbstractMotion, IPlacementAwareMotion
{
    internal const double Offset = 64d;

    private readonly bool _entering;
    private readonly Vector _offset;

    internal FeedbackCardMotion(bool isEntering, NotificationPosition position, TimeSpan? duration = null)
        : base(duration, MotionCurves.InOut)
    {
        _entering = isEntering;
        _offset = position switch
        {
            NotificationPosition.TopCenter => new Vector(0, -Offset),
            NotificationPosition.BottomCenter => new Vector(0, Offset),
            NotificationPosition.TopLeft or NotificationPosition.BottomLeft => new Vector(-Offset, 0),
            _ => new Vector(Offset, 0)
        };
    }

    public AbstractMotion Resolve(PlacementMotionContext geometry, RelativePoint? retainedOrigin)
    {
        var position = geometry.Placement switch
        {
            PlacementMode.Top => NotificationPosition.TopCenter,
            PlacementMode.Bottom => NotificationPosition.BottomCenter,
            PlacementMode.TopEdgeAlignedLeft => NotificationPosition.TopLeft,
            PlacementMode.BottomEdgeAlignedLeft => NotificationPosition.BottomLeft,
            PlacementMode.BottomEdgeAlignedRight => NotificationPosition.BottomRight,
            _ => NotificationPosition.TopRight
        };
        return ComponentMotionSelection.CopyOptions(this, new FeedbackCardMotion(_entering, position));
    }

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Origin = new RelativePoint(0, 0, RelativeUnit.Absolute);
        if (_entering)
        {
            builder.TranslateX.From(_offset.X).To(0);
            builder.TranslateY.From(_offset.Y).To(0);
            builder.Opacity.From(0).To(context.BaseOpacity);
        }
        else
        {
            builder.TranslateX.FromCurrent().To(_offset.X);
            builder.TranslateY.FromCurrent().To(_offset.Y);
            builder.Opacity.FromCurrent().To(0);
        }
    }
}
