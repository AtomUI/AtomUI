using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The message feature has two public owners, one for each level of the upstream semantic keys: the notice-level
// root, wrapper, icon and title belong to MessageCard, and the list-level list and listContent belong to
// WindowMessageManager, whose implicit root is the upstream list.
// The three parts below are static nodes of the ControlTheme of MessageCard. Their routes stay inside the default
// owner template boundary, so they need no CrossVisualRoot or RuntimeCreated flag.
//
// The wrapper: the panel inside the card frame that lays out the status icon and the message text.
// MessageCardWrapperStyle applies to the wrapper.
[SemanticPart(
    "wrapper",
    SelectorClass = "semantic-wrapper",
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
// The status icon at the left of the message text. It is collapsed while the Icon property is null.
// MessageCardIconStyle applies to the icon.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(IconPresenter),
    Since = "6.2.0")]
// The message text, which the user can select and copy. MessageCardTitleStyle applies to the text.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(Avalonia.Controls.SelectableTextBlock),
    Since = "6.2.0")]
public partial class MessageCard
{
}
