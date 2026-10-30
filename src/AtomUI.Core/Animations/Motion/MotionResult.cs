namespace AtomUI.Animations;

public enum MotionOutcome
{
    Completed,
    Superseded,
    Cancelled,
    Detached,
    NotReady
}

public readonly record struct MotionResult(MotionOutcome Outcome, long RequestId, bool WasAnimated)
{
    public bool IsCompleted => Outcome == MotionOutcome.Completed;
}
