using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(ListViewItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "groupHeader",
    SelectorClass = "semantic-group-header",
    SelectorRoute = "> .semantic-group-header",
    ContractType = typeof(ListViewItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class ListView
{
}
