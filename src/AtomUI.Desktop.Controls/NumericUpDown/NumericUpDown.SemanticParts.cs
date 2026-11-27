using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Desktop.Controls;

// The prefix inside the input frame, before the text. It presents the control's InnerLeftContent
// through InnerLeftContentTemplate and takes no space when there is no content.
// With Mode=Spinner and ShowButtonSpinner set it sits between the `-` and `+` handles.
// The template places it in the spinner's InnerLeftContent slot, which is why the route uses `>>`.
// NumericUpDownPrefixStyle applies to the prefix presenter.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-spinner >> .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The embedded text box. Its Text is two-way bound to the control's Text, and it is read-only while
// IsReadOnly is set.
// NumericUpDownInputStyle applies to the text box.
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
// The group docked to the right of the text box. It holds the clear button and the presenter that
// shows InnerRightContent through InnerRightContentTemplate.
// NumericUpDownSuffixStyle applies to the group.
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The clear button inside the suffix group. It is visible only while IsAllowClear is true, the control
// is not read-only and Text is not empty. Clicking it sets Value to null; focus returns to the text box.
// NumericUpDownClearStyle applies to the button.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = "/template/ .semantic-clear",
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
public partial class NumericUpDown
{
}
