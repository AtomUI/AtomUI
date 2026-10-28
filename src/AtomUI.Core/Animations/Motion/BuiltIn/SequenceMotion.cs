using Avalonia.Metadata;

namespace AtomUI.Animations;

public class SequenceMotion : AbstractMotion
{
    [Content]
    public IList<AbstractMotion> Children { get; } = new List<AbstractMotion>();

    public SequenceMotion()
    {
    }

    public SequenceMotion(params AbstractMotion[] children)
    {
        ArgumentNullException.ThrowIfNull(children);
        foreach (var child in children)
        {
            Children.Add(child);
        }
    }

    protected override void Build(MotionBuilder builder, in MotionContext context)
    {
        builder.Sequence(Children);
    }
}
