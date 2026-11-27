using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Desktop.Controls;

// The leading add-on inside the search box frame, built from InnerLeftContentTemplate. It exists only
// while leading inner content is set. SearchEditPrefixStyle applies to the presenter.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The presenter that renders the typed text of the search box. It exists as long as the control
// template is applied. SearchEditInputStyle applies to the presenter.
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    ContractType = typeof(TextPresenter),
    Since = "6.2.0")]
// The trailing accessory panel inside the search box frame. It hosts the clear button and the other
// trailing accessories. SearchEditSuffixStyle applies to the panel.
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The clear button inside the trailing accessory panel. The panel creates it from its template only
// while IsAllowClear is set, and it is visible only while the text can be cleared.
// SearchEditClearStyle applies to the button.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear",
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
// The search button of the search box, placed inside the input frame. It is created at runtime, not by the
// control template. SearchEditButtonStyle applies to the button.
[SemanticPart(
    "button",
    SelectorClass = "semantic-button",
    SelectorRoute = "/template/ .semantic-scope-input-frame /template/ .semantic-button",
    ContractType = typeof(Button),
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class SearchEdit
{
}
