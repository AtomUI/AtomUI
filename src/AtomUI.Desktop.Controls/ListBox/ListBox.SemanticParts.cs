using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// One entry row per ListBoxItem container, whether generated from the item source or supplied by the app.
// Generated containers are created at runtime, one per item.
// ListBoxItemStyle applies to every ListBoxItem container.
// The marker is added only when the owner's runtime type is exactly ListBox.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(ListBoxItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class ListBox
{
}
