using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Desktop.Controls;

// The prefix area in front of the selection. It presents the left add-on of the select.
// SelectPrefixStyle applies to it.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The selection area of the selector, between the prefix and the suffix. It holds the placeholder, the filter
// input and the selected options. SelectContentStyle applies to the panel.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-content",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The placeholder text in the selection area. It is visible only while the select reports that the placeholder
// should be shown. SelectPlaceholderStyle applies to it.
[SemanticPart(
    "placeholder",
    SelectorClass = "semantic-placeholder",
    SelectorRoute = "/template/ .semantic-content > .semantic-placeholder",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
// The filter text input in the selection area. In Single mode it is the input of the template. In the multiple
// and tag modes the search box is created at runtime and carries the same selector class.
// SelectInputStyle applies to both.
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    SelectorRoute = "/template/ .semantic-content > .semantic-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
// The suffix area behind the selection. It holds the max-count indicator, the right add-on and the handle with
// the arrow icon, the clear button and the form feedback. SelectSuffixStyle applies to the area.
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The clear button in the suffix handle. It is visible only when clearing is allowed, a value is selected and
// the pointer hovers or presses the selector. SelectClearStyle applies to it. The button is in the template of
// the nested handle, so the route crosses into that handle.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = ">> .semantic-scope-handle /template/ .semantic-clear",
    CrossNestedOwners = true,
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
// A selected option shown as a tag inside the selection area in the multiple and tag modes. One part exists per
// selected option and each is created at runtime inside the tags box, so the route crosses into the nested tags
// box. SelectItemStyle applies to every tag.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item",
    CrossNestedOwners = true,
    ContractType = typeof(Tag),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The text content of a selected tag. One part exists per tag and each is created at runtime inside the tag's
// template, so the route crosses into the nested tags box and the tag. SelectItemContentStyle applies to each
// text content.
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-content",
    CrossNestedOwners = true,
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
// The remove button of a selected tag. One part exists per tag and each is created at runtime inside the tag's
// template, so the route crosses into the nested tags box and the tag. SelectItemRemoveStyle applies to every
// remove button.
[SemanticPart(
    "itemRemove",
    SelectorClass = "semantic-item-remove",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-remove",
    CrossNestedOwners = true,
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
// The frame of the dropdown popup. It is in the popup's own visual root, so the route crosses visual roots.
// SelectPopupRootStyle applies to the frame.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = "/template/ .semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The option list inside the dropdown popup frame. It is created at runtime in the popup's own visual root, so
// the route crosses visual roots. SelectPopupListStyle applies to the list.
[SemanticPart(
    "popup.list",
    SelectorClass = "semantic-popup-list",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-list",
    CrossVisualRoot = true,
    ContractType = typeof(Control),
    RuntimeCreated = true,
    Since = "6.2.0")]
// An option row of the dropdown list. One part exists per option and each is created at runtime in the popup's
// own visual root, so the route crosses visual roots. SelectPopupListItemStyle applies to every row.
[SemanticPart(
    "popup.listItem",
    SelectorClass = "semantic-popup-list-item",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-list-item",
    CrossVisualRoot = true,
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
public partial class Select
{
}
