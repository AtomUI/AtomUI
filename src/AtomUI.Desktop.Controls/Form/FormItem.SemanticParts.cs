using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "label",
    SelectorClass = "semantic-label",
    ContractType = typeof(AvaloniaTextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "extra",
    SelectorClass = "semantic-extra",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "help",
    SelectorClass = "semantic-help",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
[SemanticPart(
    "helpItem",
    SelectorClass = "semantic-help-item",
    SelectorRoute = "/template/ .semantic-help > .semantic-help-item",
    ContractType = typeof(AvaloniaTextBlock),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class FormItem
{
}
