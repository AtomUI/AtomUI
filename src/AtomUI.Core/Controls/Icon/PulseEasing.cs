using Avalonia.Animation.Easings;

namespace AtomUI.Controls;

/// <summary>
/// An easing that advances in eight equal steps instead of along a continuous curve.
/// </summary>
/// <remarks>
/// The eased value is k/8 while the progress lies from k/8 up to, but not including, (k+1)/8, for k from
/// 0 to 7. It reaches 1 only when the progress reaches 1. <see cref="IconAnimation.Pulse" /> uses it to turn
/// an icon in visible steps.
/// </remarks>
public class PulseEasing : Easing
{
    private const int STEPS_COUNT = 8;

    private static readonly double[] Steps =
    {
        0d,
        1d / STEPS_COUNT,
        2d / STEPS_COUNT,
        3d / STEPS_COUNT,
        4d / STEPS_COUNT,
        5d / STEPS_COUNT,
        6d / STEPS_COUNT,
        7d / STEPS_COUNT,
        1d
    };

    /// <summary>
    /// Returns the reached step: the largest of the step values 0, 1/8, ..., 7/8 and 1 that does not exceed
    /// <paramref name="progress" />.
    /// </summary>
    /// <param name="progress">
    /// The linear progress of the animation, from 0 at the start to 1 at the end.
    /// </param>
    /// <returns>
    /// The reached step, between 0 and 1.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no step qualifies, which is the case when <paramref name="progress" /> is negative or NaN.
    /// </exception>
    public override double Ease(double progress)
    {
        for (var i = Steps.Length - 1; i >= 0; i--)
        {
            if (Steps[i] <= progress)
            {
                return Steps[i];
            }
        }

        throw new InvalidOperationException("Sequence contains no matching element");
    }
}
