using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// One tab of the strip, one element per item. The strip creates a TabStripItem at run time for each item and
// marks it with the class, so the items are matched as direct children of the strip and not through the template.
// TabStripItemStyle applies to each item.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(TabStripItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class TabStrip
{
}
