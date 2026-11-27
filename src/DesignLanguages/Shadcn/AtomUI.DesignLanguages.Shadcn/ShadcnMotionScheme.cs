using AtomUI.Animations;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.DesignLanguages.Shadcn;

internal sealed class ShadcnMotionScheme : MotionScheme
{
    public override bool TryGetMotion(MotionSlot slot, in DesignVariant variant, out AbstractMotion? motion)
    {
        motion = slot switch
        {
            MotionSlot.DialogOpen => new ZoomInMotion
            {
                FromScale = 0.95,
                DurationToken = SharedTokenKind.MotionDurationEnter,
                EasingToken = SharedTokenKind.MotionEasingEnter
            },
            MotionSlot.DialogClose => new ZoomOutMotion
            {
                ToScale = 0.95,
                DurationToken = SharedTokenKind.MotionDurationLeave,
                EasingToken = SharedTokenKind.MotionEasingLeave
            },
            MotionSlot.SelectOpen or MotionSlot.DropdownOpen or MotionSlot.SubMenuOpen or
            MotionSlot.PopoverOpen or MotionSlot.TooltipOpen =>
                new AnchoredMotion(true)
                {
                    DurationToken = SharedTokenKind.MotionDurationEnter,
                    EasingToken = SharedTokenKind.MotionEasingEnter
                },
            MotionSlot.SelectClose or MotionSlot.DropdownClose or MotionSlot.SubMenuClose or
            MotionSlot.PopoverClose or MotionSlot.TooltipClose =>
                new AnchoredMotion(false)
                {
                    DurationToken = SharedTokenKind.MotionDurationLeave,
                    EasingToken = SharedTokenKind.MotionEasingLeave
                },
            _ => null
        };
        return motion is not null;
    }

    private sealed class AnchoredMotion(bool entering) : AbstractMotion, IPlacementAwareMotion
    {
        private Vector _offset;

        public AbstractMotion Resolve(PlacementMotionContext context, RelativePoint? retainedOrigin)
        {
            var initial = context.Placement switch
            {
                PlacementMode.Top or PlacementMode.TopEdgeAlignedLeft or PlacementMode.TopEdgeAlignedRight => new Vector(0, 4),
                PlacementMode.Left or PlacementMode.LeftEdgeAlignedTop or PlacementMode.LeftEdgeAlignedBottom => new Vector(4, 0),
                PlacementMode.Right or PlacementMode.RightEdgeAlignedTop or PlacementMode.RightEdgeAlignedBottom => new Vector(-4, 0),
                _ => new Vector(0, -4)
            };
            // This is a stateless definition. Preserve all selected options in the placed copy.
            var resolved = (AnchoredMotion)MemberwiseClone();
            resolved._offset = new Vector(context.HorizontalFlipped ? -initial.X : initial.X,
                context.VerticalFlipped ? -initial.Y : initial.Y);
            resolved.Origin ??= retainedOrigin;
            return resolved;
        }

        protected override void Build(MotionBuilder builder, in MotionContext context)
        {
            if (entering)
            {
                builder.Opacity.From(0).To(1);
                builder.TranslateX.From(_offset.X).To(0);
                builder.TranslateY.From(_offset.Y).To(0);
            }
            else
            {
                builder.Opacity.FromCurrent().To(0);
                builder.TranslateX.FromCurrent().To(_offset.X);
                builder.TranslateY.FromCurrent().To(_offset.Y);
            }
        }
    }
}
