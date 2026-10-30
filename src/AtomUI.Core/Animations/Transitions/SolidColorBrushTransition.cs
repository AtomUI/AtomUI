using Avalonia.Animation;
using Avalonia.Media;

namespace AtomUI.Animations;

public class SolidColorBrushTransition : InterpolatingTransitionBase<IBrush?>
{
    protected override IBrush? Interpolate(double progress, IBrush? from, IBrush? to)
    {
        if (from is ISolidColorBrush fromBrush && to is ISolidColorBrush toBrush)
        {
            return TransitionInterpolation.SolidColorBrushInterpolate(progress, fromBrush, toBrush);
        }
        return TransitionInterpolation.BrushInterpolate(progress, from, to);
    }
}