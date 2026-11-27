namespace AtomUI.Animations;

/// <summary>
/// Lets a control animate the space it occupies while a motion with a layout transition plays on it.
/// </summary>
/// <remarks>
/// A motion that declares <see cref="MotionBuilder.Expand" /> or <see cref="MotionBuilder.Collapse" /> can only be
/// played on a target that implements this interface. Playback creates the leases and disposes them when the motion
/// ends, is canceled or is replaced.
/// </remarks>
public interface IMotionLayout
{
    /// <summary>
    /// Takes over the layout of the control for one layout transition.
    /// </summary>
    /// <param name="direction">
    /// The side of the control where space is added or removed. <c>Top</c> and <c>Bottom</c> change the height,
    /// <c>Left</c> and <c>Right</c> the width. The content stays attached to the opposite edge.
    /// </param>
    /// <param name="expanded">
    /// <see langword="true" /> when the transition ends with the content fully shown. <see langword="false" /> when it
    /// ends with the content taking no space.
    /// </param>
    /// <returns>
    /// The lease that receives the progress of the transition. It stays in charge until it is disposed. A new lease can
    /// be created before the previous one is disposed.
    /// </returns>
    IMotionLayoutLease BeginLayoutMotion(Direction direction, bool expanded);
}

/// <summary>
/// The handle of one layout transition on an <see cref="IMotionLayout" /> control.
/// </summary>
/// <remarks>
/// Playback reports the progress until the lease is disposed. Disposing the lease ends the transition and lets the
/// control lay itself out on its own again.
/// </remarks>
public interface IMotionLayoutLease : IDisposable
{
    /// <summary>
    /// Moves the layout of the control to the given position within the transition.
    /// </summary>
    /// <param name="progress">
    /// The progress of the transition. 0 is the state in which it began and 1 is its end, expanded or collapsed as
    /// <see cref="IMotionLayout.BeginLayoutMotion" /> requested.
    /// </param>
    void SetProgress(double progress);
}

internal readonly record struct MotionLayoutSegment(Direction Direction, bool Expanded, TimeSpan Start, TimeSpan Duration)
{
    internal MotionLayoutSegment At(TimeSpan start, TimeSpan duration) => this with { Start = start, Duration = duration };
}
