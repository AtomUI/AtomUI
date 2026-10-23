using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "cellList",
    SelectorClass = "semantic-cell-list",
    SelectorRoute = "/template/ .semantic-cell-list",
    ContractType = typeof(ItemsControl),
    Since = "6.2.0")]
[SemanticPart(
    "cell",
    SelectorClass = "semantic-cell",
    SelectorRoute = "/template/ .semantic-cell-list >> .semantic-scope-cell > .semantic-cell",
    ContractType = typeof(ContentControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "separator",
    SelectorClass = "semantic-separator",
    SelectorRoute = "/template/ .semantic-cell-list >> .semantic-scope-cell > .semantic-separator",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class OtpLineEdit
{
}
