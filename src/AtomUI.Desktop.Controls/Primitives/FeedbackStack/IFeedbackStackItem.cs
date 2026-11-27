using AtomUI.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Desktop feedback cards add stack visibility to the shared lifetime contract.
/// </summary>
internal interface IFeedbackStackItem : IFeedbackLifetimeItem, IFeedbackProgressItem
{
    bool IsProgressVisible { get; }
    bool IsStackVisible { get; set; }

    bool IFeedbackProgressItem.IsProgressUpdateNeeded => IsProgressVisible && IsStackVisible;
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
