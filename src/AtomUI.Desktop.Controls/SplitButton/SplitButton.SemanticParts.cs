using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The button that triggers the main action of the split button. SplitButtonPrimaryStyle applies to the button.
// The semantic parts follow the semantic DOM of the upstream Dropdown. The five flyout-side parts (popup.root,
// itemTitle, item, itemContent and itemIcon) share the markers that the shared MenuFlyout and MenuItem inject at
// runtime, as DropdownButton does. The generated dedicated Styles keep owners apart along the logical ancestor
// chain: the Popup PlacementTarget is PART_SecondaryButton in the template, and its owner is the SplitButton.
// Unlike upstream, the AtomUI SplitButton is a composite control. Both trigger buttons live in its own template
// instead of being held by the consumer, so the trigger-side static parts primary and secondary are published
// as well, for developers to customize.
[SemanticPart(
    "primary",
    SelectorClass = "semantic-primary",
    SelectorRoute = "/template/ .semantic-primary",
    ContractType = typeof(Button),
    Since = "6.2.0")]
// The button that opens the flyout when it is pressed. SplitButtonSecondaryStyle applies to the button.
[SemanticPart(
    "secondary",
    SelectorClass = "semantic-secondary",
    SelectorRoute = "/template/ .semantic-secondary",
    ContractType = typeof(Button),
    Since = "6.2.0")]
// The framed root of the flyout popup. The flyout creates it at runtime in its own popup, outside the template of
// this control, so the route crosses the visual root. SplitButtonPopupRootStyle applies to the frame.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = ">> .semantic-popup-root",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ArrowDecoratedBox),
    Since = "6.2.0")]
// The title of an item group in the flyout, one per group. The group creates it at runtime inside the template
// of a nested owner and in a separate popup, so the route crosses the visual root and the owner boundary.
// SplitButtonItemTitleStyle applies to each title.
[SemanticPart(
    "itemTitle",
    SelectorClass = "semantic-item-title",
    SelectorRoute = ">> .semantic-item-title-group /template/ .semantic-item-title",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// A menu item of the flyout, one per entry. The flyout creates the items at runtime in its own popup, so the
// route crosses the visual root. SplitButtonItemStyle applies to every item.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-item",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(MenuItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// The content of a flyout item, shown by the presenter inside the item's own template.
// SplitButtonItemContentStyle applies to the presenter of every item.
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = ">> .semantic-item /template/ .semantic-item-content",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The icon of a flyout item, shown by the presenter inside the item's own template. SplitButtonItemIconStyle
// applies to the presenter of every item.
[SemanticPart(
    "itemIcon",
    SelectorClass = "semantic-item-icon",
    SelectorRoute = ">> .semantic-item /template/ .semantic-item-icon",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(IconPresenter),
    Since = "6.2.0")]
public partial class SplitButton
{
}
