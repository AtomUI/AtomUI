using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "extra",
    SelectorClass = "semantic-extra",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "label",
    SelectorClass = "semantic-label",
    SelectorRoute = "/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-label",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-scope-items > .semantic-scope-item /template/ .semantic-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Descriptions
{
}
