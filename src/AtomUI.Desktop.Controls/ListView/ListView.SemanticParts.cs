using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// One data row of the list. The list creates the containers at run time, one for each data item, so the
// part repeats per row.
// ListViewItemStyle applies to every row.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(ListViewItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The heading row of a group of items. It exists only while the list shows grouped data: the list creates
// one container at run time for each item marked as a group heading.
// ListViewGroupHeaderStyle applies to every group heading.
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
