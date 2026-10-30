using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    ContractType = typeof(ContentControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
[SemanticPart(
    "info",
    SelectorClass = "semantic-info",
    ContractType = typeof(TextBlock),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
public partial class SimplePagination
{
}
