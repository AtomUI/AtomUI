using Avalonia.Animation;
using Avalonia.Media;

namespace AtomUI.Animations;

internal class BoxShadowsTransition : InterpolatingTransitionBase<BoxShadows>
{
    protected override BoxShadows Interpolate(double progress, BoxShadows from, BoxShadows to)
    {
        return TransitionInterpolation.BoxShadowsInterpolate(progress, from, to);
    }
}