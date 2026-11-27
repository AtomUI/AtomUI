using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

using AvaloniaListBoxItem = Avalonia.Controls.ListBoxItem;

namespace AtomUI.Desktop.Controls;

// The prefix area at the start of the input text box. TimePickerPrefixStyle applies to the prefix presenter.
[SemanticPart("prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The text box the user types the time into. TimePickerInputStyle applies to the text box.
[SemanticPart("input",
    SelectorClass = "semantic-input",
    SelectorRoute = "/template/ .semantic-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
// The suffix area at the end of the input text box. TimePickerSuffixStyle applies to the suffix panel.
[SemanticPart("suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The clear button of the input, which empties the selected time. It is shown only while a time is selected.
// TimePickerClearStyle applies to the button, which lives in the template of the handle, a nested owner.
[SemanticPart("clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = ">> .semantic-scope-handle /template/ .semantic-clear",
    CrossNestedOwners = true,
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
// The root frame of the popup, with its arrow. It lives in the separate popup visual root.
// TimePickerPopupRootStyle applies to the frame.
[SemanticPart("popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = "/template/ .semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(ArrowDecoratedBox),
    Since = "6.2.0")]
// The main layout of the popup, which holds the time columns and the footer bar. It comes from the
// template of the presenter the picker creates in code and is reached across the popup's visual root.
// TimePickerPopupContainerStyle applies to the main layout panel.
[SemanticPart("popup.container",
    SelectorClass = "semantic-popup-container",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-container",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
// The area of the popup that holds the hour, minute, second and AM/PM columns, above the footer bar.
// TimePickerPopupContentStyle applies to this area. It lies in the time view of the presenter the picker
// creates in code and is reached across the popup's visual root.
[SemanticPart("popup.content",
    SelectorClass = "semantic-time-content",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-time-content",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(Grid),
    Since = "6.2.0")]
// A column of the popup: hour, minute, second or AM/PM. The part repeats once per column, so
// TimePickerPopupColumnStyle applies to every column. The columns lie in the time view of the presenter
// the picker creates in code and are reached across the popup's visual root.
[SemanticPart("popup.column",
    SelectorClass = "semantic-time-column",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-time-column",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// One selectable option of a column, such as a single hour. The part repeats once per option, so
// TimePickerPopupItemStyle applies to every option. The cells are created in code and TimeViewCell adds
// the part class to each; they are reached across the popup's visual root.
[SemanticPart("popup.item",
    SelectorClass = "semantic-time-item",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-time-item",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(AvaloniaListBoxItem),
    Since = "6.2.0")]
// The bar at the bottom of the popup that holds the Now and OK buttons. It is visible only while
// IsNeedConfirm or IsShowNow asks for one of them. It comes from the template of the presenter created in
// code and is reached across the popup's visual root. TimePickerPopupFooterStyle applies to the bar.
[SemanticPart("popup.footer",
    SelectorClass = "semantic-popup-footer",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-footer",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(PixelAlignedBorder),
    Since = "6.2.0")]
public partial class TimePicker
{
}
