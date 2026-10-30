using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    SelectorRoute = "> .semantic-scope-item /template/ .semantic-header",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    SelectorRoute = "> .semantic-scope-item /template/ .semantic-icon",
    ContractType = typeof(IconButton),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    SelectorRoute = "> .semantic-scope-item /template/ .semantic-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    SelectorRoute = "> .semantic-scope-item /template/ .semantic-body",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Collapse
{
}
