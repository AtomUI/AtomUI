using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The clear button at the right end of the field, just before the dropdown arrow. It is visible only while
// IsAllowClear and CanClearValue are true, the control is enabled, and the pointer is over the control or
// focus is within it. ComboBoxClearStyle applies to the button.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-scope-actions > .semantic-scope-clear > .semantic-clear",
    ContractType = typeof(IconButton),
    Since = "6.2.14")]
// The prefix area inside the frame, to the left of the selected value or the typed text. It shows the
// ContentLeftAddOn content. ComboBoxPrefixStyle applies to it.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The frame around the field content. It paints the border, background, shadow and corner radius around the
// prefix, the selected value and the suffix; the outer add-ons are docked outside it.
// The ComboBox template does not create it: it belongs to the template of the AddOnDecoratedBox that the
// ComboBox template uses, so the route crosses that nested owner. ComboBoxFrameStyle applies to it.
[SemanticPart(
    "frame",
    SelectorClass = "semantic-frame",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-frame",
    CrossNestedOwners = true,
    ContractType = typeof(PixelAlignedBorder),
    Since = "6.2.0")]
// The area between the prefix and the suffix that shows the selected value. It stacks the placeholder text,
// the selected item and the editable text box, and switches their visibility. ComboBoxContentStyle applies
// to the panel.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-content",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The placeholder text shown while no item is selected. It is hidden when IsEditable is true, because the
// editable text box then draws the placeholder itself. ComboBoxPlaceholderStyle applies to this text block.
// The ContractType names the Avalonia base type on purpose: this namespace also declares AtomUI's own
// TextBox, TextBlock and ScrollViewer, and the input and popup.list parts do the same.
[SemanticPart(
    "placeholder",
    SelectorClass = "semantic-placeholder",
    SelectorRoute = "/template/ .semantic-content > .semantic-placeholder",
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
// The text box that accepts typed text. It is visible only while IsEditable is true; the placeholder text
// block and the selected item are then hidden. ComboBoxInputStyle applies to the text box.
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    SelectorRoute = "/template/ .semantic-content > .semantic-input",
    ContractType = typeof(Avalonia.Controls.TextBox),
    Since = "6.2.0")]
// The suffix area inside the frame, to the right of the selected value or the typed text. It holds the
// ContentRightAddOn content, the form feedback and the action group with the clear button and the dropdown
// handle. ComboBoxSuffixStyle applies to the panel.
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The arrow button that opens the dropdown. The ComboBox template does not create it: it belongs to the
// template of the dropdown handle that the ComboBox template creates, so the route crosses that nested
// owner. ComboBoxIndicatorStyle applies to the button.
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    SelectorRoute = ">> .semantic-scope-handle /template/ .semantic-indicator",
    CrossNestedOwners = true,
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
// The border that forms the dropdown window. Its padding, maximum height and minimum width bound the option
// list and the empty state. It lives in the popup, in a visual root separate from the ComboBox template.
// ComboBoxPopupRootStyle applies to it.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = "/template/ .semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The scrolling viewport that holds the option rows of the dropdown. It is hidden while the empty state is
// shown. It lives in the popup, in a visual root separate from the ComboBox template.
// ComboBoxPopupListStyle applies to the scroll viewer.
[SemanticPart(
    "popup.list",
    SelectorClass = "semantic-popup-list",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-list",
    CrossVisualRoot = true,
    ContractType = typeof(Avalonia.Controls.ScrollViewer),
    Since = "6.2.0")]
// The empty state of the dropdown, shown in place of the option list while IsEffectiveEmptyVisible is true.
// It shows EmptyIndicator with EmptyIndicatorTemplate, or the default Empty image. It lives in the popup,
// in a visual root separate from the ComboBox template. ComboBoxPopupEmptyStyle applies to it.
[SemanticPart(
    "popup.empty",
    SelectorClass = "semantic-popup-empty",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-empty",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// One option row of the dropdown list. The list creates one part per option at run time, in the popup's
// visual root, which is separate from the ComboBox template. ComboBoxPopupListItemStyle applies to every row.
[SemanticPart(
    "popup.listItem",
    SelectorClass = "semantic-popup-list-item",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-list-item",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(ComboBoxItem),
    Since = "6.2.0")]
public partial class ComboBox
{
}
