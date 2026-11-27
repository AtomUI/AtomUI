using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// A child node directly under this node. The child nodes are created at run time from the items, one per item.
// TreeViewItemItemStyle applies to every child node.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(TreeViewItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The expand and collapse button at the start of a node header. One per node, created at run time with the node.
// TreeViewItemItemSwitcherStyle applies to every switcher.
[SemanticPart(
    "itemSwitcher",
    SelectorClass = "semantic-item-switcher",
    SelectorRoute = "/template/ .semantic-scope-header /template/ .semantic-item-switcher",
    ContractType = typeof(ToggleButton),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The icon in a node header. One per node, created at run time with the node; hidden when the node has no icon.
// TreeViewItemItemIconStyle applies to every icon.
[SemanticPart(
    "itemIcon",
    SelectorClass = "semantic-item-icon",
    SelectorRoute = "/template/ .semantic-scope-header /template/ .semantic-item-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The title of a node in its header. One per node, created at run time with the node.
// TreeViewItemItemTitleStyle applies to every title.
[SemanticPart(
    "itemTitle",
    SelectorClass = "semantic-item-title",
    SelectorRoute = "/template/ .semantic-scope-header /template/ .semantic-item-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The check indicator of a node: a check box or a radio button, both in the header and hidden by default.
// The node shows the one that its toggle type selects.
// TreeViewItemItemIndicatorStyle applies to the indicator that is shown.
[SemanticPart(
    "itemIndicator",
    SelectorClass = "semantic-item-indicator",
    SelectorRoute = "/template/ .semantic-scope-header /template/ .semantic-item-indicator",
    ContractType = typeof(ToggleButton),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class TreeViewItem
{
}
