using Avalonia.Animation;

namespace AtomUI.Animations;

public class TransitionCompletedEventArgs : EventArgs
{
    public bool Status { get; }

    public TransitionCompletedEventArgs(bool status)
    {
        Status = status;
    }
}

public interface INotifyTransitionCompleted : ITransition
{
    public IObservable<bool> CompletedObservable { get; }
    public event EventHandler<TransitionCompletedEventArgs>? TransitionCompleted;
    public TimeSpan Duration { get; }
}