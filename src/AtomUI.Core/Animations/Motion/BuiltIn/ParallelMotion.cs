using Avalonia.Metadata;

namespace AtomUI.Animations;

public class ParallelMotion : AbstractMotion
{
    [Content]
    public IList<AbstractMotion> Children { get; } = new List<AbstractMotion>();

    public ParallelMotion()
    {
    }

    public ParallelMotion(params AbstractMotion[] children)
    {
        ArgumentNullException.ThrowIfNull(children);
        foreach (var child in children)
        {
            Children.Add(child);
        }
    }

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Parallel(Children);
    }
}
