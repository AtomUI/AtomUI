using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The region that holds the card tabs and the add-tab button, padded by the header padding.
// CardTabControlHeaderStyle applies to it.
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// The plus button that follows the last card tab. It is visible only while IsShowAddTabButton is true, and a
// click raises AddTabRequest. CardTabControlAddStyle applies to the button.
[SemanticPart(
    "add",
    SelectorClass = "semantic-add",
    ContractType = typeof(IconButton),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// The area that shows the content of the selected tab, inset by the content padding.
// CardTabControlContentStyle applies to it.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// One card tab, one element per item. The control creates a TabItem at run time for each item and marks it with
// the class, so the items are matched as direct children of the control and not through the template.
// CardTabControlItemStyle applies to each item.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(TabItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class CardTabControl
{
}
