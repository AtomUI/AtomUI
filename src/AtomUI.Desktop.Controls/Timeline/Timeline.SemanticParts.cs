using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(TimelineItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "itemWrapper",
    SelectorClass = "semantic-item-wrapper",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-wrapper",
    ContractType = typeof(Panel),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "itemIcon",
    SelectorClass = "semantic-item-icon",
    SelectorRoute = "> .semantic-item /template/ .semantic-indicator /template/ .semantic-item-icon",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "itemSection",
    SelectorClass = "semantic-item-section",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-section",
    ContractType = typeof(Panel),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "itemHeader",
    SelectorClass = "semantic-item-header",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-header",
    ContractType = typeof(StackPanel),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "itemTitle",
    SelectorClass = "semantic-item-title",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-title",
    ContractType = typeof(TextBlock),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "itemRail",
    SelectorClass = "semantic-item-rail",
    SelectorRoute = "> .semantic-item /template/ .semantic-indicator /template/ .semantic-item-rail",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Timeline
{
}
