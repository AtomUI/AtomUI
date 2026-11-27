using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// One step of the steps control: the container that holds the indicator, the texts and the connector of the step.
// The steps control creates the containers at run time for its items, one per item.
// StepsItemStyle applies to every step.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(StepsItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The frame behind a step, which paints the background, border and corner radius of the step: the status
// panel of a panel step, the hover fill of an inline step and the focus highlight. It takes no pointer input.
// The steps control creates the steps at run time, so there is one frame per item.
// StepsItemWrapperStyle applies to every frame.
[SemanticPart(
    "itemWrapper",
    SelectorClass = "semantic-item-wrapper",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-wrapper",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The indicator of the step: the step number, a custom icon, a dot or the finish or error mark, with the
// circular progress of the current step. A panel step does not show it.
// The steps control creates the steps at run time, so there is one indicator per item.
// StepsItemIconStyle applies to every indicator.
[SemanticPart(
    "itemIcon",
    SelectorClass = "semantic-item-icon",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-icon",
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The title of the step, which presents the header; a string header is shown as a wrapping text block.
// The steps control creates the steps at run time, so there is one title per item.
// StepsItemTitleStyle applies to every title.
[SemanticPart(
    "itemTitle",
    SelectorClass = "semantic-item-title",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The subtitle of the step, which presents the sub-header. It is visible only while the sub-header is not null.
// The steps control creates the steps at run time, so there is one subtitle per item.
// StepsItemSubtitleStyle applies to every subtitle.
[SemanticPart(
    "itemSubtitle",
    SelectorClass = "semantic-item-subtitle",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-subtitle",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The text block of the step, which groups the title, the subtitle and the description.
// The steps control creates the steps at run time, so there is one text block per item.
// StepsItemSectionStyle applies to every text block.
[SemanticPart(
    "itemSection",
    SelectorClass = "semantic-item-section",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-section",
    ContractType = typeof(Panel),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The description of the step, which presents the content of the step; a string is shown as a wrapping text
// block. An inline step does not show it.
// The steps control creates the steps at run time, so there is one description per item.
// StepsItemContentStyle applies to every description.
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The connector line that joins this step to the next one. It is not shown after the last step, nor in
// navigation and panel steps.
// The steps control creates the steps at run time, so there is one connector per item.
// StepsItemRailStyle applies to every connector.
[SemanticPart(
    "itemRail",
    SelectorClass = "semantic-item-rail",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-rail",
    ContractType = typeof(DashedBorder),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Steps
{
}
