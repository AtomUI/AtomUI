using AtomUI.Animations;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal static class ComponentMotionSelection
{
    internal static bool TryResolve(StyledElement owner, StyledProperty<AbstractMotion?> property, MotionSlot slot,
                                    out AbstractMotion? motion)
    {
        if (owner.IsSet(property))
        {
            motion = owner.GetValue(property);
            return true;
        }
        return ScopedMotionSelection.TryResolve(owner, slot, out motion);
    }

    internal static AbstractMotion? Resolve(StyledElement owner, StyledProperty<AbstractMotion?> property, MotionSlot slot,
                                            StyledElement? presentationScope = null)
    {
        if (owner.IsSet(property))
        {
            return owner.GetValue(property);
        }
        return ResolveScoped(owner, slot, presentationScope);
    }

    internal static AbstractMotion? ResolveScoped(StyledElement owner, MotionSlot slot, StyledElement? presentationScope)
    {
        var scope = presentationScope is not null && ThemeScope.ResolveContext(owner) is null ? presentationScope : owner;
        return ScopedMotionSelection.TryResolve(scope, slot, out var motion) ? motion : PopupMotionDefaults.CreateForSlot(slot);
    }

    internal static Task<MotionResult>? Play(AbstractMotion? definition, Control target, in PlacementMotionContext geometry,
                                             TimeSpan defaultDuration, CancellationToken cancellationToken = default,
                                             RelativePoint? retainedOrigin = null)
    {
        if (definition is null)
        {
            return null;
        }
        var motion = definition is IPlacementAwareMotion placed ? placed.Resolve(geometry, retainedOrigin) : definition;
        return motion.PlayWithFallbackAsync(target, defaultDuration, cancellationToken);
    }

    internal static TMotion CopyOptions<TMotion>(AbstractMotion source, TMotion copy) where TMotion : AbstractMotion
    {
        copy.Duration = source.Duration;
        copy.DurationToken = source.DurationToken;
        copy.Delay = source.Delay;
        copy.Easing = source.Easing;
        copy.EasingToken = source.EasingToken;
        copy.Origin = source.Origin;
        copy.FillMode = source.FillMode;
        return copy;
    }
}
