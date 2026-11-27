using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Desktop.Controls;

// The leading add-on inside the input frame, built from InnerLeftContentTemplate. It exists only while
// leading inner content is set. LineEditPrefixStyle applies to the presenter.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The presenter that renders the typed text of the line editor. It exists as long as the control
// template is applied. LineEditInputStyle applies to the presenter.
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    ContractType = typeof(TextPresenter),
    Since = "6.2.0")]
// The trailing accessory panel inside the input frame. It hosts the clear button, the inner trailing
// add-on and the character counter. LineEditSuffixStyle applies to the panel.
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The clear button inside the trailing accessory panel. The panel creates it from its template only
// while IsAllowClear is set, and it is visible only while the text can be cleared.
// LineEditClearStyle applies to the button.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear",
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
// The character counter inside the trailing accessory panel. The panel creates it from its template
// only while IsShowCount is true. LineEditCountStyle applies to the text block.
[SemanticPart(
    "count",
    SelectorClass = "semantic-count",
    SelectorRoute = "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-count",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
public partial class LineEdit
{
}
