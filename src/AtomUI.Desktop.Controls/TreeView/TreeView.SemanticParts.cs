using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// A root node of the tree view. The nodes are created at run time from the items, one per root-level item.
// TreeViewItemStyle applies to every root node.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(TreeViewItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class TreeView
{
}
