using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The bar above the items that holds the title and the extra area. It is shown only while the Header or the Extra
// property is set. DescriptionsHeaderStyle applies to this bar.
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
// The title at the left of the header, which presents the Header property. DescriptionsTitleStyle applies to it.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The extra area at the right of the header, which presents the Extra property. DescriptionsExtraStyle applies to it.
[SemanticPart(
    "extra",
    SelectorClass = "semantic-extra",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The label of one item. The items are created in code for each DescriptionItem, and the label presenter lives in the
// item's own template. DescriptionsLabelStyle applies to the label of every item.
[SemanticPart(
    "label",
    SelectorClass = "semantic-label",
    SelectorRoute = "/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-label",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The content of one item. The items are created in code for each DescriptionItem, and the content presenter lives in
// the item's own template. DescriptionsContentStyle applies to the content of every item.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Descriptions
{
}
