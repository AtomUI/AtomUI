using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The close button at the end of the tab. It is shown only when the tab's IsClosable is true.
// TabStripItemCloseStyle applies to the button.
[SemanticPart(
    "close",
    SelectorClass = "semantic-close",
    ContractType = typeof(IconButton),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// The icon in front of the tab label, taken from the tab's Icon and IconTemplate. TabStripItemIconStyle applies to it.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// The label of the tab, taken from the tab's Content. TabStripItemLabelStyle applies to it.
[SemanticPart(
    "label",
    SelectorClass = "semantic-label",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
public partial class TabStripItem
{
}
