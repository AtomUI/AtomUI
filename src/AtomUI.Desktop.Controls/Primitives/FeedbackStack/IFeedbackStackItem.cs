using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>Internal lifecycle contract shared by transient feedback surfaces.</summary>
internal interface IFeedbackStackItem
{
    bool IsClosing { get; }
    bool IsClosed { get; }
    bool IsProgressVisible { get; }
    bool IsStackVisible { get; set; }

    void RequestClose();
    void UpdateRemaining(TimeSpan remaining);
}

/// <summary>
/// Optional one-shot content snapshot capability used by feedback cards during an expanded-to-collapsed transition.
/// </summary>
internal interface IFeedbackStackTransitionSnapshotItem
{
    bool TryBeginStackCollapseSnapshot();
    void ArmStackCollapseSnapshot(ITransform targetTransform);
    void ReleaseStackTransitionSnapshot();
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
