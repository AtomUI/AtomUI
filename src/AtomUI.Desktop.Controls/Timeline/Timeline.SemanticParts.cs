using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// One node of the timeline: the TimelineItem container of each item, plus the pending item when one is set.
// Repeats once per item. The containers are created in code, and items supplied as TimelineItem receive the
// class when they are prepared. TimelineItemStyle applies to every item.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(TimelineItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The root panel of an item, which holds the section with the header, the indicator and the content.
// Repeats once per item. TimelineItemWrapperStyle applies to the wrapper of each item.
[SemanticPart(
    "itemWrapper",
    SelectorClass = "semantic-item-wrapper",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-wrapper",
    ContractType = typeof(Panel),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The center of an item's indicator: the dot, or the host of the custom indicator icon while one is set.
// The two are swapped by the icon-present state; both carry the selector class, so the style hits the visible one.
// Repeats once per item. TimelineItemIconStyle applies to the dot or to the icon host of each item.
[SemanticPart(
    "itemIcon",
    SelectorClass = "semantic-item-icon",
    SelectorRoute = "> .semantic-item /template/ .semantic-indicator /template/ .semantic-item-icon",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The section inside an item wrapper that arranges the header, the indicator and the content.
// Repeats once per item. TimelineItemSectionStyle applies to the section of each item.
[SemanticPart(
    "itemSection",
    SelectorClass = "semantic-item-section",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-section",
    ContractType = typeof(Panel),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The header stack of an item, which holds the item title.
// Repeats once per item. TimelineItemHeaderStyle applies to the header of each item.
[SemanticPart(
    "itemHeader",
    SelectorClass = "semantic-item-header",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-header",
    ContractType = typeof(StackPanel),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The title text of an item, bound to its Label. It is visible only while the Label is not null or empty.
// Repeats once per item. TimelineItemTitleStyle applies to the title of each item.
[SemanticPart(
    "itemTitle",
    SelectorClass = "semantic-item-title",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-title",
    ContractType = typeof(TextBlock),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The body of an item, which presents the item's Content.
// Repeats once per item. TimelineItemContentStyle applies to the content presenter of each item.
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = "> .semantic-item /template/ .semantic-item-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The tail line that joins an item's indicator to the indicator of the next item. It is not hit-testable.
// Repeats once per item. TimelineItemRailStyle applies to the rail of each item.
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
