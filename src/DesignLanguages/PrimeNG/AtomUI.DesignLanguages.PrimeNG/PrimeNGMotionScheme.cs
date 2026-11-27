using AtomUI.Animations;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Resources;

namespace AtomUI.DesignLanguages.PrimeNG;

internal sealed class PrimeNGMotionScheme : MotionScheme
{
    public override bool TryGetMotion(MotionSlot slot, in DesignVariant variant, out AbstractMotion? motion)
    {
        motion = slot switch
        {
            MotionSlot.DialogOpen => new ZoomInMotion { FromScale = 0.7, EasingToken = SharedTokenKind.MotionEasingEnter },
            MotionSlot.DialogClose => new ZoomOutMotion { ToScale = 0.7, EasingToken = SharedTokenKind.MotionEasingLeave },
            MotionSlot.SelectOpen or MotionSlot.DropdownOpen or MotionSlot.SubMenuOpen or MotionSlot.PopoverOpen =>
                new ZoomBigInMotion
                {
                    DurationToken = SharedTokenKind.MotionDurationEnter,
                    EasingToken = SharedTokenKind.MotionEasingEnter
                },
            MotionSlot.SelectClose or MotionSlot.DropdownClose or MotionSlot.SubMenuClose or MotionSlot.PopoverClose =>
                new ZoomBigOutMotion
                {
                    DurationToken = SharedTokenKind.MotionDurationLeave,
                    EasingToken = SharedTokenKind.MotionEasingLeave
                },
            MotionSlot.TooltipOpen => new FadeInMotion { EasingToken = SharedTokenKind.MotionEasingEnter },
            MotionSlot.TooltipClose => new FadeOutMotion { EasingToken = SharedTokenKind.MotionEasingLeave },
            _ => null
        };
        return motion is not null;
    }
}
