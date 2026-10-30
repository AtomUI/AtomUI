using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "add",
    SelectorClass = "semantic-add",
    ContractType = typeof(IconButton),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(TabStripItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class CardTabStrip
{
}
