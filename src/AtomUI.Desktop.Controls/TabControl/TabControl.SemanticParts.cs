using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The region that holds the line tabs and the ink bar, padded by the header padding.
// TabControlHeaderStyle applies to it.
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// The ink bar that marks the selected tab. It lies over the tab area, the control places it, and it is hidden
// while the control has no items. TabControlIndicatorStyle applies to it.
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// The area that shows the content of the selected tab, inset by the content padding.
// TabControlContentStyle applies to it.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// One line tab, one element per item. The control creates a TabItem at run time for each item and marks it with
// the class, so the items are matched as direct children of the control and not through the template.
// TabControlItemStyle applies to each item.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(TabItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class TabControl
{
}
