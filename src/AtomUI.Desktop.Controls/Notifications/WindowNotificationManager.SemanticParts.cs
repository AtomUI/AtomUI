using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The area that lays out and stacks all notification cards: the manager's template part PART_Items, a
// FeedbackStackPresenter exposed through the ItemsControl contract. Present once the manager's template is
// applied.
// The node belongs to the manager's own template, so it needs neither CrossVisualRoot nor RuntimeCreated.
// WindowNotificationManagerListContentStyle applies to it.
[SemanticPart(
    "listContent",
    SelectorClass = "semantic-list-content",
    ContractType = typeof(ItemsControl),
    Since = "6.2.0")]
public partial class WindowNotificationManager
{
}
