using AtomUI.Theme;
using Avalonia.Controls.Presenters;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "textarea",
    SelectorClass = "semantic-textarea",
    ContractType = typeof(TextPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear",
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
[SemanticPart(
    "count",
    SelectorClass = "semantic-count",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
public partial class TextArea
{
}
