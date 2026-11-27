using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The list content: the area that stacks all message cards of the manager. It corresponds to the arrangement,
// order and alignment duty of the PART_Items stack presenter, and to the upstream list part, whose container is
// the implicit root of the manager. The stack is exposed through the ItemsControl contract, so the part needs no
// CrossVisualRoot or RuntimeCreated flag.
// WindowMessageManagerListContentStyle applies to the list content.
[SemanticPart(
    "listContent",
    SelectorClass = "semantic-list-content",
    ContractType = typeof(ItemsControl),
    Since = "6.2.0")]
public partial class WindowMessageManager
{
}
