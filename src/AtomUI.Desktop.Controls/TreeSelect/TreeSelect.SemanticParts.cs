using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Desktop.Controls;

// The left add-on of the input. It presents ContentLeftAddOn through ContentLeftAddOnTemplate and is empty
// while ContentLeftAddOn is null. TreeSelectPrefixStyle applies to the presenter.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The selection area of the input, between the left add-on and the suffix. In single selection it shows the
// placeholder and the filter input; in multiple selection the placeholder and the tag box.
// TreeSelectContentStyle applies to the panel.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-content",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The placeholder text in the selection area, visible only while IsPlaceholderTextVisible is true.
// TreeSelectPlaceholderStyle applies to the text block.
[SemanticPart(
    "placeholder",
    SelectorClass = "semantic-placeholder",
    SelectorRoute = "/template/ .semantic-content > .semantic-placeholder",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
// The filter input of single selection (IsMultiple is false). It is hidden in multiple selection, where the
// tag box takes its place. TreeSelectInputStyle applies to the text box.
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    SelectorRoute = "/template/ .semantic-content > .semantic-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
// The strip at the right end of the input. It holds the indicator of SelectedCount against MaxCount (only
// while IsShowMaxCountIndicator is true), the right add-on, and the select handle with the drop-down
// indicator and the clear button. TreeSelectSuffixStyle applies to the strip.
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The clear button in the select handle at the right end of the input, beside the drop-down indicator. The
// handle's template creates it hidden, and the handle decides when it is shown; this control passes
// IsAllowClear and IsSelectionEmpty to the handle. TreeSelectClearStyle applies to the button.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = ">> .semantic-scope-handle /template/ .semantic-clear",
    CrossNestedOwners = true,
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
// A tag for one selected value in the tag box of multiple selection. The tag box creates the tags at
// runtime, one for each value it shows. TreeSelectItemStyle applies to every tag.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item",
    CrossNestedOwners = true,
    ContractType = typeof(Tag),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The label inside each selected-value tag, showing the tag text. It is created together with the tag.
// TreeSelectItemContentStyle applies to every label.
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-content",
    CrossNestedOwners = true,
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
// The remove button of each selected-value tag; activating it removes that value from the selection. It is
// present only while the tag's IsClosable is true. TreeSelectItemRemoveStyle applies to every button.
[SemanticPart(
    "itemRemove",
    SelectorClass = "semantic-item-remove",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-remove",
    CrossNestedOwners = true,
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
// The frame of the drop-down, shown only while the drop-down is open because it lives in the popup, outside
// this control's visual tree. The control sets its padding, minimum width and maximum height from
// PopupContentPadding, EffectivePopupWidth and MaxPopupHeight. TreeSelectPopupRootStyle applies to the frame.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = "/template/ .semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The tree inside the drop-down. It is created in code, not by the template, and lives in the popup outside
// this control's visual tree. TreeSelectPopupListStyle applies to the tree view.
[SemanticPart(
    "popup.list",
    SelectorClass = "semantic-popup-list",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-list",
    CrossVisualRoot = true,
    ContractType = typeof(Control),
    RuntimeCreated = true,
    Since = "6.2.0")]
// One node row of the drop-down tree, at any nesting depth. The containers are created in code as the tree
// needs them. TreeSelectPopupListItemStyle applies to every row container.
[SemanticPart(
    "popup.listItem",
    SelectorClass = "semantic-popup-list-item",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-list-item",
    CrossVisualRoot = true,
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
public partial class TreeSelect
{
}
