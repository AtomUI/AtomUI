using AtomUI.Animations;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Stock component definitions selected by the same slot as language and application contributions.
/// </summary>
internal static class DefaultComponentMotions
{
    internal static AbstractMotion? Create(MotionSlot slot) => slot switch
    {
        MotionSlot.DialogOpen => new DialogPlacementMotion(opening: true),
        MotionSlot.DialogClose => new DialogPlacementMotion(opening: false),
        MotionSlot.DrawerOpen => new DrawerPanelMotion(DrawerPlacement.Right, opening: true),
        MotionSlot.DrawerClose => new DrawerPanelMotion(DrawerPlacement.Right, opening: false),
        MotionSlot.OverlayMaskOpen => new FadeInMotion(),
        MotionSlot.OverlayMaskClose => new FadeOutMotion(),
        MotionSlot.MessageEnter or MotionSlot.NotificationEnter => new FeedbackCardMotion(true, NotificationPosition.TopRight),
        MotionSlot.MessageLeave or MotionSlot.NotificationLeave => new FeedbackCardMotion(false, NotificationPosition.TopRight),
        _ => null
    };
}
