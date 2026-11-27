using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// A checkable tag generated for one entry of the group's options, one per entry. Created at runtime by the
// items part of the group's template, which is why the route crosses the template.
// CheckableTagGroupItemStyle applies to every tag.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "/template/ .semantic-scope-items > .semantic-item",
    ContractType = typeof(CheckableTag),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class CheckableTagGroup
{
}
