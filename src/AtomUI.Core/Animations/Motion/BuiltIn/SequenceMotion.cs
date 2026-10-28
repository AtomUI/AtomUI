using Avalonia.Metadata;

namespace AtomUI.Animations;

public class SequenceMotion : Motion
{
    [Content]
    public IList<Motion> Children { get; } = new List<Motion>();

    public SequenceMotion()
    {
    }

    public SequenceMotion(params Motion[] children)
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
