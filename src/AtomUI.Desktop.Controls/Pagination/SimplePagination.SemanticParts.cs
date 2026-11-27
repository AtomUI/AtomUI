using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The previous and next page buttons. Both nodes are part of the control template.
// SimplePaginationItemStyle applies to both buttons.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    ContractType = typeof(ContentControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// The page indicator text next to the page number input, which shows the page count.
// SimplePaginationInfoStyle applies to the text.
[SemanticPart(
    "info",
    SelectorClass = "semantic-info",
    ContractType = typeof(TextBlock),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
public partial class SimplePagination
{
}
