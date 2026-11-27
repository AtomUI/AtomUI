using Avalonia.Animation;
using Avalonia.Media;

namespace AtomUI.Animations;

/// <summary>
/// Interpolates an <see cref="IBrush" /> property between two brushes.
/// </summary>
/// <remarks>
/// Two solid brushes blend their color and opacity. A gradient brush and a solid brush, or two gradients of
/// the same kind, are interpolated as gradients, with the solid brush converted to a gradient first. Any
/// other pair, including a <see langword="null" /> brush, switches from the old brush to the new one
/// halfway through the transition.
/// </remarks>
public class SolidColorBrushTransition : InterpolatingTransitionBase<IBrush?>
{
    /// <summary>
    /// Returns the brush between <paramref name="from" /> and <paramref name="to" /> at the given progress.
    /// </summary>
    /// <param name="progress">
    /// The eased progress of the transition, from 0 to 1.
    /// </param>
    /// <param name="from">
    /// The brush the property changes from.
    /// </param>
    /// <param name="to">
    /// The brush the property changes to.
    /// </param>
    protected override IBrush? Interpolate(double progress, IBrush? from, IBrush? to)
    {
        if (from is ISolidColorBrush fromBrush && to is ISolidColorBrush toBrush)
        {
            return TransitionInterpolation.SolidColorBrushInterpolate(progress, fromBrush, toBrush);
        }
        return TransitionInterpolation.BrushInterpolate(progress, from, to);
    }
}