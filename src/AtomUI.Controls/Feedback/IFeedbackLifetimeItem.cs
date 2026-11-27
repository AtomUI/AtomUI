namespace AtomUI.Controls;

/// <summary>
/// Item that can expire independently of its presentation.
/// </summary>
internal interface IFeedbackLifetimeItem
{
    bool IsClosing { get; }
    bool IsClosed { get; }

    void RequestClose();
    void UpdateRemaining(TimeSpan remaining);
}

/// <summary>
/// Optional progress reporting that wakes the scheduler before the deadline.
/// </summary>
internal interface IFeedbackProgressItem
{
    bool IsProgressUpdateNeeded { get; }
}

internal interface IFeedbackClock
{
    TimeSpan Now { get; }
}

internal interface IFeedbackWakeup : IDisposable
{
    void Schedule(TimeSpan due, Action callback);
    void Cancel();
}
