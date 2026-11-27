using Avalonia.Animation;

namespace AtomUI.Animations;

/// <summary>
/// Describes the end of one run of a transition that reports its completion through
/// <see cref="INotifyTransitionCompleted" />.
/// </summary>
public class TransitionCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Whether the transition run reached the end value of the transition.
    /// </summary>
    public bool Status { get; }

    /// <summary>
    /// Creates the arguments for a transition run.
    /// </summary>
    /// <param name="status">
    /// Whether the run reached the end value of the transition.
    /// </param>
    public TransitionCompletedEventArgs(bool status)
    {
        Status = status;
    }
}

/// <summary>
/// A transition that reports when one of its runs finishes.
/// </summary>
public interface INotifyTransitionCompleted : ITransition, IDisposable
{
    /// <summary>
    /// Reports whether a transition run reached the end value of the transition.
    /// </summary>
    public IObservable<bool> CompletedObservable { get; }
    /// <summary>
    /// Raised when a transition run finishes; <see cref="TransitionCompletedEventArgs.Status" /> tells whether
    /// it reached the end value.
    /// </summary>
    public event EventHandler<TransitionCompletedEventArgs>? TransitionCompleted;
    /// <summary>
    /// The duration of one run of the transition.
    /// </summary>
    public TimeSpan Duration { get; }
}