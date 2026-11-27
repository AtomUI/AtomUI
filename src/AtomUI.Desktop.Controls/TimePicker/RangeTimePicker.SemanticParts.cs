using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

using AvaloniaListBoxItem = Avalonia.Controls.ListBoxItem;

namespace AtomUI.Desktop.Controls;

// The prefix content at the left end of the input area. It is empty unless ContentLeftAddOn is set.
// RangeTimePickerPrefixStyle applies to it.
[SemanticPart("prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The text box for the start time, left of the range arrow.
// RangeTimePickerInputStyle applies to the text box.
[SemanticPart("input",
    SelectorClass = "semantic-input",
    SelectorRoute = "/template/ .semantic-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
// The text box for the end time, right of the range arrow.
// RangeTimePickerSecondaryInputStyle applies to the text box.
[SemanticPart("secondaryInput",
    SelectorClass = "semantic-secondary-input",
    SelectorRoute = "/template/ .semantic-secondary-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
// The suffix area at the right end of the input area.
// It holds the info and clear handle and the ContentRightAddOn content.
// RangeTimePickerSuffixStyle applies to the area.
[SemanticPart("suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The clear button inside the info and clear handle of the suffix area. The handle's template creates it.
// RangeTimePickerClearStyle applies to the button.
[SemanticPart("clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = ">> .semantic-scope-handle /template/ .semantic-clear",
    CrossNestedOwners = true,
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
// The frame of the open popup around the time panel. It exists only while the popup is open.
// RangeTimePickerPopupRootStyle applies to the frame.
[SemanticPart("popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = "/template/ .semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(ArrowDecoratedBox),
    Since = "6.2.0")]
// The main layout of the open popup panel inside the popup frame. It holds the footer and the time columns.
// It is created at runtime together with the panel presenter. RangeTimePickerPopupContainerStyle applies to it.
[SemanticPart("popup.container",
    SelectorClass = "semantic-popup-container",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-container",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
// The grid inside the open popup panel that arranges the time selection columns. It is created at runtime.
// RangeTimePickerPopupContentStyle applies to the grid.
[SemanticPart("popup.content",
    SelectorClass = "semantic-time-content",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-time-content",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(Grid),
    Since = "6.2.0")]
// One host panel per time selection column: hour, minute, second or the AM/PM period. Created at runtime.
// RangeTimePickerPopupColumnStyle applies to every column.
[SemanticPart("popup.column",
    SelectorClass = "semantic-time-column",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-time-column",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// One row per selectable option in a time selection column. Created at runtime.
// RangeTimePickerPopupItemStyle applies to every row.
[SemanticPart("popup.item",
    SelectorClass = "semantic-time-item",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-time-item",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(AvaloniaListBoxItem),
    Since = "6.2.0")]
// The bar at the bottom of the open popup panel that holds the Now and OK buttons. Created at runtime.
// It is visible only when the presenter shows its buttons panel.
// RangeTimePickerPopupFooterStyle applies to the bar.
[SemanticPart("popup.footer",
    SelectorClass = "semantic-popup-footer",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-footer",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(PixelAlignedBorder),
    Since = "6.2.0")]
public partial class RangeTimePicker
{
}
