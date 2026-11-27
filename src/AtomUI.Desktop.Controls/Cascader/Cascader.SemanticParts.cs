using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Desktop.Controls;

// The left add-on area of the input box, which presents the ContentLeftAddOn of the control.
// CascaderPrefixStyle applies to the presenter.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The panel inside the input box that stacks the placeholder, the selected path of a single selection and
// the tag box of a multiple selection. The control switches their visibility by mode.
// CascaderContentStyle applies to the panel.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The placeholder text in the content panel. It is visible only while IsPlaceholderTextVisible is true.
// CascaderPlaceholderStyle applies to the text block.
[SemanticPart(
    "placeholder",
    SelectorClass = "semantic-placeholder",
    SelectorRoute = "/template/ .semantic-content > .semantic-placeholder",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
// The filter text box that takes the place of the selected path in a single selection while filtering is
// enabled; it is collapsed in every other mode. CascaderInputStyle applies to the text box.
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    SelectorRoute = "/template/ .semantic-content > .semantic-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
// The area at the right end of the input box, which holds the max count indicator, the right add-on content
// and the handle with the clear, loading and open indicators. CascaderSuffixStyle applies to the stack panel.
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The clear button inside the handle at the right end of the input box. The handle is a nested owner with
// its own template, so the selector crosses owners. CascaderClearStyle applies to the button.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = ">> .semantic-scope-handle /template/ .semantic-clear",
    CrossNestedOwners = true,
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
// One tag of a selected option in the tag box of a multiple selection. One part for each selected option
// that is shown as a tag, created at runtime by the tag box, which is a nested owner.
// CascaderItemStyle applies to every tag.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item",
    CrossNestedOwners = true,
    ContractType = typeof(Tag),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The label of one selected tag. One part per tag, created at runtime together with the tag inside the
// template of the tag. CascaderItemContentStyle applies to the text of every tag.
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-content",
    CrossNestedOwners = true,
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
// The remove button of one selected tag. One part for each tag that offers a remove button, created at
// runtime together with the tag inside the template of the tag. CascaderItemRemoveStyle applies to the button.
[SemanticPart(
    "itemRemove",
    SelectorClass = "semantic-item-remove",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-remove",
    CrossNestedOwners = true,
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
// The root frame of the dropdown popup. It belongs to the popup, which is a visual root of its own.
// CascaderPopupRootStyle applies to the frame.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// One column of options in the dropdown popup, one for each level that is shown. The columns are created at
// runtime inside the template of the nested view, which sits in the popup's visual root.
// CascaderPopupListStyle applies to every column.
[SemanticPart(
    "popup.list",
    SelectorClass = "semantic-popup-list",
    SelectorRoute = "/template/ .semantic-popup-root > .semantic-scope-view /template/ .semantic-scope-frame >> .semantic-popup-list",
    CrossVisualRoot = true,
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
// One option row of a column in the dropdown popup. The rows are created at runtime for the options of each
// column, inside the popup's visual root. CascaderPopupListItemStyle applies to every row.
[SemanticPart(
    "popup.listItem",
    SelectorClass = "semantic-popup-list-item",
    SelectorRoute = "/template/ .semantic-popup-root > .semantic-scope-view /template/ .semantic-scope-frame >> .semantic-popup-list-item",
    CrossVisualRoot = true,
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
public partial class Cascader
{
}
