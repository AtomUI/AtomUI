using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// Each item laid out by the Space, one per entry of Children; the class is added at runtime.
// SpaceItemStyle applies to every item.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// A separator built from SplitTemplate and inserted between adjacent items while SplitTemplate is set.
// SpaceSeparatorStyle applies to every separator.
[SemanticPart(
    "separator",
    SelectorClass = "semantic-separator",
    SelectorRoute = "> .semantic-separator",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Space
{
}
