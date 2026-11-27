using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The prefix before the input text. It presents the left add-on content of the picker.
// DatePickerPrefixStyle applies to the presenter.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The text input of the picker. It shows the selected date as text.
// DatePickerInputStyle applies to the input.
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    SelectorRoute = "/template/ .semantic-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
// The suffix after the input text. It holds the handle button with the info icon or the clear button, and the
// right add-on content of the picker.
// DatePickerSuffixStyle applies to the suffix area.
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The clear button inside the handle button at the end of the input area.
// The handle swaps its info icon for this button while the clear button is visible.
// The route enters the handle's template, because the button is part of that template.
// DatePickerClearStyle applies to the clear button.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = ">> .semantic-scope-handle /template/ .semantic-clear",
    CrossNestedOwners = true,
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
// The root box of the popup that holds the calendar.
// The picker template builds it, but the popup hosts it in its own visual root, so the route crosses that root.
// DatePickerPopupRootStyle applies to the box.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = "/template/ .semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(ArrowDecoratedBox),
    Since = "6.2.0")]
// The container inside the popup root that places the footer below the calendar and the time view.
// The presenter that owns it is created at runtime in the popup's visual root.
// DatePickerPopupContainerStyle applies to the container.
[SemanticPart(
    "popup.container",
    SelectorClass = "semantic-popup-container",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-container",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
// The header of the date viewer above the calendar in the popup. The viewer's header is optional.
// It is created at runtime by the viewer, a nested owner in the popup's visual root.
// DatePickerPopupHeaderStyle applies to the header.
[SemanticPart(
    "popup.header",
    SelectorClass = "semantic-header",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-header",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
// The body of the date viewer below its header. It holds the calendar panels.
// It is created at runtime by the viewer, a nested owner in the popup's visual root.
// DatePickerPopupBodyStyle applies to the body.
[SemanticPart(
    "popup.body",
    SelectorClass = "semantic-body",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-body",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// A calendar panel in the body of the date viewer, such as the date panel.
// It is created at runtime by the viewer, a nested owner in the popup's visual root.
// DatePickerPopupContentStyle applies to the panel.
[SemanticPart(
    "popup.content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-content",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
// One cell of the calendar. The part repeats: there is one for each cell of the calendar.
// The cells are created at runtime by the calendar panel, a nested owner in the popup's visual root.
// DatePickerPopupCellStyle applies to every cell.
[SemanticPart(
    "popup.cell",
    SelectorClass = "semantic-cell",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-cell",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
// The footer bar at the bottom of the popup. It holds the Now, Today and OK buttons.
// It is shown only while the buttons panel is visible.
// The presenter that owns it is created at runtime in the popup's visual root.
// DatePickerPopupFooterStyle applies to the footer bar.
[SemanticPart(
    "popup.footer",
    SelectorClass = "semantic-popup-footer",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-footer",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(PixelAlignedBorder),
    Since = "6.2.0")]
public partial class DatePicker
{
}
