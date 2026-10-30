using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-spinner >> .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = "/template/ .semantic-clear",
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
public partial class NumericUpDown
{
}
