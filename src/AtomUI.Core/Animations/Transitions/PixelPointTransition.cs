using Avalonia;
using Avalonia.Animation;

namespace AtomUI.Animations;

/// <summary>
/// Interpolates a <see cref="PixelPoint" /> property between two points, with each coordinate rounded to
/// the nearest integer.
/// </summary>
public class PixelPointTransition : InterpolatingTransitionBase<PixelPoint>
{
    /// <summary>
    /// Returns the point between <paramref name="oldValue" /> and <paramref name="newValue" />, with each
    /// coordinate rounded to the nearest integer.
    /// </summary>
    /// <param name="progress">
    /// The eased progress of the transition, from 0 to 1.
    /// </param>
    /// <param name="oldValue">
    /// The point the property changes from.
    /// </param>
    /// <param name="newValue">
    /// The point the property changes to.
    /// </param>
    protected override PixelPoint Interpolate(double progress, PixelPoint oldValue, PixelPoint newValue)
    {
        var delta = newValue - oldValue;
        return new PixelPoint((int)Math.Round(delta.X * progress + oldValue.X),
            (int)Math.Round(delta.Y * progress + oldValue.Y));
    }
}