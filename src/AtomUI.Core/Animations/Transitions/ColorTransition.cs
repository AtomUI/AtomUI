using AtomUI.Animations;
using Avalonia.Animation;
using Avalonia.Media;

namespace AtomUI.Media;

/// <summary>
/// Interpolates a <see cref="Color" /> property between two colors in premultiplied-alpha space, so the
/// components of a nearly transparent color have little effect on the result.
/// </summary>
public class ColorTransition : InterpolatingTransitionBase<Color>
{
    /// <summary>
    /// Returns the color between <paramref name="from" /> and <paramref name="to" /> at the given progress.
    /// </summary>
    /// <param name="progress">
    /// The eased progress of the transition, from 0 to 1.
    /// </param>
    /// <param name="from">
    /// The color the property changes from.
    /// </param>
    /// <param name="to">
    /// The color the property changes to.
    /// </param>
    protected override Color Interpolate(double progress, Color from, Color to)
    {
        return TransitionInterpolation.ColorInterpolate(progress, from, to);
    }
}