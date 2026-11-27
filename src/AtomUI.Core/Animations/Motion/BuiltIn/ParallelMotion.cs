using Avalonia.Metadata;

namespace AtomUI.Animations;

/// <summary>
/// Plays its child motions at the same time.
/// </summary>
/// <remarks>
/// Without an explicit <see cref="AbstractMotion.Duration" /> the group lasts until its longest child has finished,
/// and each child keeps its own delay and duration. An explicit duration, including a
/// <see cref="Motion.DurationOverrideProperty" /> on the target, scales the whole timeline to fit.
/// Preparing the motion throws an <see cref="InvalidOperationException" /> when two children write the same
/// channel or property, when the group's <see cref="AbstractMotion.Easing" /> is not linear, when a child
/// does not use <see cref="Avalonia.Animation.FillMode.Both" />, or when children that scale or rotate declare
/// different origins and the group sets no <see cref="AbstractMotion.Origin" />.
/// </remarks>
public class ParallelMotion : AbstractMotion
{
    /// <summary>
    /// The child motions. The list is read each time the motion is prepared, and a <see langword="null" />
    /// element then throws an <see cref="ArgumentNullException" />.
    /// </summary>
    [Content]
    public IList<AbstractMotion> Children { get; } = new List<AbstractMotion>();

    public ParallelMotion()
    {
    }

    /// <summary>
    /// Creates the motion with the given children.
    /// </summary>
    /// <param name="children">
    /// The child motions, copied into <see cref="Children" />. The array must not be <see langword="null" />.
    /// </param>
    public ParallelMotion(params AbstractMotion[] children)
    {
        ArgumentNullException.ThrowIfNull(children);
        foreach (var child in children)
        {
            Children.Add(child);
        }
    }

    /// <summary>
    /// Declares <see cref="Children" /> as one parallel timeline.
    /// </summary>
    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Parallel(Children);
    }
}
