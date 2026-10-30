using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

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
