using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "/template/ .semantic-scope-nav > .semantic-item",
    ContractType = typeof(ContentControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Pagination
{
}
