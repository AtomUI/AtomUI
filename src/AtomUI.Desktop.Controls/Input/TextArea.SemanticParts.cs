using AtomUI.Theme;
using Avalonia.Controls.Presenters;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Desktop.Controls;

// The presenter that renders the typed text of the multi-line text area. It exists as long as the control
// template is applied. TextAreaTextareaStyle applies to the presenter.
[SemanticPart(
    "textarea",
    SelectorClass = "semantic-textarea",
    ContractType = typeof(TextPresenter),
    Since = "6.2.0")]
// The clear button in the trailing accessory area of the text area. The accessory panel creates it from
// its template only while IsAllowClear is set, and it is visible only while the text can be cleared.
// TextAreaClearStyle applies to the button.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear",
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
// The character counter of the text area. The accessory dock panel creates it from its template only
// while IsShowCount is true. TextAreaCountStyle applies to the text block.
[SemanticPart(
    "count",
    SelectorClass = "semantic-count",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
public partial class TextArea
{
}
