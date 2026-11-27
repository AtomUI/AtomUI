using Avalonia;
using Avalonia.Animation;

namespace AtomUI.Animations;

/// <summary>
/// Interpolates a <see cref="Rect" /> property between two rectangles.
/// </summary>
public class RectTransition : InterpolatingTransitionBase<Rect>
{
    /// <summary>
    /// Returns the rectangle whose position and size lie between those of <paramref name="from" /> and
    /// <paramref name="to" />.
    /// </summary>
    /// <param name="progress">
    /// The eased progress of the transition, from 0 to 1.
    /// </param>
    /// <param name="from">
    /// The rectangle the property changes from.
    /// </param>
    /// <param name="to">
    /// The rectangle the property changes to.
    /// </param>
    protected override Rect Interpolate(double progress, Rect from, Rect to)
    {
        var deltaPos  = to.Position - from.Position;
        var deltaSize = to.Size - from.Size;

        var newPos  = (deltaPos * progress) + from.Position;
        var newSize = (deltaSize * progress) + from.Size;

        return new Rect(newPos, newSize);
    }
}