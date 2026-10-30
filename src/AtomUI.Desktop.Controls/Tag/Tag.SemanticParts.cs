using AtomUI.Controls;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    SelectorRoute = "/template/ .semantic-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-content",
    ContractType = typeof(TextBlock),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
[SemanticPart(
    "close",
    SelectorClass = "semantic-close",
    SelectorRoute = "/template/ .semantic-close",
    ContractType = typeof(IconButton),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
public partial class Tag
{
}
