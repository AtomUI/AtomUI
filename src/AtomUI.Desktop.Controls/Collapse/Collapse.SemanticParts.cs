using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The header bar of a panel item, which holds the expand icon, the title and the add-on content.
// One per item, in a container that the control creates at runtime.
// CollapseHeaderStyle applies to the header of every item.
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    SelectorRoute = "> .semantic-scope-item /template/ .semantic-header",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The expand icon button in the header of a panel item, which toggles the panel.
// One per item, in a container that the control creates at runtime.
// It is visible only while the item shows its expand icon.
// CollapseIconStyle applies to the button of every item.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    SelectorRoute = "> .semantic-scope-item /template/ .semantic-icon",
    ContractType = typeof(IconButton),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The title of a panel item, which presents the header of the item.
// One per item, in a container that the control creates at runtime.
// CollapseTitleStyle applies to the title of every item.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    SelectorRoute = "> .semantic-scope-item /template/ .semantic-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The content area of a panel item, below its header.
// One per item, in a container that the control creates at runtime.
// It is visible only while the item is expanded.
// CollapseBodyStyle applies to the content area of every item.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    SelectorRoute = "> .semantic-scope-item /template/ .semantic-body",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Collapse
{
}
