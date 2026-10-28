namespace AtomUI.Animations;

public interface IMotionLayout
{
    IMotionLayoutLease BeginLayoutMotion(Direction direction, bool expanded);
}

public interface IMotionLayoutLease : IDisposable
{
    void SetProgress(double progress);
}

internal readonly record struct MotionLayoutSegment(Direction Direction, bool Expanded, TimeSpan Start, TimeSpan Duration)
{
    internal MotionLayoutSegment At(TimeSpan start, TimeSpan duration) => this with { Start = start, Duration = duration };
}
