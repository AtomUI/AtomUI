using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(TreeViewItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "itemSwitcher",
    SelectorClass = "semantic-item-switcher",
    SelectorRoute = "/template/ .semantic-scope-header /template/ .semantic-item-switcher",
    ContractType = typeof(ToggleButton),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "itemIcon",
    SelectorClass = "semantic-item-icon",
    SelectorRoute = "/template/ .semantic-scope-header /template/ .semantic-item-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "itemTitle",
    SelectorClass = "semantic-item-title",
    SelectorRoute = "/template/ .semantic-scope-header /template/ .semantic-item-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
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
