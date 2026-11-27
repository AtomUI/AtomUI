using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The prefix content at the left end of the input area, before the start date field.
// It presents the ContentLeftAddOn of the range picker. RangeDatePickerPrefixStyle applies to it.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The text box for the start date, left of the range arrow. It shows the start date as text.
// RangeDatePickerInputStyle applies to the text box.
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    SelectorRoute = "/template/ .semantic-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
// The text box for the end date, right of the range arrow. It shows the end date as text.
// RangeDatePickerSecondaryInputStyle applies to the text box.
[SemanticPart(
    "secondaryInput",
    SelectorClass = "semantic-secondary-input",
    SelectorRoute = "/template/ .semantic-secondary-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
// The suffix area at the right end of the input area. It holds the handle with the info icon or the clear
// button, and the ContentRightAddOn content of the range picker. RangeDatePickerSuffixStyle applies to the area.
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The clear button inside the handle of the suffix area. The handle swaps its info icon for this button while
// the clear button is visible, and its template creates the button, so the route enters that template.
// RangeDatePickerClearStyle applies to the button.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = ">> .semantic-scope-handle /template/ .semantic-clear",
    CrossNestedOwners = true,
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
// The root box of the open popup that holds the calendar panels and the optional time view, with the arrow that
// points at the input area. It exists only while the popup is open.
// The picker template builds it, but the popup hosts it in its own visual root, so the route crosses that root.
// RangeDatePickerPopupRootStyle applies to the box.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = "/template/ .semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(ArrowDecoratedBox),
    Since = "6.2.0")]
// The container inside the popup root that places the footer below the calendar panels and the time view.
// The presenter that owns it is created at runtime in the popup's visual root.
// RangeDatePickerPopupContainerStyle applies to the container.
[SemanticPart(
    "popup.container",
    SelectorClass = "semantic-popup-container",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-container",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
// The header above a calendar panel in the popup. The part repeats: there is one header per calendar panel.
// The headers are created at runtime by the range date viewer, a nested owner in the popup's visual root.
// RangeDatePickerPopupHeaderStyle applies to every header.
[SemanticPart(
    "popup.header",
    SelectorClass = "semantic-header",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-header",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
// The body of the range date viewer below its headers. It holds the calendar panels side by side.
// It is created at runtime by the viewer, a nested owner in the popup's visual root.
// RangeDatePickerPopupBodyStyle applies to the body.
[SemanticPart(
    "popup.body",
    SelectorClass = "semantic-body",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-body",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// A calendar panel in the body of the range date viewer. The part repeats: there is one per calendar shown.
// With two panels, the left one shows the calendar of the start date and the right one that of the end date.
// The panels are created at runtime by the viewer, a nested owner in the popup's visual root.
// RangeDatePickerPopupContentStyle applies to every panel.
[SemanticPart(
    "popup.content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-content",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
// One cell of a calendar panel in the popup. The part repeats: there is one for each cell of every calendar
// panel. The cells are created at runtime by the calendar panel, a nested owner in the popup's visual root.
// RangeDatePickerPopupCellStyle applies to every cell.
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
// RangeDatePickerPopupFooterStyle applies to the footer bar.
[SemanticPart(
    "popup.footer",
    SelectorClass = "semantic-popup-footer",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-footer",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(PixelAlignedBorder),
    Since = "6.2.0")]
public partial class RangeDatePicker
{
}
